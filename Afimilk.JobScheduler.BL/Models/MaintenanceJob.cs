namespace Afimilk.JobScheduler.BL
{
    public class MaintenanceJob : JobHandler
    {
        public override async Task ExecuteAsync(Job job)
        {

           await Task.Delay(TimeSpan.FromSeconds(5));// Simulate report generation

            // Simulate backup
            Console.WriteLine("Running daily backup...");
            //  return Task.CompletedTask;

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }


}