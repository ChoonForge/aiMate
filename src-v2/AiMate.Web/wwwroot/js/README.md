# JavaScript Integration Guide

## Setup

### 1. Add script reference to your HTML/Blazor entry point

For Blazor Server, add to `_Host.cshtml` or `_Layout.cshtml`:
```html
<script src="~/js/app.js"></script>
```

For Blazor WebAssembly, add to `wwwroot/index.html`:
```html
<script src="js/app.js"></script>
```

### 2. Use in Blazor components

Inject IJSRuntime and call the functions:

```csharp
@inject IJSRuntime JS

@code {
    private async Task DownloadConversation(Conversation conversation)
    {
        // Serialize to JSON
        var json = JsonSerializer.Serialize(conversation, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Convert to base64
        var bytes = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);

        // Trigger download
        await JS.InvokeVoidAsync("downloadFile",
            $"{conversation.Title}.json",
            "application/json",
            base64);
    }

    private async Task CopyToClipboard(string text)
    {
        var success = await JS.InvokeAsync<bool>("copyToClipboard", text);
        if (success)
        {
            Snackbar.Add("Copied to clipboard!", Severity.Success);
        }
    }
}
```

## Available Functions

### downloadFile(filename, contentType, base64Data)
Downloads a file from base64-encoded data.

**Parameters:**
- `filename` (string): Name of the file to download
- `contentType` (string): MIME type (e.g., 'application/json', 'text/plain', 'text/csv')
- `base64Data` (string): Base64-encoded file content

**Example:**
```javascript
window.downloadFile('conversation.json', 'application/json', 'eyJ0aXRsZSI6IkhlbGxvIn0=');
```

### copyToClipboard(text)
Copies text to the system clipboard.

**Returns:** Promise<boolean> - Success status

**Example:**
```javascript
const success = await window.copyToClipboard('Hello, world!');
```

### focusElement(elementId)
Focuses an element by its ID.

**Example:**
```javascript
window.focusElement('message-input');
```

### scrollToElement(elementId, smooth)
Scrolls an element into view.

**Parameters:**
- `elementId` (string): DOM element ID
- `smooth` (boolean): Use smooth scrolling (default: true)

### getElementDimensions(elementId)
Gets the width and height of an element.

**Returns:** Object with `width` and `height` properties

## File Download Example (Full Implementation)

```csharp
@page "/conversations"
@inject IJSRuntime JS
@inject ISnackbar Snackbar

<ConversationContextMenu
    ConversationId="@conversation.Id"
    ConversationTitle="@conversation.Title"
    OnDownloaded="@(() => HandleDownload(conversation))" />

@code {
    private async Task HandleDownload(Conversation conversation)
    {
        try
        {
            // Create download data
            var exportData = new
            {
                title = conversation.Title,
                created_at = conversation.CreatedAt,
                updated_at = conversation.UpdatedAt,
                personality = conversation.Personality,
                messages = conversation.Messages.Select(m => new
                {
                    role = m.Role,
                    content = m.Content,
                    timestamp = m.CreatedAt
                }).ToList()
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Convert to base64
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filename = $"{conversation.Title}_{timestamp}.json";

            // Trigger download
            await JS.InvokeVoidAsync("downloadFile", filename, "application/json", base64);

            Snackbar.Add($"Downloaded: {filename}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Download failed: {ex.Message}", Severity.Error);
        }
    }
}
```

## Browser Compatibility

All functions include fallbacks for older browsers:
- **downloadFile**: Works in all modern browsers and IE11+
- **copyToClipboard**: Uses Clipboard API with fallback to document.execCommand
- **focusElement**: Universal support
- **scrollToElement**: Uses smooth scrolling with fallback to instant scroll
