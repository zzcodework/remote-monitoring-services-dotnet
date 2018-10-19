// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceModel
    {
        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        private const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        private const string RM_CREATED_LABEL = "RMDeployment";

        public DateTime CreatedDateTimeUtc { get; set; }
        public string Id { get; set; }
        public DeploymentMetrics DeploymentMetrics { get; set; }
        public string DeviceGroupId { get; set; }
        public string DeviceGroupName { get; set; }
        public string DeviceGroupQuery { get; set; }
        public string Name {get; set; }
        public string PackageContent { get; set; }
        public string PackageName { get; set; }
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

            this.VerifyConfigurationLabel(config, DEPLOYMENT_NAME_LABEL);
            this.VerifyConfigurationLabel(config, DEPLOYMENT_GROUP_ID_LABEL);
            this.VerifyConfigurationLabel(config, RM_CREATED_LABEL);

            this.Id = config.Id;
            this.Name = config.Labels[DEPLOYMENT_NAME_LABEL];
            this.CreatedDateTimeUtc = config.CreatedTimeUtc;
            this.DeviceGroupId = config.Labels[DEPLOYMENT_GROUP_ID_LABEL];

            if (config.Labels.ContainsKey(DEPLOYMENT_GROUP_NAME_LABEL))
            {
                this.DeviceGroupName = config.Labels[DEPLOYMENT_GROUP_NAME_LABEL];
            }

            if (config.Labels.ContainsKey(DEPLOYMENT_PACKAGE_NAME_LABEL))
            {
                this.PackageName = config.Labels[DEPLOYMENT_PACKAGE_NAME_LABEL];
            }

            this.Priority = config.Priority;
            this.Type = DeploymentType.EdgeManifest;

            this.DeploymentMetrics = new DeploymentMetrics(config.SystemMetrics, config.Metrics);
        }

        private void VerifyConfigurationLabel(Configuration config, string labelName)
        {
            if (!config.Labels.ContainsKey(labelName))
            {
                throw new ArgumentException($"Configuration is missing necessary label {labelName}");
            }
        }
    }

    public enum DeploymentType {
        EdgeManifest
    }
}
