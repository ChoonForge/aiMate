using AiMate.Infrastructure.Data;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiMate.Web.Controllers;

/// <summary>
/// Usage Analytics API for tracking user consumption and costs
/// </summary>
[ApiController]
[Route("api/v1/users/usage")]
[Authorize] // Requires authentication
public class UsageApiController : ControllerBase
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<UsageApiController> _logger;

    public UsageApiController(
        AiMateDbContext context,
        ILogger<UsageApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get user usage analytics for current billing period
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsage([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            _logger.LogInformation("Fetching usage analytics for user {UserId}", userId);

            // Calculate current billing period (start of month to end of month)
            var now = DateTime.UtcNow;
            var billingPeriodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var billingPeriodEnd = billingPeriodStart.AddMonths(1).AddSeconds(-1);

            // Get all user's workspaces
            var workspaceIds = await _context.Workspaces
                .Where(w => w.UserId == userGuid)
                .Select(w => w.Id)
                .ToListAsync();

            if (workspaceIds.Count == 0)
            {
                // No workspaces, return empty usage
                return Ok(new UserUsageDto
                {
                    TotalMessages = 0,
                    TotalTokens = 0,
                    TotalCost = 0,
                    BillingPeriodStart = billingPeriodStart.ToString("MMM d, yyyy"),
                    BillingPeriodEnd = billingPeriodEnd.ToString("MMM d, yyyy"),
                    UsageByModel = new List<UsageByModelDto>()
                });
            }

            // Get all messages from user's conversations in current billing period
            // Only count assistant messages (AI responses) that have token/cost data
            var messagesQuery = _context.Messages
                .Include(m => m.Conversation)
                .Where(m => workspaceIds.Contains(m.Conversation!.WorkspaceId))
                .Where(m => m.CreatedAt >= billingPeriodStart && m.CreatedAt <= billingPeriodEnd)
                .Where(m => m.Role == Core.Enums.MessageRole.Assistant)
                .Where(m => m.TokensUsed != null || m.Cost != null);

            var messages = await messagesQuery.ToListAsync();

            // Calculate totals
            var totalMessages = messages.Count;
            var totalTokens = messages.Sum(m => m.TokensUsed ?? 0);
            var totalCost = messages.Sum(m => m.Cost ?? 0);

            // Group by model
            var usageByModel = messages
                .GroupBy(m => new
                {
                    ModelId = m.Conversation?.ModelId ?? "Unknown",
                })
                .Select(g => new UsageByModelDto
                {
                    Model = GetModelDisplayName(g.Key.ModelId),
                    Connection = GetConnectionName(g.Key.ModelId),
                    Messages = g.Count(),
                    Tokens = g.Sum(m => m.TokensUsed ?? 0),
                    Cost = g.Sum(m => m.Cost ?? 0),
                    Color = GetModelColor(g.Key.ModelId)
                })
                .OrderByDescending(u => u.Messages)
                .ToList();

            var result = new UserUsageDto
            {
                TotalMessages = totalMessages,
                TotalTokens = totalTokens,
                TotalCost = totalCost,
                BillingPeriodStart = billingPeriodStart.ToString("MMM d, yyyy"),
                BillingPeriodEnd = billingPeriodEnd.ToString("MMM d, yyyy"),
                UsageByModel = usageByModel
            };

            _logger.LogInformation(
                "Usage analytics for user {UserId}: {Messages} messages, {Tokens} tokens, ${Cost}",
                userId, totalMessages, totalTokens, totalCost);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching usage analytics for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to fetch usage analytics", message = ex.Message });
        }
    }

    /// <summary>
    /// Get model display name from model ID
    /// </summary>
    private static string GetModelDisplayName(string modelId)
    {
        // Map model IDs to friendly display names
        return modelId switch
        {
            "gpt-4" => "GPT-4",
            "gpt-4-turbo" => "GPT-4 Turbo",
            "gpt-4-turbo-preview" => "GPT-4 Turbo",
            "gpt-3.5-turbo" => "GPT-3.5 Turbo",
            "claude-3-opus" => "Claude 3 Opus",
            "claude-3-sonnet" => "Claude 3 Sonnet",
            "claude-3-haiku" => "Claude 3 Haiku",
            "claude-3-5-sonnet" => "Claude 3.5 Sonnet",
            "llama-2-70b" => "Llama 2 70B",
            "llama-2-13b" => "Llama 2 13B",
            "mistral-7b" => "Mistral 7B",
            _ => modelId // Return as-is if unknown
        };
    }

    /// <summary>
    /// Get connection/provider name from model ID
    /// </summary>
    private static string GetConnectionName(string modelId)
    {
        // Infer provider from model ID
        if (modelId.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
            return "OpenAI";
        if (modelId.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
            return "Anthropic";
        if (modelId.StartsWith("llama-", StringComparison.OrdinalIgnoreCase))
            return "Meta (via Ollama)";
        if (modelId.StartsWith("mistral-", StringComparison.OrdinalIgnoreCase))
            return "Mistral (via Ollama)";

        return "Unknown";
    }

    /// <summary>
    /// Get color for model (for UI visualization)
    /// </summary>
    private static string GetModelColor(string modelId)
    {
        // Assign colors by provider
        if (modelId.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
            return "#A855F7"; // Purple for OpenAI
        if (modelId.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
            return "#F97316"; // Orange for Anthropic
        if (modelId.StartsWith("llama-", StringComparison.OrdinalIgnoreCase))
            return "#3B82F6"; // Blue for Llama
        if (modelId.StartsWith("mistral-", StringComparison.OrdinalIgnoreCase))
            return "#EC4899"; // Pink for Mistral

        return "#6B7280"; // Gray for unknown
    }
}
