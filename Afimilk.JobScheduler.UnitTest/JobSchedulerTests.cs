using Afimilk.JobScheduler.BL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public class JobSchedulerTests
{
    private readonly Mock<IJobRepository> _jobRepositoryMock;
    private readonly Mock<IJobHandlerFactory> _jobHandlerFactoryMock;
    private readonly Mock<ILogger<JobScheduler>> _loggerMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly JobScheduler _jobScheduler;

    public JobSchedulerTests()
    {
        _jobRepositoryMock = new Mock<IJobRepository>();
        _jobHandlerFactoryMock = new Mock<IJobHandlerFactory>();
        _loggerMock = new Mock<ILogger<JobScheduler>>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IJobRepository)))
            .Returns(_jobRepositoryMock.Object);

        _jobScheduler = new JobScheduler(_serviceScopeFactoryMock.Object, _jobHandlerFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleIncompleteJobsOnStartup_LogsCorrectNumberOfJobs()
    {
        // Arrange
        var incompleteJobs = new List<Job>
        {
            new Job { Id = 1 },
            new Job { Id = 2 }
        };

        _jobRepositoryMock.Setup(repo => repo.GetIncompleteJobsAsync())
            .ReturnsAsync(incompleteJobs);

        // Act
        await _jobScheduler.HandleIncompleteJobsOnStartup();

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Number of incomplete jobs: 2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }


    [Fact]
    public async Task ExecuteDueJobsAsync_RunsMultipleJobsConcurrently()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, Type = "TestJob1" },
            new Job { Id = 2, Type = "TestJob2" }
        };

        var jobHandlerMock1 = new Mock<IJobHandler>();
        var jobHandlerMock2 = new Mock<IJobHandler>();

        _jobRepositoryMock.Setup(repo => repo.GetJobsToRunAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(jobs);

        _jobHandlerFactoryMock.Setup(factory => factory.GetHandler("TestJob1"))
            .Returns(jobHandlerMock1.Object);
        _jobHandlerFactoryMock.Setup(factory => factory.GetHandler("TestJob2"))
            .Returns(jobHandlerMock2.Object);

        jobHandlerMock1.Setup(handler => handler.ExecuteAsync(jobs[0])).Returns(Task.CompletedTask);
        jobHandlerMock2.Setup(handler => handler.ExecuteAsync(jobs[1])).Returns(Task.CompletedTask);

        // Act
        await _jobScheduler.ExecuteDueJobsAsync();

        // Assert
        _jobRepositoryMock.Verify(repo => repo.GetJobsToRunAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
        _jobHandlerFactoryMock.Verify(factory => factory.GetHandler("TestJob1"), Times.Once);
        _jobHandlerFactoryMock.Verify(factory => factory.GetHandler("TestJob2"), Times.Once);

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetched 2 jobs to run")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
