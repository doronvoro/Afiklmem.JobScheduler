
namespace Afimilk.JobScheduler.BL
{
    public interface IJobHandlerFactory
    {
        JobHandler GetHandler(string taskName);
        IEnumerable<string> GetJobTypeNames();
    }

}