# Frontend-Backend Integration Guide
**Date:** November 22, 2025
**Purpose:** Map Tailwind/Blazor components to backend API endpoints

---

## Overview

This document outlines the integration points between frontend components and the backend APIs recently implemented. Each component needs to call the appropriate API endpoints to replace mock data and placeholder functionality.

**Components Documented:** 12 total
- **Search:** 2 components (GlobalSearchDialog, Search page)
- **Files:** 3 components (Files page, FileUploadDialog, AttachFilesMenu)
- **Feedback:** 2 components (MessageFeedbackWidget, FeedbackTagsAdminTab)
- **Admin:** 3 components (OverviewAdminTab, UsersAdminTab, FeedbackTagsAdminTab)
- **Settings:** 3 components (AccountSettingsTab, ConnectionsSettingsTab, UsageSettingsTab)

**Backend APIs Covered:**
- Search API (`/api/v1/search`)
- File Management API (`/api/v1/files`)
- Feedback System API (`/api/v1/feedback`)
- Admin API (`/api/v1/admin`)
- Connection Management API (`/api/v1/connections`)
- User Profile API (`/api/v1/users`)

---

## 🔍 Search Integration

### Components Requiring Integration

#### 1. **GlobalSearchDialog.razor** (`Components/Search/GlobalSearchDialog.razor`)

**Current State:**
- ✅ UI complete with filters (Chats, Files, Artifacts, Notes, Knowledge)
- ❌ Using mock data (`GenerateMockResults()` on line 379-404)
- ❌ Client-side search only

**Backend API Available:**
- **Endpoint:** `GET /api/v1/search`
- **Controller:** `SearchApiController.cs`
- **Methods:**
  - `GET /api/v1/search/conversations?query={query}&limit={limit}`
  - `GET /api/v1/search/messages?query={query}&limit={limit}`
  - `GET /api/v1/search/knowledge/semantic?query={query}&threshold={threshold}&limit={limit}`
  - `GET /api/v1/search/knowledge?query={query}&limit={limit}`
  - `GET /api/v1/search?query={query}&limit={limit}` (global search)

**Integration Steps:**

1. **Inject HttpClient:**
   ```csharp
   @inject HttpClient Http
   ```

2. **Replace `OnSearchQueryChanged` method:**
   ```csharp
   private async Task OnSearchQueryChanged(string query)
   {
       _searchQuery = query;
       _isSearching = true;
       StateHasChanged();

       try
       {
           // Call global search API
           var response = await Http.GetFromJsonAsync<GlobalSearchResults>(
               $"/api/v1/search?query={Uri.EscapeDataString(query)}&limit=10");

           if (response != null)
           {
               _allResults = MapToSearchResults(response);
           }
       }
       catch (Exception ex)
       {
           Snackbar.Add($"Search failed: {ex.Message}", Severity.Error);
       }
       finally
       {
           _isSearching = false;
           _selectedIndex = 0;
           StateHasChanged();
       }
   }
   ```

3. **Create mapping method:**
   ```csharp
   private List<SearchResult> MapToSearchResults(GlobalSearchResults apiResponse)
   {
       var results = new List<SearchResult>();

       // Map conversations
       results.AddRange(apiResponse.Conversations.Select(c => new SearchResult
       {
           Type = "Chats",
           Title = c.Item.Title,
           Preview = c.Highlight ?? "Conversation...",
           Location = c.Item.Workspace?.Name ?? "Unknown Workspace",
           UpdatedAt = c.Item.UpdatedAt,
           Url = $"/chat/{c.Item.Id}"
       }));

       // Map messages
       results.AddRange(apiResponse.Messages.Select(m => new SearchResult
       {
           Type = "Messages",
           Title = m.Item.Content.Substring(0, Math.Min(50, m.Item.Content.Length)) + "...",
           Preview = m.Highlight ?? m.Item.Content,
           Location = "Message",
           UpdatedAt = m.Item.CreatedAt,
           Url = $"/chat/{m.Item.ConversationId}"
       }));

       // Map knowledge items
       results.AddRange(apiResponse.KnowledgeItems.Select(k => new SearchResult
       {
           Type = "Knowledge",
           Title = k.Item.Title,
           Preview = k.Highlight ?? k.Item.Content.Substring(0, Math.Min(100, k.Item.Content.Length)),
           Location = "Knowledge Base",
           UpdatedAt = k.Item.UpdatedAt,
           Url = $"/knowledge/{k.Item.Id}"
       }));

       return results;
   }
   ```

4. **Remove mock data:**
   - Delete `GenerateMockResults()` method (lines 379-404)

**Response Models Needed:**
```csharp
public class GlobalSearchResults
{
    public List<SearchResult<Conversation>> Conversations { get; set; }
    public List<SearchResult<Message>> Messages { get; set; }
    public List<SearchResult<KnowledgeItem>> KnowledgeItems { get; set; }
    public int TotalResults { get; set; }
    public string Query { get; set; }
    public long QueryTimeMs { get; set; }
}

public class SearchResult<T>
{
    public T Item { get; set; }
    public double Score { get; set; }
    public string? Highlight { get; set; }
}
```

---

#### 2. **Search.razor** (`Components/Pages/Search.razor`)

