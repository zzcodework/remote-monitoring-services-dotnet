// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IActionManager
    {
        Task ExecuteAlarmActions(string alarms);
    }

    public class ActionManager : IActionManager
    {
        private readonly ILogger logger;
        private readonly IEmailActionExecutor emailActionExecutor;

        public ActionManager(ILogger logger, IServicesConfig servicesConfig, IHttpClient httpClient)
        {
            this.logger = logger;
            this.emailActionExecutor = new EmailActionExecutor(
                servicesConfig.LogicAppEndpointUrl,
                httpClient,
                servicesConfig.SolutionName,
                this.logger);
        }

        public async Task ExecuteAlarmActions(string alarms)
        {
            IEnumerable<AsaAlarmApiModel> alarmList = ActionParser.ParseAlarmList(alarms, this.logger);
            alarmList = alarmList.Where(x => x.Actions != null && x.Actions.Count > 0);
            foreach (var alarm in alarmList)
            {
                foreach (var action in alarm.Actions)
                {
                    switch (action.Type)
                    {
                        case ActionType.Email:
                            await this.emailActionExecutor.Execute((EmailAction)action, alarm);
                            break;
                    }
                }
            }
        }
    }
}
