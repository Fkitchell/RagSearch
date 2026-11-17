namespace RagSearch.Api.Embeddings
{
    public class OpenAIOptions
    {
        public string Uri { get; set; }
        public string EmbeddingModelName { get; set; }
        public string DeploymentName { get; set; }
    }
}