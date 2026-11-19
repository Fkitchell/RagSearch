namespace RagSearch.Api.Models
{
    internal class SourceDocument
    {
        public string DocumentId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string FileType { get; set; }
        public int ActiveVersion { get; set; }
        public List<DocumentChunk> Chunks { get; set; }
    }

    internal class DocumentChunk
    {
        public string Id => $"{DocumentId}:{ChunkIndex}";
        public string DocumentId { get; set; }
        public int ChunkIndex { get; set; }
        public string EmbeddingModel { get; set; }
        public string Content { get; set; }
        public Array[] Embedding { get => field; set => field = value; }
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}