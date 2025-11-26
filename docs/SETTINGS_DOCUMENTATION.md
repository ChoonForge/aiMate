# aiMate Settings Documentation

**Version:** 1.0
**Last Updated:** 2025-01-18
**Location:** Web UI Settings Modals and Context Providers

This document provides a comprehensive reference of all available settings in the aiMate web UI application, including their types, default values, and purposes.

---

## Table of Contents

1. [User Settings Modal](#user-settings-modal)
   - [General Settings](#general-settings)
   - [Interface Settings](#interface-settings)
   - [Connections Settings](#connections-settings)
   - [Personalisation Settings](#personalisation-settings)
   - [Account Settings](#account-settings)
   - [Usage Settings](#usage-settings)

2. [Admin Settings Modal](#admin-settings-modal)
   - [General Settings (Admin)](#general-settings-admin)
   - [Interface Settings (Admin)](#interface-settings-admin)
   - [Connections Settings (Admin)](#connections-settings-admin)
   - [Models Settings](#models-settings)
   - [Plugins Settings](#plugins-settings)
   - [MCP Connectors Settings](#mcp-connectors-settings)
   - [Documents Settings](#documents-settings)
   - [Web Search Settings](#web-search-settings)
   - [Audio Settings](#audio-settings)
   - [Images Settings](#images-settings)
   - [Code Execution Settings](#code-execution-settings)

3. [Storage & Persistence](#storage--persistence)
4. [TypeScript Interfaces](#typescript-interfaces)

---

## User Settings Modal

**File:** `/src/aimate.web.ui/src/components/SettingsModal.tsx`
**Context:** `UserSettingsContext`
**Storage Key:** `aimate-user-settings`

The User Settings Modal contains personalization options for individual users. Settings are organized into 6 tabs.

### General Settings

Controls basic user preferences and communication options.

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `language` | `string` | `'en-gb'` | `'en-gb'`, `'en-us'`, `'es'`, `'fr'`, `'de'` | User's preferred language |
| `notifications` | `string` | `'off'` | `'off'`, `'on'`, `'mentions'` | Notification preference level |
| `systemPrompt` | `string` | New Zealand cultural prompt (multiline) | Free-form text | System-level prompt instructions for the AI |

**Default System Prompt:**
```
[Culture]
Location: New Zealand/Aotearoa (Land of the Long White Cloud)
Population: Kiwis/New Zealanders
Style: Laid back and friendly, less formal
Currency: NZD
Language: New Zealand English (Colour vs Color, Homogenise vs Homogenize)
```

### Interface Settings

Controls how the chat interface displays information.

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `showTimestamps` | `boolean` | `true` | Display timestamps on chat messages |
| `syntaxHighlighting` | `boolean` | `true` | Enable syntax highlighting for code blocks |
| `markdownSupport` | `boolean` | `true` | Enable markdown formatting in messages |

**Note:** Theme and color settings are managed by `ThemeProvider`, not stored in UserSettings.

### Connections Settings

Stores API credentials for external services.

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `openaiApiKey` | `string` | `''` (empty) | OpenAI API key for direct API calls |
| `anthropicApiKey` | `string` | `''` (empty) | Anthropic API key for direct API calls |
| `ollamaBaseUrl` | `string` | `''` (empty) | Base URL for local Ollama instance |

**Security Note:** These are sensitive credentials. Should never be logged or exposed in logs.

### Personalisation Settings

Customize AI response behavior and context management.

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `creativityLevel` | `string` | `'balanced'` | `'precise'`, `'balanced'`, `'creative'` | Controls temperature parameter of responses |
| `responseStyle` | `string` | `'balanced'` | `'concise'`, `'balanced'`, `'detailed'` | Desired response length and detail level |
| `customInstructions` | `string` | `''` (empty) | Free-form text | Additional custom instructions for the AI |
| `rememberContext` | `boolean` | `true` | Enable/disable | Whether to maintain conversation context across chats |

### Account Settings

User profile and account management.

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `email` | `string` | `'rich@example.com'` | User's email address |
| `username` | `string` | `'rich'` | User's display name |
| `allowAnalytics` | `boolean` | `true` | Opt-in to usage analytics |
| `personalization` | `boolean` | `true` | Allow personalization based on user behavior |

**Account Actions:**
- Change Password (requires current, new, and confirmation)
- View Subscription info
- Clear all conversations
- Reset all settings to defaults
- Delete account (irreversible)

### Usage Settings

Displays usage statistics and analytics (read-only in modal).

| Metric | Type | Description |
|--------|------|-------------|
| `totalMessages` | `number` | Total messages sent in current period |
| `totalTokens` | `number` | Total tokens consumed |
| `totalCost` | `number` | Total cost in USD |
| `byModel` | `array` | Breakdown of usage per model |

**Available in:** UsageDetailsDialog with detailed charts and analytics

---

## Admin Settings Modal

**File:** `/src/aimate.web.ui/src/components/AdminModal.tsx`
**Context:** `AdminSettingsContext`
**Storage Key:** `aimate-admin-settings`
**Access Level:** Admin only

The Admin Settings Modal provides system-wide configuration. Settings are organized into 11 tabs.

### General Settings (Admin)

System-level feature toggles.

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `adminEnabled` | `boolean` | `true` | Enable/disable admin panel |
| `userRegistration` | `boolean` | `true` | Allow new users to register |
| `apiAccess` | `boolean` | `false` | Enable API access for users |

**Note:** Changing `adminEnabled` affects entire system functionality.

### Interface Settings (Admin)

AI task automation and UI customization.

#### Task Models

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `localTaskModel` | `string` | `'Current Model'` | Model used for local task generation |
| `externalTaskModel` | `string` | `'Current Model'` | Model used for external task generation |

#### Generation Features

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `titleGeneration` | `boolean` | `true` | Auto-generate conversation titles |
| `titleGenerationPrompt` | `string` | Default prompt text | Custom prompt for title generation |
| `followUpGeneration` | `boolean` | `false` | Generate follow-up suggestions |
| `tagsGeneration` | `boolean` | `false` | Auto-generate conversation tags |
| `retrievalQueryGeneration` | `boolean` | `false` | Generate retrieval queries |
| `webSearchQueryGeneration` | `boolean` | `false` | Generate web search queries |
| `queryGenerationPrompt` | `string` | Default prompt text | Custom prompt for query generation |
| `autocompleteGeneration` | `boolean` | `false` | Enable autocomplete suggestions |
| `imagePromptGenerationPrompt` | `string` | Default prompt text | Custom prompt for image generation |
| `toolsFunctionCallingPrompt` | `string` | Default prompt text | Custom prompt for tool invocation |

#### UI Customization

| Setting | Type | Structure | Description |
|---------|------|-----------|-------------|
| `banners` | `array` | `{ id, type, text, enabled }` | Custom banners to display in UI |
| `promptSuggestions` | `array` | `{ id, title, subtitle, prompt }` | Quick prompt suggestions |

**Banners Example:**
```json
[
  {
    "id": "1",
    "type": "info",
    "text": "System maintenance scheduled",
    "enabled": true
  }
]
```

**Prompt Suggestions Example:**
```json
[
  {
    "id": "1",
    "title": "Tell me a fun fact",
    "subtitle": "about the Roman Empire",
    "prompt": "Tell me a fun fact about the Roman Empire"
  }
]
```

### Connections Settings (Admin)

External API and service connections.

#### Connection Type

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Unique identifier |
| `name` | `string` | Yes | Display name (e.g., "OpenAI API") |
| `type` | `string` | No | Connection type (e.g., "OpenAI", "Anthropic") |
| `url` | `string` | Yes | API endpoint URL |
| `enabled` | `boolean` | Yes | Enable/disable this connection |
| `isGroup` | `boolean` | No | Is this a grouped connection |
| `hasLayer` | `boolean` | No | Connection has additional layers |
| `apiKey` | `string` | No | API authentication key |

**Default Connections:**
1. OpenAI API - `https://chat.firebird.co.nz/lmstudio/v1`
2. Anthropic API - `https://api.anthropic.com/v1`
3. Local Docker - `http://host.docker.internal:9099`
4. OpenRouter - `https://openrouter.ai/api/v1`

### Models Settings

AI model configurations.

#### Model Type

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Unique model identifier |
| `name` | `string` | Yes | Display name |
| `color` | `string` | Yes | UI color indicator (hex) |
| `description` | `string` | Yes | Model description |
| `connection` | `string` | Yes | Connected API (reference to Connection ID) |

**Advanced Model Properties** (in ModelEditDialog):
- Provider type
- Context window size
- Max tokens
- System prompt
- Temperature, top-p, top-k settings
- Tools/functions support
- Filters and capabilities
- Feature flags

### Plugins Settings

System plugins and extensions.

#### Plugin Type

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string` | Unique plugin identifier |
| `name` | `string` | Plugin display name |
| `description` | `string` | What the plugin does |
| `enabled` | `boolean` | Enable/disable plugin |
| `parameters` | `array` | Plugin configuration parameters |

#### Plugin Parameters

| Field | Type | Description |
|-------|------|-------------|
| `name` | `string` | Parameter name |
| `type` | `string` | Parameter type (string, boolean, number, etc.) |
| `value` | `string` | Current parameter value |
| `required` | `boolean` | Is parameter required |

**Built-in Plugins:**
- Web Search
- Code Interpreter
- File Management
- Weather Forecast
- EchoMCP

### MCP Connectors Settings

Model Context Protocol connector configurations.

#### MCPConnector Type

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Unique connector ID |
| `name` | `string` | Yes | Connector display name |
| `type` | `string` | Yes | Connector type (stdio, sse, http) |
| `url` | `string` | Yes | Connector URL/endpoint |
| `auth` | `string` | No | Authentication method |
| `authToken` | `string` | No | Authentication token |
| `mcpId` | `string` | No | MCP identifier |
| `description` | `string` | No | Connector description |
| `visibility` | `enum` | Yes | `'private'` \| `'public'` |
| `groups` | `array` | No | User groups with access |
| `enabled` | `boolean` | Yes | Enable/disable connector |

### Documents Settings

RAG (Retrieval-Augmented Generation) and document processing configuration.

#### Content Processing

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `contentExtractionEngine` | `string` | `'Default'` | `'Default'`, `'Tika'`, `'Custom'` | Engine for extracting text from documents |
| `pdfExtractImages` | `boolean` | `false` | Enable/disable | Extract images from PDF files |
| `bypassEmbedding` | `boolean` | `false` | Enable/disable | Skip embedding generation |

#### Text Chunking

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `textSplitter` | `string` | `'RecursiveCharacter'` | Algorithm for splitting text into chunks |
| `chunkSize` | `string` | `'1500'` | Number of characters per chunk |
| `chunkOverlap` | `string` | `'100'` | Overlap between chunks for context |

#### Embedding Configuration

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `embeddingModelEngine` | `string` | `'Default'` | `'Default'`, `'OpenAI'`, `'HuggingFace'` | Embedding service provider |
| `embeddingModel` | `string` | `'sentence-transformers/all-MiniLM-L6-v2'` | Model name | Model used for creating embeddings |
| `fullContextMode` | `boolean` | `false` | Enable/disable | Include full document context in results |
| `hybridSearch` | `boolean` | `false` | Enable/disable | Combine semantic and keyword search |

#### Reranking Configuration

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `rerankingEngine` | `string` | `'Default'` | `'Default'`, `'Cohere'`, `'Custom'` | Reranking service |
| `rerankingModel` | `string` | `''` (empty) | Model name | Model for reranking search results |
| `topKReranker` | `string` | `'10'` | Number | Top K results to rerank |
| `relevanceThreshold` | `string` | `'0.5'` | 0.0-1.0 | Minimum relevance score (0-1) |
| `bm25Weight` | `number[]` | `[0.5]` | Array of numbers | BM25 algorithm weights |

#### RAG Configuration

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `ragTemplate` | `string` | Default RAG prompt | RAG template with placeholders |

### Web Search Settings

External web search configuration.

#### Search Feature Control

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `webSearchEnabled` | `boolean` | `false` | Enable/disable web search feature |
| `searchEngine` | `string` | `'google'` | Selected search provider |
| `enableQuerySuggestions` | `boolean` | `false` | Auto-suggest search queries |
| `enableWebLoader` | `boolean` | `false` | Automatically load full web pages |
| `resultCount` | `string` | `'10'` | Number of results per search (1-20) |

#### Search Engine API Keys

Depending on selected `searchEngine`, one of these fields is required:

| Setting | Type | Default Value | Used By | Description |
|---------|------|---------------|---------|-------------|
| `searxngUrl` | `string` | `''` | SearXNG | Self-hosted SearXNG instance URL |
| `serpApiKey` | `string` | `''` | SerpAPI | SerpAPI authentication key |
| `googleSearchApiKey` | `string` | `''` | Google PSE | Google Custom Search API key |
| `googleSearchEngineId` | `string` | `''` | Google PSE | Google Custom Search Engine ID |
| `braveSearchApiKey` | `string` | `''` | Brave | Brave Search API key |
| `serperApiKey` | `string` | `''` | Serper | Serper API key |
| `serplyApiKey` | `string` | `''` | Serply | Serply API key |
| `tavityApiKey` | `string` | `''` | Tavity | Tavity API key |
| `searchapiApiKey` | `string` | `''` | SearchAPI | SearchAPI.io API key |

**Available Search Engines:**
- Google PSE (Google Programmable Search Engine)
- SearXNG (Open source)
- SerpAPI
- Brave Search
- Serper
- Serply
- DuckDuckGo (free, no key required)
- Tavity
- SearchAPI

### Audio Settings

Voice input and text-to-speech configuration.

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `voiceInputEnabled` | `boolean` | `false` | Enable/disable | Allow voice input for messages |
| `textToSpeechEnabled` | `boolean` | `false` | Enable/disable | Convert AI responses to speech |
| `voiceModel` | `string` | `'alloy'` | `'alloy'`, `'echo'`, `'nova'` | Voice model for text-to-speech |

### Images Settings

Image generation configuration.

| Setting | Type | Default Value | Options | Description |
|---------|------|---------------|---------|-------------|
| `imageGenerationEnabled` | `boolean` | `false` | Enable/disable | Allow AI to generate images |
| `imageModel` | `string` | `'dall-e-3'` | `'dall-e-3'`, `'dall-e-2'`, `'stable-diffusion'` | Image generation model |

### Code Execution Settings

Code execution capabilities.

| Setting | Type | Default Value | Description |
|---------|------|---------------|-------------|
| `codeExecutionEnabled` | `boolean` | `false` | Enable/disable code execution sandbox |

**Warning:** Code execution should only be enabled in trusted environments.

---

## Storage & Persistence

### User Settings Storage

- **Storage Type:** Browser localStorage
- **Storage Key:** `aimate-user-settings`
- **Format:** JSON
- **Persistence:** Automatic on every change
- **Loading:** On app initialization, merged with defaults to handle new fields

**Storage Example:**
```json
{
  "general": {
    "language": "en-gb",
    "notifications": "on",
    "systemPrompt": "..."
  },
  "interface": {
    "showTimestamps": true,
    "syntaxHighlighting": true,
    "markdownSupport": true
  },
  "connections": {
    "openaiApiKey": "",
    "anthropicApiKey": "",
    "ollamaBaseUrl": ""
  },
  "personalisation": {
    "creativityLevel": "balanced",
    "responseStyle": "balanced",
    "customInstructions": "",
    "rememberContext": true
  },
  "account": {
    "email": "user@example.com",
    "username": "username",
    "allowAnalytics": true,
    "personalization": true
  }
}
```

### Admin Settings Storage

- **Storage Type:** Browser localStorage
- **Storage Key:** `aimate-admin-settings`
- **Format:** JSON
- **Persistence:** Automatic on every change
- **Access:** Admin users only
- **Loading:** On app initialization, merged with defaults

**Note:** Admin settings should potentially be synced with a backend database in production.

---

## TypeScript Interfaces

### User Settings Types

```typescript
// General Settings
interface GeneralSettings {
  language: string;
  notifications: string;
  systemPrompt: string;
}

// Interface Settings
interface InterfaceSettings {
  showTimestamps: boolean;
  syntaxHighlighting: boolean;
  markdownSupport: boolean;
}

// Connection Settings
interface ConnectionSettings {
  openaiApiKey: string;
  anthropicApiKey: string;
  ollamaBaseUrl: string;
}

// Personalisation Settings
interface PersonalisationSettings {
  creativityLevel: string;
  responseStyle: string;
  customInstructions: string;
  rememberContext: boolean;
}

// Account Settings
interface AccountSettings {
  email: string;
  username: string;
  allowAnalytics: boolean;
  personalization: boolean;
}

// Combined User Settings
interface UserSettings {
  general: GeneralSettings;
  interface: InterfaceSettings;
  connections: ConnectionSettings;
  personalisation: PersonalisationSettings;
  account: AccountSettings;
}
```

### Admin Settings Types

```typescript
// General Settings (Admin)
interface GeneralSettings {
  adminEnabled: boolean;
  userRegistration: boolean;
  apiAccess: boolean;
}

// Interface Settings (Admin)
interface InterfaceSettings {
  localTaskModel: string;
  externalTaskModel: string;
  titleGeneration: boolean;
  titleGenerationPrompt: string;
  followUpGeneration: boolean;
  tagsGeneration: boolean;
  retrievalQueryGeneration: boolean;
  webSearchQueryGeneration: boolean;
  queryGenerationPrompt: string;
  autocompleteGeneration: boolean;
  imagePromptGenerationPrompt: string;
  toolsFunctionCallingPrompt: string;
  banners: Array<{ id: string; type: string; text: string; enabled: boolean }>;
  promptSuggestions: Array<{ id: string; title: string; subtitle: string; prompt: string }>;
}

// Connection Type
interface Connection {
  id: string;
  name: string;
  type?: string;
  url: string;
  enabled: boolean;
  isGroup?: boolean;
  hasLayer?: boolean;
  apiKey?: string;
}

// Model Type
interface Model {
  id: string;
  name: string;
  color: string;
  description: string;
  connection: string;
}

// Plugin Type
interface Plugin {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  parameters: Array<{ name: string; type: string; value: string; required: boolean }>;
}

// MCP Connector Type
interface MCPConnector {
  id: string;
  name: string;
  type: string;
  url: string;
  auth: string;
  authToken: string;
  mcpId: string;
  description: string;
  visibility: 'private' | 'public';
  groups: string[];
  enabled: boolean;
}

// Documents Settings
interface DocumentsSettings {
  contentExtractionEngine: string;
  pdfExtractImages: boolean;
  bypassEmbedding: boolean;
  textSplitter: string;
  chunkSize: string;
  chunkOverlap: string;
  embeddingModelEngine: string;
  embeddingModel: string;
  fullContextMode: boolean;
  hybridSearch: boolean;
  rerankingEngine: string;
  rerankingModel: string;
  topKReranker: string;
  relevanceThreshold: string;
  bm25Weight: number[];
  ragTemplate: string;
}

// Web Search Settings
interface WebSearchSettings {
  webSearchEnabled: boolean;
  searchEngine: string;
  enableQuerySuggestions: boolean;
  enableWebLoader: boolean;
  resultCount: string;
  searxngUrl: string;
  serpApiKey: string;
  googleSearchApiKey: string;
  googleSearchEngineId: string;
  braveSearchApiKey: string;
  serperApiKey: string;
  serplyApiKey: string;
  tavityApiKey: string;
  searchapiApiKey: string;
}

// Audio Settings
interface AudioSettings {
  voiceInputEnabled: boolean;
  textToSpeechEnabled: boolean;
  voiceModel: string;
}

// Images Settings
interface ImagesSettings {
  imageGenerationEnabled: boolean;
  imageModel: string;
}

// Code Execution Settings
interface CodeExecutionSettings {
  codeExecutionEnabled: boolean;
}

// Combined Admin Settings
interface AdminSettings {
  general: GeneralSettings;
  interface: InterfaceSettings;
  connections: Connection[];
  models: Model[];
  plugins: Plugin[];
  mcpConnectors: MCPConnector[];
  documents: DocumentsSettings;
  webSearch: WebSearchSettings;
  audio: AudioSettings;
  images: ImagesSettings;
  codeExecution: CodeExecutionSettings;
}
```

---

## Implementation Details

### Context Hooks

**User Settings Hook:**
```typescript
function useUserSettings() {
  const context = useContext(UserSettingsContext);
  if (context === undefined) {
    throw new Error('useUserSettings must be used within a UserSettingsProvider');
  }
  return context;
}

// Usage:
const { settings, updateGeneral, updateInterface, ... } = useUserSettings();
```

**Admin Settings Hook:**
```typescript
function useAdminSettings() {
  const context = useContext(AdminSettingsContext);
  if (context === undefined) {
    throw new Error('useAdminSettings must be used within an AdminSettingsProvider');
  }
  return context;
}

// Usage:
const { settings, updateGeneral, updateInterface, ... } = useAdminSettings();
```

### Update Functions

All context providers provide focused update functions:

**User Settings Updates:**
- `updateGeneral(updates: Partial<GeneralSettings>)`
- `updateInterface(updates: Partial<InterfaceSettings>)`
- `updateConnections(updates: Partial<ConnectionSettings>)`
- `updatePersonalisation(updates: Partial<PersonalisationSettings>)`
- `updateAccount(updates: Partial<AccountSettings>)`
- `resetSettings()`

**Admin Settings Updates:**
- `updateGeneral(updates: Partial<GeneralSettings>)`
- `updateInterface(updates: Partial<InterfaceSettings>)`
- `updateConnections(connections: Connection[])`
- `updateModels(models: Model[])`
- `updatePlugins(plugins: Plugin[])`
- `updateMCPConnectors(connectors: MCPConnector[])`
- `updateDocuments(updates: Partial<DocumentsSettings>)`
- `updateWebSearch(updates: Partial<WebSearchSettings>)`
- `updateAudio(updates: Partial<AudioSettings>)`
- `updateImages(updates: Partial<ImagesSettings>)`
- `updateCodeExecution(updates: Partial<CodeExecutionSettings>)`
- `resetSettings()`

---

## File Locations

| Component | File Path |
|-----------|-----------|
| User Settings Modal | `/src/aimate.web.ui/src/components/SettingsModal.tsx` |
| Admin Settings Modal | `/src/aimate.web.ui/src/components/AdminModal.tsx` |
| User Settings Context | `/src/aimate.web.ui/src/context/UserSettingsContext.tsx` |
| Admin Settings Context | `/src/aimate.web.ui/src/context/AdminSettingsContext.tsx` |
| Base Modal Component | `/src/aimate.web.ui/src/components/BaseModal.tsx` |
| Base Dialog Component | `/src/aimate.web.ui/src/components/BaseDialog.tsx` |
| Usage Details Dialog | `/src/aimate.web.ui/src/components/UsageDetailsDialog.tsx` |

---

## Best Practices

### Accessing Settings in Components

```typescript
import { useUserSettings } from '../context/UserSettingsContext';

function MyComponent() {
  const { settings, updateGeneral } = useUserSettings();

  const handleLanguageChange = (newLanguage: string) => {
    updateGeneral({ language: newLanguage });
  };

  return (
    <select onChange={(e) => handleLanguageChange(e.target.value)}>
      <option value="en-gb">English (GB)</option>
      <option value="en-us">English (US)</option>
    </select>
  );
}
```

### Handling Sensitive Data

- API keys should never be logged or exposed in error messages
- Consider adding a "View/Hide" toggle for sensitive fields
- Consider server-side encryption for API keys
- Implement regular key rotation reminders

### Adding New Settings

1. Update the interface in the Context file
2. Add to defaultSettings with appropriate default value
3. Add the UI component in the Modal
4. Add update function if needed
5. Test localStorage persistence

---

## Notes

- Theme and color settings are managed by `ThemeProvider`, not UserSettings
- Admin settings are currently stored in localStorage but should be synced with backend in production
- Settings are automatically saved on every change (except account operations which require explicit action)
- All settings are merged with defaults on load to handle new fields in future updates
- Usage data is read-only in the modal but can be managed via API endpoints

---

**Built with ❤️ from New Zealand** 🇳🇿

Last Updated: 2025-01-18
