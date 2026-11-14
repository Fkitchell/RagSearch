import os
from seedlib.chunking import chunk_text
from azure.identity import get_bearer_token_provider, DefaultAzureCredential
from openai import AzureOpenAI

assert chunk_text("abcdefghij", 4, 1) == [
    "abcd",
    "defg",
    "ghij",
]  # expect ['abcd', 'defg', 'ghij']
assert chunk_text("ab", 4, 1) == ["ab"]
assert chunk_text("x" * 50, 500, 100) == [
    "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
]
assert chunk_text("abcdefghijk", 4, 1) == [
    "abcd",
    "defg",
    "ghij",
    "jk",
]  # 11 chars
assert chunk_text(" ", 500, 100) == []  # expect []
try:
    chunk_text("abc", 4, 4)
except ValueError:
    pass
else:
    raise AssertionError("Expected ValueError when overlap is greater than or equal to size")


openai_endpoint = os.environ["AOAI_ENDPOINT"]
credential = DefaultAzureCredential()
token_provider = get_bearer_token_provider(
    credential, "https://cognitiveservices.azure.com/.default"
)

openai_client = AzureOpenAI(
    azure_endpoint=openai_endpoint,
    azure_ad_token_provider=token_provider,
    api_version="2024-06-01",
)

fable_text = """The Jay and the Peacock: A Jay venturing into a yard where Peacocks used to walk, found
there a number of feathers which had fallen from the Peacocks when
they were moulting.  He tied them all to his tail and strutted
down towards the Peacocks.  When he came near them they soon
discovered the cheat, and striding up to him pecked at him and
plucked away his borrowed plumes.  So the Jay could do no better
than go back to the other Jays, who had watched his behaviour from
a distance; but they were equally annoyed with him, and told him:"It is not only fine feathers that make fine birds." """

chunks = chunk_text(fable_text, 500, 100)
vectors = embed(openai_client, chunks)
print(len(chunks), len(vectors), len(vectors[0]))

#len(embed(openai_client, chunk_text("x" * 50, 500, 100)))
#len(embed(openai_client, chunk_text("abcdefghijk", 4, 1))[0])