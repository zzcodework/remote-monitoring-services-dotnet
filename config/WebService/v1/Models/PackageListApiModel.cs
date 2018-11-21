// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageListApiModel
    {
        public IEnumerable<PackageApiModel> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public PackageListApiModel(IEnumerable<Package> models)
        {
            this.Items = models.Select(m => new PackageApiModel(m));

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }

        /**
         * This method helps filtering packages by packageType and configType
         */
        public PackageListApiModel(IEnumerable<Package> models, string packageType, string configType)
        {
            this.Items = models.Select(m => new PackageApiModel(m));

            if (string.IsNullOrEmpty(configType))
            {
                this.Items = this.Items.Where(
                                package => (
                                package.packageType.ToString().ToLower().Equals(packageType.ToString().ToLower())));
            }
            else
            {
                this.Items = this.Items.Where(
                                package => (
                                package.ConfigType != null
                                && package.packageType.ToString().ToLower().Equals(packageType.ToLower())
                                && package.ConfigType.ToString().ToLower().Equals(configType.ToLower())));
            }

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"DevicePropertyList;{Version.NUMBER}" },
                { "$url", $"/{Version.PATH}/deviceproperties" }
            };
        }
    }
}