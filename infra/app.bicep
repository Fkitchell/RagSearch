import * as names from 'names.bicep'

param ragsearchApiImagetag string = ''

param rgLocation string = ''

var rgIdSuffix = uniqueString(resourceGroup().id)

resource RagSearchManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: names.RagSearchManagedIdentityName
}

resource ContainerAppEnv 'Microsoft.App/managedEnvironments@2026-07-01' existing = {
  name: names.RagSearchContainerAppEnvName
}

resource RagSearchACR 'Microsoft.ContainerRegistry/registries@2025-11-01' existing = {
  name: names.RagSearchACRName(rgIdSuffix)
}

resource CosmosDBAccount 'Microsoft.DocumentDB/databaseAccounts@2026-03-15' existing = {
  name: names.CosmosDBAccountName(rgIdSuffix)
}

resource OpenAIService 'Microsoft.CognitiveServices/accounts@2025-10-01-preview' existing = {
  name: names.OpenAIServiceName(rgIdSuffix)
}

resource azOpenAiDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-10-01-preview' existing = {
  name: names.AzOpenAiDeploymentName
}

resource ContainerApp 'Microsoft.App/containerApps@2026-07-01' = {
  name: 'ragsearch-api'
  location: rgLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${RagSearchManagedIdentity.id}': {}
    }
  }
  properties: {
    workloadProfileName: 'Consumption'
    managedEnvironmentId: ContainerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        exposedPort: 0
        traffic: [
          { latestRevision: true, weight: 100 }
        ]
      }
      registries: [
        {
          server: RagSearchACR.properties.loginServer
          identity: RagSearchManagedIdentity.id
        }
      ]
    }
    template: {
      scale: {
        cooldownPeriod: 300
        maxReplicas: 5
        minReplicas: 0
        pollingInterval: 30
        rules: [
          {
            name: 'http-scale-rule'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
      containers: [
        {
          name: 'ragsearch-container'
          image: '${RagSearchACR.properties.loginServer}/ragsearch-api:${ragsearchApiImagetag}'
          env: [
            { name: 'AZURE_CLIENT_ID', value: RagSearchManagedIdentity.properties.clientId }
            { name: 'EntraId__TenantId', value: tenant().tenantId }
            { name: 'Cosmos__Endpoint', value: CosmosDBAccount.properties.documentEndpoint }
            { name: 'Cosmos__DatabaseName', value: names.CosmosDBSqlDatabaseName }
            { name: 'Cosmos__ContainerName', value: names.CosmosDBSqlContainerName }
            { name: 'OpenAI__DeploymentName', value: OpenAIService.name }
            { name: 'OpenAI__EmbeddingModelName', value: azOpenAiDeployment.properties.model.name }
            { name: 'OpenAI__Uri', value: OpenAIService.properties.endpoint }
            { name: 'RateLimit__PermitLimit', value: '50' }
            { name: 'RateLimit__QueueLimit', value: '50' }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/api/health'
                port: 8080
              }
              initialDelaySeconds: 7
              periodSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/api/health'
                port: 8080
              }
              initialDelaySeconds: 0
              periodSeconds: 5
              failureThreshold: 3
              successThreshold: 1
              timeoutSeconds: 5
            }
            {
              failureThreshold: 3
              httpGet: {
                path: '/api/health'
                port: 8080
              }
              initialDelaySeconds: 1
              periodSeconds: 1
              successThreshold: 1
              timeoutSeconds: 3
              type: 'Startup'
            }
          ]
          resources: {
            cpu: 1
            memory: '2Gi'
          }
        }
      ]
    }
  }
}
