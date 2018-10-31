// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IConfig config;

        public StatusController(ILogger logger, IConfig config)
        {
            this.log = logger;
            this.config = config;
        }

        public StatusApiModel Get()
        {
            // TODO: check AAD service once it is implemented
            var result = new StatusApiModel();
            result.Properties.Add("AuthRequired", this.config.ClientAuthConfig?.AuthRequired.ToString());
            result.Properties.Add("Port", this.config.Port.ToString());
            this.log.Info("Service status request", () => new { Healthy = true });
            return result;
        }
    }
}