**Current State:**
- ✅ UI with tabs (All, Conversations, Messages)
- ❌ Client-side search only (lines 71-94)
- ❌ Searches in-memory ChatState

**Backend API Available:**
- Same as GlobalSearchDialog above

**Integration Steps:**

1. **Add API calls to search methods:**
   ```csharp
   private async Task<List<Core.Entities.Conversation>> GetConversationResults()
   {
       if (string.IsNullOrWhiteSpace(_searchQuery))
           return new List<Core.Entities.Conversation>();

       try
       {
           var response = await Http.GetFromJsonAsync<SearchResults<Core.Entities.Conversation>>(
               $"/api/v1/search/conversations?query={Uri.EscapeDataString(_searchQuery)}&limit=50");

           return response?.Results.Select(r => r.Item).ToList() ?? new();
       }
       catch
       {
           return new List<Core.Entities.Conversation>();
       }
   }

   private async Task<List<Core.Entities.Message>> GetMessageSearchResults()
   {
       if (string.IsNullOrWhiteSpace(_searchQuery))
           return new List<Core.Entities.Message>();

       try
       {
           var response = await Http.GetFromJsonAsync<SearchResults<Core.Entities.Message>>(
               $"/api/v1/search/messages?query={Uri.EscapeDataString(_searchQuery)}&limit=50");

           return response?.Results.Select(r => r.Item).ToList() ?? new();
       }
       catch
       {
           return new List<Core.Entities.Message>();
       }
   }
   ```

2. **Add debouncing for API calls:**
   ```csharp
   private CancellationTokenSource? _searchCts;

   private async Task OnSearchQueryChanged(ChangeEventArgs e)
   {
       _searchQuery = e.Value?.ToString() ?? string.Empty;

       // Cancel previous search
       _searchCts?.Cancel();
       _searchCts = new CancellationTokenSource();

       try
       {
           await Task.Delay(300, _searchCts.Token); // Debounce 300ms
           StateHasChanged(); // Trigger re-render with new results
       }
       catch (TaskCanceledException)
       {
           // Debounce cancelled
       }
   }
   ```

---

## 📁 File Management Integration

### Components Requiring Integration

#### 3. **Files.razor** (`Components/Pages/Files.razor`)

**Current State:**
- ✅ UI with Grid/List views
- ✅ Search and filter controls
- ❌ Empty state, no API integration (line 166: "File API not implemented yet")
- ❌ All methods are stubs

**Backend API Available:**
- **Base Path:** `/api/v1/files`
- **Controller:** `FileApiController.cs`
- **Endpoints:**
  - `POST /api/v1/files/upload?workspaceId={id}` - Upload file
  - `GET /api/v1/files/{id}/download` - Download file
  - `DELETE /api/v1/files/{id}` - Delete file
  - `GET /api/v1/files?workspaceId={id}` - List files
  - `GET /api/v1/files/{id}` - Get file info

**Integration Steps:**

1. **Add service injection:**
   ```csharp
   @inject HttpClient Http
   @inject IJSRuntime JS
   ```

2. **Load files on initialization:**
   ```csharp
   [Parameter]
   [SupplyParameterFromQuery]
   public Guid? WorkspaceId { get; set; }

   protected override async Task OnInitializedAsync()
   {
       if (WorkspaceId == null)
       {
           // Get current workspace from state or default
           WorkspaceId = await GetCurrentWorkspaceId();
       }

       await LoadFiles();
   }

   private async Task LoadFiles()
   {
       _isLoading = true;
       try
       {
           var response = await Http.GetFromJsonAsync<List<WorkspaceFileDto>>(
               $"/api/v1/files?workspaceId={WorkspaceId}");

           if (response != null)
           {
               _files = response.Select(MapToFileDto).ToList();
           }
       }
       catch (Exception ex)
       {
           Snackbar.Add($"Failed to load files: {ex.Message}", Severity.Error);
       }
       finally
       {
           _isLoading = false;
       }
   }

   private FileDto MapToFileDto(WorkspaceFileDto apiFile)
   {
       return new FileDto
       {
           Id = apiFile.Id,
           Name = apiFile.FileName,
           Type = DetermineFileType(apiFile.ContentType),
           Size = apiFile.FileSize,
           UploadedAt = apiFile.UploadedAt
       };
   }

   private string DetermineFileType(string contentType)
   {
       if (contentType.StartsWith("image/")) return "Image";
       if (contentType.Contains("pdf") || contentType.Contains("document")) return "Document";
           if (contentType.Contains("text/") || contentType.Contains("code")) return "Code";
       return "Other";
   }
   ```

3. **Implement file upload:**
   ```csharp
   private async Task HandleFileUpload(IReadOnlyList<IBrowserFile> files)
   {
       if (files == null || !files.Any() || WorkspaceId == null)
           return;

       _isLoading = true;
       var successful = 0;

       try
       {
           foreach (var file in files)
           {
               using var content = new MultipartFormDataContent();
               var fileContent = new StreamContent(file.OpenReadStream(50 * 1024 * 1024)); // 50MB max
               fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

               content.Add(fileContent, "file", file.Name);

               var response = await Http.PostAsync(
                   $"/api/v1/files/upload?workspaceId={WorkspaceId}",
                   content);

               if (response.IsSuccessStatusCode)
               {
                   successful++;
               }
           }

           Snackbar.Add($"Successfully uploaded {successful} of {files.Count} file(s)", Severity.Success);
           await LoadFiles(); // Refresh list
       }
       catch (Exception ex)
       {
           Snackbar.Add($"Upload failed: {ex.Message}", Severity.Error);
       }
       finally
       {
           _isLoading = false;
       }
   }
   ```

