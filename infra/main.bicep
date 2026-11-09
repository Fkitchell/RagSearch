var location = resourceGroup().location

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
  name: 'ragsearch${uniqueString(resourceGroup().id)}'
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

resource OpenAIServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openAIService.id, RagSearchManagedIdentity.id, 'cognitive-services-contributor')
  scope: openAIService
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    )
    principalType: 'ServicePrincipal'
    principalId: RagSearchManagedIdentity.properties.principalId
  }
}

resource ContainerAppEnv 'Microsoft.App/managedEnvironments@2026-07-01' = {
  name: 'RagSearchContainerAppEnv'
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

resource ContainerApp 'Microsoft.App/containerApps@2026-07-01' = {
  name: 'ragsearch-api'
  location: location
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
          image: '${RagSearchACR.properties.loginServer}/ragsearch-api:a3ad64a'
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
  dependsOn: [
    ACRRoleAssignment
  ]
}
var accountName = toLower('OpenAiService${uniqueString(resourceGroup().id)}')

resource openAIService 'Microsoft.CognitiveServices/accounts@2025-10-01-preview' = {
  name: accountName
  location: location
  kind: 'OpenAI'
  properties: {
    customSubDomainName: accountName
    disableLocalAuth: true
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

resource azOpenAiDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-10-01-preview' = {
  parent: openAIService
  name: 'ragsearch-embedding'
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
