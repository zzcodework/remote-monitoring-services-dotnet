export PCS_IOTHUBREACT_ACCESS_CONNSTRING=HostName=iothub-2iwcm.azure-devices.net\;SharedAccessKeyName=iothubowner\;SharedAccessKey=p1N+YxW9I8PXeZjeIWcxHVXlohwIyZ5BEAz1Wg/PrR0=
export PCS_IOTHUB_CONNSTRING=HostName=iothub-2iwcm.azure-devices.net\;SharedAccessKeyName=iothubowner\;SharedAccessKey=p1N+YxW9I8PXeZjeIWcxHVXlohwIyZ5BEAz1Wg/PrR0=
export PCS_STORAGEADAPTER_DOCUMENTDB_CONNSTRING=AccountEndpoint=https://documentdb-2iwcm.documents.azure.com:443/\;AccountKey=PEYzE7iKmsfmiCT5T12Il3fjiu5hlX57eQiBmEeCEHIGndvDxvNnqauq8lTOnH5Ag0x7zlD8kHGDZJnw3Ysa5Q==\;
export PCS_TELEMETRY_DOCUMENTDB_CONNSTRING=AccountEndpoint=https://documentdb-2iwcm.documents.azure.com:443/\;AccountKey=PEYzE7iKmsfmiCT5T12Il3fjiu5hlX57eQiBmEeCEHIGndvDxvNnqauq8lTOnH5Ag0x7zlD8kHGDZJnw3Ysa5Q==\;
export PCS_TELEMETRYAGENT_DOCUMENTDB_CONNSTRING=AccountEndpoint=https://documentdb-2iwcm.documents.azure.com:443/\;AccountKey=PEYzE7iKmsfmiCT5T12Il3fjiu5hlX57eQiBmEeCEHIGndvDxvNnqauq8lTOnH5Ag0x7zlD8kHGDZJnw3Ysa5Q==\;
export PCS_IOTHUBREACT_HUB_ENDPOINT=Endpoint=sb://iothub-ns-iothub-2iw-690693-4172eac1e4.servicebus.windows.net/
export PCS_IOTHUBREACT_HUB_PARTITIONS=4
export PCS_IOTHUBREACT_HUB_NAME=iothub-2iwcm
export PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT=storage2iwcm
export PCS_IOTHUBREACT_AZUREBLOB_KEY=XE3svS3BZH0RuwkLrzIWSJLKaqMtNwwwDpwEl7Bf1LYHCoXtxdehPrI2IaUsKD6EaWE7wWieXT4zJM3L+cLhQQ==
export PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX=core.windows.net
export PCS_ASA_DATA_AZUREBLOB_ACCOUNT=storage2iwcm
export PCS_ASA_DATA_AZUREBLOB_KEY=XE3svS3BZH0RuwkLrzIWSJLKaqMtNwwwDpwEl7Bf1LYHCoXtxdehPrI2IaUsKD6EaWE7wWieXT4zJM3L+cLhQQ==
export PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX=core.windows.net
export PCS_EVENTHUB_CONNSTRING=Endpoint=sb://eventhubnamespace-2iwcm.servicebus.windows.net/\;SharedAccessKeyName=RootManageSharedAccessKey\;SharedAccessKey=7pSZ7YoZ3kvBEjOpjCJDZLWmlX4hkUPsChmgmPt7QCQ=
export PCS_EVENTHUB_NAME=eventhub-2iwcm
export PCS_AUTH_REQUIRED=false
export PCS_AZUREMAPS_KEY=static
export PCS_IOTHUBMANAGER_WEBSERVICE_URL=http://localhost:9002/v1
export PCS_CONFIG_WEBSERVICE_URL=http://localhost:9005/v1
export PCS_TELEMETRY_WEBSERVICE_URL=http://localhost:9004/v1
echo "Set environment variables"
env | grep PCS
echo "Run ASA manager..."
sleep 20
dotnet run --no-restore --no-build --no-launch-profile