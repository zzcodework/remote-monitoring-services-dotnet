// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Auth;

// TODO: tests
// TODO: handle errors
// TODO: use binding
namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        ILoggingConfig LoggingConfig { get; }

        // Service layer configuration
        IServicesConfig ServicesConfig { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }
        IDeviceGroupsConfig DeviceGroupsConfig { get; }
        IBlobStorageConfig BlobStorageConfig { get; }
    }

    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "AsaManagerService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservice_port";

        private const string LOGGING_KEY = APPLICATION_KEY + "Logging:";
        private const string LOGGING_LOGLEVEL_KEY = LOGGING_KEY + "LogLevel";
        private const string LOGGING_INCLUDEPROCESSID_KEY = LOGGING_KEY + "IncludeProcessId";
        private const string LOGGING_DATEFORMAT_KEY = LOGGING_KEY + "DateFormat";
        private const string LOGGING_BLACKLIST_PREFIX_KEY = LOGGING_KEY + "BWListPrefix";
        private const string LOGGING_BLACKLIST_SOURCES_KEY = LOGGING_KEY + "BlackListSources";
        private const string LOGGING_WHITELIST_SOURCES_KEY = LOGGING_KEY + "WhiteListSources";
        private const string LOGGING_EXTRADIAGNOSTICS_KEY = LOGGING_KEY + "ExtraDiagnostics";
        private const string LOGGING_EXTRADIAGNOSTICSPATH_KEY = LOGGING_KEY + "ExtraDiagnosticsPath";

        private const string CLIENT_AUTH_KEY = APPLICATION_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "cors_whitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "auth_type";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "auth_required";

        private const string JWT_KEY = APPLICATION_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowed_algorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "issuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clock_skew_seconds";

        private const string RULES_KEY = "DeviceTelemetryService:";
        private const string RULES_WEBSERVICE_URL_KEY = RULES_KEY + "webservice_url";
        private const string RULES_WEBSERVICE_TIMEOUT_KEY = RULES_KEY + "webservice_timeout";

        private const string CONFIG_KEY = "PCSConfigurationService:";
        private const string CONFIG_WEBSERVICE_URL_KEY = CONFIG_KEY + "webservice_url";
        private const string CONFIG_WEBSERVICE_TIMEOUT_KEY = CONFIG_KEY + "webservice_timeout";

        private const string IOTHUB_MANAGER_KEY = "IoTHubManagerService:";
        private const string IOTHUB_MANAGER_WEBSERVICE_URL_KEY = IOTHUB_MANAGER_KEY + "webservice_url";
        private const string IOTHUB_MANAGER_WEBSERVICE_TIMEOUT_KEY = IOTHUB_MANAGER_KEY + "webservice_timeout";

        private const string DEVICE_GROUPS_KEY = APPLICATION_KEY + "DeviceGroups:";
        private const string EVENTHUB_CONNECTION_KEY = DEVICE_GROUPS_KEY + "eventhub_connection_string";

        private const string BLOB_STORAGE_KEY = APPLICATION_KEY + "BlobStorage:";
        private const string STORAGE_REFERENCE_DATA_CONTAINER_KEY = BLOB_STORAGE_KEY + "reference_data_container";
        private const string STORAGE_EVENTHUB_CONTAINER_KEY = BLOB_STORAGE_KEY + "eventhub_container";
        private const string STORAGE_ACCOUNT_NAME_KEY = BLOB_STORAGE_KEY + "account_name";
        private const string STORAGE_ACCOUNT_KEY_KEY = BLOB_STORAGE_KEY + "account_key";
        private const string STORAGE_ACCOUNT_ENDPOINT_KEY = BLOB_STORAGE_KEY + "account_endpoint";
        private const string STORAGE_DEVICE_GROUPS_FILE_NAME = BLOB_STORAGE_KEY + "device_groups_file_name";
        private const string STORAGE_DATE_FORMAT = BLOB_STORAGE_KEY + "reference_data_date_format";
        private const string STORAGE_TIME_FORMAT = BLOB_STORAGE_KEY + "reference_data_time_format";

        public int Port { get; }
        public ILoggingConfig LoggingConfig { get; set; }
        public IClientAuthConfig ClientAuthConfig { get; }
        public IServicesConfig ServicesConfig { get; }
        public IDeviceGroupsConfig DeviceGroupsConfig { get; }
        public IBlobStorageConfig BlobStorageConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);
            this.LoggingConfig = GetLogConfig(configData);
            this.ServicesConfig = GetServicesConfig(configData);
            this.ClientAuthConfig = GetClientAuthConfig(configData);
            this.DeviceGroupsConfig = GetDeviceGroupsConfig(configData);
            this.BlobStorageConfig = GetBlobStorageConfig(configData);
        }

        private static ILoggingConfig GetLogConfig(IConfigData configData)
        {
            var data = configData.GetString(LOGGING_BLACKLIST_SOURCES_KEY);
            var values = data.Replace(";", ",").Replace(":", ".").Split(",");
            var blacklist = new HashSet<string>();
            foreach (var k in values) blacklist.Add(k);

            data = configData.GetString(LOGGING_WHITELIST_SOURCES_KEY);
            values = data.Replace(";", ",").Replace(":", ".").Split(",");
            var whitelist = new HashSet<string>();
            foreach (var k in values) blacklist.Add(k);

            Enum.TryParse(configData.GetString(LOGGING_LOGLEVEL_KEY, Services.Diagnostics.LoggingConfig.DEFAULT_LOGLEVEL.ToString()), true, out LogLevel logLevel);
            var result = new LoggingConfig
            {
                LogLevel = logLevel,
                BwListPrefix = configData.GetString(LOGGING_BLACKLIST_PREFIX_KEY),
                BlackList = blacklist,
                WhiteList = whitelist,
                DateFormat = configData.GetString(LOGGING_DATEFORMAT_KEY, Services.Diagnostics.LoggingConfig.DEFAULT_DATE_FORMAT),
                LogProcessId = configData.GetBool(LOGGING_INCLUDEPROCESSID_KEY, true),
                ExtraDiagnostics = configData.GetBool(LOGGING_EXTRADIAGNOSTICS_KEY, false),
                ExtraDiagnosticsPath = configData.GetString(LOGGING_EXTRADIAGNOSTICSPATH_KEY)
            };

            return result;
        }

        private static IClientAuthConfig GetClientAuthConfig(IConfigData configData)
        {
            return new ClientAuthConfig
            {
                // By default CORS is disabled
                CorsWhitelist = configData.GetString(CORS_WHITELIST_KEY, string.Empty),
                // By default Auth is required
                AuthRequired = configData.GetBool(AUTH_REQUIRED_KEY, true),
                // By default auth type is JWT
                AuthType = configData.GetString(AUTH_TYPE_KEY, "JWT"),
                // By default the only trusted algorithms are RS256, RS384, RS512
                JwtAllowedAlgos = configData.GetString(JWT_ALGOS_KEY, "RS256,RS384,RS512").Split(','),
                JwtIssuer = configData.GetString(JWT_ISSUER_KEY, String.Empty),
                JwtAudience = configData.GetString(JWT_AUDIENCE_KEY, String.Empty),
                // By default the allowed clock skew is 2 minutes
                JwtClockSkew = TimeSpan.FromSeconds(configData.GetInt(JWT_CLOCK_SKEW_KEY, 120)),
            };
        }

        private static IServicesConfig GetServicesConfig(IConfigData configData)
        {
            return new ServicesConfig
            {
                RulesWebServiceUrl = configData.GetString(RULES_WEBSERVICE_URL_KEY),
                RulesWebServiceTimeout = configData.GetInt(RULES_WEBSERVICE_TIMEOUT_KEY),
                ConfigServiceUrl = configData.GetString(CONFIG_WEBSERVICE_URL_KEY),
                ConfigServiceTimeout = configData.GetInt(CONFIG_WEBSERVICE_TIMEOUT_KEY),
                IotHubManagerServiceUrl = configData.GetString(IOTHUB_MANAGER_WEBSERVICE_URL_KEY),
                IotHubManagerServiceTimeout = configData.GetInt(IOTHUB_MANAGER_WEBSERVICE_TIMEOUT_KEY)
            };
        }

        private static IDeviceGroupsConfig GetDeviceGroupsConfig(IConfigData configData)
        {
            return new DeviceGroupsConfig
            {
                EventHubConnectionString = configData.GetString(EVENTHUB_CONNECTION_KEY)
            };
        }

        private static IBlobStorageConfig GetBlobStorageConfig(IConfigData configData)
        {
            return new BlobStorageConfig
            {
                ReferenceDataContainer = configData.GetString(STORAGE_REFERENCE_DATA_CONTAINER_KEY),
                EventHubContainer = configData.GetString(STORAGE_EVENTHUB_CONTAINER_KEY),
                AccountKey = configData.GetString(STORAGE_ACCOUNT_KEY_KEY),
                AccountName = configData.GetString(STORAGE_ACCOUNT_NAME_KEY),
                EndpointSuffix = configData.GetString(STORAGE_ACCOUNT_ENDPOINT_KEY),
                DeviceGroupsFileName = configData.GetString(STORAGE_DEVICE_GROUPS_FILE_NAME),
                DateFormat = configData.GetString(STORAGE_DATE_FORMAT),
                TimeFormat = configData.GetString(STORAGE_TIME_FORMAT)
            };
        }
    }
}
