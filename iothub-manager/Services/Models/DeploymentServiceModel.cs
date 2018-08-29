// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceModel
    {
        private const string DEPLOYMENT_NAME_KEY = "Name";
        private const string DEPLOYMENT_ID_SEPARATOR = "--";
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
            if (!config.Id.Contains(DEPLOYMENT_ID_SEPARATOR))
            {
                throw new ArgumentException($"Invalid deploymentId provided {config.Id}");
            }

            var groupAndPkgIds = config.Id.Split(new [] { DEPLOYMENT_ID_SEPARATOR }, 
                                                 StringSplitOptions.None);
            if(config.Labels?.Count > 0)
            {
                this.Name = config.Labels[DEPLOYMENT_NAME_KEY];
            }

            this.CreatedDateTimeUtc = config.CreatedTimeUtc;
            this.DeviceGroupId = groupAndPkgIds[0];
            this.Id = config.Id;
            this.PackageId = groupAndPkgIds[1];
            this.Priority = config.Priority;
            this.Type = DeploymentType.EdgeManifest;

            this.DeploymentMetrics = new DeploymentMetrics(config.SystemMetrics, config.Metrics);
        }
    }

    public enum DeploymentType {
        EdgeManifest
    }
}
