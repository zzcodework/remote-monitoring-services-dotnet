// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IAuthClient
    {
        Task<string> GetTokenAsync();
    }

    public class AuthClient : IAuthClient
    {
        private readonly IHttpClient httpClient;
        private readonly string serviceUri;

        public AuthClient(
            IHttpClient httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            // TODO
            // this.serviceUri = config.AuthApiUrl;
        }

        public async Task<string> GetTokenAsync()
        {
            var request = this.CreateRequest("/token");
            var response = await this.httpClient.GetAsync(request);

            // TODO dependent on API from auth
            return string.Empty;
        }

        private HttpRequest CreateRequest(string path)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            return request;
        }
    }
}
