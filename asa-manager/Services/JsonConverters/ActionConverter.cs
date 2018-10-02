// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters
{
    class ActionConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        private const string TYPE_KEY = "Type";
        private const string EMAIL_KEY = "Email";

        public override bool CanConvert(System.Type objectType)
        {
            return objectType == typeof(IActionApiModel);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var actionType = default(EmailActionApiModel);
            switch (jsonObject[TYPE_KEY].Value<string>())
            {
                case EMAIL_KEY:
                    actionType = new EmailActionApiModel();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), actionType);
            return actionType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization");
        }
    }
}
