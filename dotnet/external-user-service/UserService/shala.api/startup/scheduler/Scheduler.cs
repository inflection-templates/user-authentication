using System.Linq.Expressions;
using Hangfire;

namespace shala.api.startup;

// This scheduler uses hangfire in the background to schedule tasks
public static class Scheduler
{

    // NOTE: Here we are using static methods to schedule tasks, e.g. BackgroundJob.Enqueue and RecurringJob.AddOrUpdate
    // We can also use instance methods with IBackgroundJobClient and IRecurringJobManager which can be injected.
    // Examples of these classes are added in the Schedules folder.

    public static void FireAndForget(Expression<Action> methodCall)
    {
        BackgroundJob.Enqueue(methodCall);
    }

    public static void ScheduleRecurringTask(string jobName, Expression<Action> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobName, methodCall, cronExpression);
    }

    public static void ScheduleDelayedTask(Expression<Action> methodCall, int delayInSeconds)
    {
        BackgroundJob.Schedule(methodCall, TimeSpan.FromSeconds(delayInSeconds));
    }

    public static void FireAndForget<T>(Expression<Action<T>> methodCall)
    {
        BackgroundJob.Enqueue(methodCall);
    }

    public static void ScheduleRecurringTask<T>(string jobName, Expression<Action<T>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobName, methodCall, cronExpression);
    }

    public static void ScheduleDelayedTask<T>(Expression<Action<T>> methodCall, int delayInSeconds)
    {
        BackgroundJob.Schedule(methodCall, TimeSpan.FromSeconds(delayInSeconds));
    }

}
