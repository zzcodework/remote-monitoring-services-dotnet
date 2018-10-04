// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
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

        [JsonProperty(PropertyName = "DeviceGroupName")]
        public string DeviceGroupName { get; set; }

        [JsonProperty(PropertyName = "DeviceGroupQuery")]
        public string DeviceGroupQuery { get; set; }

        [JsonProperty(PropertyName = "PackageContent")]
        public string PackageContent { get; set; }

        [JsonProperty(PropertyName = "PackageName")]
        public string PackageName { get; set; }

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
            this.DeviceGroupName = serviceModel.DeviceGroupName;
            this.DeviceGroupQuery = serviceModel.DeviceGroupQuery;
            this.Name = serviceModel.Name;
            this.PackageContent = serviceModel.PackageContent;
            this.PackageName = serviceModel.PackageName;
            this.Priority = serviceModel.Priority;
            this.Type = serviceModel.Type;
            this.Metrics = new DeploymentMetricsApiModel(serviceModel.DeploymentMetrics)
            {
                DeviceStatuses = serviceModel.DeploymentMetrics?.DeviceStatuses
            };
        }

        public DeploymentApiModel(DeploymentServiceModel serviceModel,
                                  IDictionary<string, DeploymentStatus> deviceStatuses)
        {
            this.CreatedDateTimeUtc = serviceModel.CreatedDateTimeUtc;
            this.DeploymentId = serviceModel.Id;
            this.DeviceGroupId = serviceModel.DeviceGroupId;
            this.Name = serviceModel.Name;
            this.Priority = serviceModel.Priority;
            this.Type = serviceModel.Type;
            this.Metrics = new DeploymentMetricsApiModel(serviceModel.DeploymentMetrics)
            {
                DeviceStatuses = deviceStatuses
            };
        }

        public DeploymentServiceModel ToServiceModel()
        {
            return new DeploymentServiceModel() {
                DeviceGroupId = this.DeviceGroupId,
                DeviceGroupName = this.DeviceGroupName,
                DeviceGroupQuery = this.DeviceGroupQuery,
                Name = this.Name,
                PackageContent = this.PackageContent,
                PackageName = this.PackageName,
                Priority = this.Priority,
                Type = this.Type
            };
        }
    }
}
