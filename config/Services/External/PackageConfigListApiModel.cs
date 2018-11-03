using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    class PackageConfigListApiModel
    {
        private HashSet<String> customConfigTypes = new HashSet<String>();

        public List<String> configurations
        {
            get
            {
                return customConfigTypes.ToList();
            }
            set
            {
                value.ForEach(c => customConfigTypes.Add(c));
            }
        }

        internal void add(string customConfig)
        {
            customConfig = ConfigType.Custom.ToString() + " - " + customConfig;
            customConfigTypes.Add(customConfig);
        }

    }
}
