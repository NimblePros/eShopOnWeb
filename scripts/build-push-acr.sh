#!/bin/bash
# Build and push Docker image to Azure Container Registry
set -e

ACR_NAME="${1:-}"
IMAGE_NAME="${2:-eshop-web}"
IMAGE_TAG="${3:-latest}"

if [ -z "$ACR_NAME" ]; then
    echo "Usage: $0 <acr-name> [image-name] [image-tag]"
    echo "Example: $0 myacr eshop-web latest"
    exit 1
fi

echo "Building image in ACR: $ACR_NAME"
echo "⏳ This will take 2-4 minutes..."
echo ""

# Check if ACR exists, create if not
if ! az acr show --name "$ACR_NAME" &>/dev/null; then
    echo "Creating ACR..."
    az acr create \
      --name "$ACR_NAME" \
      --resource-group rg-eshop-acr \
      --location eastus \
      --sku Standard \
      --output none
    
    # Enable anonymous pull for Standard tier
    az acr update --name "$ACR_NAME" --anonymous-pull-enabled true --output none
fi

# Build and push image
az acr build \
  --registry "$ACR_NAME" \
  --image "${IMAGE_NAME}:${IMAGE_TAG}" \
  --file src/Web/Dockerfile \
  --platform linux \
  . \
  --no-logs

echo ""
echo "✅ Image built: ${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"
echo ""
echo "Deploy to Azure:"
echo "  az deployment sub create \\"
echo "    --location westus2 \\"
echo "    --template-file infra/main.bicep \\"
echo "    --parameters \\"
echo "      environmentName='eshop-prod' \\"
echo "      containerRegistry='${ACR_NAME}.azurecr.io' \\"
echo "      containerImage='${IMAGE_NAME}' \\"
echo "      containerTag='${IMAGE_TAG}' \\"
echo "      principalId=\$(az ad signed-in-user show --query id -o tsv) \\"
echo "      sqlAdminPassword='SQL\$(openssl rand -hex 12)!' \\"
echo "      appUserPassword='APP\$(openssl rand -hex 12)!'"

