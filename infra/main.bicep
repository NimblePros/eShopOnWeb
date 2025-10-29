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
param webServiceName string = 'ek-cloudx-associate-shop'
param appServicePlanName string = ''
param secondaryAppServicePlanName string = ''
param slotName string = 'test'

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

// Public API
module publicApi './core/host/appservice.bicep' = {
  name: 'public-api'
  scope: rg
  params: {
    name: !empty(webServiceName) ? '${webServiceName}-api' : '${abbrs.webSitesAppService}web-${resourceTokenPrimary}-api'
    location: primaryLocation
    appServicePlanId: publicApiPlan.outputs.id
    runtimeName: 'dotnetcore'
    runtimeVersion: '9.0'
    tags: union(tags, { 'azd-service-name': 'public-api' })
    appSettings: {
      BaseUrls__WebBase: 'https://${webServiceName}-${environmentName}.trafficmanager.net'
    }
    allowedOrigins: ['https://${webServiceName}-${environmentName}.trafficmanager.net']
  }
}

// Public API scale rule
module publicApiAutoscale './core/host/scaleoncpu.bicep' = {
  name: 'scale-rule'
  scope: rg
  params: {
    name: '${webServiceName}-api-autoscale-rule-${environmentName}'
    location: primaryLocation
    tags: tags
    targetResourceUri: '/subscriptions/${subscription().id}/resourceGroups/${rg.name}/providers/Microsoft.Web/serverfarms/${publicApiPlan.outputs.id}'
  }
}

// Web (primary)
module webPrimary './core/host/appservice.bicep' = {
  name: 'web-primary'
  scope: rg
  params: {
    name: !empty(webServiceName) ? '${webServiceName}-${primaryLocation}' : '${abbrs.webSitesAppService}web-${resourceTokenPrimary}'
    location: primaryLocation
    appServicePlanId: appServicePlanPrimary.outputs.id
    runtimeName: 'dotnetcore'
    runtimeVersion: '9.0'
    tags: union(tags, { 'azd-service-name': 'web-primary' })
    appSettings: {
      BaseUrls__ApiBase: '${publicApi.outputs.uri}/api/'
    }
    enableSlot: true
    slotName: slotName
  }
}

// Web (secondary)
module webSecondary './core/host/appservice.bicep' = {
  name: 'web-secondary'
  scope: rg
  params: {
    name: !empty(webServiceName) ? '${webServiceName}-${secondaryLocation}' : '${abbrs.webSitesAppService}web-${resourceTokenSecondary}'
    location: secondaryLocation
    appServicePlanId: appServicePlanSecondary.outputs.id
    runtimeName: 'dotnetcore'
    runtimeVersion: '9.0'
    tags: union(tags, { 'azd-service-name': 'web-secondary' })
    appSettings: {
      BaseUrls__ApiBase: '${publicApi.outputs.uri}/api/'
    }
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU (primary)
module appServicePlanPrimary './core/host/appserviceplan.bicep' = {
  name: 'appserviceplan-primary'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceTokenPrimary}'
    location: primaryLocation
    tags: tags
    sku: { 
        name: 'S1'
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

// Create an App Service Plan to group applications under the same payment plan and SKU (primary)
module publicApiPlan './core/host/appserviceplan.bicep' = {
  name: 'publicapiplan'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? '${appServicePlanName}-app' : '${abbrs.webServerFarms}app-${resourceTokenPrimary}'
    location: primaryLocation
    tags: tags
    sku: { 
        name: 'S1'
    }
  }
}

// Traffic Manager deployed as a module at resource-group scope
module trafficManager './core/network/trafficManager.bicep' = {
  name: 'traffic-manager'
  scope: rg
  params: {
    profileName: 'tm-${webServiceName}-${environmentName}'
    relativeName: '${webServiceName}-${environmentName}'
    tags: tags
    primaryId: webPrimary.outputs.id
    secondaryId: webSecondary.outputs.id
  }
}

output appUrl string = 'http://${trafficManager.outputs.relativeName}.trafficmanager.net'
