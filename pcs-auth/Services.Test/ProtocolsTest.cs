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
                }
            };

            var protocols = new Protocols(config);
            var models = protocols.GetAll();

            Assert.Equal(models.Count(), 1);
            Assert.Equal(models.Single().Name, name);
            Assert.Equal(models.Single().Type, type);
            Assert.Equal(models.Single().Parameters["p0"], parameters[0]);
            Assert.Equal(models.Single().Parameters["p1"], parameters[1]);
            Assert.Equal(models.Single().Parameters["p2"], parameters[2]);
        }
    }
}
