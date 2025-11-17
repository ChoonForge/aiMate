using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiMate.Shared.Models;
using AiMate.Shared.Plugins;

namespace AiMate.Shared.Plugins.Safety
{
    /// <summary>
    /// Mental Health Safety Plugin
    /// Monitors for distress signals and intervenes BEFORE LLM can cause harm
    /// Based on multi-model AI safety analysis (GPT-4, Grok, Claude)
    /// </summary>
    public class MentalHealthSafetyPlugin : IMessageInterceptor, IUIExtension
    {
        private readonly CrisisResourceDatabase _crisisDb;
        private int _consecutiveDistressMessages = 0;
        private DateTime? _lastDistressTimestamp;

        public MentalHealthSafetyPlugin()
        {
            _crisisDb = new CrisisResourceDatabase();
        }

        #region IPlugin Implementation

        public string Id => "mental-health-safety";
        public string Name => "Mental Health Safety Monitor";
        public string Description => "Detects and intervenes in mental health crises";
        public string Version => "1.0.0";
        public string Author => "aiMate Team";
        public string Icon => "HealthAndSafety";

        public async Task InitializeAsync()
        {
            await _crisisDb.LoadResourcesAsync();
            Console.WriteLine($"[{Name}] Safety monitoring active. Crisis resources loaded for {_crisisDb.GetRegionCount()} regions.");
            return;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        #endregion

        #region IMessageInterceptor Implementation

        public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
        {
            // CRITICAL: Check for crisis signals BEFORE LLM sees the message
            var analysis = AnalyzeForCrisis(message.Content, context);

            if (analysis.IsCritical)
            {
                // STOP THE MESSAGE - Do NOT send to LLM
                return new MessageInterceptResult
                {
                    Continue = false,  // Block message
                    CancelReason = "Crisis intervention activated",
                    ModifiedMessage = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        Role = "assistant",
                        Content = GenerateCrisisIntervention(analysis),
                        Timestamp = DateTime.UtcNow
                    }
                };
            }

            if (analysis.IsElevated)
            {
                // Let message continue but add safety context
                var safetyPrompt = GenerateSafetyPrompt(analysis);
                
                return new MessageInterceptResult
                {
                    Continue = true,
                    ModifiedMessage = new Message
                    {
                        Id = message.Id,
                        Role = message.Role,
                        Content = $@"[SAFETY CONTEXT: User may be experiencing emotional distress. Respond with empathy, avoid victim-blaming, and watch for escalation.]

{message.Content}",
                        Timestamp = message.Timestamp
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["distress_level"] = analysis.DistressLevel,
                        ["triggers"] = analysis.Triggers,
                        ["consecutive_count"] = _consecutiveDistressMessages
                    }
                };
            }

            // Reset counter if no distress detected
            if (analysis.DistressLevel == DistressLevel.None)
            {
                _consecutiveDistressMessages = 0;
                _lastDistressTimestamp = null;
            }

            return new MessageInterceptResult { Continue = true };
        }

        public async Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context)
        {
            // Scan LLM response for harmful patterns
            var harmAnalysis = DetectHarmfulPatterns(message.Content);

            if (harmAnalysis.IsHarmful)
            {
                // BLOCK the harmful response
                return new MessageInterceptResult
                {
                    Continue = true,
                    ModifiedMessage = new Message
                    {
                        Id = message.Id,
                        Role = "assistant",
                        Content = GenerateHarmBlockMessage(harmAnalysis),
                        Timestamp = message.Timestamp
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["blocked_response"] = true,
                        ["harm_patterns"] = harmAnalysis.Patterns,
                        ["original_blocked"] = message.Content
                    }
                };
            }

            return new MessageInterceptResult { Continue = true };
        }

        #endregion

        #region IUIExtension Implementation

