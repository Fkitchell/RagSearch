using RagSearch.Api.Search;

namespace RagSearch.Api.VectorStore.Cosmos;

public class CosmosVectorStore(ICosmosVectorQuery query) : IVectorStore
{
    private readonly ICosmosVectorQuery _query = query;

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(
        float[] vector,
        int top,
        SearchFilter? filter,
        CancellationToken cancellationToken)
    {
        if (top <= 0)
            return [];

        var candidates = await _query.QueryAsync(vector, top, filter, cancellationToken);

        return candidates;
    }
}
