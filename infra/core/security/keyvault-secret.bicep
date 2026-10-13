param name string
param keyVaultName string

@secure()
param secretValue string

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: name
  properties: {
    value: secretValue
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}
