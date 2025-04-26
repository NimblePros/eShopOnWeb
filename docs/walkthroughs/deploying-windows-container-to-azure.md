---
title: Deploying as a Windows Container into a Windows Container host in Azure
parent: Walkthroughs
nav_order: 8
---

To deploy the eShopOnWeb sample to a Windows 2016 virtual machine running Docker on Azure, from Visual Studio, follow these step-by-step instructions:

## Provision a Virtual Machine to Host Docker

1. Log into Azure and click on the plus symbol(+) to create a new resource. Then select compute and see all
   ![image](https://github.com/user-attachments/assets/ff387ebf-4b89-403d-bf22-8b82cdc06577)

1. In the search box enter `containers` and then select `Windows Server 2016 Datacenter - with Containers`.
   ![image](https://github.com/user-attachments/assets/d51186ed-4cab-464a-8b25-28455f26388f)

1. Click `Create`.
   ![image](https://github.com/user-attachments/assets/e0e986e3-eadb-4097-a7e0-123ee349e105)

1. Enter a name for the virtual machine. Also provide a user name and a password to log into the virtual machine. Be sure to record the password as it will be needed in subsequent steps. Choose either an existing resource group or create a new one. Finally, click `OK`
   ![image](https://github.com/user-attachments/assets/136568ba-5292-4749-a5d7-9b97cd1634de)

1. Select a size for the virtual machine. `DS1_V2` is sufficient for our needs. Click `Select`.
   ![image](https://github.com/user-attachments/assets/e174eeec-bc24-4592-a153-7f3d0f795228)

1. In the Settings, click on `Network security group` then click `Add inbound rule`.
   ![image](https://github.com/user-attachments/assets/a9f149e1-89ef-4bb1-9fc1-30179471d237)

1. Create a rule to expose the Docker agent port, 2375, by entering a name for the rule, a priority and the port. Click `OK`.
   ![image](https://github.com/user-attachments/assets/57c06ff1-b8bc-4579-9174-c1fac9c0ced9)

1. Create a rule to expose the Web Server which will be running on the Docker agent. Entering a name for the rule, a priority and select `HTTP` from the `Service` drop down. Click `OK`.
   ![image](https://github.com/user-attachments/assets/01516ba3-dd2d-47ab-8c6b-9e35d46c1849)

1. Click `OK` on the Settings blade.
1. On the Purchase blade, click Purchase
   ![image](https://github.com/user-attachments/assets/bf986324-f705-4329-b494-b767f4ffc2ab)

1. The virtual machine will now be provisioned. Click on `Virtual Machines` and then select your new virtual machine.
   ![image](https://github.com/user-attachments/assets/62b64c2f-065a-49de-86af-8cf8ea9b998f)

1. Click on `Connect` to download the remote connection file for the virtual machine. You may also wish to record the IP address of your machine at this time - it will be needed later.
   ![image](https://github.com/user-attachments/assets/c625c211-db3e-477f-b9a2-46391cfba374)
 
## Configuring Docker on the Virtual Machine

1. Launch the connection by clicking on the downloaded connection file
   ![image](https://github.com/user-attachments/assets/93709409-67a0-418e-ad93-9b62511937cc)

1. Agree to connect
  ![image](https://github.com/user-attachments/assets/c0ff7ec8-4127-4ae2-bb51-d5dcea1de40c)

1. Enter the password and user name recorded earlier. You may need to click on `More Choices` to access the user name and password boxes. Click on `OK`.
   ![image](https://github.com/user-attachments/assets/981c7bc3-1d21-48a4-a201-bf5fdcd9e808)

1. The firewall on the server must now have ports opened to allow access to the Docker daemon and container. Click on the `Start menu` and enter `firewall` then click on the `Windows Firewall with Advanced Security`.
   ![image](https://github.com/user-attachments/assets/851d8548-1d13-41cc-b071-e4e047eb18be)

1. In the firewall settings click on `Inbound Rules` then `New Rule...`.
   ![image](https://github.com/user-attachments/assets/d11a6b89-646e-4363-82d2-1b6dd01b118e)

1. Select `Port` as the rule type and click `Next`.
   ![image](https://github.com/user-attachments/assets/24b051c4-c7a3-4e39-9db1-1481403aaf6e)

1. Select `Specific local ports` and enter `2375`. Click `Next`.
   ![image](https://github.com/user-attachments/assets/08fb4870-7d7f-4177-86f0-4b4ec4e7fe25)

1. Select `Allow the connection` and click `Next`.
   ![image](https://github.com/user-attachments/assets/a893330d-6d01-4a38-9d82-fd24d7d00216)

1. Leave the settings on the Profile page unchanged and click `Next`.
   ![image](https://github.com/user-attachments/assets/99867d71-24f3-40c7-b4ee-b1a3aecb6307)

1. Enter a name such as `Docker` for the firewall rule and click `Finish`.
   ![image](https://github.com/user-attachments/assets/05bc739e-3a83-4c30-a2b4-406cc06cad2b)

1. In the firewall settings click on `Inbound Rules` then `New Rule...`.
   ![image](https://github.com/user-attachments/assets/a0b1a5c8-f75b-427b-991e-3fbc89b91997)

1. Select `Port` as the rule type and click `Next`.
   ![image](https://github.com/user-attachments/assets/231c2444-13e9-46db-8afa-0c7058aa0847)

1. Select `Specific local ports` and enter `80`. Click `Next`.
   ![image](https://github.com/user-attachments/assets/b396e677-0688-4827-bcb0-622de098cd39)

1. Select `Allow the connection` and click `Next`.
   ![image](https://github.com/user-attachments/assets/1dcc2b17-e29f-431a-bd42-aca350d41c69)

1. Leave the settings on the Profile page unchanged and click `Next`.
   ![image](https://github.com/user-attachments/assets/f46937b1-0760-445f-970d-f2e801303d53)

1. Enter a name such as `Web` for the firewall rule and click `Finish`.
   ![image](https://github.com/user-attachments/assets/6aecd10c-9109-433c-b5fc-d927bb75fc92)

1. By default the Docker daemon doesn't listen on external ports so we need to enable that. Open an administrative PowerShell window and enter:

   ```powershell
   stop-service docker
   &"C:\Program Files\Docker\dockerd.exe" -H tcp://0.0.0.0:2375
   ```

   ![image](https://github.com/user-attachments/assets/be3b7014-bca0-47ea-9b29-fe176671edae)

The server should now be set up to allow remote Docker connections. Note: It is not secure to run the Docker daemon exposed to the Internet and without any authentication. For the purposes of this tutorial we'll allow it but in real world applications [other approaches are recommended](https://docs.docker.com/engine/security/https/).

## Configure Your Computer to Talk to Docker on the Virtual Machine

Your computer needs to be configured to talk to the Docker daemon on the virtual machine that was just set up. To do this we'll need to set an environment variable pointing Docker to the remote daemon.

1. Click on the `Start button` and search for `computer`. Right click on `This PC` and select `Properties`.
   ![image](https://github.com/user-attachments/assets/a79a069c-12cb-41c6-acf6-9956d0191827)

1. Select `Advanced system settings`.
   ![image](https://github.com/user-attachments/assets/939973e8-f01a-440e-9a69-2cb6e2196207)

1. Select `Environment Variables`.
   ![image](https://github.com/user-attachments/assets/82d984be-0981-48de-a8c9-5294817b9489)

1. Add a new system variable called `DOCKER_HOST` set it to be `tcp://<ip address of your virtual machine>:2375`
   ![image](https://github.com/user-attachments/assets/17fd1075-8eb1-4b16-bb39-e3ec47c1fff2)

## Build and Run the Container

1. Clone or download the eShopOnWeb sample to a folder on your local machine.

1. Ensure the computer on which you're running has Windows containers enabled. You can read how perform the one-time setup on the [Docker Blog](https://blog.docker.com/2016/09/build-your-first-docker-windows-server-container/)

1. Right click on the `Web` project in Visual Studio and select the `Add` menu then `Docker Support`.

   ![image](https://github.com/user-attachments/assets/8070268c-6aba-4e22-a013-6c03f11adfb0)

1. Select `Windows` and click on `OK`. This will create a new project in your solution called `docker-compose`. This project contains the settings for deploying to Docker.
   ![image](https://github.com/user-attachments/assets/635934e5-70e5-4696-97c1-ea0be963e0d3)

1. Update the `docker-compose.yml` to listen to and forward port `80` to port `5106`. Change the line

   ```docker
   ports:
      - "80"
   ```
   to

   ```docker
   ports:
      - "80:5106"
   ```

1. Build the project under `Release `configuration. This will deploy the image to the remote Docker daemon.

1. Open an instance of PowerShell and run:

   `docker run -p <ip address of your virtual machine>:80:80 web -t`

1. Direct your web browser to http://your_ip_address_of_your_virtual_machine. You should see an instance of the eShopOnWeb site running

   ![image](https://github.com/user-attachments/assets/77b0031e-5e84-4118-babb-1f774d87cd3d)
