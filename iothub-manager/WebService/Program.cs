using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Config(new ConfigData());

            /*
            Print some information to help development and debugging, like
            runtime and configuration settings
            */
            Console.WriteLine($"[{Uptime.ProcessId}] Starting web service, process ID: " + Uptime.ProcessId);
            Console.WriteLine($"[{Uptime.ProcessId}] Web service listening on port " + config.Port);
            Console.WriteLine($"[{Uptime.ProcessId}] Web service health check at: http://127.0.0.1:" + config.Port + "/" + v1.Version.Path + "/status");
            Console.WriteLine($"[{Uptime.ProcessId}] Query all devices: http://127.0.0.1:" + config.Port + "/" + v1.Version.Path + "/devices");

            var host = new WebHostBuilder()
                .UseUrls("http://*:" + config.Port)
                .UseKestrel(options => { options.AddServerHeader = false; })
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
