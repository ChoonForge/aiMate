using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for general user feedback and automated error logging (alpha testing)
/// </summary>
[ApiController]
[Route("api/v1")]
public class FeedbackSystemApiController : ControllerBase
{
    private readonly IUserFeedbackService _feedbackService;
    private readonly IErrorLoggingService _errorService;
    private readonly ILogger<FeedbackSystemApiController> _logger;

    public FeedbackSystemApiController(
        IUserFeedbackService feedbackService,
        IErrorLoggingService errorService,
        ILogger<FeedbackSystemApiController> logger)
    {
        _feedbackService = feedbackService;
        _errorService = errorService;
        _logger = logger;
    }

    #region User Feedback

    /// <summary>
    /// Submit general user feedback
    /// </summary>
    /// <param name="request">Feedback details</param>
    /// <returns>Created feedback</returns>
    /// <response code="201">Feedback created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <remarks>
    /// Submit feedback about bugs, features, UX, performance, or general comments.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/system-feedback
    ///     {
    ///         "feedbackType": "Bug",
    ///         "subject": "Chat not loading",
    ///         "message": "When I click on a conversation, it shows a spinner forever",
    ///         "rating": 2,
    ///         "currentPage": "/chat/workspace-123"
    ///     }
    ///
    /// </remarks>
    [HttpPost("system-feedback")]
    [Authorize]
    [ProducesResponseType(typeof(UserFeedback), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackRequest request)
    {
        var userId = GetUserId();

        var feedback = new UserFeedback
        {
            UserId = userId,
            UserEmail = request.Email,
            FeedbackType = request.FeedbackType,
            Subject = request.Subject,
            Message = request.Message,
            Rating = request.Rating,
            CurrentPage = request.CurrentPage,
            UserAgent = Request.Headers.UserAgent.ToString(),
            ScreenResolution = request.ScreenResolution
        };

        var created = await _feedbackService.CreateFeedbackAsync(feedback);

        _logger.LogInformation("User {UserId} submitted {Type} feedback: {Subject}",
            userId, request.FeedbackType, request.Subject);

        return CreatedAtAction(nameof(GetFeedback), new { id = created.Id }, created);
    }

    /// <summary>
    /// Get user's feedback (or all if admin)
    /// </summary>
    /// <returns>List of feedback</returns>
    /// <response code="200">Returns feedback list</response>
    [HttpGet("system-feedback")]
    [Authorize]
    [ProducesResponseType(typeof(List<UserFeedback>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeedback()
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        var feedback = await _feedbackService.GetUserFeedbackAsync(userId, isAdmin);

        return Ok(feedback);
    }

    /// <summary>
    /// Get feedback with filtering and pagination (admin only)
    /// </summary>
    /// <param name="type">Filter by feedback type</param>
    /// <param name="status">Filter by status</param>
    /// <param name="fromDate">Filter from date</param>
    /// <param name="toDate">Filter to date</param>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take (max 100)</param>
    /// <returns>Paginated feedback list</returns>
    /// <response code="200">Returns paginated feedback</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet("system-feedback/filter")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PaginatedFeedbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFeedbackFiltered(
        [FromQuery] FeedbackType? type = null,
        [FromQuery] FeedbackStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        take = Math.Min(take, 100); // Limit max page size

        var (items, totalCount) = await _feedbackService.GetFeedbackAsync(
            type, status, fromDate, toDate, skip, take);

        var response = new PaginatedFeedbackResponse
        {
            Items = items,
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };

        return Ok(response);
    }

    /// <summary>
    /// Get specific feedback by ID
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <returns>Feedback details</returns>
    /// <response code="200">Returns feedback</response>
    /// <response code="404">Feedback not found</response>
    [HttpGet("system-feedback/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserFeedback), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeedback(Guid id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

        if (feedback == null)
            return NotFound();

        // Non-admins can only see their own feedback
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        if (!isAdmin && feedback.UserId != userId)
            return Forbid();

        return Ok(feedback);
    }

    /// <summary>
    /// Update feedback status (admin only)
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated feedback</returns>
    /// <response code="200">Feedback updated</response>
    /// <response code="404">Feedback not found</response>
    [HttpPut("system-feedback/{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserFeedback), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeedbackStatus(Guid id, [FromBody] UpdateFeedbackStatusRequest request)
    {
        try
        {
            var updated = await _feedbackService.UpdateFeedbackStatusAsync(
                id, request.Status, request.AdminNotes);

            _logger.LogInformation("Admin updated feedback {Id} to status {Status}", id, request.Status);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Assign feedback to admin (admin only)
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <param name="request">Assignment request</param>
    /// <returns>Updated feedback</returns>
    /// <response code="200">Feedback assigned</response>
    /// <response code="404">Feedback not found</response>
    [HttpPut("system-feedback/{id}/assign")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserFeedback), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignFeedback(Guid id, [FromBody] AssignFeedbackRequest request)
    {
        try
        {
            var updated = await _feedbackService.AssignFeedbackAsync(id, request.AssignedToUserId);

            _logger.LogInformation("Feedback {Id} assigned to user {UserId}", id, request.AssignedToUserId);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete feedback
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <response code="204">Feedback deleted</response>
    /// <response code="404">Feedback not found</response>
    [HttpDelete("system-feedback/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeedback(Guid id)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

        if (feedback == null)
            return NotFound();

        // Non-admins can only delete their own feedback
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        if (!isAdmin && feedback.UserId != userId)
            return Forbid();

        await _feedbackService.DeleteFeedbackAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Get feedback statistics (admin only)
    /// </summary>
    /// <returns>Feedback statistics</returns>
    /// <response code="200">Returns statistics</response>
    [HttpGet("system-feedback/stats")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(FeedbackStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeedbackStatistics()
    {
        var stats = await _feedbackService.GetStatisticsAsync();
        return Ok(stats);
    }

    #endregion

    #region Error Logging

    /// <summary>
    /// Log frontend error (public with rate limiting)
    /// </summary>
    /// <param name="request">Error details</param>
    /// <returns>Logged error</returns>
    /// <response code="201">Error logged successfully</response>
    /// <response code="400">Invalid request</response>
    /// <remarks>
    /// Automatically log frontend errors for debugging. Duplicates within 24 hours are grouped.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/errors/log
    ///     {
    ///         "errorType": "JavaScript",
    ///         "message": "Cannot read property 'map' of undefined",
    ///         "stackTrace": "at ChatComponent.render (Chat.razor:45)...",
    ///         "componentName": "ChatComponent",
    ///         "url": "https://app.aimate.co.nz/chat",
    ///         "severity": "High"
    ///     }
    ///
    /// </remarks>
    [HttpPost("errors/log")]
    [AllowAnonymous] // Allow pre-auth errors, but implement rate limiting
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("error-logging")]
    [ProducesResponseType(typeof(ErrorLog), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> LogError([FromBody] LogErrorRequest request)
    {
        // Get userId if authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = GetUserId();
        }

        var error = new ErrorLog
        {
            UserId = userId,
            ErrorType = request.ErrorType,
            Message = request.Message,
            StackTrace = request.StackTrace,
            ComponentName = request.ComponentName,
            Url = request.Url,
            UserAgent = Request.Headers.UserAgent.ToString(),
            BrowserInfo = request.BrowserInfo,
            AdditionalDataJson = request.AdditionalDataJson,
            Severity = request.Severity
        };

        var logged = await _errorService.LogErrorAsync(error);

        _logger.LogWarning("Frontend error logged: {ErrorType} - {Message} (Count: {Count})",
            request.ErrorType, request.Message, logged.OccurrenceCount);

        return CreatedAtAction(nameof(GetError), new { id = logged.Id }, logged);
    }

    /// <summary>
    /// Get error logs (admin only)
    /// </summary>
    /// <param name="type">Filter by error type</param>
    /// <param name="severity">Filter by severity</param>
    /// <param name="isResolved">Filter by resolution status</param>
    /// <param name="fromDate">Filter from date</param>
    /// <param name="toDate">Filter to date</param>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take (max 100)</param>
    /// <returns>Paginated error list</returns>
    /// <response code="200">Returns errors</response>
    [HttpGet("errors")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PaginatedErrorResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrors(
        [FromQuery] ErrorType? type = null,
        [FromQuery] ErrorSeverity? severity = null,
        [FromQuery] bool? isResolved = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        take = Math.Min(take, 100); // Limit max page size

        var (items, totalCount) = await _errorService.GetErrorsAsync(
            type, severity, isResolved, fromDate, toDate, skip, take);

        var response = new PaginatedErrorResponse
        {
            Items = items,
            TotalCount = totalCount,
            Skip = skip,
            Take = take
        };

        return Ok(response);
    }

    /// <summary>
    /// Get specific error by ID (admin only)
    /// </summary>
    /// <param name="id">Error ID</param>
    /// <returns>Error details</returns>
    /// <response code="200">Returns error</response>
    /// <response code="404">Error not found</response>
    [HttpGet("errors/{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ErrorLog), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetError(Guid id)
    {
        var error = await _errorService.GetErrorByIdAsync(id);

        if (error == null)
            return NotFound();

        return Ok(error);
    }

    /// <summary>
    /// Mark error as resolved (admin only)
    /// </summary>
    /// <param name="id">Error ID</param>
    /// <param name="request">Resolution details</param>
    /// <returns>Updated error</returns>
    /// <response code="200">Error resolved</response>
    /// <response code="404">Error not found</response>
    [HttpPut("errors/{id}/resolve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ErrorLog), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveError(Guid id, [FromBody] ResolveErrorRequest request)
    {
        try
        {
            var resolved = await _errorService.ResolveErrorAsync(id, request.Resolution);

            _logger.LogInformation("Error {Id} marked as resolved: {Resolution}", id, request.Resolution);

            return Ok(resolved);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get error statistics (admin only)
    /// </summary>
    /// <returns>Error statistics</returns>
    /// <response code="200">Returns statistics</response>
    [HttpGet("errors/stats")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ErrorStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrorStatistics()
    {
        var stats = await _errorService.GetStatisticsAsync();
        return Ok(stats);
    }

    #endregion

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    #endregion
}

#region Request/Response Models

public class SubmitFeedbackRequest
{
    public string? Email { get; set; }
    public FeedbackType FeedbackType { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public int? Rating { get; set; }
    public string? CurrentPage { get; set; }
    public string? ScreenResolution { get; set; }
}

public class UpdateFeedbackStatusRequest
{
    public FeedbackStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

public class AssignFeedbackRequest
{
    public Guid AssignedToUserId { get; set; }
}

public class PaginatedFeedbackResponse
{
    public List<UserFeedback> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class LogErrorRequest
{
    public ErrorType ErrorType { get; set; }
    public required string Message { get; set; }
    public string? StackTrace { get; set; }
    public string? ComponentName { get; set; }
    public string? Url { get; set; }
    public string? BrowserInfo { get; set; }
    public string? AdditionalDataJson { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Medium;
}

public class ResolveErrorRequest
{
    public string? Resolution { get; set; }
}

public class PaginatedErrorResponse
{
    public List<ErrorLog> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

#endregion
