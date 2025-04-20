using dotnetEcommerce.Data;
using dotnetEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace dotnetEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly MongoDBContext _context;

    public CartController(MongoDBContext context)
    {
        _context = context;
    }

    // POST: api/cart/add/{productId}
    [Authorize]
    [HttpPost("add/{productId}")]
    public IActionResult AddToCart(string productId)
    {

  
        // Get user ID from JWT token claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine(userId+"hello");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Please log in before adding to cart." });
        }

        // Check if user already has a cart
        var existingCart = _context.Cart.Find(c => c.UserId == userId).FirstOrDefault();

        if (existingCart != null)
        {
            if (!existingCart.ProductIds.Contains(productId))
            {
                existingCart.ProductIds.Add(productId);

                var update = Builders<Cart>.Update.Set(c => c.ProductIds, existingCart.ProductIds);
                _context.Cart.UpdateOne(c => c.Id == existingCart.Id, update);
            }
        }
        else
        {
            // Create new cart
            var newCart = new Cart
            {
                UserId = userId,
                ProductIds = new List<string> { productId }
            };
            _context.Cart.InsertOne(newCart);
        }

        return Ok(new { message = "Product added to cart." });
    }
}
