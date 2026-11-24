using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FoodSelection.Metrics;

public class MetricService
{
    private readonly Counter<int> _productCreatedCounter;
    private readonly Histogram<double> _dbOperationDuration;
    private readonly Counter<int> _cacheRequestsCounter;
    private readonly Counter<int> _productOperationsCounter;
    private readonly Histogram<double> _operationDuration;

    public MetricService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("FoodSelection.API");

        _productCreatedCounter = meter.CreateCounter<int>("foodselection.products.created",
            description: "Количество созданных продуктов");

        _dbOperationDuration = meter.CreateHistogram<double>("foodselection.db.operation.duration",
            unit: "ms", description: "Длительность операций с MongoDB");

        _cacheRequestsCounter = meter.CreateCounter<int>("foodselection.cache.requests",
            description: "Статистика обращений к кэшу (попадания и промахи)");

        _productOperationsCounter = meter.CreateCounter<int>("foodselection_product_operations");
        _operationDuration = meter.CreateHistogram<double>(
            "foodselection_product_operation_duration",
            unit: "s");
    }

    public void ProductCreated(string category) =>
        _productCreatedCounter.Add(1, new KeyValuePair<string, object?>("category", category));

    public void RecordDbOperationDuration(double durationMs, string operation) =>
        _dbOperationDuration.Record(durationMs, new KeyValuePair<string, object?>("operation", operation));

    public void RecordCacheRequest(string cacheType, bool isHit) =>
        _cacheRequestsCounter.Add(1,
            new KeyValuePair<string, object?>("cache_type", cacheType),
            new KeyValuePair<string, object?>("status", isHit ? "hit" : "miss"));

    public async Task<T> TrackOperationAsync<T>(string operation, Func<Task<T>> action)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var outcome = "success";

        try
        {
            return await action();
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            throw;
        }
        catch
        {
            outcome = "error";
            throw;
        }
        finally
        {
            var tags = new TagList
            {
                { "operation", operation },
                { "outcome", outcome }
            };

            _productOperationsCounter.Add(1, tags);
            _operationDuration.Record(Stopwatch.GetElapsedTime(startedAt).TotalSeconds, tags);
        }
    }
}