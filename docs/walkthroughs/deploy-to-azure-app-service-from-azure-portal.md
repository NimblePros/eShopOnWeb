---
title: Deploying to Azure App Service from Azure Portal
parent: Walkthroughs
nav_order: 2
---

## Deploying to Azure App Service

To deploy the eShopOnWeb sample to an Azure App Service, starting in the Azure Portal, follow these step-by-step instructions (or, [deploy to Azure directly from from Visual Studio]({{ site.baseurl }}/walkthroughs/deploy-to-azure-app-service-from-visual-studio)):

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Log in to the [Azure Portal](https://portal.azure.com/) with your Microsoft Account.

1. Click the + icon, then select `Web App`. Provide an `App name`.

   ![image](https://github.com/user-attachments/assets/ca60a9e0-feab-4809-adb0-5512968cd20b)

1. Choose a new resource group name or select an existing one, then click `Create`. In a few moments you should see that the deployment was successful.

    ![image](https://github.com/user-attachments/assets/85c2ec03-608e-416a-aa38-6aa018f9b1c4)

1. Select `Get publish profile`.

   ![image](https://github.com/user-attachments/assets/8f276945-c6d7-4801-8d1e-c2c4326d4cf9)

1. Copy the `.PublishSettings` file to the `eShopOnWeb/src/Web` folder.

   ![image](https://github.com/user-attachments/assets/7e34deb8-e127-4876-94ab-0db54154daca)

1. Open the `eShopOnWeb.sln` solution file.

1. Right-click on the `Web` project and select `Publish`. Select `Import profile`. Click `OK`.

    ![image](https://github.com/user-attachments/assets/657c54ae-8e08-44c1-bae2-bf32c2d06211)

1. Select the `.PublishSettings` file you saved in the `eShopOnWeb/src/Web` folder. You should see something like this:
 
    ![image](https://github.com/user-attachments/assets/856c96a1-0e9e-413a-b085-ccad2f4f652c)


1. Click `Publish`. You should see files being copied. When the publish process completes, the site should open in your browser.

## Optional

If you are not seeing data initially in the store, the most likely reason is that the Azure App Service is configured for `Production`, not `Development` (Note: This is not the same as `Debug` vs. `Release` configuration). The sample data is only seeded in the `Development` environment. You can configure the App Service to run in `Development` as follows:

1. In the Azure Portal, navigate to your Web App.

1. Click `Application settings`.

1. Scroll down to `App settings`.

1. Add a new key `ASPNETCORE_ENVIRONMENT` with value `Development`.

1. Click `Save`.

    ![image](https://github.com/user-attachments/assets/3104a9a4-c0d3-4152-b879-ad75ad7e9e2c)

At this point, you should be able to refresh the site and see it loaded with data (if not, publish once more from Visual Studio).

![image](https://github.com/user-attachments/assets/e0d7f51e-32d4-497a-ba70-b506ca1ef6fc)
