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
            var configTypes = configTypeList.Items.ToList<String>();

            foreach (ConfigType type in Enum.GetValues(typeof(ConfigType)))
            {
                configTypes.Insert(0, type.ToString());
              
            }

            this.Items = configTypes.ToArray<String>();
        }

    }
}
