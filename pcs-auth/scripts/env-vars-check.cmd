@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_AUTH_AAD_GLOBAL_TENANTID%" == "" (
    echo Error: the PCS_AUTH_AAD_GLOBAL_TENANTID environment variable is not defined.
    exit /B 1
)