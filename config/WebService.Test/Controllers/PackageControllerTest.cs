// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers;
using Moq;
using WebService.Test.helpers;
using Xunit;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;

namespace WebService.Test.Controllers
{
    public class PackageControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly PackageController controller;
        private readonly Random rand;
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        public PackageControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.controller = new PackageController(this.mockStorage.Object);
            this.rand = new Random();
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("EdgeManifest", "filename", true, false)]
        [InlineData("EdgeManifest", "filename", false, true)]
        [InlineData("EdgeManifest", "", true, true)]
        [InlineData("BAD_TYPE", "filename", true, true)]
        public async Task PostAsyncExceptionVerificationTest(string type, string filename,
                                                             bool isValidFileProvided, bool expectException)
        {
            // Arrange
            IFormFile file = null;
            if (isValidFileProvided)
            {
                file = this.CreateSampleFile(filename);
            }

            this.mockStorage.Setup(x => x.AddPackageAsync(
                                    It.Is<Package>(p => p.packageType.ToString().Equals(type) &&
                                                        p.Name.Equals(filename))))
                            .ReturnsAsync(new Package() {
                                Name = filename,
                                packageType = PackageType.EdgeManifest
                            });
            try
            {
                // Act
                var package = await this.controller.PostAsync(type, null, file);

                // Assert
                Assert.False(expectException);
                Assert.Equal(filename, package.Name);
                Assert.Equal(type, package.packageType.ToString());
            }
            catch (Exception)
            {
                Assert.True(expectException);
            }
        }

        [Fact]
        public async Task GetPackageTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.EdgeManifest;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DATE_FORMAT);

            this.mockStorage
                .Setup(x => x.GetPackageAsync(id))
                .ReturnsAsync(new Package()
                {
                    Id = id,
                    Name = name,
                    Content = content,
                    packageType = type,
                    ConfigType = string.Empty,
                    DateCreated = dateCreated
                });

            // Act
            var pkg = await this.controller.GetAsync(id);

            // Assert
            this.mockStorage
                .Verify(x => x.GetPackageAsync(id), Times.Once);

            Assert.Equal(id, pkg.Id);
            Assert.Equal(name, pkg.Name);
            Assert.Equal(type, pkg.packageType);
            Assert.Equal(content, pkg.Content);
            Assert.Equal(dateCreated, pkg.DateCreated);
        }

        [Fact]
        public async Task GetAllPackageTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.EdgeManifest;
            string config = string.Empty;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DATE_FORMAT);

            int[] idx = new int[] {0, 1, 2};
            var packages = idx.Select(i => new Package()
                                     {
                                         Id = id + i,
                                         Name = name + i,
                                         Content = content + i,
                                         packageType = type,
                                         ConfigType = config + i,
                                         DateCreated = dateCreated
                                     }).ToList();

            this.mockStorage
                .Setup(x => x.GetPackagesAsync())
                .ReturnsAsync(packages);

            // Act
            var resultPackages = await this.controller.GetAllAsync();

            // Assert
            this.mockStorage
                .Verify(x => x.GetPackagesAsync(), Times.Once);

            foreach (int i in idx)
            {
                var pkg = resultPackages.Items.ElementAt(i);
                Assert.Equal(id + i, pkg.Id);
                Assert.Equal(name + i, pkg.Name);
                Assert.Equal(type, pkg.packageType);
                Assert.Equal(content + i, pkg.Content);
                Assert.Equal(dateCreated, pkg.DateCreated);
            }
        }

        [Fact]
        public async Task ListPackagesTest()
        {
            // Arrange
            const string id = "packageId";
            const string name = "packageName";
            const PackageType type = PackageType.DeviceConfiguration;
            const string content = "{}";
            string dateCreated = DateTime.UtcNow.ToString(DATE_FORMAT);

            int[] idx = new int[] { 0, 1, 2 };
            var packages = idx.Select(i => new Package()
            {
                Id = id + i,
                Name = name + i,
                Content = content + i,
                packageType = type + i,
                ConfigType = (i == 0) ? ConfigType.FirmwareUpdateMxChip.ToString() : i.ToString(),
                DateCreated = dateCreated
            }).ToList();

            this.mockStorage
                .Setup(x => x.GetPackagesAsync())
                .ReturnsAsync(packages);

            // Act
            var resultPackages = await this.controller.GetAllAsync(
                                                    PackageType.DeviceConfiguration.ToString(),
                                                    ConfigType.FirmwareUpdateMxChip.ToString());

            // Assert
            this.mockStorage
                .Verify(x => x.GetPackagesAsync(), Times.Once);

            var pkg = resultPackages.Items.ElementAt(0);
            Assert.Single(resultPackages.Items);
            Assert.Equal(id + 0, pkg.Id);
            Assert.Equal(name + 0, pkg.Name);
            Assert.Equal(type, pkg.packageType);
            Assert.Equal(ConfigType.FirmwareUpdateMxChip.ToString(), pkg.ConfigType);
            Assert.Equal(content + 0, pkg.Content);
            Assert.Equal(dateCreated, pkg.DateCreated);
            
        }

        [Fact]
        public async Task GetConfigurationsTest()
        {
            // Arrange
            this.mockStorage
                .Setup(x => x.GetAllConfigurationsAsync())
                .ReturnsAsync(new ConfigTypeList());

            // Act
            var cfg = await this.controller.GetListAsync();

            // Assert
            Assert.Single(cfg.Items);
            Assert.Contains(ConfigType.FirmwareUpdateMxChip.ToString(), cfg.Items);
        }


        private FormFile CreateSampleFile(string filename)
        {
            var stream = new MemoryStream();
            stream.WriteByte(100);
            stream.Flush();
            stream.Position = 0;
            return new FormFile(stream, 0, 1, "file", filename);
        }
    }
}
