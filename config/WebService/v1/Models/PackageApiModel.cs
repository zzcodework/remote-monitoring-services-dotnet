// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageApiModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        [JsonProperty("Id")]
        public string Id;

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType Type { get; set; }

        [JsonProperty(PropertyName = "DateCreated")]
        public string DateCreated { get; set; } = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);

        [JsonProperty("Content")]
        public string Content { get; set; }

        public PackageApiModel()
        {

        }

        public PackageApiModel(Package model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.Type = model.Type;
            this.DateCreated = model.DateCreated;
            this.Content = model.Content;
        }

        public Package ToServiceModel()
        {
            return new Package
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                DateCreated = this.DateCreated,
                Content = this.Content
            };
        }
    }
}