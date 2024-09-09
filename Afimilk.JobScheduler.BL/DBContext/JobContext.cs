using Microsoft.EntityFrameworkCore;

namespace Afimilk.JobScheduler.BL
{

    public class JobsDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Id as the primary key and auto-increment
            modelBuilder.Entity<Job>()
                .HasKey(job => job.Id);

            modelBuilder.Entity<Job>()
                .Property(job => job.Id)
                .ValueGeneratedOnAdd();

            // Store TimeSpan as a string (custom conversion)
            modelBuilder.Entity<Job>()
                .Property(job => job.DailyExecutionTime)
                .HasConversion(
                    timeSpan => timeSpan.ToString(), // Convert TimeSpan to string for storage
                    timeSpanString => TimeSpan.Parse(timeSpanString) // Convert string back to TimeSpan
                );
        }
    }
}