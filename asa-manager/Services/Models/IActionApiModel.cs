// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    public interface IActionApiModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("Type")]
        Type Type { get; set; }

        [JsonProperty("Parameters")]
        IDictionary<string, object> Parameters { get; set; }
    }

    public enum Type
    {
        Email
    }
}
