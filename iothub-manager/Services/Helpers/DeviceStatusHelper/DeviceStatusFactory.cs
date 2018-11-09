using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers.DeviceStatusHelper
{
    public class DeviceStatusFactory
    {
        private static DeploymentType GetDeploymentType(Configuration deployment)
        {
            if (deployment?.Content == null)
            {
                throw new ArgumentNullException("Invalid deployment Or deployment object");
            }

            if (deployment.Content.ModulesContent != null
                && deployment.Content.DeviceContent == null)
            {
                return DeploymentType.EdgeManifest;
            }
            else if (deployment.Content.ModulesContent == null
                && (deployment.Content.DeviceContent != null))
            {
                return DeploymentType.DeviceConfiguration;
            }

            return DeploymentType.DeviceConfiguration;//TODO: Change to both
        }

        public static DeviceStatus GetDeviceStatusApi(Configuration deployment, 
                                                      RegistryManager registry, 
                                                      ILogger log)
        {
            string id = deployment.Id;
            if (GetDeploymentType(deployment).Equals(DeploymentType.EdgeManifest))
            {
                return new EdgeDeviceStatus(id, registry, log);
            }
            else if (GetDeploymentType(deployment).Equals(DeploymentType.DeviceConfiguration))
            {
                return new ADMDeviceStatus(id, registry, log);
            }
            return new ADMDeviceStatus(id, registry, log);//TODO Change to Both
        }
    }
}
