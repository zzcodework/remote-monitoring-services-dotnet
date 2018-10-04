// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Moq;
using Xunit;
using System.Threading.Tasks;
using Services.Test.helpers;
using Microsoft.Azure.Devices;

namespace Services.Test
{
    public class DeploymentsTest
    {
        private readonly Deployments deployments;
        private readonly Mock<RegistryManager> registry;

        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        private const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        private const string RM_CREATED_LABEL = "RMDeployment";
        private const string RESOURCE_NOT_FOUND_EXCEPTION =
            "Microsoft.Azure.IoTSolutions.IotHubManager.Services." +
            "Exceptions.ResourceNotSupportedException, Microsoft.Azure." + 
            "IoTSolutions.IotHubManager.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        private const string TEST_PACKAGE_JSON =
                @"{
                    ""id"": ""tempid"",
                    ""schemaVersion"": ""1.0"",
                    ""content"": {
                        ""modulesContent"": {
                        ""$edgeAgent"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""runtime"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                ""loggingOptions"": """",
                                ""minDockerVersion"": ""v1.25""
                                }
                            },
                            ""systemModules"": {
                                ""edgeAgent"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-agent:1.0"",
                                    ""createOptions"": ""{}""
                                }
                                },
                                ""edgeHub"": {
                                ""type"": ""docker"",
                                ""settings"": {
                                    ""image"": ""mcr.microsoft.com/azureiotedge-hub:1.0"",
                                    ""createOptions"": ""{}""
                                },
                                ""status"": ""running"",
                                ""restartPolicy"": ""always""
                                }
                            },
                            ""modules"": {}
                            }
                        },
                        ""$edgeHub"": {
                            ""properties.desired"": {
                            ""schemaVersion"": ""1.0"",
                            ""routes"": {
                                ""route"": ""FROM /messages/* INTO $upstream""
                            },
                            ""storeAndForwardConfiguration"": {
                                ""timeToLiveSecs"": 7200
                            }
                            }
                        }
                        }
                    },
                    ""targetCondition"": ""*"",
                    ""priority"": 30,
                    ""labels"": {
                        ""Name"": ""Test""
                    },
                    ""createdTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""lastUpdatedTimeUtc"": ""2018-08-20T18:05:55.482Z"",
                    ""etag"": null,
                    ""metrics"": {
                        ""results"": {},
                        ""queries"": {}
                    }
                 }";

        public DeploymentsTest()
        {
            this.registry = new Mock<RegistryManager>();
            this.deployments = new Deployments(this.registry.Object,
                                               "mockIoTHub");
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depname", "dvcgroupid", "dvcquery", TEST_PACKAGE_JSON, 10, "")]
        [InlineData("", "dvcgroupid", "dvcquery", TEST_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "", "dvcquery", TEST_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "", TEST_PACKAGE_JSON, 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "dvcquery", "", 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "dvcquery", TEST_PACKAGE_JSON, -1, "System.ArgumentOutOfRangeException")]
        public async Task CreateDeploymentTest(string deploymentName, string deviceGroupId,
                                               string deviceGroupQuery, string packageContent,
                                               int priority, string expectedException)
        {
            // Arrange
            var depModel = new DeploymentServiceModel()
            {
                Name = deploymentName,
                DeviceGroupId = deviceGroupId,
                DeviceGroupQuery = deviceGroupQuery,
                PackageContent = packageContent,
                Priority = priority
            };

            var newConfig = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, deploymentName },
                    { DEPLOYMENT_GROUP_ID_LABEL, deviceGroupId },
                    { RM_CREATED_LABEL, bool.TrueString },
                }, Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(It.Is<Configuration>(c =>
                    c.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL) &&
                    c.Labels.ContainsKey(RM_CREATED_LABEL) &&
                    c.Labels[DEPLOYMENT_NAME_LABEL] == deploymentName &&
                    c.Labels[DEPLOYMENT_GROUP_ID_LABEL] == deviceGroupId &&
                    c.Labels[RM_CREATED_LABEL] == bool.TrueString)))
                .ReturnsAsync(newConfig);

            // Act
            if (string.IsNullOrEmpty(expectedException))
            {
                var createdDeployment = await this.deployments.CreateAsync(depModel);

                // Assert
                Assert.False(string.IsNullOrEmpty(createdDeployment.Id));
                Assert.Equal(deploymentName, createdDeployment.Name);
                Assert.Equal(deviceGroupId, createdDeployment.DeviceGroupId);
                Assert.Equal(priority, createdDeployment.Priority);
            }
            else
            {
                await Assert.ThrowsAsync(Type.GetType(expectedException),
                    async () => await this.deployments.CreateAsync(depModel));
            }
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InvalidRmConfigurationTest()
        {
            var configuration = this.CreateConfiguration(0, false);

            this.registry.Setup(r => r.GetConfigurationAsync(It.IsAny<string>()))
                .ReturnsAsync(configuration);

            await Assert.ThrowsAsync(Type.GetType(RESOURCE_NOT_FOUND_EXCEPTION),
                    async () => await this.deployments.GetAsync(configuration.Id));
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetDeploymentsTest(int numDeployments)
        {
            // Arrange
            var configurations = new List<Configuration>();
            for (int i = numDeployments - 1; i >= 0; i--)
            {
                configurations.Add(this.CreateConfiguration(i, true));
            }

            this.registry.Setup(r => r.GetConfigurationsAsync(20)).ReturnsAsync(configurations);

            // Act
            var returnedDeployments = await this.deployments.ListAsync();

            // Assert
            Assert.Equal(numDeployments, returnedDeployments.Items.Count);

            // verify deployments are ordered by name
            for (int i = 0; i < numDeployments; i++)
            {
                Assert.Equal("deployment" + i, returnedDeployments.Items[i].Name);
            }
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetDeploymentsWithDeviceStatusTest()
        {
            // Arrange
            var configuration = this.CreateConfiguration(0, true);
            var deploymentId = configuration.Id;
            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId)).ReturnsAsync(configuration);

            IQuery queryResult = new ResultQuery(3);
            this.registry.Setup(r => r.CreateQuery(It.IsAny<string>())).Returns(queryResult);

            // Act
            var returnedDeployment = await this.deployments.GetAsync(deploymentId);
            var deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceStatuses;

            // Assert
            Assert.Null(deviceStatuses);

            returnedDeployment = await this.deployments.GetAsync(deploymentId, true);
            deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceStatuses;
            Assert.Equal(3, deviceStatuses.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task VerifyGroupAndPackageNameLabelsTest()
        {
            // Arrange
            var deviceGroupId = "dvcGroupId";
            var deviceGroupName = "dvcGroupName";
            var deviceGroupQuery = "dvcGroupQuery";
            var packageName = "packageName";
            var deploymentName = "depName";
            var priority = 10;
            var depModel = new DeploymentServiceModel()
            {
                Name = deploymentName,
                DeviceGroupId = deviceGroupId,
                DeviceGroupName = deviceGroupName,
                DeviceGroupQuery = deviceGroupQuery,
                PackageContent = TEST_PACKAGE_JSON,
                PackageName = packageName,
                Priority = priority
            };

            var newConfig = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, deploymentName },
                    { DEPLOYMENT_GROUP_ID_LABEL, deviceGroupId },
                    { RM_CREATED_LABEL, bool.TrueString },
                    { DEPLOYMENT_GROUP_NAME_LABEL, deviceGroupName },
                    { DEPLOYMENT_PACKAGE_NAME_LABEL, packageName }
                },
                Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(It.Is<Configuration>(c =>
                    c.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL) &&
                    c.Labels.ContainsKey(RM_CREATED_LABEL) &&
                    c.Labels[DEPLOYMENT_NAME_LABEL] == deploymentName &&
                    c.Labels[DEPLOYMENT_GROUP_ID_LABEL] == deviceGroupId &&
                    c.Labels[RM_CREATED_LABEL] == bool.TrueString)))
                .ReturnsAsync(newConfig);

            var createdDeployment = await this.deployments.CreateAsync(depModel);

            // Assert
            Assert.False(string.IsNullOrEmpty(createdDeployment.Id));
            Assert.Equal(deploymentName, createdDeployment.Name);
            Assert.Equal(deviceGroupId, createdDeployment.DeviceGroupId);
            Assert.Equal(priority, createdDeployment.Priority);
            Assert.Equal(deviceGroupName, createdDeployment.DeviceGroupName);
            Assert.Equal(packageName, createdDeployment.PackageName);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task FilterOutNonRmDeploymentsTest()
        {
            // Arrange
            var configurations = new List<Configuration>
            {
                this.CreateConfiguration(0, true),
                this.CreateConfiguration(1, false)
            };

            this.registry.Setup(r => r.GetConfigurationsAsync(20))
                .ReturnsAsync(configurations);

            // Act
            var returnedDeployments = await this.deployments.ListAsync();

            // Assert
            Assert.Single(returnedDeployments.Items);
            Assert.Equal("deployment0", returnedDeployments.Items[0].Name);
        }

        private Configuration CreateConfiguration(int idx, bool addCreatedByRmLabel)
        {
            var conf = new Configuration("test-config"+idx)
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, "deployment" + idx },
                    { DEPLOYMENT_GROUP_ID_LABEL, "dvcGroupId" + idx }
                }, Priority = 10
            };

            if (addCreatedByRmLabel)
            {
                conf.Labels.Add(RM_CREATED_LABEL, "true");
            }

            return conf;
        }
    }
}
