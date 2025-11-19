using AiMate.Core.Entities;
using AiMate.Core.Enums;

namespace AiMate.Web.Store.Chat;

/// <summary>
/// Chat actions - things that can happen in chat
/// </summary>

// Send message
public record SendMessageAction(Guid ConversationId, string Content);
public record SendMessageSuccessAction(Guid ConversationId, Message UserMessage, Message AssistantMessage);
public record SendMessageFailureAction(string Error);

// Streaming
public record StreamChunkReceivedAction(Guid MessageId, string Chunk);
public record StreamCompleteAction(Guid MessageId, int TokensUsed, decimal Cost);

// Conversation management
public record CreateConversationAction(string Title, PersonalityMode? Personality = null);
public record CreateConversationSuccessAction(Conversation Conversation);
public record LoadConversationsAction();
public record LoadConversationsSuccessAction(List<Conversation> Conversations);
public record DeleteConversationAction(Guid ConversationId);
public record SetActiveConversationAction(Guid ConversationId);

// Message actions
public record RegenerateMessageAction(Guid MessageId);
public record EditMessageAction(Guid MessageId, string NewContent);
public record RateMessageAction(Guid MessageId, int Rating);
public record DeleteMessageAction(Guid MessageId);

// Model selection
public record LoadModelsAction();
public record LoadModelsSuccessAction(List<string> Models);
public record SelectModelAction(string Model);

// Input
public record UpdateInputAction(string Input);
public record ClearInputAction();
