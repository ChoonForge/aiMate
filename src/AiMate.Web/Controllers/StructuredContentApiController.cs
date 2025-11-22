using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing structured content (templates, rendering, actions)
/// </summary>
[ApiController]
[Route("api/v1/structured-content")]
[Authorize]
public class StructuredContentApiController : ControllerBase
{
    private readonly IStructuredContentService _contentService;
    private readonly ILogger<StructuredContentApiController> _logger;

    public StructuredContentApiController(
        IStructuredContentService contentService,
        ILogger<StructuredContentApiController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    // ============================================================================
    // Template Management
    // ============================================================================

    /// <summary>
    /// Get all templates
    /// </summary>
    /// <param name="type">Filter by content type</param>
    /// <param name="includePrivate">Include private templates</param>
    /// <param name="userId">User ID for permission filtering</param>
    /// <returns>List of templates</returns>
    /// <response code="200">Returns list of templates</response>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<StructuredContentTemplate>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] StructuredContentType? type = null,
        [FromQuery] bool includePrivate = false,
        [FromQuery] string? userId = null)
    {
        try
        {
            Guid? userGuid = null;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed))
            {
                userGuid = parsed;
            }

            var templates = await _contentService.GetTemplatesAsync(type, includePrivate, userGuid);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            return StatusCode(500, new { error = "Failed to get templates", details = ex.Message });
        }
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Template details</returns>
    /// <response code="200">Returns the template</response>
    /// <response code="404">Template not found</response>
    [HttpGet("templates/{id}")]
    [ProducesResponseType(typeof(StructuredContentTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var templateGuid))
            {
                return BadRequest("Invalid template ID");
            }

            var template = await _contentService.GetTemplateByIdAsync(templateGuid);
            if (template == null)
            {
                return NotFound($"Template {id} not found");
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateId}", id);
            return StatusCode(500, new { error = "Failed to get template", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    /// <param name="request">Template creation request</param>
    /// <returns>Created template</returns>
    /// <response code="201">Template created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost("templates")]
    [Authorize(Policy = "CanAddCustomEndpoints")] // Developer tier required
    [ProducesResponseType(typeof(StructuredContentTemplate), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        try
        {
            var template = new StructuredContentTemplate
            {
                Name = request.Name,
                Type = request.Type,
                SchemaJson = request.SchemaJson,
                ComponentPath = request.ComponentPath,
                StylesJson = request.StylesJson,
                Description = request.Description,
                IsPublic = request.IsPublic,
                Tags = request.Tags ?? new List<string>(),
                CreatedBy = request.CreatedBy
            };

            var created = await _contentService.CreateTemplateAsync(template);

            _logger.LogInformation("Created template {TemplateName} ({TemplateId})", created.Name, created.Id);

            return CreatedAtAction(nameof(GetTemplate), new { id = created.Id.ToString() }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, new { error = "Failed to create template", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Template update request</param>
    /// <returns>Updated template</returns>
    /// <response code="200">Template updated successfully</response>
    /// <response code="404">Template not found</response>
    [HttpPut("templates/{id}")]
    [Authorize(Policy = "CanAddCustomEndpoints")]
    [ProducesResponseType(typeof(StructuredContentTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(string id, [FromBody] UpdateTemplateRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var templateGuid))
            {
                return BadRequest("Invalid template ID");
            }

            var template = await _contentService.GetTemplateByIdAsync(templateGuid);
            if (template == null)
            {
                return NotFound($"Template {id} not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Name))
                template.Name = request.Name;
            if (request.SchemaJson != null)
                template.SchemaJson = request.SchemaJson;
            if (request.ComponentPath != null)
                template.ComponentPath = request.ComponentPath;
            if (request.StylesJson != null)
                template.StylesJson = request.StylesJson;
            if (request.Description != null)
                template.Description = request.Description;
            if (request.IsPublic.HasValue)
                template.IsPublic = request.IsPublic.Value;
            if (request.Tags != null)
                template.Tags = request.Tags;

            var updated = await _contentService.UpdateTemplateAsync(template);

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, new { error = "Failed to update template", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Template deleted successfully</response>
    /// <response code="404">Template not found</response>
    [HttpDelete("templates/{id}")]
    [Authorize(Policy = "CanAddCustomEndpoints")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var templateGuid))
            {
                return BadRequest("Invalid template ID");
            }

            await _contentService.DeleteTemplateAsync(templateGuid);

            _logger.LogInformation("Deleted template {TemplateId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, new { error = "Failed to delete template", details = ex.Message });
        }
    }

    // ============================================================================
    // Content Operations
    // ============================================================================

    /// <summary>
    /// Parse structured content from text/JSON
    /// </summary>
    /// <param name="request">Parse request</param>
    /// <returns>Parsed structured content</returns>
    /// <response code="200">Returns parsed content</response>
    /// <response code="400">Invalid content or parsing failed</response>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(StructuredContent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ParseContent([FromBody] ParseContentRequest request)
    {
        try
        {
            var content = await _contentService.ParseContentAsync(request.Content);
            if (content == null)
            {
                return BadRequest("No structured content found in input");
            }

            return Ok(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing content");
            return StatusCode(500, new { error = "Failed to parse content", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate structured content
    /// </summary>
    /// <param name="content">Content to validate</param>
    /// <returns>Validation result</returns>
    /// <response code="200">Returns validation result</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ContentValidationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateContent([FromBody] StructuredContent content)
    {
        try
        {
            var result = await _contentService.ValidateContentAsync(content);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating content");
            return StatusCode(500, new { error = "Failed to validate content", details = ex.Message });
        }
    }

    /// <summary>
    /// Render structured content with template
    /// </summary>
    /// <param name="request">Render request</param>
    /// <returns>Rendered content ready for UI</returns>
    /// <response code="200">Returns rendered content</response>
    [HttpPost("render")]
    [ProducesResponseType(typeof(RenderedContent), StatusCodes.Status200OK)]
    public async Task<IActionResult> RenderContent([FromBody] RenderContentRequest request)
    {
        try
        {
            var rendered = await _contentService.RenderContentAsync(request.Content, request.TemplateName);
            return Ok(rendered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering content");
            return StatusCode(500, new { error = "Failed to render content", details = ex.Message });
        }
    }

    // ============================================================================
    // Action Execution
    // ============================================================================

    /// <summary>
    /// Execute an action
    /// </summary>
    /// <param name="request">Action execution request</param>
    /// <returns>Action result</returns>
    /// <response code="200">Returns action result</response>
    [HttpPost("actions/execute")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteAction([FromBody] ExecuteActionRequest request)
    {
        try
        {
            var context = new ActionContext
            {
                ActionId = request.ActionId,
                HandlerType = request.HandlerType,
                Config = request.Config ?? new Dictionary<string, object>(),
                Parameters = request.Parameters ?? new Dictionary<string, object>(),
                UserId = request.UserId,
                ItemId = request.ItemId,
                SelectedItems = request.SelectedItems,
                Metadata = request.Metadata ?? new Dictionary<string, object>()
            };

            var result = await _contentService.ExecuteActionAsync(context);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action");
            return StatusCode(500, new { error = "Failed to execute action", details = ex.Message });
        }
    }

    // ============================================================================
    // Export
    // ============================================================================

    /// <summary>
    /// Export structured content to file
    /// </summary>
    /// <param name="request">Export request</param>
    /// <returns>Exported file</returns>
    /// <response code="200">Returns exported file</response>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportContent([FromBody] ExportContentRequest request)
    {
        try
        {
            var result = await _contentService.ExportAsync(request.Content, request.Format);

            if (!result.Success || result.Data == null)
            {
                return BadRequest(new { error = result.Error });
            }

            return File(result.Data, result.ContentType ?? "application/octet-stream", result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting content");
            return StatusCode(500, new { error = "Failed to export content", details = ex.Message });
        }
    }
}

// ============================================================================
// Request/Response Models
// ============================================================================

public class CreateTemplateRequest
{
    public required string Name { get; set; }
    public StructuredContentType Type { get; set; }
    public required string SchemaJson { get; set; }
    public string? ComponentPath { get; set; }
    public string? StylesJson { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public List<string>? Tags { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class UpdateTemplateRequest
{
    public string? Name { get; set; }
    public string? SchemaJson { get; set; }
    public string? ComponentPath { get; set; }
    public string? StylesJson { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
    public List<string>? Tags { get; set; }
}

public class ParseContentRequest
{
    public required string Content { get; set; }
}

public class RenderContentRequest
{
    public required StructuredContent Content { get; set; }
    public string? TemplateName { get; set; }
}

public class ExecuteActionRequest
{
    public required string ActionId { get; set; }
    public ActionHandlerType HandlerType { get; set; }
    public Dictionary<string, object>? Config { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public Guid UserId { get; set; }
    public string? ItemId { get; set; }
    public List<string>? SelectedItems { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ExportContentRequest
{
    public required StructuredContent Content { get; set; }
    public ExportFormat Format { get; set; }
}
