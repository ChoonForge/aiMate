using AiMate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiMate.Shared.Services;

/// <summary>
/// Central state management for the application
/// Replaces React's useState/useContext pattern
/// </summary>
public class AppStateService
{
    // Events for state changes
    public event Action? OnChange;
    public event Action<Conversation>? OnConversationChanged;
    public event Action<Message>? OnMessageAdded;

    // Conversations
    private List<Conversation> _conversations = new();
    private string? _activeConversationId;

    // UI State
    private bool _sidebarOpen = true;
    private bool _mobileSidebarOpen = false;
    private bool _isTyping = false;

    // Models
    private List<AIModel> _availableModels = new();
    private AIModel? _selectedModel;

    // Knowledge
    private List<KnowledgeItem> _knowledgeItems = new();

    // Folders
    private List<Folder> _folders = new();

    // User Preferences
    private UserPreferences _preferences = new();

    // Debug
    private bool _debugMode = false;
    private List<string> _debugLogs = new();

    #region Properties

    public IReadOnlyList<Conversation> Conversations => _conversations.AsReadOnly();
    public Conversation? ActiveConversation =>
        _activeConversationId != null
            ? _conversations.FirstOrDefault(c => c.Id == _activeConversationId)
            : null;

    public bool SidebarOpen
    {
        get => _sidebarOpen;
        set
        {
            _sidebarOpen = value;
            NotifyStateChanged();
        }
    }

    public bool MobileSidebarOpen
    {
        get => _mobileSidebarOpen;
        set
        {
            _mobileSidebarOpen = value;
            NotifyStateChanged();
        }
    }

    public bool IsTyping
    {
        get => _isTyping;
        set
        {
            _isTyping = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyList<AIModel> AvailableModels => _availableModels.AsReadOnly();

    public AIModel? SelectedModel
    {
        get => _selectedModel;
        set
        {
            _selectedModel = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyList<KnowledgeItem> KnowledgeItems => _knowledgeItems.AsReadOnly();
    public IReadOnlyList<Folder> Folders => _folders.AsReadOnly();
    public UserPreferences Preferences => _preferences;

    public bool DebugMode
    {
        get => _debugMode;
        set
        {
            _debugMode = value;
            NotifyStateChanged();
        }
    }

    public IReadOnlyList<string> DebugLogs => _debugLogs.AsReadOnly();

    #endregion

    #region Conversation Management

    public void CreateNewConversation(string? title = null)
    {
        var conversation = new Conversation
        {
            Title = title ?? $"Chat {DateTime.Now:MMM dd, HH:mm}"
        };

        _conversations.Insert(0, conversation);
        _activeConversationId = conversation.Id;

        OnConversationChanged?.Invoke(conversation);
        NotifyStateChanged();
    }

    public void SetActiveConversation(string conversationId)
    {
        if (_conversations.Any(c => c.Id == conversationId))
        {
            _activeConversationId = conversationId;
            OnConversationChanged?.Invoke(ActiveConversation!);
            NotifyStateChanged();
        }
    }

    public void UpdateConversation(string conversationId, Action<Conversation> updateAction)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conversation != null)
        {
            updateAction(conversation);
            conversation.UpdatedAt = DateTime.UtcNow;
            OnConversationChanged?.Invoke(conversation);
            NotifyStateChanged();
        }
    }

    public void DeleteConversation(string conversationId)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conversation != null)
        {
            _conversations.Remove(conversation);

            // If we deleted the active conversation, switch to first available
            if (_activeConversationId == conversationId)
            {
                _activeConversationId = _conversations.FirstOrDefault()?.Id;
                if (ActiveConversation != null)
                {
                    OnConversationChanged?.Invoke(ActiveConversation);
                }
            }

            NotifyStateChanged();
        }
    }

    public void ArchiveConversation(string conversationId)
    {
        UpdateConversation(conversationId, conv => conv.IsArchived = true);
    }

    public void PinConversation(string conversationId, bool pinned = true)
    {
        UpdateConversation(conversationId, conv => conv.IsPinned = pinned);
    }

    #endregion

    #region Message Management

