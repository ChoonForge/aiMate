using Microsoft.CodeAnalysis;

namespace AiMate.Core.Services;

/// <summary>
/// Service for compiling and validating C# code using Roslyn
/// </summary>
public interface IRoslynCompilationService
{
    /// <summary>
    /// Compile C# code and return diagnostics
    /// </summary>
    Task<CompilationResult> CompileAsync(string code, CompilationOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate C# code syntax without full compilation
    /// </summary>
    Task<ValidationResult> ValidateSyntaxAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get semantic diagnostics for code
    /// </summary>
    Task<List<CodeDiagnostic>> GetSemanticDiagnosticsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Format C# code
    /// </summary>
    Task<string> FormatCodeAsync(string code, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of code compilation
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    public List<CodeDiagnostic> Errors { get; set; } = new();
    public List<CodeDiagnostic> Warnings { get; set; } = new();
    public List<CodeDiagnostic> Info { get; set; } = new();
    public byte[]? AssemblyBytes { get; set; }
    public TimeSpan CompilationTime { get; set; }
    public string? AssemblyName { get; set; }
}

/// <summary>
/// Result of syntax validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<CodeDiagnostic> Diagnostics { get; set; } = new();
}

/// <summary>
/// A diagnostic message from compilation
/// </summary>
public class CodeDiagnostic
{
    public string Id { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Error, Warning, Info
    public string Message { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public string? HelpLink { get; set; }
}

/// <summary>
/// Options for compilation
/// </summary>
public class CompilationOptions
{
    public string AssemblyName { get; set; } = "DynamicAssembly";
    public List<string> References { get; set; } = new();
    public bool OptimizeCode { get; set; } = false;
    public bool AllowUnsafe { get; set; } = false;
    public string? OutputKind { get; set; } // DynamicallyLinkedLibrary, ConsoleApplication, etc.
}