4. **Implement download:**
   ```csharp
   private async Task DownloadFile(FileDto file)
   {
       try
       {
           var response = await Http.GetAsync($"/api/v1/files/{file.Id}/download");

           if (response.IsSuccessStatusCode)
           {
               var stream = await response.Content.ReadAsStreamAsync();
               var fileName = file.Name;

               // Use JS interop to trigger download
               using var streamRef = new DotNetStreamReference(stream);
               await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

               Snackbar.Add($"Downloading {fileName}", Severity.Success);
           }
       }
       catch (Exception ex)
       {
           Snackbar.Add($"Download failed: {ex.Message}", Severity.Error);
       }
   }
   ```

5. **Implement delete:**
   ```csharp
   private async Task DeleteFile(FileDto file)
   {
       var confirmed = await JS.InvokeAsync<bool>("confirm", $"Delete {file.Name}?");
       if (!confirmed) return;

       try
       {
           var response = await Http.DeleteAsync($"/api/v1/files/{file.Id}");

           if (response.IsSuccessStatusCode)
           {
               Snackbar.Add($"Deleted {file.Name}", Severity.Success);
               _files.Remove(file);
               StateHasChanged();
           }
       }
       catch (Exception ex)
       {
           Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
       }
   }
   ```

6. **Add JavaScript for download (wwwroot/js/fileDownload.js):**
   ```javascript
   window.downloadFileFromStream = async (fileName, streamReference) => {
       const arrayBuffer = await streamReference.arrayBuffer();
       const blob = new Blob([arrayBuffer]);
       const url = URL.createObjectURL(blob);
       const a = document.createElement('a');
       a.href = url;
       a.download = fileName ?? 'download';
       document.body.appendChild(a);
       a.click();
       document.body.removeChild(a);
       URL.revokeObjectURL(url);
   }
   ```

**Response Models Needed:**
```csharp
public class WorkspaceFileDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
}
```

---

#### 4. **FileUploadDialog.razor** (`Components/Shared/FileUploadDialog.razor`)

**Current State:**
- ✅ UI complete
- ⚠️ Uses `IFileUploadService` (lines 2, 55, 111-117)
- ❌ Service needs backend integration

**Backend API Available:**
- Same as Files.razor above

**Integration Steps:**

1. **Update IFileUploadService interface:**
   ```csharp
   public interface IFileUploadService
   {
       Task<bool> UploadFileAsync(Guid workspaceId, string fileName, Stream stream, string contentType);
       long GetFileSizeLimit();
   }
   ```

2. **Implement service:**
   ```csharp
   public class FileUploadService : IFileUploadService
   {
       private readonly HttpClient _http;
       private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

       public FileUploadService(HttpClient http)
       {
           _http = http;
       }

       public async Task<bool> UploadFileAsync(Guid workspaceId, string fileName, Stream stream, string contentType)
       {
           using var content = new MultipartFormDataContent();
           var fileContent = new StreamContent(stream);
           fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
           content.Add(fileContent, "file", fileName);

           var response = await _http.PostAsync(
               $"/api/v1/files/upload?workspaceId={workspaceId}",
               content);

           return response.IsSuccessStatusCode;
       }

       public long GetFileSizeLimit() => MaxFileSize;
   }
   ```

3. **Register in Program.cs:**
   ```csharp
   builder.Services.AddScoped<IFileUploadService, FileUploadService>();
   ```

---

#### 5. **AttachFilesMenu.razor** (`Components/Chat/AttachFilesMenu.razor`)

**Current State:**
- ✅ UI complete with multiple attachment types
- ❌ All methods are placeholders (lines 157-240)
- ❌ No backend integration

**Backend API Available:**
- File API: `/api/v1/files/*` (for file uploads)
- Search API: `/api/v1/search/*` (for knowledge/notes picker)

**Integration Steps:**

1. **Implement file attachment:**
   ```csharp
   @inject IJSRuntime JS
   @inject HttpClient Http

   private async Task HandleAttachFiles()
   {
       // Trigger file input using JS
       var files = await JS.InvokeAsync<IJSObjectReference>("showFilePicker");
       if (files != null)
       {
           // Process and upload files
           // Then call OnFilesAttached callback
       }
   }
   ```

2. **Implement knowledge picker:**
   ```csharp
   private async Task HandleAttachKnowledge()
   {
       var parameters = new DialogParameters
       {
           ["SearchEndpoint"] = "/api/v1/search/knowledge"
       };

       var dialog = await DialogService.ShowAsync<KnowledgePickerDialog>(
           "Select Knowledge Items",
           parameters);

       var result = await dialog.Result;
       if (!result.Canceled && result.Data is List<KnowledgeItem> items)
       {
           // Attach selected knowledge items to conversation
           await OnFilesAttached.InvokeAsync(MapKnowledgeToFiles(items));
       }
   }
   ```

