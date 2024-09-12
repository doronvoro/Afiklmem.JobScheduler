//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Afimilk.JobScheduler.BL.Scheduler
//{
//    using System.Threading.Tasks.Dataflow;

//    public class JobScheduler
//    {
//        private readonly ActionBlock<Job> _jobExecutionBlock;

//        public JobScheduler()
//        {
//            // Create an ActionBlock that limits concurrency
//            _jobExecutionBlock = new ActionBlock<Job>(async job => await RunJobAsync(job),
//                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }); // Max 5 concurrent jobs
//        }

//        public async Task ExecuteDueJobsAsync(List<Job> jobs)
//        {
//            foreach (var job in jobs)
//            {
//                // Post each job to the ActionBlock, which will execute it with the concurrency limit
//                _jobExecutionBlock.Post(job);
//            }

//            // Wait for all jobs to complete
//            _jobExecutionBlock.Complete();
//            await _jobExecutionBlock.Completion;
//        }
//    }

//}
