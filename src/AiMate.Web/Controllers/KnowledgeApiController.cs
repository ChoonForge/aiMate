using AiMate.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly ILogger<KnowledgeController> _logger;
    private static readonly List<KnowledgeArticleDto> _articles = new();

    public KnowledgeController(ILogger<KnowledgeController> logger)
    {
        _logger = logger;
        if (!_articles.Any()) InitializeSampleData();
    }

    [HttpGet]
    public ActionResult<List<KnowledgeArticleDto>> GetArticles([FromQuery] string userId)
    {
        var articles = _articles
            .Where(a => a.OwnerId == userId || a.Visibility == "Public")
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.UpdatedAt)
            .ToList();
        return Ok(articles);
    }

    [HttpGet("analytics")]
    public ActionResult<KnowledgeAnalyticsDto> GetAnalytics([FromQuery] string userId)
    {
        var userArticles = _articles.Where(a => a.OwnerId == userId || a.Visibility == "Public").ToList();
        var analytics = new KnowledgeAnalyticsDto
        {
            TotalArticles = userArticles.Count,
            TotalViews = userArticles.Sum(a => a.ViewCount),
            TotalReferences = userArticles.Sum(a => a.ReferenceCount),
            MostViewed = userArticles.OrderByDescending(a => a.ViewCount).Take(5).ToList(),
            MostReferenced = userArticles.OrderByDescending(a => a.ReferenceCount).Take(5).ToList(),
            RecentlyAdded = userArticles.OrderByDescending(a => a.CreatedAt).Take(5).ToList(),
            TagCounts = userArticles.SelectMany(a => a.Tags).GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count()),
            TypeCounts = userArticles.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.Count()),
            CategoryCounts = userArticles.Where(a => !string.IsNullOrEmpty(a.Category)).GroupBy(a => a.Category!).ToDictionary(g => g.Key, g => g.Count())
        };
        return Ok(analytics);
    }

    [HttpGet("{id}")]
    public ActionResult<KnowledgeArticleDto> GetArticle(string id, [FromQuery] string userId)
    {
        var article = _articles.FirstOrDefault(a => a.Id == id);
        if (article == null) return NotFound();
        if (article.OwnerId != userId && article.Visibility == "Private") return Forbid();

        // Update view count manually (KnowledgeArticleDto is a class, not a record)
        article.ViewCount++;
        article.LastViewedAt = DateTime.UtcNow;
        return Ok(article);
    }

    [HttpPost]
    public ActionResult<KnowledgeArticleDto> CreateArticle([FromBody] CreateKnowledgeArticleRequest request, [FromQuery] string userId)
    {
        var article = new KnowledgeArticleDto
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Content = request.Content,
            ContentType = request.ContentType,
            Summary = request.Summary,
            Type = request.Type,
            Tags = request.Tags ?? new(),
            Collection = request.Collection,
            Category = request.Category,
            Source = request.Source,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };
        _articles.Add(article);
        return CreatedAtAction(nameof(GetArticle), new { id = article.Id, userId }, article);
    }

    [HttpPut("{id}")]
    public ActionResult<KnowledgeArticleDto> UpdateArticle(string id, [FromBody] UpdateKnowledgeArticleRequest request, [FromQuery] string userId)
    {
        var article = _articles.FirstOrDefault(a => a.Id == id);
        if (article == null) return NotFound();
        if (article.OwnerId != userId) return Forbid();

        // Update fields manually (KnowledgeArticleDto is a class, not a record)
        article.Title = request.Title ?? article.Title;
        article.Content = request.Content ?? article.Content;
        article.Summary = request.Summary ?? article.Summary;
        article.Tags = request.Tags ?? article.Tags;
        article.IsFeatured = request.IsFeatured ?? article.IsFeatured;
        article.IsVerified = request.IsVerified ?? article.IsVerified;
        article.UpdatedAt = DateTime.UtcNow;

        return Ok(article);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteArticle(string id, [FromQuery] string userId)
    {
        var article = _articles.FirstOrDefault(a => a.Id == id);
        if (article == null) return NotFound();
        if (article.OwnerId != userId) return Forbid();
        _articles.Remove(article);
        return NoContent();
    }

    private void InitializeSampleData()
    {
        _articles.AddRange(new[]
        {
            new KnowledgeArticleDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Getting Started with Blazor",
                Content = "# Blazor Basics\n\nBlazor is a framework for building interactive web UIs...",
                Summary = "Learn the fundamentals of Blazor development",
                Type = "Tutorial",
                Tags = new List<string> { "blazor", "csharp", "web" },
                Collection = "Development",
                Category = "Web Development",
                OwnerId = "user-1",
                Visibility = "Public",
                IsFeatured = true,
                IsVerified = true,
                ViewCount = 245,
                ReferenceCount = 12,
                UpvoteCount = 34,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new KnowledgeArticleDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "C# Best Practices",
                Content = "# Coding Standards\n\n1. Use meaningful names...",
                Summary = "Essential coding standards for C# development",
                Type = "Reference",
                Tags = new List<string> { "csharp", "best-practices" },
                Collection = "Development",
                OwnerId = "user-1",
                Visibility = "Public",
                IsVerified = true,
                ViewCount = 189,
                ReferenceCount = 28,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            }
        });
    }
}
