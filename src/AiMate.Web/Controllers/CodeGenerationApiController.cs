//using AiMate.Core.Entities;
//using AiMate.Core.Services;
//using AiMate.Shared.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Text;
//using AiMate.Shared.Models; // Ensure this namespace contains ChatRequest

//namespace AiMate.Web.Controllers;

///// <summary>
///// API for AI-powered code generation
///// </summary>
//[ApiController]
//[Route("api/v1/code/generate")]
//[Authorize(Policy = "CanAddCustomEndpoints")] // Requires Developer tier
//public class CodeGenerationApiController : ControllerBase
//{
//    private readonly ILiteLLMService _llmService;
//    private readonly ICodeFileService _codeFileService;
//    private readonly IProjectService _projectService;
//    private readonly ILogger<CodeGenerationApiController> _logger;

//    public CodeGenerationApiController(
//        ILiteLLMService llmService,
//        ICodeFileService codeFileService,
//        IProjectService projectService,
//        ILogger<CodeGenerationApiController> logger)
//    {
//        _llmService = llmService;
//        _codeFileService = codeFileService;
//        _projectService = projectService;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Generate code based on user intent and project context
//    /// </summary>
//    /// <param name="request">Code generation request</param>
//    /// <returns>Generated code with metadata</returns>
//    /// <response code="200">Returns generated code</response>
//    /// <response code="400">Invalid request</response>
//    /// <response code="404">Project not found</response>
//    /// <response code="500">Internal server error</response>
//    /// <remarks>
//    /// Sample request:
//    ///
//    ///     POST /api/v1/code/generate
//    ///     {
//    ///         "projectId": "project-guid",
//    ///         "intent": "Create a component for file upload with drag and drop support",
//    ///         "targetPath": "src/Components/FileUpload.razor",
//    ///         "language": "razor",
//    ///         "includeContext": true
//    ///     }
//    ///
//    /// The AI will analyze the project structure and generate appropriate code
//    /// that follows the project's conventions and patterns.
//    /// </remarks>
//    [HttpPost]
//    [ProducesResponseType(typeof(CodeGenerationResponse), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> GenerateCode([FromBody] CodeGenerationRequest request)
//    {
//        try
//        {
//            if (!Guid.TryParse(request.ProjectId, out var projectGuid))
//            {
//                return BadRequest("Invalid project ID");
//            }

//            var project = await _projectService.GetProjectByIdAsync(projectGuid);
//            if (project == null)
//            {
//                return NotFound($"Project {request.ProjectId} not found");
//            }

//            _logger.LogInformation("Generating code for project {ProjectId} with intent: {Intent}",
//                request.ProjectId, request.Intent);

//            // Build context from existing project files
//            var contextBuilder = new StringBuilder();
//            if (request.IncludeContext)
//            {
//                var projectFiles = await _codeFileService.GetProjectFilesAsync(projectGuid);
//                var relevantFiles = projectFiles
//                    .Where(f => IsRelevantForContext(f, request.TargetPath))
//                    .Take(5) // Limit to 5 most relevant files
//                    .ToList();

//                if (relevantFiles.Any())
//                {
//                    contextBuilder.AppendLine("Project Context:");
//                    foreach (var file in relevantFiles)
//                    {
//                        contextBuilder.AppendLine($"\n// File: {file.Path}");
//                        contextBuilder.AppendLine($"// Language: {file.Language}");
//                        contextBuilder.AppendLine(file.Content.Length > 1000
//                            ? file.Content.Substring(0, 1000) + "..."
//                            : file.Content);
//                    }
//                }
//            }

//            // Build the code generation prompt
//            var prompt = BuildCodeGenerationPrompt(
//                request.Intent,
//                request.Language,
//                request.TargetPath,
//                contextBuilder.ToString(),
//                request.AdditionalInstructions
//            );

//            // Call LLM to generate code
//            var llmRequest = new ChatRequest
//            {
//                Model = request.Model ?? "gpt-4",
//                Messages = new List<ChatMessage>
//                {
//                    new ChatMessage
//                    {
//                        Role = "system",
//                        Content = "You are an expert software developer. Generate clean, well-structured code following best practices and the project's existing patterns."
//                    },
//                    new ChatMessage
//                    {
//                        Role = "user",
//                        Content = prompt
//                    }
//                }
//            };

