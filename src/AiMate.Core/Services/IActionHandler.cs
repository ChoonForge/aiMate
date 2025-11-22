using AiMate.Core.Enums;

namespace AiMate.Core.Services;

/// <summary>
/// Interface for action handlers
/// </summary>
public interface IActionHandler
{
    /// <summary>
    /// Type of action this handler processes
    /// </summary>
    ActionHandlerType HandlerType { get; }

    /// <summary>
    /// Execute the action
    /// </summary>
    Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate the action configuration
    /// </summary>
    Task<ValidationResult> ValidateAsync(ActionContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for action execution
/// </summary>
public class ActionContext
{
    /// <summary>
    /// Unique action ID
    /// </summary>
    public required string ActionId { get; set; }

    /// <summary>
    /// Handler type
    /// </summary>
    public ActionHandlerType HandlerType { get; set; }

    /// <summary>
    /// Action configuration as dictionary
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = new();

    /// <summary>
    /// Parameters passed to the action
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// User executing the action
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional item ID (for row actions)
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// Optional selected items (for bulk actions)
    /// </summary>
    public List<string>? SelectedItems { get; set; }

    /// <summary>
    /// Metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of action execution
/// </summary>
public class ActionResult
{
    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Result data (URL for navigation, modal config, etc.)
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Error details if failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Whether to refresh the content after action
    /// </summary>
    public bool RefreshContent { get; set; }

    /// <summary>
    /// Whether to close the modal/dialog after action
    /// </summary>
    public bool CloseDialog { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
