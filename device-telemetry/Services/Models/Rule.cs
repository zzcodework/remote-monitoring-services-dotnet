// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class Rule : IComparable<Rule>
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
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
        // Possible values -["00:01:00", "00:05:00", "00:10:00"]
        public TimeSpan TimePeriod { get; set; } = new TimeSpan();
        public IList<Condition> Conditions { get; set; } = new List<Condition>();

        public Rule() { }

        public int CompareTo(Rule other)
        {
            if (other == null) return 1;

            return DateTimeOffset.Parse(other.DateCreated)
                .CompareTo(DateTimeOffset.Parse(this.DateCreated));
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
