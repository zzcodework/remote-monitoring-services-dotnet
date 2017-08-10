// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IProtocols
    {
        IEnumerable<ProtocolServiceModel> GetAll();
    }
}
