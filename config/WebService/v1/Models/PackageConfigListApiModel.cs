using System;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageConfigListApiModel
    {
        [JsonProperty("Configurations")]
        public string[] packageConfigurations{ get; set; }

        public PackageConfigListApiModel(PackageConfigurations packageConfigurations)
        {
            var customConfigs = packageConfigurations.configurations.ToList<String>();

            foreach (ConfigType type in Enum.GetValues(typeof(ConfigType)))
            {
                customConfigs.Insert(0,type.ToString());
              
            }

            this.packageConfigurations = customConfigs.ToArray<String>();
        }

    }
}
