// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IStorageAdapterClient storageAdapterClient;
        private readonly IUserManagementClient userManagementClient;
        private readonly IDeviceSimulationClient deviceSimulationClient;
        private readonly IDeviceTelemetryClient deviceTelemetryClient;
        private readonly IConfig config;

        public StatusController(
            IStorageAdapterClient storageAdapterClient,
            IUserManagementClient userManagementClient,
            IDeviceSimulationClient deviceSimulationClient,
            IDeviceTelemetryClient deviceTelemetryClient,
            ILogger logger,
            IConfig config
            )
        {
            this.log = logger;
            this.storageAdapterClient = storageAdapterClient;
            this.userManagementClient = userManagementClient;
            this.deviceSimulationClient = deviceSimulationClient;
            this.deviceTelemetryClient = deviceTelemetryClient;
            this.config = config;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel();
            var errors = new List<string>();

            // Check access to Storage Adapter
            var storageAdapterTuple = await this.storageAdapterClient.PingAsync();
            SetServiceStatus("StorageAdapter", storageAdapterTuple, result, errors);

            // Check access to Device Telemetry
            var deviceTelemetryTuple = await this.deviceTelemetryClient.PingAsync();
            SetServiceStatus("DeviceTelemetry", deviceTelemetryTuple, result, errors);

            // Check access to Device Simulation
            var devicesSmulationTuple = await this.deviceSimulationClient.PingAsync();
            SetServiceStatus("DeviceSimulation", devicesSmulationTuple, result, errors);

            // Check access to Auth
            var authTuple = await this.userManagementClient.PingAsync();
            SetServiceStatus("Auth", authTuple, result, errors);

            // Add properties
            result.Properties.Add("DeviceSimulationApiUrl", this.config.ServicesConfig?.DeviceSimulationApiUrl);
            result.Properties.Add("StorageAdapterApiUrl", this.config.ServicesConfig?.StorageAdapterApiUrl);
            result.Properties.Add("UserManagementApiUrl", this.config.ServicesConfig?.UserManagementApiUrl);
            result.Properties.Add("TelemetryApiUrl", this.config.ServicesConfig?.TelemetryApiUrl);
            result.Properties.Add("SeedTemplate", this.config.ServicesConfig?.SeedTemplate);
            result.Properties.Add("SolutionType", this.config.ServicesConfig?.SolutionType);
            result.Properties.Add("AuthRequired", this.config.ClientAuthConfig?.AuthRequired.ToString());
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
