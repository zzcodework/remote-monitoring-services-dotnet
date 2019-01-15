// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class Rule : IComparable<Rule>
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
        private const string INVALID_CHARACTER = @"[^A-Za-z0-9:;.,_\-]";

        // Comes from the StorageAdapter document and not the serialized rule
        [JsonIgnore]
        public string ETag { get; set; } = string.Empty;
        // Comes from the StorageAdapter document and not the serialized rule
        [JsonIgnore]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DateCreated { get; set; } = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
        public string DateModified { get; set; } = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
        public bool Enabled { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        [JsonConverter(typeof(StringEnumConverter))]
        public SeverityType Severity { get; set; } = new SeverityType();
        [JsonConverter(typeof(StringEnumConverter))]
        public CalculationType Calculation { get; set; } = new CalculationType();
        // Possible values -[60000, 300000, 600000] in milliseconds
        public long TimePeriod { get; set; } = 0;
        public IList<Condition> Conditions { get; set; } = new List<Condition>();
        public IList<IAction> Actions { get; set; } = new List<IAction>();
        public bool Deleted { get; set; } = false;
        public Rule() { }

        public int CompareTo(Rule other)
        {
            if (other == null) return 1;

            return DateTimeOffset.Parse(other.DateCreated)
                .CompareTo(DateTimeOffset.Parse(this.DateCreated));
        }

        public void Validate()
        {
            ValidateInput(this.ETag);
            ValidateInput(this.Id);
            ValidateInput(this.Name);
            ValidateInput(this.DateCreated);
            ValidateInput(this.DateModified);
            ValidateInput(this.Description);
            ValidateInput(this.GroupId);
        }

        // Check illegal characters in input
        private static void ValidateInput(string input)
        {
            input = input.Trim();

            if (Regex.IsMatch(input, INVALID_CHARACTER))
            {
                throw new InvalidInputException($"Input '{input}' contains invalid characters.");
            }
        }
    }

    public enum CalculationType
    {
        Average,
        Instant
    }

    public enum SeverityType
    {
        Critical,
        Warning,
        Info
    }
}
