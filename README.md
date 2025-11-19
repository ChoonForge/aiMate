# aiMate

**Free, safe, and sovereign AI for all Kiwis**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

---

## 🇳🇿 What is aiMate?

aiMate is not just another chat interface. It's a complete **AI workspace platform** built from the ground up to be:

- **🆓 Truly Free & Open Source** - MIT license, no gatekeeping, no "benefactors" tier
- **🥝 Authentically Kiwi** - Talks like a mate, not a corporate robot
- **🏢 Workspace-Focused** - Projects, knowledge graphs, persistent memory - not just Q&A
- **🔒 Privacy-First** - Self-hostable, your data stays yours
- **🌏 Culturally Aware** - Te Reo Māori integration, mental health support

**Unlike OpenWebUI or ChatGPT**, aiMate is:
- Built with proper architecture (Clean Architecture + C#)
- Designed for workspaces and collaboration, not just conversations
- Actually open source with no bait-and-switch licensing
- Tuned for New Zealand culture and needs

---

## ✨ Features

### 🎯 Core Capabilities (Production Ready)

- **💬 Real-Time Chat** - SSE streaming, markdown with code highlighting, message actions
- **🧠 AI Personalities** - 6 modes including Kiwi Mate, Te Reo Māori, Mental Health Guardian
- **📁 Workspaces** - Complete work environments with personality, context, and history
- **📚 Knowledge Base** - Vector search with pgvector, semantic memory, tag organization
- **📤 File Uploads** - Drag-and-drop, 10MB limit, image/document support
- **🔐 Authentication** - JWT + BCrypt, API key management with rate limiting
- **🔌 MCP Tools** - Web search, file reading, knowledge search, Guardian dataset generator
- **📊 Feedback System** - Rate AI responses (1-10), customizable tags, analytics
- **🚀 REST API** - OpenAI-compatible endpoints with streaming support
- **🐳 Docker Deployment** - Flexible deployment options:
  - **Quick Start:** InMemory database (no persistence, instant setup)
  - **Production:** PostgreSQL + pgvector (full persistence)
  - **Pulls from GitHub:** No local build required for public repos
  - **Optional Services:** Nginx reverse proxy, Redis caching, LiteLLM gateway

### 🎨 UI/UX

- **MudBlazor** - Material Design components, dark theme with purple accents
- **Global Search** - Keyboard navigation (Cmd/Ctrl+K), fuzzy search
- **Admin Panels** - Settings, API keys, knowledge base, feedback tags, projects
- **Responsive** - Works on desktop, tablet, mobile
- **Accessibility** - WCAG 2.1 AA compliant

### 🛠️ Developer Experience

- **Clean Architecture** - Core/Infrastructure/Web separation
- **Fluxor State Management** - Predictable Redux pattern with time-travel debugging
- **Entity Framework Core** - Type-safe database access with migrations
- **Serilog** - Structured logging from day one
- **Plugin System** - C# plugins with full type safety
- **Localization** - Built-in i18n (en-NZ, mi-NZ)

---

## 🚀 Quick Start

### Prerequisites

**For Docker (Recommended):**
- [Docker](https://docs.docker.com/get-docker/) & Docker Compose
- That's it! Database is handled by Docker (InMemory by default, PostgreSQL optional)

**For Local Development:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/) with pgvector (optional - can use InMemory)

### Run with Docker (Recommended)

**Option 1: Quick Start with InMemory Database (No Persistence)**

Perfect for testing - data is stored in memory and lost on restart.

```bash
# Clone the repository
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Copy environment template
cp .env.example .env

# Edit .env with your API keys (LITELLM_MASTER_KEY, OPENAI_API_KEY, etc.)
nano .env

# Start all services (defaults to InMemory database)
docker-compose -f docker-compose.production.yml up -d

# Open browser
open http://localhost:5000
```

**Option 2: Production with PostgreSQL (Persistent Storage)**

For production use with full data persistence.

```bash
# Clone the repository
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Copy environment template
cp .env.example .env

# Edit .env - set DATABASE_PROVIDER=PostgreSQL and configure connection
nano .env

# Start all services including PostgreSQL
docker-compose -f docker-compose.production.yml --profile with-postgres up -d

# Open browser
open http://localhost:5000
```

**See [DOCKER_DEPLOYMENT.md](src/DOCKER_DEPLOYMENT.md) for complete deployment guide including private repo setup, SSL, and troubleshooting.**

### Run Locally (Development)

```bash
# Clone the repository
git clone https://github.com/ChoonForge/aiMate.git
cd aiMate/src

# Restore dependencies
dotnet restore

# Option 1: Use InMemory database (quick start)
# No additional setup needed - just run!
dotnet run --project AiMate.Web

# Option 2: Use PostgreSQL (for production-like environment)
# Update connection string in appsettings.json
# Point to your PostgreSQL instance
# Run migrations
dotnet ef database update --project AiMate.Infrastructure
# Start the application
dotnet run --project AiMate.Web

# Open browser
open https://localhost:5001
```

---

## 📁 Project Structure

```
aiMate/
├── src/                           # Main application
│   ├── AiMate.Web/               # Blazor Server UI
│   ├── AiMate.Core/              # Domain entities and business logic
│   ├── AiMate.Infrastructure/    # Data access and external services
│   ├── AiMate.Shared/            # Shared DTOs and models
│   ├── docker-compose.yml        # Development Docker setup
│   ├── docker-compose.production.yml  # Production Docker setup
│   ├── .env.example              # Environment configuration template
│   └── DOCKER_DEPLOYMENT.md      # Comprehensive deployment guide
│
├── docs/                          # Documentation
│   ├── aiMate-Master-Plan.md     # Product vision and strategy
│   ├── COMPLETE_FEATURE_SET.md   # Full feature list
│   ├── CODING_GUIDELINES.md      # Development standards
│   └── ...
│
├── LICENSE                        # MIT License
├── CODE_OF_CONDUCT.md            # Community guidelines
├── CONTRIBUTING.md                # How to contribute
└── README.md                      # You are here
```

---

## 🧠 AI Personality System

One of aiMate's **killer features** is the personality system. Choose from:

| Personality | Description | Use Case |
|-------------|-------------|----------|
| **Kiwi Mate** | Casual, friendly, uses NZ slang | Default for Kiwis |
| **Kiwi Professional** | Professional but still Kiwi | Business contexts |
| **Kiwi Dev** | Technical, concise, developer-focused | Coding tasks |
| **Te Reo Māori** | Bilingual support, cultural awareness | Learning/practicing Te Reo |
| **Mental Health Guardian** | Empathetic, resource-focused, safe | Mental health support |
| **Standard** | Generic AI assistant | Fallback/international users |

Personalities are **auto-detected** based on context or can be manually selected per workspace.

---

## 🗄️ Tech Stack

**Frontend:**
- [Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) - Real-time UI with SignalR
- [MudBlazor 8.0](https://mudblazor.com/) - Material Design components
- [Fluxor](https://github.com/mrpmorris/Fluxor) - Redux state management

**Backend:**
- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) - Latest LTS release
- [Entity Framework Core 10](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [PostgreSQL 16+](https://www.postgresql.org/) - Primary database
- [pgvector](https://github.com/pgvector/pgvector) - Vector embeddings for semantic search
- [Redis](https://redis.io/) - Caching and session storage
- [Serilog](https://serilog.net/) - Structured logging

**AI & Integration:**
- [LiteLLM](https://github.com/BerriAI/litellm) - Multi-model AI gateway
- OpenAI, Anthropic, Google AI - Model providers
- MCP (Model Context Protocol) - Tool integration

**Deployment:**
- [Docker](https://www.docker.com/) - Containerization
- [Nginx](https://nginx.org/) - Reverse proxy and load balancing
- [Let's Encrypt](https://letsencrypt.org/) - SSL certificates

---

## 📊 Roadmap

### ✅ Completed

**Phase 1: Foundation**
- Clean Architecture setup (Core/Infrastructure/Web)
- PostgreSQL + pgvector integration
- Blazor Server + MudBlazor UI
- Fluxor state management

**Phase 2: Core Features**
- Real-time chat with SSE streaming
- AI personality system (6 personalities)
- Knowledge graph with semantic search
- Workspace management
- File uploads

**Phase 3: Advanced Features**
- Authentication (JWT + BCrypt)
- Admin panels (Settings, Knowledge Base, Projects)
- Global search with keyboard shortcuts
- Message actions (copy, regenerate, rate)

**Phase 4: Integration**
- MCP tools (web search, file reading, knowledge search)
- Guardian dataset generator
- REST API (OpenAI-compatible)
- Plugin system foundation

**Phase 5: Production Ready**
- API key management with rate limiting
- Comprehensive feedback system
- Docker deployment stack
- Database migrations
- Security hardening

**Phase 6: Documentation & Community**
- Complete documentation
- Contribution guidelines
- GitHub templates
- Open source release

### 🚧 In Progress

- User onboarding flow
- Model leaderboard and comparison
- Dataset export for fine-tuning
- Enhanced plugin marketplace

### 📅 Planned

**Q1 2025:**
- Advanced workspace collaboration
- Real-time co-editing
- Workspace templates
- Enhanced Te Reo Māori support

**Q2 2025:**
- Mobile apps (iOS/Android)
- Plugin marketplace
- Advanced analytics dashboard
- Enterprise features (SSO, audit logs)

**Q3 2025:**
- Voice input/output
- Image generation integration
- Advanced RAG capabilities
- Community-driven roadmap

---

## 🤝 Contributing

We welcome contributions! This is **truly open source** - no gatekeeping.

**Ways to contribute:**
- 🐛 Report bugs
- 💡 Suggest features
- 📝 Improve documentation
- 🌍 Add translations (especially Te Reo Māori!)
- 🎨 Design UI/UX improvements
- 🧪 Write tests
- 💻 Submit pull requests

**See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.**

**Not sure where to start?** Look for issues labeled [`good first issue`](https://github.com/ChoonForge/aiMate/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22).

---

## 📜 License

**MIT License** - See [LICENSE](LICENSE) for details.

**What this means:**
- ✅ Free to use commercially
- ✅ Free to modify
- ✅ Free to distribute
- ✅ Free to sublicense
- ✅ No restrictions on private use
- ✅ No "benefactors" tier or bait-and-switch

**Forever free. No gotchas.**

---

## 🌟 Why aiMate?

### vs OpenWebUI
- ✅ **True open source** (no "benefactors" gatekeeping)
- ✅ **Clean architecture** (not React/Svelte spaghetti)
- ✅ **Workspace-focused** (not just chat)
- ✅ **Type safety** (C# instead of JavaScript chaos)

### vs ChatGPT/Claude
- ✅ **Actually free** (especially for Kiwis)
- ✅ **Self-hostable** (data sovereignty)
- ✅ **Customizable** (tune personalities, add tools)
- ✅ **Culturally aware** (Te Reo Māori, local context)

### vs Everyone Else
- ✅ **Built with empathy** (mental health support mode)
- ✅ **Community-owned** (your feedback shapes the roadmap)
- ✅ **Production-ready** (not a weekend hack)
- ✅ **Well-documented** (comprehensive guides)

---

## 📖 Documentation

- **[Docker Deployment Guide](src/DOCKER_DEPLOYMENT.md)** - Complete Docker deployment guide with InMemory/PostgreSQL options
- **[Coding Guidelines](docs/CODING_GUIDELINES.md)** - Development standards and architecture patterns
- **[Complete Feature Set](docs/COMPLETE_FEATURE_SET.md)** - Full feature documentation
- **[Master Plan](docs/aiMate-Master-Plan.md)** - Product vision and roadmap
- **[Contributing](CONTRIBUTING.md)** - How to contribute
- **[Code of Conduct](CODE_OF_CONDUCT.md)** - Community guidelines

---

## 🙏 Acknowledgments

Built by Kiwis, for the world. Special thanks to:

- The New Zealand open source community
- Contributors who've helped along the way
- Users who provide feedback and bug reports
- The teams behind .NET, Blazor, MudBlazor, LiteLLM, and all our dependencies

---

## 📞 Contact & Community

- **GitHub Issues:** [Report bugs or request features](https://github.com/ChoonForge/aiMate/issues)
- **GitHub Discussions:** [Ask questions, share ideas](https://github.com/ChoonForge/aiMate/discussions)
- **Email:** hello@aimate.co.nz
- **Website:** [aimate.co.nz](https://aimate.co.nz) *(coming soon)*

---

## 🎯 Our Mission

**Make AI accessible, affordable, and empowering for everyone - starting with New Zealand.**

We believe AI should:
- Be free and open, not locked behind paywalls
- Respect users' privacy and data sovereignty
- Reflect local culture and values
- Empower communities, not extract from them
- Be built transparently with community input

**aiMate is our answer.**

---

## ⭐ Star Us!

If you find aiMate useful, give us a star! It helps others discover the project.

**Built with ❤️ from New Zealand** 🇳🇿

*"Not a chat app. An AI workspace."*

---

**Kia ora! Welcome to aiMate.** 🥝
