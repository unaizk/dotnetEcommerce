using dotnetEcommerce.Data;
using dotnetEcommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dotnetEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDBContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthController(MongoDBContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("signin")]
    public IActionResult SignIn([FromBody] User userInput)
    {
        // Try to find the user by email only
        var existingUser = _context.User.Find(u => u.Email == userInput.Email).FirstOrDefault();

        if (existingUser != null)
        {
            // User exists, generate token and return
            var token = GenerateJwtToken(existingUser);
            return Ok(new { token, message = "User logged in successfully." });
        }

        // User doesn't exist, create new one
        var newUser = new User
        {
            Name = userInput.Name,
            Email = userInput.Email
        };

        _context.User.InsertOne(newUser);

        var newToken = GenerateJwtToken(newUser);
        return Ok(new { token = newToken, message = "New user created and logged in successfully." });
    }


    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
