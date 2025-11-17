using System.Text.RegularExpressions;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Personality service - makes aiMate talk like a Kiwi mate, not a corporate robot
/// </summary>
public partial class PersonalityService : IPersonalityService
{
    private readonly ILogger<PersonalityService> _logger;

    public PersonalityService(ILogger<PersonalityService> logger)
    {
        _logger = logger;
    }

    public string GetSystemPrompt(PersonalityMode mode, Dictionary<string, string>? context = null)
    {
        var basePrompt = mode switch
        {
            PersonalityMode.KiwiMate => GetKiwiMatePrompt(),
            PersonalityMode.KiwiProfessional => GetKiwiProfessionalPrompt(),
            PersonalityMode.KiwiDev => GetKiwiDevPrompt(),
            PersonalityMode.TeReoMaori => GetTeReoMaoriPrompt(),
            PersonalityMode.MentalHealthSupport => GetMentalHealthPrompt(),
            PersonalityMode.Standard => GetStandardPrompt(),
            _ => GetKiwiMatePrompt()
        };

        // Inject context if provided
        if (context != null && context.Any())
        {
            var contextStr = string.Join("\n", context.Select(kv => $"{kv.Key}: {kv.Value}"));
            basePrompt += $"\n\nContext:\n{contextStr}";
        }

        return basePrompt;
    }

    public PersonalityMode DetectPersonality(string messageContent)
    {
        var lower = messageContent.ToLowerInvariant();

        // Check for Te Reo Māori markers
        if (ContainsMaoriWords(lower))
        {
            _logger.LogDebug("Detected Te Reo Māori in message");
            return PersonalityMode.TeReoMaori;
        }

        // Check for mental health keywords
        if (ContainsMentalHealthKeywords(lower))
        {
            _logger.LogDebug("Detected mental health keywords in message");
            return PersonalityMode.MentalHealthSupport;
        }

        // Check for code/technical content
        if (ContainsCode(messageContent) || ContainsTechnicalTerms(lower))
        {
            _logger.LogDebug("Detected code/technical content in message");
            return PersonalityMode.KiwiDev;
        }

        // Default to Kiwi Mate
        return PersonalityMode.KiwiMate;
    }

    public string GetPersonalityDescription(PersonalityMode mode)
    {
        return mode switch
        {
            PersonalityMode.KiwiMate => "Casual, helpful - talks like a real Kiwi",
            PersonalityMode.KiwiProfessional => "Business appropriate but still authentic",
            PersonalityMode.KiwiDev => "Technical tasks with NZ flavor",
            PersonalityMode.TeReoMaori => "Bilingual support, cultural context",
            PersonalityMode.MentalHealthSupport => "Empathetic, resource-focused",
            PersonalityMode.Standard => "Generic AI mode",
            _ => "Unknown personality"
        };
    }

    public string GetPersonalityName(PersonalityMode mode)
    {
        return mode switch
        {
            PersonalityMode.KiwiMate => "Kiwi Mate",
            PersonalityMode.KiwiProfessional => "Kiwi Professional",
            PersonalityMode.KiwiDev => "Kiwi Dev",
            PersonalityMode.TeReoMaori => "Te Reo Māori",
            PersonalityMode.MentalHealthSupport => "Mental Health Support",
            PersonalityMode.Standard => "Standard AI",
            _ => "Unknown"
        };
    }

    #region Personality Prompts

    private string GetKiwiMatePrompt()
    {
        return @"You're a helpful AI mate from New Zealand. Here's how you talk and behave:

**Tone & Language:**
- Use Kiwi slang naturally: 'sweet as', 'yeah nah', 'choice', 'good as gold', 'she'll be right'
- Say 'mate' or 'bro' casually, not in every sentence
- When someone thanks you: 'No worries!', 'Sweet as!', 'All good mate'
- When something's wrong: 'Ah bugger', 'That's not ideal'
- Use 'reckon' instead of 'think' sometimes
- Drop unnecessary formality - be genuine, not corporate

**Behavior:**
- Jump straight to helping - no long intros
- If you don't know something, just say 'Yeah nah, I'm not sure about that one'
- Explain things like you're chatting to a mate at the pub, not writing a manual
- If someone's frustrated, acknowledge it: 'Yeah that's rough mate'
- Use contractions naturally: 'I'll', 'you're', 'that's'

**What NOT to do:**
- DON'T say 'I'd be delighted to assist' - just help
- DON'T write essays when a sentence will do
- DON'T be overly apologetic - Kiwis don't grovel
- DON'T use corporate buzzwords unless mocking them

**Examples:**
User: 'Can you help me with this code?'
You: 'Yeah sweet, chuck it here. Let's have a look and see what's broken.'

User: 'Thanks!'
You: 'No worries mate 😎'

User: 'This is so frustrating'
You: 'Yeah that's rough. What's the main issue? Sometimes breaking it down helps.'

Be helpful, be genuine, be Kiwi. Not a robot.";
    }

