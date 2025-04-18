using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dotnetEcommerce.Models;

public class Order {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string ProductId {get;set;} = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}