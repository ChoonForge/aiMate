using AiMate.Core.Entities;
using Fluxor;

namespace AiMate.Web.Store.Chat;

/// <summary>
/// Chat state - all chat-related state in one place
/// </summary>
[FeatureState]
public record ChatState
{
    public Guid? ActiveConversationId { get; init; }
    public Dictionary<Guid, Conversation> Conversations { get; init; } = new();
    public bool IsStreaming { get; init; }
    public Guid? StreamingMessageId { get; init; }
    public string CurrentInput { get; init; } = string.Empty;
    public List<string> AvailableModels { get; init; } = new();
    public string SelectedModel { get; init; } = "gpt-4";
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
}
