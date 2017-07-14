// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using WebService.Test.helpers;
using WebService.Test.helpers.Http;
using Xunit;
using Xunit.Abstractions;

namespace WebService.Test.IntegrationTests
{
    public class DevicesStatusTest
    {
        private readonly ITestOutputHelper log;
        private readonly HttpClient httpClient;

        // Pull Request don't have access to secret credentials, which are
        // required to run tests interacting with Azure IoT Hub.
        // The tests should run when working locally and when merging branches.
        private readonly bool credentialsAvailable;

        public DevicesStatusTest(ITestOutputHelper log)
        {
            this.log = log;
            this.httpClient = new HttpClient(this.log);
            this.credentialsAvailable = !CIVariableHelper.IsPullRequest(this.log);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/devices");

            // Act
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceByIdIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/devices/foobar");

            // Act
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
