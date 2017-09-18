// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class JobsController : Controller
    {
        private readonly IJobs jobs;

        public JobsController(IJobs jobs)
        {
            this.jobs = jobs;
        }

        /// <summary>
        /// Get list of jobs by status/type
        /// </summary>
        /// <param name="jobType">The type of job</param>
        /// <param name="jobStatus">The status of job</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>The list of jobs</returns>
        [HttpGet]
        public async Task<IEnumerable<JobApiModel>> GetAsync([FromQuery] JobType? jobType, [FromQuery] JobStatus? jobStatus, [FromQuery] int? pageSize)
        {
            var result = await this.jobs.GetJobsAsync(jobType, jobStatus, pageSize);
            return result.Select(r => new JobApiModel(r));
        }

        /// <summary>
        /// Get job status by jobId
        /// </summary>
        /// <param name="jobId">The jobId</param>
        /// <returns>The job object</returns>
        [HttpGet("{jobId}")]
        public async Task<JobApiModel> GetJobAsync(string jobId)
        {
            var result = await this.jobs.GetJobsAsync(jobId);
            return new JobApiModel(result);
        }

        /// <summary>
        /// Schedule job
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JobApiModel> ScheduleAsync([FromBody] JobApiModel parameter)
        {
            if (parameter.UpdateTwin != null)
            {
                var result = await this.jobs.ScheduleTwinUpdateAsync(parameter.JobId, parameter.QueryCondition, parameter.UpdateTwin.ToServiceModel(), parameter.StartTimeUtc ?? DateTime.UtcNow, parameter.MaxExecutionTimeInSeconds);
                return new JobApiModel(result);
            }

            if (parameter.MethodParameter != null)
            {
                var result = await this.jobs.ScheduleDeviceMethodAsync(parameter.JobId, parameter.QueryCondition, parameter.MethodParameter.ToServiceModel(), parameter.StartTimeUtc ?? DateTime.UtcNow, parameter.MaxExecutionTimeInSeconds);
                return new JobApiModel(result);
            }

            throw new NotSupportedException();
        }
    }
}
