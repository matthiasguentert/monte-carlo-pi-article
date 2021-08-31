using Azure;
using ClusterHead.Model;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.Conventions.Files;
using Shared.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClusterHead
{
    public class ClusterController
    {
        private string outputContainerSasUrl;
        private readonly IClusterService clusterService;

        public ClusterController(IClusterService clusterService) => this.clusterService = clusterService;

        public async Task<ICloudJobWrapper> CreateJobAsync(string jobId, bool useStaticPool = true, string poolId = "staticpool-1")
        {
            ICloudJobWrapper job;
            if (useStaticPool)
            {
                job = this.clusterService.CreateJob(jobId, Tools.UseStaticPool(poolId));
            }
            else
            {
                job = this.clusterService.CreateJob(jobId, Tools.UseAutoPool());
            }

            job.SetOnAllTasksComplete(OnAllTasksComplete.TerminateJob);
            await job.CommitAsync();

            return job;
        }

        public async Task<string> GetOuputContainerSasUrl(ICloudJobWrapper job)
        {
            var storageAccount = this.clusterService.GetStorageAccount();
            await job.PrepareOutputStorageAsync(storageAccount);

            return job.GetOutputStorageContainerUrl(storageAccount);
        }

        public string GetInputContainerSasUrl()
        {
            var blobContainerClient = this.clusterService.GetBlobContainerClient("input-files");
            var containerSasUri = blobContainerClient.GenerateSasUri(
                Azure.Storage.Sas.BlobContainerSasPermissions.Read | Azure.Storage.Sas.BlobContainerSasPermissions.List,
                DateTime.UtcNow.AddHours(1));

            return containerSasUri.AbsoluteUri;
        }

        public async Task<IEnumerable<string>> CreateTasksAsync(string jobId, IEnumerable<Unit> units)
        {
            var job = this.clusterService.GetJob(jobId);
            var taskIds = new List<string>();
            this.outputContainerSasUrl = await GetOuputContainerSasUrl(job);
            var inputContainerSasUrl = GetInputContainerSasUrl();

            for (var i = 0; i < units.Count(); i++)
            {
                var id = $"task-{i}";

                var commandLine = $"cmd /c %AZ_BATCH_APP_PACKAGE_CONSUMER#1.0.0%\\Consumer.exe %AZ_BATCH_TASK_WORKING_DIR%\\input-{i}.json";
                var task = new CloudTask(id, commandLine)
                {
                    ApplicationPackageReferences = new List<ApplicationPackageReference>()
                    {
                        new ApplicationPackageReference { ApplicationId = "consumer", Version = "1.0.0" }
                    },
                    EnvironmentSettings = new List<EnvironmentSetting>()
                    {
                        new EnvironmentSetting("JOB_OUTPUT_CONTAINER_URI", this.outputContainerSasUrl)
                    },
                    ResourceFiles = new List<ResourceFile> { ResourceFile.FromStorageContainerUrl(inputContainerSasUrl, blobPrefix: $"input-{i}.json") },
                    Constraints = new TaskConstraints(retentionTime: TimeSpan.FromDays(1)),
                    //OutputFiles = await PrepareOutputFiles(cloudJob, path: id)
                };

                taskIds.Add(id);
                job.AddTask(task);
            }

            return taskIds;
        }

        public async Task UploadResourceFilesAsync(IEnumerable<Unit> units, string containerName)
        {
            var blobContainerClient = this.clusterService.GetBlobContainerClient(containerName);
            try
            {
                var result = await blobContainerClient.CreateIfNotExistsAsync();
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
            }

            var unitsArray = units.ToArray();

            for (var i = 0; i < units.Count(); i++)
            {
                var json = JsonSerializer.Serialize(unitsArray[i]);
                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

                try
                {
                    await blobContainerClient.DeleteBlobIfExistsAsync($"input-{i}.json");
                    await blobContainerClient.UploadBlobAsync($"input-{i}.json", memoryStream);
                }
                catch (RequestFailedException e)
                {
                    Console.WriteLine(e.ErrorCode); ;
                }
            }
        }

        public async Task CreateStaticPoolAsync(string poolId)
        {
            var imageReference = new ImageReference(
                    offer: "windowsserver",
                    publisher: "microsoftwindowsserver",
                    sku: Sku.DATACENTER_SMALLDISK_2012_R2);

            var applications = new List<ApplicationPackageReference>()
            {
                new ApplicationPackageReference { ApplicationId = "consumer", Version = "1.0.0" }
            };

            try
            {
                var vmConfiguration = new VirtualMachineConfiguration(imageReference, nodeAgentSkuId: "batch.node.windows amd64");

                var pool = this.clusterService.CreatePool(
                    poolId: poolId,
                    virtualMachineSize: "standard_a1",
                    virtualMachineConfiguration: vmConfiguration,
                    targetLowPriorityComputeNodes: 1);

                pool.SetApplicationPackageReferences(applications);
                pool.SetTaskSlotsPerNode(2);

                await pool.CommitAsync();
            }
            catch (BatchException e)
            {
                if (e.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine($"The pool {poolId} already exists!");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<Unit>> WaitForTasks(string jobId, IEnumerable<string> taskIds, TimeSpan timeout)
        {
            // We use the task state monitor to monitor the state of our tasks -- in this case we will wait for them all to complete.
            var taskStateMonitor = this.clusterService.CreateTaskStateMonitor();
            var job = this.clusterService.GetJob(jobId);
            var boundTasks = new List<ICloudTaskWrapper>();

            foreach (var taskId in taskIds)
            {
                var cloudTask = await job.GetTaskAsync(taskId);
                boundTasks.Add(cloudTask);
            }

            // Wait until the tasks are in completed state.
            await taskStateMonitor.WhenAll(boundTasks, TaskState.Completed, timeout);

            // dump task output
            foreach (var task in boundTasks)
            {
                Console.WriteLine($"## Task: {task.GetId()} ###########################################################");

                //Read the standard out of the task
                var standardOutFile = await task.GetNodeFileAsync(Constants.StandardOutFileName);
                var standardOutText = await standardOutFile.ReadAsStringAsync();
                Console.WriteLine("Standard out:");
                Console.WriteLine(standardOutText);

                //Read the standard error of the task
                var standardErrorFile = await task.GetNodeFileAsync(Constants.StandardErrorFileName);
                var standardErrorText = await standardErrorFile.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(standardErrorText))
                {
                    Console.WriteLine("Standard error:");
                    Console.WriteLine(standardErrorText);
                }

                Console.WriteLine();
            }

            // Retrieve output data from tasks 
            var units = new List<Unit>();
            foreach (var task in boundTasks)
            {
                var taskOutputStorage = new TaskOutputStorage(new Uri(this.outputContainerSasUrl), task.GetId());
                var output = await taskOutputStorage.GetOutputAsync(TaskOutputKind.TaskOutput, "output.txt");
                
                var stream = await output.OpenReadAsync();
                var reader = new StreamReader(stream);
                var unitAsJson = reader.ReadToEnd();

                var unit = JsonSerializer.Deserialize<Unit>(unitAsJson);
                units.Add(unit);
            }

            return units;
        }
    }
}
