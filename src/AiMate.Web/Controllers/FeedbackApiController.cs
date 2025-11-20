using AiMate.Core.Entities;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing message feedback and ratings
/// </summary>
[ApiController]
[Route("api/v1/feedback")]
[Authorize] // Requires authentication
public class FeedbackApiController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackApiController> _logger;

    public FeedbackApiController(
        IFeedbackService feedbackService,
        ILogger<FeedbackApiController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    /// <summary>
    /// Create or update feedback for a message
    /// </summary>
    [HttpPost("messages/{messageId}")]
    public async Task<IActionResult> CreateOrUpdateFeedback(
        Guid messageId,
        [FromBody] CreateFeedbackRequest request)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Enable authentication and use HttpContext.User.FindFirst("sub")?.Value
            // Add [Authorize] attribute and extract userId from JWT claims
            // For development/testing, accepts userId from request body
            var userId = request.UserId;

            var tags = request.Tags?.Select(t => new FeedbackTag
            {
                Key = t.Key,
                Value = t.Value,
                Color = t.Color,
                Sentiment = t.Sentiment
            }).ToList() ?? [];

            var feedback = await _feedbackService.CreateOrUpdateFeedbackAsync(
                messageId,
                userId,
                request.Rating,
                request.TextFeedback,
                tags,
                request.ModelId,
                request.ResponseTimeMs
            );

            _logger.LogInformation("Feedback created/updated for message {MessageId}", messageId);
            return Ok(new { feedback.Id, feedback.Rating, feedback.CreatedAt, feedback.UpdatedAt });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create feedback for message {MessageId}", messageId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get feedback for a specific message
    /// </summary>
    [HttpGet("messages/{messageId}")]
    public async Task<IActionResult> GetFeedbackByMessage(Guid messageId)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackByMessageIdAsync(messageId);
            if (feedback == null)
            {
                return NotFound(new { error = "Feedback not found" });
            }

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get feedback for message {MessageId}", messageId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get feedback by user
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetFeedbackByUser(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbackByUserIdAsync(userId, skip, take);
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get feedback for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get feedback by model
    /// </summary>
    [HttpGet("models/{modelId}")]
    public async Task<IActionResult> GetFeedbackByModel(
        string modelId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int? minRating = null)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbackByModelIdAsync(
                modelId,
                fromDate,
                toDate,
                minRating
            );

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get feedback for model {ModelId}", modelId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete feedback
    /// </summary>
    [HttpDelete("{feedbackId}")]
    public async Task<IActionResult> DeleteFeedback(Guid feedbackId)
    {
        try
        {
            var success = await _feedbackService.DeleteFeedbackAsync(feedbackId);
            if (!success)
            {
                return NotFound(new { error = "Feedback not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete feedback {FeedbackId}", feedbackId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all active feedback tag templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetActiveTagTemplates()
    {
        try
        {
            var templates = await _feedbackService.GetActiveTagTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tag templates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new tag template (admin)
    /// </summary>
    [HttpPost("templates")]
    public async Task<IActionResult> CreateTagTemplate([FromBody] CreateTagTemplateRequest request)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Add [Authorize(Roles = "Admin")] attribute to this method
            // Or check HttpContext.User.IsInRole("Admin") and return Forbid() if unauthorized

            var options = request.Options.Select(o => new FeedbackTagOption
            {
                Value = o.Value,
                Color = o.Color,
                Sentiment = o.Sentiment,
                Icon = o.Icon,
                DisplayOrder = o.DisplayOrder
            }).ToList();

            var template = await _feedbackService.CreateTagTemplateAsync(
                request.Category,
                request.Label,
                request.Description,
                options,
                request.IsRequired
            );

            return CreatedAtAction(nameof(GetActiveTagTemplates), new { id = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tag template");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update a tag template (admin)
    /// </summary>
    [HttpPut("templates/{templateId}")]
    public async Task<IActionResult> UpdateTagTemplate(
        Guid templateId,
        [FromBody] UpdateTagTemplateRequest request)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Add [Authorize(Roles = "Admin")] attribute to this method
            // Or check HttpContext.User.IsInRole("Admin") and return Forbid() if unauthorized

            var template = await _feedbackService.UpdateTagTemplateAsync(
                templateId,
                request.Category,
                request.Label,
                request.Description,
                request.IsActive,
                request.IsRequired,
                request.DisplayOrder
            );

            return Ok(template);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tag template {TemplateId}", templateId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a tag template (admin)
    /// </summary>
    [HttpDelete("templates/{templateId}")]
    public async Task<IActionResult> DeleteTagTemplate(Guid templateId)
    {
        try
        {
            // IMPLEMENTATION NEEDED: Add [Authorize(Roles = "Admin")] attribute to this method
            // Or check HttpContext.User.IsInRole("Admin") and return Forbid() if unauthorized

            var success = await _feedbackService.DeleteTagTemplateAsync(templateId);
            if (!success)
            {
                return NotFound(new { error = "Tag template not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete tag template {TemplateId}", templateId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get model statistics
    /// </summary>
    [HttpGet("stats/models/{modelId}")]
    public async Task<IActionResult> GetModelStats(
        string modelId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var stats = await _feedbackService.GetModelStatsAsync(modelId, fromDate, toDate);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats for model {ModelId}", modelId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get tag frequency statistics
    /// </summary>
    [HttpGet("stats/tags")]
    public async Task<IActionResult> GetTagFrequency(
        [FromQuery] string? modelId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var frequency = await _feedbackService.GetTagFrequencyAsync(modelId, fromDate, toDate);
            return Ok(frequency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tag frequency");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

// DTOs for API requests
public class CreateFeedbackRequest
{
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? TextFeedback { get; set; }
    public List<FeedbackTagDto>? Tags { get; set; }
    public string? ModelId { get; set; }
    public long? ResponseTimeMs { get; set; }
}

public class FeedbackTagDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Color { get; set; }
    public TagSentiment Sentiment { get; set; }
}

public class CreateTagTemplateRequest
{
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public List<TagOptionDto> Options { get; set; } = [];
}

public class TagOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string? Color { get; set; }
    public TagSentiment Sentiment { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateTagTemplateRequest
{
    public string? Category { get; set; }
    public string? Label { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
}
