// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class ActionsController : Controller
    {
        private readonly ILogger log;

        public ActionsController(ILogger logger)
        {
            this.log = logger;
        }

        [HttpGet("action-settings")]
        public async Task<ActionSettingsListApiModel> GetAllAsync()
        {
            // TODO
            return new ActionSettingsListApiModel();
        }
    }
}
