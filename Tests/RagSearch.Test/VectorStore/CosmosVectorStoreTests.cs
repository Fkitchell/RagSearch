using RagSearch.Api.Search;
using RagSearch.Api.VectorStore.Cosmos;

namespace RagSearch.Test.VectorStore
{
    public class CosmosVectorStoreTests
    {
        [Fact]
        public async Task SearchAsync_ReturnsMatchingHitsOrderedBySimilarity()
        {
            var expectedHits = new[]
            {
                new SearchHit { SimilarityScore = 0.91f },
                new SearchHit { SimilarityScore = 0.73f },
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

        [Fact]
        public async Task SearchAsync_ForwardsArgumentsToQuery()
        {
            var query = new FakeCosmosVectorQuery([]);
            var sut = new CosmosVectorStore(query);
            float[] vector = [0.1f, 0.2f, 0.3f];
            var filter = new SearchFilter();
            using var cts = new CancellationTokenSource();

            await sut.SearchAsync(vector, top: 7, filter, cts.Token);

            Assert.Equal(1, query.CallCount);
            Assert.Same(vector, query.LastVector);
            Assert.Equal(7, query.LastTop);
            Assert.Same(filter, query.LastFilter);
            Assert.Equal(cts.Token, query.LastToken);
        }

        [Fact]
        public async Task SearchAsync_ReturnsQueryResultsUnchanged()
        {
            // Deliberately not sorted by score: the store must not reorder,
            // dedupe, or truncate what the query returns.
            SearchHit[] hits =
            [
                new() { DocumentId = "b", SimilarityScore = 0.42f },
            new() { DocumentId = "a", SimilarityScore = 0.91f },
            new() { DocumentId = "c", SimilarityScore = 0.73f },
        ];
            var sut = new CosmosVectorStore(new FakeCosmosVectorQuery(hits));

            var results = await sut.SearchAsync([0.1f], top: 2, filter: null, CancellationToken.None);

            Assert.Equal(hits, results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task SearchAsync_NonPositiveTop_ReturnsEmptyWithoutQuerying(int top)
        {
            var query = new FakeCosmosVectorQuery([new SearchHit { SimilarityScore = 0.9f }]);
            var sut = new CosmosVectorStore(query);

            var results = await sut.SearchAsync([0.1f], top, filter: null, CancellationToken.None);

            Assert.Empty(results);
            Assert.Equal(0, query.CallCount);
        }


        private sealed class FakeCosmosVectorQuery(IReadOnlyList<SearchHit> hits) : ICosmosVectorQuery
        {
            public int CallCount { get; private set; }
            public float[]? LastVector { get; private set; }
            public int LastTop { get; private set; }
            public SearchFilter? LastFilter { get; private set; }
            public CancellationToken LastToken { get; private set; }

            public Task<IReadOnlyList<SearchHit>> QueryAsync(
                float[] vector,
                int top,
                SearchFilter? filter,
                CancellationToken cancellationToken)
            {
                CallCount++;
                LastVector = vector;
                LastTop = top;
                LastFilter = filter;
                LastToken = cancellationToken;
                return Task.FromResult(hits);
            }
        }
    }
}