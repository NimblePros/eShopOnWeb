targetScope = 'resourceGroup'

@minLength(1)
param profileName string

@minLength(1)
param relativeName string

param tags object = {}

@minLength(1)
param primaryId string

@minLength(1)
param secondaryId string

resource profile 'Microsoft.Network/trafficManagerProfiles@2018-08-01' = {
  name: profileName
  location: 'global'
  tags: tags
  properties: {
    profileStatus: 'Enabled'
    trafficRoutingMethod: 'Performance'
    dnsConfig: {
      relativeName: relativeName
      ttl: 30
    }
    monitorConfig: {
      protocol: 'HTTP'
      port: 80
      path: '/'
    }
    endpoints: [
      {
        name: 'primary-endpoint'
        type: 'Microsoft.Network/trafficManagerProfiles/azureEndpoints'
        properties: {
          targetResourceId: primaryId
          endpointStatus: 'Enabled'
        }
      }
      {
        name: 'secondary-endpoint'
        type: 'Microsoft.Network/trafficManagerProfiles/azureEndpoints'
        properties: {
          targetResourceId: secondaryId
          endpointStatus: 'Enabled'
        }
      }
    ]
  }
}

output relativeName string = profile.properties.dnsConfig.relativeName
output profileName string = profile.name
