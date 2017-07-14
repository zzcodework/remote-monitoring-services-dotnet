// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService;

namespace WebService.Test.helpers
{
    public class WebServiceHost
    {
        public static string GetBaseAddress()
        {
            int port = new Random().Next(40000, 60000);
            string baseAddress = "http://127.0.0.1:" + port;
            return baseAddress;
        }
    }
}
