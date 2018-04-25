:: Copyright (c) Microsoft. All rights reserved.

::  Prepare the environment variables used by the application.
::

:: Endpoint used to retrieve the list of monitoring rules
SETX PCS_TELEMETRY_WEBSERVICE_URL "http://127.0.0.1:9004/v1"

:: Endpoint used to retrieve the list of device groups
SETX PCS_CONFIG_WEBSERVICE_URL "http://127.0.0.1:9005/v1"

:: Endpoint used to retrieve the list of devices in each group
SETX PCS_IOTHUBMANAGER_WEBSERVICE_URL "http://127.0.0.1:9002/v1"

# Connection details of the Azure Blob where event hub checkpoints and reference data are stored
SETX PCS_ASA_DATA_AZUREBLOB_ACCOUNT "..."
SETX PCS_ASA_DATA_AZUREBLOB_KEY "..."
SETX PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX "..."

# Event Hub where device notifications are stored
SETX PCS_EVENTHUB_CONNSTRING "..."