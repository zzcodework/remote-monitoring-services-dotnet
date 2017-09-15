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

        /// <summary>CORS whitelist, in form { 'origins': [], 'methods': [], 'headers': [] }</summary>
        string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "iothubmanager:";
        private const string PortKey = ApplicationKey + "webservice_port";
        private const string CorsWhitelistKey = ApplicationKey + "cors_whitelist";
        private const string IoTHubConnStrKey = ApplicationKey + "iothub_connstring";

        private const string ConfigServiceKey = "config:";
        private string ConfigServiceUriKey = ConfigServiceKey + "webservice_url";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>CORS whitelist, in form { 'origins': [], 'methods': [], 'headers': [] }</summary>
        public string CorsWhitelist { get; }

        /// <summary>Service layer configuration</summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);
            this.CorsWhitelist = configData.GetString(CorsWhitelistKey);

            var connstring = configData.GetString(IoTHubConnStrKey);
            if (connstring.ToLowerInvariant().Contains("your azure iot hub"))
            {
                // In order to connect to Azure IoT Hub, the service requires a connection
                // string. The value can be found in the Azure Portal. For more information see
                // https://docs.microsoft.com/azure/iot-hub/iot-hub-csharp-csharp-getstarted
                // to find the connection string value.
                // The connection string can be stored in the 'appsettings.ini' configuration
                // file, or in the PCS_IOTHUB_CONNSTRING environment variable. When
                // working with VisualStudio, the environment variable can be set in the
                // WebService project settings, under the "Debug" tab.
                throw new Exception("The service configuration is incomplete. " +
                                    "Please provide your Azure IoT Hub connection string. " +
                                    "For more information, see the environment variables " +
                                    "used in project properties and the 'iothub_connstring' " +
                                    "value in the 'appsettings.ini' configuration file.");
            }

            this.ServicesConfig = new ServicesConfig
            {
                HubConnString = configData.GetString(IoTHubConnStrKey),
                ConfigServiceUri = configData.GetString(ConfigServiceUriKey)
            };
        }

        private static string MapRelativePath(string path)
        {
            if (path.StartsWith(".")) return AppContext.BaseDirectory + Path.DirectorySeparatorChar + path;
            return path;
        }
    }
}
