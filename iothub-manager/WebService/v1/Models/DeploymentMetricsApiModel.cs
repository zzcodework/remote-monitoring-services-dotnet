// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public DeploymentMetricsApiModel(DeploymentMetrics serviceModel)
        {
            IDictionary<string, long> metrics = serviceModel.metrics;
            this.AppliedCount = this.GetMetricsValueOrDefault(metrics, APPLIED_METRICS_KEY);
            this.TargetedCount = this.GetMetricsValueOrDefault(metrics, TARGETED_METRICS_KEY);
            this.SucceededCount = this.GetMetricsValueOrDefault(metrics, SUCCESSFUL_METRICS_KEY);
            this.FailedCount = this.GetMetricsValueOrDefault(metrics, FAILED_METRICS_KEY);
        }

        private long GetMetricsValueOrDefault(IDictionary<string, long> metrics, string key)
        {
            long value;
            return metrics.TryGetValue(key, out value) ? value : 0;
        }
    }
}
