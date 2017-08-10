// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public class UserInformation : IUserInformation
    {
        private readonly HashSet<string> supportedSignatureAlgorithms;
        private readonly ILogger logger;

        public UserInformation(IServicesConfig config, ILogger logger)
        {
            this.supportedSignatureAlgorithms = new HashSet<string>(config.SupportedSignatureAlgorithms);
            this.logger = logger;
        }

        public UserInformationServiceModel Get(string token)
        {
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = new JwtSecurityToken(token);
            }
            catch (Exception ex)
            {
                var message = $"Failed to parse JWT: {ex.Message}";
                logger.Error(message, () => { });
                throw new InvalidInputException(message);
            }

            if (!supportedSignatureAlgorithms.Contains(jwtToken.SignatureAlgorithm))
            {
                var message = "Signature algorithm is not supported";
                logger.Error(message, () => new { jwtToken.SignatureAlgorithm });
                throw new InvalidInputException(message);
            }

            var email = jwtToken.Claims.SingleOrDefault(c => c.Type == "email")?.Value;
            var name = jwtToken.Claims.SingleOrDefault(c => c.Type == "name")?.Value;
            var role = jwtToken.Claims.SingleOrDefault(c => c.Type == "roles")?.Value;

            return new UserInformationServiceModel
            {
                Id = email,
                Name = name,
                Email = email,
                Role = role,
                ValidTo = jwtToken.ValidTo
            };
        }
    }
}
