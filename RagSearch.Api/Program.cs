using Azure.Identity;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton(_ =>
    new CosmosClient(
        builder.Configuration.GetValue<string>("Cosmos:Endpoint")
            ?? throw new InvalidOperationException("Cosmos:Endpoint is not configured."),
        new DefaultAzureCredential(),
        new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        }));

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
