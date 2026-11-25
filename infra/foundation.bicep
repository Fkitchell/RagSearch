import * as names from 'names.bicep'

param myPrincipalId string = ''

var location = resourceGroup().location
var rgIdSuffix = uniqueString(resourceGroup().id)

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: 'RagSearchLogAnalyticsWorkspace'
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'pergb2018'
    }
  }
}

resource RagSearchACR 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: names.RagSearchACRName(rgIdSuffix)
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    networkRuleBypassAllowedForTasks: false
    adminUserEnabled: false
    roleAssignmentMode: 'LegacyRegistryPermissions'
  }
}

resource RagSearchManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'RagSearch_MI'
  location: location
}

resource ACRRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(RagSearchACR.id, RagSearchManagedIdentity.id, 'acrpull')
  scope: RagSearchACR
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    )
    principalType: 'ServicePrincipal'
    principalId: RagSearchManagedIdentity.properties.principalId
  }
}

resource MyACRRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (myPrincipalId != '') {
  name: guid(RagSearchACR.id, myPrincipalId, 'owner')
  scope: RagSearchACR
  properties: {
    principalId: myPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '')
  }
}

resource OpenAIServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(OpenAIService.id, RagSearchManagedIdentity.id, 'cognitive-services-contributor')
  scope: OpenAIService
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    )
    principalType: 'ServicePrincipal'
    principalId: RagSearchManagedIdentity.properties.principalId
  }
}

resource CosmosDBSQLDataContributorRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2026-03-15' = {
  parent: CosmosDBAccount
  name: guid(
    RagSearchManagedIdentity.id,
    CosmosDBAccount.name,
    CosmosDBSqlDatabase.name,
    'Cosmos DB Built-in Data Contributor'
  )
  properties: {
    principalId: RagSearchManagedIdentity.properties.principalId
    roleDefinitionId: resourceId(
      'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',
      CosmosDBAccount.name,
      '00000000-0000-0000-0000-000000000002'
    )
    scope: '${CosmosDBAccount.id}/dbs/${CosmosDBSqlDatabase.name}'
  }
}

resource CosmosDBSqlDataRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2026-03-15' = if (myPrincipalId != '') {
  parent: CosmosDBAccount
  name: guid(myPrincipalId, CosmosDBAccount.name, CosmosDBSqlDatabase.name, 'Cosmos DB Built-in Data Contributor')
  properties: {
    principalId: myPrincipalId
    roleDefinitionId: resourceId(
      'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',
      CosmosDBAccount.name,
      '00000000-0000-0000-0000-000000000002'
    )
    scope: '${CosmosDBAccount.id}/dbs/${CosmosDBSqlDatabase.name}'
  }
}

resource ContainerAppEnv 'Microsoft.App/managedEnvironments@2026-07-01' = {
  name: names.RagSearchContainerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    environmentMode: 'WorkloadProfiles'
    peerAuthentication: {
      mtls: {
        enabled: false
      }
    }
    peerTrafficConfiguration: { encryption: { enabled: false } }
    publicNetworkAccess: 'Enabled'
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

var accountName = names.OpenAIServiceName(rgIdSuffix)

resource OpenAIService 'Microsoft.CognitiveServices/accounts@2025-10-01-preview' = {
  name: accountName
  location: location
  kind: 'OpenAI'
  properties: {
    customSubDomainName: accountName
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${RagSearchManagedIdentity.id}': {}
    }
  }
  sku: {
    name: 'S0'
  }
}

resource AzOpenAiDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-10-01-preview' = {
  parent: OpenAIService
  name: names.AzOpenAiDeploymentName
  properties: {
    model: {
      name: 'text-embedding-3-small'
      format: 'OpenAI'
      version: '1'
    }
  }
  sku: {
    name: 'standard'
    capacity: 80
  }
}

param cosmosLocation string = 'eastus2'

resource CosmosDBAccount 'Microsoft.DocumentDB/databaseAccounts@2026-03-15' = {
  name: names.CosmosDBAccountName(rgIdSuffix)
  location: cosmosLocation
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
    locations: [
      {
        locationName: cosmosLocation
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableNoSQLVectorSearch'
      }
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource CosmosDBSqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2026-03-15' = {
  parent: CosmosDBAccount
  name: names.CosmosDBSqlDatabaseName
  properties: {
    resource: {
      id: 'CosmosDB_RAG'
    }
  }
}

resource CosmosDBSqlContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2026-03-15' = {
  parent: CosmosDBSqlDatabase
  name: names.CosmosDBSqlContainerName
  properties: {
    resource: {
      id: 'VectorSearch'
      partitionKey: {
        paths: ['/documentId']
        kind: 'Hash'
        version: 2
      }
      vectorEmbeddingPolicy: {
        vectorEmbeddings: [
          {
            path: '/embedding'
            dataType: 'float32'
            distanceFunction: 'cosine'
            dimensions: 1536
          }
        ]
      }
      indexingPolicy: {
        includedPaths: [{ path: '/*' }]
        excludedPaths: [{ path: '/"_etag"/?' }, { path: '/embedding/*' }]
        vectorIndexes: [{ path: '/embedding', type: 'quantizedFlat' }]
      }
    }
  }
}
