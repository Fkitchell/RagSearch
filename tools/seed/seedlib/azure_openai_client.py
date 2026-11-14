from os import environ
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

embedding_model: str


class AzureOpenAIClient:
    def __init__(self):
        self.embedding_model_name = environ["AOAI_EMBEDDING_MODEL_NAME"]

        default_credential = DefaultAzureCredential()

        self.client = AzureOpenAI(
            azure_endpoint=environ["AOAI_ENDPOINT"],
            azure_ad_token_provider=get_bearer_token_provider(
                default_credential, "https://cognitiveservices.azure.com/.default"
            ),
            api_version="2024-06-01",
        )

    def embed(self, texts: list[str]) -> list[list[float]]:

        global embedding_model
        
        embedding_model = environ["AOAI_EMBEDDING_DEPLOYMENT"]
        
        embeddings_response = self.client.embeddings.create(
            model=embedding_model, input=texts
        )

        for item in embeddings_response.data:
            if len(item.embedding) != 1536:
                raise ValueError(
                    f"chunk {item.index} expected 1536 dims, got {len(item.embedding)}"
                )

        embeddings_response.data.sort(key=lambda item: item.index)

        return [item.embedding for item in embeddings_response.data]
