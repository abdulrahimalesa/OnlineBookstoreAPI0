using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstoreAPI0.Data;
using OnlineBookstoreAPI0.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineBookstoreAPI0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly OnlineBookstoreContext _context;

        public GenresController(OnlineBookstoreContext context)
        {
            _context = context;
        }

        // Get All Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        // Get a Specific Category
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return NotFound(new { message = "Kategori bulunamadı!" });
            }

            return genre;
        }

        // Add New Category
        [HttpPost] 
         public async Task<ActionResult<Genre>> PostGenre([FromBody] Genre genre)
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
                    return Unauthorized("You do not have permission to access this resource.");
                }

                // Return error if model is not valid
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Add category to database
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();

                // If successful, return new category
                return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
            }
            catch (Exception ex)
            {
                // Return error message
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }

        // Update Category

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGenre(int id, [FromBody] Genre genre)
        {
            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "Admin")
            {
                return Unauthorized(new { message = "You do not have permission to update this category." });
            }

            if (id != genre.Id)
            {
                return BadRequest(new { message = "ID does not match!" });
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Genres.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Category not found!" });
                }
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {

            var role = HttpContext.Items["UserRole"] as string;

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Invalid or missing token." });
            }

            if (role != "Admin")
            {
                return Unauthorized(new { message = "You do not have permission to delete this category." });
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new { message = "Category not found!" });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully!", deletedGenre = genre });
        }


    }
}
