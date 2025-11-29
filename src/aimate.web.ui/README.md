# aiMate.web.ui

A sovereign AI chat platform for New Zealand, built with React, TypeScript, and Tailwind CSS.

## Overview

aiMate.web.ui is a production-ready React frontend featuring:

- **Real-time streaming chat** with SSE (Server-Sent Events)
- **Multi-workspace organization** for conversations
- **Knowledge base integration** with RAG document support
- **Admin panel** for model and connection management
- **Offline mode** for local development
- **Crisis detection** and safety infrastructure for NZ users

## Tech Stack

- **Framework**: React 18 + TypeScript
- **Build Tool**: Vite 7.x
- **Styling**: Tailwind CSS v4
- **UI Components**: shadcn/ui (48 components)
- **HTTP Client**: Axios with JWT authentication
- **Icons**: Lucide React
- **State Management**: React Query + Context API

## Quick Start

### Prerequisites

- Node.js 18+
- npm or yarn

### Installation

```bash
# Navigate to the UI directory
cd src/aimate.web.ui

# Install dependencies
npm install

# Copy environment template
cp .env.example .env

# Start development server
npm run dev
```

### Environment Variables

Create a `.env` file with:

```bash
# API Configuration
VITE_API_BASE_URL=http://localhost:5000

# Feature Flags
VITE_AUTH_ENABLED=true
VITE_DEBUG_MODE=false
```

## Project Structure

```
src/
├── api/                    # API layer
│   ├── client.ts           # Axios instance with JWT & SSE
│   ├── types.ts            # 90+ TypeScript interfaces
│   └── services/           # 16 service files
├── components/             # React components (35 custom + 48 UI)
│   ├── ui/                 # shadcn/ui components
│   └── figma/              # Figma integration
├── context/                # React context providers
├── hooks/                  # 9 custom React hooks
├── styles/                 # Global CSS
└── utils/                  # Utilities & mock data
```

## Key Features

### Chat System
- Streaming responses with character-by-character rendering
- Edit/regenerate messages
- File attachments and knowledge context
- Multi-model support

### Workspace Management
- Multiple workspaces for organization
- Conversation archiving and pinning
- Search across all conversations

### Admin Panel
- Toggle AI models on/off
- Manage BYOK API connections
- MCP server configuration
- User management

### Offline Mode
- Full functionality without backend
- 50 mock conversations
- Simulated streaming responses

## Documentation

Detailed documentation is available in `/src/`:

- `COMPONENT_ARCHITECTURE_SPEC.md` - Complete technical specification
- `SYSTEM_GUIDE.md` - Architecture and API integration
- `API_INTEGRATION_STATUS.md` - Endpoint coverage status
- `MODALS_AND_DIALOGS_GUIDE.md` - UI component documentation
- `WIRED_UP_STATUS.md` - Integration checklist
- `CHANGELOG.md` - Version history

## Development

### Available Scripts

```bash
npm run dev      # Start development server
npm run build    # Build for production
npm run preview  # Preview production build
npm run lint     # Run ESLint
```

### Debug Mode

Enable debug logging in the browser console:

```javascript
localStorage.setItem('DEBUG', 'true');
localStorage.setItem('LOG_LEVEL', 'DEBUG');
```

## API Integration

The UI integrates with 93 API endpoints across 11 categories:

| Category | Endpoints | Status |
|----------|-----------|--------|
| Auth | 5 | Ready |
| Chat | 8 | Ready |
| Conversations | 12 | Ready |
| Workspaces | 8 | Ready |
| Knowledge | 10 | Ready |
| Projects | 10 | Ready |
| Files | 6 | Ready |
| Admin | 15 | Ready |
| Settings | 5 | Ready |
| Usage | 6 | Ready |
| Connections | 8 | Ready |

## Safety Features

aiMate includes NZ-specific safety infrastructure:

- Crisis detection with verified NZ resources
- Hard blocks on unsafe model responses
- Content moderation
- Privacy-focused design

## Contributing

1. Create a feature branch
2. Make changes with tests
3. Submit a pull request

## License

Proprietary - Firebird Solutions

---

**Version**: 1.0.0
**Last Updated**: November 2025
