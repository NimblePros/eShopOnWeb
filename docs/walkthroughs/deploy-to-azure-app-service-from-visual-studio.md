---
title: Deploying to Azure App Service from Visual Studio
parent: Walkthroughs
nav_order: 1
---

To deploy the eShopOnWeb sample application to an Azure App Service (assuming you've already cloned or downloaded it locally, and you [have an Azure account](https://azure.microsoft.com/en-us/free/)), follow these steps from Visual Studio ([see here to start from the Azure Portal]({{ site.baseurl }}/walkthroughs/deploy-to-azure-app-service-from-azure-portal)):

## 1. Open the eShopOnWeb solution in Visual Studio.

## 2. Right-click on the Web project in Solution Explorer and choose Publish.

![image](https://github.com/user-attachments/assets/0d489dfb-9e3a-4bb0-b779-7e3fe0b0d96e)

## 3. Choose Microsoft Azure App Service.

Leave the option to `Create New` selected. Click `Publish`.

![image](https://github.com/user-attachments/assets/1b377746-4e21-4a4a-86db-9cfdddf27cd3)

## 4. Choose an App Name, Subscription, Resource Group, and App Service Plan.

Create a new Resource Group, if desired. You can specify a Free App Service Plan, if desired, as shown.

![image](https://github.com/user-attachments/assets/44700de9-bbac-44c5-9656-0fef86026d56)

## 5. Click Create.

The Publish profile will be saved and the eShopOnWeb sample will be published to the Azure App Service.

![image](https://github.com/user-attachments/assets/68f4b285-98b4-4ce0-b51c-dd0d5fd78cd7)

Once the publish process has completed, your deployed app will launch in your browser.

![image](https://github.com/user-attachments/assets/12c3bbdb-0e5e-4314-b6ea-86b063eb5628)

[Learn more about Azure deployment options in the official documentation](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy).

## Optional

If you are not seeing data initially in the store, the most likely reason is that the Azure App Service is configured for `Production`, not `Development` (Note: This is not the same as `Debug` vs. `Release` configuration). The sample data is only seeded in the `Development` environment. You can configure the App Service to run in `Development` as follows:

1. In the Azure Portal, navigate to your Web App.

1. Click Application settings.

1. Scroll down to App settings.

1. Add a new key `ASPNETCORE_ENVIRONMENT` with value `Development`.

1. Click `Save`.

![image](https://github.com/user-attachments/assets/89c7f1e2-6ae9-4f9c-83d9-d6a286240ef6)

At this point, you should be able to refresh the site and see it loaded with data (if not, publish once more from Visual Studio).

![image](https://github.com/user-attachments/assets/df803021-8c30-4038-a963-b6e2ce1a16ab)

## Notes

- You may need to ensure you have the [Bundler and Minifier extension](https://docs.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification) installed in Visual Studio, otherwise your CSS may not be minified and referenced correctly in the deployed version of the application. Alternately, you can modify `_Layout.cshtml` to use app.css instead of `app.css.min` in Production.