using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using static Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.DeviceStatusQueries;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class FirmwareUpdateMxChipStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, "SELECT deviceId from devices where configurations.[[{0}]].status = 'Applied'"},
            { QueryType.SUCCESSFUL, "SELECT deviceId FROM devices WHERE properties.reported.firmware.fwUpdateStatus='Current' " +
                "AND properties.reported.firmware.type='IoTDevKit'"},
            { QueryType.FAILED, "SELECT deviceId FROM devices WHERE properties.reported.firmware.fwUpdateStatus='Error' " +
                "AND properties.reported.firmware.type='IoTDevKit'"}
        };
    }

    public class EdgeDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, "select deviceId from devices.modules where moduleId = '$edgeAgent' and " +
                "configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, "select deviceId from devices.modules where moduleId = '$edgeAgent' and " +
                "properties.desired.$version = properties.reported.lastDesiredVersion and " +
                "properties.reported.lastDesiredStatus.code = 200" },
            { QueryType.FAILED, "SELECT deviceId FROM devices WHERE properties.reported.firmware.fwUpdateStatus='Error'" +
                " AND properties.reported.firmware.type='IoTDevKit'" }
        };
    }

    public class DefaultDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, "SELECT deviceId from devices where configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, String.Empty },
            { QueryType.FAILED, String.Empty }
        };
    }

    public class DeviceStatusQueries {

        private static Dictionary<string, IDictionary<QueryType, string>> AdmQueryMapping =
            new Dictionary<string, IDictionary<QueryType, string>>()
        {
            { ConfigType.FirmwareUpdateMxChip.ToString(),
                    FirmwareUpdateMxChipStatusQueries.Queries }
        };

        internal static IDictionary<QueryType, string> GetQueries(string deploymentType, string configType)
        {
            if (deploymentType.Equals(DeploymentType.EdgeManifest.ToString()))
            {
                return EdgeDeviceStatusQueries.Queries;
            }

            return AdmQueryMapping.TryGetValue(configType, 
                    out IDictionary<QueryType, string> value) ? value : DefaultDeviceStatusQueries.Queries;
        }

        public enum QueryType { APPLIED, SUCCESSFUL, FAILED };
    }
}
