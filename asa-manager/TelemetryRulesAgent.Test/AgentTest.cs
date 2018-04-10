// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using AsaConfigAgent.Test.helpers;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace TelemetryRulesAgent.Test
{
    public class AgentTest
    {
        // Protection against never ending tests, stop them and fail after 10 secs
        private const int TEST_TIMEOUT = 10;

        private readonly Mock<IRules> rulesService;
        private readonly Mock<IAsaRulesConfig> asaRulesConfigService;
        private readonly Mock<IThreadWrapper> thread;
        private readonly Mock<ILogger> logger;
        private readonly Agent target;

        public AgentTest(ITestOutputHelper log)
        {
            this.rulesService = new Mock<IRules>();
            this.asaRulesConfigService = new Mock<IAsaRulesConfig>();
            this.thread = new Mock<IThreadWrapper>();
            this.logger = new Mock<ILogger>();

            this.target = new Agent(
                this.rulesService.Object,
                this.asaRulesConfigService.Object,
                this.thread.Object,
                this.logger.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItKeepsCheckingIfRulesChanged()
        {
            // Arrange
            var loops = 8;
            this.ThereAreNoRules();
            this.StopAgentAfterNLoops(loops);

            // Act
            this.target.RunAsync().Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.thread.Verify(
                x => x.Sleep(It.IsAny<int>()),
                Times.Exactly(loops));
            this.rulesService.Verify(
                x => x.GetActiveRulesSortedByIdAsync(),
                Times.Exactly(loops));
            this.asaRulesConfigService.Verify(
                x => x.UpdateConfigurationAsync(It.IsAny<IList<Rule>>()),
                Times.Never);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItTriggersAConfigUpdateWhenRulesChange()
        {
            // Arrange
            this.RulesHaveNotChanged();
            this.StopAgentAfterNLoops(2);

            // Act
            this.target.RunAsync().Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.asaRulesConfigService.Verify(
                x => x.UpdateConfigurationAsync(It.IsAny<IList<Rule>>()),
                Times.Never);

            // Arrange
            this.RulesHaveChanged();
            this.StopAgentAfterNLoops(1);

            // Act
            this.target.RunAsync().Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.asaRulesConfigService.Verify(
                x => x.UpdateConfigurationAsync(It.IsAny<IList<Rule>>()),
                Times.Once);
        }

        private void RulesHaveNotChanged()
        {
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(It.IsAny<IList<Rule>>(), It.IsAny<IList<Rule>>()))
                .Returns(true);
        }

        private void RulesHaveChanged()
        {
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(It.IsAny<IList<Rule>>(), It.IsAny<IList<Rule>>()))
                .Returns(false);
        }

        private void ThereAreNoRules()
        {
            this.rulesService.Setup(
                    x => x.GetActiveRulesSortedByIdAsync())
                .ReturnsAsync(new List<Rule>());

            // Make sure that 2 empty lists are considered equivalent
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(
                        It.Is<IList<Rule>>(l => l.Count == 0),
                        It.Is<IList<Rule>>(l => l.Count == 0)))
                .Returns(true);
        }

        private void ThereAreNRules(int i)
        {
            var list = new List<Rule>();
            for (int j = 0; j < i; j++)
            {
                list.Add(new Rule());
            }

            this.rulesService.Setup(
                    x => x.GetActiveRulesSortedByIdAsync())
                .ReturnsAsync(list);
        }

        private void StopAgentAfterNLoops(int n)
        {
            this.thread
                .Setup(x => x.Sleep(It.IsAny<int>()))
                .Callback(() =>
                {
                    if (--n <= 0) this.target.Stop();
                });
        }
    }
}
