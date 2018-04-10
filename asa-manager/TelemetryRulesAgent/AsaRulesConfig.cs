// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent
{
    public interface IAsaRulesConfig
    {
        Task UpdateConfigurationAsync(IList<Rule> rules);
    }

    public class AsaRulesConfig : IAsaRulesConfig
    {
        public Task UpdateConfigurationAsync(IList<Rule> rules)
        {
            throw new NotImplementedException();
        }
    }
}
