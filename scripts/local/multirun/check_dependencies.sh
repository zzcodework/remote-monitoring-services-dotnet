#!/usr/bin/env bash
set -e

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )/"
echo "Working directory $APP_Home"

microservice=$1
azres=false

cd $APP_HOME/$microservice

./scripts/env-vars-check

if [ $? -ne 0 ]; then
    echo "Have you created the Azure resources for the project?"
	select yn in "Yes" "No"; do
		case $yn in
			Yes ) echo -e "Please set the env variables in .env file.\n The file is located under scripts/local folder."; azres=true; break;;
			No ) echo "Setting up Azure resources."; break;;
		esac
	done
fi

if [ "$azres" = "false" ]; then
	$APP_HOME/scripts/local/multirun/create_azure_resources.sh
fi

if [ "$microservice" = "pcs-config" ]; then
	cd $APP_HOME
	./device-simulation/scripts/docker/run
fi

set +e

