// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Moq;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class MessagesTest
    {
        private const int SKIP = 0;
        private const int LIMIT = 1000;

        private readonly Mock<IMessages> messages;

        public MessagesTest()
        {
            this.messages = new Mock<IMessages>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task InitialListIsEmptyAsync()
        {
            // Arrange
            this.ThereAreNoMessagesInStorage();

            // Act
            var list = await this.messages.Object.ListAsync(null, null, null, SKIP, LIMIT, null);

            // Assert
            Assert.Empty(list.Messages);
            Assert.Empty(list.Properties);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetListWithValuesAsync()
        {
            // Arrange
            this.ThereAreSomeMessagesInStorage();

            // Act
            var list = await this.messages.Object.ListAsync(null, null, null, SKIP, LIMIT, null);

            // Assert
            Assert.NotEmpty(list.Messages);
            Assert.NotEmpty(list.Properties);
        }

        private void ThereAreNoMessagesInStorage()
        {
            this.messages.Setup(x => x.ListAsync(null, null, null, SKIP, LIMIT, null))
                .ReturnsAsync(new MessageList());
        }

        private void ThereAreSomeMessagesInStorage()
        {
            var sampleMessages = new List<Message>();
            var sampleProperties = new List<string>();

            var data = new JObject
            {
                { "data.sample_unit", "mph" },
                { "data.sample_speed", "10" }
            };

            sampleMessages.Add(new Message("id1", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));
            sampleMessages.Add(new Message("id2", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));

            sampleProperties.Add("data.sample_unit");
            sampleProperties.Add("data.sample_speed");

            this.messages.Setup(x => x.ListAsync(null, null, null, SKIP, LIMIT, null))
                .ReturnsAsync(new MessageList(sampleMessages, sampleProperties));
        }
    }
}