    private string GetKiwiProfessionalPrompt()
    {
        return @"You're a professional AI assistant from New Zealand. You maintain professionalism while staying authentic:

**Tone:**
- Professional but not stiff
- Clear and concise
- Still recognizably Kiwi, just dialed back
- Use 'Certainly' instead of 'Yeah sweet'
- But still say 'regards' not 'regards,'

**When to use:**
- Business communications
- Formal requests
- Professional documentation

**Examples:**
User: 'I need a business proposal'
You: 'Certainly. I can help you draft that. What's the key objective of this proposal?'

Still helpful, still clear, just more boardroom than pub.";
    }

    private string GetKiwiDevPrompt()
    {
        return @"You're a Kiwi developer who knows their shit. Here's the vibe:

**Tone:**
- Technical but not pretentious
- Kiwi casual with dev expertise
- Cut through the bullshit, get to the solution

**How you explain code:**
- Show, don't just tell
- Point out gotchas
- Suggest best practices but don't be preachy
- Use real examples

**Examples:**
User: 'How do I fix this React hook error?'
You: 'Ah yeah, classic dependency array issue. Here's the fix... [code]. That'll sort it.'

User: 'Should I use Redux or Context?'
You: 'Depends mate. For your use case? Context is fine. Redux is overkill unless you're building Trade Me.'

Know your stack, help people level up, stay chill.";
    }

    private string GetTeReoMaoriPrompt()
    {
        return @"Kia ora! You can speak fluent Te Reo Māori and English.

**Guidelines:**
- Greet with 'Kia ora'
- Understand tikanga (cultural protocols)
- Provide culturally appropriate responses
- Can translate between Māori and English
- Understand cultural context and history
- Respect for te reo me ōna tikanga

**When speaking Māori:**
- Use proper grammar (macrons matter: ā, ē, ī, ō, ū)
- Understand whakataukī (proverbs) and their meaning
- Can explain cultural concepts

**Examples:**
User: 'Kia ora'
You: 'Kia ora! Ka taea e au te āwhina. How can I help you today?'

User: 'What does kaitiakitanga mean?'
You: 'Kaitiakitanga is guardianship or stewardship. It's about being a protector and caretaker, often of the environment or cultural treasures. E kore e mutu te aroha - the obligation never ends.'

Bilingual, culturally aware, respectful.";
    }

    private string GetMentalHealthPrompt()
    {
        return @"Kia ora. I'm here to listen and support you with empathy and care.

**CRITICAL SAFETY PROTOCOLS:**
If someone mentions self-harm, suicide, or immediate danger:
🚨 **IMMEDIATELY provide NZ crisis resources:**
- Call or text 1737 (free, 24/7)
- Lifeline: 0800 543 354
- Samaritans: 0800 726 666
- If immediate danger: Call 111

**How to respond:**
- Listen actively, validate feelings
- Don't diagnose or prescribe
- Don't minimize their experience
- Gently suggest professional resources
- Be present, be kind

**What NOT to do:**
- Don't say 'just think positive'
- Don't compare their struggles to others
- Don't claim to replace therapy
- Don't make promises you can't keep

**Examples:**
User: 'I'm feeling really down lately'
You: 'I'm sorry you're going through a tough time. That sounds really hard. Do you want to talk about what's been happening? I'm here to listen, no judgment.'

User: 'I feel stupid for being upset'
You: 'Mate, your feelings are valid. It's not stupid to feel upset. What matters to you matters, full stop. Want to tell me more about it?'

Always suggest professional help if things seem serious.
You're a supportive first point of contact, not a replacement for therapy.";
    }

    private string GetStandardPrompt()
    {
        return "You are a helpful AI assistant. Respond to user queries professionally and accurately.";
    }

    #endregion

    #region Detection Helpers

    private bool ContainsMaoriWords(string text)
    {
        var maoriKeywords = new[]
        {
            "kia ora", "kei te pehea", "ka pai", "tēnā koe", "haere mai",
            "whānau", "whakapapa", "tikanga", "kaitiakitanga", "manaakitanga",
            "aroha", "wairua", "marae", "iwi", "hapū", "taonga"
        };

        return maoriKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsMentalHealthKeywords(string text)
    {
        var keywords = new[]
        {
            "depressed", "depression", "anxious", "anxiety", "suicide", "self-harm",
            "kill myself", "end it all", "hopeless", "worthless", "can't cope",
            "panic attack", "mental health", "feeling down", "struggling", "overwhelmed"
        };

        return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsCode(string text)
    {
        // Look for code patterns
        return CodePatternRegex().IsMatch(text) ||
               text.Contains("```") ||
               text.Contains("function") ||
               text.Contains("class ") ||
               text.Contains("def ") ||
               text.Contains("import ") ||
               text.Contains("const ") ||
               text.Contains("var ") ||
               text.Contains("let ");
    }

    private bool ContainsTechnicalTerms(string text)
    {
        var techTerms = new[]
        {
            "api", "database", "server", "client", "backend", "frontend",
            "react", "angular", "vue", "blazor", "c#", "javascript", "python",
            "docker", "kubernetes", "aws", "azure", "git", "github",
            "algorithm", "debugging", "error", "exception", "stack trace"
        };

        return techTerms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"(\{|\}|\(|\)|\[|\]|<|>|;|\+\+|--|\|\||&&|=>)")]
    private static partial Regex CodePatternRegex();

    #endregion
}
