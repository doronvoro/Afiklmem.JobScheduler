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
            });

            _logger.LogDebug("tasks  count {TasksCount}", tasks.Count());

           await Task.WhenAll(tasks);
        }

        private async Task RunJobAsync(Job job)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var jobExecutionInfo = new JobExecutionInfo
            {
                Job = job,
                ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),// Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString(),
                MainThread = Thread.CurrentThread.IsThreadPoolThread.ToString(),
            };

            _currentRunningJobs[job.Id] = jobExecutionInfo;
            job.ExecutionStarted = DateTime.Now;    

            _logger.LogInformation("Starting job execution for Job ID {JobId}. Execution started at {StartTime}. {@jobExecutionInfo}", job.Id, job.ExecutionStarted, jobExecutionInfo);

            try
            {
                var jobHandler = _jobHandlerFactory.GetHandler(job.Type);
                await jobHandler.ExecuteAsync(job);

                job.ExecutionCompleted = DateTime.Now;
                job.RemainingOccurrences--;
                await jobRepository.UpdateJobAsync(job);

                _logger.LogInformation("Completed job execution for Job ID {JobId}. Execution completed at {EndTime}. Remaining occurrences: {RemainingOccurrences}.  {jobExecutionInfo}",
                                        job.Id, 
                                        job.ExecutionCompleted, 
                                        job.RemainingOccurrences,
                                         jobExecutionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing job with ID {JobId}.", job.Id);
            }
            finally
            {
                _currentRunningJobs.TryRemove(job.Id, out _);
                _logger.LogDebug("Job ID {JobId} removed from running jobs. Current running jobs count: {RunningJobCount}.", job.Id, _currentRunningJobs.Count);
            }
        }

        public IEnumerable<JobExecutionInfo> GetCurrentlyRunningJobs() => _currentRunningJobs.Values;
    }
}
