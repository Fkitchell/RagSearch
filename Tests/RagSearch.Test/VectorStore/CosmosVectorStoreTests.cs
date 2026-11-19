using RagSearch.Api.Search;
using RagSearch.Api.VectorStore.Cosmos;

namespace RagSearch.Test.VectorStore;

public class CosmosVectorStoreTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingHitsOrderedBySimilarity()
    {
        var expectedHits = new[]
        {
            new SearchHit { SimilarityScore = 0.91f },
            new SearchHit { SimilarityScore = 0.73f }
        };
        var query = new FakeCosmosVectorQuery(expectedHits);
        var vectorStore = new CosmosVectorStore(query);

        var results = await vectorStore.SearchAsync(
            vector: [0.1f, 0.2f],
            top: 2,
            filter: null,
            cancellationToken: CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal(0.91f, results[0].SimilarityScore);
        Assert.Equal(0.73f, results[1].SimilarityScore);
    }

    private sealed class FakeCosmosVectorQuery(IReadOnlyList<SearchHit> hits) : ICosmosVectorQuery
    {
        public Task<IReadOnlyList<SearchHit>> QueryAsync(
            float[] vector,
            int top,
            SearchFilter? filter,
            CancellationToken cancellationToken) => Task.FromResult(hits);
    }
}
