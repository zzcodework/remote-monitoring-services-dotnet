using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ConfigTypeListApiModel
    {
        [JsonProperty("Items")]
        public string[] configTypes { get; set; }

        public ConfigTypeListApiModel(ConfigTypeList configTypeList)
        {
            this.configTypes = configTypeList.ConfigTypes;
        }
    }
}
