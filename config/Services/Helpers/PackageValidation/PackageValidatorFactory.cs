using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers.PackageValidation
{
    public static class PackageValidatorFactory 
    {
        public static IPackageValidator GetValidator(PackageType packageType, ConfigType configType)
        {
            if (packageType.Equals(PackageType.EdgeManifest))
            {
                return new EdgePackageValidator();
            }
            switch (configType) {
                case ConfigType.FirmwareUpdateMxChip:
                    return new FirmwareUpdateMxChipValidator();
                default:
                    return null;
            }
        }
    }
}
