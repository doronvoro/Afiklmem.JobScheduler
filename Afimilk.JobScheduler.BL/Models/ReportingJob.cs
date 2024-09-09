namespace Afimilk.JobScheduler.BL
{
    public class ReportingJob : JobHandler
    {
        public override async Task ExecuteAsync(Job job)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));// Simulate report generation
            Console.WriteLine("Generating daily report...");
        }
    }
}