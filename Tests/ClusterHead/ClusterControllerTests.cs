using Azure.Storage.Blobs;
using ClusterHead;
using ClusterHead.Model;
using FluentAssertions;
using Microsoft.Azure.Batch;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.ClusterHead
{
    public class ClusterControllerTests
    {
        [Fact]
        public async Task ShouldCreateJobAsync()
        {
            // Arrange
            var jobMock = new Mock<ICloudJobWrapper>();
            var clusterServiceMock = new Mock<IClusterService>();
            clusterServiceMock
                .Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<PoolInformation>()))
                .Returns(jobMock.Object);

            var controller = new ClusterController(clusterServiceMock.Object);

            // Act
            var job = await controller.CreateJobAsync(jobId: "job1");

            // Assert
            clusterServiceMock.Verify(m => m.CreateJob("job1", It.IsAny<PoolInformation>()), Times.Once);
            jobMock.Verify(m => m.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ShouldCreateTasksAsync()
        {
            // Arrange
            var units = Tools.GenerateUnits(iterationsTotal: ulong.MaxValue, unitsTotal: 16);
            var jobMock = new Mock<ICloudJobWrapper>();

            var blobContainerClientMock = new Mock<BlobContainerClient>();
            blobContainerClientMock
                .Setup(m => m.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read | Azure.Storage.Sas.BlobContainerSasPermissions.List, It.IsAny<DateTimeOffset>()))
                .Returns(new Uri("https://fakesasurl.com"));
            blobContainerClientMock
                .Setup(m => m.CanGenerateSasUri)
                .Returns(true);

            var clusterServiceMock = new Mock<IClusterService>();
            clusterServiceMock
                .Setup(m => m.GetJob(It.IsAny<string>()))
                .Returns(jobMock.Object);
            clusterServiceMock
                .Setup(m => m.GetBlobContainerClient("input-files"))
                .Returns(blobContainerClientMock.Object);
            var controller = new ClusterController(clusterServiceMock.Object);

            // Act
            var tasks = await controller.CreateTasksAsync("job1", units);

            // Assert
            tasks.Should().HaveCount(16);
            clusterServiceMock.Verify(m => m.GetStorageAccount(), Times.Once);
            jobMock.Verify(m => m.PrepareOutputStorageAsync(It.IsAny<CloudStorageAccount>()), Times.Once);
            jobMock.Verify(m => m.GetOutputStorageContainerUrl(It.IsAny<CloudStorageAccount>()), Times.Once);
            jobMock.Verify(m => m.AddTask(It.IsAny<CloudTask>()), Times.Exactly(16));

        }

        [Fact]
        public async Task ShouldUploadResourceFilesAsync()
        {
            // Arrange
            var units = Tools.GenerateUnits(iterationsTotal: ulong.MaxValue, unitsTotal: 16);

            var blobContainerClientMock = new Mock<BlobContainerClient>();

            var clusterServiceMock = new Mock<IClusterService>();
            clusterServiceMock
                .Setup(m => m.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClientMock.Object);

            var controller = new ClusterController(clusterServiceMock.Object);

            // Act
            await controller.UploadResourceFilesAsync(units, "input-files");

            // Assert
            blobContainerClientMock.Verify(m => m.DeleteBlobIfExistsAsync(It.IsAny<string>(), Azure.Storage.Blobs.Models.DeleteSnapshotsOption.None, null, CancellationToken.None), Times.Exactly(16));
            blobContainerClientMock.Verify(m => m.UploadBlobAsync(It.IsAny<string>(), It.IsAny<MemoryStream>(), CancellationToken.None), Times.Exactly(16));
        }

        [Fact]
        public async Task ShouldCreateStaticPoolAsync()
        {
            // Arrange
            var poolMock = new Mock<ICloudPoolWrapper>();

            var clusterServiceMock = new Mock<IClusterService>();
            clusterServiceMock
                .Setup(m => m.CreatePool("staticpool1", "standard_a1", It.IsAny<VirtualMachineConfiguration>(), 1))
                .Returns(poolMock.Object);

            var controller = new ClusterController(clusterServiceMock.Object);

            // Act
            await controller.CreateStaticPoolAsync("staticpool1");

            // Assert
            clusterServiceMock.Verify(m => m.CreatePool("staticpool1", "standard_a1", It.IsAny<VirtualMachineConfiguration>(), 1), Times.Once);
            poolMock.Verify(m => m.CommitAsync(), Times.Once);
        }
    }
}
