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

        [HttpGet("{type}/{config}")]
        public async Task<PackageListApiModel> GetAllAsync(string type, string config)
        {
            return new PackageListApiModel(await this.storage.GetPackagesAsync(), type, config);
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
        [Route("configurations")]
        public async Task<PackageConfigListApiModel> GetListAsync()
        {
            return new PackageConfigListApiModel(await this.storage.GetAllConfigurationsAsync());
        }

        [HttpPost]
        [Authorize("CreatePackages")]
        public async Task<PackageApiModel> PostAsync(string type, string configType, IFormFile package)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new InvalidInputException("Package type must be provided");
            }

            if (configType == null)
            {
                throw new InvalidInputException("Package config type must be provided");
            }

            bool isValidPackageType = Enum.TryParse(type, true, out PackageType uploadedPackageType);
            if (!isValidPackageType)
            {
                throw new InvalidInputException($"Provided package type {type} is not valid.");
            }

            bool isValidConfigType = Enum.TryParse(configType, true, out ConfigType uploadedConfigType);
            if (!isValidConfigType && type.Equals(PackageType.DeviceConfiguration))
            {
                //TODO Log that user has selected a Custom config Type rather than pre-defined type. 
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

        private string appendCustomConfig(string config, string customConfig)
        {
            return config.ToString() + " - " + customConfig.ToString();
        }
    }
}