namespace RagSearch.Api.Embeddings
{
    public interface IEmbeddingClient
    {
        Task<float[]> EmbedAsync(string query, CancellationToken cancellationToken);
        string GetEmbeddingModelName();
    }
}
