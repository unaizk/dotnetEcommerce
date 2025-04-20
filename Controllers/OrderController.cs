using dotnetEcommerce.Data;
using dotnetEcommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace dotnetEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly MongoDBContext _context;

    public OrderController(MongoDBContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("place")]
    public IActionResult PlaceOrder()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Unauthorized. Please login first." });
        }

        var userCart = _context.Cart.Find(c => c.UserId == userId).FirstOrDefault();
        if (userCart == null || userCart.ProductIds.Count == 0)
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        var newOrder = new Order
        {
            UserId = userId,
            ProductIds = userCart.ProductIds
        };

        _context.Order.InsertOne(newOrder);

        // Optional: Clear cart after order is placed
        var update = Builders<Cart>.Update.Set(c => c.ProductIds, new List<string>());
        _context.Cart.UpdateOne(c => c.Id == userCart.Id, update);

        return Ok(new
        {
            message = "Order placed successfully.",
            orderId = newOrder.Id,
            productIds = newOrder.ProductIds,
            createdAt = newOrder.CreatedAt
        });
    }
}
