// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries
{
    public class SchemaModel
    {
        private const string DEVICE_ID_KEY = "iothub-connection-device-id";
        private readonly HashSet<string> excludeProperties;

        public SchemaModel()
        {
            // List of properties from Time Series that should be
            // excluded in conversion to message model
            this.excludeProperties = new HashSet<string>
            {
                "iothub-message-schema",
                "iothub-creation-time-utc",
                "$$CreationTimeUtc",
                "$$MessageSchema",
                "$$ContentType",
                "iothub-connection-device-id",
                "iothub-connection-auth-method",
                "iothub-connection-auth-generation-id",
                "iothub-enqueuedtime",
                "iothub-message-source",
                "content-type",
                "content-encoding"
            };
        }

        [JsonProperty("rid")]
        public long Rid { get; set; }

        [JsonProperty("$esn")]
        public string EventSourceName { get; set; }

        [JsonProperty("properties")]
        public List<PropertyModel> Properties { get; set; }

        /// <summary>
        /// Returns the properties needed to convert to message model
        /// with lookup by index, excludes iothub properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> PropertiesByIndex()
        {
            var result = new Dictionary<string, int> ();

            for (int i = 0; i < this.Properties.Count; i++)
            {
                var property = this.Properties[i];

                if (!this.excludeProperties.Contains(property.Name))
                {
                    result.Add(property.Name, i);
                }
            }

            return result;
        }

        public int GetDeviceIdIndex()
        {
            for (int i = 0; i < this.Properties.Count; i++)
            {
                if (this.Properties[i].Name.Equals(DEVICE_ID_KEY))
                {
                    return i;
                }
            }

            throw new InvalidInputException("No device id found in message schema from Time Series Insights. " +
                                            "Device id property 'iothub-connection-device-id' is missing.");
        }
    }
}
