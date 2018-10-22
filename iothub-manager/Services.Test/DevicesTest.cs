// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;
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
                // Act & Assert
                await Assert.ThrowsAsync<InvalidInputException>(async () =>
                    await this.devices.GetModuleTwinAsync(deviceId, moduleId));
            }
            else
            {
                // Arrange
                this.registryMock
                    .Setup(x => x.GetTwinAsync(deviceId, moduleId))
                    .ReturnsAsync(DevicesTest.CreateTestTwin(0));

                // Act
                var twinSvcModel = await this.devices.GetModuleTwinAsync(deviceId, moduleId);

                // Assert
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
            // Arrange
            this.registryMock
                .Setup(x => x.CreateQuery(It.IsAny<string>()))
                .Returns(new ResultQuery(numResults));

            // Act
            var queryResult = await this.devices.GetModuleTwinsByQueryAsync("", continuationToken);

            // Assert
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
            // Arrange
            this.registryMock
                .Setup(x => x.CreateQuery(queryToMatch))
                .Returns(new ResultQuery(3));

            // Act
            var queryResult = await this.devices.GetModuleTwinsByQueryAsync(query, "");

            // Assert
            Assert.Equal("continuationToken", queryResult.ContinuationToken);
            Assert.Equal(3, queryResult.Items.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetEdgeDeviceTest()
        {
            // Arrange
            var nonEdgeDevice = "nonEdgeDevice";
            var edgeDevice = "edgeDevice";
            var edgeDeviceFromTwin = "edgeDeviceFromTwin";

            this.registryMock
                .Setup(x => x.GetTwinAsync(nonEdgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestTwin(0));
            this.registryMock
                .Setup(x => x.GetTwinAsync(edgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestTwin(1));
            this.registryMock
                .Setup(x => x.GetTwinAsync(edgeDeviceFromTwin))
                .ReturnsAsync(DevicesTest.CreateTestTwin(2, true));

            this.registryMock
                .Setup(x => x.GetDeviceAsync(nonEdgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestDevice(false));
            this.registryMock
                .Setup(x => x.GetDeviceAsync(edgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestDevice(true));
            this.registryMock
                .Setup(x => x.GetDeviceAsync(edgeDeviceFromTwin))
                .ReturnsAsync(DevicesTest.CreateTestDevice(false));

            // Act
            var dvc1 = await this.devices.GetAsync(nonEdgeDevice);
            var dvc2 = await this.devices.GetAsync(edgeDevice);
            var dvc3 = await this.devices.GetAsync(edgeDeviceFromTwin);

            // Assert
            Assert.False(dvc1.IsEdgeDevice, "Non-edge device reporting edge device");
            Assert.True(dvc2.IsEdgeDevice, "Edge device reported not edge device");

            // When using getDevices method which is deprecated it doesn't return IsEdgeDevice
            // capabilities properly so we support grabbing this from the device twin as well.
            Assert.True(dvc3.IsEdgeDevice, "Edge device from twin reporting not edge device");
        }

        private static Twin CreateTestTwin(int valueToReport, bool isEdgeDevice = false)
        {
            var twin = new Twin()
            {
                Properties = new TwinProperties(),
                Capabilities = isEdgeDevice ? new DeviceCapabilities() { IotEdge = true } : null
            };
            twin.Properties.Reported = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            twin.Properties.Desired = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            return twin;
        }

        private static Device CreateTestDevice(bool isEdgeDevice)
        {
            var dvc = new Device
            {
                Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.Sas,
                    SymmetricKey = new SymmetricKey
                    {
                        PrimaryKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("SomeTestPrimaryKey")),
                        SecondaryKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("SomeTestSecondaryKey"))
                    }
                },
                Capabilities = isEdgeDevice ? new DeviceCapabilities() {IotEdge = true} : null
            };
            return dvc;
        }
    }
}
