using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Afiklmem.JobScheduler.BL
{
    public static class JobSchedulerBLStartup
    {
        public static void AddJobSchedulerBL(this IServiceCollection services, IConfiguration configuration = default)
        {
            services.AddDbContext<JobsDbContext>(options => options.UseSqlite("Data Source=jobs.db"));

            services.AddScoped<IJobRepository, JobRepository>();

            // Register JobScheduler as Singleton, with IServiceScopeFactory and ILogger<JobScheduler>
            services.AddSingleton<JobScheduler>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<JobScheduler>>();
                var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                return new JobScheduler(scopeFactory, logger);
            });

            services.AddHostedService<JobBackgroundService>();
        }
    }
}
