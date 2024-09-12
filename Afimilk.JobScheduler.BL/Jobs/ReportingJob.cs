namespace Afimilk.JobScheduler.BL
{
    public class ReportingJob : IJobHandler
    {
        public async Task ExecuteAsync(Job job)
        {
            Console.WriteLine("ReportingJob start...");

            await Task.Delay(TimeSpan.FromSeconds(5));// Simulate report generation

            Console.WriteLine("ReportingJob end...");
        }
    }
}