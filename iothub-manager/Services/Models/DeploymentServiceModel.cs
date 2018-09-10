// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceModel
    {
        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_PACKAGE_ID_LABEL = "PackageId";

        public DateTime CreatedDateTimeUtc { get; set; }
        public string Id { get; set; }
        public DeploymentMetrics DeploymentMetrics { get; set; }
        public string DeviceGroupId { get; set; }
        public string Name {get; set; }
        public string PackageId { get; set; }
        public int Priority { get; set; }
        public DeploymentType Type { get; set; }

        public DeploymentServiceModel()
        {
        }

        public DeploymentServiceModel(Configuration config)
        {
            if (string.IsNullOrEmpty(config.Id))
            {
                throw new ArgumentException($"Invalid deploymentId provided {config.Id}");
            }

            if (!config.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL))
            {
                throw new ArgumentException($"Configuration is missing necessary label {DEPLOYMENT_GROUP_ID_LABEL}");
            }

            if (!config.Labels.ContainsKey(DEPLOYMENT_PACKAGE_ID_LABEL))
            {
                throw new ArgumentException($"Configuration is missing necessary label {DEPLOYMENT_PACKAGE_ID_LABEL}");
            }

            if (!config.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL))
            {
                throw new ArgumentException($"Configuration is missing necessary label {DEPLOYMENT_NAME_LABEL}");
            }

            this.Id = config.Id;
            this.Name = config.Labels[DEPLOYMENT_NAME_LABEL];
            this.CreatedDateTimeUtc = config.CreatedTimeUtc;
            this.DeviceGroupId = config.Labels[DEPLOYMENT_GROUP_ID_LABEL];
            this.PackageId = config.Labels[DEPLOYMENT_PACKAGE_ID_LABEL];
            this.Priority = config.Priority;
            this.Type = DeploymentType.EdgeManifest;

            this.DeploymentMetrics = new DeploymentMetrics(config.SystemMetrics, config.Metrics);
        }
    }

    public enum DeploymentType {
        EdgeManifest
    }
}
