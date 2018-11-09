// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers.DeviceStatusHelper;
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
        Pending, Successful, Failed, UnKnown
    }

    public class Deployments : IDeployments
    {
        private const int MAX_DEPLOYMENTS = 20;

        private const string EDGE_MANIFEST_SCHEMA = "schemaVersion";

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
            ConfigurtionsHelper.Validate(model);//throws exception if model is not valid.

            var configuration = ConfigurtionsHelper.ToConfiguration(model);

            // TODO: Add specific exception handling when exception types are exposed
            // https://github.com/Azure/azure-iot-sdk-csharp/issues/649
            return new DeploymentServiceModel(await this.registry.AddConfigurationAsync(configuration));
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

            IDictionary<String, DeploymentStatus> deviceStatuses = null;

            if (includeDeviceStatus)
            {
                deviceStatuses = DeviceStatusFactory.GetDeviceStatusApi(deployment,
                                                                      this.registry,
                                                                      this.log)
                                                                      .GetDeviceStatuses();
            }

            return new DeploymentServiceModel(deployment)
            {
                DeploymentMetrics =
                {
                    DeviceStatuses = deviceStatuses
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

        private bool CheckIfDeploymentWasMadeByRM(Configuration conf)
        {
            return conf.Labels != null &&
                   conf.Labels.ContainsKey(ConfigurtionsHelper.RM_CREATED_LABEL) &&
                   bool.TryParse(conf.Labels[ConfigurtionsHelper.RM_CREATED_LABEL], out var res) && res;
        }
    }
}