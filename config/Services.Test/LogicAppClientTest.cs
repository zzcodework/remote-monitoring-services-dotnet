// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;
using IUserManagementClient = Microsoft.Azure.IoTSolutions.UIConfig.Services.External.IUserManagementClient;

namespace Services.Test
{
    public class LogicAppClientTest
    {
        private const string MOCK_SUBSCRIPTION_ID = @"123456abcd";
        private const string MOCK_RESOURCE_GROUP = @"example-name";
        private readonly string logicAppTestConnectionUrl;

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly Mock<IUserManagementClient> mockUserManagementClinet;

        private readonly LogicAppClient client;

        public LogicAppClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.mockUserManagementClinet = new Mock<IUserManagementClient>();
            this.client = new LogicAppClient(
                this.mockHttpClient.Object,
                new ServicesConfig
                {
                    SubscriptionId = MOCK_SUBSCRIPTION_ID,
                    ResourceGroup = MOCK_RESOURCE_GROUP
                },
                this.mockUserManagementClinet.Object);

            this.logicAppTestConnectionUrl = "https://management.azure.com/" +
                                        $"subscriptions/{MOCK_SUBSCRIPTION_ID}/" +
                                        $"resourceGroups/{MOCK_RESOURCE_GROUP}/" +
                                        "providers/Microsoft.Web/connections/" +
                                        "office365-connector/extensions/proxy/testconnection?" +
                                        "api-version=2016-06-01";
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetOffice365IsEnabled_ReturnsTrueIfEnabled()
        {
            // Arrange
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await this.client.Office365IsEnabledAsync();
            
            // Assert
            this.mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check(
                    this.logicAppTestConnectionUrl))), Times.Once);

            Assert.True(result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetOffice365IsEnabled_ReturnsFalseIfDisabled()
        {
            // Arrange
            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.GetAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await this.client.Office365IsEnabledAsync();

            // Assert
            this.mockHttpClient
                .Verify(x => x.GetAsync(
                    It.Is<IHttpRequest>(r => r.Check(
                    this.logicAppTestConnectionUrl))), Times.Once);

            Assert.False(result);
        }
    }
}
