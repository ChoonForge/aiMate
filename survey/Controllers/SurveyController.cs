using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaSurvey.Data;
using AlphaSurvey.Models;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace AlphaSurvey.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveyController : ControllerBase
{
    private readonly SurveyDbContext _context;
    private readonly ILogger<SurveyController> _logger;

    public SurveyController(SurveyDbContext context, ILogger<SurveyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitSurvey([FromBody] SurveyResponse response)
    {
        try
        {
            // Add metadata
            response.SubmittedAt = DateTime.UtcNow;
            response.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            response.UserAgent = Request.Headers["User-Agent"].ToString();

            // Validate email if interested in alpha testing
            if (response.InterestedInAlphaTesting && string.IsNullOrWhiteSpace(response.Email))
            {
                return BadRequest(new { error = "Email is required if interested in alpha testing" });
            }

            _context.SurveyResponses.Add(response);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Survey response {Id} submitted successfully", response.Id);

            return Ok(new { message = "Thank you for completing the survey!", id = response.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting survey response");
            return StatusCode(500, new { error = "An error occurred while submitting the survey" });
        }
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(SurveyStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<SurveyStats>> GetStats([FromQuery] string? adminKey)
    {
        // Simple admin authentication
        if (adminKey != Environment.GetEnvironmentVariable("SURVEY_ADMIN_KEY") && adminKey != "your-secret-admin-key-here")
        {
            return Unauthorized();
        }

        var totalResponses = await _context.SurveyResponses.CountAsync();
        var aiUsers = await _context.SurveyResponses.CountAsync(r => r.CurrentlyUsesAI);
        var interestedInTesting = await _context.SurveyResponses.CountAsync(r => r.InterestedInAlphaTesting);

        var toolUsage = await _context.SurveyResponses
            .Where(r => r.AIToolsUsed != null)
            .Select(r => r.AIToolsUsed)
            .ToListAsync();

        var allTools = toolUsage
            .SelectMany(t => t!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .GroupBy(t => t)
            .Select(g => new ToolCount { Tool = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .ToList();

        return Ok(new SurveyStats
        {
            TotalResponses = totalResponses,
            AIUsers = aiUsers,
            NonAIUsers = totalResponses - aiUsers,
            InterestedInTesting = interestedInTesting,
            TopTools = allTools.Take(10).ToList()
        });
    }

    [HttpGet("responses")]
    [ProducesResponseType(typeof(List<SurveyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SurveyResponse>>> GetResponses([FromQuery] string? adminKey)
    {
        // Simple admin authentication
        if (adminKey != Environment.GetEnvironmentVariable("SURVEY_ADMIN_KEY") && adminKey != "your-secret-admin-key-here")
        {
            return Unauthorized();
        }

        var responses = await _context.SurveyResponses
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        return Ok(responses);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv([FromQuery] string? adminKey)
    {
        // Simple admin authentication
        if (adminKey != Environment.GetEnvironmentVariable("SURVEY_ADMIN_KEY") && adminKey != "your-secret-admin-key-here")
        {
            return Unauthorized();
        }

        var responses = await _context.SurveyResponses
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.WriteRecords(responses);
        await streamWriter.FlushAsync();

        var bytes = memoryStream.ToArray();
        return File(bytes, "text/csv", $"survey_responses_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}

public class SurveyStats
{
    public int TotalResponses { get; set; }
    public int AIUsers { get; set; }
    public int NonAIUsers { get; set; }
    public int InterestedInTesting { get; set; }
    public List<ToolCount> TopTools { get; set; } = new();
}

public class ToolCount
{
    public string Tool { get; set; } = string.Empty;
    public int Count { get; set; }
}
