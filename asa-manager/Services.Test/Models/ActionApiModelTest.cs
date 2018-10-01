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
            var x = new EmailActionApiModel();
            var y = new EmailActionApiModel();

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void NonEmptyInstancesWithSameDataAreEqual()
        {
            // Arrange: action without parameters
            var x = new EmailActionApiModel()
            {
                ActionType = Microsoft.Azure.IoTSolutions.AsaManager.Services.Models.Type.Email
            };

            var y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));

            // Arrange: action with parameters
            x = new EmailActionApiModel()
            {
                ActionType = Microsoft.Azure.IoTSolutions.AsaManager.Services.Models.Type.Email,
                Parameters = this.CreateSampleParameters()
            };
            y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentDataAreDifferent()
        {
            // Arrange
            var x = new EmailActionApiModel()
            {
                ActionType = Microsoft.Azure.IoTSolutions.AsaManager.Services.Models.Type.Email,
                Parameters = this.CreateSampleParameters()
            };

            var y2 = Clone(x);
            var y3 = Clone(x);

            y2.Parameters.Add("key1", "x");
            y3.Parameters["Notes"] += "sample string";

            // Assert
            Assert.False(x.Equals(y2));
            Assert.False(x.Equals(y3));
            Assert.False(x.Equals(null));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentKeysAreDifferent()
        {
            // Arrange: different number of key-value pairs in Parameters.
            var x = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };

            var y = Clone(x);
            y.Parameters.Add("Key1", "Value1");

            // Assert
            Assert.False(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentKayValuesAreDifferent()
        {
            // Arrange: different template
            var x = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };

            var y = Clone(x);
            y.Parameters["Notes"] = "Changing note";

            // Assert
            Assert.False(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentKeyValueOfTypeListAreDifferent()
        {
            //Arrange: Differet list of email
            var x = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };

            var y = Clone(x);
            ((List<string>)y.Parameters["Recipients"]).Add("y");

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: Different list of email, same length
            x.Parameters = this.CreateSampleParameters();
            y.Parameters = new Dictionary<string, object>()
                {
                    {"Notes", "Sample Note" },
                    {"Recipients", new List<string>() {"anotherEmail1@gmail.com", "anotherEmail2@gmail.com"} }
                };

            // Assert
            Assert.False(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithSameKeyValueOfTypeListAreSame()
        {
            // Arrange: Same list of email different order.
            var x = new EmailActionApiModel()
            {
                Parameters = this.CreateSampleParameters()
            };
            var y = new EmailActionApiModel()
            {
                Parameters = new Dictionary<string, object>()
                {
                    {"Notes", "Sample Note" },
                    {"Recipients", new List<string>() {"sampleEmail2@gmail.com", "sampleEmail1@gmail.com"} }
                }
            };

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithSameKeyValuePairCaseInsensitiveAreSame()
        {
            // Arrange: Same list of email different order.
            var tempDictionary = new Dictionary<string, object>()
            {
                {"ActionType", "Email"},
                {"Parameters", this.CreateSampleParameters()}
            };

            var tempDictionary2 = new Dictionary<string, object>()
            {
                {"ActionType", "Email"},
                {"Parameters", new Dictionary<string, object>()
                {
                    {"noTeS", "Sample Note" },
                    {"REcipienTs", new List<string>() {"sampleEmail2@gmail.com", "sampleEmail1@gmail.com"} }
                } }
            };

            var a = JsonConvert.SerializeObject(tempDictionary);
            var b = JsonConvert.SerializeObject(tempDictionary2);
            var c = JsonConvert.DeserializeObject<EmailActionApiModel>(a);
            var d = JsonConvert.DeserializeObject<EmailActionApiModel>(b);
            Assert.True(c.Equals(d));
        }

        private static T Clone<T>(T o)
        {
            var a = JsonConvert.SerializeObject(o);
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(o));
        }

        private Dictionary<string, object> CreateSampleParameters()
        {
            return new Dictionary<string, object>()
            {
                { "Notes", "Sample Note" },
                { "Recipients", new List<string>() { "sampleEmail1@gmail.com", "sampleEmail2@gmail.com" } }
            };
        }
    }
}
