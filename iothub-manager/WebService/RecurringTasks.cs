// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.DevicePropertiesAgent
{
    public interface IRecurringTasks
    {
        void Run();
    }

    public class RecurringTasks : IRecurringTasks
    {
        // When cache initialization fails, retry in few seconds
        private const int CACHE_INIT_RETRY_SECS = 10;

        // After the cache is initialized, update it every few minutes
        private const int CACHE_UPDATE_SECS = 300;

        // When generating the cache, allow some time to finish, at least one minute
        private const int CACHE_TIMEOUT_SECS = 90;
        
        private readonly IDeviceProperties deviceProperties;
        private readonly ILogger log;

        public RecurringTasks(
            IDeviceProperties deviceProperties,
            ILogger logger)
        {
            this.deviceProperties = deviceProperties;
            this.log = logger;
        }

        public void Run()
        {
            this.BuildCache();
            this.ScheduleCacheUpdate();
        }

        private void BuildCache()
        {
            while (true)
            {
                try
                {
                    this.log.Info("Creating cache...", () => { });
                    this.deviceProperties.TryRecreateListAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                    this.log.Info("Cache created", () => { });
                    return;
                }
                catch (Exception e)
                {
                    this.log.Warn("Cache creation failed, will retry in few seconds", () => new { CACHE_INIT_RETRY_SECS, e });
                }

                this.log.Warn("Pausing thread before retrying cache creation", () => new { CACHE_INIT_RETRY_SECS });
                Thread.Sleep(CACHE_INIT_RETRY_SECS * 1000);
            }
        }

        private void ScheduleCacheUpdate()
        {
            try
            {
                this.log.Info("Scheduling a cache update", () => new { CACHE_UPDATE_SECS });
                var unused = new Timer(
                    this.UpdateCache,
                    null,
                    1000 * CACHE_UPDATE_SECS,
                    Timeout.Infinite);
                this.log.Info("Cache update scheduled", () => new { CACHE_UPDATE_SECS });
            }
            catch (Exception e)
            {
                this.log.Error("Cache update scheduling failed", () => new { e });
            }
        }

        private void UpdateCache(object context = null)
        {
            try
            {
                this.log.Info("Updating cache...", () => { });
                this.deviceProperties.TryRecreateListAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                this.log.Info("Cache updated", () => { });
            }
            catch (Exception e)
            {
                this.log.Warn("Cache update failed, will retry later", () => new { CACHE_UPDATE_SECS, e });
            }

            this.ScheduleCacheUpdate();
        }
    }
}
