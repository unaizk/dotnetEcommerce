using dotnetEcommerce.Data;
using dotnetEcommerce.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace dotnetEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly MongoDBContext _context;

    public ProductController(MongoDBContext context)
    {
        _context = context;
    }

    // POST: api/product
    [HttpPost]
    public IActionResult AddProduct([FromBody] Product product)
    {
        if (product == null || string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest(new { message = "Product data is invalid." });
        }

        _context.Product.InsertOne(product);
        return Ok(new { message = "Product added successfully.", product });
    }


    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(string id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || string.IsNullOrWhiteSpace(updatedProduct.Name))
        {
            return BadRequest(new { message = "Product data is invalid." });
        }

        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        var update = Builders<Product>.Update
            .Set(p => p.Name, updatedProduct.Name)
            .Set(p => p.Price, updatedProduct.Price);

        var result = _context.Product.UpdateOne(filter, update);

        if (result.MatchedCount == 0)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(new { message = "Product updated successfully." });
    }

}
