using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// General user feedback for alpha testing and feature requests
/// </summary>
public class UserFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who submitted feedback (nullable for anonymous feedback)
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Optional email for anonymous feedback follow-up
    /// </summary>
    [EmailAddress]
    [MaxLength(320)]
    public string? UserEmail { get; set; }

    /// <summary>
    /// Type of feedback
    /// </summary>
    public FeedbackType FeedbackType { get; set; }

    /// <summary>
    /// Short subject/title
    /// </summary>
    [MaxLength(200)]
    public required string Subject { get; set; }

    /// <summary>
    /// Detailed feedback message
    /// </summary>
    [MaxLength(5000)]
    public required string Message { get; set; }

    /// <summary>
    /// Optional 1-5 star rating
    /// </summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>
    /// Page where feedback was submitted
    /// </summary>
    [MaxLength(500)]
    public string? CurrentPage { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Screen resolution
    /// </summary>
    [MaxLength(50)]
    public string? ScreenResolution { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    [MaxLength(10000)]
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Current status
    /// </summary>
    public FeedbackStatus Status { get; set; } = FeedbackStatus.New;

    /// <summary>
    /// Admin notes for internal tracking
    /// </summary>
    [MaxLength(5000)]
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Admin user assigned to this feedback
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
