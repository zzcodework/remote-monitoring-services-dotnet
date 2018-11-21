// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageListApiModel
    {
        public IEnumerable<PackageApiModel> Items { get; set; }

        public PackageListApiModel(IEnumerable<Package> models)
        {
            this.Items = models.Select(m => new PackageApiModel(m));
        }
        
        /**
         * This method helps filtering packages by packageType and configType
         */
        public PackageListApiModel(IEnumerable<Package> models, string packageType, string configType)
        {
            if (string.IsNullOrEmpty(packageType))
            {
                this.Items = models.Select(m => new PackageApiModel(m));
            }
            else if (string.IsNullOrEmpty(configType))
            {
                this.Items = models.Select(m => new PackageApiModel(m)).Where(
                                                package => (
                                                package.packageType.ToString().ToLower().Equals(packageType.ToString().ToLower())));
            }
            else
            {
                this.Items = models.Select(m => new PackageApiModel(m)).Where(
                                package => (
                                package.ConfigType != null
                                && package.packageType.ToString().ToLower().Equals(packageType.ToLower())
                                && package.ConfigType.ToString().ToLower().Equals(configType.ToLower())));
            }
        }
    }
}