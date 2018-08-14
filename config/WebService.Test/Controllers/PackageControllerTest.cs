// Copyright (c) Microsoft. All rights reserved.

using System;
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

namespace WebService.Test.Controllers
{
    public class PackageControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly PackageController controller;
        private readonly Random rand;

        public PackageControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.controller = new PackageController(this.mockStorage.Object);
            this.rand = new Random();
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("EDGE_MANIFEST", "filename", true, false)]
        [InlineData("EDGE_MANIFEST", "filename", false, true)]
        [InlineData("EDGE_MANIFEST", "", true, true)]
        [InlineData("BAD_TYPE", "filename", true, true)]
        public async Task PostAsyncTest(string type, string filename, bool giveValidFile, bool expectException)
        {
            // Arrange
            IFormFile file = null;
            if(giveValidFile)
            {
                file = this.CreateSampleFile(filename);
            }

            this.mockStorage.Setup(x => x.AddPackageAsync(
                                    It.Is<Package>(p => p.Type.ToString().Equals(type) &&
                                                        p.Name.Equals(filename))))
                            .ReturnsAsync(new Package() {
                                Name = filename,
                                Type = PackageType.EDGE_MANIFEST
                            });
            try
            {
                // Act
                var package = await this.controller.PostAsync(type, file);

                // Assert
                Assert.False(expectException);
                Assert.Equal(filename, package.Name);
                Assert.Equal(type, package.Type.ToString());
            }
            catch(Exception)
            {
                Assert.True(expectException);
            }
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
