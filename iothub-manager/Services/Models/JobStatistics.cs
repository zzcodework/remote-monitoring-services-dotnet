// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    /// <summary>
    /// refer to Azure.Devices.DeviceJobStatistics
    /// </summary>
    public class JobStatistics
    {
        public JobStatistics()
        {
        }

        public JobStatistics(DeviceJobStatistics azureModel)
        {
            this.DeviceCount = azureModel.DeviceCount;
            this.FailedCount = azureModel.FailedCount;
            this.SucceededCount = azureModel.SucceededCount;
            this.RunningCount = azureModel.RunningCount;
            this.PendingCount = azureModel.PendingCount;
        }

        public int DeviceCount { get; set; }

        public int FailedCount { get; set; }

        public int SucceededCount { get; set; }

        public int RunningCount { get; set; }

        public int PendingCount { get; set; }
    }
}
