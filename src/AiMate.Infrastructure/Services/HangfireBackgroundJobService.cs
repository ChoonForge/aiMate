using AiMate.Core.Services;
using Hangfire;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Hangfire implementation of background job service
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue(Action job)
    {
        return BackgroundJob.Enqueue(() => job());
    }

    public string Schedule(Action job, TimeSpan delay)
    {
        return BackgroundJob.Schedule(() => job(), delay);
    }

    public void AddOrUpdateRecurringJob(string jobId, Action job, string cronExpression)
    {
        RecurringJob.AddOrUpdate(jobId, () => job(), cronExpression);
    }

    public void RemoveRecurringJob(string jobId)
    {
        RecurringJob.RemoveIfExists(jobId);
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }
}
