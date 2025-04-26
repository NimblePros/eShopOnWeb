---
title: Running Locally on a Linux Container from Visual Studio
parent: Walkthroughs
nav_order: 4
---

To deploy the eShopOnWeb sample to a local Linux Docker container, from Visual Studio, follow these step-by-step instructions:

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Ensure that you have installed a recent version of [Docker for Windows](https://www.docker.com/docker-windows)

1. Right click on the `Web` project in Visual Studio and select the `Add menu` then `Docker Support`

    ![image](https://github.com/user-attachments/assets/6715bf06-320b-4adf-adb1-d0999810401e)

1. Select `Linux` and click on `OK`. This will create a new project in your solution called `docker-compose`. This project contains the settings for deploying to Docker.

    ![image](https://github.com/user-attachments/assets/4394da7c-ecc0-42d3-bfb0-b0badfaa8a04)

1. Open the `docker-compose.override.yml` from the `docker-compose` project and change the line that reads `80` to read `5106`. This is the port eShopOnWeb is configured to run on. (See the `Program.cs` file for details)

   ![image](https://github.com/user-attachments/assets/47445c1e-de6f-43f6-9a11-ae1afb585cac)

1. Press `F5` to run the project or select `Debug` > `Start Debugging` from the menu.

1. Your default browser will start on a random port on localhost which is forwarded to the docker container.

   ![image](https://github.com/user-attachments/assets/59647686-a01b-4591-a10c-6af8f36c7f7c)

## Troubleshooting

You may encounter an error if your Docker for Windows is configured to run Windows containers. This setting change be changed by right clicking on the tray icon and selecting `Switch to Linux containers`.

![image](https://github.com/user-attachments/assets/b41f36f9-a08d-4653-a0bc-ad89525d9287)