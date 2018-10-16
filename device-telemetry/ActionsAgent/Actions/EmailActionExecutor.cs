// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions
{
    public class EmailActionExecutor : IActionExecutor
    {
        private readonly IServicesConfig servicesConfig;
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;

        private const string EMAIL_TEMPLATE_FILE_NAME = "EmailTemplate.txt";

        public EmailActionExecutor(
            IServicesConfig servicesConfig,
            IHttpClient httpClient,
            ILogger logger)
        {
            this.servicesConfig = servicesConfig;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        /**
         * Execute the given email action for the given alarm.
         * Sends a post request to Logic App with alarm information
         */
        public async Task Execute(IAction action, object metadata)
        {
            if (metadata.GetType() != typeof(AsaAlarmApiModel) 
                || action.GetType() != typeof(EmailAction))
            {
                this.logger.Error("Email action expects metadata to be alarm and action to be EmailAction, will not send email", 
                    () => {});
                return;
            }

            try
            {
                AsaAlarmApiModel alarm = (AsaAlarmApiModel) metadata;
                EmailAction emailAction = (EmailAction) action;
                string payload = this.GeneratePayload(emailAction, alarm);
                HttpRequest httpRequest = new HttpRequest(this.servicesConfig.LogicAppEndpointUrl);
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

        /**
         * Generate email payload for given alarm and email action.
         * Creates subject, recipients, and body based on action and alarm
         */
        private string GeneratePayload(EmailAction emailAction, AsaAlarmApiModel alarm)
        {
            string emailFormatString = File.ReadAllText(this.servicesConfig.TemplateFolder + EMAIL_TEMPLATE_FILE_NAME);
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

            EmailActionPayload payload = new EmailActionPayload
            {
                Recipients = emailAction.GetRecipients(),
                Subject = emailAction.GetSubject(),
                Body = emailContent
            };

            return JsonConvert.SerializeObject(payload);
        }

        /**
         * Generate URL to direct to maintenance dashboard for specific rule
         */
        private string GenerateRuleDetailUrl(string ruleId)
        {
            return this.servicesConfig.SolutionUrl + "/maintenance/rule/" + ruleId;
        }
    }
}
