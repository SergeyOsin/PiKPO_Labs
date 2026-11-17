using FoodGraphQL.Data;
using FoodGraphQL.Metrics;
using FoodGraphQL.Models;
using MongoDB.Driver;

namespace FoodGraphQL.Services;

public sealed class FoodProductService(MongoDbContext context, MetricService metrics)
{
    private readonly IMongoCollection<FoodProduct> _foodProducts = context.FoodProducts;

    public Task<List<FoodProduct>> GetFoodProductsAsync(
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
        return metrics.TrackOperationAsync("list", async () =>
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
        });
    }

    public Task<FoodProduct?> GetFoodProductByIdAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return metrics.TrackOperationAsync<FoodProduct?>("by_id", async () =>
        {
            return await _foodProducts
                .Find(product => product.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        });
    }

    public Task<IReadOnlyDictionary<string, FoodProduct?>> GetFoodProductsByIdsAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken)
    {
        return metrics.TrackOperationAsync("by_ids", async () =>
        {
            var products = await _foodProducts
                .Find(product => ids.Contains(product.Id!))
                .ToListAsync(cancellationToken);

            var productsById = products.ToDictionary(
                product => product.Id!,
                product => (FoodProduct?)product,
                StringComparer.OrdinalIgnoreCase);

            var results = new Dictionary<string, FoodProduct?>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in ids)
                results[id] = productsById.GetValueOrDefault(id);

            return (IReadOnlyDictionary<string, FoodProduct?>)results;
        });
    }

    public Task<FoodProduct> CreateFoodProductAsync(
        FoodProduct product,
        CancellationToken cancellationToken)
    {
        return metrics.TrackOperationAsync("create", async () =>
        {
            product.CreatedAt = DateTime.UtcNow;
            await _foodProducts.InsertOneAsync(product, cancellationToken: cancellationToken);
            return product;
        });
    }

    public Task<FoodProduct?> UpdateFoodProductAsync(
        string id,
        FoodProduct product,
        CancellationToken cancellationToken)
    {
        return metrics.TrackOperationAsync<FoodProduct?>("update", async () =>
        {
            var update = Builders<FoodProduct>.Update
                .Set(item => item.Name, product.Name)
                .Set(item => item.Calories, product.Calories)
                .Set(item => item.Protein, product.Protein)
                .Set(item => item.Carbs, product.Carbs)
                .Set(item => item.Fats, product.Fats)
                .Set(item => item.Category, product.Category)
                .Set(item => item.IsVegan, product.IsVegan)
                .Set(item => item.IsVegetarian, product.IsVegetarian);

            return await _foodProducts.FindOneAndUpdateAsync(
                item => item.Id == id,
                update,
                new FindOneAndUpdateOptions<FoodProduct>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken);
        });
    }

    public Task<bool> DeleteFoodProductAsync(string id, CancellationToken cancellationToken)
    {
        return metrics.TrackOperationAsync("delete", async () =>
        {
            var result = await _foodProducts.DeleteOneAsync(
                product => product.Id == id,
                cancellationToken);

            return result.DeletedCount > 0;
        });
    }
}