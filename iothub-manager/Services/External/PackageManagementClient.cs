// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public interface IPackageManagementClient
    {
        Task<PackageApiModel> GetPackageAsync(string packageId);
    }

    public class PackageManagementClient : IPackageManagementClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;

        public PackageManagementClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.ConfigApiUrl;
        }

        public async Task<PackageApiModel> GetPackageAsync(string packageId)
        {
            var request = this.CreateRequest($"packages/{packageId}");
            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<PackageApiModel>(response.Content);
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

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.log.Info($"Config service returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");
                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, "
                                                  +$"content = {response.Content}, request URL = {request.Uri}");
            }
        }
    }
}
