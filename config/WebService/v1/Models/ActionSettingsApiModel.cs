// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ActionSettingsApiModel
    {
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Settings")]
        public IDictionary<string, object> Settings { get; set; }

        public ActionSettingsApiModel(ActionType type, IDictionary<string, object> settings)
        {
            this.Type = type.ToString();
            this.Settings = settings;
        }

        public ActionSettingsApiModel() : this(ActionType.Email, new Dictionary<string, object>()) { }

        public ActionSettingsApiModel(IActionSettings actionSettings) :
            this(actionSettings.Type, actionSettings.Settings) { }
    }
}
