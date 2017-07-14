// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService;
using WebService.Test.helpers;

public sealed class AssemblyInitialize : IDisposable
{
    public string wsHostname { get; }
    private IWebHost host;

    static AssemblyInitialize()
    {
        Current = new AssemblyInitialize();
    }

    public static AssemblyInitialize Current { get; private set; }

    internal static void Run()
    {
    }

    private AssemblyInitialize()
    {
        this.wsHostname = WebServiceHost.GetBaseAddress();
        this.host = new WebHostBuilder()
            .UseUrls(wsHostname)
            .UseKestrel()
            .UseIISIntegration()
            .UseStartup<Startup>()
            .Build();
        this.host.Start();
    }

    ~AssemblyInitialize()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        this.host.Dispose();
    }
}