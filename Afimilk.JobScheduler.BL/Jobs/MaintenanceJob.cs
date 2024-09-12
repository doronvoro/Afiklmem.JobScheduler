namespace Afimilk.JobScheduler.BL
{
    public class MaintenanceJob : IJobHandler
    {
        public async Task ExecuteAsync(Job job)
        {
            Console.WriteLine("MaintenanceJob start...");

            await Task.Delay(TimeSpan.FromSeconds(10));// Simulate Maintenance JOb

            Console.WriteLine("MaintenanceJob end...");
        }
    }
}