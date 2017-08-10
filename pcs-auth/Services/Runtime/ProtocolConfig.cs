// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Runtime
{
    public class ProtocolConfig
    {
        public string Name { get; }
        public string Type { get; }
        public Dictionary<string, string> Parameters { get; }

        public ProtocolConfig(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            Name = string.Empty;
            Type = string.Empty;
            Parameters = new Dictionary<string, string>();

            foreach (var pair in pairs)
            {
                switch (pair.Key)
                {
                    case "name":
                        Name = pair.Value;
                        break;

                    case "type":
                        Type = pair.Value;
                        break;

                    default:
                        Parameters.Add(pair.Key, pair.Value);
                        break;
                }
            }
        }
    }
}
