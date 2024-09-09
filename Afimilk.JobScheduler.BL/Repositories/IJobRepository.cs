namespace Afimilk.JobScheduler.BL
{
    public interface IJobRepository
    {
        Task<List<Job>> GetAllJobsAsync();
        Task<Job> GetJobByIdAsync(int id);
        Task AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
        Task DeleteJobAsync(int id);
        Task<List<Job>> GetJobsToRunAsync();
    }
}