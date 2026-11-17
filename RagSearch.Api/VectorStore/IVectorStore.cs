using RagSearch.Api.Search;

namespace RagSearch.Api.VectorStore
{
    public interface IVectorStore
    {
        Task<IReadOnlyList<SearchHit>> SearchAsync(float[] vector, int top, SearchFilter? filter, CancellationToken cancellationToken);
    }
}
