using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    class ConfigListApiModel
    {
        private HashSet<String> customConfigTypes = new HashSet<String>();

        public List<String> configurations
        {
            set
            {
                value.ForEach(c => customConfigTypes.Add(c));
            }
        }

    }
}
