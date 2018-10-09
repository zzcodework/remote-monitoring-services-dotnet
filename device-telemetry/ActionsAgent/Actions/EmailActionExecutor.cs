// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public interface IEmailActionExecutor
    {
        Task<bool> Execute(EmailAction emailAction, AsaAlarmApiModel alarm);
    }

    public class EmailActionExecutor : IEmailActionExecutor
    {
        private readonly string logicAppEndpointUrl;
        private readonly IHttpClient httpClient;
        private readonly string solutionName;

        public EmailActionExecutor(string logicAppEndpointUrl, IHttpClient httpClient, string solutionName)
        {
            this.logicAppEndpointUrl = logicAppEndpointUrl;
            this.httpClient = httpClient;
            this.solutionName = solutionName;
        }

        public async Task<bool> Execute(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            string payload = this.GeneratePayload(emailAction, alarm);
            HttpRequest httpRequest = new HttpRequest(this.logicAppEndpointUrl);
            httpRequest.SetContent(payload);
            IHttpResponse response = await this.httpClient.PostAsync(httpRequest);
            return response.IsSuccess;
        }

        private string GeneratePayload(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            var emailContent = "<p>Alarm fired for rule ID: " + alarm.RuleId + "  Rule Description: " +
                               alarm.RuleDescription + " Notes: " + emailAction.GetNotes() + " Alarm Detail Page: " + this.GenerateRuleDetailUrl(alarm.RuleId) + "</p>";

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
