param name string
param location string = resourceGroup().location
param tags object = {}

// Reference Properties
param applicationInsightsName string = ''
param appServicePlanId string
param keyVaultName string = ''
param managedIdentity bool = !empty(keyVaultName)

// Container configuration
param containerRegistry string
param containerImage string
param containerTag string = 'latest'
param mainContainerName string = 'main'

// Datadog configuration
@secure()
param ddApiKey string = ''
param ddSite string = 'us3.datadoghq.com'
param ddService string = ''

// App Service configuration
param allowedOrigins array = []
param alwaysOn bool = true
param appCommandLine string = ''
param appSettings object = {}
param clientAffinityEnabled bool = false
param functionAppScaleLimit int = -1
param minimumElasticInstanceCount int = -1
param numberOfWorkers int = -1
param use32BitWorkerProcess bool = false
param ftpsState string = 'FtpsOnly'
param healthCheckPath string = '/health'

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux,container'
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: 'sitecontainers'
      alwaysOn: alwaysOn
      ftpsState: ftpsState
      minTlsVersion: '1.2'
      appCommandLine: appCommandLine
      numberOfWorkers: numberOfWorkers != -1 ? numberOfWorkers : null
      minimumElasticInstanceCount: minimumElasticInstanceCount != -1 ? minimumElasticInstanceCount : null
      use32BitWorkerProcess: use32BitWorkerProcess
      functionAppScaleLimit: functionAppScaleLimit != -1 ? functionAppScaleLimit : null
      healthCheckPath: !empty(healthCheckPath) ? healthCheckPath : null
      cors: {
        allowedOrigins: union([ 'https://portal.azure.com', 'https://ms.portal.azure.com' ], allowedOrigins)
      }
      acrUseManagedIdentityCreds: false  // Use anonymous pull (ACR Standard tier)
    }
    clientAffinityEnabled: clientAffinityEnabled
    httpsOnly: true
  }

  identity: { type: managedIdentity ? 'SystemAssigned' : 'None' }

  resource configAppSettings 'config' = {
    name: 'appsettings'
    properties: union(appSettings,
      {
        // Application settings
        ASPNETCORE_ENVIRONMENT: 'Production'
        // Container settings
        DOCKER_REGISTRY_SERVER_URL: 'https://${containerRegistry}'
        WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'false'
        DOCKER_ENABLE_CI: 'true'
        // Health check configuration
        WEBSITE_HEALTHCHECK_MAXPINGFAILURES: '10'
        WEBSITE_HEALTHCHECK_MAXUNHEALTHYWORKERPERCENT: '50'
        WEBSITE_SWAP_WARMUP_PING_PATH: !empty(healthCheckPath) ? healthCheckPath : '/'
        WEBSITE_SWAP_WARMUP_PING_STATUSES: '200'
        // Performance optimizations
        WEBSITE_ENABLE_SYNC_UPDATE_SITE: 'true'
        WEBSITE_TIME_ZONE: 'UTC'
      },
      !empty(applicationInsightsName) ? { APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsights!.properties.ConnectionString } : {},
      !empty(keyVaultName) ? { AZURE_KEY_VAULT_ENDPOINT: keyVault!.properties.vaultUri } : {},
      !empty(ddApiKey) ? {
        // Datadog configuration for main container
        DD_API_KEY: ddApiKey
        DD_SITE: ddSite
        DD_SERVICE: ddService
        DD_ENV: 'dd-eshoponweb'
        DD_VERSION: '1.0'
        DD_SOURCE: 'csharp'
        DD_SERVERLESS_LOG_PATH: '/home/LogFiles/*.log'
        DD_AAS_INSTANCE_LOGGING_ENABLED: 'true'
        // .NET Tracer configuration (required for containerized .NET apps)
        DD_DOTNET_TRACER_HOME: '/datadog/tracer'
        DD_TRACE_LOG_DIRECTORY: '/home/LogFiles/dotnet'
        CORECLR_ENABLE_PROFILING: '1'
        CORECLR_PROFILER: '{846F5F1C-F9AE-4B07-969E-05C26BC060D8}'
        CORECLR_PROFILER_PATH: '/datadog/tracer/Datadog.Trace.ClrProfiler.Native.so'
      } : {})
  }

  resource configLogs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: { fileSystem: { level: 'Verbose' } }
      detailedErrorMessages: { enabled: true }
      failedRequestsTracing: { enabled: true }
      httpLogs: { fileSystem: { enabled: true, retentionInDays: 1, retentionInMb: 35 } }
    }
    dependsOn: [
      configAppSettings
    ]
  }

  // Main application container
  resource mainContainer 'sitecontainers' = {
    name: mainContainerName
    properties: {
      image: '${containerRegistry}/${containerImage}:${containerTag}'
      isMain: true
      startUpCommand: appCommandLine
    }
    dependsOn: [
      configAppSettings
    ]
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = if (!(empty(keyVaultName))) {
  name: keyVaultName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = if (!empty(applicationInsightsName)) {
  name: applicationInsightsName
}

output identityPrincipalId string = managedIdentity ? appService.identity.principalId : ''
output name string = appService.name
output uri string = 'https://${appService.properties.defaultHostName}'
