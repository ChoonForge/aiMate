# ConversationList Component

A fully-featured conversation list component with context menu integration for aiMate v2.

## Features

- ✅ Displays all conversations from Fluxor ChatState
- ✅ Shows pinned conversations at the top
- ✅ Real-time relative timestamps ("2h ago", "Just now", etc.)
- ✅ Integrated context menu on each conversation
- ✅ Active conversation highlighting
- ✅ Smooth hover effects
- ✅ Full CRUD operations (Create, Read, Update, Delete)
- ✅ Download conversations as JSON
- ✅ Archive/Pin/Rename/Share/Delete actions
- ✅ Confirmation dialogs for destructive actions

## Usage

### Basic Integration

Add the component to your layout or page:

```razor
@page "/conversations"
@using AiMate.Web.Components.Shared

<ConversationList />
```

### In Sidebar (Recommended)

Update `Components/Layout/Sidebar.razor` to include the conversation list:

```razor
@using AiMate.Web.Components.Shared

<div class="sidebar">
    <div class="sidebar-header">
        <!-- Your logo/header -->
    </div>

    <div class="sidebar-content">
        <!-- Quick Actions -->
        <MudButton Variant="Variant.Filled"
                  Color="Color.Primary"
                  StartIcon="@Icons.Material.Filled.Add"
                  OnClick="@CreateNewConversation"
                  Class="mb-3">
            New Chat
        </MudButton>

        <!-- Conversation List -->
        <ConversationList />
    </div>

    <div class="sidebar-footer">
        <!-- Settings, About, etc. -->
    </div>
</div>
```

## Dependencies

The component requires the following services to be injected:

- `IState<ChatState>` - Fluxor state management
- `IDispatcher` - Fluxor action dispatcher
- `IDialogService` - MudBlazor dialogs
- `ISnackbar` - MudBlazor notifications
- `IJSRuntime` - JavaScript interop for downloads

All dependencies are automatically resolved by Blazor DI.

## State Management

The component uses Fluxor actions for all operations:

### Actions Dispatched

| Action | Purpose |
|--------|---------|
| `SetActiveConversationAction` | Switch to a conversation |
| `TogglePinConversationAction` | Pin/unpin a conversation |
| `RenameConversationAction` | Rename a conversation |
| `ShareConversationAction` | Share a conversation |
| `ArchiveConversationAction` | Archive a conversation |
| `DeleteConversationAction` | Delete a conversation |

### State Properties Used

- `ChatState.Conversations` - Dictionary of all conversations
- `ChatState.ActiveConversationId` - Currently selected conversation

## Context Menu Actions

Each conversation has a context menu (three-dot icon) with:

1. **Pin/Unpin** - Toggle pinned status (pinned conversations appear first)
2. **Rename** - Opens dialog to rename the conversation
3. **Share** - Share conversation (triggers ShareDialog)
4. **Download** - Export conversation as JSON file
5. **Archive** - Archive the conversation (with confirmation)
6. **Delete** - Delete conversation (with confirmation)

## Download Format

Downloaded conversations are exported as JSON with the following structure:

```json
{
  "id": "guid",
  "title": "Conversation Title",
  "created_at": "2025-01-18T10:30:00Z",
  "updated_at": "2025-01-18T11:45:00Z",
  "personality": "kiwi-mate",
  "workspace_id": "guid",
  "is_pinned": false,
  "messages": [
    {
      "id": "guid",
      "role": "User",
      "content": "Hello!",
      "created_at": "2025-01-18T10:30:00Z",
      "model": "gpt-4"
    },
    {
      "id": "guid",
      "role": "Assistant",
      "content": "Kia ora! How can I help?",
      "created_at": "2025-01-18T10:30:05Z",
      "model": "gpt-4"
    }
  ]
}
```

Files are named: `{Title}_{Timestamp}.json`

## Styling

The component includes scoped CSS (`ConversationList.razor.css`) with:

