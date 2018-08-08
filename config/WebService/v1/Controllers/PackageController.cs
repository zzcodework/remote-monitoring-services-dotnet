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
            return new PackageListApiModel(await this.storage.GetAllPackagesAync());
        }

        [HttpGet("{id}")]
        public async Task<PackageApiModel> GetAsync(string id)
        {
            return new PackageApiModel(await this.storage.GetPackageAsync(id));
        }

        [HttpPost]
        public async Task<PackageApiModel> PostAsync(string type, IFormFile package)
        {
            bool isValidPackageType = Enum.TryParse(type, out PackageType uploadedPackageType);
            
            if(!isValidPackageType)
            {
                throw new InvalidInputException($"Provided package type {type} is not valid.");
            }

            if(string.IsNullOrWhiteSpace(package.FileName) || package.Length == 0)
            {
                throw new InvalidInputException("File upload is invalid. Please check the input");
            }

            var packageContent = string.Empty;
            using(var streamReader = new StreamReader(package.OpenReadStream()))
            {
                packageContent = await streamReader.ReadToEndAsync();
            }

            var packageToAdd = new Package()
            {
                Content = packageContent,
                Name = package.FileName,
                Type = uploadedPackageType
            };
            return new PackageApiModel(await this.storage.AddPackageAsync(packageToAdd));
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await this.storage.DeletePackageAsync(id);
        }
    }
}