    public void AddMessage(string conversationId, Message message)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conversation != null)
        {
            conversation.Messages.Add(message);
            conversation.UpdatedAt = DateTime.UtcNow;

            // Auto-generate title from first message if still default
            if (conversation.Messages.Count == 1 &&
                conversation.Title.StartsWith("Chat ") &&
                message.Role == "user")
            {
                conversation.Title = message.Content.Length > 50
                    ? message.Content.Substring(0, 47) + "..."
                    : message.Content;
            }

            OnMessageAdded?.Invoke(message);
            OnConversationChanged?.Invoke(conversation);
            NotifyStateChanged();
        }
    }

    public void UpdateMessage(string conversationId, string messageId, Action<Message> updateAction)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        var message = conversation?.Messages.FirstOrDefault(m => m.Id == messageId);

        if (message != null)
        {
            updateAction(message);
            conversation!.UpdatedAt = DateTime.UtcNow;
            OnConversationChanged?.Invoke(conversation);
            NotifyStateChanged();
        }
    }

    public void DeleteMessage(string conversationId, string messageId)
    {
        var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
        var message = conversation?.Messages.FirstOrDefault(m => m.Id == messageId);

        if (message != null && conversation != null)
        {
            conversation.Messages.Remove(message);
            conversation.UpdatedAt = DateTime.UtcNow;
            OnConversationChanged?.Invoke(conversation);
            NotifyStateChanged();
        }
    }

    #endregion

    #region Model Management

    public void LoadModels(List<AIModel> models)
    {
        _availableModels = models;
        if (_selectedModel == null && models.Any())
        {
            _selectedModel = models.First(m => m.IsEnabled);
        }
        NotifyStateChanged();
    }

    public void SetSelectedModel(string modelId)
    {
        var model = _availableModels.FirstOrDefault(m => m.Id == modelId);
        if (model != null)
        {
            _selectedModel = model;
            NotifyStateChanged();
        }
    }

    #endregion

    #region Knowledge Management

    public void AddKnowledgeItem(KnowledgeItem item)
    {
        _knowledgeItems.Add(item);
        NotifyStateChanged();
    }

    public void UpdateKnowledgeItem(string itemId, Action<KnowledgeItem> updateAction)
    {
        var item = _knowledgeItems.FirstOrDefault(k => k.Id == itemId);
        if (item != null)
        {
            updateAction(item);
            item.UpdatedAt = DateTime.UtcNow;
            NotifyStateChanged();
        }
    }

    public void DeleteKnowledgeItem(string itemId)
    {
        var item = _knowledgeItems.FirstOrDefault(k => k.Id == itemId);
        if (item != null)
        {
            _knowledgeItems.Remove(item);
            NotifyStateChanged();
        }
    }

    #endregion

    #region Folder Management

    public void CreateFolder(string name, string? color = null)
    {
        var folder = new Folder
        {
            Name = name,
            Color = color,
            SortOrder = _folders.Count
        };
        _folders.Add(folder);
        NotifyStateChanged();
    }

    public void DeleteFolder(string folderId)
    {
        var folder = _folders.FirstOrDefault(f => f.Id == folderId);
        if (folder != null)
        {
            _folders.Remove(folder);
            // Remove folder from conversations
            foreach (var conv in _conversations.Where(c => c.FolderId == folderId))
            {
                conv.FolderId = null;
            }
            NotifyStateChanged();
        }
    }

    #endregion

    #region Debug

    public void AddDebugLog(string log)
    {
        _debugLogs.Add($"[{DateTime.Now:HH:mm:ss}] {log}");
        if (_debugLogs.Count > 100) // Keep last 100 logs
        {
            _debugLogs.RemoveAt(0);
        }
        NotifyStateChanged();
    }

    public void ClearDebugLogs()
    {
        _debugLogs.Clear();
        NotifyStateChanged();
    }

    #endregion

    #region State Notification

    private void NotifyStateChanged() => OnChange?.Invoke();

    #endregion

    #region Persistence

    public void LoadConversation(Conversation conversation)
    {
        if (!_conversations.Any(c => c.Id == conversation.Id))
        {
            _conversations.Add(conversation);
            NotifyStateChanged();
        }
    }

    public void LoadPreferences(UserPreferences preferences)
    {
        _preferences = preferences;
        NotifyStateChanged();
    }

    public async Task SaveState()
    {
        // Saving is handled by StorageService in components
        await Task.CompletedTask;
    }

    public async Task LoadState()
    {
        // Loading is handled in Program.cs
        await Task.CompletedTask;
    }

    #endregion
}
