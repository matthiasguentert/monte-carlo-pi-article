using ClusterHead;
using ClusterHead.Model;
using FluentAssertions;
using Microsoft.Azure.Batch;
using Shared.Model;
using Xunit;

namespace Tests.ClusterHead
{
    public class ClusterServiceTests
    {
        public class ClusterServiceTest : ClusterService
        {
            public ClusterServiceTest(BatchClient batchClient) : base(batchClient) { }
        }

        [Fact]
        public void ShouldCreatePool()
        {
            // Arrange
            var batchClient = TestUtils.CreateDummyClient();

            var imageReference = new ImageReference(
                offer: "windowsserver",
                publisher: "microsoftwindowsserver",
                Sku.DATACENTER_SMALLDISK_2012_R2);
            var vmConfiguration = new VirtualMachineConfiguration(imageReference, nodeAgentSkuId: "batch.node.windows amd64");
            var clusterService = new ClusterServiceTest(batchClient);

            // Act
            var pool = clusterService.CreatePool(poolId: "pool-1", virtualMachineSize: "standard_a1", vmConfiguration, targetLowPriorityComputeNodes: 1);

            // Assert
            pool.Should().NotBeNull();
        }

        [Fact]
        public void ShouldCreateJob()
        {
            // Arrange
            var batchClient = TestUtils.CreateDummyClient();
            var clusterService = new ClusterServiceTest(batchClient);

            // Act
            var cloudJob = clusterService.CreateJob(jobId: "job-1", Tools.UseStaticPool("pool-1"));

            // Assert
            cloudJob.Should().NotBeNull();
        }
    }
}
