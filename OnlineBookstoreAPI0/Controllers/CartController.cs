using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstoreAPI0.Data;
using OnlineBookstoreAPI0.Models;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineBookstoreAPI0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly OnlineBookstoreContext _context;

        public CartController(OnlineBookstoreContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string;

                if (string.IsNullOrEmpty(role)) return Unauthorized(new { message = "Invalid or missing token." });
                if (role != "User") return Unauthorized("You do not have permission to use this resource.");

                var cartItems = await _context.CartItems
                    .Where(cart => cart.UserId == userId)
                    .Join(_context.Books,
                          cart => cart.BookId,
                          book => book.Id,
                          (cart, book) => new
                          {
                              cart.Id,
                              cart.Quantity,
                              cart.BookId,
                              book.Title,
                              book.Price,
                              book.Author
                          })
                    .ToListAsync();

                var totalPrice = cartItems.Sum(c => c.Quantity * c.Price);
                return Ok(new { cartItems, totalPrice });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cart items.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCartItem([FromBody] CartItem cartItem)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string;

                if (string.IsNullOrEmpty(role)) return Unauthorized(new { message = "Invalid or missing token." });
                if (role != "User") return Unauthorized("You do not have permission to use this resource.");

                var book = await _context.Books.FindAsync(cartItem.BookId);
                if (book == null) return NotFound(new { message = "Book not found" });

                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == cartItem.BookId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += cartItem.Quantity;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId.Value,
                        BookId = cartItem.BookId,
                        Quantity = cartItem.Quantity,
                        Price = book.Price
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Item added to cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding item to cart.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] CartItem cartItem)
        {
            var userId = HttpContext.Items["UserId"] as int?;
            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "User")
            {
                return Unauthorized("You do not have permission to use this resource.");
            }

            var existingCartItem = await _context.CartItems.FindAsync(id);
            if (existingCartItem == null || existingCartItem.UserId != userId)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            if (cartItem.BookId > 0)
            {
                var book = await _context.Books.FindAsync(cartItem.BookId);
                if (book == null)
                {
                    return NotFound(new { message = "Book not found" });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid BookId." });
            }

            existingCartItem.Quantity = cartItem.Quantity;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cart item updated successfully" });
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string;

                if (string.IsNullOrEmpty(role)) return Unauthorized(new { message = "Invalid or missing token." });
                if (role != "User") return Unauthorized("You do not have permission to use this resource.");

                var cartItem = await _context.CartItems.FindAsync(id);
                if (cartItem == null || cartItem.UserId != userId)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing item from cart.", error = ex.Message });
            }
        }


        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = HttpContext.Items["UserId"] as int?;
            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "User")
            {
                return Unauthorized("You do not have permission to use this resource. ");
            }

            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cart cleared" });
        }


        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = HttpContext.Items["UserId"] as int?;
            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "User")
            {
                return Unauthorized("You do not have permission to use this resource. ");
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Join(_context.Books,
                      cart => cart.BookId,
                      book => book.Id,
                      (cart, book) => new
                      {
                          cart.Id,
                          cart.Quantity,
                          cart.BookId,
                          book.Title,
                          book.Price,
                          book.Author,
                          book.Stock
                      })
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest(new { message = "Your cart is empty" });

            var totalPrice = cartItems.Sum(c => c.Quantity * c.Price);

            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Stock)
                {
                    return BadRequest(new { message = $"Not enough stock for book: {item.Title}" });
                }

                var book = await _context.Books.FindAsync(item.BookId);
                if (book != null)
                {
                    book.Stock -= item.Quantity; 
                }

                var order = new Order
                {
                    UserId = userId.Value,
                    TotalAmount = (double)totalPrice,
                    OrderDate = DateTime.Now,
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    ShippingAddress = request.ShippingAddress,
                    City = request.City,
                    PostalCode = request.PostalCode,
                    Status = request.Status,
                    BookId = item.BookId,
                    BookTitle = item.Title,
                    BookPrice = (double)item.Price,
                    Quantity = item.Quantity,

                };

                _context.Orders.Add(order);
            }

            try
            {
                await _context.SaveChangesAsync();

                // Sepeti temizliyoruz
                _context.CartItems.RemoveRange(_context.CartItems.Where(c => c.UserId == userId));
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your order.", error = ex.Message });
            }

            return Ok(new { success = true, message = "Checkout successful", totalPrice });
        }

 
    }
}
