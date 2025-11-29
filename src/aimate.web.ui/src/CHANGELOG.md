# Changelog

All notable changes to aiMate.web.ui will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - November 2025

### Added

#### Core Features
- Real-time streaming chat with SSE (Server-Sent Events)
- Multi-workspace organization for conversations
- Knowledge base integration with RAG document support
- Admin panel for model and connection management
- Offline mode for local development without backend
- Crisis detection and safety infrastructure for NZ users

#### Components (35 Custom)
- `AdminModal` - 11-tab admin interface (89KB)
- `SettingsModal` - 6-tab user settings panel (24KB)
- `KnowledgeModal` - Knowledge base management (44KB)
- `UsageDetailsDialog` - Analytics dashboard (39KB)
- `ModelEditDialog` - AI model configuration (36KB)
- `ProjectModal` - Project/workspace management (28KB)
- `ConversationSidebar` - Left sidebar navigation (26KB)
- `ChatInput` - Message input with attachments (40KB)
- `ChatMessage` - Message rendering with markdown
- `ChatHeader` - Header with model selector
- `EmptyState` - Empty conversation placeholder
- `StructuredPanel` - Renders tables, forms, key-value pairs
- `AttachedContext` - Context chips display
- `KnowledgeSuggestions` - Smart knowledge suggestions
- All edit dialogs (Connection, Model, MCP, Plugin)
- All modals (Files, Notes, Search, Share, Rating, Help, About, Archived)
- Debug components (DebugContext, DebugPanel)
- Mode indicators (Offline, Showcase)

#### API Service Layer (16 Services)
- `admin.service.ts` - Admin dashboard operations
- `auth.service.ts` - Authentication & JWT management
- `chat.service.ts` - Real-time chat with streaming
- `connections.service.ts` - BYOK connection management
- `conversations.service.ts` - Conversation CRUD
- `feedback.service.ts` - Message ratings & feedback
- `files.service.ts` - File upload/download with progress
- `knowledge.service.ts` - RAG documents & semantic search
- `messages.service.ts` - Message CRUD operations
- `projects.service.ts` - Project management
- `search.service.ts` - Full-text search
- `settings.service.ts` - User preferences
- `usage.service.ts` - Analytics & billing
- `workspace.service.ts` - Single workspace operations
- `workspaces.service.ts` - Workspace list management

#### React Hooks (9 Hooks)
- `useAdmin` - Admin panel data management
- `useChat` - Real-time chat with SSE streaming
- `useConversations` - Conversation management with pagination
- `useFiles` - File upload/download with progress
- `useKnowledge` - RAG documents & semantic search
- `useProjects` - Project management
- `useSettings` - User preferences
- `useUsage` - Analytics & billing
- `useWorkspaces` - Workspace organization

#### Context Providers (4 Contexts)
- `AppDataContext` - Centralized data management
- `AuthContext` - Authentication state
- `AdminSettingsContext` - Admin panel data
- `UserSettingsContext` - User preferences

#### UI Components (48 shadcn/ui)
- Full shadcn/ui component library integration
- Dark mode support throughout
- Responsive design (mobile, tablet, desktop)

### Technical
- TypeScript with 90+ interfaces for API DTOs
- Axios client with JWT authentication & retry logic
- React Query for caching and synchronization
- Tailwind CSS v4 with custom theming
- Vite 7.x build tool

---

## [0.3.0] - October 2025

### Added
- API Integration Phase
- Service layer architecture
- Authentication flow
- Streaming chat responses
- File upload/management
- Search functionality
- Analytics integration

---

## [0.2.0] - September 2025

### Added
- Admin System
- Model management
- Safety configuration
- System settings
- User management panel

---

## [0.1.0] - August 2025

### Added
- Initial Implementation
- Basic chat interface
- Sidebar navigation
- Workspace management
- TypeScript setup
- Tailwind CSS configuration

---

## Migration Notes

### From 0.x to 1.0.0
- Services moved from `/services/` to `/api/services/`
- Service files now use `.service.ts` suffix
- Hooks now use centralized `AppDataContext`
- All API calls now go through typed service layer

---

**Maintained By**: aiMate Development Team
