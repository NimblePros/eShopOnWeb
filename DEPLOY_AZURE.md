# Deploying to Azure App Service

This repository includes a GitHub Actions workflow at `/.github/workflows/deploy-azure-appservice.yml` that publishes `src/Web/Web.csproj` and deploys to the Azure Web App `WbappEShop`.

To allow the workflow to deploy, add the App Service publish profile as a repository secret:

1. Download the publish profile:
   - Azure Portal: Navigate to your Web App (WbappEShop) → Overview → Get publish profile → Download.
   - or Azure CLI:
     ```bash
     az webapp deployment list-publishing-profiles --resource-group <RESOURCE_GROUP> --name WbappEShop --output xml > publishProfile.xml
     ```

2. Copy the full XML contents of the downloaded file.

3. In GitHub: Repository → Settings → Secrets and variables → Actions → New repository secret.
   - **Name:** `AZURE_WEBAPP_PUBLISH_PROFILE`
   - **Value:** paste the publish profile XML

Optional: if you prefer not to embed the app name in the workflow, create a secret `AZURE_WEBAPP_NAME` with value `WbappEShop` and update the workflow to use it.

Trigger: the workflow runs on pushes to `main`/`master` or via `workflow_dispatch` (manual run).
