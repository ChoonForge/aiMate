using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiMate.Shared.Models;

namespace AiMate.Shared.Plugins
{
    /// <summary>
    /// Base interface that all plugins must implement
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Unique plugin identifier (e.g., "code-generator", "web-search")
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name shown in UI
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Plugin author
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Icon (Material Design icon name)
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Called when plugin is loaded
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Called when plugin is unloaded
        /// </summary>
        Task DisposeAsync();
    }

    /// <summary>
    /// Intercepts messages before/after processing
    /// </summary>
    public interface IMessageInterceptor : IPlugin
    {
        /// <summary>
        /// Called BEFORE message is sent to LLM
        /// Can modify the message, add context, or cancel
        /// </summary>
        Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context);

        /// <summary>
        /// Called AFTER receiving response from LLM
        /// Can modify response, trigger actions, or add UI elements
        /// </summary>
        Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context);
    }

    /// <summary>
    /// Extends the chat UI with custom elements
    /// </summary>
    public interface IUIExtension : IPlugin
    {
        /// <summary>
        /// Add custom buttons to message actions
        /// </summary>
        IEnumerable<MessageActionButton> GetMessageActions(Message message);

        /// <summary>
        /// Add custom UI elements to chat input area
        /// </summary>
        IEnumerable<ChatInputExtension> GetInputExtensions();

        /// <summary>
        /// Add custom UI to settings modal
        /// </summary>
        PluginSettings? GetSettingsUI();

        /// <summary>
        /// Render custom content in message bubble
        /// </summary>
        string? RenderCustomContent(Message message);
    }

    /// <summary>
    /// Provides tools/functions that can be called during chat
    /// </summary>
    public interface IToolProvider : IPlugin
    {
        /// <summary>
        /// Register available tools
        /// </summary>
        IEnumerable<PluginTool> GetTools();

        /// <summary>
        /// Execute a tool
        /// </summary>
        Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters);
    }

    #region Supporting Models

    public class MessageInterceptResult
    {
        public bool Continue { get; set; } = true;
        public Message? ModifiedMessage { get; set; }
        public string? CancelReason { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ConversationContext
    {
        public string ConversationId { get; set; } = string.Empty;
        public List<Message> MessageHistory { get; set; } = new();
        public Dictionary<string, object> UserSettings { get; set; } = new();
        public Dictionary<string, object> PluginData { get; set; } = new();
    }

    public class MessageActionButton
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public Func<Message, Task> OnClick { get; set; } = _ => Task.CompletedTask;
        public bool ShowOnUserMessages { get; set; } = false;
        public bool ShowOnAssistantMessages { get; set; } = true;
    }

    public class ChatInputExtension
    {
        public string Id { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public Func<Task> OnClick { get; set; } = () => Task.CompletedTask;
        public int Order { get; set; } = 0; // Display order
    }

    public class PluginSettings
    {
        public string Title { get; set; } = string.Empty;
        public List<SettingField> Fields { get; set; } = new();
    }

    public class SettingField
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public SettingFieldType Type { get; set; }
        public object? DefaultValue { get; set; }
        public string? Placeholder { get; set; }
        public List<string>? Options { get; set; } // For dropdown
    }

    public enum SettingFieldType
    {
        Text,
        Number,
        Boolean,
        Dropdown,
        TextArea
    }

    public class PluginTool
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ToolParameter> Parameters { get; set; } = new();
        public bool RequiresConfirmation { get; set; } = false;
    }

    public class ToolParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Type Type { get; set; } = typeof(string);
        public bool Required { get; set; } = true;
        public object? DefaultValue { get; set; }
    }

    public class ToolResult
    {
        public bool Success { get; set; }
        public object? Result { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    #endregion
}
