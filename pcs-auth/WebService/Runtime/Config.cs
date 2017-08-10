// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }</summary>
        string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "Auth:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string CorsWhitelistKey = ApplicationKey + "cors_whitelist";
        private const string AlgorithmsKey = ApplicationKey + "supported_signature_algorithms";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }</summary>
        public string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);
            this.CorsWhitelist = configData.GetString(CorsWhitelistKey);

            this.ServicesConfig = new ServicesConfig
            {
                Protocols = configData
                    .GetSectionNames()
                    .Where(s => s.StartsWith("Protocol"))
                    .Select(key => new ProtocolConfig(configData.GetSection(key))),
                SupportedSignatureAlgorithms = configData.GetString(AlgorithmsKey).Split(',')
            };
        }
    }
}
