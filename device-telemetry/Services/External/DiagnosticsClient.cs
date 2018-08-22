// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External
{
    public interface IDiagnosticsClient
    {
        Task LogEventAsync(string eventName);

        Task LogEventAsync(string eventName, Dictionary<string, object> eventProperties);
    }

    public class DiagnosticsClient : IDiagnosticsClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUrl;
        private readonly int maxRetries;

        public DiagnosticsClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUrl = config.DiagnosticsApiUrl;
            this.maxRetries = config.DiagnosticsMaxLogRetries;
        }

        /**
         * Logs event with given event name and empty event properties
         * to diagnostics event endpoint.
         */
        public async Task LogEventAsync(string eventName)
        {
            await this.LogEventAsync(eventName, new Dictionary<string, object>());
        }

        /**
         * Logs event with given event name and event properties
         * to diagnostics event endpoint.
         */
        public async Task LogEventAsync(string eventName, Dictionary<string, object> eventProperties)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUrl}/diagnosticsevents");
            DiagnosticsRequestModel model = new DiagnosticsRequestModel
            {
                EventType = eventName,
                EventProperties = eventProperties
            };
            request.SetContent(JsonConvert.SerializeObject(model));
            await this.PostHttpRequestWithRetryAsync(request);
        }

        private async Task PostHttpRequestWithRetryAsync(HttpRequest request)
        {
            int retries = 0;
            bool requestSucceeded = false;
            while (!requestSucceeded && retries < this.maxRetries)
            {
                try
                {
                    IHttpResponse response = await this.httpClient.PostAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        retries++;
                        this.LogOnFailure(retries, response.Content);
                    }
                    else
                    {
                        requestSucceeded = true;
                    }
                }
                catch (Exception e)
                {
                    retries++;
                    this.LogOnFailure(retries, e.Message);
                }
            }
        }

        private void LogOnFailure(int retries, string errorMessage)
        {
            if (retries < this.maxRetries)
            {
                int retriesLeft = this.maxRetries - retries;
                string logString = $"Failed to log to diagnostics, {retriesLeft} retries remaining";

                this.log.Warn(logString, () => new { errorMessage });
            }
            else
            {
                this.log.Error("Failed to log to diagnostics, reached max retries and will not log", () => new { errorMessage });
            }
        }
    }
}
