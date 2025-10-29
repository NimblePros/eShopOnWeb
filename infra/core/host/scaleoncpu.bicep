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
    notifications: []
    profiles: [
      {
        name: 'scale-on-cpu'
        capacity: {
          minimum: minimum
          maximum: maximum
          default: default
        }
        rules: [
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricResourceUri: targetResourceUri
              operator: 'GreaterThan'
              statistic: 'Average'
              threshold: 70
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT5M'
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
              metricResourceUri: targetResourceUri
              operator: 'LessThan'
              statistic: 'Average'
              threshold: 30
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT5M'
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