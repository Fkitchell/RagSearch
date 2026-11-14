from seedlib.documents import SourceDocument
from os import environ
from azure.identity import DefaultAzureCredential
from azure.cosmos import CosmosClient, DatabaseAccount, container


class CosmosDocsClient:
    cosmos_client: CosmosClient
    cosmos_database: DatabaseAccount
    cosmos_container: container

    def __init__(self):
        cosmos_endpoint = environ["COSMOS_ENDPOINT"]
        cosmos_database_name = environ["COSMOS_DATABASE"]
        cosmos_container_name = environ["COSMOS_CONTAINER"]

        credential = DefaultAzureCredential()

        global cosmos_client
        global cosmos_database
        global cosmos_container

        cosmos_client = CosmosClient(url=cosmos_endpoint, credential=credential)
        cosmos_database = cosmos_client.get_database_client(cosmos_database_name)
        cosmos_container = cosmos_database.get_container_client(cosmos_container_name)

    def upsert(self, doc: SourceDocument):
        result = cosmos_container.upsert_item(doc)
        for key in result.keys:
            print(key)
        
        return
