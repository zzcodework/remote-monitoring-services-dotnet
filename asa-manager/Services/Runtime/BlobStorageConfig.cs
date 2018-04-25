// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    public interface IBlobStorageConfig
    {
        string ReferenceDataContainer { get; }
        string EventHubContainer { get; }
        string AccountName { get; }
        string AccountKey { get; }
        string EndpointSuffix { get; }
        string DeviceGroupsFileName { get; }
        string DateFormat { get; }
        string TimeFormat { get; }
    }

    public class BlobStorageConfig : IBlobStorageConfig
    {
        public string ReferenceDataContainer { get; set; }
        public string EventHubContainer { get; set; }
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string EndpointSuffix { get; set; }
        public string DeviceGroupsFileName { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
    }
}
