// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class ModulesController : Controller
    {
        const string CONTINUATION_TOKEN_NAME = "x-ms-continuation";

        private readonly IDevices devices;

        public ModulesController(IDevices devices)
        {
            this.devices = devices;
        }

        /// <summary>Get a list of devices</summary>
        /// <returns>List of devices</returns>
        [HttpGet]
        public async Task<TwinPropertiesListApiModel> GetModuleTwinsAsync([FromQuery] string query)
        {
            string continuationToken = string.Empty;
            if (this.Request.Headers.ContainsKey(CONTINUATION_TOKEN_NAME))
            {
                continuationToken = this.Request.Headers[CONTINUATION_TOKEN_NAME].FirstOrDefault();
            }

            return new TwinPropertiesListApiModel(
                await this.devices.GetModuleTwinsByQueryAsync(query, continuationToken));
        }

        [HttpPost("query")]
        public async Task<TwinPropertiesListApiModel> QueryModuleTwinsAsync([FromBody] string query)
        {
            return await this.GetModuleTwinsAsync(query);
        }

        /// <summary>Get module information for a device</summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="moduleId">Module Id</param>
        /// <returns>Device information</returns>
        [HttpGet("{deviceId}/{moduleId}")]
        public async Task<TwinPropertiesApiModel> GetModuleTwinAsync(string deviceId, string moduleId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new InvalidInputException("deviceId must be provided");
            }

            if (string.IsNullOrWhiteSpace(moduleId))
            {
                throw new InvalidInputException("moduleId must be provided");
            }

            var twin = await this.devices.GetModuleTwinAsync(deviceId, moduleId);
            return new TwinPropertiesApiModel(twin.DesiredProperties, twin.ReportedProperties,
                                              deviceId, moduleId);
        }
    }
}
