// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService;
using Microsoft.Owin.Hosting;
using WebService.Test.helpers;
using WebService.Test.helpers.Http;
using Xunit;
using Xunit.Abstractions;

namespace WebService.Test.IntegrationTests
{
    public class ServiceStatusTest
    {
        private readonly ITestOutputHelper log;
        private readonly HttpClient httpClient;

        public ServiceStatusTest(ITestOutputHelper log)
        {
            this.log = log;
            this.httpClient = new HttpClient(this.log);
        }

        [Fact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void TheServiceIsHealthy()
        {
            // Arrange
            var root = "http://127.0.0.1:30080";
            var options = new StartOptions(root);
            using (WebApp.Start<Startup>(options))
            {
                var request = new HttpRequest();
                request.SetUriFromString(root + "/v1/status");

                // Act
                var response = this.httpClient.GetAsync(request).Result;

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
