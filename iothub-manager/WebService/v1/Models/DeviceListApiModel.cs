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
            { "$type", "DeviceList;" + Version.Name },
            { "$uri", "/" + Version.Name + "/devices" }
        };

        public List<DeviceRegistryModel> Items { get; set; }

        public DeviceListApiModel(IEnumerable<DeviceServiceModel> devices)
        {
            this.Items = new List<DeviceRegistryModel>();
            foreach (var d in devices) this.Items.Add(new DeviceRegistryModel(d));
        }
    }
}
