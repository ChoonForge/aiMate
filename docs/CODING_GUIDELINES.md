# aiMate Coding Guidelines

**The Developer's Bible for aiMate Architecture**

This document is your comprehensive guide to understanding and extending aiMate. Read this before making any code changes.

---

## Table of Contents

1. [Philosophy & Core Principles](#philosophy--core-principles)
2. [Clean Architecture](#clean-architecture)
3. [Fluxor State Management](#fluxor-state-management)
4. [Entity Design Patterns](#entity-design-patterns)
5. [Service Layer Patterns](#service-layer-patterns)
6. [Component Patterns (Blazor)](#component-patterns-blazor)
7. [Extensibility Points](#extensibility-points)
8. [What NOT to Do (Anti-Patterns)](#what-not-to-do-anti-patterns)
9. [Testing Patterns](#testing-patterns)
10. [Performance Considerations](#performance-considerations)

---

## Philosophy & Core Principles

### The aiMate Way

**1. Predictability Over Cleverness**
- Code should be boring and predictable
- Avoid magic, avoid tricks, avoid "clever" solutions
- If you need comments to explain it, simplify it

**2. Type Safety is Sacred**
- Use C#'s type system to the fullest
- Explicit types over `var` (except LINQ)
- Never use `dynamic` unless absolutely necessary
- Leverage nullable reference types

**3. Immutability Where Possible**
- Use `record` types for DTOs and state
- Use `init` accessors for immutable properties
- Think twice before adding mutable state

**4. Separation of Concerns**
- Business logic in Core
- External integrations in Infrastructure
- UI concerns in Web
- Never mix these layers

**5. Fail Fast, Fail Clearly**
- Validate early, fail with meaningful messages
- Use custom exceptions with context
- Log everything important

---

## Clean Architecture

### The Three Layers

```
┌─────────────────────────────────────────┐
│          AiMate.Web (Presentation)      │
│  - Blazor Components                    │
│  - Fluxor Store (State Management)      │
│  - UI Logic Only                        │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│     AiMate.Infrastructure (External)    │
│  - Service Implementations              │
│  - Database (EF Core)                   │
│  - External APIs                        │
│  - File System                          │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│        AiMate.Core (Domain)             │
│  - Entities                             │
│  - Enums                                │
│  - Service Interfaces                   │
│  - Business Rules                       │
│  - NO DEPENDENCIES                      │
└─────────────────────────────────────────┘
```

### Layer Rules (DO NOT BREAK THESE)

#### ✅ AiMate.Core (Domain Layer)

**What Goes Here:**
- Entity classes (`User`, `Workspace`, `Message`, etc.)
- Enums (`PersonalityMode`, `WorkspaceType`, etc.)
- Service interfaces (`IWorkspaceService`, `IChatService`, etc.)
- Business rules and domain logic
- Value objects

**Dependencies:**
- ❌ NO dependencies on other projects
- ❌ NO Entity Framework
- ❌ NO external libraries (except basic .NET)
- ✅ Pure C# domain models only

**Example Entity:**
```csharp
namespace AiMate.Core.Entities;

/// <summary>
/// A workspace is the core unit of organization
/// </summary>
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public WorkspaceType Type { get; set; } = WorkspaceType.General;

    // Navigation properties
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public List<Conversation> Conversations { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**Rules:**
- Always use `Guid` for IDs
- Use `required` for mandatory properties
- Initialize collections to empty (never null)
- Use nullable reference types (`?`) appropriately
- Add XML comments to public properties

#### ✅ AiMate.Infrastructure (External Concerns)

**What Goes Here:**
- Service implementations
- `AiMateDbContext` (Entity Framework)
- Database migrations
- External API clients (OpenAI, LiteLLM, etc.)
- File storage logic
- Email services
- Caching implementations

**Dependencies:**
- ✅ Can reference `AiMate.Core`
- ✅ Entity Framework Core
- ✅ External NuGet packages
- ❌ NEVER reference `AiMate.Web`

**Example Service:**
```csharp
namespace AiMate.Infrastructure.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<WorkspaceService> _logger;

    public WorkspaceService(
        AiMateDbContext context,
        ILogger<WorkspaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Workspace> CreateAsync(CreateWorkspaceRequest request)
    {
        var workspace = new Workspace
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            UserId = request.UserId
        };

        await _context.Workspaces.AddAsync(workspace);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Workspace created: {WorkspaceId} by {UserId}",
            workspace.Id,
            request.UserId);

        return workspace;
    }
}
```

**Rules:**
- Always inject dependencies via constructor
- Always use `ILogger<T>` for logging
- Use async/await for database operations
- Wrap database operations in try-catch with meaningful logging
- Return domain entities, not DTOs (let caller map)

#### ✅ AiMate.Web (Presentation Layer)

**What Goes Here:**
- Blazor components (`.razor` files)
- Fluxor state management
- Controllers (if needed)
- View models (if needed)
- UI-specific logic

**Dependencies:**
- ✅ Can reference `AiMate.Core`
- ✅ Can reference `AiMate.Infrastructure`
- ✅ Fluxor
- ✅ MudBlazor

**Rules:**
- Components should be small and focused
- Business logic goes in services, NOT components
- State goes in Fluxor, NOT component fields
- Use dependency injection for services

---

## Fluxor State Management

### The Pattern (SACRED - DO NOT DEVIATE)

Every feature follows this **exact** structure:

```
Store/
└── FeatureName/
    ├── FeatureNameState.cs     # State shape
    ├── FeatureNameActions.cs   # Actions (events)
    ├── FeatureNameReducers.cs  # Pure state transformations
    └── FeatureNameEffects.cs   # Side effects (API calls)
```

### 1. State (FeatureNameState.cs)

**Purpose:** Define the shape of state for this feature

**Rules:**
- MUST use `record` type
- MUST be immutable (`init` accessors)
- MUST have `[FeatureState]` attribute
- Initialize all collections to empty (never null)
- Keep flat - avoid deep nesting

**Example:**
```csharp
using Fluxor;

namespace AiMate.Web.Store.Chat;

[FeatureState]
public record ChatState
{
    // Single values
    public Guid? ActiveConversationId { get; init; }
    public string CurrentInput { get; init; } = string.Empty;
    public string SelectedModel { get; init; } = "gpt-4";

    // Collections (always initialized)
    public Dictionary<Guid, Conversation> Conversations { get; init; } = new();
    public List<string> AvailableModels { get; init; } = new();

    // Loading/error states
    public bool IsLoading { get; init; }
    public bool IsStreaming { get; init; }
    public string? Error { get; init; }
}
```

**Why `record`?**
- Immutability by default
- Value-based equality
- Built-in `with` syntax for updates
- Thread-safe

### 2. Actions (FeatureNameActions.cs)

**Purpose:** Define things that can happen (events)

**Rules:**
- Use `record` types
- Name with `Action` suffix
- Group related actions together (Request, Success, Failure)
- Include all necessary data
- Never include logic

**Example:**
```csharp
namespace AiMate.Web.Store.Chat;

// Pattern: Request -> Success/Failure
public record SendMessageAction(Guid ConversationId, string Content);
public record SendMessageSuccessAction(
    Guid ConversationId,
    Message UserMessage,
    Message AssistantMessage);
public record SendMessageFailureAction(string Error);

// Streaming actions
public record StreamChunkReceivedAction(Guid MessageId, string Chunk);
public record StreamCompleteAction(Guid MessageId, int TokensUsed, decimal Cost);

// Simple actions
public record LoadConversationsAction();
public record LoadConversationsSuccessAction(List<Conversation> Conversations);
public record SetActiveConversationAction(Guid ConversationId);
public record UpdateInputAction(string Input);
```

**Naming Convention:**
- `VerbNounAction` - e.g., `LoadWorkspacesAction`
- `VerbNounSuccessAction` - e.g., `LoadWorkspacesSuccessAction`
- `VerbNounFailureAction` - e.g., `LoadWorkspacesFailureAction`

### 3. Reducers (FeatureNameReducers.cs)

**Purpose:** Transform state based on actions (PURE FUNCTIONS)

**Rules:**
- MUST be pure functions (no side effects)
- MUST use `[ReducerMethod]` attribute
- MUST return new state (use `with` keyword)
- NEVER call APIs
- NEVER access external state
- NEVER use async

**Example:**
```csharp
namespace AiMate.Web.Store.Chat;

public static class ChatReducers
{
    [ReducerMethod]
    public static ChatState ReduceSendMessage(
        ChatState state,
        SendMessageAction action)
    {
        // Set loading state
        return state with
        {
            IsLoading = true,
            Error = null
        };
    }

    [ReducerMethod]
    public static ChatState ReduceSendMessageSuccess(
        ChatState state,
        SendMessageSuccessAction action)
    {
        // Get conversation, add messages, update state
        if (!state.Conversations.TryGetValue(action.ConversationId, out var conversation))
            return state;

        var updatedConversation = conversation with
        {
            Messages = conversation.Messages
                .Append(action.UserMessage)
                .Append(action.AssistantMessage)
                .ToList()
        };

        var updatedConversations = new Dictionary<Guid, Conversation>(state.Conversations)
        {
            [action.ConversationId] = updatedConversation
        };

        return state with
        {
            Conversations = updatedConversations,
            IsLoading = false,
            CurrentInput = string.Empty
        };
    }

    [ReducerMethod]
    public static ChatState ReduceSendMessageFailure(
        ChatState state,
        SendMessageFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            Error = action.Error
        };
    }

    [ReducerMethod]
    public static ChatState ReduceUpdateInput(
        ChatState state,
        UpdateInputAction action)
    {
        return state with { CurrentInput = action.Input };
    }
}
```

**Critical Rules:**
- Always use `state with { ... }` to return new state
- Never mutate existing state
- Never call services or APIs
- Keep logic simple - complex transforms should be in services

### 4. Effects (FeatureNameEffects.cs)

**Purpose:** Handle side effects (API calls, logging, etc.)

**Rules:**
- Use `[EffectMethod]` attribute
- Can be async
- Can call services
- MUST dispatch success or failure actions
- Handle exceptions gracefully
- Use `ILogger` for logging

**Example:**
```csharp
namespace AiMate.Web.Store.Chat;

public class ChatEffects
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatEffects> _logger;

    public ChatEffects(
        IChatService chatService,
        ILogger<ChatEffects> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleSendMessage(
        SendMessageAction action,
        IDispatcher dispatcher)
    {
        try
        {
            _logger.LogInformation(
                "Sending message to conversation {ConversationId}",
                action.ConversationId);

            var (userMessage, assistantMessage) = await _chatService.SendMessageAsync(
                action.ConversationId,
                action.Content);

            dispatcher.Dispatch(new SendMessageSuccessAction(
                action.ConversationId,
                userMessage,
                assistantMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send message to conversation {ConversationId}",
                action.ConversationId);

            dispatcher.Dispatch(new SendMessageFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadConversations(
        LoadConversationsAction action,
        IDispatcher dispatcher)
    {
        try
        {
            var conversations = await _chatService.GetConversationsAsync();
            dispatcher.Dispatch(new LoadConversationsSuccessAction(conversations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load conversations");
            // Could dispatch failure action if needed
        }
    }
}
```

**Critical Rules:**
- Always wrap in try-catch
- Always log errors
- Always dispatch success/failure actions
- Never modify state directly (that's what reducers do)
- Use dependency injection for all services

### Fluxor Anti-Patterns

#### ❌ DON'T: Put logic in actions
```csharp
// WRONG - Actions should be data, not logic
public record SendMessageAction(Guid ConversationId, string Content)
{
    public bool IsValid() => !string.IsNullOrWhiteSpace(Content); // NO!
}
```

#### ✅ DO: Keep actions as pure data
```csharp
// CORRECT - Just data
public record SendMessageAction(Guid ConversationId, string Content);
```

#### ❌ DON'T: Call services in reducers
```csharp
// WRONG - Reducers must be pure
[ReducerMethod]
public static ChatState ReduceSendMessage(
    ChatState state,
    SendMessageAction action)
{
    var response = await _chatService.SendAsync(...); // NO! Side effect!
    return state with { ... };
}
```

#### ✅ DO: Call services in effects
```csharp
// CORRECT - Effects handle side effects
[EffectMethod]
public async Task HandleSendMessage(
    SendMessageAction action,
    IDispatcher dispatcher)
{
    var response = await _chatService.SendAsync(...); // YES!
    dispatcher.Dispatch(new SendMessageSuccessAction(response));
}
```

#### ❌ DON'T: Mutate state
```csharp
// WRONG - Mutating existing state
[ReducerMethod]
public static ChatState ReduceAddMessage(ChatState state, AddMessageAction action)
{
    state.Messages.Add(action.Message); // NO! Mutation!
    return state;
}
```

#### ✅ DO: Return new state
```csharp
// CORRECT - Immutable update
[ReducerMethod]
public static ChatState ReduceAddMessage(ChatState state, AddMessageAction action)
{
    return state with
    {
        Messages = state.Messages.Append(action.Message).ToList()
    };
}
```

---

## Entity Design Patterns

### Entity Structure

**Every entity follows this pattern:**

```csharp
namespace AiMate.Core.Entities;

/// <summary>
/// Brief description of what this entity represents
/// </summary>
public class EntityName
{
    // 1. Primary Key (always Guid)
    public Guid Id { get; set; } = Guid.NewGuid();

    // 2. Required properties (use 'required' keyword in .NET 10)
    public required string Name { get; set; }

    // 3. Optional properties (use nullable types)
    public string? Description { get; set; }

    // 4. Enum properties (with defaults)
    public EntityType Type { get; set; } = EntityType.Default;

    // 5. Foreign keys
    public Guid UserId { get; set; }

    // 6. Navigation properties (always nullable, initialized to null or empty)
    public User? User { get; set; }
    public List<RelatedEntity> RelatedEntities { get; set; } = new();

    // 7. Complex types (dictionaries, JSON columns)
    public Dictionary<string, string> Metadata { get; set; } = new();

    // 8. Flags/Booleans
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    // 9. Timestamps (always UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### Entity Rules

**1. Always use Guid for IDs**
```csharp
// ✅ CORRECT
public Guid Id { get; set; } = Guid.NewGuid();

// ❌ WRONG
public int Id { get; set; }
public string Id { get; set; }
```

**2. Use `required` for mandatory properties (.NET 10)**
```csharp
// ✅ CORRECT - Compiler enforces this
public required string Name { get; set; }

// ❌ WRONG - Can be null!
public string Name { get; set; }
```

**3. Initialize collections to empty**
```csharp
// ✅ CORRECT - Never null
public List<Conversation> Conversations { get; set; } = new();

// ❌ WRONG - Null reference exceptions waiting to happen
public List<Conversation> Conversations { get; set; }
```

**4. Use DateTime.UtcNow (never DateTime.Now)**
```csharp
// ✅ CORRECT - Always UTC
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// ❌ WRONG - Time zone issues
public DateTime CreatedAt { get; set; } = DateTime.Now;
```

**5. Navigation properties are nullable**
```csharp
// ✅ CORRECT
public User? User { get; set; }

// ❌ WRONG - Not always loaded
public User User { get; set; }
```

---

## Service Layer Patterns

### Service Interface (in Core)

```csharp
namespace AiMate.Core.Services;

/// <summary>
/// Service for managing workspaces
/// </summary>
public interface IWorkspaceService
{
    Task<Workspace> GetByIdAsync(Guid id);
    Task<List<Workspace>> GetByUserIdAsync(Guid userId);
    Task<Workspace> CreateAsync(CreateWorkspaceRequest request);
    Task<Workspace> UpdateAsync(Guid id, UpdateWorkspaceRequest request);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
```

### Service Implementation (in Infrastructure)

```csharp
namespace AiMate.Infrastructure.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<WorkspaceService> _logger;

    public WorkspaceService(
        AiMateDbContext context,
        ILogger<WorkspaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Workspace> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting workspace {WorkspaceId}", id);

        var workspace = await _context.Workspaces
            .Include(w => w.Conversations)
            .Include(w => w.Files)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace == null)
        {
            _logger.LogWarning("Workspace {WorkspaceId} not found", id);
            throw new NotFoundException($"Workspace {id} not found");
        }

        return workspace;
    }

    public async Task<Workspace> CreateAsync(CreateWorkspaceRequest request)
    {
        _logger.LogInformation(
            "Creating workspace {Name} for user {UserId}",
            request.Name,
            request.UserId);

        var workspace = new Workspace
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            UserId = request.UserId,
            DefaultPersonality = request.DefaultPersonality
        };

        try
        {
            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Workspace {WorkspaceId} created successfully",
                workspace.Id);

            return workspace;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Failed to create workspace {Name}",
                request.Name);
            throw;
        }
    }
}
```

### Service Patterns

**1. Constructor Injection**
```csharp
// ✅ CORRECT - All dependencies injected
public WorkspaceService(
    AiMateDbContext context,
    ILogger<WorkspaceService> logger,
    IKnowledgeGraphService knowledgeGraph)
{
    _context = context;
    _logger = logger;
    _knowledgeGraph = knowledgeGraph;
}

// ❌ WRONG - Service locator anti-pattern
public WorkspaceService(IServiceProvider serviceProvider)
{
    _context = serviceProvider.GetService<AiMateDbContext>();
}
```

**2. Logging Everywhere**
```csharp
// ✅ CORRECT - Log entry, success, and errors
_logger.LogInformation("Creating workspace {Name}", request.Name);
// ... operation ...
_logger.LogInformation("Workspace {Id} created", workspace.Id);

// ❌ WRONG - No logging
public async Task<Workspace> CreateAsync(CreateWorkspaceRequest request)
{
    // Silent operation - impossible to debug
    return await _context.Workspaces.AddAsync(workspace);
}
```

**3. Meaningful Exceptions**
```csharp
// ✅ CORRECT - Custom exception with context
if (workspace == null)
    throw new NotFoundException($"Workspace {id} not found");

// ❌ WRONG - Generic exception
if (workspace == null)
    throw new Exception("Not found");
```

---

## Component Patterns (Blazor)

### Component Structure

```razor
@* WorkspaceCard.razor *@
@using AiMate.Core.Entities
@using AiMate.Core.Enums
@inject IDispatcher Dispatcher

<MudCard Class="workspace-card">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Workspace.Name</MudText>
            <MudText Typo="Typo.body2" Color="Color.Secondary">
                @Workspace.Description
            </MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                          OnClick="@OnEditClicked"
                          Size="Size.Small" />
        </CardHeaderActions>
    </MudCardHeader>
</MudCard>

@code {
    // 1. Parameters (inputs from parent)
    [Parameter, EditorRequired]
    public Workspace Workspace { get; set; } = null!;

    [Parameter]
    public EventCallback<Workspace> OnEdit { get; set; }

    // 2. Private methods
    private async Task OnEditClicked()
    {
        // Dispatch Fluxor action
        Dispatcher.Dispatch(new EditWorkspaceAction(Workspace.Id));

        // Or invoke callback
        await OnEdit.InvokeAsync(Workspace);
    }
}
```

### Component Rules

**1. Use Parameters for Inputs**
```csharp
// ✅ CORRECT
[Parameter, EditorRequired]
public Workspace Workspace { get; set; } = null!;

[Parameter]
public EventCallback<Workspace> OnEdit { get; set; }

// ❌ WRONG - No state in components
private Workspace _workspace; // Don't store state here!
```

**2. Use EventCallback for Outputs**
```csharp
// ✅ CORRECT - Type-safe callback
[Parameter]
public EventCallback<Workspace> OnWorkspaceSelected { get; set; }

private async Task SelectWorkspace(Workspace workspace)
{
    await OnWorkspaceSelected.InvokeAsync(workspace);
}

// ❌ WRONG - Action delegate (not Blazor-aware)
[Parameter]
public Action<Workspace> OnWorkspaceSelected { get; set; }
```

**3. Inject Services, Not Components**
```csharp
// ✅ CORRECT - Inject services
@inject IState<WorkspaceState> WorkspaceState
@inject IDispatcher Dispatcher

// ❌ WRONG - Don't inject other components
@inject SomeOtherComponent OtherComponent
```

**4. Keep Components Small**
```csharp
// ✅ CORRECT - Single responsibility
// WorkspaceCard.razor - displays one workspace
// WorkspaceList.razor - displays list of workspaces
// WorkspaceEditDialog.razor - edits workspace

// ❌ WRONG - God component
// WorkspacePage.razor - does EVERYTHING
```

---

## Extensibility Points

### 1. Plugins

Plugins extend aiMate functionality without modifying core code.

**Plugin Interface:**
```csharp
namespace AiMate.Core.Interfaces;

public interface IWorkspacePlugin
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Version { get; }

    Task<bool> IsEnabledAsync(Guid workspaceId);
    Task<PluginResult> ExecuteAsync(PluginContext context);
}
```

**Example Plugin:**
```csharp
namespace AiMate.Infrastructure.Plugins;

public class MessageRatingPlugin : IWorkspacePlugin
{
    public string Id => "message-rating";
    public string Name => "Message Rating";
    public string Description => "Rate AI responses";
    public string Version => "1.0.0";

    private readonly IFeedbackService _feedbackService;

    public MessageRatingPlugin(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    public async Task<bool> IsEnabledAsync(Guid workspaceId)
    {
        // Check if enabled for this workspace
        return true;
    }

    public async Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        // Plugin logic here
        var rating = context.Parameters["rating"];
        await _feedbackService.RateMessageAsync(
            context.MessageId,
            int.Parse(rating));

        return PluginResult.Success("Rating saved");
    }
}
```

**Registering Plugins:**
```csharp
// In Program.cs or ServiceRegistration
services.AddSingleton<IWorkspacePlugin, MessageRatingPlugin>();
services.AddSingleton<IWorkspacePlugin, CodeCopyPlugin>();
```

### 2. MCP Tools

MCP tools extend AI capabilities.

**Tool Interface:**
```csharp
public interface IMCPTool
{
    string Name { get; }
    string Description { get; }
    Dictionary<string, ToolParameter> Parameters { get; }

    Task<MCPToolResult> ExecuteAsync(Dictionary<string, object> args);
}
```

**Example Tool:**
```csharp
public class WebSearchTool : IMCPTool
{
    public string Name => "web_search";
    public string Description => "Search the web for information";

    public Dictionary<string, ToolParameter> Parameters => new()
    {
        ["query"] = new ToolParameter
        {
            Type = "string",
            Description = "Search query",
            Required = true
        },
        ["max_results"] = new ToolParameter
        {
            Type = "integer",
            Description = "Maximum results",
            Required = false
        }
    };

    public async Task<MCPToolResult> ExecuteAsync(
        Dictionary<string, object> args)
    {
        var query = args["query"].ToString();
        var maxResults = args.ContainsKey("max_results")
            ? int.Parse(args["max_results"].ToString())
            : 5;

        // Execute search
        var results = await SearchAsync(query, maxResults);

        return new MCPToolResult
        {
            Success = true,
            Data = results,
            Message = $"Found {results.Count} results"
        };
    }
}
```

### 3. Personalities

Add new AI personalities by extending the enum and service.

**Add to Enum:**
```csharp
// AiMate.Core/Enums/PersonalityMode.cs
public enum PersonalityMode
{
    KiwiMate,
    KiwiProfessional,
    KiwiDev,
    TeReoMaori,
    MentalHealthGuardian,
    Standard,
    CustomPersonality // <-- Add new one
}
```

**Add Prompt in PersonalityService:**
```csharp
private string GetSystemPrompt(PersonalityMode mode)
{
    return mode switch
    {
        PersonalityMode.CustomPersonality =>
            "Your custom system prompt here...",
        // ... existing cases
        _ => GetKiwiMatePrompt()
    };
}
```

---

## What NOT to Do (Anti-Patterns)

### 🚫 Architecture Violations

#### 1. Breaking Layer Boundaries

```csharp
// ❌ WRONG - Core referencing Infrastructure
namespace AiMate.Core.Services;

public interface IWorkspaceService
{
    // NO! DbContext is in Infrastructure
    Task<Workspace> GetAsync(AiMateDbContext context, Guid id);
}

// ✅ CORRECT - Clean separation
public interface IWorkspaceService
{
    Task<Workspace> GetAsync(Guid id);
}
```

#### 2. Business Logic in UI

```razor
@* ❌ WRONG - Business logic in component *@
@code {
    private async Task CreateWorkspace()
    {
        var workspace = new Workspace
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Workspaces.AddAsync(workspace);
        await _context.SaveChangesAsync(); // NO! Use service!
    }
}

@* ✅ CORRECT - Dispatch action, let service handle it *@
@code {
    private void CreateWorkspace()
    {
        Dispatcher.Dispatch(new CreateWorkspaceAction(name));
    }
}
```

#### 3. State in Components

```csharp
// ❌ WRONG - State stored in component
@code {
    private List<Workspace> _workspaces = new();

    protected override async Task OnInitializedAsync()
    {
        _workspaces = await _workspaceService.GetAllAsync();
    }
}

// ✅ CORRECT - State in Fluxor
@code {
    [Inject] IState<WorkspaceState> WorkspaceState { get; set; }

    protected override void OnInitialized()
    {
        Dispatcher.Dispatch(new LoadWorkspacesAction());
    }

    private List<Workspace> Workspaces => WorkspaceState.Value.Workspaces;
}
```

### 🚫 Fluxor Anti-Patterns

#### 1. Mutating State

```csharp
// ❌ WRONG - Mutating existing state
[ReducerMethod]
public static WorkspaceState ReduceAddWorkspace(
    WorkspaceState state,
    AddWorkspaceAction action)
{
    state.Workspaces.Add(action.Workspace); // MUTATION!
    return state;
}

// ✅ CORRECT - Return new state
[ReducerMethod]
public static WorkspaceState ReduceAddWorkspace(
    WorkspaceState state,
    AddWorkspaceAction action)
{
    return state with
    {
        Workspaces = state.Workspaces
            .Append(action.Workspace)
            .ToList()
    };
}
```

#### 2. Side Effects in Reducers

```csharp
// ❌ WRONG - API call in reducer
[ReducerMethod]
public static async Task<WorkspaceState> ReduceLoadWorkspaces(
    WorkspaceState state,
    LoadWorkspacesAction action)
{
    var workspaces = await _service.GetAllAsync(); // NO!
    return state with { Workspaces = workspaces };
}

// ✅ CORRECT - Side effects in effects
[EffectMethod]
public async Task HandleLoadWorkspaces(
    LoadWorkspacesAction action,
    IDispatcher dispatcher)
{
    var workspaces = await _service.GetAllAsync(); // YES!
    dispatcher.Dispatch(new LoadWorkspacesSuccessAction(workspaces));
}
```

### 🚫 Entity Anti-Patterns

#### 1. Using int IDs

```csharp
// ❌ WRONG - Sequential IDs are guessable
public class Workspace
{
    public int Id { get; set; }
}

// ✅ CORRECT - GUIDs are unique and secure
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```

#### 2. Nullable Collections

```csharp
// ❌ WRONG - Can be null
public List<Conversation>? Conversations { get; set; }

// Causes null reference exceptions:
workspace.Conversations.Add(conversation); // BOOM!

// ✅ CORRECT - Never null
public List<Conversation> Conversations { get; set; } = new();

// Always safe:
workspace.Conversations.Add(conversation); // Works!
```

#### 3. Local DateTime

```csharp
// ❌ WRONG - Time zone hell
public DateTime CreatedAt { get; set; } = DateTime.Now;

// ✅ CORRECT - Always UTC
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

### 🚫 Service Anti-Patterns

#### 1. Service Locator Pattern

```csharp
// ❌ WRONG - Service locator (anti-pattern)
public class WorkspaceService
{
    private readonly IServiceProvider _serviceProvider;

    public WorkspaceService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DoSomething()
    {
        var dbContext = _serviceProvider.GetService<AiMateDbContext>();
    }
}

// ✅ CORRECT - Constructor injection
public class WorkspaceService
{
    private readonly AiMateDbContext _context;

    public WorkspaceService(AiMateDbContext context)
    {
        _context = context;
    }
}
```

#### 2. No Exception Handling

```csharp
// ❌ WRONG - No error handling
public async Task<Workspace> CreateAsync(CreateWorkspaceRequest request)
{
    var workspace = new Workspace { Name = request.Name };
    await _context.Workspaces.AddAsync(workspace);
    await _context.SaveChangesAsync(); // Could throw!
    return workspace;
}

// ✅ CORRECT - Proper error handling
public async Task<Workspace> CreateAsync(CreateWorkspaceRequest request)
{
    try
    {
        var workspace = new Workspace { Name = request.Name };
        await _context.Workspaces.AddAsync(workspace);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Workspace {Id} created", workspace.Id);
        return workspace;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Failed to create workspace");
        throw new ServiceException("Failed to create workspace", ex);
    }
}
```

---

## Testing Patterns

### Unit Tests

```csharp
public class WorkspaceServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesWorkspace()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AiMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AiMateDbContext(options);
        var logger = Mock.Of<ILogger<WorkspaceService>>();
        var service = new WorkspaceService(context, logger);

        var request = new CreateWorkspaceRequest
        {
            Name = "Test Workspace",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Workspace");
        result.Id.Should().NotBeEmpty();
    }
}
```

### Integration Tests

```csharp
public class ChatApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChatApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendMessage_ValidRequest_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SendMessageRequest
        {
            ConversationId = Guid.NewGuid(),
            Content = "Hello, aiMate!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/chat/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## Performance Considerations

### 1. Database Queries

```csharp
// ❌ WRONG - N+1 query problem
var workspaces = await _context.Workspaces.ToListAsync();
foreach (var workspace in workspaces)
{
    // This hits DB for each workspace!
    var conversations = await _context.Conversations
        .Where(c => c.WorkspaceId == workspace.Id)
        .ToListAsync();
}

// ✅ CORRECT - Single query with Include
var workspaces = await _context.Workspaces
    .Include(w => w.Conversations)
    .ToListAsync();
```

### 2. Async/Await

```csharp
// ❌ WRONG - Blocking async call
public Workspace GetById(Guid id)
{
    return _context.Workspaces.FindAsync(id).Result; // Deadlock risk!
}

// ✅ CORRECT - Async all the way
public async Task<Workspace> GetByIdAsync(Guid id)
{
    return await _context.Workspaces.FindAsync(id);
}
```

### 3. Lazy Loading

```csharp
// ❌ WRONG - Lazy loading in loop
foreach (var workspace in workspaces)
{
    // Lazy loads on each access - N+1 problem
    Console.WriteLine(workspace.User.Name);
}

// ✅ CORRECT - Eager loading
var workspaces = await _context.Workspaces
    .Include(w => w.User)
    .ToListAsync();

foreach (var workspace in workspaces)
{
    Console.WriteLine(workspace.User.Name);
}
```

---

## Summary: The Sacred Rules

### 🔴 NEVER Break These

1. **Never mix architectural layers** - Core has no dependencies
2. **Never mutate state in reducers** - Always return new state
3. **Never put business logic in components** - Use services
4. **Never use `DateTime.Now`** - Always `DateTime.UtcNow`
5. **Never leave collections nullable** - Initialize to empty
6. **Never use service locator** - Constructor injection only
7. **Never skip error handling** - Try-catch + logging
8. **Never use int IDs** - GUIDs only

### 🟡 Strongly Recommended

1. Use `record` for DTOs and state
2. Use `required` for mandatory properties
3. Log all important operations
4. Use meaningful exception messages
5. Keep components small and focused
6. Write XML comments for public APIs
7. Use async/await consistently
8. Include eager loading to avoid N+1

### 🟢 Best Practices

1. Use FluentValidation for validation
2. Use AutoMapper for object mapping (if needed)
3. Use feature folders for organization
4. Write integration tests for critical paths
5. Use guard clauses for validation
6. Prefer composition over inheritance
7. Use dependency injection extensively

---

## Questions?

If you're unsure about anything:

1. Check this document first
2. Look at existing code for patterns
3. Ask in discussions or issues
4. When in doubt, follow the simplest, most explicit approach

**Remember:** Predictability > Cleverness

**Built with ❤️ and clean architecture** 🏗️🇳🇿
