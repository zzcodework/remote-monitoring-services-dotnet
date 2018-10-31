// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.CosmosDB;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IStorageClient cosmosDb;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly IUserManagementClient userManagementClient;
        private readonly IDiagnosticsClient diagnosticsClient;
        private readonly ILogger log;

        private const string STORAGE_TYPE_KEY = "StorageType";
        private const string TIME_SERIES_KEY = "tsi";
        private const string TIME_SERIES_EXPLORER_URL_KEY = "TsiExplorerUrl";
        private const string TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR = ".";

        public StatusController(
            IConfig config,
            IStorageClient cosmosDb,
            IStorageAdapterClient storageAdapter,
            ITimeSeriesClient timeSeriesClient,
            IUserManagementClient userManagementClient,
            IDiagnosticsClient diagnosticsClient,
            ILogger logger)
        {
            this.config = config;
            this.cosmosDb = cosmosDb;
            this.storageAdapter = storageAdapter;
            this.timeSeriesClient = timeSeriesClient;
            this.userManagementClient = userManagementClient;
            this.diagnosticsClient = diagnosticsClient;
            this.log = logger;
        }

        [HttpGet]
        public async Task<StatusApiModel> Get()
        {
            var result = new StatusApiModel();
            var errors = new List<string>();
            var explorerUrl = string.Empty;

            // Check access to Storage Adapter
            var storageAdapterTuple = await this.storageAdapter.PingAsync();
            SetServiceStatus("StorageAdapter", storageAdapterTuple, result, errors);

            if (this.config.ClientAuthConfig.AuthRequired)
            {
                // Check access to Auth
                var authTuple = await this.userManagementClient.PingAsync();
                SetServiceStatus("Auth", authTuple, result, errors);
            }

            if (this.config.ServicesConfig.DiagnosticsApiUrl != "")
            {
                // Check access to diagnostics
                var diagnosticsTuple = await this.diagnosticsClient.PingAsync();
                SetServiceStatus("Diagnostics", diagnosticsTuple, result, errors);
            }

            // Check connection to CosmosDb
            var cosmosDbTuple = this.cosmosDb.Ping();
            SetServiceStatus("Storage", cosmosDbTuple, result, errors);

            // Add Time Series Dependencies if needed
            if (this.config.ServicesConfig.StorageType.Equals(
                TIME_SERIES_KEY,
                StringComparison.OrdinalIgnoreCase))
            {
                // Check connection to Time Series Insights
                var timeSeriesTuple = await this.timeSeriesClient.PingAsync();
                SetServiceStatus("Storage", timeSeriesTuple, result, errors);
               
                // Add Time Series Insights explorer url
                var timeSeriesFqdn = this.config.ServicesConfig.TimeSeriesFqdn;
                var environmentId = timeSeriesFqdn.Substring(0, timeSeriesFqdn.IndexOf(TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR));
                explorerUrl = this.config.ServicesConfig.TimeSeriesExplorerUrl +
                    "?environmentId=" + environmentId +
                    "&tid=" + this.config.ServicesConfig.ActiveDirectoryTenant;
                result.Properties.Add(TIME_SERIES_EXPLORER_URL_KEY, explorerUrl);
            }

            if (errors.Count > 0)
            {
                result.Message = string.Join("; ", errors);
            }

            this.log.Info("Service status request", () => new { Healthy = result.IsHealthy, result.Message });

            result.Properties.Add(STORAGE_TYPE_KEY, this.config.ServicesConfig?.StorageType);
            result.Properties.Add("UserManagementApiUrl", this.config.ServicesConfig?.UserManagementApiUrl);
            result.Properties.Add("StorageAdapterApiUrl", this.config.ServicesConfig?.StorageAdapterApiUrl);
            result.Properties.Add("DiagnosticsApiUrl", this.config.ServicesConfig?.DiagnosticsApiUrl);
            result.Properties.Add("Port", this.config.Port.ToString());

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
                result.IsHealthy &= serviceTuple.Item1;
            }
            result.Dependencies.Add(dependencyName, serviceStatusModel);
        }
    }
}
