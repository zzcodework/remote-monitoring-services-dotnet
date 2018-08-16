// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class TimeSeriesClientTest
    {
        private readonly Mock<ILogger> logger;
        private readonly Mock<IServicesConfig> servicesConfig;
        private readonly Mock<IHttpClient> httpClient;
        private readonly TimeSeriesClient client;

        public TimeSeriesClientTest()
        {
            this.logger = new Mock<ILogger>();
            this.servicesConfig = new Mock<IServicesConfig>();
            this.httpClient = new Mock<IHttpClient>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.servicesConfig.Object,
                this.logger.Object);
        }
    }
}
