// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class UserInformationTest
    {
        private Random rand;

        public UserInformationTest()
        {
            rand = new Random();
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetTest()
        {
            var config = new ServicesConfig
            {
                SupportedSignatureAlgorithms = new[] { "none" }
            };
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var userInformation = new UserInformation(config, logger);

            var email = rand.NextString();
            var name = rand.NextString();
            var role = rand.NextString();
            var validTo = rand.NextDateTimeOffset().UtcDateTime;

            var payload = new JwtPayload(
                issuer: rand.NextString(),
                audience: rand.NextString(),
                claims: new[]
                {
                    new Claim("email", email),
                    new Claim("name", name),
                    new Claim("roles", role)
                },
                notBefore: DateTime.MinValue,
                expires: validTo);
            var header = new JwtHeader();
            var jwt = new JwtSecurityToken(header, payload);
            var token = $"{jwt.EncodedHeader}.{jwt.EncodedPayload}.0";

            var model = userInformation.Get(token);

            Assert.Equal(model.Id, email);
            Assert.Equal(model.Name, name);
            Assert.Equal(model.Email, email);
            Assert.Equal(model.Role, role);
            Assert.Equal(model.ValidTo, validTo);
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetNotSupportedAlgorithmTest()
        {
            var config = new ServicesConfig
            {
                SupportedSignatureAlgorithms = new[] { "RS1" }
            };
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var userInformation = new UserInformation(config, logger);

            var email = rand.NextString();
            var name = rand.NextString();
            var role = rand.NextString();
            var validTo = rand.NextDateTimeOffset().UtcDateTime;

            var payload = new JwtPayload(
                issuer: rand.NextString(),
                audience: rand.NextString(),
                claims: new[]
                {
                    new Claim("email", email),
                    new Claim("name", name),
                    new Claim("roles", role)
                },
                notBefore: DateTime.MinValue,
                expires: validTo);
            var header = new JwtHeader();
            var jwt = new JwtSecurityToken(header, payload);
            var token = $"{jwt.EncodedHeader}.{jwt.EncodedPayload}.0";

            Assert.Throws<InvalidInputException>(() => userInformation.Get(token));
        }

        [Fact, Trait(Constants.Type, Constants.UnitTest)]
        public void GetInvalidJwtTest()
        {
            var config = new ServicesConfig
            {
                SupportedSignatureAlgorithms = new[] { "none" }
            };
            var logger = new Logger("UnitTest", LogLevel.Debug);
            var userInformation = new UserInformation(config, logger);

            Assert.Throws<InvalidInputException>(() => userInformation.Get(rand.NextString()));
        }
    }
}
