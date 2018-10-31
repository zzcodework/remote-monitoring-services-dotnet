// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IKeyValueContainer keyValueContainer;
        private readonly IConfig config;

        public StatusController(
            ILogger logger,
            IKeyValueContainer keyValueContainer,
            IConfig config
            )
        {
            this.log = logger;
            this.keyValueContainer = keyValueContainer;
            this.config = config;
        }

        public StatusApiModel Get()
        {
            var result = new StatusApiModel();
            var errors = new List<string>();

            // Check connection to CosmosDb
            var storageTuple = this.keyValueContainer.Ping();
            SetServiceStatus("Storage", storageTuple, result, errors);

            result.Properties.Add("StorageType", this.config.ServicesConfig?.StorageType);
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
                result.IsHealthy = serviceTuple.Item1;
            }
            result.Dependencies.Add(dependencyName, serviceStatusModel);
        }
    }
}
