// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions
{
    public class EmailActionSettings : IActionSettings
    {
        public ActionType Type { get; }

        public IDictionary<string, object> Settings { get; set; }

        public EmailActionSettings()
        {
            this.Type = ActionType.Email;
        }

        private string getToken()
        {
            // TODO
            return string.Empty;
        }
    }
}
