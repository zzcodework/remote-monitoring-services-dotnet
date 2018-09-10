// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeploymentApiModel
    {
        [JsonProperty(PropertyName = "Id")]
        public string DeploymentId { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "CreatedDateTimeUtc")]
        public DateTime CreatedDateTimeUtc { get; set; }

        [JsonProperty(PropertyName = "DeviceGroupId")]
        public string DeviceGroupId { get; set; }
        
        [JsonProperty(PropertyName = "PackageId")]
        public string PackageId { get; set; }
        
        [JsonProperty(PropertyName = "Priority")]
        public int Priority { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "Type")]
        public DeploymentType Type { get; set; }

        [JsonProperty(PropertyName = "Metrics", NullValueHandling = NullValueHandling.Ignore)]
        public DeploymentMetricsApiModel Metrics { get; set; }

        public DeploymentApiModel()
        {
        }

        public DeploymentApiModel(DeploymentServiceModel serviceModel)
        {
            this.CreatedDateTimeUtc = serviceModel.CreatedDateTimeUtc;
            this.DeploymentId = serviceModel.Id;
            this.DeviceGroupId = serviceModel.DeviceGroupId;
            this.Name = serviceModel.Name;
            this.PackageId = serviceModel.PackageId;
            this.Priority = serviceModel.Priority;
            this.Type = serviceModel.Type;
            this.Metrics = new DeploymentMetricsApiModel(serviceModel.DeploymentMetrics);
        }

        public DeploymentServiceModel ToServiceModel()
        {
            return new DeploymentServiceModel() {
                 DeviceGroupId = this.DeviceGroupId,
                 Name = this.Name,
                 PackageId = this.PackageId,
                 Priority = this.Priority,
                 Type = this.Type
            };
        }
    }
}
