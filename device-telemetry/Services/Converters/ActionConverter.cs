// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Converters
{
    public class ActionConverter : JsonConverter
    {
        private const string ACTION_TYPE_KEY = "Type";

        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAction);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            // default to email action type
            IAction result = new EmailAction();

            var actionType = Enum.Parse(
                typeof(ActionType),
                jsonObject.GetValue(ACTION_TYPE_KEY).Value<string>(),
                true);
            
            switch (actionType)
            {
                case ActionType.Email:
                    result = new EmailAction();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
