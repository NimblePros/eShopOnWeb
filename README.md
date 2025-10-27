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
- Entity Framework Core 9.0
- Blazor WebAssembly
- SQL Server
- Azure deployment (App Service, Container Registry, Key Vault)

**Origin:** Microsoft reference application, maintained by [NimblePros](https://nimblepros.com/)

![eShopOnWeb screenshot](https://user-images.githubusercontent.com/782127/88414268-92d83a00-cdaa-11ea-9b4c-db67d95be039.png)

---

## Quick Start

### Deploy to Azure (Container)

**Prerequisites:** Azure CLI, Azure subscription

```bash
# 1. Build and push Docker image to ACR
./scripts/build-push-acr.sh <your-acr-name>

# 2. Deploy infrastructure and application
export ACR_NAME=<your-acr-name>
./scripts/deploy-container-instruqt.sh
```

**Time:** 3-4 minutes (using pre-built image)

**What gets deployed:**
- Azure App Service (Linux container)
- Azure Container Registry
- Azure SQL Database (2 databases: catalog + identity)
- Azure Key Vault (connection strings)

---

### Run Locally with Docker

```bash
docker-compose build
docker-compose up
```

**Access:**
- Web: http://localhost:5106
- PublicApi: http://localhost:5200
- Admin: http://localhost:5106/admin

**Login:** `demouser@microsoft.com` / `Pass@word1`

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
└── Infrastructure/         # EF Core, data access, identity

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
