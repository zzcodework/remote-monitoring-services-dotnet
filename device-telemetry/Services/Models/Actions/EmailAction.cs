// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Converters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions
{
    public class EmailAction : IAction
    {
        private const string SUBJECT = "Subject";
        private const string NOTES = "Notes";
        private const string RECIPIENTS = "Recipients";

        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }

        // Note: Parameters should always be initialized as a case-insensitive dictionary
        [JsonConverter(typeof(EmailParametersConverter))]
        public IDictionary<string, object> Parameters { get; set; }

        public EmailAction()
        {
            this.Type = ActionType.Email;
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public EmailAction(IDictionary<string, object> parameters)
        {
            this.Type = ActionType.Email;
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Ensure input is in case-insensitive dictionary
            parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);

            if (!(parameters.ContainsKey(NOTES) &&
                  parameters.ContainsKey(SUBJECT) &&
                  parameters.ContainsKey(RECIPIENTS)))
            {
                throw new InvalidInputException("Error, missing parameter for email action. Required fields are: " +
                                                $"'{SUBJECT}', '{NOTES}', and '{RECIPIENTS}'.");
            }

            this.Parameters[NOTES] = parameters[NOTES];
            this.Parameters[SUBJECT] = parameters[SUBJECT];
            this.Parameters[RECIPIENTS] = this.ValidateAndConvertRecipientEmails(parameters[RECIPIENTS]);
        }

        /// <summary>
        /// Validates recipient email addresses and converts to a list of email strings
        /// </summary>
        private List<string> ValidateAndConvertRecipientEmails(Object emails)
        {
            List<string> result = new List<string>();

            try
            {
                result = ((JArray)emails).ToObject<List<string>>();
            }
            catch (Exception e)
            {
                throw new InvalidInputException("Error converting recipient emails to list for action type 'Email'. " +
                                                "Recipient emails provided should be an array of valid email addresses" +
                                                "as strings.");
            }

            if (!result.Any())
            {
                throw new InvalidInputException("Error, recipient email list for action type 'Email' is empty. " +
                                                "Please provide at least one valid email address.");
            }

            foreach (var email in result)
            {
                try
                {
                    // validate with attempt to create MailAddress type from string
                    var address = new MailAddress(email);
                }
                catch (Exception e)
                {
                    throw new InvalidInputException("Error with recipient email format for action type 'Email'." +
                                                    "Invalid email provided. Please ensure at least one recipient " +
                                                    "email address is provided and that all recipient email addresses " +
                                                    "are valid.");
                }
            }

            return result;
        }
    }
}
