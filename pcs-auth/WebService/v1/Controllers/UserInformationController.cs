// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Filters;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers
{
    [Route(Version.Path + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class UserInformationController : Controller
    {
        private const string BearerPrefix = "Bearer ";
        private readonly IUserInformation userInformation;

        public UserInformationController(IUserInformation userInformation)
        {
            this.userInformation = userInformation;
        }

        [HttpGet]
        public UserInformationServiceModel Get()
        {
            var authorizationHeader = Request.Headers["Authorization"].SingleOrDefault();
            if (authorizationHeader == null)
            {
                return null;
            }

            if (!authorizationHeader.StartsWith(BearerPrefix))
            {
                // Only Bearer token is supported
                return null;
            }

            var token = authorizationHeader.Substring(BearerPrefix.Length).Trim();
            return userInformation.Get(token);
        }
    }
}
