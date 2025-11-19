using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Chat;

/// <summary>
/// Chat effects - side effects that interact with services
/// </summary>
public class ChatEffects
{
    private readonly ILiteLLMService _liteLLM;
    private readonly IPersonalityService _personality;
    private readonly ILogger<ChatEffects> _logger;

    public ChatEffects(
        ILiteLLMService liteLLM,
        IPersonalityService personality,
        ILogger<ChatEffects> logger)
    {
        _liteLLM = liteLLM;
        _personality = personality;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleSendMessage(SendMessageAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Sending message to conversation {ConversationId}", action.ConversationId);

            // Create user message
            var userMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = action.ConversationId,
                Role = MessageRole.User,
                Content = action.Content,
                CreatedAt = DateTime.UtcNow
            };

            // Create placeholder assistant message
            var assistantMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = action.ConversationId,
                Role = MessageRole.Assistant,
                Content = string.Empty,
                IsStreaming = true,
                CreatedAt = DateTime.UtcNow
            };

            // Dispatch success with both messages
            dispatcher.Dispatch(new SendMessageSuccessAction(
                action.ConversationId,
                userMessage,
                assistantMessage));

            // Build chat history (simplified - in production, get from conversation)
            var messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = action.Content }
            };

            // Get personality mode (simplified - would come from conversation settings)
            var personalityMode = PersonalityMode.KiwiMate;
            var systemPrompt = _personality.GetSystemPrompt(personalityMode);

            // Prepend system message
            messages.Insert(0, new ChatMessage { Role = "system", Content = systemPrompt });

            // Create request
            var request = new ChatCompletionRequest
            {
                Model = "gpt-4", // Would come from state
                Messages = messages,
                Stream = true,
                Temperature = 0.7,
                MaxTokens = 2000
            };

            // Stream response
            await foreach (var chunk in _liteLLM.StreamChatCompletionAsync(request))
            {
                var content = chunk.Choices.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    dispatcher.Dispatch(new StreamChunkReceivedAction(assistantMessage.Id, content));
                }
            }

            // Stream complete
            dispatcher.Dispatch(new StreamCompleteAction(assistantMessage.Id, 0, 0));

            _logger.LogInformation("Message sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            dispatcher.Dispatch(new SendMessageFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadModels(LoadModelsAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Loading available models");

            var models = await _liteLLM.GetModelsAsync();

            dispatcher.Dispatch(new LoadModelsSuccessAction(models));

            _logger.LogInformation("Loaded {Count} models", models.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load models");
            // Dispatch fallback models
            dispatcher.Dispatch(new LoadModelsSuccessAction(new List<string>
            {
                "gpt-4",
                "gpt-3.5-turbo",
                "claude-3-5-sonnet-20241022"
            }));
        }
    }

    [EffectMethod]
    public Task HandleCreateConversation(CreateConversationAction action, IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation("Creating new conversation: {Title}", action.Title);

            // Create workspace (simplified - in production, would use WorkspaceService)
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = "Default Workspace",
                UserId = Guid.NewGuid(), // Would come from auth
                CreatedAt = DateTime.UtcNow
            };

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                Workspace = workspace,
                Title = action.Title,
                PersonalityOverride = action.Personality,
                Messages = new List<Message>(),
                CreatedAt = DateTime.UtcNow
            };

            dispatcher.Dispatch(new CreateConversationSuccessAction(conversation));

            _logger.LogInformation("Conversation created: {ConversationId}", conversation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create conversation");
        }

        return Task.CompletedTask;
    }
}
