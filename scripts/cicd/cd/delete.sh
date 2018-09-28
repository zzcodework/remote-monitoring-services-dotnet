#!/bin/bash
####### helm delete release
echo "Config current context"
kubectl config current-context 

echo "Deleting services from above context"
helm delete storageadapter
helm delete telemetry
helm delete iothubmanager
helm delete simulation
helm delete auth
helm delete webui
helm delete asamanager
helm delete config
####### helm purgedelete to ensure if above cmds don't work
helm del --purge storageadapter
helm del --purge telemetry
helm del --purge iothubmanager
helm del --purge simulation
helm del --purge auth
helm del --purge webui
helm del --purge asamanager
helm del --purge config
cat ~/.kube/config
exit 0
