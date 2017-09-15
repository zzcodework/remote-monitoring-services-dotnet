// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime
{
    public interface IServicesConfig
    {
        string HubConnString { get; set; }
        string ConfigServiceUri { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string HubConnString { get; set; }
        public string ConfigServiceUri { get; set; }
    }
}
