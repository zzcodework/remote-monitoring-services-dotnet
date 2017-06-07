// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime;
using Microsoft.Owin.Hosting;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService
{
    /// <summary>Application entry point</summary>
    public class Program
    {
        static readonly IConfig config = new Config(new ConfigData());

        static void Main(string[] args)
        {
            var options = new StartOptions("http://*:" + config.Port);
            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine($"[{Uptime.ProcessId}] Web service started, process ID: " + Uptime.ProcessId);
                Console.WriteLine($"[{Uptime.ProcessId}] Web service listening on port " + config.Port);
                Console.WriteLine($"[{Uptime.ProcessId}] Web service health check at: http://127.0.0.1:" + config.Port + "/" + v1.Version.Path + "/status");
                Console.WriteLine($"[{Uptime.ProcessId}] Query all devices: http://127.0.0.1:" + config.Port + "/" + v1.Version.Path + "/devices");

                // Production mode: keep the service alive until killed
                if (args.Length > 0 && args[0] == "--background")
                {
                    while (true) Console.ReadLine();
                }

                // Development mode: keep the service alive until Enter is pressed
                Console.WriteLine("Press [Enter] to quit...");
                Console.ReadLine();
            }
        }
    }
}
