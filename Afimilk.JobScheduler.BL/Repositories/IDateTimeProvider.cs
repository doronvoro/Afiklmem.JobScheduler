namespace Afimilk.JobScheduler.BL
{
    public interface IDateTimeProvider
    {
        TimeSpan GetCurrentTime();
    }
}
