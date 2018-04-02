// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IDeviceGroupsAgent
    {
        Task RunAsync();
        void Stop();
    }

    public class DeviceGroupsAgent : IDeviceGroupsAgent
    {
        private const int CHECK_INTERVAL_MSECS = 10000;

        private readonly ILogger log;
        private bool running;

        public DeviceGroupsAgent(ILogger logger)
        {
            this.log = logger;
            this.running = true;
        }

        public async Task RunAsync()
        {
            this.log.Info("Device Groups Agent running", () => { });

            while (this.running)
            {
                try
                {
                    await Task.CompletedTask;
                }
                catch (Exception e)
                {
                    this.log.Error("...", () => new { e });
                }

                Thread.Sleep(CHECK_INTERVAL_MSECS);
            }
        }

        public void Stop()
        {
            this.running = false;
        }
    }
}