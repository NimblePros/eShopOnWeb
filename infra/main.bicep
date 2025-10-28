targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param primaryLocation string = 'spaincentral'

@minLength(1)
@description('Secondary location for secondary resources')
param secondaryLocation string = 'eastus'

// Optional parameters to override the default azd resource naming conventions. Update the main.parameters.json file to provide values. e.g.,:
// "resourceGroupName": {
//      "value": "myGroupName"
// }
param resourceGroupName string = ''
param webServiceName string = ''
param catalogDatabaseName string = 'catalogDatabase'
param catalogDatabaseServerName string = ''
param identityDatabaseName string = 'identityDatabase'
param identityDatabaseServerName string = ''
param appServicePlanName string = ''
param secondaryAppServicePlanName string = ''
param keyVaultName string = ''

@description('Id of the user or app to assign application roles')
param principalId string = ''

@description('Whether to deploy Azure SQL Server and databases. Set to false to use in-memory DB.')
param deploySql bool = false

@secure()
@description('SQL Server administrator password (required when deploySql is true)')
param sqlAdminPassword string = ''

@secure()
@description('Application user password (required when deploySql is true)')
param appUserPassword string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var resourceTokenPrimary = toLower(uniqueString(subscription().id, environmentName, primaryLocation))
var resourceTokenSecondary = toLower(uniqueString(subscription().id, environmentName, secondaryLocation))
var tags = { 'azd-env-name': environmentName }

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: primaryLocation
  tags: tags
}

// The application frontend
module web './core/host/appservice.bicep' = {
  name: 'web'
  scope: rg
  params: {
    name: !empty(webServiceName) ? webServiceName : '${abbrs.webSitesAppService}web-${resourceTokenPrimary}'
    location: primaryLocation
    appServicePlanId: appServicePlan.outputs.id
    keyVaultName: keyVault.outputs.name
    runtimeName: 'dotnetcore'
    runtimeVersion: '9.0'
    tags: union(tags, { 'azd-service-name': 'web' })
    appSettings: deploySql ? {
      AZURE_SQL_CATALOG_CONNECTION_STRING_KEY: 'AZURE-SQL-CATALOG-CONNECTION-STRING'
      AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY: 'AZURE-SQL-IDENTITY-CONNECTION-STRING'
      AZURE_KEY_VAULT_ENDPOINT: keyVault.outputs.endpoint
    } : {}
  }
}

module apiKeyVaultAccess './core/security/keyvault-access.bicep' = {
  name: 'api-keyvault-access'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.name
    principalId: web.outputs.identityPrincipalId
  }
}

// The application database: Catalog
module catalogDb './core/database/sqlserver/sqlserver.bicep' = if (deploySql) {
  name: 'sql-catalog'
  scope: rg
  params: {
    name: !empty(catalogDatabaseServerName) ? catalogDatabaseServerName : '${abbrs.sqlServers}catalog-${resourceTokenPrimary}'
    databaseName: catalogDatabaseName
    location: primaryLocation
    tags: tags
    sqlAdminPassword: sqlAdminPassword
    appUserPassword: appUserPassword
    keyVaultName: keyVault.outputs.name
    connectionStringKey: 'AZURE-SQL-CATALOG-CONNECTION-STRING'
  }
}

// The application database: Identity
module identityDb './core/database/sqlserver/sqlserver.bicep' = if (deploySql) {
  name: 'sql-identity'
  scope: rg
  params: {
    name: !empty(identityDatabaseServerName) ? identityDatabaseServerName : '${abbrs.sqlServers}identity-${resourceTokenPrimary}'
    databaseName: identityDatabaseName
    location: primaryLocation
    tags: tags
    sqlAdminPassword: sqlAdminPassword
    appUserPassword: appUserPassword
    keyVaultName: keyVault.outputs.name
    connectionStringKey: 'AZURE-SQL-IDENTITY-CONNECTION-STRING'
  }
}

// Store secrets in a keyvault
module keyVault './core/security/keyvault.bicep' = {
  name: 'keyvault'
  scope: rg
  params: {
    name: !empty(keyVaultName) ? keyVaultName : '${abbrs.keyVaultVaults}${resourceTokenPrimary}'
    location: primaryLocation
    tags: tags
    principalId: principalId
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU (primary)
module appServicePlan './core/host/appServicePlanPrimary.bicep' = {
  name: 'appserviceplan-primary'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceTokenPrimary}'
    location: primaryLocation
    tags: tags
    sku: {
      name: 'B1'
    }
  }
}

// Secondary App Service Plan in the secondary location
module appServicePlanSecondary './core/host/appserviceplan.bicep' = {
  name: 'appserviceplan-secondary'
  scope: rg
  params: {
    name: !empty(secondaryAppServicePlanName) ? secondaryAppServicePlanName : '${abbrs.webServerFarms}${resourceTokenSecondary}'
    location: secondaryLocation
    tags: tags
    sku: {
      name: 'B1'
    }
  }
}

// Data outputs
output AZURE_SQL_CATALOG_CONNECTION_STRING_KEY string = deploySql ? catalogDb.outputs.connectionStringKey : ''
output AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY string = deploySql ? identityDb.outputs.connectionStringKey : ''
output AZURE_SQL_CATALOG_DATABASE_NAME string = deploySql ? catalogDb.outputs.databaseName : ''
output AZURE_SQL_IDENTITY_DATABASE_NAME string = deploySql ? identityDb.outputs.databaseName : ''

// App outputs
output AZURE_LOCATION string = primaryLocation
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_KEY_VAULT_ENDPOINT string = deploySql ? keyVault.outputs.endpoint : ''
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
