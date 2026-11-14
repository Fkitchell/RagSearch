# Dot-source before running: . .\tools\seed\env.ps1
$acct = "ragsearchcosmosdbtpgufq5i7q6ia"
$aoai = "openaiservicetpgufq5i7q6ia"
$rg   = "ragsearch"

$env:COSMOS_ENDPOINT           = az cosmosdb show -n $acct -g $rg --query documentEndpoint -o tsv
$env:COSMOS_DATABASE           = "CosmosDB_RAG"
$env:COSMOS_CONTAINER          = "VectorSearch"
$env:AOAI_ENDPOINT             = az cognitiveservices account show -n $aoai -g $rg --query properties.endpoint -o tsv
$env:AOAI_EMBEDDING_DEPLOYMENT = "ragsearch-embedding"
$env:AOAI_EMBEDDING_MODEL_NAME = "text-embedding-3-small"

"Cosmos: $env:COSMOS_ENDPOINT ($env:COSMOS_DATABASE/$env:COSMOS_CONTAINER)"
"AOAI:   $env:AOAI_ENDPOINT ($env:AOAI_EMBEDDING_DEPLOYMENT/$env:AOAI_EMBEDDING_MODEL_NAME)"