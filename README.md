# eShopOnWeb - ASP.NET Core Reference Application

**Purpose:** Reference application demonstrating monolithic application architecture with ASP.NET Core. Created to support the free eBook [Architecting Modern Web Applications with ASP.NET Core and Azure](https://aka.ms/webappebook).

**What it demonstrates:**
- Clean Architecture principles
- Domain-Driven Design (DDD) patterns
- Repository and Specification patterns
- Dependency Injection
- Entity Framework Core with SQL Server
- ASP.NET Core Identity
- Blazor WebAssembly for admin UI

**What it is NOT:** This is a teaching/reference app, not a production-ready eCommerce platform. Many features essential to real eCommerce are intentionally omitted.

**Tech Stack:**
- ASP.NET Core 9.0 MVC
- ASP.NET Core 9.0 Public API (FastEndpoints)
- Entity Framework Core 9.0
- Blazor WebAssembly
- SQL Server
- Serilog (JSON structured logging)
- Azure deployment (App Service, Container Registry, Key Vault)

**Origin:** Microsoft reference application, maintained by [NimblePros](https://nimblepros.com/)

![eShopOnWeb screenshot](https://user-images.githubusercontent.com/782127/88414268-92d83a00-cdaa-11ea-9b4c-db67d95be039.png)

---

## Quick Start

### Deploy to Azure (Container)

**Prerequisites:**
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) installed
- Azure subscription with contributor access

```bash
# 1. Login to Azure
az login

# 2. Build and push Docker image to ACR (instructor - one-time setup)
./scripts/build-push-acr.sh

# 3. Deploy infrastructure and application (students)
./scripts/deploy-container-instruqt.sh
```

**Using defaults** (pre-configured for Instruqt):
```bash
./scripts/deploy-container-instruqt.sh
# Uses: alesseshopacr, eshop-web-instruqt:latest, eshop-api-instruqt:latest, westus2
```

**Custom configuration:**
```bash
export ACR_NAME=myacr
export IMAGE_NAME=my-web-image
export API_IMAGE_NAME=my-api-image
export IMAGE_TAG=v1.0
export AZURE_LOCATION=eastus
export AZURE_ENV_NAME=my-env
./scripts/deploy-container-instruqt.sh
```

**Time:**
- First time (with build): 6-8 minutes
- Using pre-built image: 4-5 minutes

**What gets deployed:**
- Azure App Service (Linux container) - eShopOnWeb frontend application
- Azure App Service (Linux container) - PublicApi (for admin panel)
- Azure Container Registry - Docker images
- Azure SQL Database - 2 databases (catalog + identity)
- Azure Key Vault - Connection strings
- **Azure Container Instance** - Traffic simulator (runs automatically!)

**Traffic Simulation** (automatic):
The deployment includes an automated traffic simulator that runs continuously, generating realistic browser traffic for Datadog monitoring.

**Behavior:**
- 5 simulated user sessions per cycle
- 60 second delay between cycles
- Runs forever automatically
- Catalog browsing, product filtering, add to cart, basket views, admin visits
- **Datadog observability**: APM traces, RUM events, logs, metrics

**View traffic simulator logs:**
```bash
az container logs --name <container-name> --resource-group <resource-group> --follow
```

**Cleanup:**
```bash
# Delete the deployed application (keeps ACR for reuse)
az group delete --name rg-eshop-1761853864 --yes --no-wait

# Find your resource group name
az group list --query "[?tags.\"azd-env-name\"].{Name:name, EnvName:tags.\"azd-env-name\"}" -o table

# Delete everything including ACR
az group delete --name rg-eshop-<env-name> --yes --no-wait
az group delete --name rg-eshop-acr --yes --no-wait
```

---

### Run Locally with Docker

**Prerequisites:**
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Datadog API Key](https://app.datadoghq.com/organization-settings/api-keys) (for traces)

**Setup environment variables:**
```bash
# Required for Datadog traces
export DD_API_KEY="your_datadog_api_key"
export DD_SITE="us3.datadoghq.com"  # or your Datadog site
```

**Build and run:**
```bash
docker-compose build
docker-compose up
```

**Access:**
- Web: http://localhost:5106
- PublicApi: http://localhost:5200
- Admin: http://localhost:5106/admin

**Login:** `demouser@microsoft.com` / `Pass@word1`

**Datadog Instrumentation:**
- Uses [serverless-init](https://docs.datadoghq.com/serverless/guide/azure_app_service_linux_containers_serverless_init/?tab=dotnet) for agentless monitoring
- Automatically sends traces, logs, and metrics to Datadog
- No local Datadog Agent required

**Troubleshooting:**
```bash
# If you see "DD_API_KEY is required for Datadog" error:
export DD_API_KEY="your_api_key_here"

# If you hit GitHub rate limits during build (rare):
export GITHUB_TOKEN="your_github_token"
docker-compose build

# To disable Datadog (for testing without traces):
export DD_TRACE_ENABLED=false
docker-compose up
```

**Includes:** Traffic simulator runs automatically, generating continuous traffic for observability testing

---

### Run Locally with .NET CLI

**Requirements:** .NET 9.0 SDK, SQL Server (or use in-memory database)

```bash
# Use in-memory database (optional)
# Add to src/Web/appsettings.json: { "UseOnlyInMemoryDatabase": true }

# Setup database (if using SQL Server)
cd src/Web
    dotnet tool restore
    dotnet ef database update -c catalogcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj
    dotnet ef database update -c appidentitydbcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj

# Run web application
dotnet run --launch-profile https

# Run PublicApi (in separate terminal, for Blazor admin)
cd src/PublicApi
dotnet run
```

**Access:** https://localhost:5001  
**Admin:** https://localhost:5001/admin

---

## Project Structure

```
src/
├── Web/                    # Main MVC web application + Dockerfile
├── PublicApi/              # REST API for Blazor admin
├── BlazorAdmin/            # Blazor WebAssembly admin UI
├── ApplicationCore/        # Domain entities, interfaces, services
├── Infrastructure/         # EF Core, data access, identity
└── traffic-simulator/      # Automated traffic generation (Playwright)

infra/                      # Bicep templates for Azure deployment
scripts/                    # Build and deployment automation
```

---

## Key Features

- **Catalog:** Browse products by brand and type
- **Shopping Cart:** Add/remove items, update quantities
- **Checkout:** Order processing
- **Admin Panel:** Manage products, brands, types (Blazor)
- **Authentication:** ASP.NET Core Identity
- **External Login:** GitHub OAuth

### GitHub OAuth Configuration (Optional)

To enable GitHub authentication:

1. [Register a GitHub OAuth app](https://github.com/settings/applications/new)
2. Set redirect URI: `https://localhost:5001/signin-github`
3. Store credentials in user secrets:
   ```bash
   cd src/Web
   dotnet user-secrets set "Authentication:GitHub:ClientId" "<your-client-id>"
   dotnet user-secrets set "Authentication:GitHub:ClientSecret" "<your-client-secret>"
   ```

---

## Related Resources

- **eBook:** [Architecting Modern Web Applications with ASP.NET Core and Azure](https://aka.ms/webappebook)
- **Microservices Version:** [eShopOnContainers](https://github.com/dotnet/eShopOnContainers)
- **.NET Aspire Version:** [eShop](https://github.com/dotnet/eShop)
- **Wiki:** [Getting Started Guide](https://github.com/nimblepros/eShopOnWeb/wiki)

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Instrumentation
1. Datadog Azure ISV for Azure integration 
2. WebApplication is https://docs.datadoghq.com/serverless/azure_app_service/linux_container?tab=net
3. https://docs.datadoghq.com/logs/log_collection/csharp/?tab=serilog
4. Make sure WEBSITES_ENABLE_APP_SERVICE_STORAGE is set to default (true)
5. Inferred services: https://docs.datadoghq.com/tracing/services/inferred_services/?tab=agentv7600#set-up-inferred-services 