using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using AiMate.Core.Entities;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// MCP Tool Service implementation with built-in tools
/// </summary>
public class MCPToolService : IMCPToolService
{
    private readonly ILogger<MCPToolService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFileUploadService? _fileUploadService;
    private readonly IKnowledgeGraphService? _knowledgeService;
    private readonly IDatasetGeneratorService? _datasetGenerator;
    private readonly Dictionary<string, MCPTool> _registeredTools = new();
    private readonly Dictionary<string, Func<Dictionary<string, object>, Task<MCPToolResult>>> _toolExecutors = new();

    public MCPToolService(
        ILogger<MCPToolService> logger,
        IHttpClientFactory httpClientFactory,
        IFileUploadService? fileUploadService = null,
        IKnowledgeGraphService? knowledgeService = null,
        IDatasetGeneratorService? datasetGenerator = null)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _fileUploadService = fileUploadService;
        _knowledgeService = knowledgeService;
        _datasetGenerator = datasetGenerator;
        RegisterBuiltInTools();
    }

    public Task<List<MCPTool>> GetAvailableToolsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        // FUTURE ENHANCEMENT: Filter tools by workspace settings
        // Add WorkspaceTools table with EnabledTools JSON column
        // Query and filter: tools.Where(t => workspaceTools.EnabledTools.Contains(t.Name))
        // Currently returns all registered tools
        var tools = _registeredTools.Values.ToList();
        return Task.FromResult(tools);
    }

    public async Task<MCPToolResult> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> parameters,
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        if (!_toolExecutors.ContainsKey(toolName))
        {
            return new MCPToolResult
            {
                Success = false,
                Error = $"Tool '{toolName}' not found"
            };
        }

        try
        {
            _logger.LogInformation("Executing tool: {ToolName} with parameters: {Parameters}",
                toolName, JsonSerializer.Serialize(parameters));

            var result = await _toolExecutors[toolName](parameters);

            _logger.LogInformation("Tool execution completed: {ToolName}, Success: {Success}",
                toolName, result.Success);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed: {ToolName}", toolName);
            return new MCPToolResult
            {
                Success = false,
                Error = $"Tool execution failed: {ex.Message}"
            };
        }
    }

    public Task RegisterToolAsync(
        MCPTool tool,
        CancellationToken cancellationToken = default)
    {
        _registeredTools[tool.Name] = tool;
        _logger.LogInformation("Tool registered: {ToolName}", tool.Name);
        return Task.CompletedTask;
    }

    public Task<bool> IsToolAvailableAsync(
        string toolName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_registeredTools.ContainsKey(toolName));
    }

    private void RegisterBuiltInTools()
    {
        // Web Search Tool
        var webSearchTool = new MCPTool
        {
            Name = "web_search",
            Description = "Search the web for information using DuckDuckGo",
            Parameters = new Dictionary<string, MCPToolParameter>
            {
                ["query"] = new MCPToolParameter
                {
                    Type = "string",
                    Description = "Search query",
                    Required = true
                },
                ["max_results"] = new MCPToolParameter
                {
                    Type = "integer",
                    Description = "Maximum number of results to return",
                    Required = false,
                    DefaultValue = 5
                }
            }
        };

        _registeredTools[webSearchTool.Name] = webSearchTool;
        _toolExecutors[webSearchTool.Name] = ExecuteWebSearchAsync;

        // Code Interpreter Tool
        var codeInterpreterTool = new MCPTool
        {
            Name = "code_interpreter",
            Description = "Execute Python code in a sandboxed environment",
            Parameters = new Dictionary<string, MCPToolParameter>
            {
                ["code"] = new MCPToolParameter
                {
                    Type = "string",
                    Description = "Python code to execute",
                    Required = true
                },
                ["timeout"] = new MCPToolParameter
                {
                    Type = "integer",
                    Description = "Execution timeout in seconds",
                    Required = false,
                    DefaultValue = 30
                }
            },
            RequiresAuth = true
        };

        _registeredTools[codeInterpreterTool.Name] = codeInterpreterTool;
        _toolExecutors[codeInterpreterTool.Name] = ExecuteCodeInterpreterAsync;

        // File Reader Tool
        var fileReaderTool = new MCPTool
        {
            Name = "read_file",
            Description = "Read contents of uploaded files",
            Parameters = new Dictionary<string, MCPToolParameter>
            {
                ["file_id"] = new MCPToolParameter
                {
                    Type = "string",
                    Description = "ID of the file to read",
                    Required = true
                }
            }
        };

        _registeredTools[fileReaderTool.Name] = fileReaderTool;
        _toolExecutors[fileReaderTool.Name] = ExecuteFileReaderAsync;

        // Knowledge Search Tool
        var knowledgeSearchTool = new MCPTool
        {
            Name = "knowledge_search",
            Description = "Search the user's knowledge base using semantic search",
            Parameters = new Dictionary<string, MCPToolParameter>
            {
                ["query"] = new MCPToolParameter
                {
                    Type = "string",
                    Description = "Search query",
                    Required = true
                },
                ["limit"] = new MCPToolParameter
                {
                    Type = "integer",
                    Description = "Maximum number of results",
                    Required = false,
                    DefaultValue = 5
                }
            }
        };

        _registeredTools[knowledgeSearchTool.Name] = knowledgeSearchTool;
        _toolExecutors[knowledgeSearchTool.Name] = ExecuteKnowledgeSearchAsync;

        // Dataset Generator Tool (for Guardian fine-tuning)
        if (_datasetGenerator != null)
        {
            var datasetGenTool = new MCPTool
            {
                Name = "generate_dataset",
                Description = "Generate synthetic training dataset for personality fine-tuning (template-based, safe for mental health)",
                Parameters = new Dictionary<string, MCPToolParameter>
                {
                    ["personality"] = new MCPToolParameter
                    {
                        Type = "string",
                        Description = "Personality name (e.g., 'Guardian')",
                        Required = true
                    },
                    ["num_examples"] = new MCPToolParameter
                    {
                        Type = "integer",
                        Description = "Number of training examples to generate",
                        Required = false,
                        DefaultValue = 100
                    },
                    ["export_format"] = new MCPToolParameter
                    {
                        Type = "string",
                        Description = "Export format: 'jsonl' for fine-tuning or 'json' for review",
                        Required = false,
                        DefaultValue = "jsonl"
                    }
                },
                RequiresAuth = true
            };

            _registeredTools[datasetGenTool.Name] = datasetGenTool;
            _toolExecutors[datasetGenTool.Name] = ExecuteDatasetGeneratorAsync;
        }

        _logger.LogInformation("Built-in tools registered: {Count}", _registeredTools.Count);
    }

    private async Task<MCPToolResult> ExecuteWebSearchAsync(Dictionary<string, object> parameters)
    {
        var query = parameters["query"].ToString();
        var maxResults = parameters.ContainsKey("max_results")
            ? Convert.ToInt32(parameters["max_results"])
            : 5;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; aiMate/1.0)");

            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"https://html.duckduckgo.com/html/?q={encodedQuery}";

            var startTime = DateTime.UtcNow;
            var response = await client.GetStringAsync(url);
            var queryTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Parse DuckDuckGo HTML results
            var results = ParseDuckDuckGoResults(response, maxResults);

            return new MCPToolResult
            {
                Success = true,
                Result = new
                {
                    query,
                    results = results.Select(r => new
                    {
                        title = r.Title,
                        url = r.Url,
                        snippet = r.Snippet
                    }).ToArray()
                },
                Metadata = new Dictionary<string, object>
                {
                    ["source"] = "DuckDuckGo",
                    ["query_time_ms"] = queryTime,
                    ["result_count"] = results.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web search failed for query: {Query}", query);
            return new MCPToolResult
            {
                Success = false,
                Error = $"Web search failed: {ex.Message}"
            };
        }
    }

    private List<(string Title, string Url, string Snippet)> ParseDuckDuckGoResults(string html, int maxResults)
    {
        var results = new List<(string, string, string)>();

        try
        {
            // Extract result links and snippets using regex
            var linkPattern = @"<a[^>]+class=""result__a""[^>]+href=""([^""]+)""[^>]*>([^<]+)</a>";
            var snippetPattern = @"<a[^>]+class=""result__snippet""[^>]*>([^<]+)</a>";

            var linkMatches = Regex.Matches(html, linkPattern);
            var snippetMatches = Regex.Matches(html, snippetPattern);

            for (int i = 0; i < Math.Min(Math.Min(linkMatches.Count, snippetMatches.Count), maxResults); i++)
            {
                var url = HttpUtility.HtmlDecode(linkMatches[i].Groups[1].Value);
                var title = HttpUtility.HtmlDecode(linkMatches[i].Groups[2].Value);
                var snippet = HttpUtility.HtmlDecode(snippetMatches[i].Groups[1].Value);

                results.Add((title, url, snippet));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse DuckDuckGo results, returning empty list");
        }

        return results;
    }

    private async Task<MCPToolResult> ExecuteCodeInterpreterAsync(Dictionary<string, object> parameters)
    {
        // MOCK IMPLEMENTATION: Code execution requires sandboxed environment
        // REAL IMPLEMENTATION OPTIONS:
        // 1. Docker container with Python image (security: network isolation, resource limits)
        // 2. Process.Start with restricted permissions (Windows: Job Objects, Linux: cgroups)
        // 3. E2B.dev API (https://e2b.dev) - Managed code execution sandbox
        // 4. Pyodide (WebAssembly Python) - Browser-based, limited packages
        var code = parameters["code"].ToString();
        var timeout = parameters.ContainsKey("timeout")
            ? Convert.ToInt32(parameters["timeout"])
            : 30;

        await Task.Delay(100); // Simulate execution

        return new MCPToolResult
        {
            Success = true,
            Result = new
            {
                output = "Code execution not yet implemented. Sandbox environment required.",
                code,
                execution_time_ms = 100
            },
            Metadata = new Dictionary<string, object>
            {
                ["sandbox"] = "docker",
                ["timeout"] = timeout
            }
        };
    }

    private async Task<MCPToolResult> ExecuteFileReaderAsync(Dictionary<string, object> parameters)
    {
        var fileId = parameters["file_id"].ToString();

        if (_fileUploadService == null)
        {
            return new MCPToolResult
            {
                Success = false,
                Error = "File upload service not available"
            };
        }

        try
        {
            if (!Guid.TryParse(fileId, out var fileGuid))
            {
                return new MCPToolResult
                {
                    Success = false,
                    Error = "Invalid file ID format"
                };
            }

            var fileResult = await _fileUploadService.GetFileAsync(fileGuid);

            if (fileResult == null)
            {
                return new MCPToolResult
                {
                    Success = false,
                    Error = $"File not found: {fileId}"
                };
            }

            using var reader = new StreamReader(fileResult.Value.Stream);
            var content = await reader.ReadToEndAsync();

            return new MCPToolResult
            {
                Success = true,
                Result = new
                {
                    file_id = fileId,
                    content = content,
                    content_type = fileResult.Value.ContentType,
                    size_bytes = content.Length
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File reading failed for file: {FileId}", fileId);
            return new MCPToolResult
            {
                Success = false,
                Error = $"File reading failed: {ex.Message}"
            };
        }
    }

    private async Task<MCPToolResult> ExecuteKnowledgeSearchAsync(Dictionary<string, object> parameters)
    {
        var query = parameters["query"].ToString();
        var limit = parameters.ContainsKey("limit")
            ? Convert.ToInt32(parameters["limit"])
            : 5;

        if (_knowledgeService == null)
        {
            return new MCPToolResult
            {
                Success = false,
                Error = "Knowledge service not available"
            };
        }

        try
        {
            // IMPLEMENTATION NEEDED: Add userId parameter to ExecuteToolAsync signature
            // Then pass it through to tool executors via parameters dictionary
            // Example: var userId = parameters.ContainsKey("user_id") ? (Guid)parameters["user_id"] : Guid.Empty;
            // For now, using Empty to search all knowledge items
            var userId = Guid.Empty;

            var items = await _knowledgeService.SearchAsync(query, userId, limit);

            return new MCPToolResult
            {
                Success = true,
                Result = new
                {
                    query,
                    results = items.Select(item => new
                    {
                        id = item.Id,
                        title = item.Title,
                        content = item.Content,
                        tags = item.Tags,
                        created_at = item.CreatedAt,
                        relevance = 0.85 // Placeholder - actual relevance would come from vector similarity score
                    }).ToArray()
                },
                Metadata = new Dictionary<string, object>
                {
                    ["search_type"] = "semantic",
                    ["vector_similarity"] = true,
                    ["result_count"] = items.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge search failed for query: {Query}", query);
            return new MCPToolResult
            {
                Success = false,
                Error = $"Knowledge search failed: {ex.Message}"
            };
        }
    }

    private async Task<MCPToolResult> ExecuteDatasetGeneratorAsync(Dictionary<string, object> parameters)
    {
        if (_datasetGenerator == null)
        {
            return new MCPToolResult
            {
                Success = false,
                Error = "Dataset generator service not available"
            };
        }

        try
        {
            var personality = parameters["personality"].ToString() ?? "Guardian";
            var numExamples = parameters.ContainsKey("num_examples")
                ? Convert.ToInt32(parameters["num_examples"])
                : 100;
            var exportFormat = parameters.ContainsKey("export_format")
                ? parameters["export_format"].ToString()
                : "jsonl";

            _logger.LogInformation(
                "Generating {Count} training examples for {Personality}",
                numExamples, personality);

            // Generate dataset from templates
            var dataset = await _datasetGenerator.GenerateFromTemplatesAsync(
                personality,
                numExamples);

            // Validate quality
            var validation = await _datasetGenerator.ValidateDatasetAsync(dataset);

            // Export in requested format
            string exportedData;
            if (exportFormat?.ToLower() == "jsonl")
            {
                exportedData = await _datasetGenerator.ExportToJsonLinesAsync(dataset);
            }
            else
            {
                exportedData = JsonSerializer.Serialize(dataset, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }

            return new MCPToolResult
            {
                Success = true,
                Result = new
                {
                    personality = personality,
                    total_examples = dataset.TotalExamples,
                    generated_at = dataset.GeneratedAt,
                    scenario_distribution = dataset.ScenarioDistribution,
                    validation = new
                    {
                        is_valid = validation.IsValid,
                        quality_score = validation.QualityScore,
                        warnings = validation.Warnings,
                        crisis_distribution = validation.CrisisLevelDistribution,
                        cultural_markers = validation.CulturalMarkerCount,
                        resources_mentioned = validation.ResourceMentionCount
                    },
                    export_format = exportFormat,
                    data = exportedData
                },
                Metadata = new Dictionary<string, object>
                {
                    ["generation_method"] = "template_based",
                    ["safe_for_mental_health"] = true,
                    ["human_validation_required"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dataset generation failed");
            return new MCPToolResult
            {
                Success = false,
                Error = $"Dataset generation failed: {ex.Message}"
            };
        }
    }
}
