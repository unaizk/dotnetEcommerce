using dotnetEcommerce.Data;
using dotnetEcommerce.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.IO;  // For MemoryStream
using System.Text;
using System.Text.Json; // For string encoding (optional)

namespace dotnetEcommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{

    private const string base64Key = "cYjbiDpSl0wMST3BkJzN6Gy6aFv7AzPAXgn6sKddCIk="; 
    private const string base64IV = "vYvALZJYxzyrrwYgFZlmSw=="; 
    private readonly MongoDBContext _context;

    public ProductController(MongoDBContext context)
    {
        _context = context;
    }

    // GET: api/product
    [HttpGet]
    public IActionResult GetAllProducts()
    {
        var products = _context.Product.Find(_ => true).ToList();

        var jsonData = JsonSerializer.Serialize(products);

        byte[] key = Convert.FromBase64String(base64Key);
        byte[] iv = Convert.FromBase64String(base64IV);

        string encryptedData = EncryptString(jsonData, key, iv);

        
        return Ok(new
        {
            encryptedData,
            iv = base64IV 
        });
        // return Ok(products);
    }

     public static string EncryptString(string plainText, byte[] key, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
        
        sw.Write(plainText);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }
    public class EncryptedPayloadDto
    {
        public string EncryptedData { get; set; }
        public string Iv { get; set; }
    }

  [HttpPost]
    public IActionResult AddProduct([FromBody] EncryptedPayloadDto payload)
    {
        if (string.IsNullOrWhiteSpace(payload.EncryptedData) || string.IsNullOrWhiteSpace(payload.Iv))
        {
            return BadRequest(new { message = "Missing encrypted data or IV." });
        }

        try
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Convert.FromBase64String(payload.Iv);

            string decryptedJson = DecryptString(payload.EncryptedData, key, iv);

            Product? product = JsonSerializer.Deserialize<Product>(decryptedJson);

            if (product == null || string.IsNullOrWhiteSpace(product.Name))
            {
                return BadRequest(new { message = "Invalid product after decryption." });
            }
            Console.WriteLine(product.Name);
            _context.Product.InsertOne(product);

            return Ok(new { message = "Product added successfully.", product });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error decrypting or saving product.", error = ex.Message });
        }
    }
    
    public static string DecryptString(string encryptedBase64, byte[] key, byte[] iv)
    {
        byte[] buffer = Convert.FromBase64String(encryptedBase64);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor();
        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);

        return sr.ReadToEnd();
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

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(string id)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        var result = _context.Product.DeleteOne(filter);

        if (result.DeletedCount == 0)
        {
            return NotFound(new { message = "Product not found or already deleted." });
        }

        return Ok(new { message = "Product deleted successfully." });
    }


}
