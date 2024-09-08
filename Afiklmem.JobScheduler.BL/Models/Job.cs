namespace Afiklmem.JobScheduler.BL
{
    public class Job
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public TimeSpan RunAt { get; set; }
        public int Occurrences { get; set; }
        public int RemainingOccurrences { get; set; }
        public DateTime? LastRun { get; set; }
    }
}