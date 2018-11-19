// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeploymentMetricsApiModel
    {
        private const string APPLIED_METRICS_KEY = "appliedCount";
        private const string TARGETED_METRICS_KEY = "targetedCount";
        private const string SUCCESSFUL_METRICS_KEY = "successfullCount";
        private const string FAILED_METRICS_KEY = "failedCount";
        private const string PENDING_METRICS_KEY = "pendingCount";

        [JsonProperty(PropertyName = "SystemMetrics")]
        public IDictionary<string, long> SystemMetrics { get; set; }
      
        [JsonProperty(PropertyName = "CustomMetrics")]
        public IDictionary<string, long> CustomMetrics { get; set; }

        [JsonProperty(PropertyName = "DeviceStatuses")]
        public IDictionary<string, DeploymentStatus> DeviceStatuses { get; set; }

        public DeploymentMetricsApiModel()
        {}

        public DeploymentMetricsApiModel(DeploymentMetrics metricsServiceModel)
        {
            this.SystemMetrics = new Dictionary<string, long>();

            this.SystemMetrics[APPLIED_METRICS_KEY] = 0;
            this.SystemMetrics[TARGETED_METRICS_KEY] = 0;
            this.SystemMetrics[SUCCESSFUL_METRICS_KEY] = 0;
            this.SystemMetrics[FAILED_METRICS_KEY] = 0;
            this.SystemMetrics[PENDING_METRICS_KEY] = 0;

            if (metricsServiceModel == null) return;

            this.CustomMetrics = metricsServiceModel.CustomMetrics;
            this.SystemMetrics = metricsServiceModel.SystemMetrics;
            this.DeviceStatuses = metricsServiceModel.DeviceStatuses;

            if (metricsServiceModel.DeviceMetrics != null)
            {
                SystemMetrics[SUCCESSFUL_METRICS_KEY] = metricsServiceModel.DeviceMetrics[DeploymentStatus.Successful];
                SystemMetrics[FAILED_METRICS_KEY] = metricsServiceModel.DeviceMetrics[DeploymentStatus.Failed];
                SystemMetrics[PENDING_METRICS_KEY] = metricsServiceModel.DeviceMetrics[DeploymentStatus.Pending];
            }
        }
    }
}
