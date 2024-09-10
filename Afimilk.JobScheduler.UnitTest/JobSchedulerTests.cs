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
        private Mock<IJobRepository> _jobRepositoryMock;
        private Mock<IJobHandlerFactory> _jobHandlerFactoryMock;

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
            var delayTime = TimeSpan.FromSeconds(2);

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
            _jobRepositoryMock = new Mock<IJobRepository>();
        }

        private void ReplaceJobHandlerFactory(ServiceCollection services, IJobHandlerFactory jobHandlerFactory)
        {
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
            var jobs = new List<Job>
            {
                new Job { Id = 1, Type = randomJobType, DailyExecutionTime = TimeSpan.Zero, Occurrences = 1, RemainingOccurrences = 1 },
                new Job { Id = 2, Type = randomJobType, DailyExecutionTime = TimeSpan.Zero, Occurrences = 1, RemainingOccurrences = 1 },
                new Job { Id = 3, Type = randomJobType, DailyExecutionTime = TimeSpan.Zero, Occurrences = 1, RemainingOccurrences = 1 }
            };

            _jobRepositoryMock.Setup(repo => repo.GetJobsToRunAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(jobs);

            var jobRepository = _scope.ServiceProvider.GetRequiredService<IJobRepository>();
            foreach (var job in jobs)
            {
                await jobRepository.AddJobAsync(job);
            }

            var timeLimit = TimeSpan.FromSeconds(2);

            // Act
            var startTime = DateTime.UtcNow;
            await _jobScheduler.ExecuteDueJobsAsync();
            var endTime = DateTime.UtcNow;

            // Assert
            var jobExecutionInfos = _jobScheduler.GetCurrentlyRunningJobs().ToList();
            Assert.True(jobExecutionInfos.Count > 0, "No jobs are currently running.");

            var startTimes = jobExecutionInfos.Select(info => info.Job.ExecutionStarted).Where(ts => ts != default);
            var endTimes = jobExecutionInfos.Select(info => info.Job.ExecutionCompleted).Where(ts => ts != default);

            Assert.All(startTimes, startTime => Assert.True((startTime - startTime).TotalSeconds < timeLimit.TotalSeconds, "Job start times exceed the time limit."));
            Assert.All(endTimes, endTime => Assert.True((endTime - endTime).TotalSeconds < timeLimit.TotalSeconds, "Job end times exceed the time limit."));

            var maxGap = jobExecutionInfos.Max(info => (info.Job.ExecutionCompleted - info.Job.ExecutionStarted).TotalSeconds);
            Assert.True(maxGap <= timeLimit.TotalSeconds, $"Gap between job start and finish times exceeds the time limit: {maxGap} seconds.");
        }
    }
}
