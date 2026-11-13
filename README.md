# Microsoft eShopOnWeb ASP.NET Core Reference Application

This Microsoft reference application is now being maintained by [NimblePros](https://nimblepros.com/).

If you're looking for the .NET Aspire eShop sample it's [here](https://github.com/dotnet/eShop). Microsoft also recommends the [Reliable Web App](https://learn.microsoft.com/azure/architecture/web-apps/guides/reliable-web-app/overview) patterns guidance for building web apps with enterprise app patterns.

Sample ASP.NET Core reference application, powered by Microsoft, demonstrating a single-process (monolithic) application architecture and deployment model. If you're new to .NET development, read the [Getting Started for Beginners](https://nimblepros.github.io/eShopOnWeb/getting-started-for-beginners.html) guide.

A list of Frequently Asked Questions about this repository can be found [here](https://github.com/nimblepros/eShopOnWeb/wiki/Frequently-Asked-Questions).

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Overview Video](#overview-video)
- [eBook](#ebook)
- [Topics (eBook TOC)](#topics-ebook-toc)
- [Running the sample using Azd template](#running-the-sample-using-azd-template)
  - [Windows](#windows)
  - [Linux/MacOS](#linuxmacos)
- [Running the sample locally](#running-the-sample-locally)
  - [Running without Aspire](#running-without-aspire)
  - [Working with EF Core migrations](#working-with-ef-core-migrations)
- [Dev Containers for the eShopOnWeb repo](#dev-containers-for-the-eshoponweb-repo)
  - [eShopOnWeb App Dev Container](#eshoponweb-app-dev-container)
  - [eShopOnWeb Docs Dev Container](#eshoponweb-docs-dev-container)
  - [Learn More about Dev Containers](#learn-more-about-dev-containers)
- [Running the sample using Docker](#running-the-sample-using-docker)
- [Getting the GitHub Single Sign-On Working](#getting-the-github-single-sign-on-working)
- [Community Extensions](#community-extensions)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Overview Video

[Steve "ardalis" Smith](https://twitter.com/ardalis) recorded [a live stream providing an overview of the eShopOnWeb reference app](https://www.youtube.com/watch?v=vRZ8ucGac8M&ab_channel=Ardalis) in October 2020.

## eBook

This reference application is meant to support the free .PDF download ebook: [Architecting Modern Web Applications with ASP.NET Core and Azure](https://aka.ms/webappebook), updated to **ASP.NET Core 8.0**. [Also available in ePub/mobi formats](https://dotnet.microsoft.com/learn/web/aspnet-architecture).

You can also read the book in online pages at the .NET docs here:
https://docs.microsoft.com/dotnet/architecture/modern-web-apps-azure/

[<img src="https://dotnet.microsoft.com/blob-assets/images/e-books/aspnet.png" height="300" />](https://dotnet.microsoft.com/learn/web/aspnet-architecture)

The **eShopOnWeb** sample is related to the [eShopOnContainers](https://github.com/dotnet/eShopOnContainers) sample application which, in that case, focuses on a microservices/containers-based application architecture. However, **eShopOnWeb** is much simpler in regards to its current functionality and focuses on traditional Web Application Development with a single deployment.

The goal for this sample is to demonstrate some of the principles and patterns described in the [eBook](https://aka.ms/webappebook). It is not meant to be an eCommerce reference application, and as such it does not implement many features that would be obvious and/or essential to a real eCommerce application.

> ### VERSIONS
> #### The `main` branch is currently running ASP.NET Core 11.0.
> #### Older versions are tagged.

## Topics (eBook TOC)

- Introduction
- Characteristics of Modern Web Applications
- Choosing Between Traditional Web Apps and SPAs
- Architectural Principles
- Common Web Application Architectures
- Common Client Side Technologies
- Developing ASP.NET Core MVC Apps
- Working with Data in ASP.NET Core Apps
- Testing ASP.NET Core MVC Apps
- Development Process for Azure-Hosted ASP.NET Core Apps
- Azure Hosting Recommendations for ASP.NET Core Web Apps

## Running the sample using Azd template

The store's home page should look like this:

![eShopOnWeb home page screenshot](https://user-images.githubusercontent.com/782127/88414268-92d83a00-cdaa-11ea-9b4c-db67d95be039.png)

The Azure Developer CLI (`azd`) is a developer-centric command-line interface (CLI) tool for creating Azure applications.

You need to install it before running and deploying with Azure Developer CLI.

### Windows

```powershell
powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"
```

### Linux/MacOS

```
curl -fsSL https://aka.ms/install-azd.sh | bash
```

And you can also install with package managers, like winget, choco, and brew. For more details, you can follow the documentation: https://aka.ms/azure-dev/install.

After logging in with the following command, you will be able to use the azd cli to quickly provision and deploy the application.

```
azd auth login
```

Then, execute the `azd init` command to initialize the environment.
```
azd init -t NimblePros/eShopOnWeb
```

Run `azd up` to provision all the resources to Azure and deploy the code to those resources.
```
azd up
```

According to the prompt, enter an `env name`, and select `subscription` and `location`, these are the necessary parameters when you create resources. Wait a moment for the resource deployment to complete, click the web endpoint and you will see the home page.

**Notes:**
1. Considering security, we store its related data (id, password) in the **Azure Key Vault** when we create the database, and obtain it from the Key Vault when we use it. This is different from directly deploying applications locally.
2. The resource group name created in azure portal will be **rg-{env name}**.

You can also run the sample locally (see below).

## Running the sample locally

The easiest way to run the sample on Windows, macOS, or Linux is with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/). The `eShopWeb.AppHost` project starts everything the site needs:

- a SQL Server container holding the `Microsoft.eShopOnWeb.CatalogDb` (catalog and shopping cart data) and `Microsoft.eShopOnWeb.Identity` (user credentials) databases
- a [Seq](https://datalust.co/seq) container for structured logs
- the `Web` and `PublicApi` projects, with their database connection strings filled in

You need the [.NET SDK](https://dotnet.microsoft.com/download) listed in `global.json` and a container runtime such as [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/). Then, from the root folder:

```
dotnet run --project src/eShopWeb.AppHost
```

Or use the [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/cli/overview) with `aspire run`, or set `eShopWeb.AppHost` as the startup project in Visual Studio or Rider.

Open the Aspire dashboard link shown in the console to find the Web and PublicApi endpoints. The Web site is at `https://localhost:5001/`, and the admin area (Blazor WebAssembly, which calls PublicApi) is at `https://localhost:5001/admin`.

The first run downloads the SQL Server image, which can take a few minutes. When the apps start, they apply the Entity Framework Core migrations and seed both databases, so you should see products in the store and be able to log in with the `demouser@microsoft.com` account. The SQL Server container and its data volume are kept between runs, so your data survives restarts. To start over with empty databases, remove the `sqlserver` container and its volume using your container tool. The SQL Server `sa` password is generated on the first run and stored in the AppHost's [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).

> **Apple silicon:** The SQL Server image is built for x64 only. On an Apple silicon Mac, enable Rosetta emulation for x86/amd64 images in your container runtime (for example, Docker Desktop's "Use Rosetta for x86_64/amd64 emulation on Apple Silicon" setting).

### Running without Aspire

You can also run the `Web` and `PublicApi` projects directly with `dotnet run`, without containers, by using the EF Core in-memory database. Add the following to the `appsettings.json` file of each project you run, or pass `--UseOnlyInMemoryDatabase=true` on the command line:

```json
{
    "UseOnlyInMemoryDatabase": true
}
```

The in-memory data is reseeded every time the app starts. To use your own SQL Server instead, set the `ConnectionStrings:CatalogConnection` and `ConnectionStrings:IdentityConnection` settings, for example with user secrets.

Note that if you run the apps this way, you'll need to stop them manually in order to build the solution (otherwise you'll get file locking errors).

### Working with EF Core migrations

The apps apply migrations on startup, so you don't need to run `dotnet ef database update`. If you change the model and need to create migrations, install the [EF Core tools](https://learn.microsoft.com/ef/core/cli/dotnet) (`dotnet tool update --global dotnet-ef`) and run these commands from the Web folder:

```
dotnet ef migrations add InitialModel --context catalogcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj -o Data/Migrations

dotnet ef migrations add InitialIdentityModel --context appidentitydbcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj -o Identity/Migrations
```

## Dev Containers for the eShopOnWeb repo

We use dev containers to make it easier for you to run the eShopOnWeb application locally as well as our documentation.

### eShopOnWeb App Dev Container

This project includes a `.devcontainer` folder with a [dev container configuration](https://containers.dev/), which lets you use a container as a full-featured dev environment.

You can use the dev container to build and run the app without needing to install any of its tools locally! You can work in GitHub Codespaces or the VS Code Dev Containers extension.

Learn more about using the dev container in [eShopOnWeb's dev container readme](/.devcontainer/devcontainerreadme.md).

### eShopOnWeb Docs Dev Container

If you want to help maintain [the documentation](https://nimblepros.github.io/eShopOnWeb/), we have a [.devcontainer folder within the docs folder](/docs/.devcontainer). This allows us to see our documentation changes in a container running Ruby and the GitHub Pages environment.

### Learn More about Dev Containers

- [NimblePros YouTube: Run GitHub Pages Locally in a Dev Container](https://www.youtube.com/watch?v=JpLJi5JBmYM&t=5s)
- [NimblePros Blog: Run GitHub Pages Locally in a Dev Container](https://blog.nimblepros.com/blogs/github-pages-with-dev-containers/)
- [NimblePros Blog: Introduction to Dev Containers](https://blog.nimblepros.com/blogs/introduction-to-dev-containers/)
- [NimblePros Webinar: Dev Containers Unwrapped!](https://www.youtube.com/watch?v=Wvetp2YkwPY)

## Running the sample using Docker

You can run the Web sample by running these commands from the root folder (where the .sln file is located):

```
docker-compose build
docker-compose up
```

You should be able to make requests to localhost:5106 for the Web project, and localhost:5200 for the Public API project once these commands complete. If you have any problems, especially with login, try from a new guest or incognito browser instance.

You can also run the applications by using the instructions located in their `Dockerfile` file in the root of each project. Again, run these commands from the root of the solution (where the .sln file is located).

## Getting the GitHub Single Sign-On Working

We include GitHub as our external provider for single sign-on.

To get it running locally, you'll want to register an application in GitHub and store values in user secrets for the client ID and client secret.

We explain the code in detail in our course on [ASP.NET Identity in Action: Implementing Individual Accounts](https://academy.nimblepros.com/p/applying-identity-to-asp-net).

## Community Extensions

We have some great contributions from the community, and while these aren't maintained by Microsoft we still want to highlight them.

[eShopOnWeb VB.NET](https://github.com/VBAndCs/eShopOnWeb_VB.NET) by Mohammad Hamdy Ghanem

[FShopOnWeb](https://github.com/NitroDevs/FShopOnWeb) An F# take on eShopOnWeb by Sean G. Wright and Kyle McMaster
