// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models
{
    public class PackageApiModel
    {
        [JsonProperty("Id")]
        public string Id;

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType Type { get; set; }

        [JsonProperty("ConfigType")]
        public string ConfigType { get; set; }

        [JsonProperty(PropertyName = "DateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }

        public PackageApiModel(Package model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.Type = model.Type;
            this.DateCreated = model.DateCreated;
            this.Content = model.Content;
            this.ConfigType = model.ConfigType;
        }

        public PackageApiModel(
                string Content,
                string Name,
                PackageType Type,
                string ConfigType)
        {
            this.Content = Content;
            this.Name = Name;
            this.Type = Type;
            this.ConfigType = ConfigType;
        }

        public Package ToServiceModel()
        {
            return new Package()
            {
                Content = this.Content,
                Name = this.Name,
                Type = this.Type,
                ConfigType = this.ConfigType
            };
        }
    }
}