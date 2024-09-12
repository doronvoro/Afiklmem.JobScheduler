
namespace Afimilk.JobScheduler.BL
{
    public interface IJobHandlerFactory
    {
        IJobHandler GetHandler(string jobType);
        IEnumerable<string> GetJobTypeNames();
    }
}