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

        // Maps Types of actions to their Service layer class.
        private static readonly IDictionary<Models.Type, Func<IActionApiModel>> actionMapping
            = new Dictionary<Models.Type, Func<IActionApiModel>>()
                {
                    { Models.Type.Email, () => new EmailActionApiModel() }
                };

        private const string TYPE_KEY = "Type";

        public override bool CanConvert(System.Type objectType)
        {
            return objectType == typeof(IActionApiModel);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonArray = JArray.Load(reader);
            IList<IActionApiModel> actionItemList = new List<IActionApiModel>();
            foreach (var jsonObject in jsonArray)
            {
                Enum.TryParse(jsonObject[TYPE_KEY].Value<string>(), true, out Models.Type action);
                var actionItem = actionMapping[action]();
                serializer.Populate(jsonObject.CreateReader(), actionItem);
                actionItemList.Add(actionItem);
            }
            return actionItemList;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization");
        }
    }
}
