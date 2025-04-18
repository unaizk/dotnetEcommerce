using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dotnetEcommerce.Models;


public class Cart {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = null!;
    public List<string> ProductIds { get; set; } = new();
}