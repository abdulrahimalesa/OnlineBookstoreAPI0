using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstoreAPI0.Data;
using OnlineBookstoreAPI0.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookstoreAPI0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OnlineBookstoreContext _context;

        public OrderController(OnlineBookstoreContext context)
        {
            _context = context;
        }

        [HttpGet("getOrders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string; 

                if (userId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                if (role != "Admin")
                {
                    return Unauthorized("You do not have permission to view all orders.");
                }

                var orders = await _context.Orders.ToListAsync();

                if (!orders.Any())
                {
                    return NotFound(new { message = "No orders found." });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("getUserOrders")]
        public async Task<IActionResult> GetUserOrders()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string; 

                if (userId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                if (role != "User")
                {
                    return Unauthorized("You do not have permission to view all orders.");
                }

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId.Value)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return NotFound(new { message = "No orders found for this user." });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("updateStatus/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] Order request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                var role = HttpContext.Items["UserRole"] as string;

                if (userId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                if (role != "Admin")
                {
                    return Unauthorized("You do not have permission to update the order status.");
                }

                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                var validStatuses = new[] { "Pending", "Processed", "Shipped", "Completed", "Cancelled" };
                if (!validStatuses.Contains(request.Status))
                {
                    return BadRequest("Invalid status value.");
                }

                order.Status = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Order status updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });

            }
        }
    }
}
