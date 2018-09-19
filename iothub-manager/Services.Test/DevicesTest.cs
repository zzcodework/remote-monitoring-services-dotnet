// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DevicesTest
    {
        private readonly IDevices devices;
        private readonly Mock<RegistryManager> registryMock;
        private readonly string ioTHubHostName = "ioTHubHostName";

        public DevicesTest()
        {
            this.registryMock = new Mock<RegistryManager>();
            this.devices = new Devices(registryMock.Object, this.ioTHubHostName);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "", true)]
        [InlineData("asdf", "", true)]
        [InlineData("", "qwer", true)]
        [InlineData("asdf", "qwer", false)]
        public async Task GetModuleTwinTest(string deviceId, string moduleId, bool throwsException)
        {
            if (throwsException)
            {
                await Assert.ThrowsAsync<InvalidInputException>(async () =>
                    await this.devices.GetModuleTwinAsync(deviceId, moduleId));
            }
            else
            {
                this.registryMock
                    .Setup(x => x.GetTwinAsync(deviceId, moduleId))
                    .ReturnsAsync(DevicesTest.CreateTestTwin(0));

                var twinSvcModel = await this.devices.GetModuleTwinAsync(deviceId, moduleId);
                Assert.Equal("value0", twinSvcModel.ReportedProperties["test"].ToString());
                Assert.Equal("value0", twinSvcModel.DesiredProperties["test"].ToString());
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", 5)]
        [InlineData("2", 5)]
        [InlineData("6", 5)]
        public async Task TwinByQueryContinuationTest(string continuationToken, int numResults)
        {
            this.registryMock
                .Setup(x => x.CreateQuery(It.IsAny<string>()))
                .Returns(new ResultQuery(numResults));

            var queryResult = await this.devices.GetModuleTwinsByQueryAsync("", continuationToken);
            Assert.Equal("continuationToken", queryResult.ContinuationToken);

            var startIndex = string.IsNullOrEmpty(continuationToken) ? 0 : int.Parse(continuationToken);
            var total = Math.Max(0, numResults - startIndex);
            Assert.Equal(total, queryResult.Items.Count);

            for (int i = 0; i < total; i++)
            {
                var expectedValue = "value" + (i + startIndex);
                Assert.Equal(expectedValue, queryResult.Items[i].ReportedProperties["test"].ToString());
                Assert.Equal(expectedValue, queryResult.Items[i].DesiredProperties["test"].ToString());
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "SELECT * FROM devices.modules")]
        [InlineData("deviceId='test'", "SELECT * FROM devices.modules where deviceId='test'")]
        public async Task GetTwinByQueryTest(string query, string queryToMatch)
        {
            this.registryMock
                .Setup(x => x.CreateQuery(queryToMatch))
                .Returns(new ResultQuery(3));

            var queryResult = await this.devices.GetModuleTwinsByQueryAsync(query, "");
            Assert.Equal("continuationToken", queryResult.ContinuationToken);
            Assert.Equal(3, queryResult.Items.Count);
        }

        private static Twin CreateTestTwin(int valueToReport)
        {
            var twin = new Twin()
            {
                Properties = new TwinProperties()
            };
            twin.Properties.Reported = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            twin.Properties.Desired = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            return twin;
        }
    }
}
