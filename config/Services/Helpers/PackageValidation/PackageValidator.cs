using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{
    public interface IPackageValidator
    {
        JObject GetPackageContent(string package);
        Boolean Validate();
    }

    public abstract class PackageValidator : IPackageValidator
    {
        JObject IPackageValidator.GetPackageContent(string package) {
            try
            {
                return JObject.Parse(package);
            }
            catch (JsonReaderException e)
            {
                throw new InvalidInputException($"Provided package is not a valid json. Error: {e.Message}.");
            }
        }

        public abstract Boolean Validate();
    }
}
