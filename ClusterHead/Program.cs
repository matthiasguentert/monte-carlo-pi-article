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
            //controller.CreateStaticPool("staticpool-1");

            // Serialize & upload input data
            Console.WriteLine("Uploading resource files...");
            await controller.UploadResourceFilesAsync(units, containerName: "input-files");

            // Create the job 
            var jobId = Guid.NewGuid().ToString();

            Console.WriteLine("Creating the job...");
            await controller.CreateJobAsync(jobId);

            // Create the tasks 
            Console.WriteLine("Creating the tasks...");
            var taskIds = await controller.CreateTasksAsync(jobId, units);

            // Wait for all tasks to finish, print stdout, stderr, download output file and deserialize results
            Console.WriteLine("Waiting for tasks to complete...");
            var calculatedUnits = await controller.WaitForTasks(jobId, taskIds, timeout: TimeSpan.FromMinutes(15));

            // Aggregating the results
            var estimatedPi = Tools.EvaluatePi(calculatedUnits);
            Console.WriteLine($"Estimated PI: {estimatedPi}");
        }
    }
}
