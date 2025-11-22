using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing knowledge base articles with RAG support
/// </summary>
[ApiController]
[Route("api/v1/knowledge")]
[Authorize] // Requires authentication
public class KnowledgeApiController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly ILogger<KnowledgeApiController> _logger;

    public KnowledgeApiController(
        IKnowledgeService knowledgeService,
        ILogger<KnowledgeApiController> logger)
    {
        _knowledgeService = knowledgeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all published knowledge articles for a user
    /// </summary>
    /// <param name="userId">User ID to retrieve articles for</param>
    /// <returns>List of published knowledge articles</returns>
    /// <response code="200">Returns list of articles ordered by featured status and update date</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<KnowledgeArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<KnowledgeArticleDto>>> GetArticles([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var items = await _knowledgeService.GetUserKnowledgeItemsAsync(userGuid);

            // Filter to published items and map to DTOs
            var articles = items
                .Where(k => k.IsPublished)
                .Select(MapToDto)
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.UpdatedAt)
                .ToList();

            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting knowledge articles for user {UserId}", userId);
            return StatusCode(500, "Error retrieving knowledge articles");
        }
    }

    /// <summary>
    /// Get knowledge base analytics for a user
    /// </summary>
    /// <param name="userId">User ID to retrieve analytics for</param>
    /// <returns>Analytics including view counts, most viewed/referenced articles, tag statistics</returns>
    /// <response code="200">Returns knowledge base analytics</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(KnowledgeAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KnowledgeAnalyticsDto>> GetAnalytics([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var items = await _knowledgeService.GetUserKnowledgeItemsAsync(userGuid);
            var articleDtos = items.Select(MapToDto).ToList();

            var analytics = new KnowledgeAnalyticsDto
            {
                TotalArticles = articleDtos.Count,
                TotalViews = articleDtos.Sum(a => a.ViewCount),
                TotalReferences = articleDtos.Sum(a => a.ReferenceCount),
                MostViewed = articleDtos.OrderByDescending(a => a.ViewCount).Take(5).ToList(),
                MostReferenced = articleDtos.OrderByDescending(a => a.ReferenceCount).Take(5).ToList(),
                RecentlyAdded = articleDtos.OrderByDescending(a => a.CreatedAt).Take(5).ToList(),
                TagCounts = articleDtos.SelectMany(a => a.Tags).GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count()),
                TypeCounts = articleDtos.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.Count()),
                CategoryCounts = articleDtos.Where(a => !string.IsNullOrEmpty(a.Category)).GroupBy(a => a.Category!).ToDictionary(g => g.Key, g => g.Count())
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting knowledge analytics for user {UserId}", userId);
            return StatusCode(500, "Error retrieving knowledge analytics");
        }
    }

    /// <summary>
    /// Get a specific knowledge article by ID
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="userId">User ID making the request (for permission checks)</param>
    /// <returns>Knowledge article details</returns>
    /// <response code="200">Returns the article and increments view count</response>
    /// <response code="400">Invalid article or user ID format</response>
    /// <response code="403">User does not have permission to view this private article</response>
    /// <response code="404">Article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KnowledgeArticleDto>> GetArticle(string id, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var itemGuid))
            {
                return BadRequest("Invalid article ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var item = await _knowledgeService.GetKnowledgeItemByIdAsync(itemGuid);

            if (item == null)
            {
                return NotFound();
            }

            // Check permissions
            if (item.UserId != userGuid && item.Visibility == "Private")
            {
                return Forbid();
            }

            // Increment view count
            item.ViewCount++;
            item.LastViewedAt = DateTime.UtcNow;
            await _knowledgeService.UpdateKnowledgeItemAsync(item);

            return Ok(MapToDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting knowledge article {ArticleId}", id);
            return StatusCode(500, "Error retrieving knowledge article");
        }
    }

    /// <summary>
    /// Create a new knowledge article
    /// </summary>
    /// <param name="request">Article creation request with title, content, and metadata</param>
    /// <param name="userId">User ID creating the article</param>
    /// <returns>Created article details</returns>
    /// <response code="201">Article created successfully</response>
    /// <response code="400">Invalid request or user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KnowledgeArticleDto>> CreateArticle(
        [FromBody] CreateKnowledgeArticleRequest request,
        [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var item = new KnowledgeItem
            {
                UserId = userGuid,
                Title = request.Title,
                Content = request.Content,
                Summary = request.Summary,
                ContentType = request.ContentType,
                Type = request.Type,
                Tags = request.Tags ?? new List<string>(),
                Collection = request.Collection,
                Category = request.Category,
                Source = request.Source,
                PublishedAt = DateTime.UtcNow
            };

            var created = await _knowledgeService.CreateKnowledgeItemAsync(item);

            _logger.LogInformation("Created knowledge article {ArticleId} for user {UserId}", created.Id, userId);

            return CreatedAtAction(nameof(GetArticle), new { id = created.Id.ToString(), userId }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge article");
            return StatusCode(500, "Error creating knowledge article");
        }
    }

    /// <summary>
    /// Update an existing knowledge article
    /// </summary>
    /// <param name="id">Article ID to update</param>
    /// <param name="request">Article update request with modified fields</param>
    /// <param name="userId">User ID making the update (must be article owner)</param>
    /// <returns>Updated article details</returns>
    /// <response code="200">Article updated successfully</response>
    /// <response code="400">Invalid article or user ID format</response>
    /// <response code="403">User is not the owner of this article</response>
    /// <response code="404">Article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(KnowledgeArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KnowledgeArticleDto>> UpdateArticle(
        string id,
        [FromBody] UpdateKnowledgeArticleRequest request,
        [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var itemGuid))
            {
                return BadRequest("Invalid article ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var item = await _knowledgeService.GetKnowledgeItemByIdAsync(itemGuid);

            if (item == null)
            {
                return NotFound();
            }

            // Check ownership
            if (item.UserId != userGuid)
            {
                return Forbid();
            }

            // Update fields
            item.Title = request.Title ?? item.Title;
            item.Content = request.Content ?? item.Content;
            item.Summary = request.Summary ?? item.Summary;
            item.Tags = request.Tags ?? item.Tags;
            item.IsFeatured = request.IsFeatured ?? item.IsFeatured;
            item.IsVerified = request.IsVerified ?? item.IsVerified;

            var updated = await _knowledgeService.UpdateKnowledgeItemAsync(item);

            _logger.LogInformation("Updated knowledge article {ArticleId}", id);

            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge article {ArticleId}", id);
            return StatusCode(500, "Error updating knowledge article");
        }
    }

    /// <summary>
    /// Delete a knowledge article
    /// </summary>
    /// <param name="id">Article ID to delete</param>
    /// <param name="userId">User ID making the deletion (must be article owner)</param>
    /// <returns>No content</returns>
    /// <response code="204">Article deleted successfully</response>
    /// <response code="400">Invalid article or user ID format</response>
    /// <response code="403">User is not the owner of this article</response>
    /// <response code="404">Article not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteArticle(string id, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var itemGuid))
            {
                return BadRequest("Invalid article ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var item = await _knowledgeService.GetKnowledgeItemByIdAsync(itemGuid);

            if (item == null)
            {
                return NotFound();
            }

            // Check ownership
            if (item.UserId != userGuid)
            {
                return Forbid();
            }

            await _knowledgeService.DeleteKnowledgeItemAsync(itemGuid);

            _logger.LogInformation("Deleted knowledge article {ArticleId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge article {ArticleId}", id);
            return StatusCode(500, "Error deleting knowledge article");
        }
    }

    // Helper method to map KnowledgeItem entity to KnowledgeArticleDto
    private static KnowledgeArticleDto MapToDto(KnowledgeItem item)
    {
        return new KnowledgeArticleDto
        {
            Id = item.Id.ToString(),
            Title = item.Title,
            Content = item.Content,
            ContentType = item.ContentType,
            Summary = item.Summary,
            Type = item.Type,
            Tags = item.Tags,
            Collection = item.Collection,
            Category = item.Category,
            Source = item.Source,
            OwnerId = item.UserId.ToString(),
            Visibility = item.Visibility,
            IsFeatured = item.IsFeatured,
            IsVerified = item.IsVerified,
            IsPublished = item.IsPublished,
            ViewCount = item.ViewCount,
            ReferenceCount = item.ReferenceCount,
            UpvoteCount = item.UpvoteCount,
            DownvoteCount = item.DownvoteCount,
            AverageRating = item.UpvoteCount + item.DownvoteCount > 0
                ? (double)item.UpvoteCount / (item.UpvoteCount + item.DownvoteCount) * 5.0
                : 0.0,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            PublishedAt = item.PublishedAt,
            LastViewedAt = item.LastViewedAt
        };
    }
}
