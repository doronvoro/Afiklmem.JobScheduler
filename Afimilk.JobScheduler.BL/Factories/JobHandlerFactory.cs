using System.Collections.Concurrent;

namespace Afimilk.JobScheduler.BL
{
    public class JobHandlerFactory : IJobHandlerFactory
    {
        private ConcurrentDictionary<string, Func<JobHandler>> _jobHandlerTypes;

        public JobHandlerFactory()
        {

            var dictionary = new Dictionary<string, Func<JobHandler>>
             {
                 { nameof(ReportingJob), () => new ReportingJob() },
                 { nameof(MaintenanceJob), () => new MaintenanceJob() }
             };

            _jobHandlerTypes = new ConcurrentDictionary<string, Func<JobHandler>>(dictionary);
        }

        public JobHandler GetHandler(string jobType)
        {
            if (_jobHandlerTypes.TryGetValue(jobType, out var handler))
            {
                return handler();
            }
            else
            {
                throw new Exception($"Invalid job type {jobType}");
            }
        }

        public IEnumerable<string> GetJobTypeNames() => _jobHandlerTypes.Keys;
    }

}