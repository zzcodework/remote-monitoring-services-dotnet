// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
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
        private readonly string serviceUri;
        private readonly int maxRetries;

        public DiagnosticsClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.DiagnosticsApiUrl;
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
            request.SetUriFromString($"{this.serviceUri}/diagnosticsevents");
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
                    await this.httpClient.PostAsync(request);
                    requestSucceeded = true;
                }
                catch (Exception e)
                {
                    retries++;
                    if (retries < this.maxRetries)
                    {
                        this.log.Warn("Failed to log diagnostics log, retrying", () => new { e.Message });
                    }
                    else
                    {
                        this.log.Error("Failed to log diagnostics log, reached max retries and will not log", () => new { e.Message });
                    }
                }
            }
        }
    }
}
