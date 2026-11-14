from time import struct_time


class SourceDocument:
    def __init__(
        self,
        document_id: str,
        title: str,
        active_version: int,
        uploaded_at: struct_time,
        author: str,
        file_type: str,
        total_chunks: int,
        document_chunks: list,
    ):
        self.document_id: str = document_id
        self.title: str = title
        self.active_version: int = active_version
        self.uploaded_at: struct_time = uploaded_at
        self.author: str = author
        self.file_type: str = file_type
        self.total_chunks: int = total_chunks
        self.document_chunks: list[SourceDocument] = document_chunks


class DocumentChunk:
    def __init__(
        self,
        chunk_id,  # also chunk_index
        document_id,
        content,
        embedding,
        embedding_model,
        version,
        page_number,
        section_header,
        token_count,
    ):
        self.chunk_id = chunk_id
        self.document_id = document_id
        self.content = content
        self.embedding = embedding
        self.embedding_model = embedding_model
        self.version = version
        self.page_number = page_number
        self.section_header = section_header
        self.token_count = token_count
