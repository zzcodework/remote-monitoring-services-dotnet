// Copyright (c) Microsoft. All rights reserved.

using System.Web.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [ApiVersion(Version.Number)]
    public class StatusController : ApiController
    {
        /// <summary>Return the service status</summary>
        /// <returns>Status object</returns>
        public StatusApiModel Get()
        {
            return new StatusApiModel { Message = "OK" };
        }
    }
}
