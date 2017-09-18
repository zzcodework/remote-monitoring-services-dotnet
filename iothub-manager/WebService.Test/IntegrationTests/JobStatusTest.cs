// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Newtonsoft.Json;
using WebService.Test.helpers;
using WebService.Test.helpers.Http;
using Xunit;
using Xunit.Abstractions;

namespace WebService.Test.IntegrationTests
{
    public class JobStatusTest
    {
        private readonly HttpClient httpClient;

        // Pull Request don't have access to secret credentials, which are
        // required to run tests interacting with Azure IoT Hub.
        // The tests should run when working locally and when merging branches.
        private readonly bool credentialsAvailable;

        public JobStatusTest(ITestOutputHelper log)
        {
            this.httpClient = new HttpClient(log);
            this.credentialsAvailable = !CIVariableHelper.IsPullRequest(log);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void ScheduleJobIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var deviceId = "testDevice1";
            var oldTagValue = "oldTagValue";
            var newTagValue = "newTagValue";

            var tags = new Dictionary<string, string>();
            tags.Add("testTag", oldTagValue);
            var device = this.CreateDeviceIfNotExists(deviceId);

            try
            {
                var updateTwin = new JobUpdateTwinApiModel();
                updateTwin.Tags["testTag"] = newTagValue;

                var jobId = "jobTest" + DateTime.Now.Ticks;
                var jobApi = new JobApiModel()
                {
                    JobId = jobId,
                    QueryCondition = $"tags.testTag = '{oldTagValue}'",
                    UpdateTwin = updateTwin,
                    MaxExecutionTimeInSeconds = 0
                };

                var request = new HttpRequest();
                request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/jobs");
                request.SetContent(jobApi);
                var response = this.httpClient.PostAsync(request).Result;

                // Check
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var job = JsonConvert.DeserializeObject<JobApiModel>(response.Content);
                Assert.Equal(jobId, job.JobId);
                Assert.Equal(JobStatus.Queued, job.Status);
                Assert.Equal(JobType.ScheduleUpdateTwin, job.Type);

                //query job
                request = new HttpRequest();
                request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/jobs/{jobId}");
                response = this.httpClient.GetAsync(request).Result;

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                job = JsonConvert.DeserializeObject<JobApiModel>(response.Content);
                Assert.Equal(jobId, job.JobId);
            }
            finally
            {
                this.DeleteDeviceIfExists(deviceId);
            }
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetJobsIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/jobs");
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var jobs = JsonConvert.DeserializeObject<IEnumerable<JobApiModel>>(response.Content);
            Assert.True(jobs.Count() >= 0);

            // check by jobType:
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/jobs?jobType=ScheduleUpdateTwin");
            response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            jobs = JsonConvert.DeserializeObject<IEnumerable<JobApiModel>>(response.Content);
            Assert.True(jobs.Count() >= 0);
        }

        private DeviceRegistryApiModel GetDevice(string deviceId)
        {
            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
            var response = this.httpClient.GetAsync(request).Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);
        }

        private DeviceRegistryApiModel CreateDeviceIfNotExists(string deviceId, Dictionary<string, string> tags = null)
        {
            var device = this.GetDevice(deviceId);
            if (device != null)
            {
                return device;
            }

            device = new DeviceRegistryApiModel()
            {
                Id = deviceId,
            };

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    device.Tags.Add(tag.Key, tag.Value);
                }
            }

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
            request.SetContent(device);

            var response = this.httpClient.PutAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);
        }

        private void DeleteDeviceIfExists(string deviceId)
        {
            var device = this.GetDevice(deviceId);
            if (device != null)
            {
                var request = new HttpRequest();
                request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
                var response = this.httpClient.DeleteAsync(request).Result;

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
