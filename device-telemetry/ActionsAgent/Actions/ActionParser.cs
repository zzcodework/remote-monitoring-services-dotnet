// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public static class ActionParser
    {
        public static IEnumerable<AsaAlarmApiModel> ParseAlarmList(string alarms, ILogger logger)
        {
            IList<AsaAlarmApiModel> alarmList = new List<AsaAlarmApiModel>();
            JsonSerializer serializer = new JsonSerializer();
            if (!String.IsNullOrEmpty(alarms))
            {
                using (JsonTextReader reader = new JsonTextReader(new StringReader(alarms)))
                {
                    reader.SupportMultipleContent = true;
                    while (reader.Read())
                    {
                        try
                        {
                            alarmList.Add(serializer.Deserialize<AsaAlarmApiModel>(reader));
                        }
                        catch (Exception e)
                        {
                            logger.Info("Exception parsing the json string", () => new { e });
                            break;
                        }
                    }
                }
            }

            return alarmList;
        }
    }
}
