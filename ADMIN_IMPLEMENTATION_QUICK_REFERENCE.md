# Quick Reference: Admin/Settings Implementation Guide

## Current Status Snapshot

### What's Already Built
```
✅ Settings Page (/settings) with 6 tabs
✅ Fluxor State Management (Settings store)
✅ MudBlazor Dialog System (framework in place)
✅ Settings Persistence (localStorage)
✅ Tab Components (6 individual components)
✅ Connection Testing Setup
✅ User Tier System (Free/BYOK/Developer)
```

### Architecture Overview
```
┌─ Settings Page (Router)
│   └─ MudTabs (Component)
│       ├─ GeneralSettingsTab
│       ├─ InterfaceSettingsTab
│       ├─ ConnectionsSettingsTab
│       ├─ PersonalisationSettingsTab
│       ├─ AccountSettingsTab
│       └─ UsageSettingsTab
│
└─ Fluxor State (Store)
    ├─ SettingsState
    ├─ SettingsActions (60+)
    ├─ SettingsReducers
    └─ SettingsEffects
```

## File Locations Quick Map

### Settings Implementation (Complete)
- **Page**: `src-v2/AiMate.Web/Components/Pages/Settings.razor`
- **Tabs**: `src-v2/AiMate.Web/Components/Settings/*.razor` (6 files)
- **Store**: `src-v2/AiMate.Web/Store/Settings/` (4 files)

### Admin Reference (Old Version)
- **Modal**: `src/AiMate.Client/Components/Modals/AdminModal.razor`
  - Good reference for admin UI patterns
  - 6 tabs design
  - Model management table
  - MCP configuration
  - System logs
  - Factory reset section

### Dialog Patterns
- **Example 1**: `src-v2/AiMate.Web/Components/Shared/FileUploadDialog.razor`
- **Example 2**: `src-v2/AiMate.Web/Components/Shared/KnowledgeItemEditor.razor`

## Implementation Checklist

### Phase 1: Admin State Management
```
[ ] Create Store/Admin/ directory
[ ] Create AdminState.cs with admin properties:
    - Models: List<AIModel>
    - MCPServers: List<MCPServer>
    - SystemConfig: SystemConfiguration
    - IsLoading, IsSaving, Error
[ ] Create AdminActions.cs (30+ actions)
[ ] Create AdminReducers.cs (match all actions)
[ ] Create AdminEffects.cs (async operations)
```

### Phase 2: Admin Components
```
[ ] Create Components/Admin/ directory
[ ] Create AdminPage.razor (main container)
[ ] Create Tabs/:
    [ ] OverviewTab.razor
    [ ] ModelManagementTab.razor
    [ ] ConnectionsTab.razor
    [ ] MCPServersTab.razor
    [ ] SystemTab.razor
    [ ] UsersTab.razor (placeholder)
[ ] Create Dialogs/:
    [ ] AddModelDialog.razor
    [ ] EditModelDialog.razor
    [ ] DeleteConfirmDialog.razor
    [ ] AddMCPDialog.razor
    [ ] EditMCPDialog.razor
```

### Phase 3: Models & Configuration
```
[ ] Create Core/Entities/AIModel.cs
[ ] Create Core/Entities/MCPServerConfig.cs
[ ] Create Core/Entities/SystemConfiguration.cs
[ ] Create Shared/Models/AdminModels.cs
[ ] Create Core/Enums/AdminPermission.cs
```

### Phase 4: Services
```
[ ] Create Core/Services/IAdminService.cs
[ ] Create Infrastructure/Services/AdminService.cs
[ ] Create Core/Services/ILiteLLMManagementService.cs
[ ] Create Infrastructure/Services/LiteLLMManagementService.cs
[ ] Create Core/Services/IMCPManagementService.cs
[ ] Create Infrastructure/Services/MCPManagementService.cs
```

## Key Patterns to Use

### Settings Tab Pattern (Proven)
```razor
@using Fluxor
@using AiMate.Web.Store.Settings
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IState<SettingsState> SettingsState
@inject IDispatcher Dispatcher

<MudText Typo="Typo.h6" Class="mb-4">Tab Title</MudText>
<MudGrid>
    <MudItem xs="12">
        <MudSelect @bind-Value="@_value"
                  Label="Label"
                  Variant="Variant.Outlined"
                  OnValueChanged="@((string value) => Dispatcher.Dispatch(new UpdateAction(value)))">
            <MudSelectItem Value="@option1">Option 1</MudSelectItem>
        </MudSelect>
    </MudItem>
</MudGrid>

@code {
    private string _value = "";
    
    protected override void OnParametersSet()
    {
        _value = SettingsState.Value.Property;
    }
}
```

