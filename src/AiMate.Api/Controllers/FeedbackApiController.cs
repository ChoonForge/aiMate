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
    /// <param name="messageId">Message ID to provide feedback for</param>
    /// <param name="request">Feedback data including rating, text feedback, and tags</param>
    /// <returns>Created or updated feedback summary</returns>
    /// <response code="200">Feedback created or updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("messages/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="messageId">Message ID to retrieve feedback for</param>
    /// <returns>Feedback details</returns>
    /// <response code="200">Returns the feedback</response>
    /// <response code="404">Feedback not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("messages/{messageId}")]
    [ProducesResponseType(typeof(MessageFeedback), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get feedback submitted by a specific user
    /// </summary>
    /// <param name="userId">User ID to retrieve feedback for</param>
    /// <param name="skip">Number of records to skip (pagination)</param>
    /// <param name="take">Number of records to take (max 100)</param>
    /// <returns>List of feedback entries</returns>
    /// <response code="200">Returns list of user feedback</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(List<MessageFeedback>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get feedback for a specific AI model with optional filtering
    /// </summary>
    /// <param name="modelId">Model ID (e.g., "gpt-4", "claude-3-opus")</param>
    /// <param name="fromDate">Start date for filtering (optional)</param>
    /// <param name="toDate">End date for filtering (optional)</param>
    /// <param name="minRating">Minimum rating filter (1-5, optional)</param>
    /// <returns>List of feedback entries for the model</returns>
    /// <response code="200">Returns filtered feedback list</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("models/{modelId}")]
    [ProducesResponseType(typeof(List<MessageFeedback>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Delete a feedback entry
    /// </summary>
    /// <param name="feedbackId">Feedback ID to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Feedback deleted successfully</response>
    /// <response code="404">Feedback not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{feedbackId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <returns>List of active tag templates with options</returns>
    /// <response code="200">Returns list of tag templates</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<FeedbackTagTemplate>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Create a new feedback tag template (admin only)
    /// </summary>
    /// <param name="request">Tag template creation request with category, label, and options</param>
    /// <returns>Created tag template</returns>
    /// <response code="201">Tag template created successfully</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>Requires AdminOnly policy authorization</remarks>
    [HttpPost("templates")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(FeedbackTagTemplate), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTagTemplate([FromBody] CreateTagTemplateRequest request)
    {
        try
        {

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
    /// Update an existing tag template (admin only)
    /// </summary>
    /// <param name="templateId">Template ID to update</param>
    /// <param name="request">Template update request with modified fields</param>
    /// <returns>Updated tag template</returns>
    /// <response code="200">Template updated successfully</response>
    /// <response code="404">Template not found</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>Requires AdminOnly policy authorization</remarks>
    [HttpPut("templates/{templateId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(FeedbackTagTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTagTemplate(
        Guid templateId,
        [FromBody] UpdateTagTemplateRequest request)
    {
        try
        {

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
    /// Delete a tag template (admin only)
    /// </summary>
    /// <param name="templateId">Template ID to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Template deleted successfully</response>
    /// <response code="404">Template not found</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>Requires AdminOnly policy authorization</remarks>
    [HttpDelete("templates/{templateId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTagTemplate(Guid templateId)
    {
        try
        {

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
    /// Get statistical metrics for a specific AI model
    /// </summary>
    /// <param name="modelId">Model ID (e.g., "gpt-4", "claude-3-opus")</param>
    /// <param name="fromDate">Start date for statistics (optional)</param>
    /// <param name="toDate">End date for statistics (optional)</param>
    /// <returns>Model statistics including average rating, response times, feedback counts</returns>
    /// <response code="200">Returns model statistics</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("stats/models/{modelId}")]
    [ProducesResponseType(typeof(ModelFeedbackStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get frequency distribution of feedback tags
    /// </summary>
    /// <param name="modelId">Filter by model ID (optional)</param>
    /// <param name="fromDate">Start date for statistics (optional)</param>
    /// <param name="toDate">End date for statistics (optional)</param>
    /// <returns>Dictionary of tag keys/values and their occurrence counts</returns>
    /// <response code="200">Returns tag frequency statistics</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("stats/tags")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
