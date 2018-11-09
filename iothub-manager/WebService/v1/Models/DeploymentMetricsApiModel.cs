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
        private const string SUCCESSFUL_METRICS_KEY = "reportedSuccessfulCount";
        private const string FAILED_METRICS_KEY = "reportedFailedCount";

        [JsonProperty(PropertyName = "TargetedCount")]
        public long TargetedCount { get; set; }

        [JsonProperty(PropertyName = "AppliedCount")]
        public long AppliedCount { get; set; }

        [JsonProperty(PropertyName = "CustomMetrics")]
        public IDictionary<string, long> CustomMetrics { get; set; }

        [JsonProperty(PropertyName = "DeviceStatuses")]
        public IDictionary<string, DeploymentStatus> DeviceStatuses { get; set; }

        public DeploymentMetricsApiModel()
        {
        }

        public DeploymentMetricsApiModel(DeploymentMetrics metricsServiceModel)
        {
            if (metricsServiceModel == null) return;

            var metrics = metricsServiceModel.Metrics;
            this.AppliedCount = metrics.TryGetValue(APPLIED_METRICS_KEY, out var value) ? value : 0;
            this.TargetedCount = metrics.TryGetValue(TARGETED_METRICS_KEY, out value) ? value : 0;

            foreach (var metric in metrics)
            {
                if (!(metric.Key.Equals(APPLIED_METRICS_KEY) || metric.Key.Equals(TARGETED_METRICS_KEY)))
                {
                    this.CustomMetrics.Add(metric);
                }
            }
        }
    }
}