---

## 💬 Feedback Integration

### Components Requiring Integration

#### 6. **MessageFeedbackWidget.razor** (`Components/Chat/MessageFeedbackWidget.razor`)

**Current State:**
- ✅ UI complete with rating slider and tags
- ⚠️ Uses Fluxor state management (lines 2-6)
- ❌ Dispatches actions but backend integration missing

**Backend API Available:**
- **Base Path:** `/api/v1/feedback`
- **Controller:** `FeedbackSystemApiController.cs`
- **Endpoints:**
  - `POST /api/v1/feedback/message` - Submit message feedback
  - `GET /api/v1/feedback/tags/templates` - Get tag templates
  - `GET /api/v1/feedback/message/{messageId}` - Get existing feedback

**Integration Steps:**

1. **Update Fluxor effects to call API:**

Create `FeedbackEffects.cs`:
```csharp
public class FeedbackEffects
{
    private readonly HttpClient _http;

    public FeedbackEffects(HttpClient http)
    {
        _http = http;
    }

    [EffectMethod]
    public async Task HandleLoadTagTemplates(LoadTagTemplatesAction action, IDispatcher dispatcher)
    {
        try
        {
            var templates = await _http.GetFromJsonAsync<List<FeedbackTagTemplate>>(
                "/api/v1/feedback/tags/templates");

            dispatcher.Dispatch(new LoadTagTemplatesSuccessAction(templates));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadTagTemplatesFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSubmitFeedback(SubmitFeedbackAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new SubmitMessageFeedbackRequest
            {
                MessageId = action.MessageId,
                Rating = action.Rating,
                TextFeedback = action.TextFeedback,
                Tags = action.Tags.ToDictionary(t => t.Key, t => t.Value),
                ModelId = action.ModelId,
                ResponseTimeMs = action.ResponseTimeMs
            };

            var response = await _http.PostAsJsonAsync("/api/v1/feedback/message", request);

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new SubmitFeedbackSuccessAction(action.MessageId));
            }
            else
            {
                dispatcher.Dispatch(new SubmitFeedbackFailureAction("Failed to submit feedback"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SubmitFeedbackFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleQuickRate(QuickRateMessageAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new SubmitMessageFeedbackRequest
            {
                MessageId = action.MessageId,
                Rating = action.Rating,
                ModelId = action.ModelId
            };

            await _http.PostAsJsonAsync("/api/v1/feedback/message", request);
            dispatcher.Dispatch(new SubmitFeedbackSuccessAction(action.MessageId));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SubmitFeedbackFailureAction(ex.Message));
        }
    }
}
```

2. **Register effects in Program.cs:**
```csharp
builder.Services.AddScoped<FeedbackEffects>();
```

**Request Models:**
```csharp
public class SubmitMessageFeedbackRequest
{
    public Guid MessageId { get; set; }
    public int Rating { get; set; }
    public string? TextFeedback { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public string? ModelId { get; set; }
    public long? ResponseTimeMs { get; set; }
}

public class FeedbackTagTemplate
{
    public Guid Id { get; set; }
    public string Category { get; set; }
    public string Label { get; set; }
    public List<FeedbackTagOption> Options { get; set; }
    public int DisplayOrder { get; set; }
}

public class FeedbackTagOption
{
    public string Value { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public TagSentiment Sentiment { get; set; }
    public int DisplayOrder { get; set; }
}
```

---

## 🎨 Admin Components Integration

### Components Requiring Integration

#### 7. **OverviewAdminTab.razor** (`Components/Admin/OverviewAdminTab.razor`)

**Current State:**
- ✅ UI complete with system stats cards
- ⚠️ Uses Fluxor AdminState for all data
- ❌ No backend API integration (mock/static data)
- Displays: TotalUsers, TotalConversations, ActiveModels, MCP servers, LiteLLM status

**Backend API Available:**
- **Endpoint:** `GET /api/v1/admin`
- **Controller:** `AdminApiController.cs`
- **Authorization:** Requires Admin tier (`[Authorize(Policy = "AdminOnly")]`)

**Integration Steps:**

1. **Create Fluxor effects for admin data:**

Create `Store/Admin/AdminEffects.cs`:
```csharp
public class AdminEffects
{
    private readonly HttpClient _http;
    private readonly ILogger<AdminEffects> _logger;

    public AdminEffects(HttpClient http, ILogger<AdminEffects> logger)
    {
        _http = http;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadAdminData(LoadAdminDataAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<AdminDataDto>("/api/v1/admin");

            if (response != null)
            {
                dispatcher.Dispatch(new LoadAdminDataSuccessAction(response));
            }
            else
            {
                dispatcher.Dispatch(new LoadAdminDataFailureAction("No data returned"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin data");
            dispatcher.Dispatch(new LoadAdminDataFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleRefreshAdminData(RefreshAdminDataAction action, IDispatcher dispatcher)
    {
        // Auto-refresh every 30 seconds
        await Task.Delay(TimeSpan.FromSeconds(30));
        dispatcher.Dispatch(new LoadAdminDataAction());
    }
}
```

2. **Update AdminState reducer:**

