using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstoreAPI0.Data;
using OnlineBookstoreAPI0.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineBookstoreAPI0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly OnlineBookstoreContext _context;

        public BooksController(OnlineBookstoreContext context)
        {
            _context = context;
        }

        // Get All Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // Get Book by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound(new { message = "Book not found!" });
            }

            return book;
        }

        // Add New Book
        [HttpPost]
        public async Task<IActionResult> PostBook([FromBody] Book book)
        {
            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "Admin")
            {
                return Unauthorized(new { message = "You do not have permission to add a book." });
            }

            try
            {
               
                var genre = await _context.Genres.FindAsync(book.GenreId);
                if (genre == null)
                {
                    return BadRequest("Invalid GenreId.");
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

               
                genre.BookCount += 1;
                await _context.SaveChangesAsync();

               
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }



        // Update Book 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, [FromBody] Book book)
        {
            var role = HttpContext.Items["UserRole"] as string;
            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }
            if (role != "Admin")
            {
                return Unauthorized("You do not have permission to update this resource.");
            }

            if (id != book.Id)
            {
                return BadRequest(new { message = "ID mismatch!" });
            }

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound(new { message = "Book not found!" });
            }

            if (existingBook.GenreId != book.GenreId)
            {
                var oldGenre = await _context.Genres.FindAsync(existingBook.GenreId);
                if (oldGenre != null && oldGenre.BookCount > 0)
                {
                    oldGenre.BookCount -= 1;
                }

                var newGenre = await _context.Genres.FindAsync(book.GenreId);
                if (newGenre == null)
                {
                    return BadRequest(new { message = "New genre not found!" });
                }
                newGenre.BookCount += 1;
            }

            _context.Entry(existingBook).CurrentValues.SetValues(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Book not found!" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // Delete Book 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {

            var role = HttpContext.Items["UserRole"] as string;
            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            try
            {
                 

                if (role != "Admin") 
                {
                    return Unauthorized("You do not have permission to delete this resource.");
                }

                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound(new { message = "Book not found!" });
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }


        // Search Books
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
            [FromQuery] string? title,
            [FromQuery] string? author)
        {
            try
            {
                var query = _context.Books.AsQueryable();
                if (!string.IsNullOrEmpty(title))
                {
                    query = query.Where(b => b.Title.Contains(title));
                }
                if (!string.IsNullOrEmpty(author))
                {
                    query = query.Where(b => b.Author.Contains(author));
                }
                var books = await query.ToListAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Filter Books
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Book>>> FilterBooks(
        [FromQuery] int? genreId,
        [FromQuery] int? minPrice,
        [FromQuery] int? maxPrice)
        {
            try
            {
                var query = _context.Books.AsQueryable();
                if (genreId.HasValue)
                {
                    query = query.Where(b => b.GenreId == genreId);
                }
                if (minPrice.HasValue)
                {
                    query = query.Where(b => b.Price >= minPrice);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(b => b.Price <= maxPrice);
                }
                var books = await query.ToListAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

    }

}
