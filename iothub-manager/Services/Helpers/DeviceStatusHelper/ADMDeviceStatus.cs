using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers.DeviceStatusHelper
{
    public class ADMDeviceStatus : DeviceStatus
    {
        private string deploymentId;

        public ADMDeviceStatus(string deploymentId, 
                               RegistryManager registry, 
                               ILogger log) : base(registry, log)
        {
            this.deploymentId = deploymentId;
        }

        private const string APPLIED_DEVICES_QUERY =
            "select deviceId from devices" +
            " and configurations.[[{0}]].status = 'Applied'";
        /*
        private const string SUCCESSFUL_DEVICES_QUERY = APPLIED_DEVICES_QUERY +
            " and properties.desired.$version = properties.reported.lastDesiredVersion" +
            " and properties.reported.lastDesiredStatus.code = 200";

        private const string FAILED_DEVICES_QUERY = APPLIED_DEVICES_QUERY +
            " and properties.desired.$version = properties.reported.lastDesiredVersion" +
            " and properties.reported.lastDesiredStatus.code != 200";*/

        public override IDictionary<string, DeploymentStatus> GetDeviceStatuses()
        {
            var appliedDevices = this.GetDevicesInQuery(APPLIED_DEVICES_QUERY, this.deploymentId);
            //var successfulDevices = this.GetDevicesInQuery(SUCCESSFUL_DEVICES_QUERY, deploymentId);
            //var failedDevices = this.GetDevicesInQuery(FAILED_DEVICES_QUERY, deploymentId);

            var deviceWithStatus = new Dictionary<string, DeploymentStatus>();

            foreach (var device in appliedDevices)
            {
                deviceWithStatus.Add(device, DeploymentStatus.UnKnown);
            }

            /*
            foreach (var successfulDevice in successfulDevices)
            {
                deviceWithStatus.Add(successfulDevice, DeploymentStatus.Successful);
            }

            foreach (var failedDevice in failedDevices)
            {
                deviceWithStatus.Add(failedDevice, DeploymentStatus.Failed);
            }

            foreach (var device in appliedDevices)
            {
                if (!successfulDevices.Contains(device) && !failedDevices.Contains(device))
                {
                    deviceWithStatus.Add(device, DeploymentStatus.Pending);
                }
            }
            */
            return deviceWithStatus;
        }
    }
}
