// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services
{
    public interface IRules
    {
        // Fetch all the rules from the telemetry service
        Task<IList<Rule>> GetActiveRulesSortedByIdAsync();

        // Compare two list of rules
        bool RulesAreEquivalent(IList<Rule> newRules, IList<Rule> rules);
    }

    public class Rules : IRules
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string rulesWebServiceUrl;
        private readonly int rulesWebServiceTimeout;

        public Rules(
            IServicesConfig config,
            IHttpClient httpClient,
            ILogger logger)
        {
            this.log = logger;
            this.httpClient = httpClient;
            this.rulesWebServiceUrl = config.RulesWebServiceUrl;
            this.rulesWebServiceTimeout = config.RulesWebServiceTimeout;
        }

        public async Task<IList<Rule>> GetActiveRulesSortedByIdAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString(this.rulesWebServiceUrl);
            request.Options.Timeout = this.rulesWebServiceTimeout;
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "ASA Manager");

            var response = await this.httpClient.GetAsync(request);

            if (response.IsError)
            {
                this.log.Error("Request failed", () => new { uri = this.rulesWebServiceUrl, response.StatusCode });
                throw new ExternalDependencyException($"Failed to retrieve the list of rules");
            }

            var list = JsonConvert.DeserializeObject<IEnumerable<Rule>>(response.Content);

            // return only active rules, sorted by ID to facilitate comparison
            return list.Where(x => x.Enabled).OrderBy(x => x.Id).ToList();
        }

        public bool RulesAreEquivalent(IList<Rule> a, IList<Rule> b)
        {
            if (a.Count != b.Count) return false;

            return !a.Where((rule, i) => !rule.Equals(b[i])).Any();
        }
    }
}
