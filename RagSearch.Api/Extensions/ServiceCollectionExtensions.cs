using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using RagSearch.Api.Embeddings;
using RagSearch.Api.Search;
using RagSearch.Api.VectorStore;
using RagSearch.Api.VectorStore.Cosmos;

namespace RagSearch.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzOpenAi(this IServiceCollection serviceCollection, IConfiguration configuration, DefaultAzureCredential credential)
        {
            var openAISection = configuration.GetSection("OpenAI");

            serviceCollection.Configure<OpenAIOptions>(openAISection);

            serviceCollection.AddSingleton(
                new AzureOpenAIClient(new Uri(openAISection.GetValue<string>("Uri") ??
                throw new InvalidOperationException("OpenAI:Uri is not configured")),
                credential));

            return serviceCollection;
        }

        public static IServiceCollection AddCosmosDB(this IServiceCollection serviceCollection, IConfiguration configuration, DefaultAzureCredential credential)
        {

            var cosmosSection = configuration.GetSection("Cosmos");

            serviceCollection.Configure<CosmosOptions>(cosmosSection);

            serviceCollection.AddSingleton(_ =>
                new CosmosClient(
                    cosmosSection.GetValue<string>("Endpoint")
                        ?? throw new InvalidOperationException("Cosmos:Endpoint is not configured."),
                    credential,
                    new CosmosClientOptions
                    {
                        SerializerOptions = new CosmosSerializationOptions
                        {
                            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                        }
                    }));

            return serviceCollection;
        }

        public static IServiceCollection AddSearch(this IServiceCollection services)
        {
            services.AddSingleton<IEmbeddingClient, AzureOpenAiEmbeddingClient>();
            services.AddSingleton<IVectorStore, CosmosVectorStore>();
            services.AddSingleton<ICosmosVectorQuery, CosmosVectorQuery>();
            services.AddSingleton<SearchService>();

            return services;
        }
    }
}
