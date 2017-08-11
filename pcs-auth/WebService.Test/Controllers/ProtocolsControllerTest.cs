// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.Controllers
{
    public class ProtocolsControllerTest
    {
        private readonly Mock<IProtocols> mockProtocols;
        private readonly ProtocolsController controller;
        private readonly Random rand;

        public ProtocolsControllerTest()
        {
            this.mockProtocols = new Mock<IProtocols>();
            this.controller = new ProtocolsController(mockProtocols.Object);
            this.rand = new Random();
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetTest()
        {
            var protoalAName = rand.NextString();
            var protoalAType = rand.NextString();
            var protoalBName = rand.NextString();
            var protoalBType = rand.NextString();
            var algorithmA = rand.NextString();
            var algorithmB = rand.NextString();

            var model = new ProtocolListServiceModel
            {
                Items = new[]
                {
                    new ProtocolServiceModel
                    {
                        Name = protoalAName,
                        Type = protoalAType
                    },
                    new ProtocolServiceModel
                    {
                        Name = protoalBName,
                        Type = protoalBType
                    },
                },
                SupportedSignatureAlgorithms = new[] { algorithmA, algorithmB }
            };

            mockProtocols
                .Setup(x => x.GetAll())
                .Returns(model);

            var result = controller.Get();

            this.mockProtocols
                .Verify(x => x.GetAll(), Times.Once);

            Assert.Equal(model.Items.Count(), 2);
            Assert.Equal(model.Items.First().Name, protoalAName);
            Assert.Equal(model.Items.First().Type, protoalAType);
            Assert.Equal(model.Items.Last().Name, protoalBName);
            Assert.Equal(model.Items.Last().Type, protoalBType);
            Assert.Equal(result.SupportedSignatureAlgorithms.Count(), 2);
            Assert.True(result.SupportedSignatureAlgorithms.Contains(algorithmA));
            Assert.True(result.SupportedSignatureAlgorithms.Contains(algorithmB));
        }
    }
}