        public IEnumerable<MessageActionButton> GetMessageActions(Message message)
        {
            // Add "I Need Help" button to user messages
            if (message.Role == "user")
            {
                yield return new MessageActionButton
                {
                    Id = "crisis-help",
                    Label = "Get Crisis Support",
                    Icon = "LocalHospital",
                    Tooltip = "Access immediate crisis resources",
                    ShowOnUserMessages = true,
                    OnClick = async (msg) =>
                    {
                        // Show crisis resources modal
                        Console.WriteLine("Opening crisis resources...");
                        await Task.CompletedTask;
                    }
                };
            }
        }

        public IEnumerable<ChatInputExtension> GetInputExtensions()
        {
            // Add quick access to crisis resources
            yield return new ChatInputExtension
            {
                Id = "crisis-resources",
                Icon = "ContactSupport",
                Tooltip = "Crisis support resources",
                Order = 100, // Far right
                OnClick = async () =>
                {
                    Console.WriteLine("Showing crisis resources...");
                    await Task.CompletedTask;
                }
            };
        }

        public PluginSettings? GetSettingsUI()
        {
            return new PluginSettings
            {
                Title = "Mental Health Safety",
                Fields = new List<SettingField>
                {
                    new SettingField
                    {
                        Key = "region",
                        Label = "Your Region",
                        Type = SettingFieldType.Dropdown,
                        DefaultValue = "NZ",
                        Options = new List<string> { "NZ", "AU", "US", "UK", "CA" }
                    },
                    new SettingField
                    {
                        Key = "sensitivity",
                        Label = "Detection Sensitivity",
                        Type = SettingFieldType.Dropdown,
                        DefaultValue = "Conservative",
                        Options = new List<string> { "Conservative", "Moderate", "Aggressive" }
                    },
                    new SettingField
                    {
                        Key = "auto_intervene",
                        Label = "Auto-intervene on critical signals",
                        Type = SettingFieldType.Boolean,
                        DefaultValue = true
                    }
                }
            };
        }

        public string? RenderCustomContent(Message message) => null;

        #endregion

        #region Crisis Detection Logic

        private CrisisAnalysis AnalyzeForCrisis(string content, ConversationContext context)
        {
            var analysis = new CrisisAnalysis();
            var lowerContent = content.ToLower();

            // CRITICAL TRIGGERS (immediate intervention)
            var criticalTriggers = new[]
            {
                // Direct suicidal ideation
                "kill myself", "end my life", "want to die", "suicide", "suicidal",
                "not worth living", "no reason to live", "better off dead",
                
                // Active self-harm
                "cutting myself", "hurting myself", "self harm",
                
                // Imminent danger
                "going to do it", "tonight", "right now", "have the pills",
                "have a gun", "have a rope",
                
                // Goodbye messages
                "goodbye forever", "this is goodbye", "last message",
                "won't be here", "final message"
            };

            // ELEVATED TRIGGERS (add safety context)
            var elevatedTriggers = new[]
            {
                // Hopelessness
                "no hope", "hopeless", "pointless", "give up", "can't go on",
                
                // Severe distress
                "can't take it", "too much", "unbearable", "overwhelming",
                
                // Gaslighting/abuse context
                "gaslighting", "abusive", "manipulating", "controlling",
                
                // Isolation
                "nobody cares", "alone", "no one", "abandoned"
            };

            // Check for CRITICAL signals
            foreach (var trigger in criticalTriggers)
            {
                if (lowerContent.Contains(trigger))
                {
                    analysis.IsCritical = true;
                    analysis.DistressLevel = DistressLevel.Critical;
                    analysis.Triggers.Add(trigger);
                }
            }

            // Check for ELEVATED signals
            if (!analysis.IsCritical)
            {
                foreach (var trigger in elevatedTriggers)
                {
                    if (lowerContent.Contains(trigger))
                    {
                        analysis.IsElevated = true;
                        analysis.DistressLevel = DistressLevel.Elevated;
                        analysis.Triggers.Add(trigger);
                    }
                }
            }

            // Pattern detection: Multiple distress messages in short time
            if (analysis.DistressLevel >= DistressLevel.Elevated)
            {
                _consecutiveDistressMessages++;
                
                if (_lastDistressTimestamp.HasValue)
                {
                    var timeSinceLastDistress = DateTime.UtcNow - _lastDistressTimestamp.Value;
                    if (timeSinceLastDistress.TotalMinutes < 15 && _consecutiveDistressMessages >= 3)
                    {
                        // Escalating crisis - upgrade to critical
                        analysis.IsCritical = true;
                        analysis.DistressLevel = DistressLevel.Critical;
                        analysis.Triggers.Add("escalating_pattern");
                    }
                }
                
                _lastDistressTimestamp = DateTime.UtcNow;
            }

            return analysis;
        }

