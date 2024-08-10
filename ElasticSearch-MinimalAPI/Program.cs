using Bogus;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Ingest;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearch_MinimalAPI.Models;
using ElasticSearch_MinimalAPI.ViewModels;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

ElasticsearchClientSettings settings = new(new Uri("http://localhost:9200"));
settings.DefaultIndex("products");
ElasticsearchClient client = new(settings);

client.IndexAsync("products").GetAwaiter().GetResult();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/products/create", async (CreateProductVM createProduct, CancellationToken cancellationToken) =>
{
    Product product = new()
    {
        Name = createProduct.Name,
        Price = createProduct.Price,
        Stock = createProduct.Stock,
        Description = createProduct.Description
    };

    CreateRequest<Product> createRequest = new(product.Id.ToString())
    {
        Document = product,
    };

    CreateResponse createResponse = await client.CreateAsync(createRequest, cancellationToken);

    return Results.Ok(createResponse.Id);
});

app.MapGet("/products/seedData", async (CancellationToken cancellationToken) =>
{
    for (int i = 0; i < 100; i++)
    {
        Faker faker = new();
        Product product = new()
        {
            Name = faker.Commerce.ProductName(),
            Price = faker.Random.Decimal(1, 100),
            Stock = faker.Random.Int(1, 100),
            Description = faker.Lorem.Sentence()
        };

        CreateRequest<Product> createRequest = new(product.Id.ToString())
        {
            Document = product
        };

        await client.CreateAsync<Product>(createRequest, cancellationToken);
    }

    return Results.Ok("Data seeded successfully");
});


app.MapPut("/products/update", async (UpdateProductVM updateProduct, CancellationToken cancellationToken) =>
{
    UpdateRequest<Product, UpdateProductVM> updateRequest = new("products", updateProduct.Id.ToString())
    {
        Doc = updateProduct
    };

    UpdateResponse<Product> updateResponse = await client.UpdateAsync(updateRequest, cancellationToken);
    return Results.Ok(updateResponse.Id);
});


app.MapDelete("/products/delete", async (Guid id, CancellationToken cancellationToken) =>
{
    DeleteRequest deleteRequest = new("products", id.ToString());

    DeleteResponse deleteResponse = await client.DeleteAsync("products",id, cancellationToken);
    return Results.Ok("Deleted successfully.");
});


app.MapGet("/products/getAll", async (CancellationToken cancellationToken) =>
{
    SearchRequest searchRequest = new()
    {
        Size = 100,
        Sort = new List<SortOptions>
        {
            SortOptions.Field(new Field("name.keyword"), new FieldSort() { Order = SortOrder.Asc })
        },

        //Query = new MatchQuery(new Field("name"))
        //{
        //   QueryName = "productName"
        //}
    };
    SearchResponse<Product> searchResponse = await client.SearchAsync<Product>("products", cancellationToken);
    return Results.Ok(searchResponse.Documents);
});


app.Run();
