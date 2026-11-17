using RagSearch.Api.Search;

namespace RagSearch.Api.VectorStore.Cosmos;

public interface ICosmosVectorQuery
{
    Task<IReadOnlyList<SearchHit>> QueryAsync(
        float[] vector,
        int top,
        SearchFilter? filter,
        CancellationToken cancellationToken);
}
