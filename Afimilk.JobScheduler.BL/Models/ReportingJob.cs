namespace Afimilk.JobScheduler.BL
{
    public class ReportingJob : JobHandler
    {
        public override async Task ExecuteAsync(Job job)
        {
            Console.WriteLine("ReportingJob start...");

            await Task.Delay(TimeSpan.FromSeconds(30));// Simulate report generation

            Console.WriteLine("ReportingJob end...");
        }
    }
}