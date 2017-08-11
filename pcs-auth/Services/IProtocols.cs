// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.Auth.Services.Models;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IProtocols
    {
        ProtocolListServiceModel GetAll();
    }
}
