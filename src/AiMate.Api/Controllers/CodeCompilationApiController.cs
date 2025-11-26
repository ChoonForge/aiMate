using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for compiling and validating C# code using Roslyn
/// </summary>
[ApiController]
[Route("api/v1/code/compile")]
[Authorize(Policy = "CanAddCustomEndpoints")] // Requires Developer tier
public class CodeCompilationApiController : ControllerBase
{
    private readonly IRoslynCompilationService _compilationService;
    private readonly IIntelliSenseService _intelliSenseService;
    private readonly ILogger<CodeCompilationApiController> _logger;

    public CodeCompilationApiController(
        IRoslynCompilationService compilationService,
        IIntelliSenseService intelliSenseService,
        ILogger<CodeCompilationApiController> logger)
    {
        _compilationService = compilationService;
        _intelliSenseService = intelliSenseService;
        _logger = logger;
    }

    /// <summary>
    /// Compile C# code and return diagnostics
    /// </summary>
    /// <param name="request">Compilation request with code and options</param>
    /// <returns>Compilation result with errors, warnings, and assembly bytes</returns>
    /// <response code="200">Returns compilation result</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/code/compile
    ///     {
    ///         "code": "using System;\n\nnamespace MyApp\n{\n    public class Program\n    {\n        public static void Main()\n        {\n            Console.WriteLine(\"Hello World!\");\n        }\n    }\n}",
    ///         "options": {
    ///             "assemblyName": "MyApp",
    ///             "outputKind": "ConsoleApplication",
    ///             "optimizeCode": false
    ///         }
    ///     }
    ///
    /// Returns diagnostics (errors/warnings) and compiled assembly bytes if successful.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CompilationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Compile([FromBody] CompileRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            _logger.LogInformation("Compiling code (length: {Length})", request.Code.Length);

            var result = await _compilationService.CompileAsync(request.Code, request.Options);

            _logger.LogInformation(
                "Compilation completed - Success: {Success}, Errors: {Errors}, Warnings: {Warnings}, Time: {Time}ms",
                result.Success, result.Errors.Count, result.Warnings.Count, result.CompilationTime.TotalMilliseconds
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling code");
            return StatusCode(500, new { error = "Failed to compile code", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate C# code syntax without full compilation
    /// </summary>
    /// <param name="request">Validation request with code</param>
    /// <returns>Validation result with syntax diagnostics</returns>
    /// <response code="200">Returns validation result</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Faster than full compilation - only checks syntax, not semantics.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/code/compile/validate
    ///     {
    ///         "code": "public class MyClass { }"
    ///     }
    ///
    /// </remarks>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateSyntax([FromBody] ValidateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var result = await _compilationService.ValidateSyntaxAsync(request.Code);

            _logger.LogInformation(
                "Validation completed - Valid: {Valid}, Diagnostics: {Count}",
                result.IsValid, result.Diagnostics.Count
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code");
            return StatusCode(500, new { error = "Failed to validate code", details = ex.Message });
        }
    }

    /// <summary>
    /// Get semantic diagnostics for code (type errors, unused variables, etc.)
    /// </summary>
    /// <param name="request">Semantic analysis request</param>
    /// <returns>List of semantic diagnostics</returns>
    /// <response code="200">Returns semantic diagnostics</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("diagnostics")]
    [ProducesResponseType(typeof(List<CodeDiagnostic>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSemanticDiagnostics([FromBody] ValidateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var diagnostics = await _compilationService.GetSemanticDiagnosticsAsync(request.Code);

            _logger.LogInformation("Found {Count} semantic diagnostics", diagnostics.Count);

            return Ok(diagnostics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic diagnostics");
            return StatusCode(500, new { error = "Failed to get diagnostics", details = ex.Message });
        }
    }

    /// <summary>
    /// Format C# code
    /// </summary>
    /// <param name="request">Format request with code</param>
    /// <returns>Formatted code</returns>
    /// <response code="200">Returns formatted code</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Applies standard C# formatting rules to the code.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/code/compile/format
    ///     {
    ///         "code": "public class MyClass{public void Method(){Console.WriteLine(\"test\");}}"
    ///     }
    ///
    /// </remarks>
    [HttpPost("format")]
    [ProducesResponseType(typeof(FormatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FormatCode([FromBody] ValidateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var formatted = await _compilationService.FormatCodeAsync(request.Code);

            return Ok(new FormatResponse { FormattedCode = formatted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting code");
            return StatusCode(500, new { error = "Failed to format code", details = ex.Message });
        }
    }

    /// <summary>
    /// Get code completions at a specific position (IntelliSense)
    /// </summary>
    /// <param name="request">Completion request with code and cursor position</param>
    /// <returns>List of completion items</returns>
    /// <response code="200">Returns completion items</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Provides IntelliSense completions at the specified cursor position.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/code/compile/completions
    ///     {
    ///         "code": "public class MyClass\n{\n    public void Method()\n    {\n        Console.\n    }\n}",
    ///         "line": 5,
    ///         "column": 16
    ///     }
    ///
    /// Returns available completions after "Console."
    /// </remarks>
    [HttpPost("completions")]
    [ProducesResponseType(typeof(List<CompletionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompletions([FromBody] CompletionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var completions = await _intelliSenseService.GetCompletionsAsync(
                request.Code,
                request.Line,
                request.Column
            );

            _logger.LogInformation("Found {Count} completions at line {Line}, column {Column}",
                completions.Count, request.Line, request.Column);

            return Ok(completions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completions");
            return StatusCode(500, new { error = "Failed to get completions", details = ex.Message });
        }
    }

    /// <summary>
    /// Get signature help for a method call
    /// </summary>
    /// <param name="request">Signature help request</param>
    /// <returns>Signature information</returns>
    /// <response code="200">Returns signature help</response>
    /// <response code="204">No signature help available at this position</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("signature-help")]
    [ProducesResponseType(typeof(SignatureHelp), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSignatureHelp([FromBody] CompletionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var signatureHelp = await _intelliSenseService.GetSignatureHelpAsync(
                request.Code,
                request.Line,
                request.Column
            );

            if (signatureHelp == null)
            {
                return NoContent();
            }

            return Ok(signatureHelp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature help");
            return StatusCode(500, new { error = "Failed to get signature help", details = ex.Message });
        }
    }

    /// <summary>
    /// Get hover information for a symbol
    /// </summary>
    /// <param name="request">Hover request</param>
    /// <returns>Hover information</returns>
    /// <response code="200">Returns hover information</response>
    /// <response code="204">No hover information available</response>
    /// <response code="400">Invalid request</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("hover")]
    [ProducesResponseType(typeof(HoverInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHoverInfo([FromBody] CompletionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("Code is required");
            }

            var hoverInfo = await _intelliSenseService.GetHoverInfoAsync(
                request.Code,
                request.Line,
                request.Column
            );

            if (hoverInfo == null)
            {
                return NoContent();
            }

            return Ok(hoverInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hover info");
            return StatusCode(500, new { error = "Failed to get hover info", details = ex.Message });
        }
    }
}

/// <summary>
/// Request to compile code
/// </summary>
public class CompileRequest
{
    public required string Code { get; set; }
    public CompilationOptions? Options { get; set; }
}

/// <summary>
/// Request to validate code
/// </summary>
public class ValidateRequest
{
    public required string Code { get; set; }
}

/// <summary>
/// Response from code formatting
/// </summary>
public class FormatResponse
{
    public string FormattedCode { get; set; } = string.Empty;
}

/// <summary>
/// Request for code completions
/// </summary>
public class CompletionRequest
{
    public required string Code { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}
