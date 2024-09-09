using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Afimilk.JobScheduler.BL
{
    public class JobScheduler
    {
        private readonly ConcurrentDictionary<int, JobExecutionInfo> _currentRunningJobs = new();
        private readonly ILogger<JobScheduler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IJobHandlerFactory _jobHandlerFactory;

        public JobScheduler(IServiceScopeFactory serviceScopeFactory,
                            IJobHandlerFactory jobHandlerFactory,
                            ILogger<JobScheduler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _jobHandlerFactory = jobHandlerFactory;
            _logger = logger;
        }

        public async Task ExecuteDueJobsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobs = await jobRepository.GetJobsToRunAsync();

            if (jobs.Count > 0)
            {

            }

            Parallel.ForEach(jobs, async job =>
            {
                await RunJobAsync(job);
                // Task.Run(() => RunJobAsync(job, scope)); // Run jobs concurrently
            });
        }

        private async Task RunJobAsync(Job job)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            _currentRunningJobs[job.Id] = new JobExecutionInfo
            {
                Job = job, // Reference to the Job object
                ThreadName = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString(),
                MainThread = Thread.CurrentThread.IsThreadPoolThread.ToString(),
            };

            _logger.LogInformation($"Running {job.Type}...");
            //todo:
            job.ExecutionStarted = DateTime.Now;

            await _jobHandlerFactory.GetHandler(job.Type)
                                    .ExecuteAsync(job);

            job.ExecutionCompleted = DateTime.Now;
            job.RemainingOccurrences--;

            await jobRepository.UpdateJobAsync(job);

            // Mark job as completed
            // jobExecutionInfo.ExecutionCompleted = DateTime.Now;
            _currentRunningJobs.TryRemove(job.Id, out _);
        }

        public IEnumerable<JobExecutionInfo> GetCurrentlyRunningJobs() => _currentRunningJobs.Values;
    }
}
