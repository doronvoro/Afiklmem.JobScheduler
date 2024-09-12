using Afimilk.JobScheduler.BL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Afimilk.JobScheduler.UnitTests
{
    public class SchedulerController
    {
        private readonly ServiceProvider _serviceProvider;

        public SchedulerController()
        {
            // Setup the DI container
            var services = new ServiceCollection();

            // Add job scheduler services using the original method
            services.AddJobSchedulerBL();
            services.SetInMemoryDatabase();
            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task AddJob_ValidRequest_ReturnsOk()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();
            jobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "TestJob" });

            var controller = new global::SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 5, Type = "TestJob" };

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Job added successfully", okResult.Value);
        }

        [Fact]
        public async Task AddJob_InvalidJobType_ReturnsBadRequest()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();
            jobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "TestJob" }); // Valid job types

            var controller = new global::SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 5, Type = "InvalidJobType" }; // Invalid job type

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Job type InvalidJobType does not exist.", badRequestResult.Value);
        }


        [Fact]
        public async Task DeleteJob_JobNotFound_ReturnsNotFound()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();

            var controller = new global::SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            // Act
            var result = await controller.DeleteJob(999); // Non-existing ID

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Job not found", notFoundResult.Value);
        }
    }
}
