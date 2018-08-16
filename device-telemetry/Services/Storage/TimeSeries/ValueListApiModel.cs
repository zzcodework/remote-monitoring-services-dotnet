// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class ValueListApiModel
    {
        [JsonProperty("events")]
        public List<ValueApiModel> Events { get; set; }

        public ValueListApiModel()
        {
            this.Events = new List<ValueApiModel>();
        }

        public MessageList ToMessageList()
        {
            var messages = new List<Message>();
            var properties = new HashSet<string>();
            var schemas = new Dictionary<long, SchemaModel>();

            foreach (var tsiEvent in this.Events)
            {
                try
                {
                    // Track each new message schema type
                    if (!tsiEvent.SchemaRid.HasValue)
                    {
                        schemas.Add(tsiEvent.Schema.Rid, tsiEvent.Schema);
                        tsiEvent.SchemaRid = tsiEvent.Schema.Rid;

                        // Add new properties from schema
                        var schemaProperties = schemas[tsiEvent.Schema.Rid].PropertiesByIndex();
                        foreach (var property in schemaProperties)
                        {
                            properties.Add(property.Key);
                        }
                    }

                    var schema = schemas[tsiEvent.SchemaRid.Value];

                    // Add message from event
                    var message = new Message
                    {
                        DeviceId = tsiEvent.Values[schema.GetDeviceIdIndex()].ToString(),
                        Time = DateTimeOffset.Parse(tsiEvent.Timestamp),
                        Data = this.GetEventAsJson(tsiEvent.Values, schema)
                    };

                    messages.Add(message);
                }
                catch (Exception e)
                {
                    throw new InvalidInputException("Failed to parse message from Time Series Insights.", e);
                }
            }

            return new MessageList(messages, new List<string>(properties));
        }

        /// <summary>
        /// Converts the tsi paylod for 'values' to the 'data' JObject payload for the message model.
        /// </summary>
        private JObject GetEventAsJson(List<JValue> values, SchemaModel schema)
        {
            // Get dictionary of properties and index e.g. < propertyname, index > from schema
            var propertiesByIndex = schema.PropertiesByIndex();

            var result = new JObject();

            foreach (var property in propertiesByIndex)
            {
                result.Add(property.Key, values[property.Value]);
            }

            return result;
        }
    }
}
