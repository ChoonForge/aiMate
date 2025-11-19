using AiMate.Core.Entities;
using AiMate.Core.Services;
using Fluxor;

namespace AiMate.Web.Store.Feedback;

/// <summary>
/// Feedback state - manages message feedback and tag templates
/// </summary>
[FeatureState]
public record FeedbackState
{
    /// <summary>
    /// Feedback by message ID
    /// </summary>
    public Dictionary<Guid, MessageFeedback> FeedbackByMessageId { get; init; } = new();

    /// <summary>
    /// Available tag templates
    /// </summary>
    public List<FeedbackTagTemplate> TagTemplates { get; init; } = new();

    /// <summary>
    /// Model statistics by model ID
    /// </summary>
    public Dictionary<string, ModelFeedbackStats> ModelStats { get; init; } = new();

    /// <summary>
    /// Currently active feedback dialog (message ID)
    /// </summary>
    public Guid? ActiveFeedbackMessageId { get; init; }

    /// <summary>
    /// Is feedback being submitted
    /// </summary>
    public bool IsSubmitting { get; init; }

    /// <summary>
    /// Loading state
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Error message
    /// </summary>
    public string? Error { get; init; }
}
