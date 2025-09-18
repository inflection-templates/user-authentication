using Hangfire;

public class MyBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public MyBackgroundJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueJob()
    {
        _backgroundJobClient.Enqueue(() => DoWork("Fire-and-forget job executed!"));
    }

    public void ScheduleJob()
    {
        // Schedule a job to run in 10 minutes
        _backgroundJobClient.Schedule(() => DoWork("Scheduled job executed!"), TimeSpan.FromMinutes(10));
    }

    // The method to be executed in the background
    public void DoWork(string message)
    {
        Console.WriteLine(message);
    }
}