        private HarmAnalysis DetectHarmfulPatterns(string content)
        {
            var analysis = new HarmAnalysis();
            var lowerContent = content.ToLower();

            // VICTIM-BLAMING PATTERNS (from the LiquidAI failure)
            var victimBlamingPatterns = new[]
            {
                "you enabled", "your engagement contributed",
                "you allowed", "you chose to", "you participated",
                "your responsibility", "you could have stopped"
            };

            // GASLIGHTING PATTERNS
            var gaslightingPatterns = new[]
            {
                "you're overreacting", "it wasn't that bad",
                "you're too sensitive", "you misunderstood",
                "that didn't happen", "you're imagining"
            };

            // MINIMIZATION PATTERNS
            var minimizationPatterns = new[]
            {
                "it's not a big deal", "others have it worse",
                "at least", "look on the bright side",
                "everything happens for a reason"
            };

            foreach (var pattern in victimBlamingPatterns)
            {
                if (lowerContent.Contains(pattern))
                {
                    analysis.IsHarmful = true;
                    analysis.Patterns.Add($"victim_blaming: {pattern}");
                }
            }

            foreach (var pattern in gaslightingPatterns)
            {
                if (lowerContent.Contains(pattern))
                {
                    analysis.IsHarmful = true;
                    analysis.Patterns.Add($"gaslighting: {pattern}");
                }
            }

            foreach (var pattern in minimizationPatterns)
            {
                if (lowerContent.Contains(pattern))
                {
                    analysis.IsHarmful = true;
                    analysis.Patterns.Add($"minimization: {pattern}");
                }
            }

            return analysis;
        }

        #endregion

        #region Response Generation

