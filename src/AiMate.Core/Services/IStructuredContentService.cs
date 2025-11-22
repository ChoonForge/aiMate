using AiMate.Core.Entities;
using AiMate.Core.Enums;

namespace AiMate.Core.Services;

/// <summary>
/// Service for managing structured content
/// </summary>
public interface IStructuredContentService
{
    // ============================================================================
    // Template Management
    // ============================================================================

    /// <summary>
    /// Get all templates
    /// </summary>
    Task<List<StructuredContentTemplate>> GetTemplatesAsync(
        StructuredContentType? type = null,
        bool includePrivate = false,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by ID
    /// </summary>
    Task<StructuredContentTemplate?> GetTemplateByIdAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by name and type
    /// </summary>
    Task<StructuredContentTemplate?> GetTemplateByNameAsync(
        string name,
        StructuredContentType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new template
    /// </summary>
    Task<StructuredContentTemplate> CreateTemplateAsync(
        StructuredContentTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing template
    /// </summary>
    Task<StructuredContentTemplate> UpdateTemplateAsync(
        StructuredContentTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a template
    /// </summary>
    Task DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    // ============================================================================
    // Content Operations
    // ============================================================================

    /// <summary>
    /// Parse structured content from text/JSON
    /// </summary>
    Task<StructuredContent?> ParseContentAsync(
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate structured content against a template
    /// </summary>
    Task<ContentValidationResult> ValidateContentAsync(
        StructuredContent content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Render structured content (prepare for UI)
    /// </summary>
    Task<RenderedContent> RenderContentAsync(
        StructuredContent content,
        string? templateName = null,
        CancellationToken cancellationToken = default);

    // ============================================================================
    // Action Execution
    // ============================================================================

    /// <summary>
    /// Execute an action
    /// </summary>
    Task<ActionResult> ExecuteActionAsync(
        ActionContext context,
        CancellationToken cancellationToken = default);

    // ============================================================================
    // Export
    // ============================================================================

    /// <summary>
    /// Export structured content to various formats
    /// </summary>
    Task<ExportResult> ExportAsync(
        StructuredContent content,
        ExportFormat format,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Structured content data model
/// </summary>
public class StructuredContent
{
    public string Version { get; set; } = "1.0";
    public StructuredContentType Type { get; set; }
    public string? Template { get; set; }
    public ContentMetadata Metadata { get; set; } = new();
    public object Schema { get; set; } = new();
    public object Data { get; set; } = new();
    public ContentOptions Options { get; set; } = new();
    public ContentStyles? Styles { get; set; }
}

/// <summary>
/// Content metadata
/// </summary>
public class ContentMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Content rendering options
/// </summary>
public class ContentOptions
{
    public bool Searchable { get; set; }
    public bool Filterable { get; set; }
    public bool Sortable { get; set; }
    public bool Paginated { get; set; }
    public int PageSize { get; set; } = 20;
    public bool Exportable { get; set; }
    public bool Selectable { get; set; }
    public bool MultiSelect { get; set; }
    public Dictionary<string, object> Custom { get; set; } = new();
}

/// <summary>
/// Content styling
/// </summary>
public class ContentStyles
{
    public string? Theme { get; set; }
    public Dictionary<string, string> Colors { get; set; } = new();
    public string? Spacing { get; set; }
    public Dictionary<string, string> Classes { get; set; } = new();
}

/// <summary>
/// Result of content validation
/// </summary>
public class ContentValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Rendered content ready for UI
/// </summary>
public class RenderedContent
{
    public StructuredContent Content { get; set; } = new();
    public StructuredContentTemplate? Template { get; set; }
    public Dictionary<string, object> RenderData { get; set; } = new();
}

/// <summary>
/// Export formats
/// </summary>
public enum ExportFormat
{
    Csv,
    Json,
    Excel,
    Pdf
}

/// <summary>
/// Result of export operation
/// </summary>
public class ExportResult
{
    public bool Success { get; set; }
    public byte[]? Data { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Error { get; set; }
}
