---
title: Deploying with Azure Developer CLI (Azd)
parent: Walkthroughs
nav_order: 3
---

The Azure Developer CLI (`azd`) is a developer-centric command-line interface (CLI) tool for creating Azure applications.

## Install the Azure Developer CLI

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

## Use `azd` to Provision and Deploy the Application

After logging in with the following command, you will be able to use the azd cli to quickly provision and deploy the application.

```
azd auth login
```

Then, in an empty folder, execute the `azd init` command to initialize the environment.

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

## Additional Resources

- [What is the Azure Developer CLI?](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/overview?tabs=windows)
- [awesome-azd: An open-source template gallery to get started with Azure](https://azure.github.io/awesome-azd/)