# aiMate Development Guide

## Project Overview

aiMate is a privacy-first AI chat application for New Zealand. The web UI is built with React 18 + TypeScript + Vite, using shadcn/ui components and Tailwind CSS.

**Tagline**: "Your AI Mate. Free for Kiwis. Fair for Everyone."

### Why aiMate Exists

This is a **true open source (MIT) alternative** to OpenWebUI, which has moved to an enterprise license. We believe AI tooling should be:

- **Genuinely open** - MIT licensed, no enterprise license traps
- **Extensible** - MCP-first architecture, not bolted-on integrations
- **Flexible** - use personally, self-host, or build a PaaS on top
- **Community-owned** - contributions welcome, governance transparent

We're building the foundation others can build on, not a walled garden.

## Current State (Post-First Light)

We have achieved **first light** - the first successful live connection to an inference server with streaming responses working.

### What's Working
- **Core chat with SSE streaming** to live LM server
- **Direct LM server connection** (bypasses backend, works in offline mode)
- **Conversation management** - create, delete, archive, pin, search, export
- **Workspace switching** - multi-workspace support with filtering
- **Admin panel** - connections and models configuration
- **Settings modal** - user preferences and theme
- **Offline mode** - full mock data fallback with 50 sample conversations

### What's Stubbed/Placeholder
- Text-to-speech (hook ready, UI shows toast)
- Email/social sharing (link sharing works)
- MCP import/export (TODOs in code)

## Priority Focus: Chat is Core

The chat experience is the product. Everything else supports it.

### Critical Path (in order)
1. **Stable streaming** - robust reconnection, error handling
2. **Conversation context** - proper message history sent to LM
3. **System prompts** - personality/instruction configuration
4. **Model selection** - validated against available models

## Admin Panel Tabs

The admin panel (`AdminModal.tsx`) has 11 tabs. For MVP, we show only essential ones:

| Tab | Status | Notes |
|-----|--------|-------|
| General | **SHOW** | Debug settings, offline mode toggle |
| Interface | **SHOW** | UI customization |
| Users & Groups | HIDE | Not needed for single-user alpha |
| Connections | **SHOW** | Critical - LM server configuration |
| Models | **SHOW** | Critical - model enable/disable |
| Plugins | HIDE | Future feature |
| MCP | **SHOW** | First-class citizen - tool integration |
| Documents | HIDE | RAG - Phase 2 |
| Web Search | HIDE | Future feature |
| Code Execution | HIDE | Future feature |
| Images | HIDE | Future feature |

To hide tabs, filter the `tabs` array in `AdminModal.tsx` around line 58.

## Settings Modal Tabs

The settings modal (`SettingsModal.tsx`) has 7 tabs:

| Tab | Status | Notes |
|-----|--------|-------|
| General | **SHOW** | Language, notifications |
| Interface | **SHOW** | Theme, appearance |
| Connections | HIDE | BYOK - Phase 3 |
| Personalisation | **SHOW** | AI personality settings |
| Memories | **SHOW** | Persistent user facts/preferences |
| Account | **SHOW** | User profile |
| Usage | **SHOW** | Usage stats |

To hide tabs, filter the `tabs` array in `SettingsModal.tsx` around line 66.

## File Structure

```
src/aimate.web.ui/
├── src/
│   ├── api/
│   │   ├── client.ts          # Axios + SSE streaming
│   │   ├── types.ts           # 90+ TypeScript interfaces
│   │   └── services/          # 14 service files, 93 endpoints
│   ├── components/
│   │   ├── App.tsx            # Main app, hooks up everything
│   │   ├── ChatMessage.tsx    # Message rendering
│   │   ├── ChatInput.tsx      # Message input + attachments
│   │   ├── ChatHeader.tsx     # Model selection, new chat
│   │   ├── ConversationSidebar.tsx
│   │   ├── AdminModal.tsx     # Admin panel (11 tabs)
│   │   ├── SettingsModal.tsx  # User settings
│   │   ├── ConnectionHealthIndicator.tsx  # LM server status
│   │   └── ui/                # 48 shadcn/ui components
│   ├── context/
│   │   ├── AppDataContext.tsx      # Central hook provider
│   │   ├── AdminSettingsContext.tsx # Admin + localStorage
│   │   ├── AuthContext.tsx
│   │   └── UserSettingsContext.tsx
│   ├── hooks/
│   │   ├── useChat.ts         # 600+ lines - streaming, offline mock, RAG
│   │   ├── useConversations.ts
│   │   ├── useWorkspaces.ts
│   │   ├── useAdmin.ts
│   │   ├── useKnowledge.ts    # Document upload, semantic search
│   │   ├── useMemories.ts     # Persistent user memories
│   │   ├── useTools.ts        # MCP tool discovery & execution
│   │   ├── useProjects.ts
│   │   ├── useFiles.ts
│   │   ├── useSettings.ts
│   │   └── useUsage.ts
│   └── utils/
│       └── config.ts          # AppConfig, offline mode
```

