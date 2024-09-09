public class Job
{
  
    // todo:use primary key
    public int Id { get; set; }
    public string Type { get; set; }
    public TimeSpan DailyExecutionTime { get; set; } // Use TimeSpan to store the time of day
    public int Occurrences { get; set; }
    public int RemainingOccurrences { get; set; }
    // public DateTime? LastRun { get; set; }
    public DateTime ExecutionStarted { get; set; }
    public DateTime ExecutionCompleted { get; set; }
}