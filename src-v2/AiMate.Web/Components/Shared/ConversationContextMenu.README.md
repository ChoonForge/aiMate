# ConversationContextMenu Component

## Overview
A reusable context menu component for conversation actions with built-in confirmation dialogs.

## Features
- ✅ Pin/Unpin conversations
- ✅ Rename conversations
- ✅ Share conversations
- ✅ Download conversations
- ✅ Archive conversations
- ✅ Delete conversations (with confirmation)

## Usage

### In a conversation list component:

```razor
@foreach (var conversation in conversations)
{
    <div class="conversation-item">
        <span>@conversation.Title</span>

        <ConversationContextMenu
            ConversationId="@conversation.Id"
            ConversationTitle="@conversation.Title"
            IsPinned="@conversation.IsPinned"
            OnPinToggled="@(() => HandlePinToggle(conversation.Id))"
            OnRenamed="@(() => HandleRename(conversation.Id))"
            OnShared="@(() => HandleShare(conversation.Id))"
            OnDownloaded="@(() => HandleDownload(conversation.Id))"
            OnArchived="@(() => HandleArchive(conversation.Id))"
            OnDeleted="@(() => HandleDelete(conversation.Id))" />
    </div>
}
```

### In code-behind:

```csharp
private async Task HandlePinToggle(Guid conversationId)
{
    Dispatcher.Dispatch(new TogglePinAction(conversationId));
}

private async Task HandleRename(Guid conversationId)
{
    // Rename handled by dialog, just reload data
    Dispatcher.Dispatch(new LoadConversationsAction());
}

private async Task HandleShare(Guid conversationId)
{
    // Open share dialog (already exists: ShareConversationDialog.razor)
    var parameters = new DialogParameters { ["ConversationId"] = conversationId };
    await DialogService.ShowAsync<ShareConversationDialog>("Share Conversation", parameters);
}

private async Task HandleDownload(Guid conversationId)
{
    var conversation = await ConversationService.GetConversationAsync(conversationId);

    // Generate JSON export
    var json = JsonSerializer.Serialize(conversation, new JsonSerializerOptions { WriteIndented = true });
    var bytes = Encoding.UTF8.GetBytes(json);

    // Trigger browser download
    await JS.InvokeVoidAsync("downloadFile",
        $"{conversation.Title}.json",
        "application/json",
        Convert.ToBase64String(bytes));
}

private async Task HandleArchive(Guid conversationId)
{
    Dispatcher.Dispatch(new ArchiveConversationAction(conversationId));
}

private async Task HandleDelete(Guid conversationId)
{
    // Confirmation already handled by component
    Dispatcher.Dispatch(new DeleteConversationAction(conversationId));
}
```

### JavaScript helper for downloads (wwwroot/js/app.js):

```javascript
window.downloadFile = function (filename, contentType, base64Data) {
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', `data:${contentType};base64,${base64Data}`);
    linkElement.setAttribute('download', filename);
    linkElement.style.display = 'none';
    document.body.appendChild(linkElement);
    linkElement.click();
    document.body.removeChild(linkElement);
};
```

## Integration Points

### Where to add this component:

1. **Sidebar conversation list** (`Components/Layout/Sidebar.razor`)
   - Add to each conversation item in the sidebar

2. **Main chat interface** (`Components/Pages/Index.razor`)
   - Add to conversation header/toolbar

3. **Workspaces page** (`Components/Pages/Workspaces.razor`)
   - Add to each workspace card for workspace-specific conversations

## State Management Integration

### Actions needed in your Flux store:

```csharp
// Add to Store/Conversation/ConversationActions.cs
public record TogglePinAction(Guid ConversationId);
public record ArchiveConversationAction(Guid ConversationId);
public record DeleteConversationAction(Guid ConversationId);
public record RenameConversationAction(Guid ConversationId, string NewName);
```

### Effects needed:

```csharp
// In Store/Conversation/ConversationEffects.cs
[EffectMethod]
public async Task HandleTogglePin(TogglePinAction action, IDispatcher dispatcher)
{
    try
    {
        await _conversationService.TogglePinAsync(action.ConversationId);
        dispatcher.Dispatch(new LoadConversationsAction());
    }
    catch (Exception ex)
    {
        dispatcher.Dispatch(new SetErrorAction(ex.Message));
    }
}

// Similar for Archive, Delete, Rename actions
```

## Styling

The component uses MudBlazor's default menu styling. To customize:

```css
/* Add to wwwroot/css/app.css */
.mud-menu-item .mud-icon-root.mud-error {
    color: var(--mud-palette-error);
}
```

## See Also
- `ShareConversationDialog.razor` - Share dialog (already implemented)
- `RenameConversationDialog.razor` - Rename dialog (just created)
