using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dotnetEcommerce.Models;

public class Product {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}