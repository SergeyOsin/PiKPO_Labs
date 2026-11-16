using FoodGraphQL.Data;
using FoodGraphQL.Queries;
using FoodGraphQL.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MongoDBSettings>()
    .Bind(builder.Configuration.GetSection("MongoDbSettings"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.ConnectionString) &&
            !string.IsNullOrWhiteSpace(settings.DatabaseName) &&
            !string.IsNullOrWhiteSpace(settings.CollectionName),
        "MongoDbSettings must define ConnectionString, DatabaseName, and CollectionName.")
    .ValidateOnStart();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<FoodProductService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<FoodProductQuery>();

var app = builder.Build();

app.MapGraphQL("/graphql");

app.Run();