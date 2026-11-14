from pathlib import Path
from seedlib.documents import SourceDocument

DOCS_DIR = Path(__file__).resolve().parent / "sample_docs"

def chunk_text(text: str, size: int, overlap: int) -> list[str]:
    if overlap >= size:
        raise ValueError("Overlap cannot be larger than or equal to size.")

    chunked_text = []
    text_trimmed = text.strip()
    text_remains = bool(text_trimmed)
    start = 0
    end = size

    while text_remains:
        chunked_text.append(text_trimmed[start:end])

        start = end - overlap
        end = start + size

        text_remains = len(text_trimmed) - start > overlap

    return chunked_text

def load_docs() -> list[SourceDocument]:
    files = DOCS_DIR.glob("*.txt")    
    result = []
    
    for file in files:
       result.append(SourceDocument(file.stem, file.read_text()))
       
    return result
