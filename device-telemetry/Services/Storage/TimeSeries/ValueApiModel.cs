// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class ValueApiModel
    {
        [JsonProperty("schemaRid")]
        public long SchemaRid { get; set; }

        [JsonProperty("$ts")]
        public string Timestamp { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }
    }
}
