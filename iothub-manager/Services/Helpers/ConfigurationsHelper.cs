// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    public static class ConfigurationsHelper
    {
        private const string DEVICE_GROUP_ID_PARAM = "deviceGroupId";
        private const string DEVICE_GROUP_QUERY_PARAM = "deviceGroupQuery";
        private const string NAME_PARAM = "name";
        private const string PACKAGE_CONTENT_PARAM = "packageContent";
        private const string PRIORITY_PARAM = "priority";

        public const string PACKAGE_TYPE_LABEL = "Type";
        public const string CONFIG_TYPE_LABEL = "ConfigType";
        public const string DEPLOYMENT_NAME_LABEL = "Name";
        public const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        public const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        public const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        public const string RM_CREATED_LABEL = "RMDeployment";

        public static Configuration ToHubConfiguration(DeploymentServiceModel model)
        {
            var packageConfiguration = JsonConvert.DeserializeObject<Configuration>(model.PackageContent);

            if (model.Type.Equals(PackageType.EdgeManifest) &&
                packageConfiguration.Content?.DeviceContent != null)
            {
                throw new InvalidInputException("Deployment type does not match with package contents.");
            }
            else if (model.Type.Equals(PackageType.DeviceConfiguration) &&
                packageConfiguration.Content?.ModulesContent != null)
            {
                throw new InvalidInputException("Deployment type does not match with package contents.");
            }

            var deploymentId = Guid.NewGuid().ToString().ToLower();
            var configuration = new Configuration(deploymentId);
            configuration.Content = packageConfiguration.Content;

            var targetCondition = QueryConditionTranslator.ToQueryString(model.DeviceGroupQuery);
            configuration.TargetCondition = string.IsNullOrEmpty(targetCondition) ? "*" : targetCondition;
            configuration.Priority = model.Priority;
            configuration.ETag = string.Empty;

            if (configuration.Labels == null)
            {
                configuration.Labels = new Dictionary<string, string>();
            }

            // Required labels
            configuration.Labels.Add(PACKAGE_TYPE_LABEL, model.Type.ToString());
            configuration.Labels.Add(CONFIG_TYPE_LABEL, model.ConfigType);
            configuration.Labels.Add(DEPLOYMENT_NAME_LABEL, model.Name);
            configuration.Labels.Add(DEPLOYMENT_GROUP_ID_LABEL, model.DeviceGroupId);
            configuration.Labels.Add(RM_CREATED_LABEL, bool.TrueString);

            var customMetrics = packageConfiguration.Metrics?.Queries;
            if (customMetrics != null)
            {
                configuration.Metrics.Queries = customMetrics;
            }

            // Add optional labels
            if (model.DeviceGroupName != null)
            {
                configuration.Labels.Add(DEPLOYMENT_GROUP_NAME_LABEL, model.DeviceGroupName);
            }
            if (model.PackageName != null)
            {
                configuration.Labels.Add(DEPLOYMENT_PACKAGE_NAME_LABEL, model.PackageName);
            }

            return configuration;
        }

        public static Boolean IsEdgeDeployment(Configuration deployment)
        {
            deployment.Labels.TryGetValue(PACKAGE_TYPE_LABEL, out var type);

            if ( type.Equals(PackageType.EdgeManifest.ToString()))
            {
                return true;
            }

            return false;
        }
    }
}
