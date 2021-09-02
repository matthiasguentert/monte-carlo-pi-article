using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ClusterHead
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            var appConfig = config.GetSection(nameof(AppConfig)).Get<AppConfig>();

            Console.WriteLine("Running with configuration");
            Console.WriteLine();
            Console.WriteLine(appConfig);

            // Input data
            Console.WriteLine("Generating work units...");
            var units = Tools.GenerateUnits(appConfig.IterationsTotal, appConfig.UnitsTotal);
            
            // Create cluster controller
            var controller = new ClusterController(new ClusterService(appConfig));

            // Create a static pool 
            Console.WriteLine("Trying to create static pool...");
            await controller.CreateStaticPoolIfNotExistsAsync("staticpool-2", targetLowPriorityComputeNodes: 4, taskSlotsPerNode: 1);

            // Serialize & upload input data
            Console.WriteLine("Uploading resource files...");
            await controller.UploadResourceFilesAsync(units, containerName: "input-files");

            // Create the job 
            var jobId = Guid.NewGuid().ToString();

            Console.WriteLine("Creating the job...");
            await controller.CreateJobAsync(jobId, useStaticPool: true, poolId: "staticpool-2");

            // Create the tasks 
            Console.WriteLine("Creating the tasks...");
            var taskIds = await controller.CreateTasksAsync(jobId, units);

            // Wait for all tasks to finish
            Console.WriteLine("Waiting for tasks to complete...");
            await controller.WaitForTasks(jobId, timeout: TimeSpan.FromMinutes(15));

            // Print stdout & stderr
            Console.WriteLine("Retrieving stdout & stderr from tasks...");
            await controller.PrintTaskOutputAsync(jobId);

            // Download output files, deserialize and aggregate the results
            var calculatedUnits = await controller.RetrieveOutputData(jobId);
            var estimatedPi = Tools.CalculatePi(calculatedUnits);

            Console.WriteLine($"Estimated PI: {estimatedPi}");
            Console.WriteLine($"Math.Pi: {Math.PI}");
        }
    }
}
