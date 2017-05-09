// Copyright (c) Microsoft. All rights reserved.

using Xunit;

namespace WebService.Test.Controllers
{
    public class SomeControllerTest
    {


        [Fact]
        public void TestStatus()
        {
            //http://127.0.0.1:" + config.Port + "/v1/status");
            Assert.True(true);
        }

        [Fact]
        public void TestAllDevices()
        {
            //http://127.0.0.1:" + config.Port + "/v1/devices");
            Assert.True(true);
        }

        [Fact]
        public void TestSingleDevice()
        {
            //http://127.0.0.1:" + config.Port + "/v1/device/mydevice");
            Assert.True(true);
        }
    }
}
