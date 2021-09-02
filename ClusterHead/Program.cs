using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
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
            var stopwatch = new Stopwatch();

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
            stopwatch.Start();
            var taskIds = await controller.CreateTasksAsync(jobId, units);

            // Wait for all tasks to finish
            Console.WriteLine("Waiting for tasks to complete...");
            await controller.WaitForTasks(jobId, timeout: TimeSpan.FromMinutes(15));
            stopwatch.Stop();

            // Download files, deserialize and aggregate the results
            Console.WriteLine("Retrieving output from tasks...");
            var calculatedUnits = await controller.RetrieveOutputData(jobId);

            var estimatedPi = Tools.CalculatePi(calculatedUnits);

            Console.WriteLine($"Total calculation time: {Tools.GetTotalCalculationTime(calculatedUnits)}");
            Console.WriteLine($"Total run time:         {Tools.GetTotalRuntime(stopwatch.Elapsed)}");
            Console.WriteLine($"Estimated PI:           {estimatedPi}");
            Console.WriteLine($"Math.Pi:                {Math.PI}");
        }
    }
}
