using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models
{
    public class ProtocolApiModel
    {
        public string Name { get; }
        public string Type { get; }
        public Dictionary<string, string> Parameters { get; }

        public ProtocolApiModel(ProtocolServiceModel model)
        {
            Name = model.Name;
            Type = model.Type;
            Parameters = model.Parameters;
        }
    }
}
