namespace AiMate.Core.Entities;

/// <summary>
/// Enhanced feedback for a message with detailed ratings and tags
/// </summary>
public class MessageFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }
    public Message? Message { get; set; }

    /// <summary>
    /// Rating on a 1-10 scale
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Optional text feedback from the user
    /// </summary>
    public string? TextFeedback { get; set; }

    /// <summary>
    /// Tags associated with this feedback
    /// </summary>
    public List<FeedbackTag> Tags { get; set; } = new();

    /// <summary>
    /// When this feedback was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this feedback was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Model that generated the message being rated
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Response time in milliseconds (for performance tracking)
    /// </summary>
    public long? ResponseTimeMs { get; set; }

    /// <summary>
    /// User who provided the feedback
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
