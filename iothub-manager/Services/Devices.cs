// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDevices
    {
        Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken);
        Task<DeviceServiceModel> GetAsync(string id);
        Task<DeviceServiceModel> CreateAsync(DeviceServiceModel toServiceModel);
        Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel toServiceModel);
        Task DeleteAsync(string id);
    }

    public class Devices : IDevices
    {
        private const int MaxGetList = 1000;

        private readonly RegistryManager registry;
        private readonly string ioTHubHostName;

        public Devices(IServicesConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // make sure the exception could throw earlier if format is not valid
            this.registry = RegistryManager.CreateFromConnectionString(config.HubConnString);
            this.ioTHubHostName = IotHubConnectionStringBuilder.Create(config.HubConnString).HostName;
        }

        /// <summary>
        /// Query devices
        /// </summary>
        /// <param name="query">
        /// Two types of query supported:
        /// 1. Serialized Clause list in JSON. Each clause includes three parts: key, operator and value
        /// 2. The "Where" clause of official IoTHub query string, except keyword "WHERE"
        /// </param>
        /// <param name="continuationToken">Continuation token. Not in use yet</param>
        /// <returns>List of devices</returns>
        public async Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Try to translate clauses to query
                query = QueryConditionTranslator.ToQueryString(query);
            }

            // normally we need deviceTwins for all devices to show device list
            var devices = await this.registry.GetDevicesAsync(MaxGetList);

            #region WORKAROUND
            // WORKAROUND: when we have the query API supported, we can replace to use query API
            var twins = await GetTwins(devices.Select(d => d.Id));
            if (!string.IsNullOrEmpty(query))
            {
                twins = ApplyQuery(query, twins);
                devices = devices.Where(d => twins.Any(t => t.DeviceId == d.Id));
            }
            #endregion

            return new DeviceServiceListModel(devices.Select(azureDevice =>
                new DeviceServiceModel(azureDevice, twins.Single(t => t.DeviceId == azureDevice.Id),
                this.ioTHubHostName)));
        }

        public async Task<DeviceServiceModel> GetAsync(string id)
        {
            var device = this.registry.GetDeviceAsync(id);
            var twin = this.registry.GetTwinAsync(id);

            await Task.WhenAll(device, twin);

            if (device.Result == null)
            {
                throw new ResourceNotFoundException("The device doesn't exist.");
            }

            return new DeviceServiceModel(device.Result, twin.Result, this.ioTHubHostName);
        }

        public async Task<DeviceServiceModel> CreateAsync(DeviceServiceModel device)
        {
            var azureDevice = await this.registry.AddDeviceAsync(device.ToAzureModel());

            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await this.registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await this.registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.Etag);
            }

            return new DeviceServiceModel(azureDevice, azureTwin, this.ioTHubHostName);
        }

        /// <summary>
        /// We only support update twin
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task<DeviceServiceModel> CreateOrUpdateAsync(DeviceServiceModel device)
        {
            // validate device module
            var azureDevice = await this.registry.GetDeviceAsync(device.Id);
            if (azureDevice == null)
            {
                azureDevice = await this.registry.AddDeviceAsync(device.ToAzureModel());
            }

            Twin azureTwin;
            if (device.Twin == null)
            {
                azureTwin = await this.registry.GetTwinAsync(device.Id);
            }
            else
            {
                azureTwin = await this.registry.UpdateTwinAsync(device.Id, device.Twin.ToAzureModel(), device.Twin.Etag);
            }

            return new DeviceServiceModel(azureDevice, azureTwin, this.ioTHubHostName);
        }

        public async Task DeleteAsync(string id)
        {
            await this.registry.RemoveDeviceAsync(id);
        }

        // ToDo: remove workaround code once the SDK support is ready
        #region WORKAROUND
        /// <summary>
        /// Workaround before we have query in SDK
        ///     support "and" only.. case sensitive
        ///     support number/string only
        /// Query example:
        ///     tags.factory = "contoso"
        ///     desired.config.telemetryInterval = 10
        ///     properties.reported.config.sensorStatus = "normal"
        /// </summary>
        /// <param name="query">The query for the devices</param>
        /// <param name="twinList">The list of twin need to filter</param>
        /// <returns>The list of twin matching the query</returns>
        private IEnumerable<Twin> ApplyQuery(string query, IEnumerable<Twin> twinList)
        {
            if (string.IsNullOrEmpty(query))
            {
                return twinList;
            }

            var filters = query.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
            var twins = new List<Twin>(twinList);
            foreach (var filterString in filters)
            {
                var filter = new Filter(filterString);
                List<Twin> filteredTwins = new List<Twin>();
                foreach (var twin in twins)
                {
                    switch (filter.Target)
                    {
                        case Filter.FilterTarget.Tags:
                            if (filter.Match(twin.Tags))
                            {
                                filteredTwins.Add(twin);
                            }

                            break;
                        case Filter.FilterTarget.Reported:
                            if (twin.Properties != null && filter.Match(twin.Properties.Reported))
                            {
                                filteredTwins.Add(twin);
                            }

                            break;
                        case Filter.FilterTarget.Desired:
                            if (twin.Properties != null && filter.Match(twin.Properties.Desired))
                            {
                                filteredTwins.Add(twin);
                            }

                            break;
                        default:
                            break;
                    }
                }

                twins = filteredTwins;
            }

            return twins;
        }

        private async Task<IEnumerable<Twin>> GetTwins(IEnumerable<string> deviceIds)
        {
            const int batchSize = 10;
            const int batchIntervalInMilliseconds = 1000;

            var twins = new List<Twin>();
            while (deviceIds.Any())
            {
                var batch = deviceIds.Take(batchSize);
                var tasks = batch.Select(id => this.registry.GetTwinAsync(id));
                twins.AddRange(await Task.WhenAll(tasks));

                deviceIds = deviceIds.Skip(batch.Count());

                // Wait a moment to avoid throttling 
                await Task.Delay(batchIntervalInMilliseconds);
            }

            // For some reason (throttling and so on), GetTwinAsync may returns null. Ignore null values here
            return twins.Where(t => t != null);
        }

        private class Filter
        {
            private readonly string filterString;
            private string[] keyLevels;
            private bool isStringValue;
            private string Value;
            private FilterOperator Operator;

            public Filter(string filterString)
            {
                this.filterString = filterString;
                this.Initiate();
            }

            public FilterTarget Target { get; set; }

            public bool Match(TwinCollection collection)
            {
                // only support four level
                dynamic twinValue;

                try
                {
                    switch (keyLevels.Length)
                    {
                        case 2:
                            twinValue = collection[keyLevels[1]];
                            break;
                        case 3:
                            twinValue = collection[keyLevels[1]][keyLevels[2]];
                            break;
                        case 4:
                            twinValue = collection[keyLevels[1]][keyLevels[2]][keyLevels[3]];
                            break;
                        case 5:
                            twinValue = collection[keyLevels[1]][keyLevels[2]][keyLevels[3]][keyLevels[4]];
                            break;
                        default:
                            return false;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }

                try
                {
                    switch (this.Operator)
                    {
                        case FilterOperator.Equal:
                            return this.isStringValue ? twinValue == this.Value : twinValue == Double.Parse(this.Value);
                        case FilterOperator.NotEqual:
                            return this.isStringValue ? twinValue != this.Value : twinValue != Double.Parse(this.Value);
                        case FilterOperator.Greater:
                            return this.isStringValue ? twinValue > this.Value : twinValue > Double.Parse(this.Value);
                        case FilterOperator.GreaterOrEqual:
                            return this.isStringValue ? twinValue >= this.Value : twinValue >= Double.Parse(this.Value);
                        case FilterOperator.Lower:
                            return this.isStringValue ? twinValue < this.Value : twinValue < Double.Parse(this.Value);
                        case FilterOperator.LowerOrEqual:
                            return this.isStringValue ? twinValue <= this.Value : twinValue <= Double.Parse(this.Value);
                        default:
                            return false;
                    }
                }
                catch (System.FormatException)
                {
                    return false;
                }
            }

            private void Initiate()
            {
                if (this.filterString.Contains("!="))
                {
                    this.Operator = FilterOperator.NotEqual;
                    this.SeparateParts("!=");
                    return;
                }

                if (this.filterString.Contains(">="))
                {
                    this.Operator = FilterOperator.GreaterOrEqual;
                    this.SeparateParts(">=");
                    return;
                }

                if (this.filterString.Contains("<="))
                {
                    this.Operator = FilterOperator.LowerOrEqual;
                    this.SeparateParts("<=");
                    return;
                }

                if (this.filterString.Contains("="))
                {
                    this.Operator = FilterOperator.Equal;
                    this.SeparateParts("=");
                    return;
                }

                if (this.filterString.Contains(">"))
                {
                    this.Operator = FilterOperator.Greater;
                    this.SeparateParts(">");
                    return;
                }

                if (this.filterString.Contains("<"))
                {
                    this.Operator = FilterOperator.Lower;
                    this.SeparateParts("<");
                    return;
                }
            }

            private void SeparateParts(string separator)
            {
                var parts = this.filterString.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new ArgumentException("filterString");
                }

                // remove leading properties
                if (parts[0].StartsWith("properties"))
                {
                    parts[0] = parts[0].Substring("properties".Length);
                }

                // seperate key/value
                this.keyLevels = parts[0].Split('.').Select(p => p.Trim()).ToArray();
                this.isStringValue = parts[1].Trim().StartsWith("\"");
                this.Value = this.isStringValue ? parts[1].Trim().Trim('"') : parts[1].Trim();

                if (this.keyLevels[0].StartsWith("tags"))
                {
                    this.Target = FilterTarget.Tags;
                }
                else if (this.keyLevels[0].StartsWith("desired"))
                {
                    this.Target = FilterTarget.Desired;
                }
                else if (this.keyLevels[0].StartsWith("reported"))
                {
                    this.Target = FilterTarget.Reported;
                }
                else
                {
                    throw new ArgumentException("filterString");
                }
            }

            public enum FilterOperator
            {
                Equal,
                NotEqual,
                Greater,
                GreaterOrEqual,
                Lower,
                LowerOrEqual
            }

            public enum FilterTarget
            {
                Tags,
                Reported,
                Desired
            }
        }
        #endregion
    }

    // ToDo: remove workaround code once the SDK support is ready
    #region WORKAROUND for query "select count() from devices ...
    static class TwinCollectionExtension
    {
        public static dynamic Get(this TwinCollection collection, string flatName)
        {
            if (collection == null || string.IsNullOrWhiteSpace(flatName))
            {
                throw new ArgumentNullException();
            }

            return Get(collection, flatName.Split('.'));
        }

        private static dynamic Get(this TwinCollection collection, IEnumerable<string> names)
        {
            var name = names.First();

            // Pick node on current level
            if (!collection.Contains(name))
            {
                // No desired node found. Return null as error
                return null;
            }

            var child = collection[name];

            if (names.Count() == 1)
            {
                // Current node is the target node, , return the value
                return child;
            }

            if (child is TwinCollection)
            {
                // Current node is container, go to next level
                return Get(child as TwinCollection, names.Skip(1));
            }

            if (child is JContainer)
            {
                // Current node is container, go to next level
                return Get(child as JContainer, names.Skip(1));
            }

            // Currently, the container could only be TwinCollection or JContainer
            return null;
        }

        private static dynamic Get(this JContainer container, IEnumerable<string> names)
        {
            var name = names.First();

            // Pick node on current level
            var child = container[name];
            if (child == null)
            {
                // No desired node found. Return null as error
                return null;
            }

            if (names.Count() == 1)
            {
                // Current node is the target node, return the value
                return child;
            }

            if (child is JContainer)
            {
                // Current node is container, go to next level
                return Get(child as JContainer, names.Skip(1));
            }

            // The next level of JContainer must be JContainer
            return null;
        }
    }
    #endregion
}
