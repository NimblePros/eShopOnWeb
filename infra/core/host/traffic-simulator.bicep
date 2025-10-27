param name string
param location string = resourceGroup().location
param tags object = {}

param containerImage string
param containerRegistry string
param targetUrl string

param cpu int = 1
param memoryInGb int = 2
param restartPolicy string = 'Always'

resource containerInstance 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    containers: [
      {
        name: 'traffic-simulator'
        properties: {
          image: '${containerRegistry}/${containerImage}'
          resources: {
            requests: {
              cpu: cpu
              memoryInGB: memoryInGb
            }
          }
          environmentVariables: [
            {
              name: 'APP_URL'
              value: targetUrl
            }
          ]
        }
      }
    ]
    osType: 'Linux'
    restartPolicy: restartPolicy
    imageRegistryCredentials: []
  }
}

output id string = containerInstance.id
output name string = containerInstance.name

