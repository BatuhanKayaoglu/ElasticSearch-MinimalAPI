using Elastic.Clients.Elasticsearch;
using ElasticSearch_MinimalAPI.Models;
using ElasticSearch_MinimalAPI.ViewModels;

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

app.MapGet("/products/getAll", async (CancellationToken cancellationToken) =>
{
    SearchResponse<Product> searchResponse = await client.SearchAsync<Product>("products", cancellationToken);

    return Results.Ok(searchResponse.Documents);


});     





//app.MapGet("/search", (string query) =>
//{
//    var client = new ElasticClient(new ConnectionSettings(new Uri("http://localhost:9200")));
//    var response = client.Search<Dictionary<string, object>>(s => s
//           .Query(q => q
//                      .Match(m => m
//                                     .Field("title")
//                                                    .Query(query)
//                                                               )
//                             )
//              );

//    return response.Documents;
//});

app.Run();
