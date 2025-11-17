# aiMate v2 - The AI Workspace

**Your AI Mate. Free for Kiwis. Fair for Everyone.**

## 🚀 What is aiMate?

aiMate is not another chat app. It's a complete AI workspace built from the ground up to be:

- **Free & Open Source** - MIT license, fork it, brand it, own it
- **Truly Kiwi** - Fine-tuned personality that talks like a mate, not a corporate robot
- **Workspace-Focused** - Projects, knowledge graphs, persistent memory - not just Q&A
- **Privacy-First** - NZ-hosted, your data stays yours
- **No Gatekeeping** - Unlike OpenWebUI's "benefactors" model, everything is unlocked

## 🏗️ Architecture

Built with **proper architecture**, not React/Svelte spaghetti:

```
Blazor Server (C#) + MudBlazor
   ↓
Clean Architecture (Core, Infrastructure, Web)
   ↓
PostgreSQL + pgvector (semantic search)
   ↓
LiteLLM Gateway (multi-model support)
```

### Key Features

- **Workspaces** - Not conversations, complete work environments
- **Kiwi Personalities** - Multiple AI modes (Kiwi Mate, Te Reo Māori, Mental Health, etc.)
- **Knowledge Graphs** - Semantic memory that persists and connects
- **MCP Tools** - First-class tool integration
- **Plugin System** - C# plugins, not brittle JavaScript
- **Localization** - Built from day one (en-NZ, mi-NZ)
- **3-Tier Model** - Free, BYOK ($10), Developer ($30)

## 🛠️ Tech Stack

- **.NET 8** - Modern C#, proper type safety
- **Blazor Server** - Real-time SignalR streaming
- **MudBlazor** - Material Design components
- **Fluxor** - Redux state management (predictable, testable)
- **PostgreSQL** - Reliable, battle-tested database
- **pgvector** - Vector embeddings for semantic search
- **Entity Framework Core** - Clean data access
- **Serilog** - Structured logging from day one

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- PostgreSQL 16+ with pgvector extension
- (Optional) LiteLLM running on localhost:4000

### Run Locally

```bash
# Clone the repo
git clone https://github.com/yourusername/aiMate.git
cd aiMate/src-v2

# Update connection string in appsettings.json
# Then restore and run
dotnet restore
dotnet run --project AiMate.Web

# Open browser to https://localhost:5001
```

### Docker (Coming Soon)

```bash
docker-compose up -d
```

## 📁 Project Structure

```
src-v2/
├── AiMate.Web/                 # Blazor Server app
│   ├── Components/
│   │   ├── Layout/            # MainLayout, Sidebar, TopBar
│   │   ├── Pages/             # Routable pages
│   │   └── Shared/            # Shared components
│   ├── wwwroot/
│   │   ├── css/               # Custom styles
│   │   └── localization/      # i18n JSON files
│   └── Program.cs             # App entry point
│
├── AiMate.Core/                # Domain logic (business rules)
│   ├── Entities/              # Domain models
│   ├── Enums/                 # Enumerations
│   ├── Interfaces/            # Service contracts
│   └── Services/              # Business logic
│
├── AiMate.Infrastructure/      # External concerns
│   ├── Data/                  # EF Core DbContext
│   └── Services/              # API clients, file storage
│
└── AiMate.Shared/              # Shared DTOs and models
```

## 🌏 Localization

Built from the ground up for multiple languages:

- **en-NZ** - English (New Zealand)
- **mi-NZ** - Te Reo Māori
- Easily extensible to other locales

Localization files: `/wwwroot/localization/*.json`

## 🎨 UI Design

Based on the proven Figma Make design:

- Dark theme with purple accents (#8B5CF6)
- Clean, professional spacing
- Mobile-responsive
- Accessible (WCAG 2.1 AA compliant)

## 🔌 Plugin System

Write C# plugins, not fragile JavaScript:

```csharp
public class MyPlugin : IWorkspacePlugin
{
    public string Id => "my-plugin";
    public string Name => "My Plugin";

    public Task<ToolResult> ExecuteTool(string tool, Dictionary<string, object> args)
    {
        // Your logic here
    }
}
```

## 🗄️ Database Schema

PostgreSQL with pgvector for semantic search:

- **Users** - Authentication, preferences, API keys
- **Projects** - Organize workspaces
- **Workspaces** - Core unit of work
- **Conversations** - Chat threads within workspaces
- **Messages** - Individual messages with tool calls
- **KnowledgeItems** - Documents, notes, embeddings
- **WorkspaceFiles** - File attachments

## 🧠 Personality System

The **killer feature**:

- **Kiwi Mate** - Default, talks like a real Kiwi
- **Kiwi Professional** - Business appropriate
- **Kiwi Dev** - Technical tasks
- **Te Reo Māori** - Bilingual support
- **Mental Health** - Empathetic, resource-focused
- **Standard** - Generic AI (fallback)

Auto-detection based on context or user override.

## 📊 Roadmap

### ✅ Phase 1: Foundation (DONE)
- Clean architecture setup
- Database models
- UI shell matching Figma Make
- Localization infrastructure

### 🚧 Phase 2: Core Chat (In Progress)
- Message flow with streaming
- Fluxor state management
- Markdown rendering
- Message actions (edit, regenerate, rate)

### 📅 Phase 3: Workspaces (Next)
- Create/manage workspaces
- Project organization
- File uploads
- Workspace settings

### 📅 Phase 4: Knowledge System
- Auto-extract entities from conversations
- Semantic search with pgvector
- Knowledge graph visualization
- Context integration

### 📅 Phase 5: Personality & Tools
- Kiwi personality fine-tuning
- MCP integration
- Plugin system
- Tool marketplace

### 📅 Phase 6: Production Ready
- Performance optimization
- Security audit
- Mobile responsive
- PWA support
- Docker deployment

## 🤝 Contributing

This is **truly open source** - no gatekeeping:

1. Fork the repo
2. Create a feature branch
3. Make your changes
4. Submit a PR

All contributions welcome!

## 📜 License

**MIT License** - Free to use, modify, and distribute.

No "benefactors" tier, no bait-and-switch. Forever.

## 🌟 Why aiMate?

### vs OpenWebUI
- **Clean architecture** vs React/Svelte spaghetti
- **True open source** vs "benefactors" gatekeeping
- **Workspace-focused** vs chat-only
- **Proper type safety** (C#) vs JavaScript chaos

### vs ChatGPT/Claude
- **Actually free** (for Kiwis)
- **Talks like a mate** not a corporate robot
- **NZ-hosted** data sovereignty
- **Open source** - you own it

### vs Everyone Else
- **Built with empathy** - Mental health support mode
- **Culturally aware** - Te Reo Māori integration
- **Community-owned** - Your feedback shapes the roadmap

## 📞 Contact

Built by Kiwis, for the world.

- GitHub: [github.com/yourusername/aiMate](https://github.com/yourusername/aiMate)
- Email: your@email.com
- Website: aimate.co.nz (coming soon)

---

**aiMate** - By Kiwis, for Kiwis, with the world watching. 🇳🇿

*"Not a chat app. An AI workspace."*
