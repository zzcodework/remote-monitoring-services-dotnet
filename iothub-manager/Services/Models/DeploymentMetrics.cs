// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    /// <summary>
    /// Statistics exposed by configuration queries
    /// </summary>
    public class DeploymentMetrics
    {
        // private const string APPLIED_METRICS_KEY = "appliedCount";
        // private const string TARGETED_METRICS_KEY = "targetedCount";
        // private const string SUCCESSFUL_METRICS_KEY = "reportedSuccessfulCount";
        // private const string FAILED_METRICS_KEY = "reportedFailedCount";

        // public long AppliedCount { get; set; }
        
        // public long FailedCount { get; set; }

        // public long SucceededCount { get; set; }

        // public long TargetedCount { get; set; }

        public IDictionary<string, long> metrics { get; set; }

        public DeploymentMetrics(ConfigurationMetrics systemMetrics, ConfigurationMetrics customMetrics)
        {
            this.metrics = new Dictionary<string, long>();

            // TODO: Cleaner way to copy to dictionary
            if (systemMetrics?.Results.Count > 0)
            {
                foreach (KeyValuePair<string, long> pair in systemMetrics.Results)
                {
                    this.metrics.Add(pair);
                }
            }

            if (customMetrics?.Results.Count > 0)
            {
                foreach (KeyValuePair<string, long> pair in customMetrics.Results)
                {
                    this.metrics.Add(pair);
                }
            }
        }
    }
}
