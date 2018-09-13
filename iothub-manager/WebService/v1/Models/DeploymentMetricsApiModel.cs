// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
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

        [JsonProperty(PropertyName = "AppliedCount")]
        public long AppliedCount { get; set; }
        
        [JsonProperty(PropertyName = "FailedCount")]
        public long FailedCount { get; set; }

        [JsonProperty(PropertyName = "SucceededCount")]
        public long SucceededCount { get; set; }

        [JsonProperty(PropertyName = "TargetedCount")]
        public long TargetedCount { get; set; }

        public DeploymentMetricsApiModel()
        {
        }

        public DeploymentMetricsApiModel(DeploymentMetrics metricsServiceModel)
        {
            if (metricsServiceModel == null) return;

            var metrics = metricsServiceModel.Metrics;
            this.AppliedCount = metrics.TryGetValue(APPLIED_METRICS_KEY, out var value) ? value : 0;
            this.TargetedCount = metrics.TryGetValue(TARGETED_METRICS_KEY, out value) ? value : 0;
            this.SucceededCount = metrics.TryGetValue(SUCCESSFUL_METRICS_KEY, out value) ? value : 0;
            this.FailedCount = metrics.TryGetValue(FAILED_METRICS_KEY, out value) ? value : 0;
        }
    }
}
