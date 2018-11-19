// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceModel
    {

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
        public string ConfigType { get; set; }

        public DeploymentServiceModel()
        {
        }

        public DeploymentServiceModel(Configuration deployment)
        {
            if (string.IsNullOrEmpty(deployment.Id))
            {
                throw new ArgumentException($"Invalid deploymentId provided {deployment.Id}");
            }

            this.VerifyConfigurationLabel(deployment, ConfigurationsHelper.DEPLOYMENT_NAME_LABEL);
            this.VerifyConfigurationLabel(deployment, ConfigurationsHelper.DEPLOYMENT_GROUP_ID_LABEL);
            this.VerifyConfigurationLabel(deployment, ConfigurationsHelper.RM_CREATED_LABEL);

            this.Id = deployment.Id;
            this.Name = deployment.Labels[ConfigurationsHelper.DEPLOYMENT_NAME_LABEL];
            this.CreatedDateTimeUtc = deployment.CreatedTimeUtc;
            this.DeviceGroupId = deployment.Labels[ConfigurationsHelper.DEPLOYMENT_GROUP_ID_LABEL];

            if (deployment.Labels.ContainsKey(ConfigurationsHelper.DEPLOYMENT_GROUP_NAME_LABEL))
            {
                this.DeviceGroupName = deployment.Labels[ConfigurationsHelper.DEPLOYMENT_GROUP_NAME_LABEL];
            }

            if (deployment.Labels.ContainsKey(ConfigurationsHelper.DEPLOYMENT_PACKAGE_NAME_LABEL))
            {
                this.PackageName = deployment.Labels[ConfigurationsHelper.DEPLOYMENT_PACKAGE_NAME_LABEL];
            }

            this.Priority = deployment.Priority;

            if (deployment.Labels.ContainsKey(ConfigurationsHelper.DEPLOYMENT_TYPE_LABEL) &&
                !(string.IsNullOrEmpty(deployment.Labels[ConfigurationsHelper.DEPLOYMENT_TYPE_LABEL])))
            {
                if (deployment.Labels.Values.Contains(DeploymentType.EdgeManifest.ToString()))
                {
                    this.Type = DeploymentType.EdgeManifest;
                }
                else if (deployment.Labels.Values.Contains(DeploymentType.DeviceConfiguration.ToString()))
                {
                    this.Type = DeploymentType.DeviceConfiguration;
                }
            }
            else
            {
                if (deployment.Content?.ModulesContent != null)
                {
                    this.Type = DeploymentType.EdgeManifest;
                }
                else if (deployment.Content?.DeviceContent != null)
                {
                    this.Type = DeploymentType.DeviceConfiguration;
                }
            }

            if (deployment.Labels.ContainsKey(ConfigurationsHelper.CONFIG_TYPE_LABEL))
            {
                this.ConfigType = deployment.Labels[ConfigurationsHelper.CONFIG_TYPE_LABEL];
            }
            else
            {
                this.ConfigType = DeploymentType.EdgeManifest.ToString();
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

    //Sync these variables with PackageType in Config 
    public enum DeploymentType {
        EdgeManifest,
        DeviceConfiguration
    }


}
