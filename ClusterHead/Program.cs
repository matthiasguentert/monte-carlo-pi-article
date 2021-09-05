using ClusterHead.Model;
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

            Console.WriteLine("Running with configuration");
            Console.WriteLine();
            Console.WriteLine(appConfig);

            Console.WriteLine("Generating work units...");
            var units = Tools.GenerateUnits(appConfig.IterationsTotal, appConfig.UnitsTotal);

            var jobId = Tools.GenerateJobId("estimate-pi", appConfig.IterationsTotal, appConfig.UnitsTotal);
            var poolId = "staticpool-1";
            var controller = new ClusterController(new ClusterService(appConfig));
            var stopwatch = new Stopwatch();

            Console.WriteLine("Trying to create static pool...");
            await controller.CreateStaticPoolIfNotExistsAsync(poolId, VirtualMachineSize.STANDARD_A1_V2, targetLowPriorityComputeNodes: 6, taskSlotsPerNode: 1);

            Console.WriteLine("Uploading resource files...");
            await controller.UploadResourceFilesAsync(units, containerName: "input-files");

            Console.WriteLine("Creating the job...");
            await controller.CreateJobAsync(jobId, useStaticPool: true, poolId);

            Console.WriteLine("Creating the tasks...");
            stopwatch.Start();
            var taskIds = await controller.CreateTasksAsync(jobId, units);

            Console.WriteLine("Waiting for tasks to complete...");
            await controller.WaitForTasks(jobId, timeout: TimeSpan.FromDays(1));
            stopwatch.Stop();

            Console.WriteLine("Retrieving output from tasks...");
            var calculatedUnits = await controller.RetrieveOutputData(jobId);
        
            var estimatedPi = Tools.CalculatePi(calculatedUnits);

            Console.WriteLine($"Total calculation time: {Tools.GetTotalCalculationTime(calculatedUnits)}");
            Console.WriteLine($"Total run time:         {Tools.GetTotalRuntime(stopwatch.Elapsed)}");
            Console.WriteLine();
            Console.WriteLine($"Estimated PI:           {estimatedPi}");
            Console.WriteLine($"Math.Pi:                {Math.PI}");
        }
    }
}
