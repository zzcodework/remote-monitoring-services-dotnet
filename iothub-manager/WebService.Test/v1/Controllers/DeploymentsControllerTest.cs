// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{

    public class DeploymentsControllerTest
    {
        private readonly DeploymentsController deploymentsController;
        private readonly Mock<IDeployments> deploymentsMock;
        private const string DEPLOYMENT_NAME = "depname";
        private const string DEVICE_GROUP_ID = "dvcGroupId";
        private const string PACKAGE_ID = "packageId";
        private const string DEPLOYMENT_ID = "dvcGroupId--packageId";
        private const int PRIORITY = 10;


        public DeploymentsControllerTest()
        {
            this.deploymentsMock = new Mock<IDeployments>();
            this.deploymentsController = new DeploymentsController(this.deploymentsMock.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetDeploymentTest()
        {
            // Arrange
            this.deploymentsMock.Setup(x => x.GetAsync(DEPLOYMENT_ID)).ReturnsAsync(new DeploymentServiceModel()
            {
                Name = DEPLOYMENT_NAME,
                DeviceGroupId = DEVICE_GROUP_ID,
                PackageId = PACKAGE_ID,
                Priority = PRIORITY,
                Id = DEPLOYMENT_ID,
                Type = DeploymentType.EdgeManifest,
                CreatedDateTimeUtc = DateTime.UtcNow
            });

            // Act
            var result = await this.deploymentsController.GetAsync(DEPLOYMENT_ID);

            // Assert
            Assert.Equal(DEPLOYMENT_ID, result.DeploymentId);
            Assert.Equal(DEPLOYMENT_NAME, result.Name);
            Assert.Equal(PACKAGE_ID, result.PackageId);
            Assert.Equal(DEVICE_GROUP_ID, result.DeviceGroupId);
            Assert.Equal(PRIORITY, result.Priority);
            Assert.Equal(DeploymentType.EdgeManifest, result.Type);
            Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetDeploymentsTest(int numDeployments)
        {
            // Arrange
            var deploymentsList = new List<DeploymentServiceModel>();
            for (var i = 0; i < numDeployments; i++)
            {
                deploymentsList.Add(new DeploymentServiceModel()
                {
                    Name = DEPLOYMENT_NAME + i,
                    DeviceGroupId = DEVICE_GROUP_ID + i,
                    PackageId = PACKAGE_ID + i,
                    Priority = PRIORITY + i,
                    Id = DEPLOYMENT_ID + i,
                    Type = DeploymentType.EdgeManifest,
                    CreatedDateTimeUtc = DateTime.UtcNow
                });
            }

            this.deploymentsMock.Setup(x => x.GetAsync()).ReturnsAsync(
                new DeploymentServiceListModel(deploymentsList)
            );

            // Act
            var results = await this.deploymentsController.GetAsync();

            // Assert
            Assert.Equal(numDeployments, results.Items.Count);
            for (var i = 0; i < numDeployments; i++)
            {
                var result = results.Items[i];
                Assert.Equal(DEPLOYMENT_ID + i, result.DeploymentId);
                Assert.Equal(DEPLOYMENT_NAME + i, result.Name);
                Assert.Equal(PACKAGE_ID + i, result.PackageId);
                Assert.Equal(DEVICE_GROUP_ID + i, result.DeviceGroupId);
                Assert.Equal(PRIORITY + i, result.Priority);
                Assert.Equal(DeploymentType.EdgeManifest, result.Type);
                Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depName", "dvcGroupId", "pkgId", 10, false)]
        [InlineData("", "dvcGroupId", "pkgId", 10, true)]
        [InlineData("depName", "", "pkgId", 10, true)]
        [InlineData("depName", "dvcGroupId", "", 10, true)]
        [InlineData("depName", "dvcGroupId", "pkgId", -1, true)]
        public async Task PostDeploymentTest(string name, string deviceGroupId,
                                             string packageId, int priority,
                                             bool throwsException)
        {
            // Arrange
            this.deploymentsMock.Setup(x => x.CreateAsync(
                                        Match.Create<DeploymentServiceModel>(model =>
                                            model.DeviceGroupId == deviceGroupId &&
                                            model.PackageId == packageId &&
                                            model.Priority == priority &&
                                            model.Name == name &&
                                            model.Type == DeploymentType.EdgeManifest)))
                                .ReturnsAsync(new DeploymentServiceModel()
            {
                Name = name,
                DeviceGroupId = deviceGroupId,
                PackageId = packageId,
                Priority = priority,
                Id = $"{deviceGroupId}--{packageId}",
                Type = DeploymentType.EdgeManifest,
                CreatedDateTimeUtc = DateTime.UtcNow
            });

            try
            {
                // Act
                var depApiModel = new DeploymentApiModel()
                {
                    Name = name,
                    DeviceGroupId = deviceGroupId,
                    PackageId = packageId,
                    Type = DeploymentType.EdgeManifest,
                    Priority = priority
                };
                var result = await this.deploymentsController.PostAsync(depApiModel);

                // Assert
                Assert.False(throwsException);
                Assert.Equal($"{deviceGroupId}--{packageId}", result.DeploymentId);
                Assert.Equal(name, result.Name);
                Assert.Equal(packageId, result.PackageId);
                Assert.Equal(deviceGroupId, result.DeviceGroupId);
                Assert.Equal(priority, result.Priority);
                Assert.Equal(DeploymentType.EdgeManifest, result.Type);
                Assert.True((DateTimeOffset.UtcNow - result.CreatedDateTimeUtc).TotalSeconds < 5);
            }
            catch (Exception ex)
            {
                Assert.True(throwsException, ex.Message);
            }
        }
    }
}
