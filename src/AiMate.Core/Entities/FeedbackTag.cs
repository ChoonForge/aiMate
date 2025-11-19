namespace AiMate.Core.Entities;

/// <summary>
/// A tag applied to feedback (e.g., "Accurate", "Helpful", "Too Verbose")
/// </summary>
public class FeedbackTag
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageFeedbackId { get; set; }
    public MessageFeedback? MessageFeedback { get; set; }

    /// <summary>
    /// Tag key/category (e.g., "accuracy", "tone", "helpfulness")
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Tag value (e.g., "high", "professional", "very helpful")
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Optional color for UI display (hex code)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Whether this is a positive, negative, or neutral tag
    /// </summary>
    public TagSentiment Sentiment { get; set; } = TagSentiment.Neutral;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sentiment of a feedback tag
/// </summary>
public enum TagSentiment
{
    Positive,
    Neutral,
    Negative
}