```csharp
[FeatureState]
public class AdminState
{
    public AdminOverviewDto? Overview { get; init; }
    public List<ActiveModelDto>? ActiveModels { get; init; }
    public List<MCPServerDto>? MCPServers { get; init; }
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? LastUpdated { get; init; }

    private AdminState() { } // Required for Fluxor

    public AdminState(
        AdminOverviewDto? overview,
        List<ActiveModelDto>? activeModels,
        List<MCPServerDto>? mcpServers,
        bool isLoading = false,
        string? errorMessage = null,
        DateTime? lastUpdated = null)
    {
        Overview = overview;
        ActiveModels = activeModels;
        MCPServers = mcpServers;
        IsLoading = isLoading;
        ErrorMessage = errorMessage;
        LastUpdated = lastUpdated;
    }
}

public static class AdminReducers
{
    [ReducerMethod]
    public static AdminState OnLoadAdminData(AdminState state, LoadAdminDataAction action)
    {
        return state with { IsLoading = true, ErrorMessage = null };
    }

    [ReducerMethod]
    public static AdminState OnLoadAdminDataSuccess(AdminState state, LoadAdminDataSuccessAction action)
    {
        return new AdminState(
            action.Data.Overview,
            action.Data.ActiveModels,
            action.Data.MCPServers,
            isLoading: false,
            lastUpdated: DateTime.UtcNow
        );
    }

    [ReducerMethod]
    public static AdminState OnLoadAdminDataFailure(AdminState state, LoadAdminDataFailureAction action)
    {
        return state with { IsLoading = false, ErrorMessage = action.ErrorMessage };
    }
}
```

3. **Update component to load data on init:**

```csharp
@inherits Fluxor.Blazor.Web.Components.FluxorComponent

protected override void OnInitialized()
{
    base.OnInitialized();

    // Subscribe to state changes
    SubscribeToAction<LoadAdminDataSuccessAction>(action =>
    {
        StateHasChanged();
    });

    // Load initial data
    Dispatcher.Dispatch(new LoadAdminDataAction());
}
```

**Response Models:**
```csharp
public class AdminDataDto
{
    public AdminOverviewDto Overview { get; set; }
    public List<ActiveModelDto> ActiveModels { get; set; }
    public List<MCPServerDto> MCPServers { get; set; }
}

public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalConversations { get; set; }
    public int ConversationsToday { get; set; }
    public int TotalMessages { get; set; }
    public int MessagesToday { get; set; }
    public int ActiveConnections { get; set; }
    public long StorageUsedBytes { get; set; }
    public int TotalKnowledgeItems { get; set; }
}

public class ActiveModelDto
{
    public string ModelId { get; set; }
    public string Provider { get; set; }
    public int UsageCount { get; set; }
    public DateTime LastUsed { get; set; }
}

public class MCPServerDto
{
    public string Name { get; set; }
    public string Status { get; set; }
    public int ToolCount { get; set; }
}
```

---

#### 8. **UsersAdminTab.razor** (`Components/Admin/UsersAdminTab.razor`)

**Current State:**
- ✅ UI shows single-user mode warning
- ❌ Multi-user management not implemented
- Shows current user stats from AdminState

**Backend API Available:**
- Currently in single-user mode (no multi-user endpoints yet)
- Future: `GET /api/v1/admin/users` - List all users
- Future: `PUT /api/v1/admin/users/{id}/tier` - Update user tier

**Integration Notes:**
This component is ready for future multi-user expansion. When multi-user support is added:
1. Create user management endpoints in AdminApiController
2. Add user CRUD operations
3. Implement tier management (Free → BYOK → Developer → Admin)
4. Add user activity tracking

**Current Implementation:**
No changes needed - component correctly shows single-user mode warning.

---

#### 9. **FeedbackTagsAdminTab.razor** (`Components/Admin/FeedbackTagsAdminTab.razor`)

**Backend API Available:**
- `GET /api/v1/feedback/admin/tags/templates` - List all tag templates
- `POST /api/v1/feedback/admin/tags/templates` - Create tag template
- `PUT /api/v1/feedback/admin/tags/templates/{id}` - Update tag template
- `DELETE /api/v1/feedback/admin/tags/templates/{id}` - Delete tag template
- `GET /api/v1/feedback/admin/stats` - Get feedback statistics

**Integration Steps:**
1. Inject HttpClient
2. Load tag templates on initialization
3. Implement CRUD operations
4. Display statistics dashboard

---

## ⚙️ Settings Components Integration

### Components Requiring Integration

#### 10. **AccountSettingsTab.razor** (`Components/Settings/AccountSettingsTab.razor`)

**Current State:**
- ✅ UI complete with username, email, avatar inputs
- ⚠️ Uses Fluxor SettingsState
- ❌ Dispatches actions but no backend sync
- Has tier selection dropdown (Free, BYOK, Developer)
- Delete account button (no implementation)

**Backend API Available:**
- `GET /api/v1/users/profile` - Get user profile
- `PUT /api/v1/users/profile` - Update user profile
- `DELETE /api/v1/users/account` - Delete account
- `GET /api/v1/users/tier` - Get current tier
- `PUT /api/v1/users/tier` - Update tier (admin only)

**Integration Steps:**

1. **Create SettingsEffects for profile updates:**

