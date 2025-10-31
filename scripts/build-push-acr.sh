#!/bin/bash
# Build and push Docker image to Azure Container Registry
# Pre-configured for Instruqt deployment
set -e

# Start timing
START_TIME=$(date +%s)

# Change to repository root (script can be run from anywhere)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

# Instruqt configuration (hardcoded)
ACR_NAME="alesseshopacr"
WEB_IMAGE_NAME="eshop-web"
API_IMAGE_NAME="eshop-publicapi"
TRAFFIC_IMAGE_NAME="eshop-traffic-simulator"
IMAGE_TAG="${1:-latest}"
LOCATION="westus2"
RESOURCE_GROUP="rg-eshop-acr"

echo "Building Docker images for Instruqt"
echo "Repository: $REPO_ROOT"
echo "ACR: ${ACR_NAME}.azurecr.io"
echo "Web Image: ${WEB_IMAGE_NAME}:${IMAGE_TAG}"
echo "API Image: ${API_IMAGE_NAME}:${IMAGE_TAG}"
echo "Traffic Image: ${TRAFFIC_IMAGE_NAME}:${IMAGE_TAG}"
echo "⏳ This will take 5-20 minutes..."
echo ""

# Check if ACR exists, create if not
if ! az acr show --name "$ACR_NAME" &>/dev/null; then
    echo "Creating ACR..."
    
    # Create resource group if it doesn't exist
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none 2>/dev/null || true
    
    # Create ACR with anonymous pull enabled
    az acr create \
      --name "$ACR_NAME" \
      --resource-group "$RESOURCE_GROUP" \
      --location "$LOCATION" \
      --sku Standard \
      --output none
    
    az acr update --name "$ACR_NAME" --anonymous-pull-enabled true --output none
    
    echo "✅ ACR created: ${ACR_NAME}.azurecr.io"
fi

# Build and push web application image
echo "Building web application image..."
az acr build \
  --registry "$ACR_NAME" \
  --image "${WEB_IMAGE_NAME}:${IMAGE_TAG}" \
  --file src/Web/Dockerfile \
  --platform linux \
  . \
  --no-logs

echo "✅ Web image built: ${ACR_NAME}.azurecr.io/${WEB_IMAGE_NAME}:${IMAGE_TAG}"
echo ""

# Build and push public API image
echo "Building public API image..."
az acr build \
  --registry "$ACR_NAME" \
  --image "${API_IMAGE_NAME}:${IMAGE_TAG}" \
  --file src/PublicApi/Dockerfile \
  --platform linux \
  . \
  --no-logs

echo "✅ API image built: ${ACR_NAME}.azurecr.io/${API_IMAGE_NAME}:${IMAGE_TAG}"
echo ""

# Build and push traffic simulator image
echo "Building traffic simulator image..."
az acr build \
  --registry "$ACR_NAME" \
  --image "${TRAFFIC_IMAGE_NAME}:${IMAGE_TAG}" \
  --file src/traffic-simulator/Dockerfile \
  --platform linux \
  src/traffic-simulator \
  --no-logs

echo "✅ Traffic image built: ${ACR_NAME}.azurecr.io/${TRAFFIC_IMAGE_NAME}:${IMAGE_TAG}"
echo ""
echo "ACR: ${ACR_NAME}.azurecr.io"
echo "Resource Group: $RESOURCE_GROUP"

# Calculate elapsed time
END_TIME=$(date +%s)
ELAPSED=$((END_TIME - START_TIME))
MINUTES=$((ELAPSED / 60))
SECONDS=$((ELAPSED % 60))

echo ""
echo "⏱️  Build completed in ${MINUTES}m ${SECONDS}s"
echo ""
echo "Next step: Deploy to Azure"
echo "  ./scripts/deploy-container-instruqt.sh"
echo ""
echo "To delete the ACR, run:"
echo "  az group delete --name $RESOURCE_GROUP --yes --no-wait"

