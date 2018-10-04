// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeployments
    {
        Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model);
        Task<DeploymentServiceListModel> ListAsync();
        Task<DeploymentServiceModel> GetAsync(string id, bool includeDeviceStatus);
        Task DeleteAsync(string deploymentId);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeploymentStatus
    {
        Pending, Successful, Failed
    }

    public class Deployments : IDeployments
    {
        private const int MAX_DEPLOYMENTS = 20;

        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        private const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        private const string RM_CREATED_LABEL = "RMDeployment";

        private const string DEVICE_GROUP_ID_PARAM = "deviceGroupId";
        private const string DEVICE_GROUP_QUERY_PARAM = "deviceGroupQuery";
        private const string NAME_PARAM = "name";
        private const string PACKAGE_CONTENT_PARAM = "packageContent";
        private const string PRIORITY_PARAM = "priority";

        private const string DEVICE_ID_KEY = "DeviceId";
        private const string EDGE_MANIFEST_SCHEMA = "schemaVersion";

        private const string APPLIED_DEVICES_QUERY =
            "select deviceId from devices.modules where moduleId = '$edgeAgent'" + 
            " and configurations.[[{0}]].status = 'Applied'";

        private const string SUCCESSFUL_DEVICES_QUERY = APPLIED_DEVICES_QUERY + 
            " and properties.desired.$version = properties.reported.lastDesiredVersion" + 
            " and properties.reported.lastDesiredStatus.code = 200";

        private const string FAILED_DEVICES_QUERY = APPLIED_DEVICES_QUERY +
            " and properties.desired.$version = properties.reported.lastDesiredVersion" +
            " and properties.reported.lastDesiredStatus.code != 200";

        private RegistryManager registry;
        private string ioTHubHostName;
        private readonly ILogger log;

        public Deployments(
            IServicesConfig config,
            ILogger logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.IoTHubConnString, (conn) =>
            {
                this.registry = RegistryManager.CreateFromConnectionString(conn);
                this.ioTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
            });

            this.log = logger;
        }

        public Deployments(
            RegistryManager registry,
            string ioTHubHostName)
        {
            this.registry = registry;
            this.ioTHubHostName = ioTHubHostName;
        }

        /// <summary>
        /// Schedules a deployment of the provided package, to the given group.
        /// </summary>
        /// <returns>Scheduled deployment</returns>
        public async Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model)
        {
            if (string.IsNullOrEmpty(model.DeviceGroupId))
            {
                throw new ArgumentNullException(DEVICE_GROUP_ID_PARAM);
            }

            if (string.IsNullOrEmpty(model.DeviceGroupQuery))
            {
                throw new ArgumentNullException(DEVICE_GROUP_QUERY_PARAM);
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                throw new ArgumentNullException(NAME_PARAM);
            }

            if (string.IsNullOrEmpty(model.PackageContent))
            {
                throw new ArgumentNullException(PACKAGE_CONTENT_PARAM);
            }

            if (model.Priority < 0)
            {
                throw new ArgumentOutOfRangeException(PRIORITY_PARAM,
                    model.Priority,
                    "The priority provided should be 0 or greater");
            }

            var edgeConfiguration = this.CreateEdgeConfiguration(model);

            // TODO: Add specific exception handling when exception types are exposed
            // https://github.com/Azure/azure-iot-sdk-csharp/issues/649
            return new DeploymentServiceModel(await this.registry.AddConfigurationAsync(edgeConfiguration));
        }

        /// <summary>
        /// Retrieves all deployments that have been scheduled on the iothub.
        /// Only deployments which were created by RM will be returned.
        /// </summary>
        /// <returns>All scheduled deployments with RMDeployment label</returns>
        public async Task<DeploymentServiceListModel> ListAsync()
        {
            // TODO: Currently they only support 20 deployments
            var deployments = await this.registry.GetConfigurationsAsync(MAX_DEPLOYMENTS);

            if (deployments == null)
            {
                throw new ResourceNotFoundException($"No deployments found for {this.ioTHubHostName} hub.");
            }

            List<DeploymentServiceModel> serviceModelDeployments = 
                deployments.Where(this.CheckIfDeploymentWasMadeByRM)
                           .Select(config => new DeploymentServiceModel(config))
                           .OrderBy(conf => conf.Name)
                           .ToList();

            return new DeploymentServiceListModel(serviceModelDeployments);
        }

        /// <summary>
        /// Retrieve information on a single deployment given its id.
        /// If includeDeviceStatus is included additional queries are created to retrieve the status of
        /// the deployment per device.
        /// </summary>
        /// <returns>Deployment for the given id</returns>
        public async Task<DeploymentServiceModel> GetAsync(string deploymentId, bool includeDeviceStatus = false)
        {
            if (string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException(nameof(deploymentId));
            }

            var deployment = await this.registry.GetConfigurationAsync(deploymentId);
            if (deployment == null)
            {
                throw new ResourceNotFoundException($"Deployment with id {deploymentId} not found.");
            }

            if (!this.CheckIfDeploymentWasMadeByRM(deployment))
            {
                throw new ResourceNotSupportedException($"Deployment with id {deploymentId}" + @" was 
                                                        created externally and therefore not supported");
            }

            return new DeploymentServiceModel(deployment)
            {
                DeploymentMetrics =
                {
                    DeviceStatuses = includeDeviceStatus ? this.GetDeviceStatuses(deploymentId) : null
                }
            };
        }

        /// <summary>
        /// Delete a given deployment by id.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(string deploymentId)
        {
            if(string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException(nameof(deploymentId));
            }

            await this.registry.RemoveConfigurationAsync(deploymentId);
        }

        private Configuration CreateEdgeConfiguration(DeploymentServiceModel model)
        {
            var packageContent = model.PackageContent;
            var deploymentId = Guid.NewGuid().ToString().ToLower();
            var edgeConfiguration = new Configuration(deploymentId);

            // TODO: Remove workaround for .net sdk issue which doesn't handle null schemaVersion
            var schemaVersion = JToken.Parse(packageContent)[EDGE_MANIFEST_SCHEMA];
            if (schemaVersion == null)
            {
                var packageJson = JToken.Parse(packageContent);
                packageJson[EDGE_MANIFEST_SCHEMA] = "1.0";
                packageContent = packageJson.ToString();
            }
            var packageEdgeConfiguration = JsonConvert.DeserializeObject<Configuration>(packageContent);
            edgeConfiguration.Content = packageEdgeConfiguration.Content;

            edgeConfiguration.TargetCondition = QueryConditionTranslator.ToQueryString(model.DeviceGroupQuery);
            edgeConfiguration.Priority = model.Priority;
            edgeConfiguration.ETag = string.Empty;

            if(edgeConfiguration.Labels == null)
            {
                edgeConfiguration.Labels = new Dictionary<string, string>();
            }

            // Required labels
            edgeConfiguration.Labels.Add(DEPLOYMENT_NAME_LABEL, model.Name);
            edgeConfiguration.Labels.Add(DEPLOYMENT_GROUP_ID_LABEL, model.DeviceGroupId);
            edgeConfiguration.Labels.Add(RM_CREATED_LABEL, bool.TrueString);

            // Add optional labels
            if (model.DeviceGroupName != null)
            {
                edgeConfiguration.Labels.Add(DEPLOYMENT_GROUP_NAME_LABEL, model.DeviceGroupName);
            }
            if (model.PackageName != null)
            {
                edgeConfiguration.Labels.Add(DEPLOYMENT_PACKAGE_NAME_LABEL, model.PackageName);
            }

            return edgeConfiguration;
        }

        private bool CheckIfDeploymentWasMadeByRM(Configuration conf)
        {
            return conf.Labels != null &&
                   conf.Labels.ContainsKey(RM_CREATED_LABEL) &&
                   bool.TryParse(conf.Labels[RM_CREATED_LABEL], out var res) && res;
        }

        private IDictionary<string, DeploymentStatus> GetDeviceStatuses(string deploymentId)
        {
            var appliedDevices = this.GetDevicesInQuery(APPLIED_DEVICES_QUERY, deploymentId);
            var successfulDevices = this.GetDevicesInQuery(SUCCESSFUL_DEVICES_QUERY, deploymentId);
            var failedDevices = this.GetDevicesInQuery(FAILED_DEVICES_QUERY, deploymentId);

            var deviceWithStatus = new Dictionary<string, DeploymentStatus>();

            foreach (var successfulDevice in successfulDevices)
            {
                deviceWithStatus.Add(successfulDevice, DeploymentStatus.Successful);
            }

            foreach (var failedDevice in failedDevices)
            {
                deviceWithStatus.Add(failedDevice, DeploymentStatus.Failed);
            }

            foreach (var device in appliedDevices)
            {
                if (!successfulDevices.Contains(device) && !failedDevices.Contains(device))
                {
                    deviceWithStatus.Add(device, DeploymentStatus.Pending);
                }
            }

            return deviceWithStatus;
        }

        private HashSet<string> GetDevicesInQuery(string hubQuery, string deploymentId)
        {
            var query = string.Format(hubQuery, deploymentId);
            var queryResponse = this.registry.CreateQuery(query);
            var deviceIds = new HashSet<string>();

            try
            {
                while (queryResponse.HasMoreResults)
                {
                    // TODO: Add pagination with queryOptions
                    var resultSet = queryResponse.GetNextAsJsonAsync();
                    foreach (var result in resultSet.Result)
                    {
                        var deviceId = JToken.Parse(result)[DEVICE_ID_KEY];
                        deviceIds.Add(deviceId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting status of devices in query {query}", () => new { ex.Message });
            }

            return deviceIds;
        }
    }
}