// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using System.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.Path + "/[controller]"), ExceptionsFilter]
    public class DevicesController : Controller
    {
        private readonly IDevices devices;

        const string ContinuationTokenName = "x-ms-continuation";

        public DevicesController(IDevices devices)
        {
            this.devices = devices;
        }

        /// <summary>Get a list of devices</summary>
        /// <returns>List of devices</returns>
        [HttpGet]
        public async Task<DeviceListApiModel> GetDevicesAsync([FromQuery] string query)
        {
            string continuationToken = string.Empty;
            if( Request.Headers.ContainsKey(ContinuationTokenName))
            {
                continuationToken = Request.Headers[ContinuationTokenName].FirstOrDefault();
            }

            return new DeviceListApiModel(await this.devices.GetListAsync(query, continuationToken));
        }

        /// <summary>Get one device</summary>
        /// <param name="id">Device Id</param>
        /// <returns>Device information</returns>
        [HttpGet("{id}")]
        public async Task<DeviceRegistryApiModel> GetDeviceAsync(string id)
        {
            return new DeviceRegistryApiModel(await this.devices.GetAsync(id));
        }

        /// <summary>Create one device</summary>
        /// <param name="device">Device information</param>
        /// <returns>Device information</returns>
        [HttpPost]
        public async Task<DeviceRegistryApiModel> PostAsync([FromBody] DeviceRegistryApiModel device)
        {
            return new DeviceRegistryApiModel(await this.devices.CreateAsync(device.ToServiceModel()));
        }

        /// <summary>Update device twin</summary>
        /// <param name="id">Device Id</param>
        /// <param name="device">Device information</param>
        /// <returns>Device information</returns>
        [HttpPut("{id}")]
        public async Task<DeviceRegistryApiModel> PutAsync(string id, [FromBody] DeviceRegistryApiModel device)
        {
            return new DeviceRegistryApiModel(await this.devices.CreateOrUpdateAsync(device.ToServiceModel()));
        }

        /// <summary>Remove device</summary>
        /// <param name="id">Device Id</param>
        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await this.devices.DeleteAsync(id);
        }
    }
}
