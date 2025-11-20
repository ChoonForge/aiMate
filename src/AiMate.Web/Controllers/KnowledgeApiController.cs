using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

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

    [HttpGet]
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

    [HttpGet("analytics")]
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

    [HttpGet("{id}")]
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

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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
