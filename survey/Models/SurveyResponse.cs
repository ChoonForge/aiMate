using System.ComponentModel.DataAnnotations;

namespace AlphaSurvey.Models;

public class SurveyResponse
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Demographics (non-intrusive)
    public string? AgeRange { get; set; } // e.g., "18-24", "25-34", "35-44", "45-54", "55+"
    public string? GeneralLocation { get; set; } // e.g., "Pacific Northwest", "Midwest", "Southeast"
    public string? OccupationCategory { get; set; } // e.g., "Technology", "Healthcare", "Education", "Business"
    public string? TechComfortLevel { get; set; } // "Beginner", "Intermediate", "Advanced", "Expert"

    // Current AI Usage
    [Required]
    public bool CurrentlyUsesAI { get; set; }

    public string? AIToolsUsed { get; set; } // Comma-separated: "ChatGPT, Claude, Gemini, etc."
    public string? PrimaryAITool { get; set; } // Main tool they use
    public string? FrequencyOfUse { get; set; } // "Daily", "Weekly", "Monthly", "Rarely"

    // What they do with AI
    public bool UseForWork { get; set; }
    public bool UseForPersonalProjects { get; set; }
    public bool UseForLearning { get; set; }
    public bool UseForCreativeWork { get; set; }
    public bool UseForCoding { get; set; }
    public bool UseForWriting { get; set; }
    public bool UseForResearch { get; set; }
    public bool UseForOther { get; set; }
    public string? OtherUseCase { get; set; }

    // Barriers to AI adoption (for non-users or limited users)
    public bool BarrierCost { get; set; }
    public bool BarrierComplexity { get; set; }
    public bool BarrierPrivacyConcerns { get; set; }
    public bool BarrierDontKnowHowToStart { get; set; }
    public bool BarrierDontSeeBenefit { get; set; }
    public bool BarrierOther { get; set; }
    public string? OtherBarrier { get; set; }

    // What's missing in current tools
    [MaxLength(1000)]
    public string? WhatsMissing { get; set; } // Open-ended feedback

    [MaxLength(1000)]
    public string? IdealAIFeatures { get; set; } // What would make the perfect AI tool for them

    [MaxLength(500)]
    public string? BiggestFrustration { get; set; } // Biggest pain point with current AI tools

    // Interest in alpha testing
    [Required]
    public bool InterestedInAlphaTesting { get; set; }

    [EmailAddress]
    public string? Email { get; set; } // Only if interested in alpha testing

    public string? AdditionalComments { get; set; }

    // Metadata
    public string? ReferralSource { get; set; } // Which Facebook group or source
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
