using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Afimilk.JobScheduler.BL
{
    public class JobHandlerFactory : IJobHandlerFactory
    {
        private readonly ConcurrentDictionary<string, Func<IJobHandler>> _jobHandlerTypes;

        public JobHandlerFactory(IServiceProvider serviceProvider)
        {
            var handlers = serviceProvider.GetServices<IJobHandler>();

            _jobHandlerTypes = new ConcurrentDictionary<string, Func<IJobHandler>>(
                handlers.ToDictionary(
                    handler => handler.GetType().Name,
                    handler => (Func<IJobHandler>)(() => (IJobHandler)serviceProvider.GetRequiredService(handler.GetType()))
                )
            );
        }

        public IJobHandler GetHandler(string jobType)
        {
            if (_jobHandlerTypes.TryGetValue(jobType, out var handler))
            {
                return handler();
            }
            throw new ArgumentException($"Invalid job type {jobType}. Valid job types are: {string.Join(", ", _jobHandlerTypes.Keys)}");
        }

        public IEnumerable<string> GetJobTypeNames() => _jobHandlerTypes.Keys;
    }
}