// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Auth;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class StatusController : Controller
    {

        private const string JSON_TRUE = "true";
        private const string JSON_FALSE = "false";
        private const string PREPROVISIONED_IOTHUB_KEY = "PreprovisionedIoTHub";

        private readonly ILogger log;
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IUserManagementClient userManagementClient;
        private readonly IConfig config;
        private readonly IDevices devices;
        private readonly IDeviceProperties deviceProperties;

        public StatusController(
            IStorageAdapterClient storageAdapter,
            IUserManagementClient userManagementClient,
            ILogger logger,
            IConfig config,
            IDevices devices,
            IDeviceProperties deviceProperties
            )
        {
            this.log = logger;
            this.storageAdapter = storageAdapter;
            this.userManagementClient = userManagementClient;
            this.config = config;
            this.devices = devices;
            this.deviceProperties = deviceProperties;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var result = new StatusApiModel();
            var errors = new List<string>();

            // Check access to Storage Adapter
            var storageAdapterTuple = await this.storageAdapter.PingAsync();
            SetServiceStatus("StorageAdapter", storageAdapterTuple, result, errors);

            if (this.config.ClientAuthConfig.AuthRequired)
            {
                // Check access to Auth
                var authTuple = await this.userManagementClient.PingAsync();
                SetServiceStatus("Auth", authTuple, result, errors);
            }

            // Check access to IoTHub
            var ioTHubTuple = await this.devices.PingRegistryAsync();
            SetServiceStatus("IoTHub", ioTHubTuple, result, errors);


            // Preprovisioned IoT hub status
            var isHubPreprovisioned = this.IsHubConnectionStringConfigured();
            result.Properties.Add(PREPROVISIONED_IOTHUB_KEY, isHubPreprovisioned ? JSON_TRUE : JSON_FALSE);
            result.Properties.Add("AuthRequired", this.config.ClientAuthConfig?.AuthRequired.ToString());
            result.Properties.Add("StorageAdapterApiUrl", this.config.ServicesConfig?.StorageAdapterApiUrl);
            result.Properties.Add("UserManagementApiUrl", this.config.ServicesConfig?.UserManagementApiUrl);
            result.Properties.Add("Port", this.config.Port.ToString());

            if (errors.Count > 0)
            {
                result.Message = string.Join("; ", errors);
            }
            this.log.Info("Service status request", () => new { Healthy = result.IsHealthy, result.Message });
            return result;
        }

        // Check whether the configuration contains a connection string
        private bool IsHubConnectionStringConfigured()
        {
            var cs = this.config.ServicesConfig?.IoTHubConnString?.ToLowerInvariant().Trim();
            return (!string.IsNullOrEmpty(cs)
                    && cs.Contains("hostname=")
                    && cs.Contains("sharedaccesskeyname=")
                    && cs.Contains("sharedaccesskey="));
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
                result.IsHealthy &= serviceTuple.Item1;
            }
            result.Dependencies.Add(dependencyName, serviceStatusModel);
        }
    }
}
