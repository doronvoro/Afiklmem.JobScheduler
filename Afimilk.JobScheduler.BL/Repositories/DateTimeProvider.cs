namespace Afimilk.JobScheduler.BL
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public TimeSpan GetCurrentTime() => DateTime.Now.TimeOfDay;
    }
}