Create `Store/Settings/SettingsEffects.cs`:
```csharp
public class SettingsEffects
{
    private readonly HttpClient _http;
    private readonly ILogger<SettingsEffects> _logger;

    public SettingsEffects(HttpClient http, ILogger<SettingsEffects> logger)
    {
        _http = http;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadUserProfile(LoadUserProfileAction action, IDispatcher dispatcher)
    {
        try
        {
            var profile = await _http.GetFromJsonAsync<UserProfileDto>("/api/v1/users/profile");

            if (profile != null)
            {
                dispatcher.Dispatch(new LoadUserProfileSuccessAction(profile));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user profile");
            dispatcher.Dispatch(new LoadUserProfileFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateUsername(UpdateUsernameAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new UpdateProfileRequest { Username = action.Username };
            var response = await _http.PutAsJsonAsync("/api/v1/users/profile", request);

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new UpdateUsernameSuccessAction(action.Username));
            }
            else
            {
                dispatcher.Dispatch(new UpdateProfileFailureAction("Failed to update username"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update username");
            dispatcher.Dispatch(new UpdateProfileFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateEmail(UpdateEmailAction action, IDispatcher dispatcher)
    {
        try
        {
            var request = new UpdateProfileRequest { Email = action.Email };
            var response = await _http.PutAsJsonAsync("/api/v1/users/profile", request);

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new UpdateEmailSuccessAction(action.Email));
            }
            else
            {
                dispatcher.Dispatch(new UpdateProfileFailureAction("Failed to update email"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email");
            dispatcher.Dispatch(new UpdateProfileFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAccount(DeleteAccountAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await _http.DeleteAsync("/api/v1/users/account");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteAccountSuccessAction());
                // Redirect to logout or goodbye page
            }
            else
            {
                dispatcher.Dispatch(new DeleteAccountFailureAction("Failed to delete account"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete account");
            dispatcher.Dispatch(new DeleteAccountFailureAction(ex.Message));
        }
    }
}
```

2. **Update component with delete confirmation:**

```csharp
private async Task HandleDeleteAccount()
{
    var confirmed = await DialogService.ShowMessageBox(
        "Delete Account",
        "Are you sure you want to delete your account? This action cannot be undone.",
        yesText: "Delete",
        cancelText: "Cancel");

    if (confirmed == true)
    {
        Dispatcher.Dispatch(new DeleteAccountAction());
    }
}
```

**Request/Response Models:**
```csharp
public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? AvatarUrl { get; set; }
    public UserTier Tier { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}
```

---

#### 11. **ConnectionsSettingsTab.razor** (`Components/Settings/ConnectionsSettingsTab.razor`)

**Current State:**
- ✅ Full BYOK connection management UI
- ⚠️ Uses Fluxor ConnectionState
- ❌ Dispatches actions but needs API integration
- Features: Add/Edit/Delete/Test connections
- Has LiteLLM legacy settings section

**Backend API Available:**
- **Base Path:** `/api/v1/connections`
- **Controller:** `ConnectionApiController.cs`
- **Endpoints:**
  - `GET /api/v1/connections?userId={id}&tierStr={tier}` - List connections
  - `GET /api/v1/connections/{id}?userId={id}&tierStr={tier}` - Get connection
  - `POST /api/v1/connections?userId={id}&tierStr={tier}` - Create connection
  - `PUT /api/v1/connections/{id}?userId={id}&tierStr={tier}` - Update connection
  - `DELETE /api/v1/connections/{id}?userId={id}&tierStr={tier}` - Delete connection
  - `POST /api/v1/connections/{id}/test?userId={id}` - Test connection
  - `GET /api/v1/connections/limits?tierStr={tier}` - Get tier limits

**Integration Steps:**

1. **Create ConnectionEffects for API calls:**

