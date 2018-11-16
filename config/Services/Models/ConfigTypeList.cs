using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public class ConfigTypeList
    {
        private HashSet<String> configTypes = new HashSet<String>();

        public string[] Items
        {
            get
            {
                return configTypes.ToArray<String>();
            }
            set
            {
                Array.ForEach<String>(value, (c => configTypes.Add(c)));
            }
        }

        internal void add(string customConfig)
        {
            configTypes.Add(customConfig.Trim());
        }

    }
}
