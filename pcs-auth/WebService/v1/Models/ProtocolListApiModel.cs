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

        public IEnumerable<string> SupportedSignatureAlgorithms { get; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; }

        public ProtocolListApiModel(ProtocolListServiceModel model)
        {
            Items = model.Items.Select(m => new ProtocolApiModel(m));

            SupportedSignatureAlgorithms = model.SupportedSignatureAlgorithms;

            Metadata = new Dictionary<string, string>
            {
                { "$type", $"ProtocolList;{Version.Number}" },
                { "$url", $"/{Version.Path}/protocols" }
            };
        }
    }
}
