using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using RagSearch.Api.Search;

namespace RagSearch.Api.VectorStore.Cosmos
{
    public class CosmosVectorQuery : ICosmosVectorQuery
    {

        private readonly Container _container;

        public CosmosVectorQuery(IOptions<CosmosOptions> options, CosmosClient cosmosClient)
        {
            var opt = options.Value;
            var database = cosmosClient.GetDatabase(opt.DatabaseName);
            _container = database.GetContainer(opt.ContainerName);
        }

        public async Task<IReadOnlyList<SearchHit>> QueryAsync(float[] vector, int top, SearchFilter? filter = null, CancellationToken cancellationToken = default)
        {
            QueryDefinition queryDefinition = new QueryDefinition(
                """
                SELECT TOP @top
                    c.documentId,
                    c.chunkIndex,
                    c.title,
                    c.author,
                    c.fileType,
                    c.content,
                    c.version,
                    VectorDistance(c.embedding, @embedding) as similarityScore
                FROM c
                    WHERE c.activeVersion = c.version
                ORDER BY 
                    VectorDistance(c.embedding, @embedding) 
                """)
                .WithParameter("@top", top)
                .WithParameter("@embedding", vector);

            var iterator = _container.GetItemQueryIterator<SearchHit>(queryDefinition);
            List<SearchHit> hits = new();

            Console.WriteLine(queryDefinition.QueryText);

            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync(cancellationToken);

                hits.AddRange(page.Resource);
            }

            return hits;
        }
    }
}
