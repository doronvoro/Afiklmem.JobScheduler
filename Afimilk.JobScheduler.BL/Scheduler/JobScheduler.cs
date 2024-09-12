using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Afimilk.JobScheduler.BL
{
    public class JobScheduler
    {
        private readonly ConcurrentDictionary<int, Job> _currentRunningJobs = new();
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
            var jobs = await jobRepository.GetJobsToRunAsync(_currentRunningJobs.Keys);

            _logger.LogDebug("Fetched {JobCount} jobs to run. Current running jobs: {@RunningJobCount}.", jobs.Count, _currentRunningJobs.Count);

            // Run jobs concurrently
            var tasks = jobs.Where(w => !_currentRunningJobs.Keys.Contains(w.Id))
                            .Select(async job =>
            {
                try
                {
                    await RunJobAsync(job);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute job with ID {JobId}.", job.Id);
                }
            }).ToArray();

            _logger.LogDebug("executeed tasks count {TasksCount}", tasks.Length);

            await Task.WhenAll(tasks);
        }

        public async Task HandleIncompleteJobsOnStartup()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var incompleteJobs = await jobRepository.GetIncompleteJobsAsync(); // Jobs where ExecutionCompleted is null

            _logger.LogInformation("Number of incomplete jobs: {Count}", incompleteJobs.Count);

            foreach (var job in incompleteJobs)
            {
                _logger.LogInformation("Running incomplete job: {JobId} - {JobName}", job.Id, job.Type);
                await RunJobAsync(job);
            }
        }

        private async Task RunJobAsync(Job job)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            if (!_currentRunningJobs.TryAdd(job.Id, job))
            {
                _logger.LogWarning("Job ID {JobId} is already running.", job.Id);
                return;
            }

            job.ExecutionStarted = DateTime.Now;
            await jobRepository.UpdateJobAsync(job);

            _logger.LogInformation("Starting job execution for Job ID {JobId} ({JobType}).", job.Id, job.Type);

            try
            {
                var jobHandler = _jobHandlerFactory.GetHandler(job.Type);
                await jobHandler.ExecuteAsync(job);

                job.ExecutionCompleted = DateTime.Now;
                job.RemainingOccurrences--;
                await jobRepository.UpdateJobAsync(job);

                _logger.LogInformation("Completed job execution for Job ID {JobId}. Execution completed at {EndTime}. Remaining occurrences: {RemainingOccurrences}.",
                                        job.Id,
                                        job.ExecutionCompleted,
                                        job.RemainingOccurrences);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing job with ID {JobId}.", job.Id);
            }
            finally
            {
                if (_currentRunningJobs.ContainsKey(job.Id))
                {
                    _currentRunningJobs.TryRemove(job.Id, out _);
                }
                _logger.LogDebug("Job ID {JobId} removed from running jobs. Current running jobs count: {RunningJobCount}.", job.Id, _currentRunningJobs.Count);
            }
        }

        public IEnumerable<Job> GetCurrentlyRunningJobs() => _currentRunningJobs.Values;
    }
}
