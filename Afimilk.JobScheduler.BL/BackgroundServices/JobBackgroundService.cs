using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Afimilk.JobScheduler.BL
{
    public class JobBackgroundService : BackgroundService
    {
        private readonly JobScheduler _jobScheduler;
        private readonly ILogger<JobBackgroundService> _logger;
        private Timer? _timer;

        public JobBackgroundService(JobScheduler jobScheduler, ILogger<JobBackgroundService> logger)
        {
            _jobScheduler = jobScheduler;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork!, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // todo: get from configuration 
            _logger.LogDebug("Timer Createad XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogDebug("Timer execute ================================================================================");
            await _jobScheduler.ExecuteDueJobsAsync();
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(stoppingToken);
        }
    }

}