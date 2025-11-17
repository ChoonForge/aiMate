using Fluxor;

namespace AiMate.Web.Store.Chat;

/// <summary>
/// Chat reducers - pure functions that update state
/// </summary>
public static class ChatReducers
{
    [ReducerMethod]
    public static ChatState OnSendMessage(ChatState state, SendMessageAction action)
    {
        return state with { IsStreaming = true, IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static ChatState OnSendMessageSuccess(ChatState state, SendMessageSuccessAction action)
    {
        var conversation = state.Conversations.GetValueOrDefault(action.ConversationId);
        if (conversation == null) return state;

        var updatedConversation = conversation with
        {
            Messages = new List<Core.Entities.Message>(conversation.Messages)
            {
                action.UserMessage,
                action.AssistantMessage
            },
            UpdatedAt = DateTime.UtcNow
        };

        var newConversations = new Dictionary<Guid, Core.Entities.Conversation>(state.Conversations)
        {
            [action.ConversationId] = updatedConversation
        };

        return state with
        {
            Conversations = newConversations,
            StreamingMessageId = action.AssistantMessage.Id,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static ChatState OnSendMessageFailure(ChatState state, SendMessageFailureAction action)
    {
        return state with
        {
            IsStreaming = false,
            IsLoading = false,
            StreamingMessageId = null,
            Error = action.Error
        };
    }

    [ReducerMethod]
    public static ChatState OnStreamChunkReceived(ChatState state, StreamChunkReceivedAction action)
    {
        if (state.ActiveConversationId == null) return state;

        var conversation = state.Conversations.GetValueOrDefault(state.ActiveConversationId.Value);
        if (conversation == null) return state;

        var message = conversation.Messages.FirstOrDefault(m => m.Id == action.MessageId);
        if (message == null) return state;

        // Update message content
        var updatedMessage = message with
        {
            Content = message.Content + action.Chunk
        };

        var updatedMessages = conversation.Messages.Select(m =>
            m.Id == action.MessageId ? updatedMessage : m).ToList();

        var updatedConversation = conversation with { Messages = updatedMessages };

        var newConversations = new Dictionary<Guid, Core.Entities.Conversation>(state.Conversations)
        {
            [state.ActiveConversationId.Value] = updatedConversation
        };

        return state with { Conversations = newConversations };
    }

    [ReducerMethod]
    public static ChatState OnStreamComplete(ChatState state, StreamCompleteAction action)
    {
        return state with
        {
            IsStreaming = false,
            StreamingMessageId = null
        };
    }

    [ReducerMethod]
    public static ChatState OnCreateConversationSuccess(ChatState state, CreateConversationSuccessAction action)
    {
        var newConversations = new Dictionary<Guid, Core.Entities.Conversation>(state.Conversations)
        {
            [action.Conversation.Id] = action.Conversation
        };

        return state with
        {
            Conversations = newConversations,
            ActiveConversationId = action.Conversation.Id
        };
    }

    [ReducerMethod]
    public static ChatState OnLoadConversationsSuccess(ChatState state, LoadConversationsSuccessAction action)
    {
        var conversations = action.Conversations.ToDictionary(c => c.Id);
        return state with { Conversations = conversations, IsLoading = false };
    }

    [ReducerMethod]
    public static ChatState OnSetActiveConversation(ChatState state, SetActiveConversationAction action)
    {
        return state with { ActiveConversationId = action.ConversationId };
    }

    [ReducerMethod]
    public static ChatState OnLoadModelsSuccess(ChatState state, LoadModelsSuccessAction action)
    {
        return state with { AvailableModels = action.Models };
    }

    [ReducerMethod]
    public static ChatState OnSelectModel(ChatState state, SelectModelAction action)
    {
        return state with { SelectedModel = action.Model };
    }

    [ReducerMethod]
    public static ChatState OnUpdateInput(ChatState state, UpdateInputAction action)
    {
        return state with { CurrentInput = action.Input };
    }

    [ReducerMethod]
    public static ChatState OnClearInput(ChatState state, ClearInputAction action)
    {
        return state with { CurrentInput = string.Empty };
    }

    [ReducerMethod]
    public static ChatState OnDeleteConversation(ChatState state, DeleteConversationAction action)
    {
        var newConversations = new Dictionary<Guid, Core.Entities.Conversation>(state.Conversations);
        newConversations.Remove(action.ConversationId);

        var newActiveId = state.ActiveConversationId == action.ConversationId
            ? newConversations.Keys.FirstOrDefault()
            : state.ActiveConversationId;

        return state with
        {
            Conversations = newConversations,
            ActiveConversationId = newActiveId
        };
    }
}
