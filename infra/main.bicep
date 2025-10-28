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

// The application frontend (primary)
module webPrimary './core/host/appservice.bicep' = {
  name: 'web-primary'
  scope: rg
  params: {
    name: !empty(webServiceName) ? '${webServiceName}-${primaryLocation}' : '${abbrs.webSitesAppService}web-${resourceTokenPrimary}'
    location: primaryLocation
    appServicePlanId: appServicePlan.outputs.id
    runtimeName: 'dotnetcore'
    runtimeVersion: '9.0'
    tags: union(tags, { 'azd-service-name': 'web-primary' })
    enableSlot: true
    slotName: slotName
  }
}

// The application frontend (secondary)
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
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU (primary)
module appServicePlan './core/host/appserviceplan.bicep' = {
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