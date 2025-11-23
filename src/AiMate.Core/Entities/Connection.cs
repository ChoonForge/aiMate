using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Connection to an AI provider (OpenAI, Anthropic, custom endpoints, MCP servers)
/// Can be scoped to User, Group, Organization, or Public
/// </summary>
public class Connection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of connection (OpenAI, Anthropic, Custom, MCP)
    /// </summary>
    public ConnectionType Type { get; set; }

    /// <summary>
    /// Owner of this connection
    /// </summary>
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Visibility level (Private, Group, Organization, Public)
    /// </summary>
    public ConnectionVisibility Visibility { get; set; } = ConnectionVisibility.Private;

    /// <summary>
    /// Organization this connection belongs to (if Organization visibility)
    /// </summary>
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>
    /// Groups that have access to this connection (if Group visibility)
    /// </summary>
    public List<Group> AllowedGroups { get; set; } = new();

    /// <summary>
    /// API key or credentials (encrypted)
    /// </summary>
    [MaxLength(5000)]
    public string? EncryptedCredentials { get; set; }

    /// <summary>
    /// Base URL for custom endpoints
    /// </summary>
    [Url]
    [MaxLength(2048)]
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Configuration JSON (provider-specific settings)
    /// </summary>
    [MaxLength(10000)]
    public string? ConfigurationJson { get; set; }

    /// <summary>
    /// Is this connection enabled?
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Is this a BYOK (Bring Your Own Key) connection?
    /// </summary>
    public bool IsBYOK { get; set; } = false;

    /// <summary>
    /// Available models for this connection
    /// </summary>
    public List<string> AvailableModels { get; set; } = new();

    /// <summary>
    /// Usage statistics
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalRequests { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int TotalTokens { get; set; } = 0;

    public DateTime? LastUsedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of connections
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// OpenAI API (GPT models)
    /// </summary>
    OpenAI = 0,

    /// <summary>
    /// Anthropic API (Claude models)
    /// </summary>
    Anthropic = 1,

    /// <summary>
    /// Custom OpenAI-compatible endpoint
    /// </summary>
    CustomOpenAI = 2,

    /// <summary>
    /// MCP (Model Context Protocol) server
    /// </summary>
    MCP = 3,

    /// <summary>
    /// Google AI (Gemini models)
    /// </summary>
    Google = 4,

    /// <summary>
    /// Azure OpenAI Service
    /// </summary>
    AzureOpenAI = 5,

    /// <summary>
    /// Other provider
    /// </summary>
    Other = 99
}
