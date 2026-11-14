from datetime import datetime, timezone
from seedlib.cosmos_client import CosmosDocsClient
from seedlib.azure_openai_client import AzureOpenAIClient
from seedlib.chunking import load_docs, chunk_text, DOCS_DIR

openai_client = AzureOpenAIClient()
cosmos = CosmosDocsClient()

docs = load_docs()
if not docs:
    raise SystemExit(f"No .txt files found in {DOCS_DIR}")

for doc_id, text in docs:
    chunks = chunk_text(text, 500, 100)
    vectors = openai_client.embed(chunks)
    uploaded_at = datetime.now(timezone.utc).isoformat()

    for i, (chunk, vector) in enumerate(zip(chunks, vectors, strict=True)):
        cosmos.upsert(
            {
                "id": f"{doc_id}:{i}",
                "documentId": doc_id,
                "chunkIndex": i,
                "title": doc_id.replace("-", " "),
                "author": "Aesop",
                "fileType": "txt",
                "content": chunk,
                "embedding": vector,
                "embeddingModel": openai_client.embedding_model_name,
                "version": 0,
                "uploadedAt": uploaded_at,
            }
        )
    print(f"{doc_id}: upserted {len(chunks)} chunks")
