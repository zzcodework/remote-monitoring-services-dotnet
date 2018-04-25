// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IDeviceGroupsAgent
    {
        Task RunAsync();
        void Stop();
    }

    public class GroupsAgent : IDeviceGroupsAgent
    {
        // ASA cannot have new reference data more than once per minute
        private const int CHECK_INTERVAL_MSECS = 60000;

        private readonly ILogger log;
        private readonly IDeviceGroupsWriter deviceGroupsWriter;
        private readonly IDeviceGroupsClient deviceGroupsClient;
        private bool running;

        public GroupsAgent(
            IDeviceGroupsWriter deviceGroupsWriter,
            IDeviceGroupsClient deviceGroupsClient,
            ILogger logger)
        {
            this.log = logger;
            this.running = true;
            this.deviceGroupsWriter = deviceGroupsWriter;
            this.deviceGroupsClient = deviceGroupsClient;
        }

        public async Task RunAsync()
        {
            this.log.Info("Device Groups Agent running", () => { });

            while (this.running)
            {
                try
                {
                    DeviceGroupListApiModel deviceGroupList = await this.deviceGroupsClient.GetDeviceGroupsAsync();
                    var deviceGroupMapping = await this.deviceGroupsClient.GetGroupToDevicesMappingAsync(deviceGroupList);
                    await this.deviceGroupsWriter.ExportMapToReferenceData(deviceGroupMapping, DateTimeOffset.UtcNow);
                }
                catch (Exception e)
                {
                    this.log.Error("Received error updating device to device group mapping", () => new { e });
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