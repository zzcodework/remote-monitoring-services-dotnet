#!/bin/bash

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )"

source $APP_HOME/scripts/local/launch/.env
source $APP_HOME/scripts/local/launch/.env_uris

set -e

cd $APP_HOME/device-simulation/scripts/docker
./run
set +e