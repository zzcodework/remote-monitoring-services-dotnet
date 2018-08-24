// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeploymentServiceListModel
    {
        public List<DeploymentServiceModel> Items { get; set; }

        public DeploymentServiceListModel(List<DeploymentServiceModel> items)
        {
            this.Items = items;
        }

        public DeploymentServiceListModel(IEnumerable<Configuration> configs)
        {   
            configs.AsParallel().Select(config => new DeploymentServiceModel(config))
                                .OrderBy(conf => conf.Name)
                                .ToList();

        }
    }
}
