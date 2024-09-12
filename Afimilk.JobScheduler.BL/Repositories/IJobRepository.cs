
namespace Afimilk.JobScheduler.BL
{
    public interface IJobRepository
    {
        Task<List<Job>> GetAllJobsAsync();
        Task<Job> GetJobByIdAsync(int id);
        Task AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
        Task<bool> DeleteJobAsync(int id);
        Task<List<Job>> GetJobsToRunAsync(IEnumerable<int> notIncludeIds);
    }
}