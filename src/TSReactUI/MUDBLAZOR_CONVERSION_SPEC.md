# MudBlazor Conversion Specification
## React to Blazor Migration Guide for AI Chat Interface

**Document Version:** 1.0  
**Date:** November 13, 2025  
**Source:** React/TypeScript Application  
**Target:** Blazor Server/.NET Core 9 with MudBlazor

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Application Architecture](#application-architecture)
3. [Technology Stack Mapping](#technology-stack-mapping)
4. [Component Mapping](#component-mapping)
5. [State Management Conversion](#state-management-conversion)
6. [Data Models & Interfaces](#data-models--interfaces)
7. [API Integration Strategy](#api-integration-strategy)
8. [Styling & Theming](#styling--theming)
9. [Feature Implementation Guide](#feature-implementation-guide)
10. [Migration Roadmap](#migration-roadmap)

---

## Executive Summary

This document provides a comprehensive guide for converting the **AI Chat Interface** from React/TypeScript to Blazor Server with MudBlazor. The application is a full-featured AI work studio with 73 total components, knowledge management, conversation history, and multi-modal capabilities.

### Key Statistics
- **Total Components:** 73 (28 custom + 45 UI library components)
- **Lines of Code (Core):** ~15,000+
- **Features:** 15+ major feature areas
- **API Endpoints:** 10+ stub endpoints ready for .NET integration
- **Theme:** Purple-to-blue gradient with dark/light mode

---

## Application Architecture

### Current React Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         App.tsx                              │
│  (Main Component - Conversation & Message Management)        │
│                                                               │
│  State Management:                                           │
│  • conversations, activeConversationId                       │
│  • isTyping, sidebarOpen, mobileSidebarOpen                 │
│  • selectedModel, enabledModels                              │
│                                                               │
└───────────────────┬─────────────────────────────────────────┘
                    │
        ┌───────────┴──────────────────┐
        │                              │
┌───────▼────────┐          ┌──────────▼──────────┐
│  ThemeProvider │          │   DebugProvider     │
│  (Dark/Light)  │          │   (API Logging)     │
└───────┬────────┘          └──────────┬──────────┘
        │                              │
        └───────────┬──────────────────┘
                    │
        ┌───────────┴──────────────────────────────┐
        │                                          │
┌───────▼───────────┐                  ┌──────────▼──────────┐
│ ConversationSidebar│                 │   Main Chat Area    │
│ - Navigation       │                 │ - ChatHeader        │
│ - Project List     │                 │ - ScrollArea        │
│ - User Menu        │                 │   - ChatMessage[]   │
│ - Settings Modal   │                 │   - EmptyState      │
│ - Admin Modal      │                 │ - ChatInput         │
│ - Search Modal     │                 │ - DebugPanel        │
│ - Knowledge Modal  │                 │                     │
│ - Notes Modal      │                 │                     │
│ - Files Modal      │                 │                     │
│ - Projects Modal   │                 │                     │
│ - Share Modal      │                 │                     │
└───────────────────┘                  └─────────────────────┘
```

### Proposed Blazor Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    MainLayout.razor                          │
│           (MudLayout with MudDrawer + MudAppBar)             │
│                                                               │
└───────────────────┬─────────────────────────────────────────┘
                    │
        ┌───────────┴──────────────────┐
        │                              │
┌───────▼────────┐          ┌──────────▼──────────┐
│ ThemeProvider  │          │   StateContainer    │
│ (MudTheme)     │          │   (Scoped Service)  │
└───────┬────────┘          └──────────┬──────────┘
        │                              │
        └───────────┬──────────────────┘
                    │
        ┌───────────┴──────────────────────────────┐
        │                                          │
┌───────▼───────────┐                  ┌──────────▼──────────┐
│ ConversationDrawer │                 │   Chat.razor        │
│ (MudDrawer)        │                 │ - ChatHeader.razor  │
│ Components:        │                 │ - MudScrollToBottom │
│ - NavMenu.razor    │                 │ - ChatMessages      │
│ - ProjectList      │                 │   (foreach)         │
│ - UserMenu         │                 │ - ChatInput.razor   │
│ - MudDialog x8     │                 │ - DebugPanel.razor  │
│   (Modals)         │                 │                     │
└───────────────────┘                  └─────────────────────┘
```

---

## Technology Stack Mapping

### Core Technologies

| React/TypeScript | Blazor/.NET | Notes |
|-----------------|-------------|-------|
| **React 18.3.1** | **Blazor Server (.NET 9)** | Component-based UI framework |
| **TypeScript 5.5** | **C# 12** | Strongly-typed language |
| **Vite** | **Webpack/MSBuild** | Build system (built into .NET) |
| **npm/package.json** | **NuGet** | Package management |
| **TSX files** | **Razor files (.razor)** | Component markup |

### UI Component Libraries

| React | MudBlazor | Component Type |
|-------|-----------|----------------|
| **Radix UI** | **MudBlazor** | Primitive components |
| **shadcn/ui** | **MudBlazor** | Pre-built components |
| **lucide-react** | **MudBlazor.Icons.Material** | Icon library |
| **Tailwind CSS** | **MudBlazor + Custom CSS** | Styling system |

### State Management

| React | Blazor | Purpose |
|-------|--------|---------|
| `useState` | `private field` + `StateHasChanged()` | Local component state |
| `useEffect` | `OnInitializedAsync`, `OnAfterRenderAsync` | Lifecycle hooks |
| `useRef` | `ElementReference` | DOM references |
| `useContext` | `CascadingParameter`, `DI Services` | Shared state |
| Props | `[Parameter]` attributes | Component parameters |
| Custom hooks | Service classes | Reusable logic |

### Key Dependencies

| React Package | .NET/MudBlazor Equivalent |
|---------------|---------------------------|
| `react-markdown` | `Markdig` NuGet package |
| `sonner` (toasts) | `MudBlazor.Snackbar` |
| `clsx` / `tailwind-merge` | C# string interpolation |
| `date-fns` | `.NET DateTime` / `TimeSpan` |
| `recharts` | `MudBlazor.Charts` or `ApexCharts.Blazor` |
| `vaul` (drawer) | `MudDrawer` |

---

## Component Mapping

### Complete Component Inventory

#### Custom Components (28)

| # | React Component | MudBlazor Equivalent | Notes |
|---|----------------|---------------------|-------|
| 1 | **App.tsx** | **Chat.razor** (Page) | Main application page |
| 2 | **ConversationSidebar.tsx** | **ConversationDrawer.razor** | MudDrawer component |
| 3 | **ChatMessage.tsx** | **ChatMessage.razor** | Message bubble component |
| 4 | **ChatInput.tsx** | **ChatInput.razor** | Complex input with attachments |
| 5 | **ChatHeader.tsx** | **ChatHeader.razor** | Top app bar content |
| 6 | **EmptyState.tsx** | **EmptyState.razor** | Welcome screen |
| 7 | **AttachedContext.tsx** | **AttachedContext.razor** | Context chips display |
| 8 | **KnowledgeSuggestions.tsx** | **KnowledgeSuggestions.razor** | Smart suggestions |
| 9 | **StructuredPanel.tsx** | **StructuredPanel.razor** | Data visualization panel |
| 10 | **SettingsModal.tsx** | **SettingsDialog.razor** | MudDialog with tabs |
| 11 | **AdminModal.tsx** | **AdminDialog.razor** | MudDialog for admin |
| 12 | **AboutModal.tsx** | **AboutDialog.razor** | MudDialog |
| 13 | **ProjectModal.tsx** | **ProjectDialog.razor** | MudDialog |
| 14 | **SearchModal.tsx** | **SearchDialog.razor** | MudDialog |
| 15 | **NotesModal.tsx** | **NotesDialog.razor** | MudDialog |
| 16 | **KnowledgeModal.tsx** | **KnowledgeDialog.razor** | MudDialog |
| 17 | **FilesModal.tsx** | **FilesDialog.razor** | MudDialog |
| 18 | **ArchivedModal.tsx** | **ArchivedDialog.razor** | MudDialog |
| 19 | **ShareModal.tsx** | **ShareDialog.razor** | MudDialog |
| 20 | **ShareDialog.tsx** | **ShareDialog.razor** | MudDialog variant |
| 21 | **RatingModal.tsx** | **RatingDialog.razor** | MudDialog |
| 22 | **BaseModal.tsx** | **BaseDialog.razor** | Dialog wrapper |
| 23 | **BaseDialog.tsx** | **BaseDialog.razor** | Dialog wrapper |
| 24 | **ConnectionEditDialog.tsx** | **ConnectionEditDialog.razor** | MudDialog |
| 25 | **ModelEditDialog.tsx** | **ModelEditDialog.razor** | MudDialog |
| 26 | **MCPEditDialog.tsx** | **MCPEditDialog.razor** | MudDialog |
| 27 | **PluginEditDialog.tsx** | **PluginEditDialog.razor** | MudDialog |
| 28 | **UsageDetailsDialog.tsx** | **UsageDetailsDialog.razor** | MudDialog |
| 29 | **DebugContext.tsx** | **DebugService.cs** | Scoped service |
| 30 | **DebugPanel.tsx** | **DebugPanel.razor** | Debug overlay |
| 31 | **ThemeProvider.tsx** | **MudThemeProvider + Service** | Built-in |

#### Radix/Shadcn UI to MudBlazor Mapping (45 components)

| React Component | MudBlazor Component | Properties |
|----------------|---------------------|------------|
| `Accordion` | `MudExpansionPanels` | `MultiExpansion`, `Elevation` |
| `AlertDialog` | `MudDialog` | `Options.CloseButton`, `MaxWidth` |
| `Alert` | `MudAlert` | `Severity`, `Variant` |
| `Avatar` | `MudAvatar` | `Image`, `Color`, `Size` |
| `Badge` | `MudBadge` | `Content`, `Color`, `Overlap` |
| `Breadcrumb` | `MudBreadcrumbs` | `Items`, `Separator` |
| `Button` | `MudButton` | `Variant`, `Color`, `StartIcon` |
| `Calendar` | `MudDatePicker` | `Date`, `DateFormat` |
| `Card` | `MudCard`, `MudCardContent` | `Elevation`, `Outlined` |
| `Carousel` | `MudCarousel` | `AutoCycle`, `ShowArrows` |
| `Checkbox` | `MudCheckBox` | `Checked`, `Label` |
| `Collapsible` | `MudCollapse` | `Expanded` |
| `Command` | `MudAutocomplete` | Custom implementation |
| `ContextMenu` | `MudMenu` | `ActivatorContent` |
| `Dialog` | `MudDialog` | `IsVisible`, `Options` |
| `Drawer/Sheet` | `MudDrawer` | `Open`, `Anchor`, `Variant` |
| `DropdownMenu` | `MudMenu` | `Icon`, `Label` |
| `HoverCard` | `MudPopover` | `Open`, `AnchorOrigin` |
| `Input` | `MudTextField` | `Variant`, `Label`, `Required` |
| `Label` | `MudText` | Prop on MudTextField |
| `Menubar` | `MudAppBar` + `MudMenu` | Custom layout |
| `NavigationMenu` | `MudNavMenu` | `NavLink` items |
| `Pagination` | `MudPagination` | `Count`, `Selected` |
| `Popover` | `MudPopover` | `Open`, `AnchorOrigin` |
| `Progress` | `MudProgressLinear` | `Value`, `Color` |
| `RadioGroup` | `MudRadioGroup` | `SelectedOption` |
| `ScrollArea` | `MudScrollToBottom` or CSS | `overflow-y: auto` |
| `Select` | `MudSelect` | `Value`, `Items` |
| `Separator` | `MudDivider` | `DividerType`, `Class` |
| `Skeleton` | `MudSkeleton` | `SkeletonType`, `Animation` |
| `Slider` | `MudSlider` | `Value`, `Min`, `Max` |
| `Switch` | `MudSwitch` | `Checked`, `Label` |
| `Table` | `MudTable`, `MudTd`, `MudTh` | `Items`, `Dense` |
| `Tabs` | `MudTabs`, `MudTabPanel` | `ActivePanelIndex` |
| `Textarea` | `MudTextField` | `Lines`, `AutoGrow` |
| `Toast (Sonner)` | `MudSnackbar` | `Configuration`, `Add()` |
| `Toggle` | `MudToggleIconButton` | `Icon`, `ToggledIcon` |
| `Tooltip` | `MudTooltip` | `Text`, `Placement` |

---

## Data Models & Interfaces

### TypeScript → C# Conversion

#### Message Model

**TypeScript (React):**
```typescript
interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: string;
  structuredContent?: any;
}
```

**C# (Blazor):**
```csharp
public class Message
{
    public string Id { get; set; } = string.Empty;
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public StructuredContent? StructuredContent { get; set; }
}

public enum MessageRole
{
    User,
    Assistant
}
```

#### Conversation Model

**TypeScript:**
```typescript
interface Conversation {
  id: string;
  title: string;
  lastMessage: string;
  timestamp: Date;
}

interface ConversationData {
  id: string;
  messages: Message[];
  title: string;
  createdAt: Date;
}
```

**C#:**
```csharp
public class Conversation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ConversationData
{
    public string Id { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

#### AttachedContext Models

**TypeScript:**
```typescript
interface AttachedItem {
  id: string;
  name: string;
  type: "file" | "image" | "document" | "note" | "knowledge" | "chat" | "webpage";
  size?: string;
  preview?: string;
}
```

**C#:**
```csharp
public class AttachedItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AttachmentType Type { get; set; }
    public string? Size { get; set; }
    public string? Preview { get; set; }
}

public enum AttachmentType
{
    File,
    Image,
    Document,
    Note,
    Knowledge,
    Chat,
    Webpage
}
```

#### Knowledge Item Model

**TypeScript:**
```typescript
interface KnowledgeItem {
  id: string;
  title: string;
  type: string;
  url: string;
  date: string;
  size: string;
  tags: string[];
  summary: string;
  entities: string[];
  collection: string;
  usageCount: number;
  lastUsed?: string;
  source: string;
  version: number;
  updatedBy: string;
}
```

**C#:**
```csharp
public class KnowledgeItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Size { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public string Collection { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime? LastUsed { get; set; }
    public string Source { get; set; } = string.Empty;
    public int Version { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
```

#### Structured Content Models

**TypeScript:**
```typescript
interface StructuredData {
  type: "panel.table" | "panel.list" | "panel.form" | "panel.kv";
  title: string;
  columns?: string[];
  rows?: any[][];
  items?: Array<{ title: string; content: string }>;
  fields?: Array<{ 
    name: string; 
    label: string; 
    type: string; 
    value: string; 
    required?: boolean 
  }>;
  kvPairs?: Array<{ key: string; value: string }>;
  rowActions?: StructuredAction[];
  actions?: StructuredAction[];
  submit?: StructuredAction;
}
```

**C#:**
```csharp
public class StructuredData
{
    public StructuredPanelType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string>? Columns { get; set; }
    public List<List<object>>? Rows { get; set; }
    public List<ListItem>? Items { get; set; }
    public List<FormField>? Fields { get; set; }
    public List<KeyValuePair<string, string>>? KvPairs { get; set; }
    public List<StructuredAction>? RowActions { get; set; }
    public List<StructuredAction>? Actions { get; set; }
    public StructuredAction? Submit { get; set; }
}

public enum StructuredPanelType
{
    Table,
    List,
    Form,
    KeyValue
}

public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Required { get; set; }
}

public class ListItem
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class StructuredAction
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Tool { get; set; }
    public Dictionary<string, object>? Args { get; set; }
}
```

---

## State Management Conversion

### React Hooks → Blazor Patterns

#### 1. Local State (useState)

**React:**
```typescript
const [isTyping, setIsTyping] = useState(false);
const [sidebarOpen, setSidebarOpen] = useState(true);
const [selectedModel, setSelectedModel] = useState("gpt-4");
```

**Blazor:**
```csharp
private bool isTyping = false;
private bool sidebarOpen = true;
private string selectedModel = "gpt-4";

private void SetIsTyping(bool value)
{
    isTyping = value;
    StateHasChanged();
}
```

#### 2. Effect Hook (useEffect)

**React:**
```typescript
useEffect(() => {
  scrollToBottom();
}, [messages, isTyping]);

useEffect(() => {
  const handleResize = () => {
    if (window.innerWidth < 1024 && sidebarOpen) {
      setSidebarOpen(false);
    }
  };
  window.addEventListener('resize', handleResize);
  return () => window.removeEventListener('resize', handleResize);
}, [sidebarOpen]);
```

**Blazor:**
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    await ScrollToBottom();
}

protected override async Task OnInitializedAsync()
{
    await JSRuntime.InvokeVoidAsync("window.addEventListener", "resize", 
        DotNetObjectReference.Create(this), nameof(HandleResize));
}

[JSInvokable]
public void HandleResize()
{
    // Handle resize logic
    StateHasChanged();
}
```

#### 3. Context (useContext)

**React:**
```typescript
// DebugContext.tsx
export const DebugContext = createContext<DebugContextType | undefined>(undefined);

export function useDebug() {
  const context = useContext(DebugContext);
  if (!context) throw new Error("useDebug must be used within DebugProvider");
  return context;
}

// Usage
const { addLog } = useDebug();
addLog({ action: 'Message sent', api: '/api/v1/chat', type: 'success' });
```

**Blazor:**
```csharp
// Services/DebugService.cs
public class DebugService
{
    public List<DebugLog> Logs { get; } = new();
    
    public event Action? OnChange;
    
    public void AddLog(DebugLog log)
    {
        Logs.Add(log);
        OnChange?.Invoke();
    }
}

// Program.cs
builder.Services.AddScoped<DebugService>();

// Component usage
@inject DebugService DebugService

@code {
    protected override void OnInitialized()
    {
        DebugService.OnChange += StateHasChanged;
    }
    
    private void LogMessage()
    {
        DebugService.AddLog(new DebugLog 
        { 
            Action = "Message sent", 
            Api = "/api/v1/chat", 
            Type = LogType.Success 
        });
    }
}
```

### Global State Management

**Recommended Approach: Scoped Services**

```csharp
// Services/ConversationStateService.cs
public class ConversationStateService
{
    private List<ConversationData> _conversations = new();
    private string? _activeConversationId;
    
    public event Action? OnChange;
    
    public List<ConversationData> Conversations => _conversations;
    public string? ActiveConversationId => _activeConversationId;
    
    public void SetActiveConversation(string id)
    {
        _activeConversationId = id;
        NotifyStateChanged();
    }
    
    public void AddMessage(string conversationId, Message message)
    {
        var conv = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conv != null)
        {
            conv.Messages.Add(message);
            NotifyStateChanged();
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

---

## API Integration Strategy

### API Stub Structure

The React app uses `/utils/api-stubs.ts` with this pattern:

```typescript
export async function getKnowledge(userId: string): Promise<ApiResponse<any[]>> {
  await delay();
  return {
    success: true,
    data: [...],
    metadata: { endpoint: `/api/v1/GetKnowledge?UserID=${userId}`, ... }
  };
}
```

### .NET Core API Controller Pattern

```csharp
// Controllers/KnowledgeController.cs
[ApiController]
[Route("api/v1/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    
    public KnowledgeController(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<KnowledgeItem>>>> GetKnowledge(
        [FromQuery] string userId)
    {
        try
        {
            var knowledge = await _knowledgeService.GetKnowledgeAsync(userId);
            return Ok(new ApiResponse<List<KnowledgeItem>>
            {
                Success = true,
                Data = knowledge,
                Metadata = new ResponseMetadata
                {
                    Endpoint = $"/api/v1/Knowledge?UserID={userId}",
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<List<KnowledgeItem>>
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}
```

### API Endpoints to Implement

| Endpoint | Method | Purpose | React Stub Reference |
|----------|--------|---------|---------------------|
| `/api/v1/GetChats` | GET | Get conversation history | `getChats()` |
| `/api/v1/GetNotes` | GET | Get user notes | `getNotes()` |
| `/api/v1/GetKnowledge` | GET | Get knowledge items | `getKnowledge()` |
| `/api/v1/GetFiles` | GET | Get uploaded files | `getFiles()` |
| `/api/v1/AttachFile` | POST | Attach file to chat | `attachFile()` |
| `/api/v1/AttachNote` | POST | Attach note to chat | `attachNote()` |
| `/api/v1/AttachKnowledge` | POST | Attach knowledge to chat | `attachKnowledge()` |
| `/api/v1/ReferenceChat` | POST | Reference another chat | `referenceChat()` |
| `/api/v1/AttachWebpage` | POST | Attach webpage URL | `attachWebpage()` |
| `/api/v1/TestConnection` | POST | Test AI model connection | `testConnection()` |
| `/api/v1/SendMessage` | POST | Send chat message | Inline in `handleSendMessage` |
| `/api/v1/CreateConversation` | POST | Create new conversation | Inline in `createNewConversation` |
| `/api/v1/DeleteConversation` | DELETE | Delete conversation | Inline in `handleDeleteConversation` |
| `/api/v1/SaveUserSettings` | POST | Save settings | In SettingsModal |

### HttpClient Service Pattern (Blazor)

```csharp
// Services/ChatApiService.cs
public class ChatApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatApiService> _logger;
    
    public ChatApiService(HttpClient httpClient, ILogger<ChatApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ApiResponse<List<KnowledgeItem>>> GetKnowledgeAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/GetKnowledge?UserID={userId}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<KnowledgeItem>>>();
            return result ?? new ApiResponse<List<KnowledgeItem>> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching knowledge");
            return new ApiResponse<List<KnowledgeItem>> 
            { 
                Success = false, 
                Error = ex.Message 
            };
        }
    }
}

// Program.cs registration
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5000") 
});
builder.Services.AddScoped<ChatApiService>();
```

---

## Styling & Theming

### Tailwind to MudBlazor Theme Conversion

#### Color Palette

**Current Tailwind Gradient (React):**
```css
:root {
  --gradient-from: rgb(147, 51, 234);  /* purple-600 */
  --gradient-to: rgb(59, 130, 246);    /* blue-500 */
}
```

**MudBlazor Theme (C#):**
```csharp
private MudTheme _customTheme = new()
{
    Palette = new PaletteLight
    {
        Primary = "#9333EA",        // purple-600
        Secondary = "#3B82F6",      // blue-500
        AppbarBackground = "#030213",
        Background = "#FFFFFF",
        Surface = "#FFFFFF",
        DrawerBackground = "#F9FAFB",
        DrawerText = "rgba(0,0,0, 0.87)",
        Success = "#10B981",
        Info = "#3B82F6",
        Warning = "#F59E0B",
        Error = "#EF4444",
        Dark = "#030213",
        TextPrimary = "rgba(0,0,0, 0.87)",
        TextSecondary = "rgba(0,0,0, 0.60)",
        Divider = "rgba(0,0,0, 0.12)"
    },
    PaletteDark = new PaletteDark
    {
        Primary = "#9333EA",
        Secondary = "#3B82F6",
        AppbarBackground = "#030213",
        Background = "#030213",
        Surface = "#1F1F1F",
        DrawerBackground = "#030213",
        DrawerText = "rgba(255,255,255, 0.87)",
        Success = "#10B981",
        Info = "#3B82F6",
        Warning = "#F59E0B",
        Error = "#EF4444",
        Dark = "#030213",
        TextPrimary = "rgba(255,255,255, 0.87)",
        TextSecondary = "rgba(255,255,255, 0.60)",
        Divider = "rgba(255,255,255, 0.12)"
    },
    Typography = new Typography
    {
        Default = new Default
        {
            FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
            FontSize = "14px",
            FontWeight = 400,
            LineHeight = 1.5
        },
        H1 = new H1 { FontSize = "2rem", FontWeight = 500 },
        H2 = new H2 { FontSize = "1.5rem", FontWeight = 500 },
        H3 = new H3 { FontSize = "1.25rem", FontWeight = 500 },
        H4 = new H4 { FontSize = "1rem", FontWeight = 500 },
        Body1 = new Body1 { FontSize = "14px", FontWeight = 400 },
        Button = new Button { FontSize = "14px", FontWeight = 500 }
    }
};
```

#### Component-Specific Styling

**AttachedContext Color-Coded Chips**

**React (Tailwind):**
```tsx
const getTypeConfig = (type: 'knowledge' | 'note' | 'file' | 'chat' | 'webpage') => {
  switch (type) {
    case 'knowledge':
      return {
        colorClass: 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300'
      };
    // ... more cases
  }
};
```

**Blazor (MudBlazor):**
```razor
@code {
    private Color GetChipColor(AttachmentType type) => type switch
    {
        AttachmentType.Knowledge => Color.Primary,      // Purple
        AttachmentType.Note => Color.Info,              // Blue
        AttachmentType.File => Color.Success,           // Green
        AttachmentType.Webpage => Color.Tertiary,       // Cyan
        AttachmentType.Chat => Color.Warning,           // Orange
        _ => Color.Default
    };
}

<MudChip Color="@GetChipColor(item.Type)" 
         Variant="Variant.Outlined" 
         Size="Size.Small"
         OnClose="@(() => OnDetach(item.Id))">
    @item.Title
</MudChip>
```

### Custom CSS for Gradient Effects

**wwwroot/css/app.css:**
```css
.gradient-primary {
    background: linear-gradient(to right, #9333EA, #3B82F6);
}

.ai-avatar {
    background: linear-gradient(to right, #9333EA, #3B82F6);
    animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.75; }
}

.mud-drawer-responsive {
    width: 320px !important;
}

@media (max-width: 768px) {
    .mud-drawer-responsive {
        width: 100% !important;
    }
}
```

---

## Feature Implementation Guide

### Feature 1: Chat Message Component

**Complexity:** Medium  
**Dependencies:** Markdig (for markdown), MudBlazor

**React Implementation:**
```tsx
<ChatMessage
  role={message.role}
  content={message.content}
  timestamp={message.timestamp}
  structuredContent={message.structuredContent}
  onEdit={(newContent) => handleEditMessage(message.id, newContent)}
  onRegenerate={handleRegenerateResponse}
/>
```

**Blazor Implementation:**

```razor
<!-- ChatMessage.razor -->
@using Markdig

<div class="d-flex gap-3 @(IsUser ? "flex-row-reverse" : "flex-row") mud-message-group">
    <MudAvatar Color="@(IsUser ? Color.Default : Color.Primary)" Size="Size.Small">
        @if (IsUser)
        {
            <MudIcon Icon="@Icons.Material.Filled.Person" />
        }
        else
        {
            <MudIcon Icon="@Icons.Material.Filled.AutoAwesome" Class="gradient-primary" />
        }
    </MudAvatar>

    <div class="flex-grow-1" style="max-width: 80%;">
        @if (isEditing)
        {
            <MudTextField @bind-Value="editedContent" 
                          Lines="5" 
                          Variant="Variant.Outlined"
                          AutoGrow />
            <div class="d-flex gap-2 mt-2">
                <MudButton StartIcon="@Icons.Material.Filled.Send" 
                           OnClick="HandleSaveEdit" 
                           Color="Color.Primary" 
                           Size="Size.Small">
                    Submit
                </MudButton>
                <MudButton StartIcon="@Icons.Material.Filled.Close" 
                           OnClick="HandleCancelEdit" 
                           Variant="Variant.Outlined" 
                           Size="Size.Small">
                    Cancel
                </MudButton>
            </div>
        }
        else
        {
            <MudPaper Elevation="1" Class="pa-3 @(IsUser ? "bg-gray-700" : "bg-gray-100")">
                @if (IsUser)
                {
                    <MudText>@Content</MudText>
                }
                else
                {
                    @((MarkupString)Markdown.ToHtml(Content))
                }
            </MudPaper>

            @if (StructuredContent != null)
            {
                <StructuredPanel Data="StructuredContent" OnAction="HandleAction" />
            }

            @if (!IsUser)
            {
                <div class="d-flex gap-1 mt-2">
                    <MudIconButton Icon="@Icons.Material.Filled.ContentCopy" 
                                   Size="Size.Small" 
                                   OnClick="HandleCopy" 
                                   Title="Copy" />
                    <MudIconButton Icon="@Icons.Material.Filled.VolumeUp" 
                                   Size="Size.Small" 
                                   OnClick="HandleReadAloud" 
                                   Title="Read Aloud" />
                    <MudIconButton Icon="@Icons.Material.Filled.ThumbUp" 
                                   Size="Size.Small" 
                                   OnClick="HandleLike" />
                    <MudIconButton Icon="@Icons.Material.Filled.ThumbDown" 
                                   Size="Size.Small" 
                                   OnClick="HandleDislike" />
                    @if (OnRegenerate != null)
                    {
                        <MudIconButton Icon="@Icons.Material.Filled.Refresh" 
                                       Size="Size.Small" 
                                       OnClick="OnRegenerate" 
                                       Title="Regenerate" />
                    }
                </div>
            }
        }
    </div>
</div>

@code {
    [Parameter] public MessageRole Role { get; set; }
    [Parameter] public string Content { get; set; } = string.Empty;
    [Parameter] public DateTime Timestamp { get; set; }
    [Parameter] public StructuredData? StructuredContent { get; set; }
    [Parameter] public EventCallback<string> OnEdit { get; set; }
    [Parameter] public EventCallback OnRegenerate { get; set; }

    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool IsUser => Role == MessageRole.User;
    private bool isEditing = false;
    private string editedContent = string.Empty;

    private async Task HandleCopy()
    {
        // Copy to clipboard using JS interop
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Content);
        Snackbar.Add("Copied to clipboard", Severity.Success);
    }

    private void HandleSaveEdit()
    {
        if (!string.IsNullOrWhiteSpace(editedContent))
        {
            OnEdit.InvokeAsync(editedContent);
            isEditing = false;
        }
    }
}
```

### Feature 2: AttachedContext Component

**Complexity:** Medium  
**Dependencies:** MudBlazor

**Blazor Implementation:**

```razor
<!-- AttachedContext.razor -->
<MudCollapse Expanded="@isOpen">
    <div class="d-flex justify-space-between align-center mb-2">
        <MudButton OnClick="@(() => isOpen = !isOpen)" 
                   StartIcon="@Icons.Material.Filled.Psychology" 
                   Color="Color.Primary" 
                   Variant="Variant.Text" 
                   Size="Size.Small">
            Smart Context
            <MudIcon Icon="@(isOpen ? Icons.Material.Filled.ExpandLess : Icons.Material.Filled.ExpandMore)" 
                     Size="Size.Small" 
                     Class="ml-2" />
        </MudButton>
        <MudText Typo="Typo.caption" Color="Color.Secondary">
            @TotalAttached attached
        </MudText>
    </div>

    <MudPaper Elevation="0" Class="pa-3 d-flex flex-wrap gap-2" Style="background-color: var(--mud-palette-background-grey);">
        @foreach (var item in AllAttachedItems)
        {
            <MudChip Color="@GetChipColor(item.Type)" 
                     Variant="Variant.Outlined" 
                     Size="Size.Small"
                     OnClose="@(() => item.OnDetach.Invoke())"
                     Icon="@GetIcon(item.Type)">
                @item.Title
            </MudChip>
        }
    </MudPaper>
</MudCollapse>

@code {
    [Parameter] public List<string> AttachedKnowledge { get; set; } = new();
    [Parameter] public List<string> AttachedNotes { get; set; } = new();
    [Parameter] public List<string> AttachedFiles { get; set; } = new();
    [Parameter] public List<string> AttachedChats { get; set; } = new();
    [Parameter] public List<string> AttachedWebpages { get; set; } = new();
    
    [Parameter] public List<KnowledgeItem> KnowledgeItems { get; set; } = new();
    [Parameter] public List<NoteItem> NoteItems { get; set; } = new();
    [Parameter] public List<FileItem> FileItems { get; set; } = new();
    [Parameter] public List<ChatItem> ChatItems { get; set; } = new();
    [Parameter] public List<WebpageItem> WebpageItems { get; set; } = new();
    
    [Parameter] public EventCallback<string> OnDetachKnowledge { get; set; }
    [Parameter] public EventCallback<string> OnDetachNote { get; set; }
    [Parameter] public EventCallback<string> OnDetachFile { get; set; }
    [Parameter] public EventCallback<string> OnDetachChat { get; set; }
    [Parameter] public EventCallback<string> OnDetachWebpage { get; set; }

    private bool isOpen = true;
    private int TotalAttached => AttachedKnowledge.Count + AttachedNotes.Count + 
                                  AttachedFiles.Count + AttachedChats.Count + AttachedWebpages.Count;

    private List<AttachedItemView> AllAttachedItems
    {
        get
        {
            var items = new List<AttachedItemView>();
            
            items.AddRange(AttachedKnowledge.Select(id => new AttachedItemView
            {
                Id = id,
                Title = KnowledgeItems.FirstOrDefault(k => k.Id == id)?.Title ?? $"Item {id}",
                Type = AttachmentType.Knowledge,
                OnDetach = () => OnDetachKnowledge.InvokeAsync(id)
            }));
            
            // Add other types similarly...
            
            return items;
        }
    }

    private Color GetChipColor(AttachmentType type) => type switch
    {
        AttachmentType.Knowledge => Color.Primary,
        AttachmentType.Note => Color.Info,
        AttachmentType.File => Color.Success,
        AttachmentType.Webpage => Color.Tertiary,
        AttachmentType.Chat => Color.Warning,
        _ => Color.Default
    };

    private string GetIcon(AttachmentType type) => type switch
    {
        AttachmentType.Knowledge => Icons.Material.Filled.Psychology,
        AttachmentType.Note => Icons.Material.Filled.Description,
        AttachmentType.File => Icons.Material.Filled.AttachFile,
        AttachmentType.Webpage => Icons.Material.Filled.Language,
        AttachmentType.Chat => Icons.Material.Filled.Chat,
        _ => Icons.Material.Filled.Circle
    };

    private class AttachedItemView
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public AttachmentType Type { get; set; }
        public Action OnDetach { get; set; } = () => { };
    }
}
```

### Feature 3: Structured Panel Component

**Complexity:** High  
**Dependencies:** MudBlazor Tables, Forms

**Blazor Implementation:**

```razor
<!-- StructuredPanel.razor -->
@switch (Data.Type)
{
    case StructuredPanelType.Table:
        <StructuredTable Data="Data" OnAction="OnAction" />
        break;
    case StructuredPanelType.List:
        <StructuredList Data="Data" />
        break;
    case StructuredPanelType.Form:
        <StructuredForm Data="Data" OnAction="OnAction" />
        break;
    case StructuredPanelType.KeyValue:
        <StructuredKeyValue Data="Data" />
        break;
}

@code {
    [Parameter] public StructuredData Data { get; set; } = new();
    [Parameter] public EventCallback<(StructuredAction action, object? rowData)> OnAction { get; set; }
}

<!-- StructuredTable.razor -->
<MudCard Class="my-3" Outlined>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Data.Title</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            @if (Data.Actions != null)
            {
                foreach (var action in Data.Actions)
                {
                    <MudButton Color="Color.Primary" 
                               Size="Size.Small" 
                               OnClick="@(() => OnAction.InvokeAsync((action, null)))">
                        @action.Title
                    </MudButton>
                }
            }
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudTable Items="@GetRows()" 
                  Dense 
                  Hover 
                  Bordered
                  @bind-SelectedItem="selectedRow"
                  OnRowClick="RowClickEvent">
            <HeaderContent>
                @if (Data.Columns != null)
                {
                    foreach (var column in Data.Columns)
                    {
                        <MudTh>
                            <MudTableSortLabel T="List<object>">
                                @column
                            </MudTableSortLabel>
                        </MudTh>
                    }
                }
                @if (Data.RowActions != null && Data.RowActions.Any())
                {
                    <MudTh Style="text-align: right;">Actions</MudTh>
                }
            </HeaderContent>
            <RowTemplate>
                @foreach (var cell in context)
                {
                    <MudTd DataLabel="@Data.Columns?[context.IndexOf(cell)]">
                        @cell
                    </MudTd>
                }
                @if (Data.RowActions != null && Data.RowActions.Any())
                {
                    <MudTd Style="text-align: right;">
                        @foreach (var action in Data.RowActions)
                        {
                            <MudButton Size="Size.Small" 
                                       Variant="Variant.Text" 
                                       OnClick="@(() => HandleRowAction(action, context))">
                                @action.Title
                            </MudButton>
                        }
                    </MudTd>
                }
            </RowTemplate>
            <PagerContent>
                <MudTablePager />
            </PagerContent>
        </MudTable>
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public StructuredData Data { get; set; } = new();
    [Parameter] public EventCallback<(StructuredAction action, object? rowData)> OnAction { get; set; }

    private List<object>? selectedRow;

    private IEnumerable<List<object>> GetRows()
    {
        return Data.Rows?.Select(r => r.ToList()) ?? Enumerable.Empty<List<object>>();
    }

    private async Task HandleRowAction(StructuredAction action, List<object> row)
    {
        var processedAction = new StructuredAction
        {
            Type = action.Type,
            Title = action.Title,
            Tool = action.Tool,
            Args = action.Args?.ToDictionary(
                kvp => kvp.Key,
                kvp => ProcessPlaceholder(kvp.Value.ToString() ?? "", row)
            )
        };

        await OnAction.InvokeAsync((processedAction, row));
    }

    private object ProcessPlaceholder(string value, List<object> row)
    {
        if (value.StartsWith("$row[") && value.EndsWith("]"))
        {
            var indexStr = value.Substring(5, value.Length - 6);
            if (int.TryParse(indexStr, out int index) && index < row.Count)
            {
                return row[index];
            }
        }
        return value;
    }
}
```

### Feature 4: Conversation Sidebar/Drawer

**Complexity:** High  
**Dependencies:** MudBlazor, MudDrawer

**Blazor Implementation:**

```razor
<!-- ConversationDrawer.razor -->
<MudDrawer @bind-Open="@DrawerOpen" 
           Anchor="Anchor.Left" 
           Variant="@(IsMobile ? DrawerVariant.Temporary : DrawerVariant.Persistent)"
           ClipMode="DrawerClipMode.Always"
           Width="320px"
           Class="mud-drawer-responsive">
    
    <!-- Logo and Header -->
    <MudDrawerHeader Class="d-flex align-center justify-space-between pa-4">
        <div class="d-flex align-center gap-2">
            <MudIcon Icon="@Icons.Material.Filled.AutoAwesome" Color="Color.Primary" Size="Size.Large" />
            <MudText Typo="Typo.h5">aimate</MudText>
        </div>
        @if (IsMobile)
        {
            <MudIconButton Icon="@Icons.Material.Filled.Close" OnClick="@(() => DrawerOpen = false)" />
        }
    </MudDrawerHeader>

    <!-- Navigation Links -->
    <MudNavMenu Class="pa-2">
        <MudNavLink Icon="@Icons.Material.Filled.Chat" OnClick="OnNewConversation">
            Start a new chat
        </MudNavLink>
        <MudNavLink Icon="@Icons.Material.Filled.Search" OnClick="@(() => searchDialogOpen = true)">
            Search
        </MudNavLink>
        <MudNavLink Icon="@Icons.Material.Filled.Description" OnClick="@(() => notesDialogOpen = true)">
            Notes
        </MudNavLink>
        <MudNavLink Icon="@Icons.Material.Filled.Psychology" OnClick="@(() => knowledgeDialogOpen = true)">
            Knowledge
        </MudNavLink>
        <MudNavLink Icon="@Icons.Material.Filled.AttachFile" OnClick="@(() => filesDialogOpen = true)">
            Files
        </MudNavLink>
        
        <!-- Projects Collapsible -->
        <MudNavGroup Title="Projects" 
                     Icon="@Icons.Material.Filled.FolderOpen" 
                     Expanded="@projectsExpanded"
                     ExpandedChanged="@((bool expanded) => projectsExpanded = expanded)">
            <MudNavLink Icon="@Icons.Material.Filled.Add" OnClick="HandleNewProject">
                New Project
            </MudNavLink>
            @foreach (var project in Projects)
            {
                <MudNavLink Icon="@Icons.Material.Filled.Folder" OnClick="@(() => HandleViewProject(project))">
                    @project.Name
                </MudNavLink>
            }
        </MudNavGroup>
    </MudNavMenu>

    <!-- Conversations List -->
    <MudScrollToTop Selector="#conversation-list">
        <div id="conversation-list" style="flex-grow: 1; overflow-y: auto;" class="pa-2">
            @if (!Conversations.Any())
            {
                <MudText Typo="Typo.body2" Color="Color.Secondary" Align="Align.Center" Class="pa-4">
                    No conversations yet. Start a new chat!
                </MudText>
            }
            else
            {
                @if (PinnedConversations.Any())
                {
                    <MudText Typo="Typo.caption" Color="Color.Secondary" Class="px-2 py-1">Pinned</MudText>
                    @foreach (var conv in PinnedConversations)
                    {
                        <ConversationItem Conversation="conv" 
                                         IsActive="@(conv.Id == ActiveConversationId)"
                                         IsPinned="true"
                                         OnSelect="@(() => OnSelectConversation.InvokeAsync(conv.Id))"
                                         OnDelete="@(() => OnDeleteConversation.InvokeAsync(conv.Id))"
                                         OnPin="@(() => HandlePin(conv.Id))" />
                    }
                }

                @if (RecentConversations.Any())
                {
                    @if (PinnedConversations.Any())
                    {
                        <MudText Typo="Typo.caption" Color="Color.Secondary" Class="px-2 py-1 mt-3">Recent</MudText>
                    }
                    @foreach (var conv in RecentConversations)
                    {
                        <ConversationItem Conversation="conv" 
                                         IsActive="@(conv.Id == ActiveConversationId)"
                                         IsPinned="false"
                                         OnSelect="@(() => OnSelectConversation.InvokeAsync(conv.Id))"
                                         OnDelete="@(() => OnDeleteConversation.InvokeAsync(conv.Id))"
                                         OnPin="@(() => HandlePin(conv.Id))" />
                    }
                }
            }
        </div>
    </MudScrollToTop>

    <!-- User Footer -->
    <MudMenu AnchorOrigin="Origin.TopCenter" TransformOrigin="Origin.BottomCenter">
        <ActivatorContent>
            <MudPaper Elevation="0" Class="d-flex align-center gap-3 pa-3 cursor-pointer hover-bg-gray-100">
                <MudAvatar Color="Color.Primary" Size="Size.Small">JD</MudAvatar>
                <div style="flex-grow: 1;">
                    <MudText Typo="Typo.body2">John Doe</MudText>
                    <MudText Typo="Typo.caption" Color="Color.Secondary">john.doe@example.com</MudText>
                </div>
                <MudIcon Icon="@Icons.Material.Filled.ExpandLess" Size="Size.Small" />
            </MudPaper>
        </ActivatorContent>
        <ChildContent>
            <MudMenuItem Icon="@Icons.Material.Filled.Settings" OnClick="@(() => settingsDialogOpen = true)">
                Settings
            </MudMenuItem>
            <MudMenuItem Icon="@Icons.Material.Filled.Archive" OnClick="@(() => archivedDialogOpen = true)">
                Archived
            </MudMenuItem>
            <MudMenuItem Icon="@Icons.Material.Filled.AdminPanelSettings" OnClick="@(() => adminDialogOpen = true)">
                Admin Panel
            </MudMenuItem>
            <MudMenuItem Icon="@Icons.Material.Filled.Info" OnClick="@(() => aboutDialogOpen = true)">
                About
            </MudMenuItem>
            <MudDivider />
            <MudMenuItem Icon="@Icons.Material.Filled.Logout" IconColor="Color.Error">
                Logout
            </MudMenuItem>
        </ChildContent>
    </MudMenu>
</MudDrawer>

<!-- Dialogs -->
<SettingsDialog @bind-Open="settingsDialogOpen" />
<AdminDialog @bind-Open="adminDialogOpen" EnabledModels="EnabledModels" />
<SearchDialog @bind-Open="searchDialogOpen" />
<!-- ... other dialogs -->

@code {
    [Parameter] public List<Conversation> Conversations { get; set; } = new();
    [Parameter] public string? ActiveConversationId { get; set; }
    [Parameter] public EventCallback<string> OnSelectConversation { get; set; }
    [Parameter] public EventCallback OnNewConversation { get; set; }
    [Parameter] public EventCallback<string> OnDeleteConversation { get; set; }
    [Parameter] public Dictionary<string, bool> EnabledModels { get; set; } = new();
    [Parameter] public bool DrawerOpen { get; set; } = true;
    [Parameter] public EventCallback<bool> DrawerOpenChanged { get; set; }

    [Inject] private IBreakpointService BreakpointService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool IsMobile = false;
    private bool projectsExpanded = false;
    private List<string> pinnedIds = new();
    private bool settingsDialogOpen = false;
    // ... other dialog states

    protected override async Task OnInitializedAsync()
    {
        var breakpoint = await BreakpointService.GetBreakpoint();
        IsMobile = breakpoint < Breakpoint.Md;
    }

    private List<Conversation> PinnedConversations => 
        Conversations.Where(c => pinnedIds.Contains(c.Id)).ToList();
    
    private List<Conversation> RecentConversations => 
        Conversations.Where(c => !pinnedIds.Contains(c.Id)).ToList();

    private void HandlePin(string id)
    {
        if (pinnedIds.Contains(id))
        {
            pinnedIds.Remove(id);
            Snackbar.Add("Conversation unpinned", Severity.Success);
        }
        else
        {
            pinnedIds.Add(id);
            Snackbar.Add("Conversation pinned", Severity.Success);
        }
    }
}
```

### Feature 5: Main Chat Page

**Complexity:** High  
**Dependencies:** All components

**Blazor Implementation:**

```razor
<!-- Pages/Chat.razor -->
@page "/chat"
@page "/"
@using Microsoft.AspNetCore.SignalR.Client

<PageTitle>aimate - AI Work Studio</PageTitle>

<MudThemeProvider @bind-IsDarkMode="@isDarkMode" Theme="@customTheme" />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <!-- Conversation Drawer -->
    <ConversationDrawer Conversations="@conversationList"
                       ActiveConversationId="@activeConversationId"
                       OnSelectConversation="HandleSelectConversation"
                       OnNewConversation="CreateNewConversation"
                       OnDeleteConversation="HandleDeleteConversation"
                       OnRenameConversation="HandleRenameConversation"
                       OnCloneConversation="HandleCloneConversation"
                       EnabledModels="@enabledModels"
                       @bind-DrawerOpen="@sidebarOpen" />

    <!-- Main Content -->
    <MudMainContent>
        <!-- Chat Header -->
        <ChatHeader OnNewChat="CreateNewConversation"
                   OnToggleSidebar="@(() => sidebarOpen = !sidebarOpen)"
                   SidebarOpen="@sidebarOpen"
                   SelectedModel="@selectedModel"
                   OnModelChange="@((model) => selectedModel = model)"
                   EnabledModels="@enabledModels" />

        <!-- Messages Area -->
        <MudContainer MaxWidth="MaxWidth.Large" Style="height: calc(100vh - 200px); overflow-y: auto;" id="messages-container">
            @if (!messages.Any())
            {
                <EmptyState OnSendMessage="HandleSendMessage" />
            }
            else
            {
                <div class="pa-4">
                    @foreach (var message in messages)
                    {
                        <ChatMessage Role="@message.Role"
                                    Content="@message.Content"
                                    Timestamp="@message.Timestamp"
                                    StructuredContent="@message.StructuredContent"
                                    OnEdit="@(message.Role == MessageRole.User ? (content) => HandleEditMessage(message.Id, content) : null)"
                                    OnRegenerate="@(message.Role == MessageRole.Assistant && message.Id == messages.Last().Id && !isTyping ? HandleRegenerateResponse : null)" />
                    }

                    @if (isTyping)
                    {
                        <div class="d-flex align-center gap-3 my-4">
                            <MudAvatar Color="Color.Primary" Size="Size.Small" Class="ai-avatar">
                                <MudIcon Icon="@Icons.Material.Filled.AutoAwesome" />
                            </MudAvatar>
                            <MudText Typo="Typo.body2" Color="Color.Secondary">AI is thinking...</MudText>
                            <MudProgressCircular Size="Size.Small" Indeterminate Color="Color.Primary" />
                        </div>
                    }
                </div>
            }
        </MudContainer>

        <!-- Chat Input -->
        <MudPaper Elevation="4" Class="pa-4">
            <MudContainer MaxWidth="MaxWidth.Large">
                <ChatInput OnSend="HandleSendMessage" Disabled="@isTyping" />
            </MudContainer>
        </MudPaper>

        <!-- Debug Panel -->
        @if (showDebugPanel)
        {
            <DebugPanel />
        }
    </MudMainContent>
</MudLayout>

@code {
    [Inject] private ConversationStateService ConversationState { get; set; } = default!;
    [Inject] private ChatApiService ChatApi { get; set; } = default!;
    [Inject] private DebugService DebugService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool isDarkMode = true;
    private bool sidebarOpen = true;
    private bool isTyping = false;
    private bool showDebugPanel = false;
    private string selectedModel = "gpt-4";
    private Dictionary<string, bool> enabledModels = new();
    
    private List<ConversationData> conversations = new();
    private string? activeConversationId;
    private List<Message> messages => GetActiveMessages();
    private List<Conversation> conversationList => ConversationsToList();

    private MudTheme customTheme = new()
    {
        // ... theme definition from earlier
    };

    protected override async Task OnInitializedAsync()
    {
        // Initialize enabled models
        enabledModels = new Dictionary<string, bool>
        {
            ["gpt-4"] = true,
            ["gpt-4-turbo"] = true,
            ["gpt-3.5-turbo"] = true,
            ["claude-3-opus"] = true,
            ["claude-3-sonnet"] = true,
            ["structured-gpt"] = true
        };

        // Load mock conversations
        LoadMockConversations();

        // Set active conversation
        activeConversationId = conversations.FirstOrDefault()?.Id;

        // Subscribe to state changes
        ConversationState.OnChange += StateHasChanged;
    }

    private async Task HandleSendMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        if (activeConversationId == null)
        {
            CreateNewConversation();
            await Task.Delay(10); // Wait for state update
        }

        var userMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Role = MessageRole.User,
            Content = content,
            Timestamp = DateTime.Now
        };

        var conversation = conversations.FirstOrDefault(c => c.Id == activeConversationId);
        if (conversation != null)
        {
            conversation.Messages.Add(userMessage);
            
            // Update title if first message
            if (conversation.Messages.Count == 1)
            {
                conversation.Title = content.Length > 50 ? content.Substring(0, 50) + "..." : content;
            }

            StateHasChanged();
            await ScrollToBottom();

            // Simulate AI response
            isTyping = true;
            StateHasChanged();

            await Task.Delay(1500);

            var aiMessage = await GenerateAIResponse(content);
            conversation.Messages.Add(aiMessage);

            isTyping = false;
            StateHasChanged();
            await ScrollToBottom();

            DebugService.AddLog(new DebugLog
            {
                Action = "Message Sent & Received",
                Api = "/api/v1/chat/completion",
                Timestamp = DateTime.Now,
                Type = LogType.Success
            });
        }
    }

    private async Task<Message> GenerateAIResponse(string userMessage)
    {
        // Check for structured content triggers
        var lowerContent = userMessage.Trim().ToLower();
        
        if (lowerContent == "gettable")
        {
            return new Message
            {
                Id = Guid.NewGuid().ToString(),
                Role = MessageRole.Assistant,
                Content = "Projects loaded.",
                Timestamp = DateTime.Now,
                StructuredContent = GetMockTableData()
            };
        }
        
        // Default mock response
        return new Message
        {
            Id = Guid.NewGuid().ToString(),
            Role = MessageRole.Assistant,
            Content = "I'm here to help! What would you like to know?",
            Timestamp = DateTime.Now
        };
    }

    private async Task ScrollToBottom()
    {
        await JSRuntime.InvokeVoidAsync("scrollToBottom", "messages-container");
    }

    private List<Message> GetActiveMessages()
    {
        var conv = conversations.FirstOrDefault(c => c.Id == activeConversationId);
        return conv?.Messages ?? new List<Message>();
    }

    private List<Conversation> ConversationsToList()
    {
        return conversations.Select(c => new Conversation
        {
            Id = c.Id,
            Title = c.Title,
            LastMessage = c.Messages.LastOrDefault()?.Content ?? "No messages yet",
            Timestamp = c.CreatedAt
        }).ToList();
    }

    public void Dispose()
    {
        ConversationState.OnChange -= StateHasChanged;
    }
}
```

**wwwroot/js/site.js:**
```javascript
window.scrollToBottom = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};
```

---

## Migration Roadmap

### Phase 1: Infrastructure Setup (Week 1)
- [ ] Create Blazor Server project (.NET 9)
- [ ] Install MudBlazor NuGet package
- [ ] Set up project structure (Pages, Components, Services, Models)
- [ ] Configure MudBlazor in Program.cs
- [ ] Create base theme configuration
- [ ] Set up API controllers structure

### Phase 2: Core Models & Services (Week 1-2)
- [ ] Convert all TypeScript interfaces to C# classes
- [ ] Implement ConversationStateService
- [ ] Implement DebugService
- [ ] Create ChatApiService with HTTP client
- [ ] Implement API stub endpoints as controllers
- [ ] Create API response models

### Phase 3: Basic UI Components (Week 2-3)
- [ ] Create MainLayout with MudLayout
- [ ] Implement ChatMessage component
- [ ] Implement ChatInput component
- [ ] Implement ChatHeader component
- [ ] Implement EmptyState component
- [ ] Create basic navigation structure

### Phase 4: Sidebar & Navigation (Week 3-4)
- [ ] Implement ConversationDrawer
- [ ] Create ConversationItem component
- [ ] Implement user menu
- [ ] Add project management UI
- [ ] Implement collapsible sections
- [ ] Add responsive behavior

### Phase 5: Modal Dialogs (Week 4-5)
- [ ] Create BaseDialog wrapper
- [ ] Implement SettingsDialog with tabs
- [ ] Implement AdminDialog
- [ ] Implement SearchDialog
- [ ] Implement NotesDialog
- [ ] Implement KnowledgeDialog
- [ ] Implement FilesDialog
- [ ] Implement other dialogs (Share, Rating, etc.)

### Phase 6: Advanced Features (Week 5-6)
- [ ] Implement AttachedContext component
- [ ] Create KnowledgeSuggestions component
- [ ] Implement StructuredPanel (Table, List, Form, KV)
- [ ] Add drag-and-drop file upload
- [ ] Implement markdown rendering
- [ ] Add message editing/regeneration

### Phase 7: API Integration (Week 6-7)
- [ ] Connect to real API endpoints
- [ ] Implement authentication
- [ ] Add error handling
- [ ] Implement retry logic
- [ ] Add loading states
- [ ] Test all API calls

### Phase 8: Testing & Polish (Week 7-8)
- [ ] Test all components
- [ ] Fix responsive issues
- [ ] Optimize performance
- [ ] Add accessibility features
- [ ] Refine animations
- [ ] Documentation

### Phase 9: Deployment (Week 8)
- [ ] Configure production build
- [ ] Set up hosting environment
- [ ] Deploy to staging
- [ ] User acceptance testing
- [ ] Deploy to production

---

## Key Differences & Considerations

### 1. **Event Handling**

**React:**
```typescript
onClick={() => handleClick()}
onChange={(e) => setValue(e.target.value)}
```

**Blazor:**
```razor
@onclick="HandleClick"
@bind-Value="value"
```

### 2. **Conditional Rendering**

**React:**
```tsx
{isVisible && <Component />}
{condition ? <ComponentA /> : <ComponentB />}
```

**Blazor:**
```razor
@if (isVisible)
{
    <Component />
}

@if (condition)
{
    <ComponentA />
}
else
{
    <ComponentB />
}
```

### 3. **List Rendering**

**React:**
```tsx
{items.map((item) => (
  <div key={item.id}>{item.name}</div>
))}
```

**Blazor:**
```razor
@foreach (var item in items)
{
    <div @key="item.Id">@item.Name</div>
}
```

### 4. **Two-Way Binding**

**React:**
```tsx
<input value={text} onChange={(e) => setText(e.target.value)} />
```

**Blazor:**
```razor
<MudTextField @bind-Value="text" />
```

### 5. **Lifecycle Methods**

| React Hook | Blazor Method |
|-----------|---------------|
| `useEffect(() => { ... }, [])` | `OnInitializedAsync()` |
| `useEffect(() => { ... })` | `OnAfterRenderAsync(bool firstRender)` |
| `useEffect(() => { return cleanup; })` | `IDisposable.Dispose()` |

### 6. **Async Operations**

**React:**
```typescript
useEffect(() => {
  async function fetchData() {
    const result = await api.getData();
    setData(result);
  }
  fetchData();
}, []);
```

**Blazor:**
```csharp
protected override async Task OnInitializedAsync()
{
    var result = await api.GetDataAsync();
    data = result;
}
```

### 7. **CSS Classes**

**React (Tailwind):**
```tsx
<div className="flex gap-3 items-center bg-gray-100 dark:bg-gray-900">
```

**Blazor (MudBlazor):**
```razor
<div class="d-flex gap-3 align-center mud-paper">
```

---

## Performance Considerations

### Blazor Server Specifics

1. **SignalR Connection**: Blazor Server uses WebSockets. Monitor connection stability.
2. **StateHasChanged**: Call judiciously to avoid excessive re-renders.
3. **Virtualization**: Use `<Virtualize>` for long lists (conversation history).
4. **Debouncing**: Implement for real-time features (search, typing indicators).

### Optimization Tips

```csharp
// Use ShouldRender to prevent unnecessary renders
protected override bool ShouldRender()
{
    return hasChanges;
}

// Virtualize long lists
<Virtualize Items="@conversations" Context="conversation">
    <ConversationItem Conversation="conversation" />
</Virtualize>

// Use EventCallback for parent-child communication
[Parameter] public EventCallback<string> OnMessageSent { get; set; }
```

---

## Testing Strategy

### Unit Tests

```csharp
// Example using bUnit
[Fact]
public void ChatMessage_RendersUserMessage_Correctly()
{
    using var ctx = new TestContext();
    
    var component = ctx.RenderComponent<ChatMessage>(parameters => parameters
        .Add(p => p.Role, MessageRole.User)
        .Add(p => p.Content, "Hello, World!")
    );
    
    component.MarkupMatches("<div>Hello, World!</div>");
}
```

### Integration Tests

```csharp
[Fact]
public async Task SendMessage_AddsMessageToConversation()
{
    // Arrange
    var service = new ConversationStateService();
    
    // Act
    await service.SendMessageAsync("test-conv-id", "Hello");
    
    // Assert
    var messages = service.GetMessages("test-conv-id");
    Assert.Single(messages);
    Assert.Equal("Hello", messages[0].Content);
}
```

---

## Resources & References

### Documentation
- [MudBlazor Documentation](https://mudblazor.com/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)

### Component Libraries
- [MudBlazor GitHub](https://github.com/MudBlazor/MudBlazor)
- [bUnit Testing](https://bunit.dev/)

### Key NuGet Packages
```xml
<PackageReference Include="MudBlazor" Version="7.0.0" />
<PackageReference Include="Markdig" Version="0.37.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
```

---

## Appendix A: File Structure Comparison

### React Project Structure
```
/
├── components/
│   ├── ChatMessage.tsx
│   ├── ChatInput.tsx
│   ├── ConversationSidebar.tsx
│   ├── ui/
│   │   ├── button.tsx
│   │   ├── dialog.tsx
│   │   └── ...
├── utils/
│   └── api-stubs.ts
├── styles/
│   └── globals.css
├── App.tsx
├── main.tsx
└── package.json
```

### Blazor Project Structure
```
/
├── Pages/
│   └── Chat.razor
├── Components/
│   ├── ChatMessage.razor
│   ├── ChatInput.razor
│   ├── ConversationDrawer.razor
│   └── Shared/
│       └── MainLayout.razor
├── Services/
│   ├── ConversationStateService.cs
│   ├── ChatApiService.cs
│   └── DebugService.cs
├── Models/
│   ├── Message.cs
│   ├── Conversation.cs
│   └── ApiResponse.cs
├── Controllers/
│   ├── ChatController.cs
│   ├── KnowledgeController.cs
│   └── ...
├── wwwroot/
│   ├── css/
│   │   └── app.css
│   └── js/
│       └── site.js
├── Program.cs
└── appsettings.json
```

---

## Appendix B: Quick Reference Tables

### Color Mapping

| React Tailwind | MudBlazor | Hex |
|---------------|-----------|-----|
| `purple-600` | `Color.Primary` | `#9333EA` |
| `blue-500` | `Color.Secondary` | `#3B82F6` |
| `green-500` | `Color.Success` | `#10B981` |
| `yellow-500` | `Color.Warning` | `#F59E0B` |
| `red-500` | `Color.Error` | `#EF4444` |
| `gray-500` | `Color.Dark` | `#6B7280` |

### Size Mapping

| React | MudBlazor |
|-------|-----------|
| `size="sm"` | `Size="Size.Small"` |
| `size="md"` | `Size="Size.Medium"` |
| `size="lg"` | `Size="Size.Large"` |

### Variant Mapping

| React | MudBlazor |
|-------|-----------|
| `variant="ghost"` | `Variant="Variant.Text"` |
| `variant="outline"` | `Variant="Variant.Outlined"` |
| `variant="default"` | `Variant="Variant.Filled"` |

---

**End of Specification Document**

This comprehensive guide should provide everything needed to successfully convert the React AI Chat Interface to Blazor with MudBlazor. For questions or clarifications during the migration process, refer to the code examples and patterns provided throughout this document.
