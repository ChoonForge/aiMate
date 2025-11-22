namespace AiMate.Core.Services;

/// <summary>
/// Service for scheduling and managing background jobs
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue a background job to run immediately
    /// </summary>
    string Enqueue(Action job);

    /// <summary>
    /// Schedule a background job to run after a delay
    /// </summary>
    string Schedule(Action job, TimeSpan delay);

    /// <summary>
    /// Schedule a recurring job with cron expression
    /// </summary>
    void AddOrUpdateRecurringJob(string jobId, Action job, string cronExpression);

    /// <summary>
    /// Remove a recurring job
    /// </summary>
    void RemoveRecurringJob(string jobId);

    /// <summary>
    /// Delete a background job
    /// </summary>
    bool Delete(string jobId);
}

/// <summary>
/// Background jobs for system maintenance and async tasks
/// </summary>
public interface IBackgroundJobs
{
    /// <summary>
    /// Clean up old error logs (resolved errors older than 90 days)
    /// </summary>
    Task CleanupOldErrorLogsAsync();

    /// <summary>
    /// Clean up old resolved feedback (older than 1 year)
    /// </summary>
    Task CleanupOldFeedbackAsync();

    /// <summary>
    /// Generate embeddings for knowledge items without embeddings
    /// </summary>
    Task GenerateMissingEmbeddingsAsync();

    /// <summary>
    /// Send daily summary email to admins
    /// </summary>
    Task SendDailySummaryEmailAsync();

    /// <summary>
    /// Clean up orphaned files (files in storage without database records)
    /// </summary>
    Task CleanupOrphanedFilesAsync();
}
