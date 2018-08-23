#!/bin/bash
# Copyright (c) Microsoft. All rights reserved.

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )" 2> /dev/null

echo "Checking environment variables ...."

source $APP_HOME/scripts/local/launch/helpers/.env_uris.sh 2> /dev/null
source $APP_HOME/scripts/local/launch/helpers/.env.sh 2> /dev/null

cd $APP_HOME/scripts/local/launch

sh helpers/check-dependencies.sh device-telemetry $azres
azres=$?
sh helpers/check-dependencies.sh iothub-manager $azres
azres=$(($azres+$?))
sh helpers/check-dependencies.sh auth $azres
azres=$(($azres+$?))
sh helpers/check-dependencies.sh config $azres
azres=$(($azres+$?))
sh helpers/check-dependencies.sh asa-manager $azres
azres=$(($azres+$?))
sh helpers/check-dependencies.sh storage-adapter $azres
azres=$(($azres+$?))

if [ $azres -ne 0 ]; then
	read -p "Have you created required Azure resources (Y/N)?" yn
	echo -e "\n"

	case $yn in
	"Y") 
		echo -e "Please set the env variables on your machine. You need not run this script again.";  
		exit 0
	;;
	"N") 
		echo "Setting up Azure resources."; 
		$APP_HOME/scripts/local/launch/helpers/create-azure-resources.sh;
	;;
	*)
		echo "Incorrect option. Please re-run the script."
		exit 0
	;;
	esac
fi
