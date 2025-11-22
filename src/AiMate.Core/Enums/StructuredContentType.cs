namespace AiMate.Core.Enums;

/// <summary>
/// Types of structured content that can be rendered
/// </summary>
public enum StructuredContentType
{
    /// <summary>
    /// Tabular data with sorting, filtering, pagination
    /// </summary>
    Table,

    /// <summary>
    /// Key-value pairs, property lists, configuration
    /// </summary>
    KeyValueList,

    /// <summary>
    /// Card-based grid layouts
    /// </summary>
    Card,

    /// <summary>
    /// Chronological timeline of events
    /// </summary>
    Timeline,

    /// <summary>
    /// Data visualizations (bar, line, pie, etc.)
    /// </summary>
    Chart,

    /// <summary>
    /// Interactive forms with validation
    /// </summary>
    Form,

    /// <summary>
    /// Code blocks with syntax highlighting
    /// </summary>
    CodeBlock,

    /// <summary>
    /// Hierarchical file/folder structures
    /// </summary>
    FileTree,

    /// <summary>
    /// Image/media galleries
    /// </summary>
    Gallery,

    /// <summary>
    /// Kanban board layouts
    /// </summary>
    Kanban,

    /// <summary>
    /// User-defined custom types
    /// </summary>
    Custom
}

/// <summary>
/// Action handler types
/// </summary>
public enum ActionHandlerType
{
    /// <summary>
    /// Navigate to a URL or route
    /// </summary>
    Navigation,

    /// <summary>
    /// Open a modal dialog
    /// </summary>
    Modal,

    /// <summary>
    /// Call a backend API
    /// </summary>
    ApiCall,

    /// <summary>
    /// Export data to file
    /// </summary>
    Export,

    /// <summary>
    /// Copy to clipboard
    /// </summary>
    Copy,

    /// <summary>
    /// User-defined custom handler
    /// </summary>
    Custom
}
