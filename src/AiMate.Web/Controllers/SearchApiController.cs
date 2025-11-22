using AiMate.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for searching across all content types
/// </summary>
[ApiController]
[Route("api/v1/search")]
[Authorize]
public class SearchApiController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchApiController> _logger;

    public SearchApiController(ISearchService searchService, ILogger<SearchApiController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Search conversations by title or metadata
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
    /// <returns>Search results with relevance scores</returns>
    /// <response code="200">Returns matching conversations</response>
    /// <response code="400">Invalid query or limit</response>
    /// <remarks>
    /// Search your conversations by title. Results are ranked by relevance.
    ///
    /// Example request:
    ///
    ///     GET /api/v1/search/conversations?query=api design&amp;limit=10
    ///
    /// </remarks>
    [HttpGet("conversations")]
    [OutputCache(PolicyName = "search")]
    [ProducesResponseType(typeof(SearchResults<Core.Entities.Conversation>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchConversations(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        var userId = GetUserId();
        var results = await _searchService.SearchConversationsAsync(userId, query, limit, cancellationToken);

        _logger.LogInformation("User {UserId} searched conversations for '{Query}', found {Count} results in {TimeMs}ms",
            userId, query, results.ReturnedCount, results.QueryTimeMs);

        return Ok(results);
    }

    /// <summary>
    /// Search messages by content
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
    /// <returns>Search results with highlights</returns>
    /// <response code="200">Returns matching messages</response>
    /// <response code="400">Invalid query or limit</response>
    /// <remarks>
    /// Search within message content across all your conversations.
    ///
    /// Example request:
    ///
    ///     GET /api/v1/search/messages?query=authentication error&amp;limit=20
    ///
    /// </remarks>
    [HttpGet("messages")]
    [OutputCache(PolicyName = "search")]
    [ProducesResponseType(typeof(SearchResults<Core.Entities.Message>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchMessages(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        var userId = GetUserId();
        var results = await _searchService.SearchMessagesAsync(userId, query, limit, cancellationToken);

        _logger.LogInformation("User {UserId} searched messages for '{Query}', found {Count} results in {TimeMs}ms",
            userId, query, results.ReturnedCount, results.QueryTimeMs);

        return Ok(results);
    }

    /// <summary>
    /// Semantic search in knowledge base using AI embeddings
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="threshold">Similarity threshold (0.0-1.0, default: 0.7)</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
    /// <returns>Semantically similar knowledge items</returns>
    /// <response code="200">Returns similar knowledge items</response>
    /// <response code="400">Invalid parameters</response>
    /// <remarks>
    /// Semantic search uses AI to find knowledge items by meaning, not just keywords.
    /// Returns items ranked by semantic similarity.
    ///
    /// Example request:
    ///
    ///     GET /api/v1/search/knowledge/semantic?query=how to deploy to production&amp;threshold=0.75
    ///
    /// </remarks>
    [HttpGet("knowledge/semantic")]
    [OutputCache(PolicyName = "search")]
    [ProducesResponseType(typeof(SearchResults<Core.Entities.KnowledgeItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchKnowledgeSemantic(
        [FromQuery] string query,
        [FromQuery] double threshold = 0.7,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        if (threshold < 0 || threshold > 1)
            return BadRequest("Threshold must be between 0.0 and 1.0");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        var userId = GetUserId();
        var results = await _searchService.SearchKnowledgeSemanticAsync(
            userId, query, threshold, limit, cancellationToken);

        _logger.LogInformation("User {UserId} performed semantic search for '{Query}', found {Count} results in {TimeMs}ms",
            userId, query, results.ReturnedCount, results.QueryTimeMs);

        return Ok(results);
    }

    /// <summary>
    /// Full-text search in knowledge base
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
    /// <returns>Matching knowledge items</returns>
    /// <response code="200">Returns matching knowledge items</response>
    /// <response code="400">Invalid query or limit</response>
    /// <remarks>
    /// Traditional keyword-based search in knowledge base title, content, and tags.
    ///
    /// Example request:
    ///
    ///     GET /api/v1/search/knowledge?query=deployment&amp;limit=10
    ///
    /// </remarks>
    [HttpGet("knowledge")]
    [OutputCache(PolicyName = "search")]
    [ProducesResponseType(typeof(SearchResults<Core.Entities.KnowledgeItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchKnowledge(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        var userId = GetUserId();
        var results = await _searchService.SearchKnowledgeFullTextAsync(userId, query, limit, cancellationToken);

        _logger.LogInformation("User {UserId} searched knowledge for '{Query}', found {Count} results in {TimeMs}ms",
            userId, query, results.ReturnedCount, results.QueryTimeMs);

        return Ok(results);
    }

    /// <summary>
    /// Global search across all content types
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum results per type (default: 5, max: 20)</param>
    /// <returns>Combined search results from all content types</returns>
    /// <response code="200">Returns results from conversations, messages, and knowledge</response>
    /// <response code="400">Invalid query or limit</response>
    /// <remarks>
    /// Search across conversations, messages, and knowledge base simultaneously.
    /// Returns top results from each type.
    ///
    /// Example request:
    ///
    ///     GET /api/v1/search?query=authentication&amp;limit=5
    ///
    /// Example response:
    ///
    ///     {
    ///         "conversations": [...],
    ///         "messages": [...],
    ///         "knowledgeItems": [...],
    ///         "totalResults": 12,
    ///         "query": "authentication",
    ///         "queryTimeMs": 145
    ///     }
    ///
    /// </remarks>
    [HttpGet]
    [OutputCache(PolicyName = "search")]
    [ProducesResponseType(typeof(GlobalSearchResults), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchGlobal(
        [FromQuery] string query,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        if (limit < 1 || limit > 20)
            return BadRequest("Limit must be between 1 and 20");

        var userId = GetUserId();
        var results = await _searchService.SearchGlobalAsync(userId, query, limit, cancellationToken);

        _logger.LogInformation("User {UserId} performed global search for '{Query}', found {Count} total results in {TimeMs}ms",
            userId, query, results.TotalResults, results.QueryTimeMs);

        return Ok(results);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