Create `Store/Connection/ConnectionEffects.cs`:
```csharp
public class ConnectionEffects
{
    private readonly HttpClient _http;
    private readonly ILogger<ConnectionEffects> _logger;
    private readonly IState<SettingsState> _settingsState;

    public ConnectionEffects(
        HttpClient http,
        ILogger<ConnectionEffects> logger,
        IState<SettingsState> settingsState)
    {
        _http = http;
        _logger = logger;
        _settingsState = settingsState;
    }

    [EffectMethod]
    public async Task HandleLoadConnections(LoadConnectionsAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = _settingsState.Value.UserId;
            var tier = _settingsState.Value.Tier;

            var connections = await _http.GetFromJsonAsync<List<ProviderConnectionDto>>(
                $"/api/v1/connections?userId={userId}&tierStr={tier}");

            if (connections != null)
            {
                dispatcher.Dispatch(new LoadConnectionsSuccessAction(connections));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connections");
            dispatcher.Dispatch(new LoadConnectionsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleCreateConnection(CreateConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = _settingsState.Value.UserId;
            var tier = _settingsState.Value.Tier;

            var response = await _http.PostAsJsonAsync(
                $"/api/v1/connections?userId={userId}&tierStr={tier}",
                action.Connection);

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<ProviderConnectionDto>();
                dispatcher.Dispatch(new CreateConnectionSuccessAction(created!));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new CreateConnectionFailureAction(error));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadFromJsonAsync<dynamic>();
                dispatcher.Dispatch(new CreateConnectionFailureAction(error?.message ?? "Limit reached"));
            }
            else
            {
                dispatcher.Dispatch(new CreateConnectionFailureAction("Failed to create connection"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection");
            dispatcher.Dispatch(new CreateConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateConnection(UpdateConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = _settingsState.Value.UserId;
            var tier = _settingsState.Value.Tier;

            var response = await _http.PutAsJsonAsync(
                $"/api/v1/connections/{action.Connection.Id}?userId={userId}&tierStr={tier}",
                action.Connection);

            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<ProviderConnectionDto>();
                dispatcher.Dispatch(new UpdateConnectionSuccessAction(updated!));
            }
            else
            {
                dispatcher.Dispatch(new UpdateConnectionFailureAction("Failed to update connection"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update connection");
            dispatcher.Dispatch(new UpdateConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteConnection(DeleteConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = _settingsState.Value.UserId;
            var tier = _settingsState.Value.Tier;

            var response = await _http.DeleteAsync(
                $"/api/v1/connections/{action.ConnectionId}?userId={userId}&tierStr={tier}");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteConnectionSuccessAction(action.ConnectionId));
            }
            else
            {
                dispatcher.Dispatch(new DeleteConnectionFailureAction("Failed to delete connection"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete connection");
            dispatcher.Dispatch(new DeleteConnectionFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleTestConnection(TestConnectionAction action, IDispatcher dispatcher)
    {
        try
        {
            var userId = _settingsState.Value.UserId;

            var response = await _http.PostAsync(
                $"/api/v1/connections/{action.ConnectionId}/test?userId={userId}",
                null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TestConnectionResult>();
                dispatcher.Dispatch(new TestConnectionSuccessAction(
                    action.ConnectionId,
                    result!.Success,
                    result.Message));
            }
            else
            {
                dispatcher.Dispatch(new TestConnectionFailureAction(
                    action.ConnectionId,
                    "Test failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection");
            dispatcher.Dispatch(new TestConnectionFailureAction(action.ConnectionId, ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadConnectionLimits(LoadConnectionLimitsAction action, IDispatcher dispatcher)
    {
        try
        {
            var tier = _settingsState.Value.Tier;

            var limits = await _http.GetFromJsonAsync<ConnectionLimitsDto>(
                $"/api/v1/connections/limits?tierStr={tier}");

            if (limits != null)
            {
                dispatcher.Dispatch(new LoadConnectionLimitsSuccessAction(limits));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connection limits");
        }
    }
}
```

2. **Update component to load data on init:**

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();

    // Load connections and limits
    Dispatcher.Dispatch(new LoadConnectionsAction());
    Dispatcher.Dispatch(new LoadConnectionLimitsAction());
}
```

3. **Add tier limit display:**

```csharp
@if (ConnectionState.Value.Limits != null)
{
    <div class="mb-4 p-4 bg-blue-50 dark:bg-blue-900 rounded-lg">
        <div class="text-sm">
            <strong>@ConnectionState.Value.Connections.Count</strong> of
            <strong>@ConnectionState.Value.Limits.MaxConnections</strong> connections used
        </div>
        @if (ConnectionState.Value.Connections.Count >= ConnectionState.Value.Limits.MaxConnections)
        {
            <div class="text-sm text-red-600 dark:text-red-400 mt-2">
                ⚠️ Connection limit reached. Upgrade your tier for more connections.
            </div>
        }
    </div>
}
```

**Request/Response Models:**
```csharp
public class ProviderConnectionDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } // "Cloud", "Local", "MCP"
    public string? Url { get; set; }
    public string Auth { get; set; } // "ApiKey", "Bearer", etc.
    public string? ApiKey { get; set; }
    public string ProviderType { get; set; } // "OpenAI", "Anthropic", etc.
    public List<string>? ModelIds { get; set; }
    public bool IsEnabled { get; set; }
    public string? OwnerId { get; set; }
    public string Visibility { get; set; } // "Private", "Group", "Organization"
    public List<string>? Tags { get; set; }
}

public class TestConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int ModelCount { get; set; }
    public long LatencyMs { get; set; }
}

