param name string
param targetResourceUri string
param minimum string = '1'
param maximum string = '2'
param default string = '1'

param location string = resourceGroup().location
param tags object = {}

resource scaleOnCpu 'Microsoft.Insights/autoscaleSettings@2015-04-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    enabled: true
    targetResourceUri: targetResourceUri
    targetResourceLocation: location
    notifications: []
    profiles: [
      {
        name: 'Scale on CPU'
        capacity: {
          minimum: minimum
          maximum: maximum
          default: default
        }
        rules: [
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: targetResourceUri
              operator: 'GreaterThan'
              statistic: 'Average'
              threshold: 70
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
            }
            scaleAction: {
              cooldown: 'PT5M'
              direction: 'Increase'
              type: 'ChangeCount'
              value: '1'
            }
          }
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: targetResourceUri
              operator: 'LessThan'
              statistic: 'Average'
              threshold: 50
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
            }
            scaleAction: {
              cooldown: 'PT5M'
              direction: 'Decrease'
              type: 'ChangeCount'
              value: '1'
            }
          }
        ]
      }
    ]
  }
}