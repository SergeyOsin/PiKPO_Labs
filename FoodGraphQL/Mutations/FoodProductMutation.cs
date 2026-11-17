using FoodGraphQL.Models;
using FoodGraphQL.Services;
using HotChocolate;

namespace FoodGraphQL.Mutations;

public sealed class FoodProductMutation
{
    public Task<FoodProduct> CreateFoodProductAsync(
        [Service] FoodProductService service,
        string name,
        int calories,
        double protein,
        double carbs,
        double fats,
        string category,
        bool isVegan,
        bool isVegetarian,
        CancellationToken ct = default) =>
        service.CreateFoodProductAsync(new FoodProduct
        {
            Name = name,
            Calories = calories,
            Protein = protein,
            Carbs = carbs,
            Fats = fats,
            Category = category,
            IsVegan = isVegan,
            IsVegetarian = isVegetarian,
        }, ct);

    public Task<FoodProduct?> UpdateFoodProductAsync(
        [Service] FoodProductService service,
        string id,
        string name,
        int calories,
        double protein,
        double carbs,
        double fats,
        string category,
        bool isVegan,
        bool isVegetarian,
        CancellationToken ct = default) =>
        service.UpdateFoodProductAsync(id, new FoodProduct
        {
            Id = id,
            Name = name,
            Calories = calories,
            Protein = protein,
            Carbs = carbs,
            Fats = fats,
            Category = category,
            IsVegan = isVegan,
            IsVegetarian = isVegetarian,
        }, ct);

    public Task<bool> DeleteFoodProductAsync(
        [Service] FoodProductService service,
        string id,
        CancellationToken ct = default) =>
        service.DeleteFoodProductAsync(id, ct);
}