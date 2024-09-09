namespace Afimilk.JobScheduler.BL
{
    public abstract class JobHandler
    {
        public abstract Task ExecuteAsync(Job job);
        public virtual string GetJobName() => this.GetType().Name;
    }
}