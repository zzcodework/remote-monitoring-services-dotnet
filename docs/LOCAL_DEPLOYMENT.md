Starting Microservices on local environemnt
=====
### New & Existing Users
The new repository contains a **start** script and few other scripts to bootstrap the new users with the required cloud resources. These scripts are used to create azure resources like Cosmos DB, IoTHub, Azure Stream Analytics etc. The start script is located in *scripts / local / launch* folder under root directory of the repository.


**Please Note:**
*The scripts are executable in **bash shell only**. On windows these scripts can be manually run using* *Git Bash shell or by using Windows Sub system for Linux. The instructions to enable WSL are available* *[here](https://docs.microsoft.com/en-us/windows/wsl/install-win10).*


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
![start](https://user-images.githubusercontent.com/39531904/44294555-edc39700-a24d-11e8-8053-07ab4c185442.PNG)
 
#### Helper scripts
These scripts are located under helpers folder which is under the launch folder. The script create_azure_resources.sh can be independently called to create resources in the cloud. The script check_dependencies.sh checks if environment variables are set for a particular microservices.
##### Usage:
1) check environment variables for a microservice 
sh check_dependencies.sh <microservice_folder_name> 
2) create Azure resources 
sh create_azure_resources.sh

After creating the reuired azure resources, using start or create_azure_resources.sh, one should execute the scripts under *os/{linux / win / osx}* to set the environment variables. 

#### Recap of steps to set environent variables
1) Run start.sh
2) Run scripts under os folder. 

#### Walk through for importing new Solution in IDE
##### VS Code 
The preconfigured launch & task configuration(s) for VS code are included in the *scripts / local / launch / idesettings* folder. These settings are useful for building individual OR all microservices. 

##### Steps to import launch settings

1) Set the required environment variables in the .env file under launch folder. 
2) Click of the debug icon on the left-hand panel of the IDE. (This will create .vs folder under root folder in the repo) 
![vs](https://user-images.githubusercontent.com/39531904/44294751-611ad800-a251-11e8-8a14-7fc7bc3c6aed.PNG)
3) Replace the auto-created launch.json & task.json files under .vs folder with files under idesettings folder. 
4) This will automatically list all the debug/build configuration. Build All will build all the microservice(s). 