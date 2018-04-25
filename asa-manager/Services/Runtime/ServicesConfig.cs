// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    public interface IServicesConfig
    {
        string RulesWebServiceUrl { get; set; }
        int RulesWebServiceTimeout { get; set; }
        string ConfigServiceUrl { get; set; }
        int ConfigServiceTimeout { get; set; }
        string IotHubManagerServiceUrl { get; set; }
        int IotHubManagerServiceTimeout { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string RulesWebServiceUrl { get; set; }
        public int RulesWebServiceTimeout { get; set; }
        public string ConfigServiceUrl { get; set; }
        public int ConfigServiceTimeout { get; set; }
        public string IotHubManagerServiceUrl { get; set; }
        public int IotHubManagerServiceTimeout { get; set; }
    }
}
