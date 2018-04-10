// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    public interface IServicesConfig
    {
        string RulesWebServiceUrl { get; set; }
        int RulesWebServiceTimeout { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string RulesWebServiceUrl { get; set; }
        public int RulesWebServiceTimeout { get; set; }
    }
}
