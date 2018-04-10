// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent
{
    public interface ITelemetryRulesAgent
    {
        Task RunAsync();
        void Stop();
    }

    public class Agent : ITelemetryRulesAgent
    {
        // Check if rules have been modified every 10 seconds
        // (not too frequently, but frequently enough to provide a good UX)
        private const int CHECK_INTERVAL_MSECS = 10000;

        private readonly ILogger log;
        private readonly IRules rulesService;
        private readonly IAsaRulesConfig asaRulesConfigService;
        private readonly IThreadWrapper thread;
        private bool running;
        private IList<Rule> rules;
        private bool updateRequired;

        public Agent(
            IRules rulesService,
            IAsaRulesConfig asaRulesConfigService,
            IThreadWrapper thread,
            ILogger logger)
        {
            this.rulesService = rulesService;
            this.asaRulesConfigService = asaRulesConfigService;
            this.thread = thread;
            this.log = logger;
            this.running = true;
            this.updateRequired = false;
            this.rules = new List<Rule>();
        }

        /// <summary>
        /// Keep checking to see whether the list of rules have been modified.
        /// When the rules change, invoke the service responsible for exporting
        /// them to ASA.
        /// </summary>
        public async Task RunAsync()
        {
            this.log.Info("ASA Job Configuration Agent running", () => { });
            this.running = true;

            while (this.running)
            {
                await this.CheckIfRulesHaveChangedAsync();
                await this.UpdateAsaIfNeededAsync();

                this.thread.Sleep(CHECK_INTERVAL_MSECS);
            }
        }

        public void Stop()
        {
            this.running = false;
        }

        private async Task UpdateAsaIfNeededAsync()
        {
            if (this.updateRequired)
            {
                try
                {
                    await this.asaRulesConfigService.UpdateConfigurationAsync(this.rules);
                    this.updateRequired = false;
                }
                catch (Exception e)
                {
                    this.log.Error("Error while updating ASA", () => new { e });
                }
            }
        }

        private async Task CheckIfRulesHaveChangedAsync()
        {
            try
            {
                if (this.updateRequired || await this.RulesHaveChangedAsync())
                {
                    this.updateRequired = true;
                    this.log.Info("Device monitoring rules have changed", () => { });
                }
            }
            catch (Exception e)
            {
                this.log.Error("Error while checking the rules", () => new { e });
            }
        }

        private async Task<bool> RulesHaveChangedAsync()
        {
            var newRules = await this.rulesService.GetActiveRulesSortedByIdAsync();
            if (this.rulesService.RulesAreEquivalent(newRules, this.rules))
            {
                return false;
            }

            this.rules = newRules;
            return true;
        }
    }
}
