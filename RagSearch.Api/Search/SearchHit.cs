using System.Reflection.Metadata;

namespace RagSearch.Api.Search
{
    public class SearchHit
    {
        public string? DocumentId { get; set; } 
        public int ChunkIndex { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string FileType { get; set; }
        public string Content { get; set; }
        public int Version { get; set; }
        public int ActiveVersion { get; set; }
        public float SimilarityScore { get; set; }
    }
}
