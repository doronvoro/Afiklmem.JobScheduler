using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Afiklmem.JobScheduler.BL
{
    public class JobScheduler
    {
        private readonly ConcurrentDictionary<int, JobExecutionInfo> _currentRunningJobs = new();
        private readonly ILogger<JobScheduler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JobScheduler(IServiceScopeFactory serviceScopeFactory, ILogger<JobScheduler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task ExecuteDueJobsAsync()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                var jobs = await jobRepository.GetJobsToRunAsync(DateTime.Now.TimeOfDay);

                Parallel.ForEach(jobs, job =>
                {
                    Task.Run(() => RunJobAsync(job, scope)); // Run jobs concurrently
                });
            }
        }

        private async Task RunJobAsync(Job job, IServiceScope scope)
        {
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var jobExecutionInfo = new JobExecutionInfo
            {
                Job = job, // Reference to the Job object
                ThreadName = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString(),
                MainThread = Thread.CurrentThread.IsThreadPoolThread.ToString(),
                ExecutionStarted = DateTime.Now
            };

            _currentRunningJobs[job.Id] = jobExecutionInfo;

            _logger.LogInformation($"Running {job.TaskName}...");

            await Task.Delay(1000); // Simulating task running

            // Update job state after execution
            job.LastRun = DateTime.Now;
            job.RemainingOccurrences--;

            await jobRepository.UpdateJobAsync(job);

            // Mark job as completed
            jobExecutionInfo.ExecutionCompleted = DateTime.Now;
            _currentRunningJobs.TryRemove(job.Id, out _);
        }

        public IEnumerable<JobExecutionInfo> GetCurrentlyRunningJobs() => _currentRunningJobs.Values;
    }
}
