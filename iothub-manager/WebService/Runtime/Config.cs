// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "iothubmanager:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string IoTHubConnStrKey = ApplicationKey + "iothub_connstring";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);

            this.ServicesConfig = new ServicesConfig
            {
                HubConnString = configData.GetString(IoTHubConnStrKey)
            };
        }

        private static string MapRelativePath(string path)
        {
            if (path.StartsWith(".")) return AppContext.BaseDirectory + Path.DirectorySeparatorChar + path;
            return path;
        }
    }
}
