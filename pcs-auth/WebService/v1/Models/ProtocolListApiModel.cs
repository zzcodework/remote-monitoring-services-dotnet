// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models
{
    public class ProtocolListApiModel
    {
        public IEnumerable<ProtocolApiModel> Items { get; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; }

        public ProtocolListApiModel(IEnumerable<ProtocolServiceModel> models)
        {
            Items = models.Select(m => new ProtocolApiModel(m));

            Metadata = new Dictionary<string, string>
            {
                { "$type", $"ProtocolList;{Version.Number}" },
                { "$url", $"/{Version.Path}/protocols" }
            };
        }
    }
}
