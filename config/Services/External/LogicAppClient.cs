// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface ILogicAppClient
    {
        Task<bool> Office365IsEnabledAsync();
    }

    public class LogicAppClient : ILogicAppClient
    {
        private readonly IHttpClient httpClient;
        private readonly IUserManagementClient userManagementClient;

        public LogicAppClient(
            IHttpClient httpClient,
            IServicesConfig config,
            IUserManagementClient userManagementClient)
        {
            this.httpClient = httpClient;
            this.userManagementClient = userManagementClient;
        }

        public async Task<bool> Office365IsEnabledAsync()
        {
            string subscription = "skdf";
            string resourceGroup = "sdkfj";
            string logicAppTestConnectionUri = "https://management.azure.com/subscriptions/" +
                                               $"{subscription}/resourceGroups/{resourceGroup}/" +
                                               "providers/Microsoft.Web/connections/" +
                                               "office365-connector/extensions/proxy/testconnection?" +
                                               "api-version=2016-06-01";

            var request = await this.CreateRequest(logicAppTestConnectionUri);
            var response = await this.httpClient.GetAsync(request);

            return false;
        }

        private async Task<HttpRequest> CreateRequest(string uri, IEnumerable<string> content = null)
        {
            var request = new HttpRequest();
            if (uri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            var token = await this.userManagementClient.GetTokenAsync();
            request.Headers.Add("Authorization", "Bearer " + token);

            return request;
        }
    }
}
