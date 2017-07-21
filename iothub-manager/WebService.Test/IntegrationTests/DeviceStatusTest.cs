// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using WebService.Test.helpers;
using WebService.Test.helpers.Http;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System;
using System.Collections.Generic;

namespace WebService.Test.IntegrationTests
{
    public class DevicesStatusTest
    {
        private readonly HttpClient httpClient;

        // Pull Request don't have access to secret credentials, which are
        // required to run tests interacting with Azure IoT Hub.
        // The tests should run when working locally and when merging branches.
        private readonly bool credentialsAvailable;

        public DevicesStatusTest(ITestOutputHelper log)
        {
            this.httpClient = new HttpClient(log);
            this.credentialsAvailable = !CIVariableHelper.IsPullRequest(log);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/devices");

            // Act
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceByIdIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + "/v1/devices/foobar");

            // Act
            var response = this.httpClient.GetAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void CreateDeleteDeviceIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var deviceId = "testDevice123";
            this.DeleteDeviceIfExists(deviceId);

            // create device
            var device = new DeviceRegistryApiModel()
            {
                Id = deviceId
            };

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices");
            request.SetContent(device);

            var response = this.httpClient.PostAsync(request).Result;
            var azureDevice = JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);

            Assert.Equal(deviceId, azureDevice.Id);

            // clean it up
            this.DeleteDeviceIfExists(deviceId);
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void UpdateTwinIsHealthy()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var deviceId = "testDevice1";
            var device = this.CreateDeviceIfNotExists(deviceId);

            try
            {
                var newTagValue = System.Guid.NewGuid().ToString();

                // update twin by adding/editing a tag
                if (device.Twin.Tags.ContainsKey("newTag"))
                {
                    device.Twin.Tags["newTag"] = newTagValue;
                }
                else
                {
                    device.Twin.Tags.Add("newTag", newTagValue);
                }

                var newConfig = new NewConfig
                {
                    TelemetryInterval = 10,
                    TelemetryType = "Type1;Type2"
                };

                // update twin by adding config 
                var configValue = JToken.Parse(JsonConvert.SerializeObject(newConfig));
                if (device.Twin.DesiredProperties.ContainsKey("config"))
                {
                    device.Twin.DesiredProperties["config"] = configValue;
                }
                else
                {
                    device.Twin.DesiredProperties.Add("config", configValue);
                }

                var request = new HttpRequest();
                request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
                request.SetContent(device);

                var response = this.httpClient.PutAsync(request).Result;

                device = JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);
                Assert.Equal(newTagValue, device.Twin.Tags["newTag"]);

                // get device again
                device = this.GetDevice(deviceId);
                Assert.Equal(newTagValue, device.Twin.Tags["newTag"]);

                var configInTwin = JsonConvert.DeserializeObject<NewConfig>(device.Twin.DesiredProperties["config"].ToString());
                Assert.Equal(10, configInTwin.TelemetryInterval);

            }
            finally
            {
                // clean it up
                this.DeleteDeviceIfExists(deviceId);
            }
        }

        public class NewConfig
        {
            public int TelemetryInterval { get; set; }
            public string TelemetryType { get; set; }
        }

        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceListTest()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var deviceId = "testDevice-getdeviceList";
            var device = this.CreateDeviceIfNotExists(deviceId);

            try
            {
                var request = new HttpRequest();
                request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices");
                var response = this.httpClient.GetAsync(request).Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var deviceList = JsonConvert.DeserializeObject<DeviceListApiModel>(response.Content);
                Assert.True(deviceList.Items.Count >= 1);
                Assert.True(deviceList.Items.Any(d => d.Id == deviceId));
            }
            finally
            {
                // clean it up
                this.DeleteDeviceIfExists(deviceId);
            }
        }
        
        [SkippableFact, Trait(Constants.Type, Constants.IntegrationTest)]
        public void GetDeviceListWithQueryTest()
        {
            Skip.IfNot(this.credentialsAvailable, "Skipping this test for Travis pull request as credentials are not available");

            var deviceId = "testDevice-getdeviceListWithReportedQuery";
            var device = this.CreateDeviceIfNotExists(deviceId);

            try
            {
                var newTagValue = $"mynewTag{DateTime.Now.Ticks}";

                // update newTag/desired property
                {
                    if (device.Twin.Tags.ContainsKey("newTag"))
                    {
                        device.Twin.Tags["newTag"] = newTagValue;
                    }
                    else
                    {
                        device.Twin.Tags.Add("newTag", newTagValue);
                    }

                    var newConfig = new NewConfig
                    {
                        TelemetryInterval = 10
                    };

                    // update desired value
                    var configValue = JToken.Parse(JsonConvert.SerializeObject(newConfig));
                    if (device.Twin.DesiredProperties.ContainsKey("config"))
                    {
                        device.Twin.DesiredProperties["config"] = configValue;
                    }
                    else
                    {
                        device.Twin.DesiredProperties.Add("config", configValue);
                    }

                    var newReport = new NewConfig
                    {
                        TelemetryInterval = 15
                    };

                    var request = new HttpRequest();
                    request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
                    request.SetContent(device);

                    var response = this.httpClient.PutAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }

                // verify
                var queries = new List<string>();
                queries.Add(WebUtility.UrlEncode($"tags.newTag = \"{newTagValue}\""));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval = 10"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval > 9"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval >= 10"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval < 11"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval <= 10"));
                queries.Add(WebUtility.UrlEncode($"tags.newTag = \"{newTagValue}\" and desired.config.TelemetryInterval = 10"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval = 10"));

                foreach (var query in queries)
                {
                    var request = new HttpRequest();
                    request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices?query={query}");
                    var response = this.httpClient.GetAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    var deviceList = JsonConvert.DeserializeObject<DeviceListApiModel>(response.Content);
                    Assert.True(deviceList.Items.Count >= 1, query);
                    Assert.True(deviceList.Items.Any(d => d.Id == deviceId), query);
                }

                queries.Clear();
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval != 10"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval < 9"));
                queries.Add(WebUtility.UrlEncode($"desired.config.TelemetryInterval > 10 and desired.config.TelemetryInterval = 10"));
                foreach (var query in queries)
                {
                    var request = new HttpRequest();
                    request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices?query={query}");
                    var response = this.httpClient.GetAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    var deviceList = JsonConvert.DeserializeObject<DeviceListApiModel>(response.Content);
                    Assert.True(!deviceList.Items.Any(d => d.Id == deviceId), query);
                }

            }
            finally
            {
                // clean it up
                this.DeleteDeviceIfExists(deviceId);
            }
        }

        private DeviceRegistryApiModel GetDevice(string deviceId)
        {
            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
            var response = this.httpClient.GetAsync(request).Result;

            if(response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);
        }

        private DeviceRegistryApiModel CreateDeviceIfNotExists(string deviceId)
        {
            var device = GetDevice(deviceId);
            if( device != null)
            {
                return device;
            }

            device = new DeviceRegistryApiModel()
            {
                Id = deviceId
            };

            var request = new HttpRequest();
            request.SetUriFromString(AssemblyInitialize.Current.WsHostname + $"/v1/devices/{deviceId}");
            request.SetContent(device);

            var response = this.httpClient.PutAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return JsonConvert.DeserializeObject<DeviceRegistryApiModel>(response.Content);
        }

        private void DeleteDeviceIfExists(string deviceId)
        {
            var device = GetDevice(deviceId);
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
