using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Models
{
    public class ProtocolListServiceModel
    {
        public IEnumerable<ProtocolServiceModel> Items { get; set; }

        public IEnumerable<string> SupportedSignatureAlgorithms { get; set; }
    }
}
