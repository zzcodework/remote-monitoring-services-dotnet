#!/usr/bin/bash
# Copyright (c) Microsoft. All rights reserved.
set -e

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../../ && pwd )" 2> /dev/null

set +e
test=`echo $APP_HOME | grep "remote\-monitoring\-services\-dotnet$"`
set -e
if [ -z $test ];
then
    APP_HOME="$APP_HOME/remote-monitoring-services-dotnet"
fi

cd $APP_HOME/$1

./scripts/env-vars-check > /dev/null

set +e
