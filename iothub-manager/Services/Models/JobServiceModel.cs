// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class JobServiceModel
    {
        public string JobId { get; set; }

        public string QueryCondition { get; set; }

        public DateTime? CreatedTimeUtc { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public long MaxExecutionTimeInSeconds { get; set; }

        public JobType Type { get; set; }

        public JobStatus Status { get; set; }

        public MethodParameterServiceModel MethodParameter { get; set; }

        public DeviceTwinServiceModel UpdateTwin { get; set; }

        public string FailureReason { get; set; }

        public string StatusMessage { get; set; }

        public JobStatistics ResultStatistics { get; set; }

        public JobServiceModel()
        {
        }

        public JobServiceModel(JobResponse jobResponse)
        {
            this.JobId = jobResponse.JobId;
            this.QueryCondition = jobResponse.QueryCondition;
            this.CreatedTimeUtc = jobResponse.CreatedTimeUtc;
            this.StartTimeUtc = jobResponse.StartTimeUtc;
            this.EndTimeUtc = jobResponse.EndTimeUtc;
            this.MaxExecutionTimeInSeconds = jobResponse.MaxExecutionTimeInSeconds;

            switch (jobResponse.Type)
            {
                case Azure.Devices.JobType.ScheduleDeviceMethod:
                case Azure.Devices.JobType.ScheduleUpdateTwin:
                    this.Type = (JobType) jobResponse.Type;
                    break;
                default:
                    this.Type = JobType.Unknown;
                    break;
            }

            switch (jobResponse.Status)
            {
                case Azure.Devices.JobStatus.Enqueued:
                case Azure.Devices.JobStatus.Running:
                case Azure.Devices.JobStatus.Completed:
                case Azure.Devices.JobStatus.Failed:
                case Azure.Devices.JobStatus.Cancelled:
                case Azure.Devices.JobStatus.Scheduled:
                case Azure.Devices.JobStatus.Queued:
                    this.Status = (JobStatus) jobResponse.Status;
                    break;
                default:
                    this.Status = JobStatus.Unknown;
                    break;
            }
            //this.Type =
            //this.Status =

            if (jobResponse.CloudToDeviceMethod != null)
            {
                this.MethodParameter = new MethodParameterServiceModel(jobResponse.CloudToDeviceMethod);
            }

            if (jobResponse.UpdateTwin != null)
            {
                this.UpdateTwin = new DeviceTwinServiceModel(jobResponse.UpdateTwin);
            }

            this.FailureReason = jobResponse.FailureReason;
            this.StatusMessage = jobResponse.StatusMessage;

            if (jobResponse.DeviceJobStatistics != null)
            {
                this.ResultStatistics = new JobStatistics(jobResponse.DeviceJobStatistics);
            }
        }

        public static Azure.Devices.JobType? ToJobTypeAzureModel(JobType? jobType)
        {
            if (!jobType.HasValue)
            {
                return null;
            }

            switch (jobType.Value)
            {
                case JobType.ScheduleDeviceMethod:
                case JobType.ScheduleUpdateTwin:
                    return (Azure.Devices.JobType) jobType.Value;
                default:
                    return (Azure.Devices.JobType) JobType.Unknown;
            }
        }

        public static Azure.Devices.JobStatus? ToJobStatusAzureModel(JobStatus? jobStatus)
        {
            if (!jobStatus.HasValue)
            {
                return null;
            }

            switch (jobStatus.Value)
            {
                case JobStatus.Enqueued:
                case JobStatus.Running:
                case JobStatus.Completed:
                case JobStatus.Failed:
                case JobStatus.Cancelled:
                case JobStatus.Scheduled:
                case JobStatus.Queued:
                    return (Azure.Devices.JobStatus) jobStatus.Value;
                default:
                    return Azure.Devices.JobStatus.Unknown;
            }
        }
    }

    /// <summary>
    /// refer to Microsoft.Azure.Devices.JobType
    /// </summary>
    public enum JobType
    {
        Unknown = 0,
        ScheduleDeviceMethod = 3,
        ScheduleUpdateTwin = 4
    }

    /// <summary>
    /// refer to Microsoft.Azure.Devices.JobStatus
    /// </summary>
    public enum JobStatus
    {
        Unknown = 0,
        Enqueued = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Scheduled = 6,
        Queued = 7
    }
}
