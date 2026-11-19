using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.RateLimiting;
using RagSearch.Api.Extensions;
using RagSearch.Api.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

DefaultAzureCredential azCredential = new(
    options: new DefaultAzureCredentialOptions()
    {
        TenantId = builder.Configuration.GetSection("EntraId").GetValue<string>("TenantId")
            ?? throw new InvalidOperationException("EntraId:TenantId is not configured.")
    });
    
var concurrencyPolicy = "Concurrency";
var rateLimitOptions = new RateLimitOptions();
builder.Configuration.GetSection(RateLimitOptions.RateLimitSection).Bind(rateLimitOptions);

builder.Services.AddRateLimiter(_ => _
    .AddConcurrencyLimiter(policyName: concurrencyPolicy, options =>
    {
        options.PermitLimit = rateLimitOptions.PermitLimit;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = rateLimitOptions.QueueLimit;
    }));

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

app.UseRateLimiter();

app.Run();
