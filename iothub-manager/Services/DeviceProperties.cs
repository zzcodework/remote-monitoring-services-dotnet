// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeviceProperties
    {
        Task<List<string>> GetListAsync();

        Task<DevicePropertyServiceModel> UpdateListAsync(DevicePropertyServiceModel devicePropertyServiceModel);

        Task<bool> TryRecreateListAsync(bool force = false);
    }

    public class DeviceProperties : IDeviceProperties
    {
        private readonly IStorageAdapterClient storageClient;
        private readonly IDevices devices;
        private readonly ILogger log;
        private readonly string devicePropertiesWhitelist;
        private readonly long devicePropertiesTtl;
        private readonly long devicePropertiesRebuildTimeout;
        private readonly TimeSpan serviceQueryInterval = TimeSpan.FromSeconds(10);
        internal const string CACHE_COLLECTION_ID = "cache";
        internal const string CACHE_KEY = "twin";

        private const string WHITELIST_TAG_PREFIX = "tags.";
        private const string WHITELIST_REPORTED_PREFIX = "reported.";
        private const string TAG_PREFIX = "Tags.";
        private const string REPORTED_PREFIX = "Properties.Reported.";

        public DeviceProperties(IStorageAdapterClient storageClient,
            IServicesConfig config,
            ILogger logger,
            IDevices devices)
        {
            this.storageClient = storageClient;
            this.log = logger;
            this.devicePropertiesWhitelist = config.DevicePropertiesWhiteList;
            this.devicePropertiesTtl = config.DevicePropertiesTTL;
            this.devicePropertiesRebuildTimeout = config.DevicePropertiesRebuildTimeout;
            this.devices = devices;
        }

        public async Task<List<string>> GetListAsync()
        {
            try
            {
                var response = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                var properties =  JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
                List<string> result = new List<string>();
                foreach (string tag in properties.Tags)
                    result.Add(TAG_PREFIX + tag);
                foreach (string reported in properties.Reported)
                    result.Add(REPORTED_PREFIX + reported);
                return result;
            }
            catch (ResourceNotFoundException)
            {
                this.log.Info($"Cache get: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found", () => { });
                return new List<string>();
            }
        }

        public async Task<bool> TryRecreateListAsync(bool force = false)
        {
            var @lock = new StorageWriteLock<DevicePropertyServiceModel>(
                this.storageClient,
                CACHE_COLLECTION_ID,
                CACHE_KEY,
                (c, b) => c.Rebuilding = b,
                m => this.NeedBuild(force, m));

            while (true)
            {
                var locked = await @lock.TryLockAsync();
                if (locked == null)
                {
                    this.log.Warn("Cache rebuilding: lock failed due to conflict. Retry soon", () => { });
                    continue;
                }

                if (!locked.Value)
                {
                    return false;
                }

                // Build the cache content
                var twinNamesTask = this.GetValidNamesAsync();

                try
                {
                    Task.WaitAll(twinNamesTask);
                }
                catch (Exception)
                {
                    this.log.Warn($"Some underlying service is not ready. Retry after {this.serviceQueryInterval}", () => { });
                    await @lock.ReleaseAsync();
                    await Task.Delay(this.serviceQueryInterval);
                    continue;
                }

                var twinNames = twinNamesTask.Result;

                var updated = await @lock.WriteAndReleaseAsync(new DevicePropertyServiceModel
                {
                    Tags = twinNames.Tags,
                    Reported = twinNames.ReportedProperties
                });

                if (updated)
                {
                    return true;
                }

                this.log.Warn("Cache rebuilding: write failed due to conflict. Retry soon", () => { });
            }
        }

        public async Task<DevicePropertyServiceModel> UpdateListAsync(DevicePropertyServiceModel deviceProperties)
        {
            // To simplify code, use empty set to replace null set
            deviceProperties.Tags = deviceProperties.Tags ?? new HashSet<string>();
            deviceProperties.Reported = deviceProperties.Reported ?? new HashSet<string>();

            string etag = null;
            while (true)
            {
                ValueApiModel model = null;
                try
                {
                    model = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                }
                catch (ResourceNotFoundException)
                {
                    this.log.Info($"Cache updating: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found", () => { });
                }

                if (model != null)
                {
                    DevicePropertyServiceModel devicePropertiesFromStorage;

                    try
                    {
                        devicePropertiesFromStorage = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(model.Data);
                    }
                    catch
                    {
                        devicePropertiesFromStorage = new DevicePropertyServiceModel();
                    }
                    devicePropertiesFromStorage.Tags = devicePropertiesFromStorage.Tags ?? new HashSet<string>();
                    devicePropertiesFromStorage.Reported = devicePropertiesFromStorage.Reported ?? new HashSet<string>();

                    deviceProperties.Tags.UnionWith(devicePropertiesFromStorage.Tags);
                    deviceProperties.Reported.UnionWith(devicePropertiesFromStorage.Reported);
                    etag = model.ETag;
                    if (deviceProperties.Tags.Count == devicePropertiesFromStorage.Tags.Count && deviceProperties.Reported.Count == devicePropertiesFromStorage.Reported.Count)
                    {
                        return deviceProperties;
                    }
                }

                var value = JsonConvert.SerializeObject(deviceProperties);
                try
                {
                    var response = await this.storageClient.UpdateAsync(CACHE_COLLECTION_ID, CACHE_KEY, value, etag);
                    return JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
                }
                catch (ConflictingResourceException)
                {
                    this.log.Info("Cache updating: failed due to conflict. Retry soon", () => { });
                }
            }
        }
 
        private async Task<DeviceTwinName> GetValidNamesAsync()
        {
            ParseWhitelist(this.devicePropertiesWhitelist, out var fullNameWhitelist, out var prefixWhitelist);

            var validNames = new DeviceTwinName
            {
                Tags = fullNameWhitelist.Tags,
                ReportedProperties = fullNameWhitelist.ReportedProperties
            };

            if (prefixWhitelist.Tags.Any() || prefixWhitelist.ReportedProperties.Any())
            {
                DeviceTwinName allNames = await this.devices.GetDeviceTwinNamesAsync();

                validNames.Tags.UnionWith(
                    allNames.Tags
                        .Where(s => prefixWhitelist.Tags.Any(s.StartsWith)));

                validNames.ReportedProperties.UnionWith(
                    allNames.ReportedProperties
                        .Where(s => prefixWhitelist.ReportedProperties.Any(s.StartsWith)));
            }

            return validNames;
        }

        private static void ParseWhitelist(string whitelist, out DeviceTwinName fullNameWhitelist, out DeviceTwinName prefixWhitelist)
        {
            var whitelistItems = whitelist.Split(',').Select(s => s.Trim());

            var tags = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_TAG_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_TAG_PREFIX.Length));

            var reported = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_REPORTED_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_REPORTED_PREFIX.Length));

            var fixedTags = tags.Where(s => !s.EndsWith("*"));
            var fixedReported = reported.Where(s => !s.EndsWith("*"));

            var regexTags = tags.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));
            var regexReported = reported.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));

            fullNameWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(fixedTags),
                ReportedProperties = new HashSet<string>(fixedReported)
            };

            prefixWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(regexTags),
                ReportedProperties = new HashSet<string>(regexReported)
            };
        }

        private bool NeedBuild(bool force, ValueApiModel valueApiModel)
        {
            if (force)
            {
                this.log.Info("Cache will be rebuilt due to the force flag", () => { });
                return true;
            }

            if (valueApiModel == null)
            {
                this.log.Info("Cache will be rebuilt since no cache was found", () => { });
                return true;
            }

            var cacheValue = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(valueApiModel.Data);
            var timstamp = DateTimeOffset.Parse(valueApiModel.Metadata["$modified"]);

            if (cacheValue.Rebuilding)
            {
                if (timstamp.AddSeconds(this.devicePropertiesRebuildTimeout) < DateTimeOffset.UtcNow)
                {
                    this.log.Info("Cache will be rebuilt since last rebuilding was timeout", () => { });
                    return true;
                }
                else
                {
                    this.log.Info("Cache rebuilding skipped since it was rebuilding by other instance", () => { });
                    return false;
                }
            }
            else
            {
                if (cacheValue.IsNullOrEmpty())
                {
                    this.log.Info("Cache will be rebuilt since it is empty", () => { });
                    return true;
                }

                if (timstamp.AddSeconds(this.devicePropertiesTtl) < DateTimeOffset.UtcNow)
                {
                    this.log.Info("Cache will be rebuilt since it was expired", () => { });
                    return true;
                }
                else
                {
                    this.log.Info("Cache rebuilding skipped since it was not expired", () => { });
                    return false;
                }
            }
        }
    }
}
