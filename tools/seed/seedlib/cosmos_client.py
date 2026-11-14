from os import environ
from azure.identity import DefaultAzureCredential
from azure.cosmos import CosmosClient


class CosmosDocsClient:
    def __init__(self):
        client = CosmosClient(
            environ["COSMOS_ENDPOINT"], credential=DefaultAzureCredential()
        )
        self.container = client.get_database_client(
            environ["COSMOS_DATABASE"]
        ).get_container_client(environ["COSMOS_CONTAINER"])

    def upsert(self, item: dict) -> dict:
        return self.container.upsert_item(item)
