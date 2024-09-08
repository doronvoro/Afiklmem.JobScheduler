using Microsoft.EntityFrameworkCore;
namespace Afiklmem.JobScheduler.BL
{
    public class JobRepository : IJobRepository
    {
        private readonly JobsDbContext _dbContext;

        public JobRepository(JobsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Job>> GetAllJobsAsync()
        {
            return await _dbContext.Jobs.ToListAsync();
        }

        public async Task<Job> GetJobByIdAsync(int id)
        {
            return await _dbContext.Jobs.FindAsync(id);
        }

        public async Task AddJobAsync(Job job)
        {
            await _dbContext.Jobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateJobAsync(Job job)
        {
            _dbContext.Jobs.Update(job);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteJobAsync(int id)
        {
            var job = await _dbContext.Jobs.FindAsync(id);
            if (job != null)
            {
                _dbContext.Jobs.Remove(job);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Job>> GetJobsToRunAsync(TimeSpan currentTime)
        {
            var currentTimeToday = DateTime.Today.Add(currentTime);
            //var a = await _dbContext.Jobs.ToListAsync();

            return await _dbContext.Jobs
                .Where(job =>
                    job.RunAt  <= currentTime
                    && job.RemainingOccurrences > 0)
                .ToListAsync();
        }

    }
}