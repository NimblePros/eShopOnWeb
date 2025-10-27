#!/bin/bash
# Deploy eShopOnWeb from pre-built container image
set -e

# Required configuration
ACR_NAME="${ACR_NAME}"
LOCATION="${AZURE_LOCATION:-westus2}"
ENV_NAME="${AZURE_ENV_NAME:-eshop-$(date +%s)}"
IMAGE_NAME="${IMAGE_NAME:-eshop-web}"
IMAGE_TAG="${IMAGE_TAG:-latest}"

# Validate required parameters
if [ -z "$ACR_NAME" ]; then
    echo "❌ ERROR: ACR_NAME is required"
    echo ""
    echo "Usage:"
    echo "  export ACR_NAME=<your-acr-name>"
    echo "  ./scripts/deploy-container-instruqt.sh"
    echo ""
    echo "Or build the image first:"
    echo "  ./scripts/build-push-acr.sh <your-acr-name>"
    exit 1
fi

echo "Deploying eShopOnWeb"
echo "ACR: ${ACR_NAME}.azurecr.io | Image: ${IMAGE_NAME}:${IMAGE_TAG}"
echo "Location: $LOCATION | Environment: $ENV_NAME"
echo ""

# Verify ACR exists
if ! az acr show --name "$ACR_NAME" &>/dev/null; then
    echo "❌ ERROR: ACR '$ACR_NAME' not found"
    echo "Create it with: ./scripts/build-push-acr.sh $ACR_NAME"
    exit 1
fi

# Verify image exists
if ! az acr repository show --name "$ACR_NAME" --repository "$IMAGE_NAME" &>/dev/null; then
    echo "❌ ERROR: Image '${IMAGE_NAME}' not found in ACR"
    echo "Build it with: ./scripts/build-push-acr.sh $ACR_NAME"
    exit 1
fi

echo "✅ ACR and image verified"

# Deploy infrastructure
echo "Deploying infrastructure (3-4 min)..."
PRINCIPAL_ID=$(az ad signed-in-user show --query id -o tsv 2>/dev/null || echo "")

az deployment sub create \
  --name "deploy-${ENV_NAME}" \
  --location "$LOCATION" \
  --template-file infra/main.bicep \
  --parameters \
    environmentName="$ENV_NAME" \
    location="$LOCATION" \
    containerRegistry="${ACR_NAME}.azurecr.io" \
    containerImage="$IMAGE_NAME" \
    containerTag="$IMAGE_TAG" \
    principalId="$PRINCIPAL_ID" \
    sqlAdminPassword="SQL$(openssl rand -hex 12)!" \
    appUserPassword="APP$(openssl rand -hex 12)!" \
  --output none

# Get app info
RG_NAME=$(az group list --tag azd-env-name="$ENV_NAME" --query "[0].name" -o tsv)
APP_NAME=$(az webapp list --resource-group "$RG_NAME" --query "[0].name" -o tsv)
APP_URL=$(az webapp show --name "$APP_NAME" --resource-group "$RG_NAME" --query "defaultHostName" -o tsv)

echo ""
echo "✅ Deployment Complete!"
echo "App URL: https://$APP_URL"
echo ""
echo "Cleanup: az group delete --name $RG_NAME --yes --no-wait"

