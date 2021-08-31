using Azure.Storage.Blobs;
using ClusterHead.Model;
using Microsoft.Azure.Batch;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;

namespace ClusterHead
{
    public interface IClusterService
    {
        ICloudPoolWrapper CreatePool(string poolId, string virtualMachineSize, VirtualMachineConfiguration virtualMachineConfiguration, int targetLowPriorityComputeNodes);

        ICloudJobWrapper CreateJob(string jobId, PoolInformation poolInformation);

        ICloudJobWrapper GetJob(string jobId);

        CloudStorageAccount GetStorageAccount();

        BlobContainerClient GetBlobContainerClient(string containerName);

        void AddTask(string jobId, IEnumerable<CloudTask> tasksToAdd);

        ITaskStateMonitorWrapper CreateTaskStateMonitor();
    }
}
