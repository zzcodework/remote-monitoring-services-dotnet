using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers.DeviceStatusHelper
{
    public interface IDeviceStatus
    {
        IDictionary<string, DeploymentStatus> GetDeviceStatuses();
    }
    public abstract class DeviceStatus : IDeviceStatus
    {
        private RegistryManager registry;
        private ILogger log;
        private const string DEVICE_ID_KEY = "DeviceId";

        public DeviceStatus(RegistryManager registry, ILogger log)
        {
            this.registry = registry;
            this.log = log;
        }

        protected HashSet<string> GetDevicesInQuery(string hubQuery, string deploymentId)
        {
            var query = string.Format(hubQuery, deploymentId);
            var queryResponse = this.registry.CreateQuery(query);
            var deviceIds = new HashSet<string>();

            try
            {
                while (queryResponse.HasMoreResults)
                {
                    // TODO: Add pagination with queryOptions
                    var resultSet = queryResponse.GetNextAsJsonAsync();
                    foreach (var result in resultSet.Result)
                    {
                        var deviceId = JToken.Parse(result)[DEVICE_ID_KEY];
                        deviceIds.Add(deviceId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting status of devices in query {query}", () => new { ex.Message });
            }

            return deviceIds;
        }

        public abstract IDictionary<string, DeploymentStatus> GetDeviceStatuses();
    }
}
