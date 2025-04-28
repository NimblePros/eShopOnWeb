---
title: Running Locally on a Linux Container from Visual Studio for Mac
parent: Walkthroughs
nav_order: 6
---

To deploy the eShopOnWeb sample to a local Linux Docker container, from Visual Studio for Mac, follow these step-by-step instructions:

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Ensure that you have installed a recent version of [Docker for Mac](https://docs.docker.com/docker-for-mac/install/)

   > **Note** : (steps 3 and 4 have been done already for eShopOnWeb, but are included so you see how you would add support to your own projects)

1. Right click on the `Web` project in VS for Mac and select the `Add` menu then `Docker Support`.

1. Select `Linux` and click on `OK`. This will create a new project in your solution called `docker-compose`. This project contains the settings for deploying to Docker.

1. Open the `docker-compose.override.yml` from the `docker-compose` project and change the line that reads `80` to read `5106`. This is the port eShopOnWeb is configured to run on. (See the `Program.cs` file for details)

1. Run the project with `Run` > `Start Debugging` from the menu.

1. Your default browser will start on a random port on `localhost` which is forwarded to the docker container.

## Troubleshooting

The first time you run the application you may need to add a folder sharing path to Docker. Make sure you add the `/usr/local/share/dotnet/sdk/NuGetFallbackFolder` folder to Docker's file sharing options [as detailed here](https://docs.microsoft.com/en-us/visualstudio/mac/docker-quickstart?view=vsmac-2019).