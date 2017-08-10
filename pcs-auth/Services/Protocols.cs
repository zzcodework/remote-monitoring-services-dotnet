// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public class Protocols : IProtocols
    {
        private readonly IEnumerable<ProtocolServiceModel> protocols;

        public Protocols(IServicesConfig config)
        {
            protocols = config.Protocols.Select(p => new ProtocolServiceModel
            {
                Name = p.Name,
                Type = p.Type,
                Parameters = p.Parameters
            });
        }

        public IEnumerable<ProtocolServiceModel> GetAll()
        {
            return protocols;
        }
    }
}
