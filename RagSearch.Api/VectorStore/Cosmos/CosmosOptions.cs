namespace RagSearch.Api.VectorStore.Cosmos
{
    public class CosmosOptions
    {
        public string Endpoint{ get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
    }
}
