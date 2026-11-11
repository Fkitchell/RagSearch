# RagSearch

RAG search API in .NET on Azure: Container Apps, Cosmos DB vector search, pgvector, Redis, Service Bus, and Functions.

> **Status:** in active development. Everything below shows what works today and decisions I intend to make and document later.

## What it does

RagSearch is the retrieval layer of a retrieval-augmented generation (RAG) system. It ingests documents, generates embeddings, and returns the most relevant passages for a natural-language query, with optional metadata filters. New and updated documents flow through an event-driven pipeline, so search results stay current without manual reindexing.

## Architecture

```mermaid
---
config:
  layout: elk
  theme: dark
---

flowchart LR
    L3[Built] ~~~ Client -.->|search| API[ASP.NET Core API<br/>Container Apps]
    API -.-> Redis[(Azure Managed Redis<br/>cache)]
    Client -.->|upload| SB[[Service Bus queue]]
    SB -.-> Fn[Ingestion Function]
    Fn & API -.-> |embeddings| Cosmos[(Cosmos DB for NoSQL<br/>vector search)] & PG[(PostgreSQL + pgvector<br/>filtered search)]
    Cosmos -.->|change feed| Inval[Cache invalidation]
    PG -.->|change feed| Inval[Cache invalidation]
    Inval -.-> Redis

classDef planned stroke-dasharray: 5 5
classDef partial stroke-width:3px,stroke:orange

class API partial
class Cosmos,Inval,Redis,Client,SB,PG,Fn planned

subgraph Legend
    direction TB
    L1[Planned]:::planned ~~~
    L2[Partial]:::partial ~~~
    L3[Built]
    end
```

Every service authenticates with managed identity. Secrets live in Key Vault, settings in App Configuration, and all components emit OpenTelemetry traces to Application Insights.

<!-- Update this diagram whenever the architecture changes. Name the embedding model once it's chosen. -->

## Status

- [x] API running in Container Apps, built with ACR Tasks, with a KEDA scale rule
- [ ] Vector search in Cosmos DB
- [ ] Change feed processor driving Redis cache invalidation
- [ ] pgvector similarity search with metadata filters
- [ ] Service Bus–triggered ingestion Function with dead-letter handling
- [ ] Managed identity, Key Vault, and App Configuration (no secrets in config)
- [ ] OpenTelemetry tracing across all services, with KQL queries below

## Design decisions

Each section records what I chose (as they come up), what I rejected, and the trade-off I accepted.

### Container Apps over AKS and App Service

I decided against App Service to avoid paying for a service that wouldn't need to be online 24/7. Not that this project would need an expensive sku tier, but a production service likely would which would make this a good decision to make.

<!-- Scaling needs, operational overhead, and when AKS would become the better choice. -->
### Cosmos DB

#### Schema

The schema decision was driven by the access patterns I'd reasonably expect from a real system like this. They're ordered by frequency:

1. Semantic search across all documents, comparing a search term against the document chunks.
2. Reading a single document, i.e., an end user asks for the source document the LLM used for an answer.
3. An admin UI for listing and deleting the documents stored in the database.
4. Editing a single document.

The first pattern is a cross-document search, so it fans out across partitions no matter what the partition key is. That means the partition key decision is driven by patterns two, three, and four.

Using `documentId` as the partition key keeps all of a document's chunks in one logical partition. Single-batch operations on those chunks are atomic, and the write load for any one document lands on a single logical partition. It also makes pattern two a point read when the `documentId` is already known, and keeps edits and deletes (patterns three and four) scoped to one partition. The admin list in pattern three is still a cross-partition query, which is acceptable since it's low volume.

``` mermaid
---
config:
  layout: elk
  theme: dark
---
classDiagram
    direction LR

    namespace CosmosDB_RAG_Database {
        class SourceDocuments {
            <<Container>>
            +String id [Document_ID, PartitionKey]
            +String title
            +String sourceUri
            +String contentHash
            +Int activeVersion
            +DateTime uploadedAt
            +String author
            +String fileType
            +Array~String~ tags
            +Int totalChunks
        }

        class DocumentChunks {
            <<Container - Vector Enabled>>
            +String id [Chunk_ID]
            +String documentId [PartitionKey, Ref]
            +Int chunkIndex
            +String content
            +Array~Float~ embedding [VectorIndex_1536d]
            +String embeddingModel
            +Int version
            +Int pageNumber
            +String sectionHeader
            +Int tokenCount
        }
    }

    SourceDocuments "1" ..> "many" DocumentChunks : chunked into (documentId)
```

#### Partition Key

I chose `/documentId` as the partition key for the chunked documents, because the id is the only value that's known in any of the cases an operation touches Cosmos - specifically, when a document is being updated/deleted. Due to this reality, the only other times that an operation interacts with CosmosDB is when making a write action to the database - write, update, or delete - all of which will be cases when we know what the `documentId` which will eliminate working cross partition in those cases. Moreover, having chunks partitioned by `documentId`.

#### Potential growth opportunities

- If the solution would ever need to support multiple tenants, the database could be altered to use a hierarchical partition key namely `/tenantId/documentId`.

#### Known tradeoffs

- because a `documentId` isn't known at the time of a search, the vector search operation must be across all partitions and will inevitably cost more request units.

<!-- ### Why both Cosmos DB and pgvector
An opportunity to learn on both systems, understand tradeoffs and where both options outperform the other.  -->

<!-- ### Cache invalidation through the change feed
Why event-driven invalidation instead of TTL alone; how stale reads are bounded. -->

<!-- ### Service Bus over Event Grid for ingestion
Ordering, retries, dead-lettering, and why ingestion is a work queue rather than an event notification. -->

<!-- ### Managed identity everywhere
What it removed (connection strings, secret rotation burden) and what it required. -->

## Running locally

Prerequisites: .NET SDK, Docker, and the Azure CLI.

1. Clone the repo locally.
2. configure...
3. run...

## Deploying to Azure

<!-- Infrastructure steps or IaC commands. -->
Currently I have a manual deployment process, which will be moved to a CI/CD pipeline in GitHub Actions when the process is more mature.

## Observability and Validation

### Scaling Test

I executed a load test to validate the scaling rules. See the [scripts](Tests/loadtest) or the [results](Tests/loadtest/results).
![Replica count during scaling](Docs/ReplicaCount.png)
![Requests during load test](Docs/Requests.png)

<!-- 2–3 KQL queries with a sentence each on what they reveal, e.g. slowest search requests, ingestion failures, cache hit rate. -->

<!-- ## Cost
Approximate monthly cost of the running deployment and the biggest cost drivers. -->

---

Built as hands-on preparation for Microsoft's AI-200 (Azure AI Cloud Developer Associate) certification.
