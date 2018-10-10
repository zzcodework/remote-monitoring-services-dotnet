// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ActionSettingsApiModel
    {
        private const string DEFAULT_TYPE = "Email";

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Settings")]
        public IDictionary<string, object> Settings { get; set; }

        public ActionSettingsApiModel() : this(DEFAULT_TYPE, new Dictionary<string, object>()) { }

        public ActionSettingsApiModel(string type, Dictionary<string, object> settings)
        {
            this.Type = Enum.Parse(typeof(ActionType), type, true).ToString();
            this.Settings = settings;
        }
    }
}
