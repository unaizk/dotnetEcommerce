using dotnetEcommerce.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace dotnetEcommerce.Data{

    public class MongoDBContext{
        private readonly IMongoDatabase _database;

        public MongoDBContext(IOptions<MongoDBSettings> settings){
             var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> User => _database.GetCollection<User>("Users");
        public IMongoCollection<Product> Product => _database.GetCollection<Product>("Products");
        public IMongoCollection<Cart> Cart => _database.GetCollection<Cart>("Carts");
        public IMongoCollection<Order> Order => _database.GetCollection<Order>("Orders");
    }

}