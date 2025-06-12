---
title: Deploying to Azure App Service from Visual Studio for Mac
parent: Walkthroughs
nav_order: 3
---

To deploy the eShopOnWeb sample application to an Azure App Service (assuming you've already cloned or downloaded it locally, and you [have an Azure account](https://azure.microsoft.com/en-us/free/)), follow these steps from Visual Studio for Mac:

## 1. Open the eShopOnWeb solution in Visual Studio for Mac.

![image](https://github.com/user-attachments/assets/a77111c5-4ad5-4b56-ada0-85042d2bf5de)

## 2. Right-click on the Web project in Solution Explorer and choose Publish - Publish to Azure...

![image](https://github.com/user-attachments/assets/cc0d964f-dee0-43b5-975e-fead90cbc034)

## 3. Sign in if necessary, then choose an existing App Service or create a new one.

![image](https://github.com/user-attachments/assets/bf8fbd3a-7ff3-4efc-bf70-8a0bc81e1055)

Click `New` in this case.

## 4. Choose an App Name, Subscription, Resource Group, and App Service Plan, including region and pricing.

![image](https://github.com/user-attachments/assets/f9cbd89d-8e68-4a46-8176-efcd27820541)

You can choose a free tier just to test things out. Click `Next`.

## 5. Configure Docker Container.

If you want, you can deploy the app into a container. To do so, the container will be published to Azure's container registry. Specify the registry details and then click Create. If you don't want to deploy as a container, uncheck the box and then click Create.

![image](https://github.com/user-attachments/assets/b0914cd4-6a81-4e36-9f82-8d9a991acde5)

## 6. Continue Working.

Creating the app service may take a few minutes. You're free to continue working in the meantime.

![image](https://github.com/user-attachments/assets/6a2ca011-8fb4-4f6d-a2e2-86ae20b016d0)

Once the publish process has completed, your deployed app will launch in your browser.

### Troubleshooting

You may see an error page like this one:

![image](https://github.com/user-attachments/assets/33118b1f-1de4-4544-837b-51f9007a81e0)

If so, you need to either use the `Development` environment in your Azure App Service (which should only be done when testing), or configure an actual SQL Server database for the application to use.

To make this change, log in to the Azure Portal, navigate to your App Service, and go to its Configuration blade. Add a new configuration setting called `ASPNETCORE_ENVIRONMENT` and give it a value of `Development`.

![image](https://github.com/user-attachments/assets/c8f5e00d-6071-466c-baf6-533ce271505d)

Don't forget to hit `Save`, and then refresh your application. It should now load successfully:

![image](https://github.com/user-attachments/assets/876f93d4-c156-4919-8050-c8ffa21c54b8)

[Learn more about Azure deployment options in the official documentation](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy).