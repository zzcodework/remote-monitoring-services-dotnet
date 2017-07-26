// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using System;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class JobApiModel
    {
        public string JobId { get; set; }

        public string QueryCondition { get; set; }

        public DateTime? CreatedTimeUtc { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public long MaxExecutionTimeInSeconds { get; set; }

        public JobType Type { get; set; }

        public JobStatus Status { get; set; }

        public MethodParameterApiModel MethodParameter { get; set; }

        public DeviceTwinApiModel UpdateTwin { get; set; }

        public string FailureReason { get; set; }

        public string StatusMessage { get; set; }

        public JobStatistics ResultStatistics { get; set; }

        public JobApiModel()
        {
        }

        public JobApiModel(JobServiceModel serviceModel)
        {
            if (serviceModel != null)
            {
                this.JobId = serviceModel.JobId;
                this.QueryCondition = serviceModel.QueryCondition;
                this.CreatedTimeUtc = serviceModel.CreatedTimeUtc;
                this.StartTimeUtc = serviceModel.StartTimeUtc;
                this.EndTimeUtc = serviceModel.EndTimeUtc;
                this.MaxExecutionTimeInSeconds = serviceModel.MaxExecutionTimeInSeconds;
                this.Type = serviceModel.Type;
                this.Status = serviceModel.Status;
                this.MethodParameter = new MethodParameterApiModel(serviceModel.MethodParameter);
                this.UpdateTwin = new DeviceTwinApiModel(null, serviceModel.UpdateTwin);
                this.FailureReason = serviceModel.FailureReason;
                this.StatusMessage = serviceModel.StatusMessage;
                this.ResultStatistics = serviceModel.ResultStatistics;
            }
        }

        public JobServiceModel ToServiceModel()
        {
            return new JobServiceModel()
            {
                JobId = this.JobId,
                QueryCondition = this.QueryCondition,
                CreatedTimeUtc = this.CreatedTimeUtc,
                StartTimeUtc = this.StartTimeUtc,
                EndTimeUtc = this.EndTimeUtc,
                MaxExecutionTimeInSeconds = this.MaxExecutionTimeInSeconds,
                Type = this.Type,
                Status = this.Status,
                MethodParameter = this.MethodParameter.ToServiceModel(),
                UpdateTwin = this.UpdateTwin.ToServiceModel(),
                FailureReason = this.FailureReason,
                StatusMessage = this.StatusMessage,
                ResultStatistics = this.ResultStatistics
            };
        }
    }
}
