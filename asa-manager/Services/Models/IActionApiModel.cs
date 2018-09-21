// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    public interface IActionApiModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Type ActionType { get; set; }
        IDictionary<string, object> Parameters { get; set; }
    }

    public enum Type
    {
        Email
    }
}
