using AiMate.Core.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Service for providing IntelliSense features using Roslyn
/// </summary>
public class IntelliSenseService : IIntelliSenseService
{
    private readonly ILogger<IntelliSenseService> _logger;
    private static readonly MetadataReference[] DefaultReferences = GetDefaultReferences();

    public IntelliSenseService(ILogger<IntelliSenseService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Core.Services.CompletionItem>> GetCompletionsAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var compilation = CSharpCompilation.Create(
                "IntelliSense",
                new[] { syntaxTree },
                DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var position = GetPosition(code, line, column);

            var root = await syntaxTree.GetRootAsync(cancellationToken);
            var node = root.FindToken(position).Parent;

            if (node == null)
                return new List<Core.Services.CompletionItem>();

            var workspace = new AdhocWorkspace();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(
                semanticModel,
                position,
                workspace,
                cancellationToken: cancellationToken
            );

            var completions = symbols
                .Select(symbol => new Core.Services.CompletionItem
                {
                    Label = symbol.Name,
                    Kind = GetSymbolKind(symbol),
                    Detail = symbol.ToDisplayString(),
                    Documentation = GetDocumentation(symbol),
                    InsertText = symbol.Name,
                    SortOrder = GetSortOrder(symbol)
                })
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Label)
                .Take(100) // Limit to top 100 completions
                .ToList();

            _logger.LogInformation("Found {Count} completions at line {Line}, column {Column}",
                completions.Count, line, column);

            return completions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completions");
            return new List<Core.Services.CompletionItem>();
        }
    }

    public async Task<SignatureHelp?> GetSignatureHelpAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var compilation = CSharpCompilation.Create(
                "SignatureHelp",
                new[] { syntaxTree },
                DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var position = GetPosition(code, line, column);

            var root = await syntaxTree.GetRootAsync(cancellationToken);

            // Find the argument list containing the position
            var argumentList = root.FindToken(position).Parent?
                .AncestorsAndSelf()
                .OfType<ArgumentListSyntax>()
                .FirstOrDefault();

            if (argumentList == null)
                return null;

            var invocation = argumentList.Parent as InvocationExpressionSyntax;
            if (invocation == null)
                return null;

            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return null;

            var signatures = new List<SignatureInformation>
            {
                new SignatureInformation
                {
                    Label = methodSymbol.ToDisplayString(),
                    Documentation = GetDocumentation(methodSymbol),
                    Parameters = methodSymbol.Parameters.Select(p => new ParameterInformation
                    {
                        Label = p.Name,
                        Documentation = GetParameterDocumentation(p)
                    }).ToList()
                }
            };

            // Calculate active parameter
            var activeParameter = argumentList.Arguments
                .TakeWhile(arg => arg.Span.End <= position)
                .Count();

            return new SignatureHelp
            {
                Signatures = signatures,
                ActiveSignature = 0,
                ActiveParameter = activeParameter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature help");
            return null;
        }
    }

    public async Task<HoverInfo?> GetHoverInfoAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var compilation = CSharpCompilation.Create(
                "HoverInfo",
                new[] { syntaxTree },
                DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var position = GetPosition(code, line, column);

            var root = await syntaxTree.GetRootAsync(cancellationToken);
            var node = root.FindToken(position).Parent;

            if (node == null)
                return null;

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return null;

            var lineSpan = node.GetLocation().GetLineSpan();

            return new HoverInfo
            {
                Content = $"**{symbol.Kind}**: {symbol.ToDisplayString()}\n\n{GetDocumentation(symbol)}",
                Location = new Core.Services.Location
                {
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    EndLine = lineSpan.EndLinePosition.Line + 1,
                    EndColumn = lineSpan.EndLinePosition.Character + 1
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hover info");
            return null;
        }
    }

    public async Task<List<Core.Services.Location>> GetDefinitionAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var compilation = CSharpCompilation.Create(
                "GoToDefinition",
                new[] { syntaxTree },
                DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var position = GetPosition(code, line, column);

            var root = await syntaxTree.GetRootAsync(cancellationToken);
            var node = root.FindToken(position).Parent;

            if (node == null)
                return new List<Core.Services.Location>();

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (symbol == null || symbol.Locations.IsEmpty)
                return new List<Core.Services.Location>();

            return symbol.Locations
                .Where(loc => loc.IsInSource)
                .Select(loc =>
                {
                    var lineSpan = loc.GetLineSpan();
                    return new Core.Services.Location
                    {
                        FilePath = lineSpan.Path,
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        EndLine = lineSpan.EndLinePosition.Line + 1,
                        EndColumn = lineSpan.EndLinePosition.Character + 1
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting definition");
            return new List<Core.Services.Location>();
        }
    }

    public async Task<List<Core.Services.Location>> GetReferencesAsync(
        string code,
        int line,
        int column,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            var compilation = CSharpCompilation.Create(
                "FindReferences",
                new[] { syntaxTree },
                DefaultReferences
            );

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var position = GetPosition(code, line, column);

            var root = await syntaxTree.GetRootAsync(cancellationToken);
            var node = root.FindToken(position).Parent;

            if (node == null)
                return new List<Core.Services.Location>();

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
                return new List<Core.Services.Location>();

            var references = new List<Core.Services.Location>();

            // Find all nodes in the tree
            var allNodes = root.DescendantNodes();

            foreach (var descendantNode in allNodes)
            {
                var descendantSymbol = semanticModel.GetSymbolInfo(descendantNode).Symbol;
                if (SymbolEqualityComparer.Default.Equals(descendantSymbol, symbol))
                {
                    var lineSpan = descendantNode.GetLocation().GetLineSpan();
                    references.Add(new Core.Services.Location
                    {
                        FilePath = lineSpan.Path,
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        EndLine = lineSpan.EndLinePosition.Line + 1,
                        EndColumn = lineSpan.EndLinePosition.Character + 1
                    });
                }
            }

            return references;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting references");
            return new List<Core.Services.Location>();
        }
    }

    private static int GetPosition(string code, int line, int column)
    {
        var sourceText = SourceText.From(code);
        var linePosition = new LinePosition(line - 1, column - 1);
        return sourceText.Lines.GetPosition(linePosition);
    }

    private static string GetSymbolKind(ISymbol symbol)
    {
        return symbol.Kind switch
        {
            SymbolKind.Method => "Method",
            SymbolKind.Property => "Property",
            SymbolKind.Field => "Field",
            SymbolKind.Local => "Variable",
            SymbolKind.Parameter => "Parameter",
            SymbolKind.NamedType => "Class",
            SymbolKind.Namespace => "Namespace",
            SymbolKind.Event => "Event",
            _ => "Unknown"
        };
    }

    private static string GetDocumentation(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(xml))
            return string.Empty;

        // Simple extraction of summary tag
        var summaryStart = xml.IndexOf("<summary>");
        var summaryEnd = xml.IndexOf("</summary>");

        if (summaryStart >= 0 && summaryEnd > summaryStart)
        {
            var summary = xml.Substring(summaryStart + 9, summaryEnd - summaryStart - 9);
            return summary.Trim();
        }

        return string.Empty;
    }

    private static string GetParameterDocumentation(IParameterSymbol parameter)
    {
        return $"{parameter.Type.ToDisplayString()} {parameter.Name}";
    }

    private static int GetSortOrder(ISymbol symbol)
    {
        return symbol.Kind switch
        {
            SymbolKind.Local => 1,
            SymbolKind.Parameter => 2,
            SymbolKind.Property => 3,
            SymbolKind.Method => 4,
            SymbolKind.Field => 5,
            SymbolKind.Event => 6,
            SymbolKind.NamedType => 7,
            SymbolKind.Namespace => 8,
            _ => 9
        };
    }

    private static MetadataReference[] GetDefaultReferences()
    {
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(IEnumerable<>).Assembly,
            typeof(System.Linq.Enumerable).Assembly,
        };

        return assemblies
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray();
    }
}
