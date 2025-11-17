namespace AiMate.Core.Services;

/// <summary>
/// Generate synthetic training datasets for personality fine-tuning
/// </summary>
public interface IDatasetGeneratorService
{
    /// <summary>
    /// Generate dataset from templates (safe for mental health content)
    /// </summary>
    Task<TrainingDataset> GenerateFromTemplatesAsync(
        string personalityName,
        int numExamples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export dataset to JSONL format for fine-tuning
    /// </summary>
    Task<string> ExportToJsonLinesAsync(
        TrainingDataset dataset,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate dataset quality
    /// </summary>
    Task<DatasetValidationResult> ValidateDatasetAsync(
        TrainingDataset dataset,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available templates for personality
    /// </summary>
    Task<List<ConversationTemplate>> GetTemplatesForPersonalityAsync(
        string personalityName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Training dataset containing conversations
/// </summary>
public class TrainingDataset
{
    public string PersonalityName { get; set; } = string.Empty;
    public List<TrainingConversation> Conversations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public int TotalExamples => Conversations.Count;
    public Dictionary<string, int> ScenarioDistribution { get; set; } = new();
}

/// <summary>
/// Single training conversation
/// </summary>
public class TrainingConversation
{
    public string Scenario { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantMessage { get; set; } = string.Empty;
    public ConversationContext Context { get; set; } = new();
    public List<ConversationTurn> FollowUps { get; set; } = new();
}

/// <summary>
/// Conversation context metadata
/// </summary>
public class ConversationContext
{
    public int CrisisLevel { get; set; } // 1-5
    public List<string> CulturalMarkers { get; set; } = new();
    public List<string> ResourcesMentioned { get; set; } = new();
    public List<string> ActionItems { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Follow-up conversation turn
/// </summary>
public class ConversationTurn
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Conversation template for generation
/// </summary>
public class ConversationTemplate
{
    public string Scenario { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CrisisLevel { get; set; }
    public List<string> UserVariations { get; set; } = new();
    public List<string> AssistantVariations { get; set; } = new();
    public List<string> RequiredElements { get; set; } = new();
    public ConversationContext BaseContext { get; set; } = new();
}

/// <summary>
/// Dataset validation result
/// </summary>
public class DatasetValidationResult
{
    public bool IsValid { get; set; }
    public double QualityScore { get; set; } // 0.0 - 1.0
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, int> CrisisLevelDistribution { get; set; } = new();
    public int CulturalMarkerCount { get; set; }
    public int ResourceMentionCount { get; set; }
}
