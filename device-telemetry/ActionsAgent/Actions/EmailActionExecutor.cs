// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IEmailActionExecutor
    {
        // Execute the given email action for the given alarm
        Task Execute(EmailAction emailAction, AsaAlarmApiModel alarm);
    }

    public class EmailActionExecutor : IEmailActionExecutor
    {
        private readonly string logicAppEndpointUrl;
        private readonly IHttpClient httpClient;
        private readonly string solutionName;
        private readonly ILogger logger;

        public EmailActionExecutor(
            string logicAppEndpointUrl, 
            IHttpClient httpClient, 
            string solutionName,
            ILogger logger)
        {
            this.logicAppEndpointUrl = logicAppEndpointUrl;
            this.httpClient = httpClient;
            this.solutionName = solutionName;
            this.logger = logger;
        }

        public async Task Execute(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            try
            {
                string payload = this.GeneratePayload(emailAction, alarm);
                HttpRequest httpRequest = new HttpRequest(this.logicAppEndpointUrl);
                httpRequest.SetContent(payload);
                IHttpResponse response = await this.httpClient.PostAsync(httpRequest);
                if (!response.IsSuccess)
                {
                    this.logger.Error("Could not execute email action against logic app", () => { });
                }
            }
            catch (JsonException e)
            {
                this.logger.Error("Could not create email payload to send to logic app,", () => new { e });
            }
            catch (Exception e)
            {
                this.logger.Error("Could not execute email action against logic app", () => new { e });
            }
        }

        private string GeneratePayload(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            string emailHeader = "<head><style>.email-body {{ font-family:\"Segoe UI\";width: 300px;margin: auto;color: #3e4145;}}</style></head>";
            string emailBody = "<body class=\"email-body\"><h1>{0}</h1><h2>Details</h2><p><b>Time Triggered:</b> {1}</p><p><b>Rule Id:</b> {2}</p>" +
                               "<p><b>Rule Description:</b> {3}</p><p><b>Severity:</b> {4}</p><p><b>Device Id:</b> {5}</p>" + 
                               "<h2>Notes</h2><p>{6}</p><p>See alert and device details <a href=\"{7}\">here</a></p></body>";
            string emailFormatString = emailHeader + emailBody;
            string alarmDate = DateTimeOffset.FromUnixTimeMilliseconds(alarm.DateCreated).ToString("r");
            string emailContent = String.Format(emailFormatString, 
                emailAction.GetSubject(), 
                alarmDate, 
                alarm.RuleId, 
                alarm.RuleDescription, 
                alarm.RuleSeverity,
                alarm.DeviceId,
                emailAction.GetNotes(), 
                this.GenerateRuleDetailUrl(alarm.RuleId));

            EmailActionPayload payload = new EmailActionPayload()
            {
                Recipients = emailAction.GetRecipients(),
                Subject = emailAction.GetSubject(),
                Body = emailContent
            };

            return JsonConvert.SerializeObject(payload);
        }

        private string GenerateRuleDetailUrl(string ruleId)
        {
            return "https://" + this.solutionName + ".azurewebsites.net/maintenance/rule/" + ruleId;
        }

        private class EmailActionPayload
        {
            [JsonProperty(PropertyName = "recipients")]
            public List<string> Recipients { get; set; }

            [JsonProperty(PropertyName = "body")]
            public string Body { get; set; }

            [JsonProperty(PropertyName = "subject")]
            public string Subject { get; set; }
        }


    }
}
