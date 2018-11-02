// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Models
{
    public class StatusResultServiceModel
    {
        public bool IsHealthy { get; set; }

        public string Message { get; set; }

        public StatusResultServiceModel(StatusResultServiceModel result)
        {
            IsHealthy = result.IsHealthy;
            Message = result.Message;
        }

        public StatusResultServiceModel(bool isHealthy, string message)
        {
            IsHealthy = isHealthy;
            Message = message;
        }
    }
}
