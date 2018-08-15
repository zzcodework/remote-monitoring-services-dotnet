// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using System.Linq;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public interface ITimeSeriesClient
    {
        Task<MessageList> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);
        Tuple<bool, string> Ping();
    }

    public class TimeSeriesClient : ITimeSeriesClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;

        private readonly string applicationId;
        private readonly string applicationSecret;
        private readonly string tenant;
        private readonly string fqdn;

        private const string TIME_SERIES_API_VERSION = "api-version=2016-12-12";
        private const string TIME_SERIES_TIMEOUT = "timeout=PT20S";
        private const string EVENTS_KEY = "events";
        private const string SEARCH_SPAN_KEY = "searchSpan";
        private const string PREDICATE_KEY = "predicate";
        private const string TOP_KEY = "top";
        private const string TOP_SORT_KEY = "sort";
        private const string TOP_INPUT_KEY = "input";
        private const string TOP_BUILT_IN_PROP_KEY = "builtInProperty";
        private const string TOP_BUILT_IN_PROP_VALUE = "$ts";
        private const string TOP_ORDER_KEY = "order";
        private const string TOP_COUNT_KEY = "count";
        private const string FROM_KEY = "from";
        private const string TO_KEY = "to";

        public TimeSeriesClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger log)
        {
            this.httpClient = httpClient;
            this.log = log;
            this.applicationId = config.ActiveDirectoryAppId;
            this.applicationSecret = config.ActiveDirectoryAppSecret;
            this.tenant = config.ActiveDirectoryTenant;
            this.fqdn = config.TimeSeriesFqdn;
        }

        public async Task<MessageList> ListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order, int skip,
            int limit,
            string[] devices)
        {
            // Acquire an access token.
            string accessToken = await AcquireAccessTokenAsync();

            // Prepare request
            HttpRequest request = PrepareRequest(
                this.fqdn,
                EVENTS_KEY,
                accessToken,
                new[] { TIME_SERIES_TIMEOUT });

            request.SetContent(
                this.PrepareInput(from, to, order, skip, limit, devices));

            var msg = "Making Query to Time Series: Uri" + request.Uri.ToString() + " Body: " + request.Content.ToString();
            this.log.Info(msg, () => new { request });

            var response = await this.httpClient.PostAsync(request);
            var jsonResponse = JsonConvert.DeserializeObject<JToken>(response.Content);

            throw new NotImplementedException();
        }

        public Tuple<bool, string> Ping()
        {
            return new Tuple<bool, string>(false,
                    "Could not reach Time Series Service.");
        }

        private async Task<string> AcquireAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(this.applicationId) ||
                string.IsNullOrEmpty(this.applicationSecret) ||
                string.IsNullOrEmpty(this.tenant))
            {
                throw new InvalidConfigurationException(
                    $"Active Directory properties 'ApplicationClientId', 'ApplicationClientSecret' and 'Tenant' are not set.");
            }

            var authenticationContext = new AuthenticationContext(
                $"https://login.windows.net/{this.tenant}",
                TokenCache.DefaultShared);

            AuthenticationResult token = await authenticationContext.AcquireTokenAsync(
                resource: "https://api.timeseries.azure.com/",
                clientCredential: new ClientCredential(
                    clientId: this.applicationId,
                    clientSecret: this.applicationSecret));

            return token.AccessToken;
        }

        private JObject PrepareInput(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            var result = new JObject();

            // Add searchSpan if from or to are provided
            if (from.HasValue || to.HasValue)
            {
                if (!to.HasValue) to = DateTimeOffset.UtcNow;
                if (!from.HasValue) from = DateTimeOffset.MaxValue;

                result.Add(SEARCH_SPAN_KEY, new JObject(
                    new JProperty(FROM_KEY, from),
                    new JProperty(TO_KEY, to)));
            }

            // TODO How to do skip?

            // Add limit and order to query
            result.Add(TOP_KEY, new JObject(
                new JProperty(TOP_SORT_KEY, new JArray(
                    new JObject(TOP_INPUT_KEY, new JObject(
                        new JProperty(TOP_BUILT_IN_PROP_KEY, TOP_BUILT_IN_PROP_VALUE),
                        new JProperty(TOP_ORDER_KEY, order)))),
                new JProperty(TOP_COUNT_KEY, limit))));

            return result;
        }

        /// <summary>
        /// Creates an HttpRequest for Time Series Insights with the required headers and tokens.
        /// </summary>
        private HttpRequest PrepareRequest(
            string host,
            string path,
            string accessToken,
            string[] queryArgs = null)
        {
            string args = TIME_SERIES_API_VERSION;
            if (args != null && queryArgs.Any())
            {
                args += "&" + String.Join("&", queryArgs);
            }

            Uri uri = new UriBuilder("https", host)
            {
                Path = path,
                Query = args
            }.Uri;
            HttpRequest request = new HttpRequest(uri);
            request.Headers.Add("x-ms-client-application-name", this.applicationId);
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            return request;
        }
    }

}
