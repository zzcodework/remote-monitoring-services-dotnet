// Copyright (c) Microsoft. All rights reserved.

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

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageType Type { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }

        public PackageApiModel()
        {

        }

        public PackageApiModel(Package model)
        {
            this.Id = model.Id;
            this.Name = model.Name;
            this.Content = model.Content;
            this.Type = model.Type;
        }

        public Package ToServiceModel()
        {
            return new Package
            {
                Id = this.Id,
                Content = this.Content,
                Name = this.Name,
                Type = this.Type
            };
        }
    }
}