public class ConnectionLimitsDto
{
    public string Tier { get; set; }
    public int MaxConnections { get; set; }
    public bool ByokEnabled { get; set; }
    public bool CanAddOwnKeys { get; set; }
    public bool CanAddCustomEndpoints { get; set; }
    public bool CanShareConnections { get; set; }
    public bool CanUsePlatformKeys { get; set; }
}
```

---

#### 12. **UsageSettingsTab.razor** (`Components/Settings/UsageSettingsTab.razor`)

**Current State:**
- ✅ UI complete with usage charts
- ❌ Using hardcoded mock data (147,532 tokens, $4.23 NZD)
- Shows usage by model table
- Has export usage data button (no implementation)

**Backend API Available:**
- `GET /api/v1/users/usage` - Get usage statistics
- `GET /api/v1/users/usage/export?format={format}` - Export usage data (CSV, JSON)
- `GET /api/v1/users/usage/models` - Get usage by model

**Integration Steps:**

1. **Create usage effects:**

```csharp
[EffectMethod]
public async Task HandleLoadUsageData(LoadUsageDataAction action, IDispatcher dispatcher)
{
    try
    {
        var usage = await _http.GetFromJsonAsync<UsageDataDto>("/api/v1/users/usage");

        if (usage != null)
        {
            dispatcher.Dispatch(new LoadUsageDataSuccessAction(usage));
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load usage data");
        dispatcher.Dispatch(new LoadUsageDataFailureAction(ex.Message));
    }
}
```

2. **Implement export functionality:**

```csharp
private async Task HandleExportUsage()
{
    try
    {
        var response = await Http.GetAsync("/api/v1/users/usage/export?format=csv");

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var fileName = $"usage-export-{DateTime.UtcNow:yyyy-MM-dd}.csv";

            using var streamRef = new DotNetStreamReference(stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

            Snackbar.Add("Usage data exported successfully", Severity.Success);
        }
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Export failed: {ex.Message}", Severity.Error);
    }
}
```

3. **Replace mock data:**

Remove hardcoded values and bind to state:
```csharp
<div class="text-2xl font-semibold">
    @SettingsState.Value.Usage?.TotalTokens.ToString("N0") tokens
</div>
<div class="text-sm text-gray-500">
    @SettingsState.Value.Usage?.EstimatedCost.ToString("C2") NZD
</div>
```

**Response Models:**
```csharp
public class UsageDataDto
{
    public long TotalTokens { get; set; }
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; }
    public List<ModelUsageDto> ModelUsage { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class ModelUsageDto
{
    public string ModelId { get; set; }
    public string Provider { get; set; }
    public long TokensUsed { get; set; }
    public int RequestCount { get; set; }
    public decimal Cost { get; set; }
}
```

---

## 🔑 Authentication & Authorization

### Required Headers

All API calls must include JWT authentication:

```csharp
// Add to HttpClient configuration in Program.cs
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

    // Add JWT token to all requests
    var authState = sp.GetService<AuthenticationStateProvider>();
    if (authState != null)
    {
        var state = await authState.GetAuthenticationStateAsync();
        var token = state.User.FindFirst("access_token")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    return http;
});
```

---

## 📊 Rate Limiting Awareness

Backend has rate limiting configured:
- **API**: 60 requests/minute (default)
- **Developer tier**: 120 requests/minute
- **Admin**: 200 requests/minute
- **Error logging**: 10 requests/minute

### Frontend Handling

Implement exponential backoff for 429 responses:

```csharp
public class RetryHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var maxRetries = 3;
        var retryCount = 0;

        while (true)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests || retryCount >= maxRetries)
            {
                return response;
            }

            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount));
            await Task.Delay(retryAfter, cancellationToken);
            retryCount++;
        }
    }
}
```

---

## 🚀 Quick Start Checklist

For each component integration:

- [ ] Remove mock data and placeholder methods
- [ ] Inject HttpClient or relevant service
- [ ] Create API request/response DTOs
- [ ] Implement API calls with proper error handling
- [ ] Add loading states (spinners, skeletons)
- [ ] Handle 401 Unauthorized (redirect to login)
- [ ] Handle 403 Forbidden (show permission error)
- [ ] Handle 429 Too Many Requests (retry with backoff)
- [ ] Handle 500 Server Error (show friendly error)
- [ ] Add success notifications (Snackbar)
- [ ] Test with real backend

---

## 📝 Priority Order

Recommended implementation order:

### Phase 1: Core User Features (High Priority)
1. **Search** - GlobalSearchDialog + Search.razor (most visible feature)
2. **Files** - Files.razor + FileUploadDialog (file management essential)
3. **Feedback** - MessageFeedbackWidget (alpha testing critical)

### Phase 2: Enhanced UX (Medium Priority)
4. **Attach Menu** - AttachFilesMenu (enhances chat experience)
5. **Account Settings** - AccountSettingsTab (user profile management)
6. **Connections** - ConnectionsSettingsTab (BYOK functionality)

### Phase 3: Admin & Analytics (Lower Priority)
7. **Admin Dashboard** - OverviewAdminTab (system monitoring)
8. **Usage Tracking** - UsageSettingsTab (cost visibility)
9. **Feedback Admin** - FeedbackTagsAdminTab (admin tools)

**Note:** UsersAdminTab is future-ready but doesn't require immediate integration (single-user mode).

---

## 🔗 API Documentation

Full API documentation available at: `/api/docs` (Swagger UI)

- Browse all endpoints
- Test API calls directly
- View request/response schemas
- Copy curl commands for testing

---

## ⚠️ Known Issues & TODOs

1. **MudBlazor to Tailwind Migration:**
   - GlobalSearchDialog still uses MudBlazor components
   - Need to convert to Tailwind CSS classes
   - Consider creating reusable Tailwind components

2. **WebSocket for Real-time Updates:**
   - File uploads could use SignalR for progress
   - Search could show live results
   - Feedback could update in real-time

3. **Caching Strategy:**
   - Backend has 2-minute cache for search results
   - Frontend should respect cache headers
   - Consider client-side caching for static data

---

## 📞 Support

For questions or issues:
- Check Swagger docs: `/api/docs`
- Review backend tests: `tests/AiMate.Infrastructure.Tests/`
- Backend implementation: `src/AiMate.Web/Controllers/`

---

**Last Updated:** November 22, 2025
**Version:** 1.0
**Status:** Ready for Implementation
