using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var config = builder.Configuration.AddUserSecrets<Program>().Build();

CosmosClient client = new(
    config.GetValue<string>("CosmosDB:Endpoint"),
    config.GetValue<string>("CosmosDB:PrimaryKey")
    );

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
