// Copyright (c) Microsoft. All rights reserved.

using System.Web.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Microsoft.Web.Http;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [ApiVersion(Version.Number), ExceptionsFilter]
    public class StatusController : ApiController
    {
        private readonly IDevices devices;

        // TODO: check if the dependencies are healthy
        public StatusController(IDevices devices)
        {
            // This dependency is not used yet, however just having the instance
            // helps to verify whether DI works
            this.devices = devices;
        }

        /// <summary>Return the service status</summary>
        /// <returns>Status object</returns>
        public StatusApiModel Get()
        {
            return new StatusApiModel(true, "Alive and well");
        }
    }
}
