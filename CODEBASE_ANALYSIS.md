# aiMate Codebase: Admin/Settings Infrastructure Analysis

## Current Project Structure

### Main Directories
- **src/**: Original implementation (Blazor WebAssembly)
- **src-v2/**: New implementation with Fluxor state management
  - `AiMate.Web`: Web frontend with Blazor components
  - `AiMate.Core`: Domain entities and services
  - `AiMate.Infrastructure`: Database, migrations, services
  - `AiMate.Shared`: Shared models

### Technology Stack
- **Frontend**: Blazor WebAssembly with MudBlazor UI components
- **State Management**: Fluxor (Redux-like pattern)
- **API Layer**: LiteLLM proxy for unified AI model access
- **Theme**: MudBlazor with custom dark theme

---

## EXISTING ADMIN/SETTINGS COMPONENTS

### 1. Settings Infrastructure (src-v2)

#### Page Component
**File**: `/home/user/aiMate/src-v2/AiMate.Web/Components/Pages/Settings.razor`
- Route: `/settings`
- Uses MudBlazor tabs for navigation
- Integrated with Fluxor state management
- 6 main tabs implemented:
  1. **General** - Language, timezone, notifications, sounds
  2. **Interface** - Theme, code theme, font size, display preferences
  3. **Connections** - LiteLLM configuration, API settings
  4. **Personalisation** - Personality mode, default model, temperature, system prompts
  5. **Account** - Username, email, avatar, subscription tier
  6. **Usage** - Cost tracking, budgets, usage alerts

#### Tab Components
All located in `/home/user/aiMate/src-v2/AiMate.Web/Components/Settings/`

1. **GeneralSettingsTab.razor**
   - Language selection (English NZ, Te Reo Māori, EN-US, EN-GB)
   - Timezone selection (multiple options)
   - Notification toggles
   - Sound effects toggle

2. **InterfaceSettingsTab.razor**
   - Theme selector (dark, light, auto)
   - Code theme selector (dracula, monokai, github, nord, solarized)
   - Font size slider (10-24px)
   - Compact mode toggle
   - Line numbers toggle
   - Markdown preview toggle

3. **ConnectionsSettingsTab.razor**
   - LiteLLM URL configuration
   - API key input (password field)
   - Request timeout control
   - Max retries configuration
   - Streaming toggle
   - Test connection button with result feedback

4. **PersonalisationSettingsTab.razor**
   - Default personality selector (KiwiMate, Professional, Dev, Te Reo, Mental Health Support, Standard)
   - Default model selector (hardcoded: GPT-4, GPT-3.5, Claude variants, Gemini)
   - Temperature slider (0-2)
   - Max tokens input (100-8000)
   - System prompt override textarea

5. **AccountSettingsTab.razor**
   - Username input
   - Email input
   - Avatar URL input
   - Subscription tier selector (Free, BYOK $10/month, Developer $30/month)
   - Delete account button
   - "True Open Source" notice

6. **UsageSettingsTab.razor**
   - Usage tracking toggle
   - Cost estimates display toggle
   - Usage alerts toggle
   - Monthly budget input (NZD)
   - Current month usage display (hardcoded stats)
   - Usage by model table
   - Export usage data button

#### State Management Pattern

**Store Location**: `/home/user/aiMate/src-v2/AiMate.Web/Store/Settings/`

**SettingsState.cs** - Main state record with:
```
- General: Language, TimeZone, EnableNotifications, EnableSoundEffects
- Interface: Theme, FontSize, CompactMode, ShowLineNumbers, EnableMarkdownPreview, CodeTheme
- Connections: LiteLLMUrl, ApiKey, RequestTimeout, MaxRetries, UseStreamingByDefault
- Personalisation: DefaultPersonality, DefaultModel, Temperature, MaxTokens, SystemPromptOverride
- Account: Username, Email, AvatarUrl, UserTier
- Usage: TrackUsage, ShowCostEstimates, MonthlyBudget, EnableUsageAlerts
- UI: IsLoading, IsSaving, Error, ActiveTab
```

**SettingsActions.cs** - 60+ action records for:
- Loading/saving settings
- Updating individual settings
- Connection testing
- Tab management
- Error handling

**SettingsReducers.cs** - Immutable state mutations
- All actions have corresponding reducer methods
- Uses record `with` syntax for immutability

**SettingsEffects.cs** - Side effects handler
- Loads/saves settings to localStorage
- Connection testing (TODO: integrate actual LiteLLM service)
- Uses IJSRuntime for browser storage access

### 2. Existing Modal/Dialog Patterns (src-v2)

Located in `/home/user/aiMate/src-v2/AiMate.Web/Components/Shared/`

#### FileUploadDialog.razor
**Pattern Example**:
- Uses `MudDialog` component with `@bind-IsVisible`
- `[Parameter]` properties for visibility and callbacks
- `DialogOptions` configuration
- `TitleContent` and `DialogContent` sections
- `DialogActions` with Cancel/Confirm buttons
- Event callbacks for communication with parent
- Loading state management with progress indicator

#### KnowledgeItemEditor.razor & KnowledgeItemViewer.razor
- Similar modal patterns with Fluxor integration
- Show/hide state managed in Fluxor store
- Custom dialog options (width, styles)

#### MainLayout.razor Setup
- `<MudDialogProvider />` - Required for MudDialog functionality
- `<MudThemeProvider />` - Theme management
- `<MudSnackbarProvider />` - Notifications

### 3. Existing Admin Modal (src - older version)

**File**: `/home/user/aiMate/src/AiMate.Client/Components/Modals/AdminModal.razor`

**Features**:
- 6 tabs: Overview, Models, Connections, MCP, Users, System
- System statistics dashboard with cards (users, conversations, models, MCP servers)
- System health status table (LiteLLM connection, storage, version, uptime)
- Model management table with enable/disable switches
- Connection configuration (LiteLLM, OpenAI, Anthropic, Google, Azure)
- MCP server management table
- User management placeholder (disabled, requires multi-user backend)
- System section with:
  - Database management (LocalStorage, IndexedDB) with progress bars
  - System logs viewer with refresh/clear/export
  - Maintenance actions (export config, import config, backup)
  - Factory reset in danger zone
- Nested MudDialog for Add Model
- Nested MudDialog for Add MCP Server

---

## MODAL/DIALOG COMPONENTS IN CODEBASE

### MudBlazor Dialog System
- **Provider**: Required in MainLayout (`<MudDialogProvider />`)
- **Pattern 1**: Component-based with parameters and event callbacks
- **Pattern 2**: Service-based using IDialogService

### Existing Modal Components
1. FileUploadDialog - Simple functional dialog
2. KnowledgeItemEditor - Fluxor-integrated dialog
3. WorkspaceEditor - Dialog with state management
4. KnowledgeItemViewer - Modal viewer pattern
5. AdminModal (src) - Complex multi-tab administrative panel

---

## TAB IMPLEMENTATIONS

### MudBlazor Tabs Pattern
All implementations use `<MudTabs>` with:
- `ActivePanelIndex` binding
- `ActivePanelIndexChanged` callback
- `<MudTabPanel>` child components with:
  - `Text` property for tab label
  - `Icon` property for Material Icons
  - Content markup inside

### Settings Tab Pattern (Best Practice)
- Tab index tracking in Fluxor state
- `SetActiveTabAction` dispatch on tab change
- Dedicated component per tab for modularity
- Each tab component:
  - Uses `@inherits FluxorComponent`
  - Injects `IState<SettingsState>`
  - Injects `IDispatcher`
  - Dispatches actions on control changes
  - Updates local state in `OnParametersSet`

---

## CONFIGURATION MODELS & ENTITIES

### Core Entities (src-v2)
**Location**: `/home/user/aiMate/src-v2/AiMate.Core/Entities/`
- User.cs
- Workspace.cs
- Conversation.cs
- Message.cs
- Project.cs
- KnowledgeItem.cs
- WorkspaceFile.cs
- ToolCall.cs

### Core Enums
**Location**: `/home/user/aiMate/src-v2/AiMate.Core/Enums/`
- MessageRole.cs
- WorkspaceType.cs
- UserTier.cs (Free, BYOK, Developer)
- PersonalityMode.cs

### Shared Models
**Location**: `/home/user/aiMate/src-v2/AiMate.Shared/Models/`
- LiteLLMModels.cs
  - `ChatCompletionRequest`
  - `ChatCompletionResponse`
  - `ChatMessage`
  - `ToolDefinition`
  - `Usage`

### Settings Models
- Defined inline in SettingsState.cs as properties
- No separate entity model currently

---

## EXISTING STATE MANAGEMENT STORES

Located in `/home/user/aiMate/src-v2/AiMate.Web/Store/`

1. **Settings** - User preferences and configuration
2. **Chat** - Conversation and message state
3. **Workspace** - Workspace management
4. **Project** - Project state
5. **Knowledge** - Knowledge items and graphs
6. **Auth** - Authentication state

All follow same pattern:
- `*State.cs` - Record with `[FeatureState]`
- `*Actions.cs` - Record action definitions
- `*Reducers.cs` - Pure functions for state mutations
- `*Effects.cs` - Side effects (async, API calls, storage)

---

## WHAT EXISTS

1. ✅ Full settings page with 6 tabs
2. ✅ Settings state management (Fluxor) with 60+ actions
3. ✅ Individual tab components properly organized
4. ✅ Modal/dialog pattern examples
5. ✅ LocalStorage persistence for settings
6. ✅ MudBlazor integration and theme system
7. ✅ AdminModal skeleton in src (older version)
8. ✅ Tab-based navigation pattern
9. ✅ Event callbacks for dialog state

---

## WHAT NEEDS TO BE IMPLEMENTED

### 1. Admin Settings/Configuration
- [ ] Create AdminSettings page component (if not just modal)
- [ ] Create AdminSettingsState (or extend existing Settings store)
- [ ] Create AdminSettingsActions for admin-only operations
- [ ] Admin permission checks/guards

### 2. Admin-Specific State Management
- [ ] AdminState store for system-level configuration
- [ ] Actions for:
  - Model management (CRUD)
  - MCP server configuration
  - System settings
  - User management (if multi-user enabled)
  - API key management
  - Backup/restore operations

### 3. Modal Dialogs for Admin Functions
- [ ] Add Model Dialog (backend integration)
- [ ] Edit Model Dialog
- [ ] Delete Model Confirmation Dialog
- [ ] Add MCP Server Dialog
- [ ] Edit MCP Server Dialog
- [ ] System Configuration Dialog
- [ ] Backup/Restore Dialog
- [ ] Factory Reset Confirmation Dialog

### 4. Admin Components
- [ ] Admin Overview/Dashboard component
- [ ] Model Management component with table/CRUD
- [ ] MCP Server Management component
- [ ] System Configuration component
- [ ] Logs Viewer component
- [ ] Storage Management component
- [ ] User Management component (for multi-user)

### 5. Models & Configuration
- [ ] AdminModel entity/record
- [ ] MCPServerConfiguration model
- [ ] SystemConfiguration model
- [ ] SettingsBackup model
- [ ] AdminAuditLog model

### 6. Services
- [ ] IAdminService interface and implementation
- [ ] IModelManagementService
- [ ] IMCPManagementService
- [ ] ISystemConfigurationService
- [ ] IBackupService
- [ ] ILicenseService (for tiering)

### 7. API Integration
- [ ] Admin API endpoints (if needed)
- [ ] Model management endpoints
- [ ] System configuration endpoints
- [ ] Backup/restore endpoints

### 8. Advanced Features
- [ ] Usage analytics/reporting
- [ ] System health monitoring
- [ ] Error logging and review
- [ ] Audit trail
- [ ] Configuration import/export
- [ ] Multi-user permission system

### 9. Testing & Validation
- [ ] Admin permission tests
- [ ] Modal interaction tests
- [ ] State management tests
- [ ] Settings persistence tests
- [ ] Factory reset tests

### 10. UI Polish
- [ ] Confirmation dialogs for destructive actions
- [ ] Loading indicators
- [ ] Error notifications
- [ ] Success/validation messages
- [ ] Form validation
- [ ] Input sanitization

---

## RECOMMENDED IMPLEMENTATION APPROACH

### Phase 1: Core Admin Infrastructure
1. Create AdminState store (follow Settings pattern)
2. Create AdminActions for system operations
3. Create AdminReducers and AdminEffects
4. Establish admin permission/authorization mechanism

### Phase 2: Admin Page & Modals
1. Create Admin settings page (or route to AdminModal)
2. Implement FileUploadDialog pattern for dialogs
3. Create tab components for admin sections:
   - System Overview
   - Model Management
   - Connections
   - MCP Servers
   - Users (placeholder)
   - System Configuration

### Phase 3: Backend Integration
1. Create admin services
2. Integrate API calls in AdminEffects
3. Implement actual LiteLLM API integration
4. Add database operations for admin settings

### Phase 4: Advanced Features
1. Backup/restore system
2. Audit logging
3. Analytics
4. Multi-user support

---

## DESIGN PATTERNS TO FOLLOW

### 1. Component Structure
```
Components/
├── Admin/
│   ├── AdminPage.razor (main container)
│   ├── Tabs/
│   │   ├── OverviewTab.razor
│   │   ├── ModelManagementTab.razor
│   │   └── ...
│   └── Dialogs/
│       ├── AddModelDialog.razor
│       ├── DeleteConfirmDialog.razor
│       └── ...
```

### 2. Store Structure (Fluxor)
```
Store/
├── Admin/
│   ├── AdminState.cs
│   ├── AdminActions.cs
│   ├── AdminReducers.cs
│   └── AdminEffects.cs
```

### 3. Modal Pattern (MudBlazor)
```razorhtml
<MudDialog @bind-IsVisible="@IsOpen">
    <DialogContent>
        <!-- Content here -->
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="@Close">Cancel</MudButton>
        <MudButton Variant="Variant.Filled" OnClick="@Confirm">Confirm</MudButton>
    </DialogActions>
</MudDialog>
```

### 4. State Dispatch Pattern
```csharp
private void HandleAction()
{
    Dispatcher.Dispatch(new SomeAction(parameter));
}
```

### 5. State Subscription Pattern
```csharp
protected override void OnParametersSet()
{
    var value = SomeState.Value.Property;
}
```

---

## KEY FILES TO REFERENCE

### Examples to Follow
- `/home/user/aiMate/src-v2/AiMate.Web/Components/Pages/Settings.razor` - Main settings page
- `/home/user/aiMate/src-v2/AiMate.Web/Store/Settings/` - Complete store example
- `/home/user/aiMate/src-v2/AiMate.Web/Components/Settings/GeneralSettingsTab.razor` - Tab component example
- `/home/user/aiMate/src-v2/AiMate.Web/Components/Shared/FileUploadDialog.razor` - Dialog pattern
- `/home/user/aiMate/src/AiMate.Client/Components/Modals/AdminModal.razor` - Admin modal design reference

### Infrastructure
- `/home/user/aiMate/src-v2/AiMate.Web/Components/Layout/MainLayout.razor` - Theme & dialog setup
- `/home/user/aiMate/src-v2/AiMate.Core/Enums/` - Enum conventions
- `/home/user/aiMate/src-v2/AiMate.Shared/Models/LiteLLMModels.cs` - Model conventions

---

## NOTES & CONSIDERATIONS

1. **Settings Page vs Admin Modal**: Currently settings is a page, admin could be modal or separate page
2. **Multi-tab Pattern**: Uses Fluxor's `ActiveTab` state - proven pattern
3. **LocalStorage**: Settings already persist to localStorage - can extend for admin settings
4. **Permission System**: Needs to be designed and implemented for admin access
5. **Real LiteLLM Integration**: Currently mocked - implement actual service integration
6. **MCP Servers**: Currently placeholder - needs backend integration
7. **User Management**: Marked as "single-user mode" - needs multi-user backend
8. **Cost Tracking**: Hardcoded data - needs real API integration
9. **Dialog Provider**: Already in MainLayout - can create multiple dialogs immediately
10. **Theme System**: MudBlazor provides accessibility - maintain patterns
