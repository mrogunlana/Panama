namespace Panama.Jobs
{
    public interface IScheduler
    {
        void Start();
        void Stop();
        void Queue<T>(T job);
        void Queue<T>(T job, int minutes);
        int Count();
    }
}
