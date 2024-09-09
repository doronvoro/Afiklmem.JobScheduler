using Afimilk.JobScheduler.BL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Afimilk.JobScheduler.BL
{
    public static class JobSchedulerBLStartup
    {
        public static void AddJobSchedulerBL(this IServiceCollection services, IConfiguration configuration = default)
        {
            // Register the JobsDbContext to use SQLite
            services.AddDbContext<JobsDbContext>(options =>
                options.UseSqlite("Data Source=jobs.db"));

            services.AddSingleton<IJobHandlerFactory, JobHandlerFactory>();

            // Register the JobRepository with Scoped lifetime
            services.AddScoped<IJobRepository, JobRepository>();

            // Register JobScheduler as Singleton, passing IServiceScopeFactory and ILogger
            services.AddSingleton<JobScheduler>(provider =>
            {
                var serviceScopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                var logger = provider.GetRequiredService<ILogger<JobScheduler>>();
                var jobHandlerFactory = provider.GetRequiredService<IJobHandlerFactory>();


                return new JobScheduler(serviceScopeFactory, jobHandlerFactory, logger); // Pass the IServiceScopeFactory and ILogger to JobScheduler
            });

            // Register JobBackgroundService as a Hosted Service
            services.AddHostedService<JobBackgroundService>();
        }

        public static void InitializeDatabase(this IServiceProvider serviceProvider)
        {
            // Create a scope to get the DbContext service from the provider
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<JobsDbContext>();

            // Ensure the database is created
            dbContext.Database.EnsureCreated(); // Creates the database and tables if they do not exist
        }
    }
}