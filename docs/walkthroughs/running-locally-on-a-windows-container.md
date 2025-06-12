---
title: Running locally on a Windows Container from Visual Studio
parent: Walkthroughs
nav_order: 7
---

To deploy the eShopOnWeb sample to a local Windows Docker container from Visual Studio, follow these step-by-step instructions:

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Ensure the computer on which you're running has Windows containers enabled. You can read how perform the one-time setup on the [Docker Blog](https://blog.docker.com/2016/09/build-your-first-docker-windows-server-container/)

1. Right click on the `Web` project in Visual Studio and select the `Add` menu then `Docker Support`.

   ![image](https://github.com/user-attachments/assets/a754d664-ce4b-40a6-80b5-7e3712c8214d)

1. Select `Windows` and click on `OK`. This will create a new project in your solution called `docker-compose`. This project contains the settings for deploying to Docker.

   ![image](https://github.com/user-attachments/assets/ecc8a070-bacd-4133-8c2d-ab85cf58eb24)

1. Open the `docker-compose.override.yml` from the `docker-compose` project and change the line that reads `80` to read `5106`. This is the port eShopOnWeb is configured to run on. (See the `Program.cs` file for details)

   ![image](https://github.com/user-attachments/assets/7fedcb4b-92b3-4998-aec2-d3e73ac065c0)

1. Press `F5` to run the project or select `Debug` > `Start Debugging` from the menu.

1. Your default browser will start on a random port of the IP address which is forwarded to the docker container.

   ![image](https://github.com/user-attachments/assets/2d75473b-ec2b-48f9-bae6-3b0aa8523946)

## Troubleshooting

You may encounter an error if your Docker for Windows is configured to run Linux containers. This setting may be changed by right clicking on the tray icon and selecting Switch to Windows containers

![image](https://github.com/user-attachments/assets/58ce6f51-5794-4603-b269-7ec99d536390)