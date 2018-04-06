:: Copyright (c) Microsoft. All rights reserved.

::  Prepare the environment variables used by the application.
::

:: Endpoint used to retrieve the list of monitoring rules
SETX PCS_TELEMETRY_WEBSERVICE_URL "http://127.0.0.1:9004/v1"

:: Endpoint used to retrieve the list of device groups
SETX PCS_CONFIG_WEBSERVICE_URL "http://127.0.0.1:9005/v1"

:: Endpoint used to retrieve the list of devices in each group
SETX PCS_IOTHUBMANAGER_WEBSERVICE_URL "http://127.0.0.1:9002/v1"
