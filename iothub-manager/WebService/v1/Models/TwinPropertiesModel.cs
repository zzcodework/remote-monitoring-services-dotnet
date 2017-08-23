// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class TwinPropertiesModel
    {
        public TwinPropertiesModel(Dictionary<string, JToken> desired, Dictionary<string, JToken> reported)
        {
            this.Desired = desired;
            this.Reported = reported;
        }

        [JsonProperty(PropertyName = "Reported", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JToken> Reported { get; set; }

        [JsonProperty(PropertyName = "Desired", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JToken> Desired { get; set; }
    }
}