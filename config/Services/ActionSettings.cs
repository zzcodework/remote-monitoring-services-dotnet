// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface IActionSettings
    {
        Task<List<ActionSettings>> GetListAsync();
    }

    public class ActionSettings : IActionSettings
    {
        private readonly ILogger log;
        private readonly IUserManagementClient userManagementClient;

        public ActionSettings(
            IUserManagementClient userManagementClient,
            ILogger log)
        {

        }

        public Task <List<ActionSettings>> GetListAsync()
        {

            throw new NotImplementedException();
        }
    }
}
