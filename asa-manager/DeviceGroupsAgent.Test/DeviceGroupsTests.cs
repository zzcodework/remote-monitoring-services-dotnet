// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using DeviceGroupsAgent.Test.Helpers;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Xunit;

namespace DeviceGroupsAgent.Test
{
    public class DeviceGroupsTests
    {
        private readonly Mock<ILogger> log;
        private readonly Mock<IFileWrapper> fileWrapper;
        private readonly Mock<ICloudStorageWrapper> cloudStorageWrapper;
        private readonly Mock<IBlobStorageConfig> blobStorageConfig;
        private readonly DateTimeOffset timestamp;
        private readonly string expectedBlobName;
        private readonly string expectedConnectionString;

        private const string REFERENCE_CONTAINER_NAME = "referencedata";
        private const string ACCOUNT_NAME = "blobAccount";
        private const string ACCOUNT_KEY = "xyz";
        private const string ENDPOINT_SUFFIX = "endpoint";
        private const string FILE_NAME = "devicegroups.csv";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string TIME_FORMAT = "HH-mm";
        private const string TEMP_FILE_NAME = "temp.tmp";

        public DeviceGroupsTests()
        {
            this.blobStorageConfig = new Mock<IBlobStorageConfig>();
            this.blobStorageConfig.Setup(x => x.DateFormat).Returns(DATE_FORMAT);
            this.blobStorageConfig.Setup(x => x.TimeFormat).Returns(TIME_FORMAT);
            this.blobStorageConfig.Setup(x => x.DeviceGroupsFileName).Returns(FILE_NAME);
            this.blobStorageConfig.Setup(x => x.AccountName).Returns(ACCOUNT_NAME);
            this.blobStorageConfig.Setup(x => x.AccountKey).Returns(ACCOUNT_KEY);
            this.blobStorageConfig.Setup(x => x.EndpointSuffix).Returns(ENDPOINT_SUFFIX);
            this.blobStorageConfig.Setup(x => x.ReferenceDataContainer).Returns(REFERENCE_CONTAINER_NAME);

            this.log = new Mock<ILogger>();

            this.fileWrapper = new Mock<IFileWrapper>();
            this.fileWrapper.Setup(x => x.GetTempFileName()).Returns(TEMP_FILE_NAME);
            this.fileWrapper.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

            this.cloudStorageWrapper = new Mock<ICloudStorageWrapper>();

            this.timestamp = new DateTime(2018, 4, 20, 10, 0, 0);
            this.expectedBlobName = $"{this.timestamp.ToString(DATE_FORMAT)}/{this.timestamp.ToString(TIME_FORMAT)}/{FILE_NAME}";
            this.expectedConnectionString = $"DefaultEndpointsProtocol=https;AccountName={ACCOUNT_NAME};AccountKey={ACCOUNT_KEY};EndpointSuffix={ENDPOINT_SUFFIX}";
        }

        /**
         * Verifies file name is generated as expected as no errors are thrown
         * when device groups writer is called
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void BasicReferenceDataIsWritten()
        {
            // Arrange
            IBlobStorageHelper blobStorageHelper = new BlobStorageHelper(this.blobStorageConfig.Object, this.cloudStorageWrapper.Object, this.log.Object);

            IDeviceGroupsWriter deviceGroupsWriter = new DeviceGroupsWriter(this.blobStorageConfig.Object,
                blobStorageHelper,
                this.fileWrapper.Object,
                this.log.Object);
            var deviceMapping = new Dictionary<string, IEnumerable<string>>
            {
                ["group1"] = new[] { "device1", "device2", "device3" },
                ["group2"] = new[] { "device6", "device4", "device5" }
            };

            // Act
            deviceGroupsWriter.ExportMapToReferenceData(deviceMapping, this.timestamp);

            // Assert
            this.VerifyFileWrapperMethods(deviceMapping);

            this.VerifyCloudStorageWrapperMethods();

            this.VerifyNoErrorsLogged();
        }

        /**
         * Verifies file name is generated as expected as no errors are thrown
         * when device groups writer is called with an empty dictionary of mappings
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ReferenceDataWrittenWithEmptyMapping()
        {
            // Arrange
            IBlobStorageHelper blobStorageHelper = new BlobStorageHelper(this.blobStorageConfig.Object, this.cloudStorageWrapper.Object, this.log.Object);

            IDeviceGroupsWriter deviceGroupsWriter = new DeviceGroupsWriter(this.blobStorageConfig.Object,
                blobStorageHelper,
                this.fileWrapper.Object,
                this.log.Object);
            Dictionary<string, IEnumerable<string>> deviceMapping = new Dictionary<string, IEnumerable<string>>();

            // Act
            deviceGroupsWriter.ExportMapToReferenceData(deviceMapping, this.timestamp);

            // Assert
            this.VerifyFileWrapperMethods(deviceMapping);

            this.VerifyCloudStorageWrapperMethods();

            this.VerifyNoErrorsLogged();
        }

        // Verify all cloud storage methods are called correctly
        private void VerifyCloudStorageWrapperMethods()
        {
            this.cloudStorageWrapper.Verify(c => c.Parse(this.expectedConnectionString));
            this.cloudStorageWrapper.Verify(c => c.CreateCloudBlobClient(It.IsAny<CloudStorageAccount>()));
            this.cloudStorageWrapper.Verify(c => c.GetContainerReference(It.IsAny<CloudBlobClient>(), REFERENCE_CONTAINER_NAME));
            this.cloudStorageWrapper.Verify(c => c.CreateIfNotExistsAsync(
                It.IsAny<CloudBlobContainer>(),
                BlobContainerPublicAccessType.Blob,
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>()));
            this.cloudStorageWrapper.Verify(c => c.GetBlockBlobReference(It.IsAny<CloudBlobContainer>(), this.expectedBlobName));
        }

        // Verify file is written correctly in csv format
        private void VerifyFileWrapperMethods(Dictionary<string, IEnumerable<string>> deviceMapping)
        {
            this.fileWrapper.Verify(f => f.WriteLine(It.IsAny<StreamWriter>(), "\"DeviceId\",\"GroupId\""));
            foreach (string key in deviceMapping.Keys)
            {
                foreach (string value in deviceMapping[key])
                {
                    this.fileWrapper.Verify(f => f.WriteLine(It.IsAny<StreamWriter>(), $"\"{value}\",\"{key}\""), Times.Once());
                }
            }
            this.fileWrapper.Verify(f => f.Delete(TEMP_FILE_NAME));
        }

        // Verify no errors are thrown by log
        private void VerifyNoErrorsLogged()
        {
            this.log.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Action>()), Times.Never());
        }
    }
}
