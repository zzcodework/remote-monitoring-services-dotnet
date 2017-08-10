// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class UserInformationControllerTest
    {
        private readonly Mock<IUserInformation> mockUserInformation;
        private readonly UserInformationController controller;
        private readonly Random rand;

        public UserInformationControllerTest()
        {
            this.mockUserInformation = new Mock<IUserInformation>();
            this.controller = new UserInformationController(mockUserInformation.Object);
            this.rand = new Random();
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetTest()
        {
            var token = rand.NextString();
            var model = new UserInformationServiceModel
            {
                Id = rand.NextString(),
                Name = rand.NextString(),
                Email = rand.NextString(),
                Role = rand.NextString(),
                ValidTo = rand.NextDateTimeOffset().UtcDateTime
            };

            using (var mockContext = new MockHttpContext())
            {
                controller.ControllerContext.HttpContext = mockContext.Object;

                mockUserInformation
                    .Setup(x => x.Get(It.IsAny<string>()))
                    .Returns(model);

                mockContext.SetHeader("Authorization", $"Bearer {token}");
                var result = controller.Get();

                Assert.Equal(result.Id, model.Id);
                Assert.Equal(result.Name, model.Name);
                Assert.Equal(result.Email, model.Email);
                Assert.Equal(result.Role, model.Role);
                Assert.Equal(result.ValidTo, model.ValidTo);
            }
        }
    }
}
