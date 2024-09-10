namespace Afimilk.JobScheduler.BL
{
    public class MaintenanceJob : JobHandler
    {
        public override async Task ExecuteAsync(Job job)
        {
            Console.WriteLine("MaintenanceJob start...");

            await Task.Delay(TimeSpan.FromSeconds(30));// Simulate report generation

            Console.WriteLine("MaintenanceJob end...");
        }
    }
}