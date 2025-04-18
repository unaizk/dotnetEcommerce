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

        
    }

}