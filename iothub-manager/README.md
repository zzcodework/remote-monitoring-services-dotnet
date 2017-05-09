[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

IoTHub Manager
=====================

Handles communication with the IoT Hub (device registration, device queries, etc.)

Overview
========

WebService.csproj - C# web service exposing REST interface for IoT Hub management functionality 
WebService.Test.csproj - Unit tests for web services functionality 

Services.csproj - C# dll containining business logic for interacting with Azure services (IoTHub, etc.) 
Services.Test.csproj - Unit tests for services functionality 

Solution/scripts - contains build scripts, docker container creation scripts, and scripts for running the microservice from the command line 


How to use it
=============

For Debugging:
1. If using Visual Studio make sure you run VS as an Administrator.
2. Set your PCS_IOTHUB_CONN_STRING system environment variable for your IoT Hub connection.
3. Run F5 from VS.
4. Hit the REST api for the web service using: 
	a. http://127.0.0.1:8080/v1/status (checks status of the web service) 
	b. http://127.0.0.1:8080/v1/devices (queries for all devices) 
	c. http://127.0.0.1:8080//v1/devices/<yourindividualdevice> (queries for a single device) 
	d. <todo - create device> 
	e. <todo - create device> 

5. <todo Swagger> 

6. For running locally in a container: 
	a. <todo> 

7. For running on Azure in a container in ACS: 
	a. <todo> 


			
Configuration
=============

1. webservice\application.conf allows setting of port for the microservice (defaults to 8080). 
2. PCS_IOTHUB_CONN_STRING is a system environment variable and should contain your IoT Hub connection string. Create this environment variable before running the microservice. 
3. <todo logging/monitoring>




Other documents
===============

1. [Contributing and Development setup](CONTRIBUTING.md)
2. <todo architecture docs link>
3. <doc pointing to overarching doc for how this microservice is used in remote monitoring and other PCS types>



[build-badge]: https://img.shields.io/travis/Azure/iothub-manager-dotnet.svg
[build-url]: https://travis-ci.org/Azure/iothub-manager-dotnet
[issues-badge]: https://img.shields.io/github/issues/azure/iothub-manager-dotnet.svg
[issues-url]: https://github.com/Azure/iothub-manager-dotnet/issues
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-pcs.js.svg
[gitter-url]: https://gitter.im/azure/iot-pcs
