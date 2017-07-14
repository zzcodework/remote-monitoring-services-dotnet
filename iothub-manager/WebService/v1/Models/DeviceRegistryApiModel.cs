// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeviceRegistryApiModel
    {
        [JsonProperty(PropertyName = "Etag")]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "C2DMessageCount")]
        public int C2DMessageCount { get; set; }

        [JsonProperty(PropertyName = "LastActivity")]
        public DateTime LastActivity { get; set; }

        [JsonProperty(PropertyName = "Connected")]
        public bool Connected { get; set; }

        [JsonProperty(PropertyName = "Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "LastStatusUpdated")]
        public DateTime LastStatusUpdated { get; set; }

        [JsonProperty(PropertyName = "IoTHubHostName")]
        public string IoTHubHostName { get; set; }

        [JsonProperty(PropertyName = "AuthPrimaryKey")]
        public string AuthPrimaryKey { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "Device;" + Version.Number },
            { "$uri", "/" + Version.Path + "/devices/" + this.Id },
            { "$twin_uri", "/" + Version.Path + "/devices/" + this.Id + "/twin" }
        };

        [JsonProperty(PropertyName = "Twin", NullValueHandling = NullValueHandling.Ignore)]
        public DeviceTwinApiModel Twin { get; set; }

        public DeviceRegistryApiModel()
        {
        }

        public DeviceRegistryApiModel(DeviceServiceModel device)
        {
            if (device == null) return;

            this.Id = device.Id;
            this.Etag = device.Etag;
            this.C2DMessageCount = device.C2DMessageCount;
            this.LastActivity = device.LastActivity;
            this.Connected = device.Connected;
            this.Enabled = device.Enabled;
            this.LastStatusUpdated = device.LastStatusUpdated;
            this.Twin = new DeviceTwinApiModel(device.Id,device.Twin);
            this.IoTHubHostName = device.IoTHubHostName;
            this.AuthPrimaryKey = device.AuthPrimaryKey;
        }

        public DeviceServiceModel ToServiceModel()
        {
            return new DeviceServiceModel
            (
                etag: this.Etag,
                id: this.Id,
                c2DMessageCount: this.C2DMessageCount,
                lastActivity: this.LastActivity,
                connected: this.Connected,
                enabled: this.Enabled,
                lastStatusUpdated: this.LastStatusUpdated,
                twin: this.Twin?.ToServiceModel(),
                ioTHubHostName: this.IoTHubHostName,
                primaryKey: this.AuthPrimaryKey
            );
        }
    }
}
