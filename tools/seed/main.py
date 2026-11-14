import time
from seedlib.documents import DocumentChunk, SourceDocument
from seedlib.cosmos_client import CosmosDocsClient
from seedlib.azure_openai_client import AzureOpenAIClient
from seedlib.chunking import load_docs, chunk_text

openai_client = AzureOpenAIClient()

docs = load_docs()
chunks = []
embeddings = []

for doc in docs:
    chunks = chunk_text(doc, 500, 100)
    embeddings = openai_client.embed(chunks)
    print(len(embeddings))
    cosmos_resp = CosmosDocsClient.upsert(
        SourceDocument(
            document_id=doc.documentId,
            title=doc.documentId.replace("-", " "),
            active_version=0,
            uploaded_at=time.gmtime(),
            author="Aesop",
            file_type=doc.file_type,
            total_chunks=len(chunks),
            document_chunks=[
                DocumentChunk(
                    chunk_id=embedding.index,
                    document_id=doc.document_id,
                    content=chunks[embedding.index],
                    embedding=embeddings,
                    embedding_model=openai_client.embedding_model,
                    version=0,
                    page_number=1,
                    section_header="",
                    token_count=len(chunks[embedding.index]),
                )
                for embedding in embeddings
            ],
        )
    )
