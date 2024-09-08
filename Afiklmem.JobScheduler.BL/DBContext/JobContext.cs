using Microsoft.EntityFrameworkCore;

namespace Afiklmem.JobScheduler.BL
{

    public class JobsDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
        {
        }
    }
}