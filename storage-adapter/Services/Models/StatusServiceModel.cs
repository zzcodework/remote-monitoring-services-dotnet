// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models
{
    public class StatusServiceModel
    {        
        public StatusResultServiceModel Status { get; set; }

        public Dictionary<string, string> Properties { get; set; }
        
        public Dictionary<string, StatusResultServiceModel> Dependencies { get; set; }

        public StatusServiceModel(bool isHealthy, string message)
        {
            this.Status = new StatusResultServiceModel(isHealthy, message);
            this.Dependencies = new Dictionary<string, StatusResultServiceModel>();
            this.Properties = new Dictionary<string, string>();
        }
    }
}