### Admin Dialog Pattern
```razor
<MudDialog @bind-IsVisible="@IsOpen">
    <TitleContent>
        <MudText Typo="Typo.h6">Dialog Title</MudText>
    </TitleContent>
    <DialogContent>
        <MudStack Spacing="3">
            <!-- Form fields here -->
        </MudStack>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="@Close" Variant="Variant.Text">Cancel</MudButton>
        <MudButton OnClick="@Submit" Variant="Variant.Filled" Color="Color.Primary">Save</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    
    private async Task Close() {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }
}
```

### Fluxor Action Pattern
```csharp
// Actions.cs
public record CreateModelAction(AIModel Model);
public record CreateModelSuccessAction(AIModel Model);
public record CreateModelFailureAction(string Error);

// Reducers.cs
[ReducerMethod]
public static AdminState OnCreateModel(AdminState state, CreateModelAction action)
{
    return state with { IsLoading = true, Error = null };
}

// Effects.cs
[EffectMethod]
public async Task HandleCreateModel(CreateModelAction action, IDispatcher dispatcher)
{
    try
    {
        var result = await _adminService.CreateModelAsync(action.Model);
        dispatcher.Dispatch(new CreateModelSuccessAction(result));
    }
    catch (Exception ex)
    {
        dispatcher.Dispatch(new CreateModelFailureAction(ex.Message));
    }
}
```

## Integration Points

### 1. Add Admin Route
File: `src-v2/AiMate.Web/Components/Routes.razor`
```razor
<Route path="admin" Component="typeof(AdminPage)" />
```

### 2. Add Admin Navigation
File: `src-v2/AiMate.Web/Components/Layout/Sidebar.razor`
```razor
<MudNavLink Href="/admin" Icon="@Icons.Material.Filled.AdminPanelSettings">
    Admin
</MudNavLink>
```

### 3. Register Fluxor Store
File: `src-v2/AiMate.Web/Program.cs`
```csharp
builder.Services.AddFluxor(options =>
    options.ScanAssemblies(typeof(App).Assembly));
```

## Common Implementation Tasks

### Add a Settings Tab
1. Create new file: `Components/Settings/NewSettingsTab.razor`
2. Copy pattern from GeneralSettingsTab.razor
3. Add new action to SettingsActions.cs
4. Add reducer to SettingsReducers.cs
5. Add new tab to Settings.razor page

### Add a Dialog
1. Create new file: `Components/Admin/Dialogs/NewDialog.razor`
2. Copy FileUploadDialog.razor pattern
3. Add [Parameter] properties
4. Implement event callbacks
5. Call from parent component with `@bind-IsVisible`

### Add State to Fluxor
1. Create AdminState.cs with `[FeatureState]`
2. Add all necessary properties
3. Create AdminActions.cs with action records
4. Create AdminReducers.cs with all reducer methods
5. Create AdminEffects.cs for async operations

## Common Issues & Solutions

### Dialog not appearing?
- Ensure `<MudDialogProvider />` is in MainLayout.razor ✓ (Already present)
- Check `@bind-IsVisible` binding
- Verify state being set to true

### State not updating?
- Check reducer method signature: `public static AdminState On[Action]`
- Verify action parameter matches dispatcher call
- Check Dispatcher injection

### Services not working?
- Register in Program.cs: `builder.Services.AddScoped<IServiceName, ServiceImpl>();`
- Inject in Effects constructor
- Use await for async calls

## Testing Areas

1. **State Management**
   - Dispatch actions and verify state changes
   - Test reducers in isolation
   - Test effects with mocked services

2. **UI Interactions**
   - Tab switching
   - Dialog open/close
   - Form submissions
   - Validation

3. **Integration**
   - Component receives state correctly
   - Actions dispatch properly
   - Effects call services

4. **Persistence**
   - Settings saved to localStorage
   - Settings loaded on page refresh
   - Reset functionality

## Resources

- MudBlazor Docs: https://mudblazor.com/
- Fluxor Docs: https://github.com/mrpmorris/Fluxor
- Blazor Docs: https://learn.microsoft.com/aspnet/core/blazor/
- LiteLLM API: Reference old AdminModal for connection patterns

## Next Steps

1. Start with Phase 1: Create AdminState store
2. Follow the Settings pattern exactly
3. Reference AdminModal from src for UI design
4. Use FileUploadDialog as dialog template
5. Test each phase before moving to next
