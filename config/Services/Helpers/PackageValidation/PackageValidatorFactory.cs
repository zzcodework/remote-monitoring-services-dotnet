using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{
    public static class PackageValidatorFactory 
    {
        //TODO:Return double checked singleton objects
        public static IPackageValidator GetValidator(PackageType packageType, string configType)
        {
            if (packageType.Equals(PackageType.EdgeManifest))
            {
                return new EdgePackageValidator();
            }
            if (configType.Equals(ConfigType.FirmwareUpdateMxChip.ToString()))
            {
                return new FirmwareUpdateMxChipValidator();
            }
            else
            {
                return null;
            }
        }
    }
}
