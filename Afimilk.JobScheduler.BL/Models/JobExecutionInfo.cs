namespace Afimilk.JobScheduler.BL
{
    public class JobExecutionInfo
    {
        public Job Job { get; set; } // Direct reference to the Job object
        public string ThreadName { get; set; }
        public string MainThread { get; set; }
        //public DateTime ExecutionStarted { get; set; }
        //public DateTime ExecutionCompleted { get; set; }
    }
}