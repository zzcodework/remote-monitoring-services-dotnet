using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = String.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<String, Object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ActionApiModel() { }

        public ActionApiModel(string action, Dictionary<String, Object> parameters)
        {
            this.ActionType = action;
            this.Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ActionApiModel))
            {
                return false;
            }
            else
            {
                obj = (ActionApiModel)obj;
            }
            return this.ActionType.Equals(((ActionApiModel)obj).ActionType)
                && this.Parameters.Count == ((ActionApiModel)obj).Parameters.Count
                && this.IsEqualDictionary(((ActionApiModel)obj).Parameters);

        }

        public override int GetHashCode()
        {
            var hashCode = (this.ActionType != null ? this.ActionType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.Parameters != null ? this.Parameters.GetHashCode() : 0);
            return hashCode;
        }

        private bool IsEqualDictionary(IDictionary<string, object> comapreDictionary)
        {
            // Update dictionary comparison.
            return true;
        }

        public string getParameters()
        {
            if (this.Parameters["Email"] != null)
            {
                this.Parameters["Email"] = ((Newtonsoft.Json.Linq.JArray)this.Parameters["Email"]).ToObject<string[]>();
            }
            return JsonConvert.SerializeObject(this.Parameters, Formatting.None);
        }
    }
}
