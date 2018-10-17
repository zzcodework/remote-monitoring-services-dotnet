// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions
{
    public class EmailActionSettings : IActionSettings
    {
        private const string IS_ENABLED_KEY = "IsEnabled";
        private const string OFFICE365_CONNECTOR_URL_KEY = "Office365ConnectorUrl";

        private readonly ILogicAppClient logicAppClient;
        private readonly IServicesConfig servicesConfig;
        private readonly ILogger log;

        public ActionType Type { get; }

        public IDictionary<string, object> Settings { get; set; }

        // Use the Factory Pattern to create instance of Email Action Settings
        // because of the asynchronous calls to get Logic App Status.
        public EmailActionSettings(
            ILogicAppClient logicAppClient,
            IServicesConfig servicesConfig,
            ILogger log)
        {
            this.logicAppClient = logicAppClient;
            this.servicesConfig = servicesConfig;
            this.log = log;

            this.Type = ActionType.Email;
            this.Settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task InitializeAsync()
        {
            // Check signin status of Office 365 Logic App Connector
            var office365IsEnabled = await this.logicAppClient.Office365IsEnabledAsync();
            this.Settings.Add(IS_ENABLED_KEY, office365IsEnabled);

            // Get Url for Office 365 Logic App Connector setup in portal
            // for display on the webui for one-time setup.
            this.Settings.Add(OFFICE365_CONNECTOR_URL_KEY, this.servicesConfig.Office365LogicAppUrl);

            this.log.Debug("Email Action Settings Retrieved. Email setup status: " + office365IsEnabled, () => new { this.Settings });
        }
    }
}
