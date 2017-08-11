// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class ProtocolsTest
    {
        private readonly Random rand;

        public ProtocolsTest()
        {
            rand = new Random();
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetAllTest()
        {
            var name = rand.NextString();
            var type = rand.NextString();
            var parameters = new[]
            {
                rand.NextString(),
                rand.NextString(),
                rand.NextString()
            };
            var algorithmsA = rand.NextString();
            var algorithmsB = rand.NextString();

            var config = new ServicesConfig
            {
                Protocols = new[]
                {
                    new ProtocolConfig(new Dictionary<string, string>
                    {
                        { "name", name },
                        { "type", type },
                        { "p0", parameters[0] },
                        { "p1", parameters[1] },
                        { "p2", parameters[2] }
                    })
                },
                SupportedSignatureAlgorithms = new[] { algorithmsA, algorithmsB }
            };

            var protocols = new Protocols(config);
            var model = protocols.GetAll();

            Assert.Equal(model.Items.Count(), 1);
            Assert.Equal(model.Items.Single().Name, name);
            Assert.Equal(model.Items.Single().Type, type);
            Assert.Equal(model.Items.Single().Parameters["p0"], parameters[0]);
            Assert.Equal(model.Items.Single().Parameters["p1"], parameters[1]);
            Assert.Equal(model.Items.Single().Parameters["p2"], parameters[2]);
            Assert.Equal(model.SupportedSignatureAlgorithms.Count(), 2);
            Assert.True(model.SupportedSignatureAlgorithms.Contains(algorithmsA));
            Assert.True(model.SupportedSignatureAlgorithms.Contains(algorithmsB));
        }
    }
}
