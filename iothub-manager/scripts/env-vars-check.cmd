@ECHO off
setlocal enableextensions enabledelayedexpansion

IF "%PCS_IOTHUBMANAGER_WEBSERVICE_PORT%" == "" (
    echo Error: the PCS_IOTHUBMANAGER_WEBSERVICE_PORT environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUB_CONNSTRING%" == "" (
    echo Error: the PCS_IOTHUB_CONNSTRING environment variable is not defined.
    exit /B 1
)

endlocal
