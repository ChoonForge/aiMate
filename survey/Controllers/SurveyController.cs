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

    [HttpGet("use-cases")]
    [ProducesResponseType(typeof(List<UseCaseCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UseCaseCategoryDto>>> GetUseCases()
    {
        var categories = await _context.UseCaseCategories
            .Include(c => c.Options)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new UseCaseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Options = c.Options
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => new UseCaseOptionDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Description = o.Description
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitSurvey([FromBody] SurveySubmissionDto submission)
    {
        try
        {
            var response = new SurveyResponse
            {
                SubmittedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),

                // Demographics
                AgeRange = submission.AgeRange,
                GeneralLocation = submission.GeneralLocation,
                OccupationCategory = submission.OccupationCategory,
                TechComfortLevel = submission.TechComfortLevel,

                // AI Usage
                CurrentlyUsesAI = submission.CurrentlyUsesAI,
                AIToolsUsed = submission.AIToolsUsed,
                PrimaryAITool = submission.PrimaryAITool,
                FrequencyOfUse = submission.FrequencyOfUse,

                // Legacy use case fields (kept for backward compatibility)
                UseForWork = submission.UseForWork,
                UseForPersonalProjects = submission.UseForPersonalProjects,
                UseForLearning = submission.UseForLearning,
                UseForCreativeWork = submission.UseForCreativeWork,
                UseForCoding = submission.UseForCoding,
                UseForWriting = submission.UseForWriting,
                UseForResearch = submission.UseForResearch,
                UseForOther = submission.UseForOther,
                OtherUseCase = submission.OtherUseCase,

                // Barriers
                BarrierCost = submission.BarrierCost,
                BarrierComplexity = submission.BarrierComplexity,
                BarrierPrivacyConcerns = submission.BarrierPrivacyConcerns,
                BarrierDontKnowHowToStart = submission.BarrierDontKnowHowToStart,
                BarrierDontSeeBenefit = submission.BarrierDontSeeBenefit,
                BarrierOther = submission.BarrierOther,
                OtherBarrier = submission.OtherBarrier,

                // Feedback
                WhatsMissing = submission.WhatsMissing,
                IdealAIFeatures = submission.IdealAIFeatures,
                BiggestFrustration = submission.BiggestFrustration,

                // NEW: Feature & LLM metrics
                TypicalConversationLength = submission.TypicalConversationLength,
                NeedsLongTermMemory = submission.NeedsLongTermMemory,
                NeedsContextAcrossSessions = submission.NeedsContextAcrossSessions,
                WorksWithDocuments = submission.WorksWithDocuments,
                WorksWithImages = submission.WorksWithImages,
                WorksWithCode = submission.WorksWithCode,
                WorksWithData = submission.WorksWithData,
                TypicalFileSize = submission.TypicalFileSize,
                PreferredResponseStyle = submission.PreferredResponseStyle,
                QualityVsSpeed = submission.QualityVsSpeed,
                NeedsCodeExecution = submission.NeedsCodeExecution,
                NeedsImageGeneration = submission.NeedsImageGeneration,
                NeedsWebSearch = submission.NeedsWebSearch,
                NeedsDataVisualization = submission.NeedsDataVisualization,
                NeedsTeamCollaboration = submission.NeedsTeamCollaboration,
                NeedsWorkspaceOrganization = submission.NeedsWorkspaceOrganization,
                WouldShareConversations = submission.WouldShareConversations,
                NeedsAPIAccess = submission.NeedsAPIAccess,
                NeedsIntegrationWithTools = submission.NeedsIntegrationWithTools,
                SpecificIntegrations = submission.SpecificIntegrations,
                DataPrivacyConcern = submission.DataPrivacyConcern,
                PreferredDataLocation = submission.PreferredDataLocation,
                WillingToShareDataForImprovement = submission.WillingToShareDataForImprovement,
                WillingToPayMonthly = submission.WillingToPayMonthly,
                PreferredPricingModel = submission.PreferredPricingModel,
                MostImportantValue = submission.MostImportantValue,
                UsesWeb = submission.UsesWeb,
                UsesDesktop = submission.UsesDesktop,
                UsesMobile = submission.UsesMobile,
                PrimaryPlatform = submission.PrimaryPlatform,
                FamiliarWithGPT4 = submission.FamiliarWithGPT4,
                FamiliarWithClaude = submission.FamiliarWithClaude,
                FamiliarWithGemini = submission.FamiliarWithGemini,
                FamiliarWithOpenSource = submission.FamiliarWithOpenSource,
                PreferredLLMFeatures = submission.PreferredLLMFeatures,
                NeedsVoiceInput = submission.NeedsVoiceInput,
                NeedsVoiceOutput = submission.NeedsVoiceOutput,
                NeedsVideoAnalysis = submission.NeedsVideoAnalysis,
                PrimaryLanguage = submission.PrimaryLanguage,
                NeedsMultilingualSupport = submission.NeedsMultilingualSupport,

                // Alpha testing
                InterestedInAlphaTesting = submission.InterestedInAlphaTesting,
                Email = submission.Email,
                AdditionalComments = submission.AdditionalComments,
                ReferralSource = submission.ReferralSource
            };

            // Validate email if interested in alpha testing
            if (response.InterestedInAlphaTesting && string.IsNullOrWhiteSpace(response.Email))
            {
                return BadRequest(new { error = "Email is required if interested in alpha testing" });
            }

            _context.SurveyResponses.Add(response);
            await _context.SaveChangesAsync();

            // Add selected use cases
            if (submission.SelectedUseCaseIds?.Any() == true)
            {
                var useCases = submission.SelectedUseCaseIds
                    .Select(ucId => new SurveyResponseUseCase
                    {
                        SurveyResponseId = response.Id,
                        UseCaseOptionId = ucId
                    })
                    .ToList();

                _context.SurveyResponseUseCases.AddRange(useCases);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Survey response {Id} submitted successfully with {UseCaseCount} use cases",
                response.Id, submission.SelectedUseCaseIds?.Count ?? 0);

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

public class UseCaseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<UseCaseOptionDto> Options { get; set; } = new();
}

public class UseCaseOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SurveySubmissionDto
{
    // Demographics
    public string? AgeRange { get; set; }
    public string? GeneralLocation { get; set; }
    public string? OccupationCategory { get; set; }
    public string? TechComfortLevel { get; set; }

    // AI Usage
    public bool CurrentlyUsesAI { get; set; }
    public string? AIToolsUsed { get; set; }
    public string? PrimaryAITool { get; set; }
    public string? FrequencyOfUse { get; set; }

    // Legacy use cases (kept for backward compatibility)
    public bool UseForWork { get; set; }
    public bool UseForPersonalProjects { get; set; }
    public bool UseForLearning { get; set; }
    public bool UseForCreativeWork { get; set; }
    public bool UseForCoding { get; set; }
    public bool UseForWriting { get; set; }
    public bool UseForResearch { get; set; }
    public bool UseForOther { get; set; }
    public string? OtherUseCase { get; set; }

    // NEW: Dynamic use cases
    public List<int>? SelectedUseCaseIds { get; set; }

    // Barriers
    public bool BarrierCost { get; set; }
    public bool BarrierComplexity { get; set; }
    public bool BarrierPrivacyConcerns { get; set; }
    public bool BarrierDontKnowHowToStart { get; set; }
    public bool BarrierDontSeeBenefit { get; set; }
    public bool BarrierOther { get; set; }
    public string? OtherBarrier { get; set; }

    // Feedback
    public string? WhatsMissing { get; set; }
    public string? IdealAIFeatures { get; set; }
    public string? BiggestFrustration { get; set; }

    // NEW: Feature & LLM metrics
    public string? TypicalConversationLength { get; set; }
    public bool NeedsLongTermMemory { get; set; }
    public bool NeedsContextAcrossSessions { get; set; }
    public bool WorksWithDocuments { get; set; }
    public bool WorksWithImages { get; set; }
    public bool WorksWithCode { get; set; }
    public bool WorksWithData { get; set; }
    public string? TypicalFileSize { get; set; }
    public string? PreferredResponseStyle { get; set; }
    public string? QualityVsSpeed { get; set; }
    public bool NeedsCodeExecution { get; set; }
    public bool NeedsImageGeneration { get; set; }
    public bool NeedsWebSearch { get; set; }
    public bool NeedsDataVisualization { get; set; }
    public bool NeedsTeamCollaboration { get; set; }
    public bool NeedsWorkspaceOrganization { get; set; }
    public bool WouldShareConversations { get; set; }
    public bool NeedsAPIAccess { get; set; }
    public bool NeedsIntegrationWithTools { get; set; }
    public string? SpecificIntegrations { get; set; }
    public string? DataPrivacyConcern { get; set; }
    public string? PreferredDataLocation { get; set; }
    public bool WillingToShareDataForImprovement { get; set; }
    public string? WillingToPayMonthly { get; set; }
    public string? PreferredPricingModel { get; set; }
    public string? MostImportantValue { get; set; }
    public bool UsesWeb { get; set; }
    public bool UsesDesktop { get; set; }
    public bool UsesMobile { get; set; }
    public string? PrimaryPlatform { get; set; }
    public bool FamiliarWithGPT4 { get; set; }
    public bool FamiliarWithClaude { get; set; }
    public bool FamiliarWithGemini { get; set; }
    public bool FamiliarWithOpenSource { get; set; }
    public string? PreferredLLMFeatures { get; set; }
    public bool NeedsVoiceInput { get; set; }
    public bool NeedsVoiceOutput { get; set; }
    public bool NeedsVideoAnalysis { get; set; }
    public string? PrimaryLanguage { get; set; }
    public bool NeedsMultilingualSupport { get; set; }

    // Alpha testing
    public bool InterestedInAlphaTesting { get; set; }
    public string? Email { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ReferralSource { get; set; }
}
