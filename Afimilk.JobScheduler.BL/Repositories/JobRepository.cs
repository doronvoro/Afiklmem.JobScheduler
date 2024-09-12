using Microsoft.EntityFrameworkCore;
namespace Afimilk.JobScheduler.BL
{
    public class JobRepository : IJobRepository
    {
        private readonly JobsDbContext _dbContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public JobRepository(JobsDbContext dbContext, IDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
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
        
        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _dbContext.Jobs.FindAsync(id);
            if (job == null)
            {
                return false; // Or throw exception
            }
            _dbContext.Jobs.Remove(job);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Job>> GetJobsToRunAsync(IEnumerable<int> notIncludeIds)
        {
            var currentTime = _dateTimeProvider.GetCurrentTime();

            return await _dbContext.Jobs
                .WhereIf(notIncludeIds.Any(), job => !notIncludeIds.Contains(job.Id))
                .Where(job => job.RemainingOccurrences > 0 && job.DailyExecutionTime <= currentTime)
                .ToListAsync();
        }
    }
}
