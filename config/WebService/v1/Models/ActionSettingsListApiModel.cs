// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ActionSettingsListApiModel
    {
        [JsonProperty("Items")]
        public List<ActionSettingsApiModel> Items { get; set; }

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public ActionSettingsListApiModel()
        {
            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"ActionSettingsList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/action-settings" }
            };
        }
    }
}
