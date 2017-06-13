// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDevices
    {
        Task<IEnumerable<DeviceServiceModel>> GetListAsync();
        Task<DeviceServiceModel> GetAsync(string id);
        Task<DeviceServiceModel> CreateAsync(DeviceServiceModel toServiceModel);
    }

    public class Devices : IDevices
    {
        private const int MaxGetList = 1000;
        private readonly IServicesConfig config;
        private readonly IDeviceTwins deviceTwins;
        private readonly Lazy<RegistryManager> registry;

        public Devices(IServicesConfig config)
        {
            this.config = config;
            this.deviceTwins = new DeviceTwins(config);
            this.registry = new Lazy<RegistryManager>(this.CreateRegistry);
        }

        public async Task<IEnumerable<DeviceServiceModel>> GetListAsync()
        {
            var devices = await this.registry.Value.GetDevicesAsync(MaxGetList);

            return devices.Select(azureDevice => new DeviceServiceModel(azureDevice, (Twin)null)).ToList();
        }

        public async Task<DeviceServiceModel> GetAsync(string id)
        {
            var remoteDevice = await this.registry.Value.GetDeviceAsync(id);

            return remoteDevice == null ? null : new DeviceServiceModel(remoteDevice, await this.deviceTwins.GetAsync(id));
        }

        public async Task<DeviceServiceModel> CreateAsync(DeviceServiceModel device)
        {
            var azureDevice = await this.registry.Value.AddDeviceAsync(device.ToAzureModel());

            // TODO: do we need to fetch the twin and return it?
            if (device.Twin == null) return new DeviceServiceModel(azureDevice, (Twin)null);

            // TODO: do we need to fetch the twin Etag first? - how will concurrency work?
            var azureTwin = await this.registry.Value.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.Etag);
            return new DeviceServiceModel(azureDevice, azureTwin);
        }

        private RegistryManager CreateRegistry()
        {
            // Note: this will cause an exception if the connection string
            // is not available or badly formatted, e.g. during a test.
            return RegistryManager.CreateFromConnectionString(this.config.HubConnString);
        }
    }
}
