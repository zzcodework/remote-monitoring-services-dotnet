// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public class DeviceGroupApiModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Conditions")]
        public IEnumerable<DeviceGroupConditionApiModel> Conditions { get; set; }
        
        [JsonProperty("ETag")]
        public string ETag { get; set; }
    }
}
