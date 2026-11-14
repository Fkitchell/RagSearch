import os
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from azure.cosmos import CosmosClient
from openai import AzureOpenAI

cosmos_endpoint = os.environ["COSMOS_ENDPOINT"]
cosmos_database_name = os.environ["COSMOS_DATABASE"]
cosmos_container_name = os.environ["COSMOS_CONTAINER"]

openai_endpoint = os.environ["AOAI_ENDPOINT"]

credential = DefaultAzureCredential()

cosmos_client = CosmosClient(url=cosmos_endpoint, credential=credential)
cosmos_database = cosmos_client.get_database_client(cosmos_database_name)
cosmos_container = cosmos_database.get_container_client(cosmos_container_name)

token_provider = get_bearer_token_provider(
    credential, "https://cognitiveservices.azure.com/.default"
)

openai_client = AzureOpenAI(
    azure_endpoint=openai_endpoint,
    azure_ad_token_provider=token_provider,
    api_version="2024-06-01",
)
embedding_model = "ragsearch-embedding"

response = embeddings = openai_client.embeddings.create(
    model=embedding_model,
    input=["Hello world", "My name is John"],
)

print(len(response.data[0].embedding)) # confirm functionality of openAi model and my connection
print(next(cosmos_container.query_items(
        "SELECT VALUE COUNT(1) FROM c", enable_cross_partition_query=True))) # confirm cosmosdb connection works
 
