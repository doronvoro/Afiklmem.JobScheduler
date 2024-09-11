using Afimilk.JobScheduler.BL;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Afimilk.JobScheduler.UnitTests
{
    public class JobSchedulerTests
    {
        private IServiceProvider _serviceProvider;
        private IServiceScope _scope;
        private BL.JobScheduler _jobScheduler;
        //private Mock<IJobRepository> _jobRepositoryMock;
        private Mock<IJobHandlerFactory> _jobHandlerFactoryMock;

        private const int _defaultJobExecuteTime = 5 * 1000; //convert sec to milisecound
        private const double _normalGapJobExecuteTime = 0.1; // precent

        public JobSchedulerTests()
        {
            // Setup the DI container
            var services = new ServiceCollection();

            // Add job scheduler services using the original method
            services.AddJobSchedulerBL();
            services.SetInMemoryDatabase();
            services.AddLogging();

            // Mock JobHandlerFactory
            _jobHandlerFactoryMock = new Mock<IJobHandlerFactory>();

            // Define a random job type
            var randomJobType = $"JobType_{Guid.NewGuid()}";

            // Define the delay time
            var delayTime = TimeSpan.FromMilliseconds(_defaultJobExecuteTime);

            // Mock the job handler to return a Task.Delay with the specified delay time
            var mockJobHandler = new Mock<JobHandler>();
            mockJobHandler.Setup(jh => jh.ExecuteAsync(It.IsAny<Job>()))
                .Returns(async () => await Task.Delay(delayTime));

            // Setup GetHandler to return the mock job handler
            _jobHandlerFactoryMock.Setup(f => f.GetHandler(randomJobType))
                .Returns(mockJobHandler.Object);

            // Setup GetJobTypeNames to return the registered job type
            _jobHandlerFactoryMock.Setup(f => f.GetJobTypeNames())
                .Returns([randomJobType]);

            // Remove existing IJobHandlerFactory and add the mock factory
            ReplaceJobHandlerFactory(services, _jobHandlerFactoryMock.Object);

            // Build the service provider again with the modified services
            _serviceProvider = services.BuildServiceProvider();
            _serviceProvider.InitializeDatabase();

            _scope = _serviceProvider.CreateScope();
            _jobScheduler = _scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            //  _jobRepositoryMock = new Mock<IJobRepository>();
        }

        private void ReplaceJobHandlerFactory(ServiceCollection services, IJobHandlerFactory jobHandlerFactory)
        {
            //
            // Remove the original registration of IJobHandlerFactory
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IJobHandlerFactory));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add the mock job handler factory
            services.AddSingleton(jobHandlerFactory);
        }

        [Fact]
        public async Task JobExecutionTimes_ShouldStartAndFinishWithinTimeLimit()
        {
            // Arrange
            var randomJobType = _jobHandlerFactoryMock.Object.GetJobTypeNames().First();

            var jobs = Enumerable.Range(0, 3).Select(s => new Job
            {
                Type = randomJobType,
                DailyExecutionTime = TimeSpan.Zero,
                Occurrences = 1,
                RemainingOccurrences = 1
            }
            );

            var jobRepository = _scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var jobTasks = jobs.Select(async job => await jobRepository.AddJobAsync(job));

            await Task.WhenAll(jobTasks);
             
            // Act
            var startTime = DateTime.UtcNow;

            var executeJobsTask = _jobScheduler.ExecuteDueJobsAsync();

            var jobExecution = _jobScheduler.GetCurrentlyRunningJobs().ToList();

            Assert.True(jobExecution.Count > 0, "No jobs are currently running.");

            await executeJobsTask;
            var endTime = DateTime.UtcNow;

            // Assert
            var jobExecution1 = _jobScheduler.GetCurrentlyRunningJobs().ToList();
            Assert.True(jobExecution1.Count == 0, "jobs are currently running.");

            var startTimes = jobExecution.Select(job => job.ExecutionStarted).Where(ts => ts != default);
            var endTimes = jobExecution.Select(job => job.ExecutionCompleted).Where(ts => ts != default);

            var timeLimitValue = _defaultJobExecuteTime * (1 + _normalGapJobExecuteTime);
            var timeLimit = TimeSpan.FromMilliseconds(timeLimitValue);

            Assert.All(startTimes, t => Assert.True((startTime - t).TotalMilliseconds < timeLimit.TotalMilliseconds, "Job start times exceed the time limit."));
            Assert.All(endTimes, t => Assert.True((endTime - t).TotalMilliseconds < timeLimit.TotalMilliseconds, "Job end times exceed the time limit."));

            var maxGap = jobExecution.Max(job => (job.ExecutionCompleted - job.ExecutionStarted).TotalMilliseconds);
            Assert.True(maxGap <= timeLimit.TotalMilliseconds, $"Gap between job start and finish times exceeds the time limit: {maxGap} seconds.");
        }
    }
}
