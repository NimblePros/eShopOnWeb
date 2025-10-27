#!/bin/bash
# Deploy eShopOnWeb from pre-built container image
# Configured for Instruqt deployment - accepts environment variables
set -e

# Start timing
START_TIME=$(date +%s)

# Change to repository root (script can be run from anywhere)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

# Configuration (with defaults matching build-push-acr.sh)
ACR_NAME="${ACR_NAME:-alesseshopacr}"
IMAGE_NAME="${IMAGE_NAME:-eshop-web-instruqt}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
LOCATION="${AZURE_LOCATION:-westus2}"
ENV_NAME="${AZURE_ENV_NAME:-eshop-$(date +%s)}"

echo "Deploying eShopOnWeb"
echo "Repository: $REPO_ROOT"
echo "ACR: ${ACR_NAME}.azurecr.io"
echo "Image: ${IMAGE_NAME}:${IMAGE_TAG}"
echo "Location: $LOCATION"
echo "Environment: $ENV_NAME"
echo ""

# Verify ACR exists
if ! az acr show --name "$ACR_NAME" &>/dev/null; then
    echo "❌ ERROR: ACR '$ACR_NAME' not found"
    echo "Build the image first: ./scripts/build-push-acr.sh"
    exit 1
fi

# Verify image exists
if ! az acr repository show --name "$ACR_NAME" --repository "$IMAGE_NAME" &>/dev/null; then
    echo "❌ ERROR: Image '${IMAGE_NAME}' not found in ACR"
    echo "Build the image first: ./scripts/build-push-acr.sh"
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
echo ""
echo "App URL: https://$APP_URL"
echo "Resource Group: $RG_NAME"
echo "App Service: $APP_NAME"

# Calculate elapsed time
END_TIME=$(date +%s)
ELAPSED=$((END_TIME - START_TIME))
MINUTES=$((ELAPSED / 60))
SECONDS=$((ELAPSED % 60))

echo ""
echo "⏱️  Deployment completed in ${MINUTES}m ${SECONDS}s"
echo ""
echo "To delete these resources, run:"
echo "  az group delete --name $RG_NAME --yes --no-wait"

