using AiMate.Core.Enums;

namespace AiMate.Core.Services;

/// <summary>
/// Personality system - the KILLER feature of aiMate
/// </summary>
public interface IPersonalityService
{
    /// <summary>
    /// Get system prompt for a personality mode
    /// </summary>
    string GetSystemPrompt(PersonalityMode mode, Dictionary<string, string>? context = null);

    /// <summary>
    /// Auto-detect personality based on message content
    /// </summary>
    PersonalityMode DetectPersonality(string messageContent);

    /// <summary>
    /// Get personality description for UI
    /// </summary>
    string GetPersonalityDescription(PersonalityMode mode);

    /// <summary>
    /// Get personality name (localized)
    /// </summary>
    string GetPersonalityName(PersonalityMode mode);
}
