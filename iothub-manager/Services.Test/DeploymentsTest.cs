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
        [InlineData("depname", "dvcgroupid", "packageid", 10, false)]
        [InlineData("", "dvcgroupid", "packageid", 10, true)]
        [InlineData("depname", "", "packageid", 10, true)]
        [InlineData("depname", "dvcgroupid", "", 10, true)]
        [InlineData("depname", "dvcgroupid", "packageid", -2, true)]
        public async Task CreateDeploymentTest(string deploymentName, string deviceGroupId,
                                               string packageId, int priority,
                                               bool expectException)
        {
            string deploymentId = $"{deviceGroupId}--{packageId}";
            var depModel = new DeploymentServiceModel()
            {
                DeviceGroupId = deviceGroupId,
                PackageId = packageId,
                Priority = priority,
                Name = deploymentName
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
                                               Id = deviceGroupId,
                                               DisplayName = deviceGroupId + "Name",
                                               ETag = deviceGroupId + "Etag"
                                           });

            var newConfig = new Configuration($"{deviceGroupId}--{packageId}")
            {
                Labels = new Dictionary<string, string>() {{"Name", deploymentName }},
                Priority = priority
            };

            this.registry.Setup(r => r.AddConfigurationAsync(
                    It.Is<Configuration>(c => c.Id == deploymentId)))
                         .ReturnsAsync(newConfig);

            try
            {
                var createdDeployment = await this.deployments.CreateAsync(depModel);
                Assert.False(expectException);
                Assert.Equal(deploymentId, createdDeployment.Id);
                Assert.Equal(deploymentName, createdDeployment.Name);
                Assert.Equal(packageId, createdDeployment.PackageId);
                Assert.Equal(deviceGroupId, createdDeployment.DeviceGroupId);
                Assert.Equal(priority, createdDeployment.Priority);
            }
            catch (Exception)
            {
                Assert.True(expectException);
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        public async Task GetDeploymentTest(bool validDeploymentId, bool createdByRm, bool throwsException)
        {
            var deploymentId = validDeploymentId ? "test--config" : "";
            var configuration = this.CreateConfiguration(deploymentId, 0, createdByRm);

            this.registry.Setup(r => r.GetConfigurationAsync(deploymentId))
                .ReturnsAsync(configuration);

            try
            {
                var returnedDeployment = await this.deployments.GetAsync(deploymentId);
                Assert.False(throwsException);
                Assert.Equal("test--config0", returnedDeployment.Id);
                Assert.Equal("deployment0", returnedDeployment.Name);
                Assert.Equal("config0", returnedDeployment.PackageId);
                Assert.Equal("test", returnedDeployment.DeviceGroupId);
                Assert.Equal(10, returnedDeployment.Priority);
            }
            catch (Exception)
            {
                Assert.True(throwsException);
            }
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
                configurations.Add(this.CreateConfiguration("test--config", i, true));
            }

            this.registry.Setup(r => r.GetConfigurationsAsync(20))
                         .ReturnsAsync(configurations);

            var returnedDeployments = await this.deployments.GetAsync();
            Assert.Equal(numDeployments, returnedDeployments.Items.Count);

            // verify deployments are ordered by name
            for (int i = 0; i < numDeployments; i++)
            {
                Assert.Equal("deployment" + i, returnedDeployments.Items[i].Name);
            }
        }


        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task FilterOutNonRmDeploymentsTest()
        {
            var configurations = new List<Configuration>();
            configurations.Add(this.CreateConfiguration("test--config", 0, true));
            configurations.Add(this.CreateConfiguration("nonrm--config", 1, false));

            this.registry.Setup(r => r.GetConfigurationsAsync(20))
                .ReturnsAsync(configurations);

            var returnedDeployments = await this.deployments.GetAsync();
            Assert.Single(returnedDeployments.Items);
            Assert.StartsWith("test--config", returnedDeployments.Items[0].Id);
        }

        private Configuration CreateConfiguration(string id, int idx, bool addCreatedByRmLabel)
        {
            var conf = new Configuration(id + idx)
            {
                Labels = new Dictionary<string, string>()
                {
                    {"Name", "deployment" + idx}
                },
                Priority = 10
            };

            if (addCreatedByRmLabel)
            {
                conf.Labels.Add("RMDeployment", "true");
            }

            return conf;
        }
    }
}
