// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Converters
{
    public class ActionItemConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IActionItem);
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var actionType = default(EmailActionItem);
            switch (jsonObject["ActionType"].Value<string>())
            {
                case "Email":
                    actionType = new EmailActionItem();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), actionType);
            return actionType;
        }
    }
}
