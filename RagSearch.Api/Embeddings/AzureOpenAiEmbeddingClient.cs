using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace RagSearch.Api.Embeddings
{
    public class AzureOpenAiEmbeddingClient : IEmbeddingClient
    {
        public AzureOpenAiEmbeddingClient(AzureOpenAIClient azureOpenAIClient, IOptions<OpenAIOptions> openAiOptions)
        {
            var options = openAiOptions.Value;
            _embeddingModelName = options.EmbeddingModelName;

            _embeddingClient = azureOpenAIClient.GetEmbeddingClient(openAiOptions.Value.DeploymentName);
        }

        private readonly string _embeddingModelName;
        private readonly EmbeddingClient _embeddingClient;
        public async Task<float[]> EmbedAsync(string query, CancellationToken cancellationToken)
        {
            var result = await _embeddingClient.GenerateEmbeddingAsync(query, cancellationToken: cancellationToken);
            return result.Value.ToFloats().ToArray();
        }

        public string GetEmbeddingModelName() => _embeddingModelName;
    }
}
