---
title: Deploying as a Linux Container into Azure App Service
parent: Walkthroughs
nav_order: 5
---

To deploy the eShopOnWeb sample to a Linux Docker container on Azure App Services from Visual Studio, follow these step-by-step instructions:

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Ensure that you have installed a recent version of [Docker for Windows](https://www.docker.com/docker-windows). You do not need to run Docker containers locally but you will need Docker command line tools to build the image.

1. Right click on the `Web` project in Visual Studio and select the `Add` menu then `Docker Support`.

   ![image](https://github.com/user-attachments/assets/aaafd6c8-01ef-4637-bbdf-7918503130bb)

1. Select `Linux` and click on `OK`. This will create a new project in your solution called `docker-compose`. This project contains the settings for deploying to Docker.

    ![image](https://github.com/user-attachments/assets/5e0d2bc1-9c95-45f2-a1bf-7434ad46ccad)

1. Update the `docker-compose.yml` to listen to and forward port 80 to port 5106. Change the line

    ```docker
    ports:
    - "80"
    ```

    to

    ```docker
    ports:
    - "80:5106"
   ```

1. Right click on the `Web` project and select `Publish`
    ![image](https://github.com/user-attachments/assets/8c8f50e1-ec73-4a2a-ba8b-ff740c74bf0b)

1. In the `Publish` dialog select `Azure App Service Linux`

    ![image](https://github.com/user-attachments/assets/de774502-1aad-4062-9d0c-3c17ee0043d5)

1. Fill in the fields in the App Services Dialog.
    * `App Name` - Name off the app service - this will be used in the default URL
    * `Subscription `- Select the Azure subscription to use
    * `Resource Group` - Select a resource group to use for all newly created services. You can either use an existing one or create a new one.
    * `App Services Plan` - The name of the app services plan to use. This can be an existing App Service or a new one
    * `Container Registry` - The instance of Azure Container Registry to use to hold the images to deploy. This can be an existing registry or a new one.

1. Click `Create` to provision the resources on Azure.
    ![image](https://github.com/user-attachments/assets/b6beb4ff-cada-4086-bf21-23865f2a4723)

1. Once the deployment is complete a browser will open with the newly containerized application running on a Linux App Service.

    ![image](https://github.com/user-attachments/assets/3af18bbc-fb59-4b87-b8fe-c4f67636befc)    
