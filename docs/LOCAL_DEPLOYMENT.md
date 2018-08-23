Starting Microservices on local environment
=====
### New & Existing Users
The new repository contains a **start** script and few other scripts to bootstrap the new users with the required cloud resources. These scripts are used to create azure resources like Cosmos DB, IoTHub, Azure Stream Analytics etc. The start script is located in *scripts / local / launch* folder under root directory of this repository. If you have cloned azure-iot-pcs-remote-monitoring-dotnet repository, the scripts folder is present under services submodule (folder).

**Please Note:**
*These scripts are executable in **bash shell only**. On windows these scripts can be run manually using Git Bash shell or by using Windows Sub system for Linux. The instructions to enable WSL are available* *[here](https://docs.microsoft.com/en-us/windows/wsl/install-win10).*

#### Start script
This script checks if required environment variables are set on the local system. If the variables are set then one can open the IDE to start the microservices. If the variables are not set then this script will guide through the process of creating the new variables. It will then create different scripts under *scripts / local / launch / os / OS_TYPE /* which can be used to set environment variables on the machine.

For users who have already created the required azure resources, please set the envvironment variables on your machine so as to be accessible by the IDE. Alternatively, these varaibles can be set in the launch configurations of the IDE in launch.json for VS code or Debug Settings under Properties for solution in Visual Studio. Although not recommended, environment variables can also be set in appsettings.ini file present under WebService folder for each of the microservices.

**Please Note:**
*This script requires **Node.js** to execute, please install Node (version < 8.11.2) before using this script. Also, this script might require administartive privileges or sudo permission as it tries to install node packages, if they are not already installed. At times, the script might fail while installing npm packages. In such cases, please install npm package **iot-solutions** using following command using administartive privileges or sudo access.*

*npm install -g iot-solutions*
&nbsp; 

##### Usage:   
````
abc@pcs sh start.sh   
````
![start](https://user-images.githubusercontent.com/39531904/44435771-6ab08280-a566-11e8-93c9-e6f35e5df247.PNG)
 
#### helpers scripts
These scripts are located under helpers folder which is under the launch folder. The script create-azure-resources.sh can be independently called to create resources in the cloud. The script check_dependencies.sh checks if environment variables are set for a particular microservices.
##### Usage:
1) check environment variables for a microservice 
sh check-dependencies.sh <microservice_folder_name> 
2) create Azure resources 
sh create-azure-resources.sh

After creating the required azure resources, using start or create-azure-resources.sh, one should execute the following scripts present under *os/{linux / win / osx}* to set the environment variables. 
1) set-env-uri
2) set-env

**Please Note:**
*If you are using windows, you will have to execute these scripts in CMD shell. On OSX, these scripts are automatically run by the start script. For linux, the environment variables present in these scripts need to be set at global location, depending upon the flavour of linux you are using.* 

#### Recap of steps to create resources and set environment variables
1) Run start.sh
2) Run scripts under os folder. 

#### Walk through for importing new Solution in IDE
##### VS Code 
The preconfigured launch & task configuration(s) for VS code are included in the *scripts / local / launch / idesettings* folder. These settings are useful for building individual OR all microservices. 

##### Steps to import launch settings
1) Import this repository OR the services submodule from the azure-iot-pcs-remote-monitoring-dotnet.
2) Click the Add Configuration under present under debug menu. (This will create .vs folder) 
![vs](https://user-images.githubusercontent.com/39531904/44294751-611ad800-a251-11e8-8a14-7fc7bc3c6aed.PNG)
3) Replace the auto-created launch.json & task.json files with files under vscode folder which is present under idesettings. 
4) This will list all the debug/build configuration. 

##### Visual Studio
1) If you have set the environment variables using the scripts, then you could use the Visual Studio to debug by starting multiple startup projects. Please follow the instructions [here](https://msdn.microsoft.com/en-us/library/ms165413.aspx) to set multiple startup projects.
2) If you haven't set the environment variables, then they could be set in following files.
    1. appsettings.ini under WebService
    2. launchSettings.json under Properties folder under WebService.
3) For multiple startup project settings, please set only WebService projects as startup projects.   
4) Start device simulation service using start-device-simulation script present under launch folder.

Structure of the microservices
=========
Each microservice comprises of following projects/folders. 
1) scripts 
2) WebService  
3) Service  
4) WebService.Test  
5) Service.Test

Description: 
1) Scripts  
The scripts folder is organized as follows\
i. **docker** sub folder for building docker containers of the current microservice.\
ii. **root** folder contains scripts for building and running services natively.\
&nbsp; 
![script folder structure](https://user-images.githubusercontent.com/39531904/44290937-10df4e00-a230-11e8-9cd4-a9c0644e166b.PNG "Caption")\
The docker build scripts require environment variables to be set up before execution. The run scripts can run both natively built and dockerized microservice. The run script under docker folder can also be independently used to pull and run published docker images. One can modify the tag and the account to pull different version or privately built docker images.
&nbsp; 

2) WebService  
It contains code for REST endpoints of the microservice.
&nbsp;  

3) Service  
It contains business logic and code interfacing various SDKs. 
&nbsp;

4) WebService.Test  
It contains unit tests for the REST endpoints of the microservice. 
&nbsp; 

5) Service  
It contains unit tests for the business logic and code interfacing various SDKs.
&nbsp;  

6) Other Projects  
The microservice might contain other projects such as RecurringTaskAgent etc.
