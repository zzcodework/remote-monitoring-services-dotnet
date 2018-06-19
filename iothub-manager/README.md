[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

# IoTHub Manager Overview

This service manages most of Azure IoT Hub interactions, such as creating and managing IoT devices, device twins, invoking methods, managing IoT credentials, etc. This service is also used to run queries to retrieve devices belonging to a particular group (defined by the user).

The microservice provides a RESTful endpoint to manage devices, device twins, commands, methods and all the Azure IoT Hub features required by the [Azure IoT Remote Monitoring](https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet) project.

## Why?

This microservice was built as part of the [Azure IoT Remote Monitoring](https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet) project to provide a generic implementation for an end-to-end IoT solution. More information [here][rm-arch-url].

## Features

* Device creation in IoT Hub
* Read for all devices
* Read for a single device
* Query for set of devices
* Update devices
* Delete devices
* Schedule jobs
    * Execute methods
    * Update the device twin
* Get a list of jobs
* Get a single job

## Documentation

* View the API documentation in the [Wiki](https://github.com/Azure/iothub-manager-dotnet/wiki)
* [Contributing and Development setup](CONTRIBUTING.md)
* [Development setup, scripts and tools](DEVELOPMENT.md)

# How to use

## Running the service with Docker

You can run the microservice and its dependencies using [Docker](https://www.docker.com/)
with the instructions [here][run-with-docker-url].

## Running the service locally

## Prerequisites

### 1. Deploy Azure Services

This service has a dependency on the following Azure resources. Follow the instructions
for [Deploy the Azure services](https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-deploy-local#deploy-the-azure-services) to deploy the required resources.

* [Azure IoT Hub](https://docs.microsoft.com/azure/iot-hub/)

### 2. Setup Dependencies

This service depends on the [Config microservice](https://github.com/Azure/pcs-config-dotnet). Run the config service using the the instructions in the Config service [README.md](https://github.com/Azure/pcs-config-dotnet/blob/master/README.md).

* [Config microservice](https://github.com/Azure/pcs-config-dotnet)

> Note: you can also use a [deployed endpoint][deploy-rm] with [Authentication disabled][disable-auth] (e.g. https://{your-resource-group}.azurewebsites.net/config/v1)

### 3. Environment variables required to run the service
In order to run the service, some environment variables need to be created
at least once. See specific instructions for IDE or command line setup below for
more information. More information on environment variables
[here](#configuration-and-environment-variables).

* `PCS_IOTHUB_CONNSTRING` = {your Azure IoT Hub connection string from [Deploy Azure Services](#deploy-azure-services)}
    *  More information on where to find your IoT Hub connection string [here].[iothub-connstring-blog].
* `PCS_CONFIG_WEBSERVICE_URL` = http://localhost:9005/v1
    * The url for the [Config microservice](https://github.com/Azure/pcs-config-dotnet) from [Setup Dependencies](#setup-dependencies)

## Running the service with Visual Studio
1. Make sure the [Prerequisites](#prerequisites) are set up.
1. Install any edition of [Visual Studio 2017][vs-install-url] or Visual
   Studio for Mac. When installing check ".NET Core" workload. If you
   already have Visual Studio installed, then ensure you have
   [.NET Core Tools for Visual Studio 2017][dotnetcore-tools-url]
   installed (Windows only).
1. Open the solution in Visual Studio
1. Edit the WebService project properties by right clicking on the
   Webservice project > Properties > Debug. Add following required
   environment variables to the Debug settings. In Windows
   you can also set these [in your system][windows-envvars-howto-url].
   1. `PCS_IOTHUB_CONNSTRING` = {your Azure IoT Hub connection string}
   1. `PCS_CONFIG_WEBSERVICE_URL` = {config service endpoint}
1. In Visual Studio, start the WebService project
1. Using an HTTP client like [Postman][postman-url],
   use the [RESTful API][project-wiki] to test out the service.

## Running the service from the command line

1. Make sure the [Prerequisites](#prerequisites) are set up.
1. Set the following environment variables in your system. More information on environment variables [here](#configuration-and-environment-variables).
    1. `PCS_IOTHUB_CONNSTRING` = {your Azure IoT Hub connection string}
    1. `PCS_CONFIG_WEBSERVICE_URL` = {config service endpoint}
1. Use the scripts in the [scripts](scripts) folder for many frequent tasks:

* `build`: compile all the projects and run the tests.
* `compile`: compile all the projects.
* `run`: compile the projects and run the service. This will prompt for
  elevated privileges in Windows to run the web service.

## Project Structure

This microservice contains the following projects:
* **WebService.csproj** - C# web service exposing REST interface for storage
functionality
* **WebService.Test.csproj** - Unit tests for web services functionality
* **Services.csproj** - C# assembly containining business logic for interacting 
with Azure Cosmos account with type SQL
* **Services.Test.csproj** - Unit tests for services functionality
* **Solution/scripts** - Contains build scripts, docker container creation scripts, 
and scripts for running the microservice from the command line

## Updating the Docker image

The `scripts` folder includes a [docker](scripts/docker) subfolder with the files
required to package the service into a Docker image:

* `Dockerfile`: docker images specifications
* `build`: build a Docker container and store the image in the local registry
* `run`: run the Docker container from the image stored in the local registry
* `content`: a folder with files copied into the image, including the entry point script

# Configuration and Environment variables

The service configuration is stored using ASP.NET Core configuration
adapters, in [appsettings.ini](WebService/appsettings.ini). The INI format allows to
store values in a readable format, with comments. The application also
supports references to environment variables, which is used to import
credentials and networking details.

The configuration files in the repository reference some environment
variables that need to be created at least once. Depending on your OS and
the IDE, there are several ways to manage environment variables:

* Windows: the variables can be set [in the system][windows-envvars-howto-url]
  as a one time only task. The
  [env-vars-setup.cmd](scripts/env-vars-setup.cmd) script included needs to
  be prepared and executed just once. The settings will persist across
  terminal sessions and reboots.
* Visual Studio: the variables can be set in the project settings for WebService
  under Project Properties -> Configuration
  Properties -> Environment
* For Linux and OSX environments, the [env-vars-setup](scripts/env-vars-setup)
  script needs to be executed every time a new console is opened.
  Depending on the OS and terminal, there are ways to persist values
  globally, for more information these pages should help:
  * https://stackoverflow.com/questions/13046624/how-to-permanently-export-a-variable-in-linux
  * https://stackoverflow.com/questions/135688/setting-environment-variables-in-os-x
  * https://help.ubuntu.com/community/EnvironmentVariables

# Contributing to the solution

Please follow our [contribution guidelines](CONTRIBUTING.md).  We love PRs too.

# Feedback

Please enter issues, bugs, or suggestions as GitHub Issues [here](https://github.com/Azure/iothub-manager-dotnet/issues).

# License

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the [MIT](LICENSE) License.

[build-badge]: https://img.shields.io/travis/Azure/iothub-manager-dotnet.svg
[build-url]: https://travis-ci.org/Azure/iothub-manager-dotnet
[issues-badge]: https://img.shields.io/github/issues/azure/iothub-manager-dotnet.svg
[issues-url]: https://github.com/Azure/iothub-manager-dotnet/issues
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-solutions.js.svg
[gitter-url]: https://gitter.im/azure/iot-solutions

[project-wiki]: https://github.com/Azure/device-telemetry-dotnet/wiki/%5BAPI-Specifications%5D-Messages
[run-with-docker-url]:(https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-deploy-local#run-the-microservices-in-docker)
[rm-arch-url]:https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-sample-walkthrough
[postman-url]: https://www.getpostman.com
[vs-install-url]: https://www.visualstudio.com/downloads
[dotnetcore-tools-url]: https://www.microsoft.com/net/core#windowsvs2017
[windows-envvars-howto-url]: https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10
[iothub-connstring-blog]:https://blogs.msdn.microsoft.com/iotdev/2017/05/09/understand-different-connection-strings-in-azure-iot-hub/
[deploy-rm]:https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-deploy
[disable-auth]:https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet/wiki/Developer-Reference-Guide#disable-authentication
