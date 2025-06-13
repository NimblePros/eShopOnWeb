---
title: Deploying to Azure App Service from Visual Studio
parent: Walkthroughs
nav_order: 1
---

_**Last updated**: June 13, 2025_

To deploy the eShopOnWeb sample application to an Azure App Service (assuming you've already cloned or downloaded it locally, and you [have an Azure account](https://azure.microsoft.com/en-us/free/)), follow these steps from Visual Studio ([see here to start from the Azure Portal]({{ site.baseurl }}/walkthroughs/deploy-to-azure-app-service-from-azure-portal)):

## 1. Open the eShopOnWeb solution in Visual Studio.

## 2. Right-click on the Web project in Solution Explorer and choose Publish.

![Solution Explorer is open. The context menu is shown for the Web project. Publish is highlighted.](../assets/images/walkthroughs/app-service-from-visual-studio/publish-from-web-project.jpg)

## 3. From the Publish screen, select `+ New profile`

![The Publish tab is open. The "+ New profile" button is highlighted.](../assets/images/walkthroughs/app-service-from-visual-studio/publish-select-new-profile.jpg)

## 4. In the Publish dialog, select `Azure` as the target.

![Publish dialog is displayed. This is the Target step. Azure is the selected target. The Next button is highlighted.](../assets/images/walkthroughs/app-service-from-visual-studio/select-publish-target.jpg)

## 5. Select `Azure App Service (Linux)` for the Specific target.

![Publish dialog is displayed. This is the specific target step. Azure App Service (Linux) is the selected target. The Next button is highlighted.](../assets/images/walkthroughs/app-service-from-visual-studio/specific-target-azure-app-service-linux.jpg)

## 6. Select the subscription and app service.

In the upper-right corner of the dialog, select your Microsoft account. Then, select your **Subscription name**. For this walkthrough, we'll create a new App Service, so select **+ Create new**.

![On the Publish dialog, confirm the Microsoft account and subscription. "+ Create new" is highlighted. ](../assets/images/walkthroughs/app-service-from-visual-studio/select-subscription.jpg)

## 7. Add the app services details.

On the **App Service (Linux)** dialog, make sure that the following details are correct:

- Microsoft account
- **Name** - This is for the App Service's name. This is unique for app services.
- **Subscription name**
- **Resource group** - If needed, create a new resource group.
- **Hosting Plan** - You can create a Linux hosting plan here.

If all looks well, then select **Create**. This will create the Azure resources.

![The App Service (Linux) dialog is displayed. The settings are for the subscription, app service name, resource group, and hosting plan.](../assets/images/walkthroughs/app-service-from-visual-studio/app-service-details.jpg)

## 8. Update Azure App Settings' Environment Variables

In order to populate the seed data, we need to set the `ASPNETCORE_ENVIRONMENT` variable to `Development`. (Note: This is not the same as `Debug` vs. `Release` configuration). The sample data is only seeded in the `Development` environment. You can configure the App Service to run in `Development` as follows:

1. In the Azure Portal, navigate to your Web App.

1. Select **Settings** > **Environment Variables**.

1. In the **App settings** section, add a new key `ASPNETCORE_ENVIRONMENT` with value `Development`.

1. Select **Apply**.

1. Add another key for `UseOnlyInMemoryDatabase` with the value of `true`.

1. Select **Apply**.

    ![Add the ASPNETCORE_ENVIRONMENT variable in the Azure Portal.](../assets/images/walkthroughs/app-service-from-visual-studio/azure-app-service-environment-variables.jpg)

1. Finally, select **Apply** to apply all of these changes to the App Service.

## 9. Publish the app.

Once the Azure resources are created, the Publish dialog will appear for you to select an existing or new Azure resource. Select the new Azure App Service (Linux) that you created above. Then, select **Finish**.

![In the Publish dialog, on the App Service step. The new app service is highlighted, then the Finish button is highlighted.](../assets/images/walkthroughs/app-service-from-visual-studio/finalize-publish.jpg)

The Publish profile will be saved.

You will then need to select the **Publish** button to publish the app to the App Service.

![image](https://github.com/user-attachments/assets/68f4b285-98b4-4ce0-b51c-dd0d5fd78cd7)

At this point, you should be able to refresh the site and see it loaded with data (if not, publish once more from Visual Studio).

![eShopOnWeb portal logged in as the demouser and shown running on the resource we created in this demo.](../assets/images/walkthroughs/app-service-from-azure-portal/eShopOnWeb-running.jpg)

[Learn more about Azure deployment options in the official documentation](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy).


## Notes

- You may need to ensure you have the [Bundler and Minifier extension](https://docs.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification) installed in Visual Studio, otherwise your CSS may not be minified and referenced correctly in the deployed version of the application. Alternately, you can modify `_Layout.cshtml` to use app.css instead of `app.css.min` in Production.