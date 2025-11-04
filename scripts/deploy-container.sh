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
IMAGE_NAME="${IMAGE_NAME:-eshop-web}"
API_IMAGE_NAME="${API_IMAGE_NAME:-eshop-publicapi}"
TRAFFIC_IMAGE_NAME="${TRAFFIC_IMAGE_NAME:-eshop-traffic-simulator}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
LOCATION="${AZURE_LOCATION:-westus2}"
ENV_NAME="${AZURE_ENV_NAME:-eshop-$(date +%s)}"

# Datadog configuration
DD_API_KEY="${DD_API_KEY:-}"
DD_SITE="${DD_SITE:-us3.datadoghq.com}"

echo "Deploying eShopOnWeb"
echo "Repository: $REPO_ROOT"
echo "ACR: ${ACR_NAME}.azurecr.io"
echo "Web Image: ${IMAGE_NAME}:${IMAGE_TAG}"
echo "API Image: ${API_IMAGE_NAME}:${IMAGE_TAG}"
echo "Traffic Image: ${TRAFFIC_IMAGE_NAME}:${IMAGE_TAG}"
echo "Location: $LOCATION"
echo "Environment: $ENV_NAME"
if [ -n "$DD_API_KEY" ]; then
  echo "Datadog: Enabled (Site: $DD_SITE)"
else
  echo "❌  Please set DD_API_KEY."
  exit 1
fi
echo ""

# Verify ACR exists
if ! az acr show --name "$ACR_NAME" &>/dev/null; then
    echo "❌ ERROR: ACR '$ACR_NAME' not found"
    echo "Build the image first: ./scripts/build-push-acr.sh"
    exit 1
fi

# Verify web image exists
if ! az acr repository show --name "$ACR_NAME" --repository "$IMAGE_NAME" &>/dev/null; then
    echo "❌ ERROR: Web image '${IMAGE_NAME}' not found in ACR"
    echo "Build the image first: ./scripts/build-push-acr.sh"
    exit 1
fi

# Verify API image exists
if ! az acr repository show --name "$ACR_NAME" --repository "$API_IMAGE_NAME" &>/dev/null; then
    echo "❌ ERROR: API image '${API_IMAGE_NAME}' not found in ACR"
    echo "Build the image first: ./scripts/build-push-acr.sh"
    exit 1
fi

# Verify traffic image exists
if ! az acr repository show --name "$ACR_NAME" --repository "$TRAFFIC_IMAGE_NAME" &>/dev/null; then
    echo "❌ ERROR: Traffic simulator image '${TRAFFIC_IMAGE_NAME}' not found in ACR"
    echo "Build the image first: ./scripts/build-push-acr.sh"
    exit 1
fi

echo "✅ ACR and images verified"

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
    apiImage="$API_IMAGE_NAME" \
    containerTag="$IMAGE_TAG" \
    trafficImage="$TRAFFIC_IMAGE_NAME" \
    deployTrafficSimulator=true \
    principalId="$PRINCIPAL_ID" \
    sqlAdminPassword="SQL$(openssl rand -hex 12)!" \
    appUserPassword="APP$(openssl rand -hex 12)!" \
  --output none

# Get app info
RG_NAME=$(az group list --tag azd-env-name="$ENV_NAME" --query "[0].name" -o tsv)
WEB_APP_NAME=$(az webapp list --resource-group "$RG_NAME" --query "[?contains(name, 'web')].name | [0]" -o tsv)
API_APP_NAME=$(az webapp list --resource-group "$RG_NAME" --query "[?contains(name, 'api')].name | [0]" -o tsv)
WEB_URL=$(az webapp show --name "$WEB_APP_NAME" --resource-group "$RG_NAME" --query "defaultHostName" -o tsv)
API_URL=$(az webapp show --name "$API_APP_NAME" --resource-group "$RG_NAME" --query "defaultHostName" -o tsv)
CONTAINER_NAME=$(az container list --resource-group "$RG_NAME" --query "[0].name" -o tsv 2>/dev/null || echo "")

# Export for traffic simulation script
export APP_URL="https://$WEB_URL"

echo ""
echo "✅ Deployment Complete!"
echo ""
echo "Web App URL: https://$WEB_URL"
echo "Public API URL: https://$API_URL"
echo "Resource Group: $RG_NAME"
echo "Web App Service: $WEB_APP_NAME"
echo "API App Service: $API_APP_NAME"
if [ -n "$CONTAINER_NAME" ]; then
  echo "Traffic Simulator: $CONTAINER_NAME (running continuously)"
fi

# Calculate elapsed time
END_TIME=$(date +%s)
ELAPSED=$((END_TIME - START_TIME))
MINUTES=$((ELAPSED / 60))
SECONDS=$((ELAPSED % 60))

echo ""
echo "⏱️  Deployment completed in ${MINUTES}m ${SECONDS}s"
echo ""
echo "To delete all resources:"
echo "  az group delete --name $RG_NAME --yes --no-wait"

