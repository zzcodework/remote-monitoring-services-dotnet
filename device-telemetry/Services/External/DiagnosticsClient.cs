// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External
{
    public interface IDiagnosticsClient
    {
        void LogEventAsync(string eventName);
    }

    public class DiagnosticsClient: IDiagnosticsClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;

        public DiagnosticsClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.DiagnosticsApiUrl;
        }

        public async void LogEventAsync(string eventName)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/diagnosticsevents");
            DiagnosticsRequestModel model = new DiagnosticsRequestModel
            {
                EventType = eventName
            };
            request.SetContent(JsonConvert.SerializeObject(model));
            await this.httpClient.PostAsync(request);
        }
    }
}
