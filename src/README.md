# aiMate - AI Chat Platform

A modern AI chat interface built with Blazor WebAssembly and MudBlazor, designed to replicate and improve upon Open WebUI.

## 🚀 Features

### Currently Implemented
- ✅ **Blazor WebAssembly** with .NET 9
- ✅ **MudBlazor UI** - Material Design components
- ✅ **Conversation Management** - Create, switch, archive, pin conversations
- ✅ **Real-time Streaming** - Token-by-token streaming from LiteLLM
- ✅ **Model Selection** - Support for multiple AI models
- ✅ **Persistent Sidebar** - Conversation history and navigation
- ✅ **Dark/Light Theme** - Purple-to-blue gradient theme
- ✅ **State Management** - Centralized AppStateService
- ✅ **Responsive Design** - Works on desktop, tablet, mobile

### Coming Soon
- 🔜 Knowledge Base Management
- 🔜 File Attachments & Uploads
- 🔜 Structured Content Rendering (tables, forms, code)
- 🔜 Search & Filter Conversations
- 🔜 Settings Panel (6 tabs)
- 🔜 Admin Panel
- 🔜 Local Storage Persistence
- 🔜 Tool Integration (MCP, web search)
- 🔜 Markdown Rendering
- 🔜 Code Syntax Highlighting
- 🔜 Message Editing & Regeneration
- 🔜 Export Conversations
- 🔜 User Preferences

## 📁 Project Structure

```
src/
├── AiMate.Client/              # Blazor WebAssembly app
│   ├── Pages/
│   │   └── Chat.razor          # Main chat interface
│   ├── Shared/
│   │   └── MainLayout.razor    # App layout with sidebar
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css         # Custom styles
│   │   ├── index.html          # Entry point
│   │   └── appsettings.json    # Configuration
│   ├── App.razor               # Router configuration
│   ├── Program.cs              # DI and startup
│   └── _Imports.razor          # Global using statements
├── AiMate.Shared/              # Shared library
│   ├── Models/
│   │   └── ChatModels.cs       # Data models
│   └── Services/
│       ├── AppStateService.cs  # State management
│       └── LiteLLMService.cs   # LiteLLM integration
└── AiMate.Server/              # (Future) Backend API
```

## 🛠️ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [LiteLLM](https://docs.litellm.ai/) running locally or accessible endpoint

## 🏃 Getting Started

### 1. Clone the repository

```bash
cd E:\source\repos\aiMate
```

### 2. Start LiteLLM (if not already running)

```bash
# Option 1: Docker
docker run -p 4000:4000 ghcr.io/berriai/litellm:main-latest

# Option 2: Local installation
pip install litellm[proxy]
litellm --port 4000
```

### 3. Configure LiteLLM endpoint

Edit `src/AiMate.Client/wwwroot/appsettings.json`:

```json
{
  "LiteLLM": {
    "BaseUrl": "http://localhost:4000",
    "ApiKey": ""  // Optional, if LiteLLM requires auth
  }
}
```

### 4. Build and run

```bash
cd src/AiMate.Client
dotnet run
```

Or open in Visual Studio and press F5.

The app will be available at `https://localhost:5001` (or the port shown in the console).

## 🎨 Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | Blazor WebAssembly (.NET 9) |
| UI Library | MudBlazor 7.20 |
| Language | C# 12 |
| Styling | MudBlazor + Custom CSS |
| Icons | Material Design Icons |
| State Management | Scoped Services + Events |
| HTTP Client | System.Net.Http |
| AI Backend | LiteLLM Proxy |

## 📊 Architecture

### State Management

The app uses a centralized `AppStateService` for state management:

```csharp
// Inject the service
@inject AppStateService AppState

// Subscribe to changes
AppState.OnChange += StateHasChanged;
AppState.OnConversationChanged += HandleConversationChange;

// Modify state
AppState.CreateNewConversation();
AppState.AddMessage(conversationId, message);
```

### Streaming Responses

The `LiteLLMService` supports streaming completions:

```csharp
await foreach (var chunk in LiteLLM.CreateChatCompletionStreamAsync(model, messages))
{
    if (chunk.Choices.Any())
    {
        var content = chunk.Choices[0].Delta?.Content;
        // Update UI in real-time
    }
}
```

## 🔧 Configuration

### LiteLLM Models

The app automatically fetches available models from LiteLLM on startup. If the API call fails, it falls back to default models:

- GPT-4
- GPT-3.5 Turbo
- Claude 3.5 Sonnet

You can customize models in `LiteLLMService.GetDefaultModels()`.

### Theme Customization

The theme is defined in `MainLayout.razor`:

```csharp
private MudTheme _theme = new MudTheme
{
    PaletteDark = new PaletteDark
    {
        Primary = "#8B5CF6",      // Purple
        Secondary = "#3B82F6",    // Blue
        // ...
    }
};
```

## 🐛 Debugging

Enable debug mode via the sidebar settings (coming soon) or manually:

```csharp
AppState.DebugMode = true;
AppState.AddDebugLog("Custom debug message");
```

## 📝 Development Roadmap

### Phase 1: Core Chat (✅ Complete)
- ✅ Basic UI layout
- ✅ Conversation management
- ✅ Message streaming
- ✅ Model selection

### Phase 2: Enhanced Features (🔜 In Progress)
- 🔜 Knowledge base
- 🔜 File attachments
- 🔜 Structured content
- 🔜 Settings panel

### Phase 3: Advanced Features
- 🔜 Tool integration (MCP)
- 🔜 Web search
- 🔜 Code interpreter
- 🔜 Custom prompts

### Phase 4: Production Ready
- 🔜 User authentication
- 🔜 Database persistence
- 🔜 Analytics & monitoring
- 🔜 Rate limiting
- 🔜 API usage tracking

## 🤝 Contributing

This is the reference implementation for the EchoMCP connector. Contributions are welcome!

## 📄 License

MIT License - see LICENSE file for details.

## 🙏 Acknowledgments

- Built as a Blazor alternative to Open WebUI
- Powered by [LiteLLM](https://docs.litellm.ai/)
- UI components by [MudBlazor](https://mudblazor.com/)
- Inspired by Claude.ai's interface design

---

**Built with ❤️ in Blazor and .NET 9**
