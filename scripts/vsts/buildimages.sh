#!/usr/bin/env bash
# Copyright (c) Microsoft. All rights reserved.
# Note: Windows Bash doesn't support shebang extra params

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../ && pwd )/"

servicestobuild=$(IdentifyMS.servicestobuild)
declare -A microservicefolders

microservicefolders+=(
        ["asamanager"]="asa-manager"
        ["pcsauth"]="pcs-auth"
        ["pcsconfig"]="pcs-config"
        ["iothubmanager"]="iothub-manager"
        ["pcsstorageadapter"]="pcs-storage-adapter"
        ["devicetelemetry"]="device-telemetry"
        ["devicesimulation"]="device-simulation"
)

build()
{
    IFS=','; microservices=($servicestobuild); unset IFS;
    for microservice in ${!microservices[@]}; do
        msfolder=${microservices[${microservice}]}
	    location=${microservicefolders[${msfolder}]}
		cd $location
		scripts/docker/build
		cd ..
    done
}

build