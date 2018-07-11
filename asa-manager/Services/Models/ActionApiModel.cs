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

        // Checks if both the dictionaries have the same keys and values.
        // For a dictionary[key] => list, does a comaprison of all the elements of the list, regardless of order. 
        private bool IsEqualDictionary(IDictionary<string, object> compareDictionary)
        {
             /*
             Possible cases: 
             1. Both null.
             2. One is null.
             3. Different number of key value pairs.
             4. Same key, different value of type string.
             5. Same key, different value of type list.
             6. Same key, same value in different order for list.
             7. Same key, same value in same order.
             8. Same key, one is string, one is list.
             */
            if (this.Parameters.Count != compareDictionary.Count) return false;

            foreach(var key in this.Parameters.Keys)
            {
                if (!compareDictionary.ContainsKey(key) || 
                    !IsSameType(this.Parameters[key], compareDictionary[key]))
                {
                    return false;
                }
                else if (this.Parameters[key] is IList<string> && 
                    !IsListEqual((List<string>)this.Parameters[key], (List<string>)compareDictionary[key]))
                {
                    return false;
                }
                else if (!(this.Parameters[key] is IList<string>) && 
                    !compareDictionary[key].Equals(this.Parameters[key]))
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

        private static bool IsSameType(object a, object b)
        {
            // Checks if two objects are of same type, in the same inheritance tree, or one is implemented by the other. 
            var type1 = a.GetType();
            var type2 = b.GetType();
            return type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1);
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
