// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    [JsonConverter(typeof(ActionConverter))]
    public interface IActionApiModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("Type")]
        ActionType Type { get; set; }

        [JsonProperty("Parameters")]
        IDictionary<string, object> Parameters { get; set; }
    }

    public enum ActionType
    {
        Email
    }
}
