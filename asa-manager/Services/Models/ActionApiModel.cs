// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public IDictionary<string, Object> Parameters { get; set; }

        public ActionApiModel() {
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ActionApiModel(string action, Dictionary<string, Object> parameters)
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

        private bool IsEqualDictionary(IDictionary<string, object> compareDictionary)
        {
            if ((compareDictionary == null) && (this.Parameters == null)) return true;
            if ((compareDictionary == null) || (this.Parameters == null) || (this.Parameters.Count != compareDictionary.Count)) return false;
            
            foreach(var key in this.Parameters.Keys)
            {
                if (!compareDictionary.ContainsKey(key))
                {
                    return false;
                }
                else if ((compareDictionary[key] is IList<string>) && (this.Parameters[key] is IList<string>) && !IsListEqual((List<string>)this.Parameters[key], (List<string>)compareDictionary[key]))
                {
                    return false;
                }
                else if (!(compareDictionary[key] is IList<string>) && !compareDictionary[key].Equals(this.Parameters[key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsListEqual(List<string> list1, List<string> list2)
        {
            return list1.Count == list2.Count && !list1.Except(list2).Any();
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
