// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Controllers
{
    /*
     * StatusController is not accessible on deployed services because it is an internal service running in
     * the background. The below status API responds only on local machine. 
     */
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IDeviceGroupsClient configClient;
        private readonly IDevicesClient ioTHubManagerClient;
        private readonly IRules deviceTelemetryClient;
        private readonly IBlobStorageHelper blobStorageHelper;
        private readonly IAsaStorage asaStorage;
        private readonly IAgent eventHubHelper;
        private readonly IConfig config;

        private const string MESSAGE_STORAGE_TYPE = "CosmosDbSql";

        public StatusController(
            IDeviceGroupsClient configClient,
            IDevicesClient ioTHubManagerClient,
            IRules deviceTelemetryClient,
            IBlobStorageHelper blobStorageHelper,
            IAsaStorage asaStorage,
            IAgent eventHubHelper,
            ILogger logger,
            IConfig config
            )
        {
            this.log = logger;
            this.configClient = configClient;
            this.ioTHubManagerClient = ioTHubManagerClient;
            this.blobStorageHelper = blobStorageHelper;
            this.deviceTelemetryClient = deviceTelemetryClient;
            this.asaStorage = asaStorage;
            this.eventHubHelper = eventHubHelper;
            this.config = config;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel();
            var errors = new List<string>();

            // Check access to Config
            var configTuple = await this.configClient.PingAsync();
            SetServiceStatus("Config", configTuple, result, errors);

            // Check access to Device Telemetry
            var deviceTelemetryTuple = await this.deviceTelemetryClient.PingAsync();
            SetServiceStatus("DeviceTelemetry", deviceTelemetryTuple, result, errors);

            // Check access to IoTHubManager
            var ioTHubmanagerTuple = await this.ioTHubManagerClient.PingAsync();
            SetServiceStatus("IoTHubManager", ioTHubmanagerTuple, result, errors);

            // Check access to Blob
            var blobTuple = await this.blobStorageHelper.PingAsync();
            SetServiceStatus("Blob", blobTuple, result, errors);

            // Check access to Storage
            if (this.config.ServicesConfig.MessagesStorageType.ToString().Equals(
            MESSAGE_STORAGE_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                var storageTuple = await this.asaStorage.PingAsync();
                SetServiceStatus("Storage", storageTuple, result, errors);
            }

            // Check access to Event
            var eventHubTuple = await this.eventHubHelper.PingEventHubAsync();
            SetServiceStatus("EventHub", eventHubTuple, result, errors);
            result.Properties.Add("EventHubSetUp", this.eventHubHelper.IsEventHubSetupSuccessful().ToString());

            // Add properties
            result.Properties.Add("ConfigServiceUrl", this.config.ServicesConfig?.ConfigServiceUrl);
            result.Properties.Add("IotHubManagerServiceUrl", this.config.ServicesConfig?.IotHubManagerServiceUrl);
            result.Properties.Add("TelemetryServiceUrl", this.config.ServicesConfig?.RulesWebServiceUrl);
            result.Properties.Add("EventHubName", this.config.ServicesConfig?.EventHubName);
            result.Properties.Add("MessagesStorageType", this.config.ServicesConfig?.MessagesStorageType.ToString());
            result.Properties.Add("AlarmsStorageType", this.config.ServicesConfig?.AlarmsStorageType.ToString());
            result.Properties.Add("Port", this.config.Port.ToString());

            if (errors.Count > 0)
            {
                result.Message = string.Join("; ", errors);
            }

            this.log.Info("Service status request", () => new { Healthy = result.IsHealthy, result.Message });
            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            Tuple<bool, string> serviceTuple,
            StatusApiModel result,
            List<string> errors
            )
        {
            if (serviceTuple == null)
            {
                return;
            }
            var serviceStatusModel = new StatusModel
            {
                Message = serviceTuple.Item2,
                IsHealthy = serviceTuple.Item1
            };

            if (!serviceTuple.Item1)
            {
                errors.Add(dependencyName + " check failed");
                result.IsHealthy = serviceTuple.Item1;
            }
            result.Dependencies.Add(dependencyName, serviceStatusModel);
        }
    }
}
