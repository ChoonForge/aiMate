using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing message feedback and ratings
/// </summary>
public interface IFeedbackService
{
    /// <summary>
    /// Create or update feedback for a message
    /// </summary>
    Task<MessageFeedback> CreateOrUpdateFeedbackAsync(
        Guid messageId,
        Guid userId,
        int rating,
        string? textFeedback,
        List<FeedbackTag> tags,
        string? modelId = null,
        long? responseTimeMs = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback for a specific message
    /// </summary>
    Task<MessageFeedback?> GetFeedbackByMessageIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all feedback for a user
    /// </summary>
    Task<List<MessageFeedback>> GetFeedbackByUserIdAsync(
        Guid userId,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback by model with filtering
    /// </summary>
    Task<List<MessageFeedback>> GetFeedbackByModelIdAsync(
        string modelId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minRating = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete feedback
    /// </summary>
    Task<bool> DeleteFeedbackAsync(
        Guid feedbackId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active feedback tag templates
    /// </summary>
    Task<List<FeedbackTagTemplate>> GetActiveTagTemplatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new tag template (admin)
    /// </summary>
    Task<FeedbackTagTemplate> CreateTagTemplateAsync(
        string category,
        string label,
        string? description,
        List<FeedbackTagOption> options,
        bool isRequired = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a tag template (admin)
    /// </summary>
    Task<FeedbackTagTemplate> UpdateTagTemplateAsync(
        Guid templateId,
        string? category = null,
        string? label = null,
        string? description = null,
        bool? isActive = null,
        bool? isRequired = null,
        int? displayOrder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a tag template (admin)
    /// </summary>
    Task<bool> DeleteTagTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add option to a tag template (admin)
    /// </summary>
    Task<FeedbackTagOption> AddTagOptionAsync(
        Guid templateId,
        string value,
        string? color,
        TagSentiment sentiment,
        string? icon = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove option from a tag template (admin)
    /// </summary>
    Task<bool> RemoveTagOptionAsync(
        Guid optionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback statistics for a model
    /// </summary>
    Task<ModelFeedbackStats> GetModelStatsAsync(
        string modelId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tag frequency statistics
    /// </summary>
    Task<Dictionary<string, Dictionary<string, int>>> GetTagFrequencyAsync(
        string? modelId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics for model feedback
/// </summary>
public class ModelFeedbackStats
{
    public string ModelId { get; set; } = string.Empty;
    public int TotalFeedbacks { get; set; }
    public double AverageRating { get; set; }
    public int HighestRating { get; set; }
    public int LowestRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public Dictionary<string, int> TagCounts { get; set; } = new();
    public long? AverageResponseTimeMs { get; set; }
}
