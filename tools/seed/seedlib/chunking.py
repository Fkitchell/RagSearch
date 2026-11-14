from pathlib import Path

DOCS_DIR = Path(__file__).resolve().parent.parent / "sample_docs"


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


def load_docs() -> list[tuple[str, str]]:
    return [
        (f.stem, f.read_text(encoding="utf-8")) for f in sorted(DOCS_DIR.glob("*.txt"))
    ]
