// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers
{
    [Route(Version.Path + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class ProtocolsController : Controller
    {
        private readonly IProtocols protocols;

        public ProtocolsController(IProtocols protocols)
        {
            this.protocols = protocols;
        }

        [HttpGet]
        public ProtocolListApiModel Get()
        {
            return new ProtocolListApiModel(protocols.GetAll());
        }
    }
}
