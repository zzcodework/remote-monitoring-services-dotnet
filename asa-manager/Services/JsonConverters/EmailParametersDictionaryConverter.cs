// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters
{
    public class EmailParametersDictionaryConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        private const string SUBJECT_KEY = "Subject";
        private const string NOTES_KEY = "Notes";
        private const string RECIPIENTS_KEY = "Recipients";

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var returnDictionary = new Dictionary<string, object>();
            JObject jsonObject = JObject.Load(reader);

            // Convert to a case-insensitive dictionary for case insensitive look up.
            Dictionary<string, object> caseInsensitiveJsonDictionary =
                new Dictionary<string, object>(jsonObject.ToObject<Dictionary<string, object>>(), StringComparer.OrdinalIgnoreCase);
            if (caseInsensitiveJsonDictionary.ContainsKey(RECIPIENTS_KEY) && caseInsensitiveJsonDictionary[RECIPIENTS_KEY] != null)
                returnDictionary[RECIPIENTS_KEY] = ((JArray)caseInsensitiveJsonDictionary[RECIPIENTS_KEY]).ToObject<List<string>>();
            if (caseInsensitiveJsonDictionary.ContainsKey(NOTES_KEY) && caseInsensitiveJsonDictionary[NOTES_KEY] != null)
                returnDictionary[NOTES_KEY] = caseInsensitiveJsonDictionary[NOTES_KEY];
            if (caseInsensitiveJsonDictionary.ContainsKey(SUBJECT_KEY) && caseInsensitiveJsonDictionary[SUBJECT_KEY] != null)
                returnDictionary[SUBJECT_KEY] = caseInsensitiveJsonDictionary[SUBJECT_KEY];
            return returnDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
