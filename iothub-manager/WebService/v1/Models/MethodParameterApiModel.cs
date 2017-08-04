// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using System;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class MethodParameterApiModel
    {
        public string Name { get; set; }

        public TimeSpan? ResponseTimeout { get; set; }

        public TimeSpan? ConnectionTimeout { get; set; }

        public string JsonPayload { get; set; }

        public MethodParameterApiModel()
        {
        }

        public MethodParameterApiModel(MethodParameterServiceModel serviceModel)
        {
            if (serviceModel != null)
            {
                this.Name = serviceModel.Name;
                this.ResponseTimeout = serviceModel.ResponseTimeout;
                this.ConnectionTimeout = serviceModel.ConnectionTimeout;
                this.JsonPayload = serviceModel.JsonPayload;
            }
        }

        public MethodParameterServiceModel ToServiceModel()
        {
            return new MethodParameterServiceModel()
            {
                Name = this.Name,
                ResponseTimeout = this.ResponseTimeout,
                ConnectionTimeout = this.ConnectionTimeout,
                JsonPayload = this.JsonPayload
            };
        }
    }
}
