using FoodGraphQL.Data;
using FoodGraphQL.Metrics;
using FoodGraphQL.Mutations;
using FoodGraphQL.Queries;
using FoodGraphQL.Services;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MongoDBSettings>()
    .Bind(builder.Configuration.GetSection("MongoDbSettings"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.ConnectionString) &&
            !string.IsNullOrWhiteSpace(settings.DatabaseName) &&
            !string.IsNullOrWhiteSpace(settings.CollectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<MetricService>();
builder.Services.AddSingleton<FoodProductService>();
builder.Services.AddDataLoader<FoodProductDataLoader>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddPrometheusExporter();
        metrics.AddMeter("FoodGraphQL.API");
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                    0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
    });

builder.Services
    .AddGraphQLServer()
    .AddQueryType<FoodProductQuery>()
    .AddMutationType<FoodProductMutation>();

var app = builder.Build();

app.MapGraphQL("/graphql");
app.MapPrometheusScrapingEndpoint();

app.Run();