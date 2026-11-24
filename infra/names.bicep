@export()
var RagSearchContainerAppEnvName = 'RagSearchContainerAppEnv'

@export()
var RagSearchManagedIdentityName = 'RagSearch_MI'

@export()
var CosmosDBSqlDatabaseName = 'CosmosDB_RAG'

@export()
var CosmosDBSqlContainerName = 'VectorSearch'

@export()
func OpenAIServiceName(suffix string) string => toLower('OpenAiService${suffix}')

@export()
func RagSearchACRName(suffix string) string => 'ragsearch${suffix}'

@export()
func CosmosDBAccountName(suffix string) string => 'ragsearchcosmosdb${suffix}'

@export()
var AzOpenAiDeploymentName = 'ragsearch-embedding'
