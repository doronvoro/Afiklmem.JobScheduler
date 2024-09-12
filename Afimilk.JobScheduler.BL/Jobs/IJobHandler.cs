namespace Afimilk.JobScheduler.BL
{
    public interface IJobHandler
    {
        Task ExecuteAsync(Job job);
    }
}