//            var llmResponse = await _llmService.GetChatCompletionAsync(llmRequest);

//            var generatedCode = ExtractCodeFromResponse(llmResponse.Choices?[0]?.Message?.Content ?? "");

//            _logger.LogInformation("Successfully generated code for {TargetPath}", request.TargetPath);

//            return Ok(new CodeGenerationResponse
//            {
//                GeneratedCode = generatedCode,
//                TargetPath = request.TargetPath,
//                Language = request.Language,
//                SuggestedFileName = GetSuggestedFileName(request.TargetPath),
//                Explanation = llmResponse.Choices?[0]?.Message?.Content ?? "",
//                TokensUsed = llmResponse.Usage?.TotalTokens ?? 0
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating code");
//            return StatusCode(500, new { error = "Failed to generate code", details = ex.Message });
//        }
//    }

//    /// <summary>
//    /// Generate multiple files for a feature
//    /// </summary>
//    /// <param name="request">Multi-file generation request</param>
//    /// <returns>Generated files</returns>
//    /// <response code="200">Returns generated files</response>
//    /// <response code="400">Invalid request</response>
//    /// <response code="500">Internal server error</response>
//    /// <remarks>
//    /// Sample request:
//    ///
//    ///     POST /api/v1/code/generate/multi-file
//    ///     {
//    ///         "projectId": "project-guid",
//    ///         "featureDescription": "Add user authentication with login and registration pages",
//    ///         "includeContext": true
//    ///     }
//    ///
//    /// The AI will generate multiple related files for the feature.
//    /// </remarks>
//    [HttpPost("multi-file")]
//    [ProducesResponseType(typeof(MultiFileGenerationResponse), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> GenerateMultipleFiles([FromBody] MultiFileGenerationRequest request)
//    {
//        try
//        {
//            if (!Guid.TryParse(request.ProjectId, out var projectGuid))
//            {
//                return BadRequest("Invalid project ID");
//            }

//            var project = await _projectService.GetProjectByIdAsync(projectGuid);
//            if (project == null)
//            {
//                return NotFound($"Project {request.ProjectId} not found");
//            }

//            _logger.LogInformation("Generating multiple files for feature: {Feature}", request.FeatureDescription);

//            // Build prompt for multi-file generation
//            var prompt = $@"Generate all necessary files for the following feature:

//Feature: {request.FeatureDescription}

//Project: {project.Name}

//Please provide:
//1. A list of files to create with their paths
//2. The complete code for each file
//3. A brief explanation of what each file does

//Format your response as JSON with this structure:
//{{
//  ""files"": [
//    {{
//      ""path"": ""path/to/file"",
//      ""language"": ""language-name"",
//      ""content"": ""file content"",
//      ""description"": ""what this file does""
//    }}
//  ]
//}}";

//            var llmRequest = GetLlmRequest(request, prompt);

//            var llmResponse = await _llmService.GetChatCompletionAsync(llmRequest);
//            var responseContent = llmResponse.Choices?[0]?.Message?.Content ?? "";

//            // Parse the JSON response (simplified - production would need better parsing)
//            var generatedFiles = new List<GeneratedFile>
//            {
//                new GeneratedFile
//                {
//                    Path = "placeholder.cs",
//                    Language = "csharp",
//                    Content = responseContent,
//                    Description = "Generated content (requires JSON parsing implementation)"
//                }
//            };

//            return Ok(new MultiFileGenerationResponse
//            {
//                Files = generatedFiles,
//                FeatureDescription = request.FeatureDescription,
//                TokensUsed = llmResponse.Usage?.TotalTokens ?? 0
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error generating multiple files");
//            return StatusCode(500, new { error = "Failed to generate files", details = ex.Message });
//        }
//    }

