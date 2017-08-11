// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public class Protocols : IProtocols
    {
        private readonly ProtocolListServiceModel protocols;

        public Protocols(IServicesConfig config)
        {
            protocols = new ProtocolListServiceModel
            {
                Items = config.Protocols.Select(p => new ProtocolServiceModel
                {
                    Name = p.Name,
                    Type = p.Type,
                    Parameters = p.Parameters
                }),
                SupportedSignatureAlgorithms = config.SupportedSignatureAlgorithms
            };
        }

        public ProtocolListServiceModel GetAll()
        {
            return protocols;
        }
    }
}
