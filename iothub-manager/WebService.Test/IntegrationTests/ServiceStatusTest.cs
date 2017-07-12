// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService;
using System.Net;
using WebService.Test.helpers;
using WebService.Test.helpers.Http;
using Xunit;
using Xunit.Abstractions;

namespace WebService.Test.IntegrationTests
{
    public class ServiceStatusTest : System.IDisposable
    {
        private readonly ITestOutputHelper log;
        private readonly HttpClient httpClient;
        private readonly string root;
        private readonly IWebHost host;

        public ServiceStatusTest(ITestOutputHelper log)
        {
            this.log = log;
            this.httpClient = new HttpClient(this.log);
            root = WebServiceHost.GetBaseAddress();
            host = new WebHostBuilder()
                .UseUrls(root)
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Start();
        }

        [Fact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void TheServiceIsHealthy()
        {
            var request = new HttpRequest();
            request.SetUriFromString(root + "/v1/status");

            // Act
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
