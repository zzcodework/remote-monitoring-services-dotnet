// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.AsaManager.AsaConfigAgent
{
    public interface IAsaConfigAgent
    {
        Task RunAsync();
        void Stop();
    }

    public class AsaConfigAgent : IAsaConfigAgent
    {
        private const int CHECK_INTERVAL_MSECS = 10000;

        private readonly ILogger log;
        private bool running;

        public AsaConfigAgent(ILogger logger)
        {
            this.log = logger;
            this.running = true;
        }

        public async Task RunAsync()
        {
            this.log.Info("ASA Job Configuration Agent running", () => { });

            while (this.running)
            {
                try
                {
                    await Task.CompletedTask;
                }
                catch (Exception e)
                {
                    this.log.Error("...", () => new { e });
                }

                Thread.Sleep(CHECK_INTERVAL_MSECS);
            }
        }

        public void Stop()
        {
            this.running = false;
        }
    }
}