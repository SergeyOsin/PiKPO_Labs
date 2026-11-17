using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FoodGraphQL.Metrics;

public sealed class MetricService
{
    private readonly Counter<int> _productOperationsCounter;
    private readonly Histogram<double> _operationDuration;

    public MetricService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("FoodGraphQL.API");

        _productOperationsCounter = meter.CreateCounter<int>(
            "foodgraphql_product_operations");

        _operationDuration = meter.CreateHistogram<double>(
            "foodgraphql_product_operation_duration",
            unit: "s");
    }

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
