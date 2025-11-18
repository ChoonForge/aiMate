namespace AiMate.Core.Entities;

/// <summary>
/// Admin-defined template for feedback tags
/// Users can select from these predefined options when providing feedback
/// </summary>
public class FeedbackTagTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Category name (e.g., "Quality", "Tone", "Accuracy")
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Display label for the UI
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Description of what this tag category measures
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Available options for this tag category
    /// </summary>
    public List<FeedbackTagOption> Options { get; set; } = new();

    /// <summary>
    /// Whether this tag template is active and should be shown to users
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order in the UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this tag is required when providing feedback
    /// </summary>
    public bool IsRequired { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Individual option within a feedback tag template
/// </summary>
public class FeedbackTagOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FeedbackTagTemplateId { get; set; }
    public FeedbackTagTemplate? FeedbackTagTemplate { get; set; }

    /// <summary>
    /// Display value for this option
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Color for UI display (hex code)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Sentiment associated with this option
    /// </summary>
    public TagSentiment Sentiment { get; set; } = TagSentiment.Neutral;

    /// <summary>
    /// Display order within the template
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Optional icon name (MudBlazor icon)
    /// </summary>
    public string? Icon { get; set; }
}
