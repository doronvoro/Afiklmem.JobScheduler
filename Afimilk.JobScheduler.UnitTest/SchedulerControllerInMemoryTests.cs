using Afimilk.JobScheduler.BL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Afimilk.JobScheduler.UnitTests
{
    public class SchedulerControllerInMemoryTests
    {
        private readonly ServiceProvider _serviceProvider;

        public SchedulerControllerInMemoryTests()
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

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

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

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 5, Type = "InvalidJobType" }; // Invalid job type

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Job type InvalidJobType does not exist.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddJob_ZeroOccurrences_ReturnsBadRequest()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();
            jobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "TestJob" });

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 0, Type = "TestJob" }; // Invalid Occurrences

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Occurrences must be greater than 0", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task AddJob_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();
            jobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "TestJob" });

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);
            controller.ModelState.AddModelError("Occurrences", "Occurrences is required");

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 5, Type = "TestJob" };

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Occurrences is required", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetAllJobs_ReturnsOkWithJobs()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            // Seed the in-memory database
            await jobRepository.AddJobAsync(new Job { Id = 1, Type = "TestJob1", Occurrences = 3 });
            await jobRepository.AddJobAsync(new Job { Id = 2, Type = "TestJob2", Occurrences = 2 });

            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            // Act
            var result = await controller.GetAllJobs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedJobs = Assert.IsAssignableFrom<IEnumerable<Job>>(okResult.Value);
            Assert.Equal(2, returnedJobs.Count());
        }

        [Fact]
        public async Task DeleteJob_JobExists_ReturnsOk()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            // Seed the in-memory database
            await jobRepository.AddJobAsync(new Job { Id = 1, Type = "TestJob1", Occurrences = 3 });

            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            // Act
            var result = await controller.DeleteJob(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Job deleted successfully", okResult.Value);
        }

        [Fact]
        public async Task DeleteJob_JobNotFound_ReturnsNotFound()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            // Act
            var result = await controller.DeleteJob(999); // Non-existing ID

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Job not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddJob_DuplicateJobType_ReturnsBadRequest()
        {
            // Arrange
            var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            // Seed a job with a duplicate type
            await jobRepository.AddJobAsync(new Job { Id = 1, Type = "DuplicateJob", Occurrences = 3 });

            var jobScheduler = scope.ServiceProvider.GetRequiredService<BL.JobScheduler>();
            var jobHandlerFactory = new Mock<IJobHandlerFactory>();
            jobHandlerFactory.Setup(f => f.GetJobTypeNames()).Returns(new List<string> { "DuplicateJob" });

            var controller = new SchedulerController(jobScheduler, jobRepository, jobHandlerFactory.Object);

            var jobRequest = new JobRequest { DailyExecutionTime = TimeSpan.Zero, Occurrences = 5, Type = "DuplicateJob" };

            // Act
            var result = await controller.AddJob(jobRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Job type DuplicateJob does not exist.", badRequestResult.Value);
        }
    }
}
