using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers;
using Moq;
using Xunit;

namespace WebService.Test.Controllers
{
    public class ConfigTypesControllerTest
    {
        private readonly Mock<IStorage> mockStorage;
        private readonly ConfigTypeController controller;
        private readonly Random rand;
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        public ConfigTypesControllerTest()
        {
            this.mockStorage = new Mock<IStorage>();
            this.controller = new ConfigTypeController(this.mockStorage.Object);
            this.rand = new Random();
        }
    }
}
