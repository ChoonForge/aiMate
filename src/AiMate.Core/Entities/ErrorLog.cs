using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Automated error logging from frontend for debugging and monitoring
/// </summary>
public class ErrorLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who experienced the error (nullable for pre-auth errors)
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Type of error
    /// </summary>
    public ErrorType ErrorType { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    [MaxLength(5000)]
    public required string Message { get; set; }

    /// <summary>
    /// Stack trace (if available)
    /// </summary>
    [MaxLength(10000)]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Component or file where error occurred
    /// </summary>
    [MaxLength(200)]
    public string? ComponentName { get; set; }

    /// <summary>
    /// URL where error occurred
    /// </summary>
    [Url]
    [MaxLength(2048)]
    public string? Url { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Browser info (name, version)
    /// </summary>
    [MaxLength(200)]
    public string? BrowserInfo { get; set; }

    /// <summary>
    /// Additional context data (JSON)
    /// </summary>
    [MaxLength(10000)]
    public string? AdditionalDataJson { get; set; }

    /// <summary>
    /// Error severity
    /// </summary>
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Medium;

    /// <summary>
    /// Whether the error has been resolved
    /// </summary>
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// Resolution notes
    /// </summary>
    [MaxLength(5000)]
    public string? Resolution { get; set; }

    /// <summary>
    /// Number of times this exact error has occurred
    /// </summary>
    [Range(1, int.MaxValue)]
    public int OccurrenceCount { get; set; } = 1;

    /// <summary>
    /// First time this error occurred
    /// </summary>
    public DateTime FirstOccurrence { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Most recent occurrence
    /// </summary>
    public DateTime LastOccurrence { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
