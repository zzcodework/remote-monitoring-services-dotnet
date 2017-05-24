// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Microsoft.Web.Http;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [ApiVersion(Version.Number)]
    public class DevicesController : ApiController
    {
        private static readonly IConfig config = new Config();
        private readonly IDevices devices = new Devices(config.ServicesConfig);

        /// <summary>Get a list of devices</summary>
        /// <returns>List of devices</returns>
        public async Task<DeviceListApiModel> Get()
        {
            return new DeviceListApiModel(await this.devices.GetListAsync());
        }

        /// <summary>Get one device</summary>
        /// <param name="id">Device Id</param>
        /// <returns>Device information</returns>
        public async Task<DeviceRegistryApiModel> Get(string id)
        {
            return new DeviceRegistryApiModel(await this.devices.GetAsync(id));
        }

        /// <summary>Create one device</summary>
        /// <param name="device">Device information</param>
        /// <returns>Device information</returns>
        public async Task<DeviceRegistryApiModel> Post(DeviceRegistryApiModel device)
        {
            return new DeviceRegistryApiModel(await this.devices.CreateAsync(device.ToServiceModel()));
        }
    }
}
