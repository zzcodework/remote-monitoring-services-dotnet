// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Runtime
{

    public interface IServicesConfig
    {
        IEnumerable<ProtocolConfig> Protocols { get; }

        IEnumerable<string> SupportedSignatureAlgorithms { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public IEnumerable<ProtocolConfig> Protocols { get; set; }

        public IEnumerable<string> SupportedSignatureAlgorithms { get; set; }
    }
}
