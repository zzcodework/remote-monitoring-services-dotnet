// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public DeploymentServiceModel(Configuration deployment)
        {
            if (string.IsNullOrEmpty(deployment.Id))
            {
                throw new ArgumentException($"Invalid deploymentId provided {deployment.Id}");
            }

            this.VerifyConfigurationLabel(deployment, DEPLOYMENT_NAME_LABEL);
            this.VerifyConfigurationLabel(deployment, DEPLOYMENT_GROUP_ID_LABEL);
            this.VerifyConfigurationLabel(deployment, RM_CREATED_LABEL);

            this.Id = deployment.Id;
            this.Name = deployment.Labels[DEPLOYMENT_NAME_LABEL];
            this.CreatedDateTimeUtc = deployment.CreatedTimeUtc;
            this.DeviceGroupId = deployment.Labels[DEPLOYMENT_GROUP_ID_LABEL];

            if (deployment.Labels.ContainsKey(DEPLOYMENT_GROUP_NAME_LABEL))
            {
                this.DeviceGroupName = deployment.Labels[DEPLOYMENT_GROUP_NAME_LABEL];
            }

            if (deployment.Labels.ContainsKey(DEPLOYMENT_PACKAGE_NAME_LABEL))
            {
                this.PackageName = deployment.Labels[DEPLOYMENT_PACKAGE_NAME_LABEL];
            }

            this.Priority = deployment.Priority;

            if (deployment.Labels.Values.Contains(DeploymentType.EdgeManifest.ToString()))
            {
                this.Type = DeploymentType.EdgeManifest;
            }
            else if (deployment.Labels.Values.Contains(DeploymentType.DeviceConfiguration.ToString()))
            {
                this.Type = DeploymentType.DeviceConfiguration;
            }

            this.DeploymentMetrics = new DeploymentMetrics(deployment.SystemMetrics, deployment.Metrics);
        }

        private void VerifyConfigurationLabel(Configuration deployment, string labelName)
        {
            if (!deployment.Labels.ContainsKey(labelName))
            {
                throw new ArgumentException($"Configuration is missing necessary label {labelName}");
            }
        }
    }

    public enum DeploymentType {
        EdgeManifest,
        DeviceConfiguration
    }
}
