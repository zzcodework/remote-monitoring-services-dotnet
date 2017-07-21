// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<DeviceServiceListModel> GetListAsync(string query, string continuationToken)
        {
            // normally we need deviceTwins for all devices to show device list
            var devices = await this.registry.GetDevicesAsync(MaxGetList);

            #region WORKAROUND

            // WORKAROUND: when we have the query API supported, we can replace to use query API
            var twinTasks = new List<Task<Twin>>();
            foreach (var item in devices)
            {
                twinTasks.Add(this.registry.GetTwinAsync(item.Id));
            }

            await Task.WhenAll(twinTasks.ToArray());

            // Workaround: filter by query
            var twins = twinTasks.Select(t => t.Result);
            if (!string.IsNullOrEmpty(query))
            {
                twins = ApplyQuery(query, twinTasks.Select(t => t.Result));
                devices = devices.Where(d => twins.Any(t => t.DeviceId == d.Id));
            } 
            #endregion

            return new DeviceServiceListModel(devices.Select(azureDevice => 
                                                new DeviceServiceModel(azureDevice, twins.Single(t => t.DeviceId == azureDevice.Id), 
                                                this.ioTHubHostName)), 
                                               null);
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
            if( azureDevice == null)
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
}
