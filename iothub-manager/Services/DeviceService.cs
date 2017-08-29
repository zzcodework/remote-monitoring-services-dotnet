// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeviceService
    {
        Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter);
    }

    public class DeviceService : IDeviceService
    {
        private ServiceClient serviceClient;

        public DeviceService(IServicesConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.HubConnString, (conn) =>
            {
                this.serviceClient = ServiceClient.CreateFromConnectionString(conn);
            });
        }

        public async Task<MethodResultServiceModel> InvokeDeviceMethodAsync(string deviceId, MethodParameterServiceModel parameter)
        {
            var result = await this.serviceClient.InvokeDeviceMethodAsync(deviceId, parameter.ToAzureModel());
            return new MethodResultServiceModel(result);
        }
    }
}
