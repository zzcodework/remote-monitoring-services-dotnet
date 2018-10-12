// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.Actions;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }

    public class Actions : IActions
    {
        private readonly ILogger log;
        private readonly IUserManagementClient userManagementClient;

        public Actions(
            IUserManagementClient userManagementClient,
            ILogger log)
        {
            this.userManagementClient = userManagementClient;
            this.log = log;
        }

        public Task <List<IActionSettings>> GetListAsync(string token)
        {
            var result = new List<IActionSettings>();

            // Get Email Action Settings
            result.Add(await new EmailActionSettings(this.userManagementClient, token));

            return result;
        }
    }
}
