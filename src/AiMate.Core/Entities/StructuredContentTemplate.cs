using AiMate.Core.Enums;

namespace AiMate.Core.Entities;

/// <summary>
/// Template for rendering structured content
/// </summary>
public class StructuredContentTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Template name (e.g., "default", "compact", "custom-name")
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Type of content this template renders
    /// </summary>
    public StructuredContentType Type { get; set; }

    /// <summary>
    /// JSON schema definition for the template
    /// </summary>
    public required string SchemaJson { get; set; }

    /// <summary>
    /// Optional path to custom component (for Blazor)
    /// </summary>
    public string? ComponentPath { get; set; }

    /// <summary>
    /// Custom styling (Tailwind classes, CSS, etc.) as JSON
    /// </summary>
    public string? StylesJson { get; set; }

    /// <summary>
    /// Whether this is a built-in template
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// Whether this template is publicly available
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// User who created this template (null for built-in)
    /// </summary>
    public Guid? CreatedBy { get; set; }
    public User? Creator { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preview/thumbnail image URL
    /// </summary>
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Usage count (for analytics)
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Template version
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// When the template was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the template was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
