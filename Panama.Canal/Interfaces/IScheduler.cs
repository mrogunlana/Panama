namespace Panama.Canal.Interfaces
{
    public interface IScheduler : ICanalService
    {
        Quartz.IScheduler? Current { get; }
    }
}
