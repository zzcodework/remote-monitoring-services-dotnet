// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeployments
    {
        Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model);
        Task<DeploymentServiceListModel> GetAsync();
        Task<DeploymentServiceModel> GetAsync(string id);
        Task DeleteAsync(string deploymentId);
    }

    public class Deployments : IDeployments
    {
        private const int MAX_DEPLOYMENTS = 20;
        private const string DEPLOYMENT_NAME_KEY = "Name";
        private RegistryManager registry;
        private string ioTHubHostName;
        private IDeviceGroupsClient deviceGroupsClient;
        private IPackageManagementClient packageClient;

        public Deployments(
            IServicesConfig config,
            IDeviceGroupsClient deviceGroupsClient,
            IPackageManagementClient packageClient)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.IoTHubConnString, (conn) =>
            {
                this.registry = RegistryManager.CreateFromConnectionString(conn);
                this.ioTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
            });

            this.deviceGroupsClient = deviceGroupsClient;
            this.packageClient = packageClient;
        }

        /// <summary>
        /// Schedules a deployment of the provided package, to the given group.
        /// </summary>
        /// <returns>Scheduled deployment</returns>
        public async Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model)
        {
            if(string.IsNullOrEmpty(model.DeviceGroupId))
            {
                throw new ArgumentNullException("deviceGroupId");
            }

            if(string.IsNullOrEmpty(model.PackageId))
            {
                throw new ArgumentNullException("packageId");
            }

            if (model.Priority < 0)
            {
                throw new ArgumentOutOfRangeException("priority", 
                                                      model.Priority,
                                                      "The priority provided should be 0 or greater");
            }


            try{
                await this.packageClient.GetPackageAsync(model.PackageId);
            } catch (Exception ex) {
                Console.WriteLine("failing: " + ex);
            }

            var getDeviceGroupTask = this.deviceGroupsClient.GetDeviceGroupsAsync(model.DeviceGroupId);
            var package = await this.packageClient.GetPackageAsync(model.PackageId);
            var deviceGroup = await getDeviceGroupTask;

            var edgeConfiguration = this.CreateEdgeConfiguration(deviceGroup, package, model.Priority, model.Name);
            return new DeploymentServiceModel(await this.registry.AddConfigurationAsync(edgeConfiguration));
        }

        /// <summary>
        /// Retrieves all deployments that have been scheduled on the iothub.
        /// </summary>
        /// <returns>All scheduled deployments</returns>
        public async Task<DeploymentServiceListModel> GetAsync()
        {
            // TODO: Currently they only support 20 deployments
            IEnumerable<Configuration> deployments = await this.registry.GetConfigurationsAsync(MAX_DEPLOYMENTS);

            List<DeploymentServiceModel> serviceModelDeployments = 
                    deployments.AsParallel().Select(config => new DeploymentServiceModel(config))
                                                .OrderBy(conf => conf.Name)
                                                .ToList();

            return new DeploymentServiceListModel(serviceModelDeployments);
        }

        /// <summary>
        /// Retrieve information on a single deployment given its id.
        /// </summary>
        /// <returns>Deployment for the given id</returns>
        public async Task<DeploymentServiceModel> GetAsync(string deploymentId)
        {
            if(string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException("deploymentId");
            }

            Configuration deployment = await this.registry.GetConfigurationAsync(deploymentId);
            if(deployment == null)
            {
                throw new ResourceNotFoundException($"Deployment with id {deploymentId} not found.");
            }
            
            return new DeploymentServiceModel(deployment);
        }

        /// <summary>
        /// Delete a given deployment by id.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(string deploymentId)
        {
            if(string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException("deploymentId");
            }

            await this.registry.RemoveConfigurationAsync(deploymentId);
        }

        private Configuration CreateEdgeConfiguration(DeviceGroupApiModel deviceGroup, 
                                                      PackageApiModel package, 
                                                      int priority,
                                                      string name)
        {
            // Deployment IDs must be lowercase
            var deploymentId = $"{deviceGroup.Id}--{package.Id}".ToLower();
            var edgeConfiguration = new Configuration(deploymentId);

            var packageEdgeConfiguration = JsonConvert.DeserializeObject<Configuration>(package.Content);
            edgeConfiguration.Content = packageEdgeConfiguration.Content;

            var query = JsonConvert.SerializeObject(deviceGroup.Conditions);
            query = QueryConditionTranslator.ToQueryString(query);
            query = string.IsNullOrEmpty(query) ? "*" : query;

            edgeConfiguration.TargetCondition = query;
            edgeConfiguration.Priority = priority;
            edgeConfiguration.ETag = string.Empty;

            if(edgeConfiguration.Labels == null)
            {
                edgeConfiguration.Labels = new Dictionary<string, string>();
            }
            edgeConfiguration.Labels.Add(DEPLOYMENT_NAME_KEY, name);

            return edgeConfiguration;
        }
    }
}