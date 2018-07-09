// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        [JsonConverter(typeof(ParametersDictionaryConverter))]
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ActionApiModel() { }

        public ActionApiModel(string action, Dictionary<String, Object> parameters)
        {
            this.ActionType = action;
            this.Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ActionApiModel x))
            {
                return false;
            }
            else
            {
                obj = (ActionApiModel)obj;
            }
            return this.ActionType.Equals(x.ActionType)
                && this.IsEqualDictionary(x.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = (this.ActionType != null ? this.ActionType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.Parameters != null ? this.Parameters.GetHashCode() : 0);
            return hashCode;
        }

        private bool IsEqualDictionary(IDictionary<string, object> comapreDictionary)
        {
            if ((comapreDictionary == null) || (this.Parameters == null) || (this.Parameters.Count != comapreDictionary.Count)) return false;
            
            foreach(var key in this.Parameters.Keys)
            {
                if (!comapreDictionary.ContainsKey(key)) return false;
            }

            foreach(var key in this.Parameters.Keys)
            {
                if ( key != "Email" && (!comapreDictionary[key].Equals(this.Parameters[key]))) return false;
            }
            // Compare Email list.
            if (this.Parameters.ContainsKey("Email") && !IsListEqual((List<string>)this.Parameters["Email"], (List<string>)comapreDictionary["Email"])) return false;
            return true;
        }

        private static bool IsListEqual(List<string> list1, List<string> list2)
        {
            int listLength = list1.Count;
            bool listMatch = listLength == list2.Count;
            while (listMatch && --listLength >= 0)
            {
                listMatch = listMatch && list1[listLength] == list2[listLength];
            }
            return listMatch;
        }
    }

    public class ParametersDictionaryConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var returnDictionary = new Dictionary<string, object>();
            JObject jsonObject = JObject.Load(reader);
            // Casting to proper types.
            if (jsonObject["Email"] != null) returnDictionary["Email"] = ((JArray)jsonObject["Email"]).ToObject<List<string>>();
            if (jsonObject["Template"] != null) returnDictionary["Template"] = jsonObject["Template"].ToObject<string>();
            if (jsonObject["Subject"] != null) returnDictionary["Subject"] = jsonObject["Subject"].ToObject<string>();
            return returnDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
