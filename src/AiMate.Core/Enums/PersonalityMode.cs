namespace AiMate.Core.Enums;

/// <summary>
/// AI personality modes - the killer feature of aiMate
/// </summary>
public enum PersonalityMode
{
    /// <summary>
    /// Default Kiwi mate - casual, helpful, speaks like a real New Zealander
    /// "Yeah sweet, what do you need?"
    /// </summary>
    KiwiMate,

    /// <summary>
    /// Professional Kiwi - business appropriate but still authentic
    /// "Certainly, I can help with that."
    /// </summary>
    KiwiProfessional,

    /// <summary>
    /// Kiwi developer - technical tasks with NZ flavor
    /// "Yeah sweet, what's the issue - frontend or backend?"
    /// </summary>
    KiwiDev,

    /// <summary>
    /// Te Reo Māori mode - bilingual support, cultural context
    /// "Kia ora! Ka taea e au te āwhina."
    /// </summary>
    TeReoMaori,

    /// <summary>
    /// Mental health support - empathetic, resource-focused
    /// "Kia ora. I'm here to listen. How are you feeling today?"
    /// </summary>
    MentalHealthSupport,

    /// <summary>
    /// Standard AI - generic fallback
    /// "I am ready to assist with your request."
    /// </summary>
    Standard
}
