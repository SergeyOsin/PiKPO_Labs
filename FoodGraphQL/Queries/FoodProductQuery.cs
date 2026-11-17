using FoodGraphQL.Models;
using FoodGraphQL.Services;
using GreenDonut;
using HotChocolate;

namespace FoodGraphQL.Queries;

public sealed class FoodProductQuery
{
    public Task<List<FoodProduct>> GetFoodProductsAsync(
        [Service] FoodProductService service,
        string? category = null,
        bool? isVegan = null,
        bool? isVegetarian = null,
        int? minCalories = null,
        int? maxCalories = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default) =>
        service.GetFoodProductsAsync(
            category,
            isVegan,
            isVegetarian,
            minCalories,
            maxCalories,
            createdAfter,
            createdBefore,
            skip,
            take,
            cancellationToken);

    public Task<FoodProduct?> GetFoodProductByIdAsync(
        [Service] FoodProductDataLoader dataLoader,
        string id,
        CancellationToken cancellationToken = default) =>
        dataLoader.LoadAsync(id, cancellationToken);
}