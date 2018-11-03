// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH + "/packages"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class PackageController : Controller
    {
        private readonly IStorage storage;

        public PackageController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        public async Task<PackageListApiModel> GetAllAsync()
        {
            return new PackageListApiModel(await this.storage.GetPackagesAsync());
        }

        [HttpGet("{id}")]
        public async Task<PackageApiModel> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidInputException("Valid id must be provided");
            }

            return new PackageApiModel(await this.storage.GetPackageAsync(id));
        }

        [HttpPost]
        [Authorize("CreatePackages")]
        public async Task<PackageApiModel> PostAsync(string type, string config, IFormFile package, string customConfigType=null)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new InvalidInputException("Package type must be provided");
            }

            if (string.IsNullOrEmpty(config))
            {
                config = null;
            }

            bool isValidPackageType = Enum.TryParse(type, true, out PackageType uploadedPackageType);
            if (!isValidPackageType)
            {
                throw new InvalidInputException($"Provided package type {type} is not valid.");
            }

            bool isValidConfigType = Enum.TryParse(config, true, out ConfigType uploadedConfigType);
            if (!isValidConfigType)
            {
                throw new InvalidInputException($"Provided config type {type} is not valid.");
            }

            if (package == null || package.Length == 0 || string.IsNullOrEmpty(package.FileName))
            {
                throw new InvalidInputException("Package uploaded is missing or invalid.");
            }

            string packageContent;
            using (var streamReader = new StreamReader(package.OpenReadStream()))
            {
                packageContent = await streamReader.ReadToEndAsync();
            }

            var packageToAdd = new Package()
            {
                Content = packageContent,
                Name = package.FileName,
                Type = uploadedPackageType, 
                Config = uploadedConfigType,
                CustomConfig = customConfigType
            };

            if (!string.IsNullOrEmpty(customConfigType))
            {
                await this.storage.UpdateConfigurationsAsync(customConfigType);
            }

            return new PackageApiModel(await this.storage.AddPackageAsync(packageToAdd));
        }

        [HttpDelete("{id}")]
        [Authorize("DeletePackages")]
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidInputException("Valid id must be provided");
            }

            await this.storage.DeletePackageAsync(id);
        }
    }
}