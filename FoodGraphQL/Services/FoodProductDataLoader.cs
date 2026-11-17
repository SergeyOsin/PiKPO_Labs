using FoodGraphQL.Models;
using GreenDonut;

namespace FoodGraphQL.Services;

public sealed class FoodProductDataLoader : BatchDataLoader<string, FoodProduct?>
{
    private readonly FoodProductService _service;

    public FoodProductDataLoader(
        IBatchScheduler batchScheduler,
        FoodProductService service,
        DataLoaderOptions? options = null) :
        base(batchScheduler, options ?? new DataLoaderOptions()) => _service = service;

    protected override Task<IReadOnlyDictionary<string, FoodProduct?>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken) =>
        _service.GetFoodProductsByIdsAsync(keys, cancellationToken);
}
