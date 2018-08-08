// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Type = Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = string.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ActionApiModel(string action, Dictionary<string, object> parameters)
        {
            this.ActionType = action;
            this.Parameters = parameters;
        }

        public ActionApiModel(IActionItem action)
        {
            this.ActionType = action.Type.ToString();
            this.Parameters = action.Parameters;
        }

        public ActionApiModel() { }

        public IActionItem ToServiceModel()
        {
            if (!Enum.TryParse(this.ActionType, true, out Type action))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }

            switch (action)
            {
                case Type.Email:
                    return new EmailActionItem(action, this.Parameters);
                default:
                    throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
        }
    }
}
