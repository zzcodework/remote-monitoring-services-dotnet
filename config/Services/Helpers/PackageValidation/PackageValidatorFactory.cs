using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{


    public static class PackageValidatorFactory
    {
        private static Dictionary<ConfigType, IPackageValidator> validatorMapping =
            new Dictionary<ConfigType, IPackageValidator>()
        {
            { ConfigType.FirmwareUpdateMxChip, new FirmwareUpdateMxChipValidator() }
        };

        //TODO:Return double checked singleton objects
        public static IPackageValidator GetValidator(PackageType packageType, string configType)
        {
            if (packageType.Equals(PackageType.EdgeManifest))
            {
                return new EdgePackageValidator();
            }

            return validatorMapping.TryGetValue(ConfigType.FirmwareUpdateMxChip, out IPackageValidator value) ? value : null;
        }
    }
}
