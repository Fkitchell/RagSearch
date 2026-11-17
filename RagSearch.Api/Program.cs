using Azure.Identity;
using RagSearch.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

DefaultAzureCredential azCredential = new(
    options: new DefaultAzureCredentialOptions()
    {
        TenantId = builder.Configuration.GetSection("EntraId").GetValue<string>("TenantId")
            ?? throw new InvalidOperationException("EntraId:TenantId is not configured.")
    });

builder.Services.AddCosmosDB(builder.Configuration, azCredential);
builder.Services.AddAzOpenAi(builder.Configuration, azCredential);
builder.Services.AddSearch();

builder.Host.UseDefaultServiceProvider(o =>
{
    o.ValidateScopes = true;
    o.ValidateOnBuild = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
