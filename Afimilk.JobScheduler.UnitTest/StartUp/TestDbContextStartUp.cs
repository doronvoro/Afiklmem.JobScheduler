using Afimilk.JobScheduler.BL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afimilk.JobScheduler.UnitTests
{
    public static class TestDbContextStartUp
    {
        public static void SetInMemoryDatabase(this IServiceCollection services)
        {
            // Remove existing DbContext configuration if present
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<JobsDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database configuration
            services.AddDbContext<JobsDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryJobsDb"));
        }
    }
}
