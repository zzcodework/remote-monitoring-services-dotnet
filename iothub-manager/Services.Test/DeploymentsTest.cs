// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
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
        private readonly Mock<IPackageManagementClient> packageClient;
        private readonly Mock<IDeviceGroupsClient> deviceGroups;
        private readonly Mock<RegistryManager> registry;

        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_PACKAGE_ID_LABEL = "PackageId";
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
            this.packageClient = new Mock<IPackageManagementClient>();
            this.deviceGroups = new Mock<IDeviceGroupsClient>();
            this.registry = new Mock<RegistryManager>();
            this.deployments = new Deployments(this.deviceGroups.Object,
                                               this.packageClient.Object,
                                               this.registry.Object,
                                               "mockIoTHub");
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("depname", "dvcgroupid", "packageid", 10, "")]
        [InlineData("", "dvcgroupid", "packageid", 10, "System.ArgumentNullException")]
        [InlineData("depname", "", "packageid", 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "", 10, "System.ArgumentNullException")]
        [InlineData("depname", "dvcgroupid", "packageid", -2, "System.ArgumentOutOfRangeException")]
        public async Task CreateDeploymentTest(string deploymentName, string deviceGroupId,
                                               string packageId, int priority,
                                               string expectedException)
        {
            var depModel = new DeploymentServiceModel()
            {
                Name = deploymentName,
                DeviceGroupId = deviceGroupId,
                PackageId = packageId,
                Priority = priority
            };

            this.packageClient.Setup(p => p.GetPackageAsync(It.Is<string>(s => s == packageId)))
                .ReturnsAsync(new PackageApiModel()
                {
                    Id = packageId,
                    Name = packageId + "Name",
                    Type = PackageType.EdgeManifest,
                    Content = TEST_PACKAGE_JSON
                });

            this.deviceGroups.Setup(d => d.GetDeviceGroupsAsync(It.Is<string>(s => s == deviceGroupId)))
                .ReturnsAsync(new DeviceGroupApiModel()
                {
                    Id = deviceGroupId, DisplayName = deviceGroupId + "Name", ETag = deviceGroupId + "Etag"
                });

            var newConfig = new Configuration("test-config")
            {
                Labels = new Dictionary<string, string>()
                {
                    { DEPLOYMENT_NAME_LABEL, deploymentName },
                    { DEPLOYMENT_GROUP_ID_LABEL, deviceGroupId },
                    { DEPLOYMENT_PACKAGE_ID_LABEL, packageId },
                    { RM_CREATED_LABEL, Boolean.TrueString },
                }, Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(It.Is<Configuration>(c =>
                    c.Labels.ContainsKey(DEPLOYMENT_NAME_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_GROUP_ID_LABEL) &&
                    c.Labels.ContainsKey(DEPLOYMENT_PACKAGE_ID_LABEL) &&
                    c.Labels[DEPLOYMENT_NAME_LABEL] == deploymentName &&
                    c.Labels[DEPLOYMENT_GROUP_ID_LABEL] == deviceGroupId &&
                    c.Labels[DEPLOYMENT_PACKAGE_ID_LABEL] == packageId)))
                .ReturnsAsync(newConfig);

            if (string.IsNullOrEmpty(expectedException))
            {
                var createdDeployment = await this.deployments.CreateAsync(depModel);
                Assert.False(string.IsNullOrEmpty(createdDeployment.Id));
                Assert.Equal(deploymentName, createdDeployment.Name);
                Assert.Equal(packageId, createdDeployment.PackageId);
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
            var configurations = new List<Configuration>();
            for (int i = numDeployments - 1; i >= 0; i--)
            {
                configurations.Add(this.CreateConfiguration(i, true));
            }

            this.registry.Setup(r => r.GetConfigurationsAsync(20)).ReturnsAsync(configurations);

            var returnedDeployments = await this.deployments.ListAsync();
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
            var configuration = this.CreateConfiguration(0, true);
            var deploymentId = configuration.Id;
            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId)).ReturnsAsync(configuration);

            IQuery queryResult = new ResultQuery(3);
            this.registry.Setup(r => r.CreateQuery(It.IsAny<string>())).Returns(queryResult);

            var returnedDeployment = await this.deployments.GetAsync(deploymentId);
            var deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceWithStatus;
            Assert.Null(deviceStatuses);

            returnedDeployment = await this.deployments.GetAsync(deploymentId, true);
            deviceStatuses = returnedDeployment.DeploymentMetrics.DeviceWithStatus;
            Assert.Equal(3, deviceStatuses.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task FilterOutNonRmDeploymentsTest()
        {
            var configurations = new List<Configuration>
            {
                this.CreateConfiguration(0, true),
                this.CreateConfiguration(1, false)
            };

            this.registry.Setup(r => r.GetConfigurationsAsync(20))
                .ReturnsAsync(configurations);

            var returnedDeployments = await this.deployments.ListAsync();
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
                    { DEPLOYMENT_GROUP_ID_LABEL, "dvcGroupId" + idx },
                    { DEPLOYMENT_PACKAGE_ID_LABEL, "packageId" + idx }
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
