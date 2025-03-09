using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstoreAPI0.Data;
using OnlineBookstoreAPI0.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[Route("api/auth")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly OnlineBookstoreContext _context;
    private readonly IConfiguration _configuration;

    // Single constructor to inject IConfiguration and DbContext
    public UserController(OnlineBookstoreContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Register (Create User or Admin)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDTO userDto)
    {
        try
        {
            if (userDto is null)
            {
                throw new ArgumentNullException(nameof(userDto));
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == userDto.email.ToLower()))
                return BadRequest("Email already in use");

            if (userDto.password.Length < 8 || !userDto.password.Any(char.IsDigit))
                return BadRequest("Password must be at least 8 characters long and contain a number.");

            var user = new User
            {
                Name = userDto.name,
                Email = userDto.email.ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.password, workFactor: 12),
                Role = userDto.role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Name, user.Email, user.Role });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
        }
    }

    // Login (Authenticate)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            // JWT token creation
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
        }
    }

    // Logout (Invalidate JWT Token on client side)
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during logout.", error = ex.Message });
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var role = HttpContext.Items["UserRole"] as string;

            if (role != "Admin")
            {
                return Unauthorized("You do not have permission to access this resource.");
            }

            var users = await _context.Users.ToListAsync();

            var usersDto = users.Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role
            });

            return Ok(usersDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving users.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var role = HttpContext.Items["UserRole"] as string;

            if (role != "User")
            {
                return Unauthorized("You do not have permission to access this resource.");
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user information.", error = ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