- Hover effects on conversation items
- Active conversation highlighting
- Pin icon styling
- Smooth transitions
- Responsive design

### Custom Styling

Override styles in your global CSS:

```css
.conversation-list-container {
    /* Your custom styles */
}

.conversation-item-wrapper.active {
    background-color: var(--my-custom-color);
}
```

## Keyboard Navigation

Future enhancement - could add:
- Arrow keys to navigate conversations
- Enter to select
- Delete key to delete (with confirmation)

## Filtering/Search

To add search functionality, wrap the component:

```razor
<MudTextField @bind-Value="searchQuery"
             Label="Search Conversations"
             Adornment="Adornment.End"
             AdornmentIcon="@Icons.Material.Filled.Search" />

<ConversationList />
```

Then implement a filtering action in your ChatState.

## Example: Full Sidebar Integration

```razor
@page "/layout"
@using Fluxor
@using AiMate.Web.Store.Chat
@using AiMate.Web.Components.Shared
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IState<ChatState> ChatState
@inject IDispatcher Dispatcher

<MudLayout>
    <MudDrawer Open="true" Elevation="2" Variant="DrawerVariant.Persistent">
        <MudDrawerHeader>
            <MudText Typo="Typo.h5">aiMate</MudText>
        </MudDrawerHeader>

        <MudDivider />

        <div style="padding: 16px;">
            <MudButton Variant="Variant.Filled"
                      Color="Color.Primary"
                      FullWidth="true"
                      StartIcon="@Icons.Material.Filled.Add"
                      OnClick="@CreateNewChat">
                New Chat
            </MudButton>
        </div>

        <ConversationList />

        <MudSpacer />

        <MudDivider />

        <div style="padding: 16px;">
            <MudButton Variant="Variant.Text"
                      StartIcon="@Icons.Material.Filled.Settings"
                      Href="/settings"
                      FullWidth="true">
                Settings
            </MudButton>
        </div>
    </MudDrawer>

    <MudMainContent>
        @Body
    </MudMainContent>
</MudLayout>

@code {
    private void CreateNewChat()
    {
        Dispatcher.Dispatch(new CreateConversationAction("New Chat"));
    }
}
```

## Testing

To test the component:

1. **Create conversations**: Dispatch `CreateConversationAction`
2. **Pin a conversation**: Right-click menu → Pin
3. **Rename**: Right-click menu → Rename → Enter new name
4. **Download**: Right-click menu → Download → Check Downloads folder
5. **Archive**: Right-click menu → Archive → Confirm
6. **Delete**: Right-click menu → Delete → Confirm

## Troubleshooting

### Download not working

Ensure `wwwroot/js/app.js` is loaded:
```html
<script src="js/app.js"></script>
```

Check browser console for errors.

### Context menu not appearing

Ensure `ConversationContextMenu.razor` is in the same namespace:
```razor
@using AiMate.Web.Components.Shared
```

### Conversations not updating

Check Fluxor DevTools (browser extension) to verify:
1. Actions are dispatched
2. Reducers are updating state
3. Component is subscribed to state changes (inherits from FluxorComponent)

## Future Enhancements

- [ ] Drag-and-drop reordering
- [ ] Bulk actions (select multiple, delete all)
- [ ] Conversation grouping by date
- [ ] Search/filter conversations
- [ ] Keyboard shortcuts
- [ ] Conversation folders/categories
- [ ] Export multiple conversations at once
- [ ] Import conversations from JSON

## Related Components

- `ConversationContextMenu.razor` - Context menu with all actions
- `RenameConversationDialog.razor` - Rename dialog
- `ShareDialog.razor` - Share conversation dialog (if implemented)

## State Management Reference

See also:
- `Store/Chat/ChatState.cs` - State definition
- `Store/Chat/ChatActions.cs` - Action definitions
- `Store/Chat/ChatReducers.cs` - State update logic

---

**Built with ❤️ from New Zealand** 🇳🇿
