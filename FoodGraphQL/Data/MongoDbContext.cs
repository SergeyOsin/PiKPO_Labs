using FoodGraphQL.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodGraphQL.Data;

public sealed class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDBSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);

        FoodProducts = client
            .GetDatabase(settings.DatabaseName)
            .GetCollection<FoodProduct>(settings.CollectionName);
    }

    public IMongoCollection<FoodProduct> FoodProducts { get; }
}
