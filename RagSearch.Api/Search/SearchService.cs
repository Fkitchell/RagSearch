using RagSearch.Api.Embeddings;
using RagSearch.Api.VectorStore;

namespace RagSearch.Api.Search
{
    public class SearchService(IEmbeddingClient embeddingClient, IVectorStore vectorStore)
    {
        private readonly IEmbeddingClient _embeddingClient = embeddingClient;
        private readonly IVectorStore _vectorStore = vectorStore;

        public async Task<IReadOnlyList<SearchHit>> SearchAsync(
            string query,
            int top,
            SearchFilter searchFilter,
            CancellationToken cancellationToken) => await _vectorStore.SearchAsync(await _embeddingClient.EmbedAsync(query, cancellationToken), top, searchFilter, cancellationToken);
    }
}
