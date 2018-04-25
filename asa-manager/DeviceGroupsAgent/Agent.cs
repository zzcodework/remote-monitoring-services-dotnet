// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        // ASA cannot have new reference data more than once per minute
        private const int CHECK_INTERVAL_MSECS = 60000;

        private readonly ILogger log;
        private readonly IDeviceGroupsWriter deviceGroupsWriter;
        private readonly IDeviceGroupsClient deviceGroupsClient;

        public Agent(
            IDeviceGroupsWriter deviceGroupsWriter,
            IDeviceGroupsClient deviceGroupsClient,
            ILogger logger)
        {
            this.log = logger;
            this.deviceGroupsWriter = deviceGroupsWriter;
            this.deviceGroupsClient = deviceGroupsClient;
        }

        public async Task RunAsync(CancellationToken runState)
        {
            this.log.Info("Device Groups Agent running", () => { });

            while (!runState.IsCancellationRequested)
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
    }
}