//    //private static ChatRequest GetLlmRequest(MultiFileGenerationRequest request, string prompt)
//    //{
//    //    return new ChatRequest
//    //    {
//    //        Model = request.Model ?? "gpt-4",
//    //        Messages = new List<ChatMessage>
//    //            {
//    //                new ChatMessage { Role = "system", Content = "You are an expert software developer." },
//    //                new ChatMessage { Role = "user", Content = prompt }
//    //            }
//    //    };
//    //}

//    private static string BuildCodeGenerationPrompt(
//        string intent,
//        string language,
//        string targetPath,
//        string context,
//        string? additionalInstructions)
//    {
//        var prompt = new StringBuilder();
//        prompt.AppendLine($"Generate {language} code for the following requirement:");
//        prompt.AppendLine();
//        prompt.AppendLine($"Requirement: {intent}");
//        prompt.AppendLine($"Target file: {targetPath}");
//        prompt.AppendLine();

//        if (!string.IsNullOrWhiteSpace(context))
//        {
//            prompt.AppendLine(context);
//            prompt.AppendLine();
//        }

//        if (!string.IsNullOrWhiteSpace(additionalInstructions))
//        {
//            prompt.AppendLine($"Additional instructions: {additionalInstructions}");
//            prompt.AppendLine();
//        }

//        prompt.AppendLine("Generate complete, production-ready code following best practices.");
//        prompt.AppendLine("Include necessary using statements, proper error handling, and documentation.");

//        return prompt.ToString();
//    }

//    private static string ExtractCodeFromResponse(string response)
//    {
//        // Extract code from markdown code blocks if present
//        var codeBlockStart = response.IndexOf("```");
//        if (codeBlockStart >= 0)
//        {
//            var firstNewLine = response.IndexOf('\n', codeBlockStart);
//            var codeBlockEnd = response.IndexOf("```", firstNewLine);

//            if (codeBlockEnd > firstNewLine)
//            {
//                return response.Substring(firstNewLine + 1, codeBlockEnd - firstNewLine - 1).Trim();
//            }
//        }

//        return response;
//    }

//    private static bool IsRelevantForContext(CodeFile file, string targetPath)
//    {
//        // Simple relevance check - same directory or similar filename
//        var targetDir = Path.GetDirectoryName(targetPath) ?? "";
//        var fileDir = Path.GetDirectoryName(file.Path) ?? "";

//        return fileDir == targetDir || file.Path.Contains("Component") || file.Path.Contains("Service");
//    }

//    private static string GetSuggestedFileName(string targetPath)
//    {
//        return Path.GetFileName(targetPath);
//    }
//}

///// <summary>
///// Request for code generation
///// </summary>
//public class CodeGenerationRequest
//{
//    public required string ProjectId { get; set; }
//    public required string Intent { get; set; }
//    public required string TargetPath { get; set; }
//    public required string Language { get; set; }
//    public bool IncludeContext { get; set; } = true;
//    public string? Model { get; set; }
//    public string? AdditionalInstructions { get; set; }
//}

///// <summary>
///// Response from code generation
///// </summary>
//public class CodeGenerationResponse
//{
//    public string GeneratedCode { get; set; } = string.Empty;
//    public string TargetPath { get; set; } = string.Empty;
//    public string Language { get; set; } = string.Empty;
//    public string SuggestedFileName { get; set; } = string.Empty;
//    public string Explanation { get; set; } = string.Empty;
//    public int TokensUsed { get; set; }
//}

///// <summary>
///// Request for multi-file generation
///// </summary>
//public class MultiFileGenerationRequest
//{
//    public required string ProjectId { get; set; }
//    public required string FeatureDescription { get; set; }
//    public bool IncludeContext { get; set; } = true;
//    public string? Model { get; set; }
//}

///// <summary>
///// Response from multi-file generation
///// </summary>
//public class MultiFileGenerationResponse
//{
//    public List<GeneratedFile> Files { get; set; } = new();
//    public string FeatureDescription { get; set; } = string.Empty;
//    public int TokensUsed { get; set; }
//}

///// <summary>
///// A generated file
///// </summary>
//public class GeneratedFile
//{
//    public string Path { get; set; } = string.Empty;
//    public string Language { get; set; } = string.Empty;
//    public string Content { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//}
