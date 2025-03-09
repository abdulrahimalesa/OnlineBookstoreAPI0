using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var userRoleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        context.Items["UserId"] = userId;
                    }

                    if (!string.IsNullOrEmpty(userRoleClaim))
                    {
                        context.Items["UserRole"] = userRoleClaim;
                    }
                }
            }
            catch
            {
                
            }
        }

        await _next(context);
    }
}
