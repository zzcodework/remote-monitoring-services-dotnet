// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.Models
{
    public class ActionApiModelTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void EmptyInstancesAreEqual()
        {
            // Arrange
            var x = new ActionApiModel();
            var y = new ActionApiModel();

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void NonEmptyInstancesWithSameDataAreEqual()
        {
            // Arrange: rule without parameters
            var x = new ActionApiModel()
            {
                ActionType = Guid.NewGuid().ToString()
            };

            var y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));

            // Arrange: rule with conditions
            x = new ActionApiModel()
            {
                ActionType = Guid.NewGuid().ToString(),
                 Parameters = new Dictionary<string, object>()
                {
                    {"Template", "Sample Template" },
                    {"Email", new List<string>() { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"} }
                }
            };
            y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentDataAreDifferent()
        {
            // Arrange
            var x = new ActionApiModel()
            {
                ActionType = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {"Template", "Sample Template" },
                    {"Email", new List<string>() { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"} }
                }
            };

            var y1 = Clone(x);
            var y2 = Clone(x);
            var y3 = Clone(x);

            y1.ActionType += "x";
            y2.Parameters.Add("key1", "x");
            y3.Parameters["Template"] += "sample string";

            // Assert
            Assert.False(x.Equals(y1));
            Assert.False(x.Equals(y2));
            Assert.False(x.Equals(y3));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentParametersAreDifferent()
        {
            // Arrange: different number of key-value pairs in Parameters.
            var x = new ActionApiModel()
            {
                Parameters = new Dictionary<string, object>()
                {
                    {"Template", "Sample Template" },
                    {"Email", new List<string>() { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"} }
                }
            };
            var y = Clone(x);
            y.Parameters.Add("Key1", "Value1");

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: different Email
            x.Parameters = new Dictionary<string, object>()
                {
                    {"Template", "Sample Template" },
                    {"Email", new List<string>() { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com"} }
                };
            y = Clone(x);
            ((List<string>)y.Parameters["Email"]).Add("y");

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: different template
            y = Clone(x);
            y.Parameters["Template"] = "Changing template";

            // Assert
            Assert.False(x.Equals(y));
        }

        private static T Clone<T>(T o)
        {
            var a = JsonConvert.SerializeObject(o);
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(o));
        }
    }
}
