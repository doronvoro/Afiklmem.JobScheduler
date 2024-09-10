using Microsoft.EntityFrameworkCore;
namespace Afimilk.JobScheduler.BL
{
    public class JobRepository : IJobRepository
    {
        private readonly JobsDbContext _dbContext;

        public JobRepository(JobsDbContext dbContext )
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

        public async Task<List<Job>> GetJobsToRunAsync(IEnumerable<int> notIncludeIds)
        {
            // Fetch all jobs with remaining occurrences from the database
            var allJobs = await _dbContext.Jobs
                .WhereIf(notIncludeIds.Count() > 0, job => !notIncludeIds.Contains(job.Id))
                .Where(job => job.RemainingOccurrences > 0)
                .ToListAsync();

            // Get the current time of day
            var currentTime = DateTime.Now.TimeOfDay;

            // Filter jobs based on the execution time in memory
            var jobsToRun = allJobs
                .Where(job => job.DailyExecutionTime <= currentTime)
                .ToList();

            return jobsToRun;
        }
    }
}