// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Akka.Configuration;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }

        /// <summary>Service layer configuration</summary>
        Services.Runtime.IConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string Namespace = "com.microsoft.azure.iotsolutions.";
        private const string Application = "IotHubManager.";

        public Config()
        {
            string hoconConfig = GetHoconConfiguration();

            Akka.Configuration.Config config = ConfigurationFactory.ParseString(hoconConfig);

            this.Port = config.GetInt(Namespace + Application + "webservice-port");

            this.ServicesConfig = new Services.Runtime.Config
            {
                HubConnString = config.GetString(Namespace + Application + "iothub.connstring")
            };
        }

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        /// <summary>Service layer configuration</summary>
        public Services.Runtime.IConfig ServicesConfig { get; }

        /// <summary>
        /// Read the `application.conf` HOCON file, enabling substitutions of
        /// ${NAME} placeholders with environment variables values.
        /// </summary>
        /// <returns>Configuration text content</returns>
        private static string GetHoconConfiguration()
        {
            var hocon = File.ReadAllText("application.conf");

            // Append environment variables to allow Hocon substitutions on them
            var filter = new Regex(@"^[a-zA-Z0-9_/.,:;#(){}^=+~| !@$%&*'[\\\]-]*$");
            hocon += "\n";
            foreach (DictionaryEntry x in Environment.GetEnvironmentVariables())
            {
                //TODO: this pulls in just the connection string environment variable, it'd be better if akka could read it.
                if (x.Key.ToString() == "PCS_IOTHUB_CONN_STRING") hocon += x.Key + " : \"" + x.Value + "\"\n";
                //if (filter.IsMatch(x.Value.ToString())) hocon += x.Key + " : \"" + x.Value + "\"\n";
            }

            return hocon;
        }
    }
}
