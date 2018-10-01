// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions
{
    /// <summary>
    /// Interface for all Actions that can be added as part of a Rule.
    /// New action types should implement ActionItem and be added to the ActionType enum.
    /// Parameters should be a case-insensitive dictionary used to pass additional
    /// information required for any given action type.
    /// </summary>
    public interface IActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        ActionType ActionType { get; set; }

        // Note: Parameters should always be initialized as a case-insensitive dictionary
        IDictionary<string, object> Parameters { get; set; }
    }

    public enum ActionType
    {
        Email
    }
}
