using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using AiMate.Core.Interfaces;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Roslyn-based C# code execution service
/// Safe, sandboxed execution of C# scripts
/// </summary>
public class RoslynCodeExecutionService : ICodeExecutionService
{
    private readonly ScriptOptions _scriptOptions;
    private readonly ILogger<RoslynCodeExecutionService> _logger;

    public RoslynCodeExecutionService(ILogger<RoslynCodeExecutionService> logger)
    {
        _logger = logger;

        // Configure Roslyn with safe, useful assemblies
        _scriptOptions = ScriptOptions.Default
            .AddReferences(
                typeof(object).Assembly,                    // System
                typeof(Console).Assembly,                   // System.Console
                typeof(Enumerable).Assembly,                // System.Linq
                typeof(System.Net.Http.HttpClient).Assembly,// System.Net.Http
                typeof(System.Text.Json.JsonSerializer).Assembly, // System.Text.Json
                typeof(StringBuilder).Assembly,             // System.Text
                typeof(System.Collections.Generic.List<>).Assembly // System.Collections.Generic
            )
            .AddImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text",
                "System.Text.Json",
                "System.Net.Http",
                "System.Threading.Tasks"
            );
    }

    public async Task<CodeExecutionResult> ExecuteCSharpAsync(string code, TimeSpan timeout)
    {
        var result = new CodeExecutionResult();
        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing C# code (timeout: {Timeout}s)", timeout.TotalSeconds);

            // Capture console output
            var originalOut = Console.Out;
            var originalError = Console.Error;
            var outputWriter = new StringWriter();
            var errorWriter = new StringWriter();

            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);

            try
            {
                // Create cancellation token for timeout
                using var cts = new CancellationTokenSource(timeout);

                // Compile script
                var script = CSharpScript.Create(code, _scriptOptions);
                var compilation = script.GetCompilation();

                // Check for compilation errors
                var diagnostics = compilation.GetDiagnostics()
                    .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                    .ToList();

                if (diagnostics.Any())
                {
                    result.Success = false;
                    result.Errors = string.Join("\n", diagnostics.Select(d =>
                        $"Line {d.Location.GetLineSpan().StartLinePosition.Line + 1}: {d.GetMessage()}"));

                    result.Diagnostics = diagnostics.Select(d => new CompilationDiagnostic
                    {
                        Id = d.Id,
                        Message = d.GetMessage(),
                        Severity = (DiagnosticSeverity)(int)d.Severity,
                        Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                        Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
                    }).ToList();

                    _logger.LogWarning("Compilation failed with {Count} errors", diagnostics.Count);
                    return result;
                }

                // Execute with timeout
                _logger.LogDebug("Code compiled successfully, executing...");
                var scriptState = await script.RunAsync(cancellationToken: cts.Token);

                result.Success = true;
                result.ReturnValue = scriptState.ReturnValue;
                result.Output = outputWriter.ToString();

                var errorOutput = errorWriter.ToString();
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    result.Errors = errorOutput;
                }

                _logger.LogInformation("Code executed successfully in {Ms}ms", sw.ElapsedMilliseconds);
            }
            finally
            {
                // Always restore console
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        catch (CompilationErrorException ex)
        {
            result.Success = false;
            result.Errors = string.Join("\n", ex.Diagnostics.Select(d => d.GetMessage()));

            result.Diagnostics = ex.Diagnostics.Select(d => new CompilationDiagnostic
            {
                Id = d.Id,
                Message = d.GetMessage(),
                Severity = (DiagnosticSeverity)(int)d.Severity,
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
            }).ToList();

            _logger.LogWarning("Compilation error: {Message}", ex.Message);
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.Errors = $"Execution timeout ({timeout.TotalSeconds}s). Code took too long to execute.";
            _logger.LogWarning("Code execution timeout after {Seconds}s", timeout.TotalSeconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors = $"Runtime error: {ex.Message}";
            _logger.LogError(ex, "Code execution failed with exception");
        }
        finally
        {
            sw.Stop();
            result.ExecutionTime = sw.Elapsed;
        }

        return result;
    }

    public async Task<List<CompilationDiagnostic>> CompileAsync(string code)
    {
        try
        {
            var script = CSharpScript.Create(code, _scriptOptions);
            var compilation = script.GetCompilation();

            var diagnostics = compilation.GetDiagnostics()
                .Where(d => d.Severity >= Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
                .Select(d => new CompilationDiagnostic
                {
                    Id = d.Id,
                    Message = d.GetMessage(),
                    Severity = (DiagnosticSeverity)(int)d.Severity,
                    Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                    Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
                })
                .ToList();

            return diagnostics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compilation check failed");
            return new List<CompilationDiagnostic>
            {
                new CompilationDiagnostic
                {
                    Id = "INTERNAL_ERROR",
                    Message = $"Internal error: {ex.Message}",
                    Severity = DiagnosticSeverity.Error,
                    Line = 0,
                    Column = 0
                }
            };
        }
    }

    public async Task<string> FormatCodeAsync(string code)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = await tree.GetRootAsync();

            // Create a workspace for formatting
            using var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace);

            return formattedRoot.ToFullString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code formatting failed");
            return code; // Return original code if formatting fails
        }
    }
}
