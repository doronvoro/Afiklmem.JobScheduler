using Afimilk.JobScheduler.BL;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Xunit;
namespace Afimilk.JobScheduler.Tests
{
    public class SchedulerControllerTests
    {
        private readonly SchedulerController _controller;
        private readonly Mock<BL.JobScheduler> _mockJobScheduler;
        private readonly Mock<IJobRepository> _mockJobRepository;
        private readonly Mock<IJobHandlerFactory> _mockJobHandlerFactory;

        public SchedulerControllerTests()
        {
            _mockJobScheduler = new Mock<BL.JobScheduler>(null, null, null);
            _mockJobRepository = new Mock<IJobRepository>();
            _mockJobHandlerFactory = new Mock<IJobHandlerFactory>();

            _controller = new SchedulerController(
                _mockJobScheduler.Object,
                _mockJobRepository.Object,
                _mockJobHandlerFactory.Object);
        }

        [Fact]
        public async Task AddJob_ReturnsOk_WhenJobIsValid()
        {
            // Arrange
            var jobRequest = new JobRequest
            {
                DailyExecutionTime = TimeSpan.FromHours(12),
                Occurrences = 1,
                Type = "ReportingJob"
            };

            _mockJobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "ReportingJob" });
            _mockJobRepository.Setup(r => r.AddJobAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddJob(jobRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Job added successfully", okResult.Value);
        }

        [Fact]
        public async Task AddJob_ReturnsBadRequest_WhenJobTypeIsInvalid()
        {
            // Arrange
            var jobRequest = new JobRequest
            {
                DailyExecutionTime = TimeSpan.FromHours(12),
                Occurrences = 1,
                Type = "InvalidJobType"
            };

            _mockJobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "ReportingJob" });

            // Act
            var result = await _controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Job type InvalidJobType does not exist.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetJobById_ReturnsOk_WhenJobExists()
        {
            // Arrange
            var jobId = 1;
            var job = new Job { Id = jobId };

            _mockJobRepository.Setup(r => r.GetJobByIdAsync(jobId)).ReturnsAsync(job);

            // Act
            var result = await _controller.GetJobById(jobId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(job, okResult.Value);
        }

        [Fact]
        public async Task GetJobById_ReturnsNotFound_WhenJobDoesNotExist()
        {
            // Arrange
            var jobId = 1;
            _mockJobRepository.Setup(r => r.GetJobByIdAsync(jobId)).ReturnsAsync((Job)null);

            // Act
            var result = await _controller.GetJobById(jobId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Job not found", notFoundResult.Value);
        }
    }
}