## Key Code Locations

| Feature | File | Notes |
|---------|------|-------|
| Chat streaming | `hooks/useChat.ts` | SSE handling, abort, retry |
| LM server connection | `hooks/useChat.ts:sendMessage` | Checks for enabled LM connection first |
| RAG injection | `hooks/useChat.ts:sendMessage` | Fetches document chunks, injects to system context |
| Knowledge search | `components/KnowledgeSuggestions.tsx` | Debounced semantic search with fallback |
| Memory persistence | `hooks/useMemories.ts` | localStorage with auto-extraction from messages |
| Memories UI | `components/MemoriesPanel.tsx` | View/add/delete memories |
| MCP tool discovery | `hooks/useTools.ts` | Load tools from enabled MCP servers |
| MCP tool execution | `api/services/tools.service.ts` | Execute tools, validate params, mock responses |
| Tool call UI | `components/ToolCallCard.tsx` | Render tool calls with status, params, results |
| Admin connections | `context/AdminSettingsContext.tsx` | Persisted to localStorage |
| API client | `api/client.ts` | Axios instance, JWT, retry logic |
| Message rendering | `components/ChatMessage.tsx` | Markdown, code blocks, actions, tool calls |

## Chat Flow (Priority System)

```
1. Check AdminSettings for enabled LM Server connection
   └── YES → Direct to LM server at connection.baseUrl + /chat/completions

2. Check if offline mode enabled
   └── YES → Return mock streaming response

3. Use backend API
   └── POST /api/v1/chat/send with SSE streaming
```

## Development Stages

### Stage 1: Stabilize First Light ✓
- [x] Connection health indicator in UI (`ConnectionHealthIndicator.tsx`)
- [x] Reconnection logic with exponential backoff (3 retries)
- [x] Graceful error messages with toast notifications
- [x] Model validation with available models suggestion

### Stage 2: Core UX Polish ✓
- [x] System prompt configuration (wired from Settings → General)
- [x] Continue message feature (appends to last assistant message)
- [x] Handle mid-stream network drops (partial content saved)
- [x] Mobile responsive fixes (ChatHeader, model selector)

### Stage 3: BYOK & Multi-Provider ✓
- [x] Connection testing (ModelEditDialog tests /models endpoint)
- [x] Multiple provider support (dropdown with auto-configured URLs)
- [x] Usage tracking per connection (getUsageByConnection, local tracking)

### Stage 4: Knowledge & Memory ✓
- [x] Document upload end-to-end (ChatInput → App → useChat → knowledgeService)
- [x] Semantic search in chat context (KnowledgeSuggestions with debounced API)
- [x] Memory persistence (useMemories hook, MemoriesPanel in Settings)

### Stage 5: MCP & Tools ✓
- [x] MCP server integration (tools.service.ts with mock data fallback)
- [x] Tool execution in chat (useTools hook, ToolCallCard rendering)
- [x] Tool call parsing (XML and JSON formats from assistant messages)
- [x] Custom MCP server configuration (MCPEditDialog already existed)

### Stage 6: Production Readiness (Next)
- [ ] Error boundary for graceful failures
- [ ] Performance optimization (React.memo, virtualization)
- [ ] Accessibility improvements (ARIA labels, keyboard nav)
- [ ] E2E testing setup

## Commands

```bash
# Development
cd src/aimate.web.ui
npm run dev

# Build
npm run build

# Type check
npm run typecheck
```

## Notes

- The UI is **production-ready** for core chat functionality
- All 93 API endpoints are integrated in the service layer
- Offline mode provides full functionality for development/demos
- Theme defaults to dark mode, persisted in localStorage
