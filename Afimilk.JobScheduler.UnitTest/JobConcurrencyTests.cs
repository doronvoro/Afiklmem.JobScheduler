using Afimilk.JobScheduler.BL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Afimilk.JobScheduler.UnitTests
{
    public class JobConcurrencyTests
    {
        private readonly ServiceProvider _serviceProvider;

        public JobConcurrencyTests()
        {

            // Create a service collection for DI
            var services = new ServiceCollection();

            // Add in-memory EF Core and register all services using AddJobSchedulerBL
            services.AddJobSchedulerBL();
            services.SetInMemoryDatabase();
            // Register mock logging services
            services.AddLogging();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Initialize the in-memory database
            _serviceProvider.InitializeDatabase();
        }

        [Fact]
        public async Task Jobs_ShouldStartAndFinish_AtTheSameTime_OrWithTolerableGap()
        {
            // Arrange
            var jobScheduler = _serviceProvider.GetRequiredService<BL.JobScheduler>();
            var jobRepository = _serviceProvider.GetRequiredService<IJobRepository>();

            // Add multiple jobs of the same type
            const int jobCount = 5;
            const string jobType = "TestJob";
            const double maxTimeGap = 500; // Allowable gap in milliseconds

            for (int i = 1; i <= jobCount; i++)
            {
                var job = new Job
                {
                    Id = i,
                    DailyExecutionTime = TimeSpan.FromHours(2),
                    Occurrences = 1,
                    RemainingOccurrences = 1,
                    Type = jobType,
                    //ExecutionStarted = null
                };
                await jobRepository.AddJobAsync(job);
            }

            // Act
            var backgroundService = _serviceProvider.GetRequiredService<IHostedService>() as JobBackgroundService;
            Assert.NotNull(backgroundService);

            // Trigger the background service's work (which should execute all the jobs)
            await backgroundService.StartAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(11)); // Wait for the jobs to execute

            // Get the updated jobs from the repository
            var updatedJobs = await jobRepository.GetAllJobsAsync();

            // Get the start and finish times for all jobs
            var startTimes = updatedJobs.Select(j => j.ExecutionStarted).Where(t => t != null).ToList();
            var endTimes = updatedJobs.Select(j => j.ExecutionCompleted).Where(t => t != null).ToList();

            // Assert all jobs have started and finished
            Assert.Equal(jobCount, startTimes.Count);
            Assert.Equal(jobCount, endTimes.Count);

            // Calculate the max time gap for start and finish times
            var maxStartGap = (startTimes.Max() - startTimes.Min()).TotalMilliseconds;
            var maxEndGap = (endTimes.Max() - endTimes.Min()).TotalMilliseconds;

            // Assert that both start and finish gaps are within the acceptable limit
            Assert.True(maxStartGap <= maxTimeGap, $"Max start time gap is {maxStartGap}, but allowed is {maxTimeGap}");
            Assert.True(maxEndGap <= maxTimeGap, $"Max end time gap is {maxEndGap}, but allowed is {maxTimeGap}");
        }
    }
}
