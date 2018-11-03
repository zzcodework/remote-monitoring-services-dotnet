using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class PackageConfigurations
    {
        private HashSet<String> customConfigTypes = new HashSet<String>();

        public string[] configurations
        {
            get
            {
                return customConfigTypes.ToArray<String>();
            }
            set
            {
                Array.ForEach<String>(value, (c => customConfigTypes.Add(c)));
            }
        }

        internal void add(string customConfig)
        {
            customConfig = ConfigType.Custom.ToString() + " - " + customConfig;
            customConfigTypes.Add(customConfig.Trim());
        }

    }
}
