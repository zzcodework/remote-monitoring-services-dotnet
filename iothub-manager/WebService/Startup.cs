// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService
{
    public class Startup
    {
        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
            this.Configuration = builder.Build();
        }

        // This is where you register dependencies, add services to the
        // container. This method is called by the runtime, before the
        // Configure method below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add CORS service
            services.AddCors();

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            this.ApplicationContainer = DependencyResolution.Setup(services);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method is called by the runtime, after the ConfigureServices
        // method above. Use this method to add middleware.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(this.BuildCorsPolicy);

            app.UseMvc();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());
        }

        private void BuildCorsPolicy(CorsPolicyBuilder builder)
        {
            var config = this.ApplicationContainer.Resolve<IConfig>();

            // ToDo: replace Trace via ILogger aftre sync with template project
            CorsWhitelistModel model;
            try
            {
                model = JsonConvert.DeserializeObject<CorsWhitelistModel>(config.CorsWhitelist);
                if (model == null)
                {
                    Console.WriteLine($"Invalid CORS whitelist, ignored: {config.CorsWhitelist}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid CORS whitelist, ignored: {config.CorsWhitelist}, {ex.Message}");
                return;
            }

            if (model.Origins == null)
            {
                Console.WriteLine("No setting for CORS origin policy was found, ignore");
            }
            else if (model.Origins.Contains("*"))
            {
                Console.WriteLine("CORS policy allowed any origin");
                builder.AllowAnyOrigin();
            }
            else
            {
                Console.WriteLine($"Add specified origins to CORS policy: [{string.Join(", ", model.Origins)}]");
                builder.WithOrigins(model.Origins);
            }

            if (model.Origins == null)
            {
                Console.WriteLine("No setting for CORS method policy was found, ignore");
            }
            else if (model.Methods.Contains("*"))
            {
                Console.WriteLine("CORS policy allowed any method");
                builder.AllowAnyMethod();
            }
            else
            {
                Console.WriteLine($"Add specified methods to CORS policy: [{string.Join(", ", model.Methods)}]");
                builder.WithMethods(model.Methods);
            }

            if (model.Origins == null)
            {
                Console.WriteLine("No setting for CORS header policy was found, ignore");
            }
            else if (model.Headers.Contains("*"))
            {
                Console.WriteLine("CORS policy allowed any header");
                builder.AllowAnyHeader();
            }
            else
            {
                Console.WriteLine($"Add specified headers to CORS policy: [{string.Join(", ", model.Headers)}]");
                builder.WithHeaders(model.Headers);
            }
        }
    }
}
