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

        public PackageListApiModel(IEnumerable<Package> models, string type, string config)
        {
            if (string.IsNullOrEmpty(type))
            {
                this.Items = models.Select(m => new PackageApiModel(m));
            }
            else if (string.IsNullOrEmpty(config))
            {
                this.Items = models.Select(m => new PackageApiModel(m)).Where(
                                                package => (
                                                package.packageType.ToString().ToLower().Equals(type.ToString().ToLower())));
            }
            else
            {
                this.Items = models.Select(m => new PackageApiModel(m)).Where(
                                package => (
                                package.ConfigType != null
                                && package.packageType.ToString().ToLower().Equals(type.ToLower())
                                && package.ConfigType.ToString().ToLower().Equals(config.ToLower())));
            }
        }
    }
}