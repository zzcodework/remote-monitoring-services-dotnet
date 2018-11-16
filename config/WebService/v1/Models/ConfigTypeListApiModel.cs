using System;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class ConfigTypeListApiModel
    {
        [JsonProperty("Items")]
        public string[] Items { get; set; }

        public ConfigTypeListApiModel(ConfigTypeList configTypeList)
        {

            foreach (ConfigType type in Enum.GetValues(typeof(ConfigType)))
            {
                configTypeList.Items.Prepend(type.ToString());
            }

            this.Items = configTypeList.Items;
        }

    }
}
