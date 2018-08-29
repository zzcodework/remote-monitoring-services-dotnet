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
    public interface IDeviceGroupsClient
    {
        Task<DeviceGroupApiModel> GetDeviceGroupsAsync(string deviceGroupId);
    }

    public class DeviceGroupsClient : IDeviceGroupsClient
    {
        private readonly ILogger logger;
        private readonly IHttpClient httpClient;
        private readonly string baseUrl;

        public DeviceGroupsClient(
            IHttpClient httpClient,
            IServicesConfig servicesConfig,
            ILogger logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.baseUrl = $"{servicesConfig.ConfigApiUrl}/devicegroups";
        }

        /**
         * Queries for the list of device group definitions and returns the list
         */
        public async Task<DeviceGroupApiModel> GetDeviceGroupsAsync(string deviceGroupId)
        {
            var request = this.CreateRequest($"{deviceGroupId}");
            var response = await this.httpClient.GetAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<DeviceGroupApiModel>(response.Content);
        }

        private HttpRequest CreateRequest(string path, ValueApiModel content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.baseUrl}/{path}");
            if (this.baseUrl.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            return request;
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.logger.Info($"StorageAdapter returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");

                case HttpStatusCode.Conflict:
                    throw new ConflictingResourceException($"{response.Content}, request URL = {request.Uri}");

                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, content = {response.Content}, request URL = {request.Uri}");
            }
        }
    }
}
