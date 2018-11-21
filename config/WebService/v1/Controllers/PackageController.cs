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

        /**
         * This function can be used to packages with and without parameters packageType, configType
         */
        [HttpGet]
        public async Task<PackageListApiModel> GetListAsync([FromQuery]string packageType, [FromQuery]string configType)
        {
            if (string.IsNullOrEmpty(packageType) && string.IsNullOrEmpty(configType))
            {
                return new PackageListApiModel(await this.storage.GetPackagesAsync());
            }

            if (string.IsNullOrEmpty(packageType))
            {
                throw new InvalidInputException("Valid package Type must be provided");
            }

            return new PackageListApiModel(await this.storage.GetPackagesAsync(), packageType, configType);
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

        [HttpGet]
        [Route("configtypes")]
        public async Task<ConfigTypeListApiModel> GetListAsync()
        {
            return new ConfigTypeListApiModel(await this.storage.GetConfigTypeListAsync());
        }

        [HttpPost]
        [Authorize("CreatePackages")]
        public async Task<PackageApiModel> PostAsync(string packageType, string configType, IFormFile package)
        {
            if (string.IsNullOrEmpty(packageType))
            {
                throw new InvalidInputException("Package Type must be provided");
            }

            if (configType == null)
            {
                configType = string.Empty;
            }

            bool isValidPackageType = Enum.TryParse(packageType, true, out PackageType uploadedPackageType);
            if (!isValidPackageType)
            {
                throw new InvalidInputException($"Provided packageType {packageType} is not valid.");
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

            var packageToAdd = new PackageApiModel(
                packageContent,
                package.FileName,
                uploadedPackageType, 
                configType);

            return new PackageApiModel(await this.storage.AddPackageAsync(packageToAdd.ToServiceModel()));
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