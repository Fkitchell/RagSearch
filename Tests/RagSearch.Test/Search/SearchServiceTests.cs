using RagSearch.Api.Embeddings;
using RagSearch.Api.Search;
using RagSearch.Api.VectorStore;

namespace RagSearch.Test.Search
{
    public class SearchServiceTests
    {
        [Fact]
        public async Task SearchTest()
        {
            // arrange
            float[] vector = [0.1f, 0.3f, 0.4f];
            string modelName = "test-embedding-model-name";
            var embeddingClient = new FakeEmbeddingClient(vector, modelName);

            var expectedHits = new[]
            {
                new SearchHit { SimilarityScore = 0.91f },
                new SearchHit { SimilarityScore = 0.73f },
                new SearchHit { SimilarityScore = 0.42f }
            };

            var vectorStore = new FakeVectorStore(expectedHits);
            var sut = new SearchService(embeddingClient, vectorStore);

            // Act
            var results = await sut.SearchAsync("hello world!", 3, CancellationToken.None, null);

            // Assert 
            Assert.Equal(3, results.Count);
            Assert.Equal(0.91f, results[0].SimilarityScore);
            Assert.Equal(0.42f, results[2].SimilarityScore);
        }
    }

    public sealed class FakeEmbeddingClient(float[] vector, string embeddingModelName) : IEmbeddingClient
    {
        public string GetEmbeddingModelName() => embeddingModelName;
        public Task<float[]> EmbedAsync(string query, CancellationToken cancellationToken)
        => Task.FromResult(vector);
    }

    public sealed class FakeVectorStore(IReadOnlyList<SearchHit> hits) : IVectorStore
    {
        public Task<IReadOnlyList<SearchHit>> SearchAsync(float[] vector, int top, SearchFilter? filter, CancellationToken cancellationToken) => Task.FromResult(hits);
    }
}
