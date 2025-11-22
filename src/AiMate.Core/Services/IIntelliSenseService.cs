namespace AiMate.Core.Services;

/// <summary>
/// Service for providing IntelliSense features
/// </summary>
public interface IIntelliSenseService
{
    /// <summary>
    /// Get code completions at a specific position
    /// </summary>
    Task<List<CompletionItem>> GetCompletionsAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signature help for a method call
    /// </summary>
    Task<SignatureHelp?> GetSignatureHelpAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hover information for a symbol
    /// </summary>
    Task<HoverInfo?> GetHoverInfoAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get definition location for a symbol
    /// </summary>
    Task<List<Location>> GetDefinitionAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get references to a symbol
    /// </summary>
    Task<List<Location>> GetReferencesAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A code completion item
/// </summary>
public class CompletionItem
{
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // Method, Property, Class, etc.
    public string Detail { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string InsertText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

/// <summary>
/// Signature help for method calls
/// </summary>
public class SignatureHelp
{
    public List<SignatureInformation> Signatures { get; set; } = new();
    public int ActiveSignature { get; set; }
    public int ActiveParameter { get; set; }
}

/// <summary>
/// Information about a method signature
/// </summary>
public class SignatureInformation
{
    public string Label { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public List<ParameterInformation> Parameters { get; set; } = new();
}

/// <summary>
/// Information about a parameter
/// </summary>
public class ParameterInformation
{
    public string Label { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Hover information for a symbol
/// </summary>
public class HoverInfo
{
    public string Content { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
}

/// <summary>
/// A location in code
/// </summary>
public class Location
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}
