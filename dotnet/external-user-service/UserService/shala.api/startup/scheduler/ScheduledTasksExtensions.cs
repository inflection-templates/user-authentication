namespace shala.api.startup.scheduler;

public static class ScheduledTasksExtensions
{
    public static WebApplication ScheduleTasks(this WebApplication app)
    {
        // Schedule tasks here

        Scheduler.FireAndForget(() => Console.WriteLine("Hello World!"));

        Scheduler.ScheduleRecurringTask("RecurringTask", () => Console.WriteLine("Recurring Task"), "*/15 * * * *");

        Scheduler.ScheduleDelayedTask(() => Console.WriteLine("Delayed Task"), 5);

        // Scheduler.FireAndForget<CustomService>(service => service.CustomMethod());

        return app;
    }
}
