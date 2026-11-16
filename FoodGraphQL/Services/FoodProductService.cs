using FoodGraphQL.Data;
using FoodGraphQL.Models;
using MongoDB.Driver;

namespace FoodGraphQL.Services;

public sealed class FoodProductService(MongoDbContext context)
{
    private readonly IMongoCollection<FoodProduct> _foodProducts = context.FoodProducts;

    public async Task<List<FoodProduct>> GetFoodProductsAsync(
        string? category,
        bool? isVegan,
        bool? isVegetarian,
        int? minCalories,
        int? maxCalories,
        DateTime? createdAfter,
        DateTime? createdBefore,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<FoodProduct>.Filter;
        var filters = new List<FilterDefinition<FoodProduct>>();

        if (!string.IsNullOrWhiteSpace(category))
            filters.Add(filterBuilder.Eq(product => product.Category, category));

        if (isVegan.HasValue)
            filters.Add(filterBuilder.Eq(product => product.IsVegan, isVegan.Value));

        if (isVegetarian.HasValue)
            filters.Add(filterBuilder.Eq(product => product.IsVegetarian, isVegetarian.Value));

        if (minCalories.HasValue)
            filters.Add(filterBuilder.Gte(product => product.Calories, minCalories.Value));

        if (maxCalories.HasValue)
            filters.Add(filterBuilder.Lte(product => product.Calories, maxCalories.Value));

        if (createdAfter.HasValue)
            filters.Add(filterBuilder.Gte(product => product.CreatedAt, createdAfter.Value));

        if (createdBefore.HasValue)
            filters.Add(filterBuilder.Lte(product => product.CreatedAt, createdBefore.Value));

        var filter = filters.Count == 0 ? filterBuilder.Empty : filterBuilder.And(filters);

        return await _foodProducts
            .Find(filter)
            .SortByDescending(product => product.CreatedAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<FoodProduct?> GetFoodProductByIdAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return await _foodProducts
            .Find(product => product.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}