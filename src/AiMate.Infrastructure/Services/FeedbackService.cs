using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for managing message feedback and ratings
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(AiMateDbContext context, ILogger<FeedbackService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MessageFeedback> CreateOrUpdateFeedbackAsync(
        Guid messageId,
        Guid userId,
        int rating,
        string? textFeedback,
        List<FeedbackTag> tags,
        string? modelId = null,
        long? responseTimeMs = null,
        CancellationToken cancellationToken = default)
    {
        // Validate rating range
        if (rating < 1 || rating > 10)
        {
            throw new ArgumentException("Rating must be between 1 and 10", nameof(rating));
        }

        // Check if feedback already exists
        var existingFeedback = await _context.MessageFeedbacks
            .Include(f => f.Tags)
            .FirstOrDefaultAsync(f => f.MessageId == messageId && f.UserId == userId, cancellationToken);

        if (existingFeedback != null)
        {
            // Update existing feedback
            existingFeedback.Rating = rating;
            existingFeedback.TextFeedback = textFeedback;
            existingFeedback.ModelId = modelId;
            existingFeedback.ResponseTimeMs = responseTimeMs;
            existingFeedback.UpdatedAt = DateTime.UtcNow;

            // Remove old tags and add new ones
            _context.FeedbackTags.RemoveRange(existingFeedback.Tags);

            foreach (var tag in tags)
            {
                tag.MessageFeedbackId = existingFeedback.Id;
                existingFeedback.Tags.Add(tag);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated feedback {FeedbackId} for message {MessageId}", existingFeedback.Id, messageId);
            return existingFeedback;
        }
        else
        {
            // Create new feedback
            var feedback = new MessageFeedback
            {
                MessageId = messageId,
                UserId = userId,
                Rating = rating,
                TextFeedback = textFeedback,
                ModelId = modelId,
                ResponseTimeMs = responseTimeMs,
                Tags = tags
            };

            foreach (var tag in tags)
            {
                tag.MessageFeedbackId = feedback.Id;
            }

            _context.MessageFeedbacks.Add(feedback);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created feedback {FeedbackId} for message {MessageId}", feedback.Id, messageId);
            return feedback;
        }
    }

    public async Task<MessageFeedback?> GetFeedbackByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MessageFeedbacks
            .Include(f => f.Tags)
            .Include(f => f.Message)
            .FirstOrDefaultAsync(f => f.MessageId == messageId, cancellationToken);
    }

    public async Task<List<MessageFeedback>> GetFeedbackByUserIdAsync(
        Guid userId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.MessageFeedbacks
            .Include(f => f.Tags)
            .Include(f => f.Message)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MessageFeedback>> GetFeedbackByModelIdAsync(
        string modelId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minRating = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MessageFeedbacks
            .Include(f => f.Tags)
            .Where(f => f.ModelId == modelId);

        if (fromDate.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(f => f.CreatedAt <= toDate.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(f => f.Rating >= minRating.Value);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteFeedbackAsync(
        Guid feedbackId,
        CancellationToken cancellationToken = default)
    {
        var feedback = await _context.MessageFeedbacks.FindAsync(new object[] { feedbackId }, cancellationToken);
        if (feedback == null)
        {
            return false;
        }

        _context.MessageFeedbacks.Remove(feedback);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted feedback {FeedbackId}", feedbackId);
        return true;
    }

    public async Task<List<FeedbackTagTemplate>> GetActiveTagTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.FeedbackTagTemplates
            .Include(t => t.Options.OrderBy(o => o.DisplayOrder))
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<FeedbackTagTemplate> CreateTagTemplateAsync(
        string category,
        string label,
        string? description,
        List<FeedbackTagOption> options,
        bool isRequired = false,
        CancellationToken cancellationToken = default)
    {
        var maxOrder = await _context.FeedbackTagTemplates
            .MaxAsync(t => (int?)t.DisplayOrder, cancellationToken) ?? 0;

        var template = new FeedbackTagTemplate
        {
            Category = category,
            Label = label,
            Description = description,
            IsRequired = isRequired,
            DisplayOrder = maxOrder + 1,
            Options = options
        };

        // Set proper IDs and relationships
        foreach (var option in options)
        {
            option.FeedbackTagTemplateId = template.Id;
        }

        _context.FeedbackTagTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created tag template {TemplateId} for category {Category}", template.Id, category);
        return template;
    }

    public async Task<FeedbackTagTemplate> UpdateTagTemplateAsync(
        Guid templateId,
        string? category = null,
        string? label = null,
        string? description = null,
        bool? isActive = null,
        bool? isRequired = null,
        int? displayOrder = null,
        CancellationToken cancellationToken = default)
    {
        var template = await _context.FeedbackTagTemplates.FindAsync(new object[] { templateId }, cancellationToken);
        if (template == null)
        {
            throw new KeyNotFoundException($"Tag template {templateId} not found");
        }

        if (category != null) template.Category = category;
        if (label != null) template.Label = label;
        if (description != null) template.Description = description;
        if (isActive.HasValue) template.IsActive = isActive.Value;
        if (isRequired.HasValue) template.IsRequired = isRequired.Value;
        if (displayOrder.HasValue) template.DisplayOrder = displayOrder.Value;

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated tag template {TemplateId}", templateId);
        return template;
    }

    public async Task<bool> DeleteTagTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _context.FeedbackTagTemplates.FindAsync(new object[] { templateId }, cancellationToken);
        if (template == null)
        {
            return false;
        }

        _context.FeedbackTagTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted tag template {TemplateId}", templateId);
        return true;
    }

    public async Task<FeedbackTagOption> AddTagOptionAsync(
        Guid templateId,
        string value,
        string? color,
        TagSentiment sentiment,
        string? icon = null,
        CancellationToken cancellationToken = default)
    {
        var template = await _context.FeedbackTagTemplates
            .Include(t => t.Options)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Tag template {templateId} not found");
        }

        var maxOrder = template.Options.Count > 0
            ? template.Options.Max(o => o.DisplayOrder)
            : 0;

        var option = new FeedbackTagOption
        {
            FeedbackTagTemplateId = templateId,
            Value = value,
            Color = color,
            Sentiment = sentiment,
            Icon = icon,
            DisplayOrder = maxOrder + 1
        };

        template.Options.Add(option);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added option {OptionId} to template {TemplateId}", option.Id, templateId);
        return option;
    }

    public async Task<bool> RemoveTagOptionAsync(
        Guid optionId,
        CancellationToken cancellationToken = default)
    {
        var option = await _context.FeedbackTagOptions.FindAsync(new object[] { optionId }, cancellationToken);
        if (option == null)
        {
            return false;
        }

        _context.FeedbackTagOptions.Remove(option);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed tag option {OptionId}", optionId);
        return true;
    }

    public async Task<ModelFeedbackStats> GetModelStatsAsync(
        string modelId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MessageFeedbacks
            .Include(f => f.Tags)
            .Where(f => f.ModelId == modelId);

        if (fromDate.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(f => f.CreatedAt <= toDate.Value);
        }

        var feedbacks = await query.ToListAsync(cancellationToken);

        if (feedbacks.Count == 0)
        {
            return new ModelFeedbackStats
            {
                ModelId = modelId,
                TotalFeedbacks = 0,
                AverageRating = 0,
                RatingDistribution = new Dictionary<int, int>(),
                TagCounts = new Dictionary<string, int>()
            };
        }

        var stats = new ModelFeedbackStats
        {
            ModelId = modelId,
            TotalFeedbacks = feedbacks.Count,
            AverageRating = feedbacks.Average(f => f.Rating),
            HighestRating = feedbacks.Max(f => f.Rating),
            LowestRating = feedbacks.Min(f => f.Rating),
            RatingDistribution = feedbacks
                .GroupBy(f => f.Rating)
                .ToDictionary(g => g.Key, g => g.Count()),
            TagCounts = feedbacks
                .SelectMany(f => f.Tags)
                .GroupBy(t => $"{t.Key}:{t.Value}")
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageResponseTimeMs = feedbacks
                .Where(f => f.ResponseTimeMs.HasValue)
                .Select(f => f.ResponseTimeMs!.Value)
                .DefaultIfEmpty()
                .Average()
        };

        return stats;
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetTagFrequencyAsync(
        string? modelId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FeedbackTags
            .Include(t => t.MessageFeedback)
            .AsQueryable();

        if (!string.IsNullOrEmpty(modelId))
        {
            query = query.Where(t => t.MessageFeedback!.ModelId == modelId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.MessageFeedback!.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.MessageFeedback!.CreatedAt <= toDate.Value);
        }

        var tags = await query.ToListAsync(cancellationToken);

        return tags
            .GroupBy(t => t.Key)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(t => t.Value)
                      .ToDictionary(vg => vg.Key, vg => vg.Count())
            );
    }
}