        private string GenerateCrisisIntervention(CrisisAnalysis analysis)
        {
            var resources = _crisisDb.GetResourcesForRegion("NZ"); // TODO: Get from user settings

            return $@"## ⚠️ Crisis Support Activated

I've detected that you may be in distress. **Please know that you are not alone, and help is available right now.**

### Immediate Support (New Zealand)

**📞 Call or Text:**
- **1737** - Free 24/7 support, call or text
- **0800 543 354** (Lifeline) - 24/7 phone support
- **0800 111 777** (Depression Helpline) - 24/7

**💬 Online Chat:**
- [1737.org.nz/chat](https://1737.org.nz/chat) - Free text chat support

**🏥 Emergency:**
- **111** - For immediate danger, call emergency services

---

### Why I'm Showing You This

The message you sent contained words that suggest you might be thinking about harming yourself. Whether that's the case or not, I want you to know:

1. **Your feelings are real** - Whatever you're experiencing is valid
2. **This is not your fault** - You deserve support, not judgment
3. **Help exists** - Trained professionals are available 24/7
4. **You matter** - Your life has value, even when it doesn't feel that way

---

### What Happens Next

I'm here to listen if you want to talk, but **I'm not equipped to provide crisis support**. The people at the numbers above are specifically trained to help in situations like this.

If you're comfortable, you can continue our conversation, but please consider reaching out to one of these services first. They can provide the immediate, professional support you deserve.

**You don't have to face this alone.**

---

*This intervention was triggered by aiMate's Mental Health Safety Monitor. If you feel this was triggered in error, you can adjust sensitivity in Settings > Plugins > Mental Health Safety.*";
        }

        private string GenerateSafetyPrompt(CrisisAnalysis analysis)
        {
            return $@"[SAFETY GUIDANCE FOR AI]
- User shows signs of emotional distress
- Detected triggers: {string.Join(", ", analysis.Triggers)}
- Respond with genuine empathy and validation
- NEVER victim-blame or suggest responsibility for others' harmful actions
- NEVER minimize their experience
- NEVER gaslight or question their perception
- If they mention harm to self/others, prioritize crisis resources
- Use supportive language, avoid therapy-speak that could sound condescending
";
        }

        private string GenerateHarmBlockMessage(HarmAnalysis analysis)
        {
            return $@"I apologize, but I need to pause our conversation.

The response I was about to give contained patterns that could be harmful, particularly:
{string.Join("\n", analysis.Patterns.Select(p => $"- {p}"))}

Let me try again with a more supportive approach.

**What I should say instead:**

Whatever you're experiencing is real and valid. If someone has harmed you, that's on them - not you. You didn't cause their behavior, and you're not responsible for their choices.

Would you like to talk about what's going on? I'm here to listen without judgment.

---

*This message was blocked by aiMate's Mental Health Safety Monitor to prevent potential harm.*";
        }

        #endregion

        #region Supporting Classes

        private class CrisisAnalysis
        {
            public bool IsCritical { get; set; }
            public bool IsElevated { get; set; }
            public DistressLevel DistressLevel { get; set; } = DistressLevel.None;
            public List<string> Triggers { get; set; } = new();
        }

        private class HarmAnalysis
        {
            public bool IsHarmful { get; set; }
            public List<string> Patterns { get; set; } = new();
        }

        private enum DistressLevel
        {
            None = 0,
            Mild = 1,
            Elevated = 2,
            Critical = 3
        }

        private class CrisisResourceDatabase
        {
            private readonly Dictionary<string, CrisisResources> _resources = new();

            public Task LoadResourcesAsync()
            {
                // New Zealand
                _resources["NZ"] = new CrisisResources
                {
                    Region = "New Zealand",
                    Hotlines = new List<CrisisHotline>
                    {
                        new() { Name = "1737", Number = "1737", Available = "24/7", CanText = true },
                        new() { Name = "Lifeline", Number = "0800 543 354", Available = "24/7" },
                        new() { Name = "Depression Helpline", Number = "0800 111 777", Available = "24/7" },
                        new() { Name = "Youthline", Number = "0800 376 633", Available = "24/7", CanText = true }
                    },
                    WebChats = new List<string>
                    {
                        "https://1737.org.nz/chat",
                        "https://www.lifeline.org.nz/services/chat",
                        "https://www.youthline.co.nz/web-chat"
                    },
                    Emergency = "111"
                };

                // Australia
                _resources["AU"] = new CrisisResources
                {
                    Region = "Australia",
                    Hotlines = new List<CrisisHotline>
                    {
                        new() { Name = "Lifeline", Number = "13 11 14", Available = "24/7" },
                        new() { Name = "Beyond Blue", Number = "1300 22 4636", Available = "24/7" },
                        new() { Name = "Kids Helpline", Number = "1800 55 1800", Available = "24/7" }
                    },
                    Emergency = "000"
                };

                // TODO: Add more regions (US, UK, CA, etc.)

                return Task.CompletedTask;
            }

            public CrisisResources GetResourcesForRegion(string region)
            {
                return _resources.GetValueOrDefault(region, _resources["NZ"]);
            }

            public int GetRegionCount() => _resources.Count;
        }

        private class CrisisResources
        {
            public string Region { get; set; } = string.Empty;
            public List<CrisisHotline> Hotlines { get; set; } = new();
            public List<string> WebChats { get; set; } = new();
            public string Emergency { get; set; } = string.Empty;
        }

        private class CrisisHotline
        {
            public string Name { get; set; } = string.Empty;
            public string Number { get; set; } = string.Empty;
            public string Available { get; set; } = "24/7";
            public bool CanText { get; set; }
        }

        #endregion
    }
}
