---
title: ".NET Maintenance"
parent: Explore
---

When we need to update to a new version of .NET, the following .NET references need to be updated:

## Root

- [Directory.Packages.props](https://github.com/NimblePros/eShopOnWeb/blob/main/Directory.Packages.props)
- [global.json](https://github.com/NimblePros/eShopOnWeb/blob/main/global.json)
- [README](https://github.com/NimblePros/eShopOnWeb/blob/main/README.md) - see the VERSIONS header.

## eShopWeb.AppHost

- [eShopWeb.AppHost.csproj](https://github.com/NimblePros/eShopOnWeb/blob/main/src/eShopWeb.AppHost/eShopWeb.AppHost.csproj)

## eShopWeb.AspireServiceDefaults

- [eShopWeb.AspireServiceDefaults.csproj](https://github.com/NimblePros/eShopOnWeb/blob/main/src/eShopWeb.AspireServiceDefaults/eShopWeb.AspireServiceDefaults.csproj)

## PublicApi

- [PublicApi Dockerfile](https://github.com/NimblePros/eShopOnWeb/blob/main/src/PublicApi/Dockerfile)

## Web

- [Web Dockerfile](https://github.com/NimblePros/eShopOnWeb/blob/main/src/Web/Dockerfile)
- [dotnet tools config in Web](https://github.com/NimblePros/eShopOnWeb/blob/main/src/Web/.config/dotnet-tools.json)


- [.vscode/launch.json](https://github.com/NimblePros/eShopOnWeb/blob/main/.vscode/launch.json) - check `configurations.program` folder path

- [infra/main.bicep](https://github.com/NimblePros/eShopOnWeb/blob/main/infra/main.bicep) - `runtimeVersion`
