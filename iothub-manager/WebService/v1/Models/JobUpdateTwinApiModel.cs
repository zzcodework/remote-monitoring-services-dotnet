// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class JobUpdateTwinApiModel
    {
        public string Etag { get; set; }
        public string DeviceId { get; set; }
        public TwinPropertiesApiModel Properties { get; set; }
        public Dictionary<string, JToken> Tags { get; set; }
        public bool IsSimulated { get; set; }

        public JobUpdateTwinApiModel()
        {
            this.Tags = new Dictionary<string, JToken>();
            this.Properties = new TwinPropertiesApiModel();
        }

        public JobUpdateTwinApiModel(string deviceId, DeviceTwinServiceModel deviceTwin)
        {
            if (deviceTwin != null)
            {
                this.Etag = deviceTwin.Etag;
                this.DeviceId = deviceId;
                this.Properties = new TwinPropertiesApiModel(deviceTwin.DesiredProperties, deviceTwin.ReportedProperties);
                this.Tags = deviceTwin.Tags;
                this.IsSimulated = deviceTwin.IsSimulated;
            }
        }

        public DeviceTwinServiceModel ToServiceModel()
        {
            return new DeviceTwinServiceModel
            (
                etag: this.Etag,
                deviceId: this.DeviceId,
                desiredProperties: this.Properties.Desired,
                reportedProperties: this.Properties.Reported,
                tags: this.Tags,
                isSimulated: this.IsSimulated
            );
        }
    }
}
