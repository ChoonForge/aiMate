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

    // NEW: Dynamic Use Cases (replaces boolean fields above)
    public ICollection<SurveyResponseUseCase> SelectedUseCases { get; set; } = new List<SurveyResponseUseCase>();

    // NEW: Feature & LLM Selection Metrics
    // Context & Conversation Needs
    public string? TypicalConversationLength { get; set; } // "Single question", "Few exchanges (2-5)", "Extended (6-15)", "Very long (15+)"
    public bool NeedsLongTermMemory { get; set; } // Remember across sessions
    public bool NeedsContextAcrossSessions { get; set; } // Continue where you left off

    // File & Document Handling
    public bool WorksWithDocuments { get; set; } // PDFs, Word docs, etc.
    public bool WorksWithImages { get; set; } // View/analyze images
    public bool WorksWithCode { get; set; } // Code files
    public bool WorksWithData { get; set; } // CSV, Excel, databases
    public string? TypicalFileSize { get; set; } // "Small (<1MB)", "Medium (1-10MB)", "Large (10-100MB)", "Very Large (100MB+)"

    // Response Preferences
    public string? PreferredResponseStyle { get; set; } // "Concise & brief", "Moderate detail", "Very detailed", "Depends on task"
    public string? QualityVsSpeed { get; set; } // "Speed is critical", "Balanced", "Quality over speed", "Highest quality always"

    // Advanced Features
    public bool NeedsCodeExecution { get; set; } // Run/test code
    public bool NeedsImageGeneration { get; set; } // Create images
    public bool NeedsWebSearch { get; set; } // Real-time web search
    public bool NeedsDataVisualization { get; set; } // Charts, graphs

    // Collaboration & Sharing
    public bool NeedsTeamCollaboration { get; set; } // Share with team members
    public bool NeedsWorkspaceOrganization { get; set; } // Projects, folders, organization
    public bool WouldShareConversations { get; set; } // Share publicly or with others

    // Integration Needs
    public bool NeedsAPIAccess { get; set; }
    public bool NeedsIntegrationWithTools { get; set; } // Slack, email, IDEs, etc.
    public string? SpecificIntegrations { get; set; } // Comma-separated list

    // Privacy & Data Preferences
    public string? DataPrivacyConcern { get; set; } // "Not concerned", "Somewhat concerned", "Very concerned", "Critical concern"
    public string? PreferredDataLocation { get; set; } // "Cloud is fine", "Prefer local option", "Must be local only"
    public bool WillingToShareDataForImprovement { get; set; } // Opt-in to training data

    // Pricing & Cost
    public string? WillingToPayMonthly { get; set; } // "Free only", "$0-10", "$10-20", "$20-50", "$50+", "Depends on value"
    public string? PreferredPricingModel { get; set; } // "Subscription", "Pay-per-use", "One-time purchase", "Free with ads", "Freemium"
    public string? MostImportantValue { get; set; } // "Cost", "Features", "Privacy", "Speed", "Accuracy", "Ease of use"

    // Platform Preferences
    public bool UsesWeb { get; set; }
    public bool UsesDesktop { get; set; }
    public bool UsesMobile { get; set; }
    public string? PrimaryPlatform { get; set; } // "Web", "Desktop", "Mobile"

    // Specific LLM Preferences/Familiarity
    public bool FamiliarWithGPT4 { get; set; }
    public bool FamiliarWithClaude { get; set; }
    public bool FamiliarWithGemini { get; set; }
    public bool FamiliarWithOpenSource { get; set; } // Llama, Mistral, etc.
    public string? PreferredLLMFeatures { get; set; } // What do they like about their preferred LLM

    // Multimodal Needs
    public bool NeedsVoiceInput { get; set; }
    public bool NeedsVoiceOutput { get; set; }
    public bool NeedsVideoAnalysis { get; set; }

    // Language Preferences
    public string? PrimaryLanguage { get; set; } // "English", "Spanish", "Multilingual", etc.
    public bool NeedsMultilingualSupport { get; set; }

    // Metadata
    public string? ReferralSource { get; set; } // Which Facebook group or source
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
