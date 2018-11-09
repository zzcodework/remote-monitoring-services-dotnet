using System.Collections.Generic;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;


namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers.DeviceStatusHelper
{
    public class EdgeDeviceStatus : DeviceStatus
    {
        private string deploymentId;
        public EdgeDeviceStatus(string deploymentId,
                                RegistryManager registry, 
                                ILogger log) : base(registry, log)
        {
            this.deploymentId = deploymentId;
        }

        private const string APPLIED_DEVICES_QUERY =
            "select deviceId from devices.modules where moduleId = '$edgeAgent'" +
            " and configurations.[[{0}]].status = 'Applied'";

        private const string SUCCESSFUL_DEVICES_QUERY = APPLIED_DEVICES_QUERY +
            " and properties.desired.$version = properties.reported.lastDesiredVersion" +
            " and properties.reported.lastDesiredStatus.code = 200";

        private const string FAILED_DEVICES_QUERY = APPLIED_DEVICES_QUERY +
            " and properties.desired.$version = properties.reported.lastDesiredVersion" +
            " and properties.reported.lastDesiredStatus.code != 200";

        public override IDictionary<string, DeploymentStatus> GetDeviceStatuses()
        {
            var appliedDevices = this.GetDevicesInQuery(APPLIED_DEVICES_QUERY, this.deploymentId);
            var successfulDevices = this.GetDevicesInQuery(SUCCESSFUL_DEVICES_QUERY, this.deploymentId);
            var failedDevices = this.GetDevicesInQuery(FAILED_DEVICES_QUERY, this.deploymentId);

            var deviceWithStatus = new Dictionary<string, DeploymentStatus>();

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

            return deviceWithStatus;
        }
    }
}
