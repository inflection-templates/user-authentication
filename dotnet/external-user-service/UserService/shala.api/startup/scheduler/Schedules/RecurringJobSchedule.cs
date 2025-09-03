using Hangfire;

public class MyRecurringJobService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public MyRecurringJobService(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public void ScheduleRecurringJob()
    {
        // Schedules a job to run every day at 12:00 noon (Cron format: "0 12 * * *")
        _recurringJobManager.AddOrUpdate("DailyTask",
            () => DoWork("Recurring job executed!"),
            "0 12 * * *");
    }

    // The method to be executed as a recurring job
    public void DoWork(string message)
    {
        Console.WriteLine(message);
    }
}
