using System.Diagnostics;
using AiMate.Core.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for compiling and validating C# code using Roslyn
/// </summary>
public class RoslynCompilationService : IRoslynCompilationService
{
    private readonly ILogger<RoslynCompilationService> _logger;
    private static readonly MetadataReference[] DefaultReferences = GetDefaultReferences();

    public RoslynCompilationService(ILogger<RoslynCompilationService> logger)
    {
        _logger = logger;
    }

    public async Task<Core.Services.CompilationResult> CompileAsync(
        string code,
        Core.Services.CompilationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Core.Services.CompilationResult();

        try
        {
            options ??= new Core.Services.CompilationOptions();

            // Parse the code into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);

            // Create compilation
            var compilation = CSharpCompilation.Create(
                assemblyName: options.AssemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: DefaultReferences,
                options: new CSharpCompilationOptions(
                    outputKind: GetOutputKind(options.OutputKind),
                    optimizationLevel: options.OptimizeCode ? OptimizationLevel.Release : OptimizationLevel.Debug,
                    allowUnsafe: options.AllowUnsafe
                )
            );

            // Get diagnostics
            var diagnostics = compilation.GetDiagnostics(cancellationToken);

            // Convert diagnostics to our format
            foreach (var diagnostic in diagnostics)
            {
                var codeDiagnostic = MapDiagnostic(diagnostic);

                switch (diagnostic.Severity)
                {
                    case DiagnosticSeverity.Error:
                        result.Errors.Add(codeDiagnostic);
                        break;
                    case DiagnosticSeverity.Warning:
                        result.Warnings.Add(codeDiagnostic);
                        break;
                    case DiagnosticSeverity.Info:
                        result.Info.Add(codeDiagnostic);
                        break;
                }
            }

            result.Success = !result.Errors.Any();

            // If successful, emit assembly
            if (result.Success)
            {
                using var ms = new MemoryStream();
                var emitResult = compilation.Emit(ms, cancellationToken: cancellationToken);

                if (emitResult.Success)
                {
                    result.AssemblyBytes = ms.ToArray();
                    result.AssemblyName = options.AssemblyName;
                }
                else
                {
                    result.Success = false;
                    foreach (var diagnostic in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        result.Errors.Add(MapDiagnostic(diagnostic));
                    }
                }
            }

            stopwatch.Stop();
            result.CompilationTime = stopwatch.Elapsed;

            _logger.LogInformation(
                "Compilation completed in {Time}ms - Success: {Success}, Errors: {Errors}, Warnings: {Warnings}",
                stopwatch.ElapsedMilliseconds, result.Success, result.Errors.Count, result.Warnings.Count
            );

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during compilation");
            result.Success = false;
            result.Errors.Add(new CodeDiagnostic
            {
                Id = "CS0000",
                Severity = "Error",
                Message = $"Compilation error: {ex.Message}",
                Line = 0,
                Column = 0
            });
            return result;
        }
    }

    public async Task<ValidationResult> ValidateSyntaxAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var diagnostics = syntaxTree.GetDiagnostics(cancellationToken);

            var result = new ValidationResult
            {
                IsValid = !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                Diagnostics = diagnostics.Select(MapDiagnostic).ToList()
            };

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during syntax validation");
            return new ValidationResult
            {
                IsValid = false,
                Diagnostics = new List<CodeDiagnostic>
                {
                    new CodeDiagnostic
                    {
                        Id = "CS0000",
                        Severity = "Error",
                        Message = $"Validation error: {ex.Message}",
                        Line = 0,
                        Column = 0
                    }
                }
            };
        }
    }

    public async Task<List<CodeDiagnostic>> GetSemanticDiagnosticsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);

            var compilation = CSharpCompilation.Create(
                assemblyName: "SemanticAnalysis",
                syntaxTrees: new[] { syntaxTree },
                references: DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var diagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

            return await Task.FromResult(diagnostics.Select(MapDiagnostic).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic diagnostics");
            return new List<CodeDiagnostic>
            {
                new CodeDiagnostic
                {
                    Id = "CS0000",
                    Severity = "Error",
                    Message = $"Semantic analysis error: {ex.Message}",
                    Line = 0,
                    Column = 0
                }
            };
        }
    }

    public async Task<string> FormatCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var root = await syntaxTree.GetRootAsync(cancellationToken);

            var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace, cancellationToken: cancellationToken);

            return formattedRoot.ToFullString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting code");
            return code; // Return original code if formatting fails
        }
    }

    private static CodeDiagnostic MapDiagnostic(Diagnostic diagnostic)
    {
        var lineSpan = diagnostic.Location.GetLineSpan();

        return new CodeDiagnostic
        {
            Id = diagnostic.Id,
            Severity = diagnostic.Severity.ToString(),
            Message = diagnostic.GetMessage(),
            Line = lineSpan.StartLinePosition.Line + 1,
            Column = lineSpan.StartLinePosition.Character + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1,
            EndColumn = lineSpan.EndLinePosition.Character + 1,
            HelpLink = diagnostic.Descriptor.HelpLinkUri
        };
    }

    private static OutputKind GetOutputKind(string? kind)
    {
        return kind?.ToLower() switch
        {
            "consoleapplication" => OutputKind.ConsoleApplication,
            "windowsapplication" => OutputKind.WindowsApplication,
            "dll" or "library" or "dynamicallylinkedlibrary" => OutputKind.DynamicallyLinkedLibrary,
            _ => OutputKind.DynamicallyLinkedLibrary
        };
    }

    private static MetadataReference[] GetDefaultReferences()
    {
        var assemblies = new[]
        {
            typeof(object).Assembly,                    // System.Private.CoreLib
            typeof(Console).Assembly,                   // System.Console
            typeof(IEnumerable<>).Assembly,            // System.Collections
            typeof(System.Linq.Enumerable).Assembly,   // System.Linq
            typeof(System.Text.StringBuilder).Assembly, // System.Text
        };

        return assemblies
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray();
    }
}
