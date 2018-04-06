:: Copyright (c) Microsoft. All rights reserved.

@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_TELEMETRY_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_TELEMETRY_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_CONFIG_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_CONFIG_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBMANAGER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_IOTHUBMANAGER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

endlocal
