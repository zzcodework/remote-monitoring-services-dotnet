// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeviceTwins
    {
        Task<IEnumerable<DeviceTwinServiceModel>> GetListAsync();

        Task<DeviceTwinServiceModel> GetAsync(string deviceId);
    }

    public class DeviceTwins : IDeviceTwins
    {
        // Max is 1000
        private const int PageSize = 1000;

        private readonly IServicesConfig config;
        private readonly Lazy<RegistryManager> registry;

        public DeviceTwins(IServicesConfig config)
        {
            this.config = config;
            this.registry = new Lazy<RegistryManager>(this.CreateRegistry);
        }

        public async Task<IEnumerable<DeviceTwinServiceModel>> GetListAsync()
        {
            var result = new List<DeviceTwinServiceModel>();
            var query = this.registry.Value.CreateQuery("SELECT * FROM devices", PageSize);
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                result.AddRange(page.Select(x => new DeviceTwinServiceModel(x)));
            }

            return result;
        }

        public async Task<DeviceTwinServiceModel> GetAsync(string id)
        {
            var twin = await this.registry.Value.GetTwinAsync(id);
            return twin == null ? null : new DeviceTwinServiceModel(twin);
        }

        private RegistryManager CreateRegistry()
        {
            // Note: this will cause an exception if the connection string
            // is not available or badly formatted, e.g. during a test.
            return RegistryManager.CreateFromConnectionString(this.config.HubConnString);
        }
    }
}
