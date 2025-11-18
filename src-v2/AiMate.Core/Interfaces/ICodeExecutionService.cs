namespace AiMate.Core.Interfaces;

/// <summary>
/// Service for executing code in various languages (primarily C# via Roslyn)
/// </summary>
public interface ICodeExecutionService
{
    /// <summary>
    /// Execute C# code using Roslyn scripting API
    /// </summary>
    /// <param name="code">The C# code to execute</param>
    /// <param name="timeout">Maximum execution time</param>
    /// <returns>Execution result with output, errors, and diagnostics</returns>
    Task<CodeExecutionResult> ExecuteCSharpAsync(string code, TimeSpan timeout);

    /// <summary>
    /// Compile C# code and return diagnostics without executing
    /// </summary>
    /// <param name="code">The C# code to compile</param>
    /// <returns>List of compilation diagnostics</returns>
    Task<List<CompilationDiagnostic>> CompileAsync(string code);

    /// <summary>
    /// Format C# code using Roslyn
    /// </summary>
    /// <param name="code">The C# code to format</param>
    /// <returns>Formatted code</returns>
    Task<string> FormatCodeAsync(string code);
}

/// <summary>
/// Result of code execution
/// </summary>
public class CodeExecutionResult
{
    public bool Success { get; set; }
    public object? ReturnValue { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Errors { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public List<CompilationDiagnostic> Diagnostics { get; set; } = new();
}

/// <summary>
/// Compilation diagnostic (error, warning, info)
/// </summary>
public class CompilationDiagnostic
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DiagnosticSeverity Severity { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}

/// <summary>
/// Severity levels for diagnostics
/// </summary>
public enum DiagnosticSeverity
{
    Hidden = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}
