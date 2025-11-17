using AiMate.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// Knowledge API for Developer tier - semantic search and knowledge base access
/// </summary>
[ApiController]
[Route("api/v1/knowledge")]
public class KnowledgeApiController : ControllerBase
{
    private readonly IKnowledgeGraphService _knowledgeService;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<KnowledgeApiController> _logger;

    public KnowledgeApiController(
        IKnowledgeGraphService knowledgeService,
        IApiKeyService apiKeyService,
        ILogger<KnowledgeApiController> logger)
    {
        _knowledgeService = knowledgeService;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    /// <summary>
    /// Search knowledge base with semantic similarity
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] KnowledgeSearchRequest request)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Knowledge search for query: {Query}", request.Query);

            var results = await _knowledgeService.SearchAsync(
                request.WorkspaceId,
                request.Query,
                request.Limit,
                request.Tags);

            return Ok(new
            {
                query = request.Query,
                results = results.Select(r => new
                {
                    id = r.Id,
                    title = r.Title,
                    content = r.Content,
                    similarity = r.Similarity,
                    tags = r.Tags,
                    createdAt = r.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge search failed");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Add knowledge item to workspace
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddKnowledgeItem([FromBody] AddKnowledgeItemRequest request)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            _logger.LogInformation("Adding knowledge item to workspace {WorkspaceId}", request.WorkspaceId);

            var item = await _knowledgeService.AddKnowledgeAsync(
                request.WorkspaceId,
                request.Title,
                request.Content,
                request.Tags);

            return CreatedAtAction(
                nameof(GetKnowledgeItem),
                new { id = item.Id },
                new
                {
                    id = item.Id,
                    title = item.Title,
                    content = item.Content,
                    tags = item.Tags,
                    createdAt = item.CreatedAt
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add knowledge item");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get knowledge item by ID
    /// </summary>
    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetKnowledgeItem(Guid id)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = await _apiKeyService.ValidateApiKeyAsync(apiKey);

        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid API key" });
        }

        try
        {
            // IMPLEMENTATION NEEDED: Add GetByIdAsync to IKnowledgeGraphService
            // Method signature: Task<KnowledgeItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken)
            // Implementation: _context.KnowledgeItems.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId)
            // Then: var item = await _knowledgeService.GetByIdAsync(id, userId.Value);
            _logger.LogInformation("Fetching knowledge item {Id}", id);

            return StatusCode(501, new { error = "GetByIdAsync not yet implemented in IKnowledgeGraphService" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch knowledge item");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class KnowledgeSearchRequest
{
    public Guid WorkspaceId { get; set; }
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
    public List<string>? Tags { get; set; }
}

public class AddKnowledgeItemRequest
{
    public Guid WorkspaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
