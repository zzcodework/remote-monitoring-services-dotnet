// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDevices
    {
        Task<DeviceServiceListModel> GetListAsync(string continueousToken);
        Task<DeviceServiceModel> GetAsync(string id);
        Task<DeviceServiceModel> CreateAsync(DeviceServiceModel toServiceModel);
        Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel toServiceModel);
        Task DeleteAsync(string id);
    }

    public class Devices : IDevices
    {
        private const int MaxGetList = 1000;

        private readonly RegistryManager registry;
        private readonly string ioTHubHostName;

        public Devices(IServicesConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // make sure the exception could throw earlier if format is not valid
            this.registry = RegistryManager.CreateFromConnectionString(config.HubConnString);
            this.ioTHubHostName = IotHubConnectionStringBuilder.Create(config.HubConnString).HostName;
        }

        public async Task<DeviceServiceListModel> GetListAsync(string continueousToken)
        {
            // normally we need deviceTwins for all devices to show device list
            var devices = await this.registry.GetDevicesAsync(MaxGetList);

            // WORKAROUND: when we have the query API supported, we can replace to use query API
            var twinTasks = new List<Task<Twin>>();
            foreach (var item in devices)
            {
                twinTasks.Add(this.registry.GetTwinAsync(item.Id));
            }

            await Task.WhenAll(twinTasks.ToArray());

            return new DeviceServiceListModel(devices.Select(azureDevice => 
                                                new DeviceServiceModel(azureDevice, twinTasks.Select(t => t.Result).Single(t => t.DeviceId == azureDevice.Id), 
                                                this.ioTHubHostName)), 
                                               null);
        }

        public async Task<DeviceServiceModel> GetAsync(string id)
        {
            var device = this.registry.GetDeviceAsync(id);
            var twin = this.registry.GetTwinAsync(id);

            await Task.WhenAll(device, twin);

            if (device.Result == null)
            {
                throw new ResourceNotFoundException("The device doesn't exist.");
            }

            return new DeviceServiceModel(device.Result, twin.Result, this.ioTHubHostName);
        }

        public async Task<DeviceServiceModel> CreateAsync(DeviceServiceModel device)
        {
            var azureDevice = await this.registry.AddDeviceAsync(device.ToAzureModel());

            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await this.registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await this.registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.Etag);
            }
            
            return new DeviceServiceModel(azureDevice, azureTwin, this.ioTHubHostName);
        }

        /// <summary>
        /// We only support update twin
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel device)
        {
            // validate device module
            var azureDevice = await this.registry.GetDeviceAsync(device.Id);
            if( azureDevice == null)
            {
                azureDevice = await this.registry.AddDeviceAsync(device.ToAzureModel());
            }

            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await this.registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await this.registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.Etag);
            }

            return new DeviceServiceModel(azureDevice, azureTwin, this.ioTHubHostName);
        }

        public async Task DeleteAsync(string id)
        {
            await this.registry.RemoveDeviceAsync(id);
        }
    }
}
