// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Runtime
{
    public interface IDeviceGroupsConfig
    {
        string EventHubConnectionString { get; }
    }

    public class DeviceGroupsConfig : IDeviceGroupsConfig
    {
        public string EventHubConnectionString { get; set; }
    }
}
