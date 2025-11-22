using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Implementation of background maintenance jobs
/// </summary>
public class BackgroundJobs : IBackgroundJobs
{
    private readonly AiMateDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<BackgroundJobs> _logger;

    public BackgroundJobs(
        AiMateDbContext context,
        IEmbeddingService embeddingService,
        IFileStorageService fileStorage,
        ILogger<BackgroundJobs> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task CleanupOldErrorLogsAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-90);

            var oldErrors = await _context.ErrorLogs
                .Where(e => e.IsResolved && e.ResolvedAt < cutoffDate)
                .ToListAsync();

            if (oldErrors.Any())
            {
                _context.ErrorLogs.RemoveRange(oldErrors);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old error logs", oldErrors.Count);
            }
            else
            {
                _logger.LogInformation("No old error logs to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old error logs");
        }
    }

    public async Task CleanupOldFeedbackAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-1);

            var oldFeedback = await _context.UserFeedbacks
                .Where(f => f.Status == Core.Enums.FeedbackStatus.Resolved &&
                           f.ResolvedAt < cutoffDate)
                .ToListAsync();

            if (oldFeedback.Any())
            {
                _context.UserFeedbacks.RemoveRange(oldFeedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old feedback records", oldFeedback.Count);
            }
            else
            {
                _logger.LogInformation("No old feedback to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old feedback");
        }
    }

    public async Task GenerateMissingEmbeddingsAsync()
    {
        try
        {
            // Find knowledge items without embeddings
            var itemsWithoutEmbeddings = await _context.KnowledgeItems
                .Where(k => k.Embedding == null)
                .Take(50) // Process in batches of 50
                .ToListAsync();

            if (!itemsWithoutEmbeddings.Any())
            {
                _logger.LogInformation("No knowledge items missing embeddings");
                return;
            }

            _logger.LogInformation("Generating embeddings for {Count} knowledge items", itemsWithoutEmbeddings.Count);

            var successCount = 0;
            var failCount = 0;

            foreach (var item in itemsWithoutEmbeddings)
            {
                try
                {
                    // Generate embedding for title + content
                    var text = $"{item.Title}\n{item.Content}";
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(text);

                    if (embedding != null && embedding.Length > 0)
                    {
                        item.Embedding = new Pgvector.Vector(embedding);
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to generate embedding for knowledge item {Id}", item.Id);
                        failCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating embedding for knowledge item {Id}", item.Id);
                    failCount++;
                }
            }

            if (successCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Generated {Success} embeddings, {Failed} failures", successCount, failCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate missing embeddings");
        }
    }

    public async Task SendDailySummaryEmailAsync()
    {
        try
        {
            _logger.LogInformation("Daily summary email job started");

            // Get statistics for the last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);

            var stats = new
            {
                NewUsers = await _context.Users.CountAsync(u => u.CreatedAt >= yesterday),
                NewConversations = await _context.Conversations.CountAsync(c => c.CreatedAt >= yesterday),
                NewFeedback = await _context.UserFeedbacks.CountAsync(f => f.CreatedAt >= yesterday),
                NewErrors = await _context.ErrorLogs.CountAsync(e => e.CreatedAt >= yesterday),
                UnresolvedErrors = await _context.ErrorLogs.CountAsync(e => !e.IsResolved),
                PendingFeedback = await _context.UserFeedbacks.CountAsync(f => f.Status == Core.Enums.FeedbackStatus.New)
            };

            // TODO: Send email to admins with stats
            // For now, just log the stats
            _logger.LogInformation("Daily Summary - New Users: {Users}, New Conversations: {Conversations}, " +
                "New Feedback: {Feedback}, New Errors: {Errors}, Unresolved Errors: {UnresolvedErrors}, Pending Feedback: {PendingFeedback}",
                stats.NewUsers, stats.NewConversations, stats.NewFeedback, stats.NewErrors, stats.UnresolvedErrors, stats.PendingFeedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily summary email");
        }
    }

    public async Task CleanupOrphanedFilesAsync()
    {
        try
        {
            _logger.LogInformation("Cleanup orphaned files job started");

            // Get all file paths from database
            var dbFilePaths = await _context.WorkspaceFiles
                .Select(f => f.FilePath)
                .ToHashSetAsync();

            // TODO: Scan storage directory and find files not in database
            // This is a simplified version - full implementation would need to recursively scan directories

            _logger.LogInformation("Orphaned files cleanup completed. Database has {Count} file references", dbFilePaths.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup orphaned files");
        }
    }
}
