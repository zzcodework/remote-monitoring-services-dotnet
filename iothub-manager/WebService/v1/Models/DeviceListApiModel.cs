// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeviceListApiModel
    {
        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "DeviceList;" + Version.Number },
            { "$uri", "/" + Version.Path + "/devices" }
        };

        public List<DeviceRegistryApiModel> Items { get; set; }

        public DeviceListApiModel(IEnumerable<DeviceServiceModel> devices)
        {
            this.Items = new List<DeviceRegistryApiModel>();
            foreach (var d in devices) this.Items.Add(new DeviceRegistryApiModel(d));
        }
    }
}
