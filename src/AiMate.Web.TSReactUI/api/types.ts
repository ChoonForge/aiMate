// ============================================================================
// User & Authentication Types
// ============================================================================

export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  permissions: string[];
  createdAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  user: User;
  expiresAt: string;
}

export interface ApiKey {
  id: string;
  name: string;
  key: string;
  createdAt: string;
  expiresAt?: string;
  lastUsed?: string;
}

// ============================================================================
// Workspace & Conversation Types
// ============================================================================

export interface Workspace {
  id: string;
  name: string;
  description?: string;
  type: string;
  personality?: string;
  createdAt: string;
  updatedAt: string;
  userId: string;
}

export interface Conversation {
  id: string;
  workspaceId: string;
  title: string;
  model?: string;
  messageCount: number;
  lastMessageAt?: string;
  createdAt: string;
  updatedAt: string;
  isArchived?: boolean;
}

export interface Message {
  id: string;
  conversationId: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  metadata?: {
    model?: string;
    tokens?: number;
    finishReason?: string;
    [key: string]: any;
  };
  attachments?: Attachment[];
  createdAt: string;
}

// ============================================================================
// Chat Completion Types (OpenAI-compatible)
// ============================================================================

export interface ChatCompletionMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
}

export interface ChatCompletionRequest {
  model: string;
  messages: ChatCompletionMessage[];
  temperature?: number;
  maxTokens?: number;
  stream?: boolean;
  topP?: number;
  frequencyPenalty?: number;
  presencePenalty?: number;
  stop?: string | string[];
}

export interface ChatCompletionChoice {
  index: number;
  message: ChatCompletionMessage;
  finishReason: string;
}

export interface ChatCompletionUsage {
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
}

export interface ChatCompletionResponse {
  id: string;
  object: string;
  created: number;
  model: string;
  choices: ChatCompletionChoice[];
  usage: ChatCompletionUsage;
}

export interface StreamChunk {
  id: string;
  object: string;
  created: number;
  model: string;
  choices: Array<{
    index: number;
    delta: {
      role?: string;
      content?: string;
    };
    finishReason?: string;
  }>;
}

// ============================================================================
// Knowledge & Files
// ============================================================================

export interface KnowledgeItem {
  id: string;
  title: string;
  content: string;
  type: string;
  tags: string[];
  source?: string;
  metadata?: Record<string, any>;
  createdAt: string;
  updatedAt: string;
  userId: string;
}

export interface FileItem {
  id: string;
  name: string;
  size: number;
  mimeType: string;
  url?: string;
  path?: string;
  uploadedAt: string;
  userId: string;
}

export interface Attachment {
  id: string;
  type: 'file' | 'note' | 'knowledge' | 'webpage' | 'chat';
  referenceId: string;
  name?: string;
  metadata?: any;
  createdAt?: string;
}

// ============================================================================
// Notes
// ============================================================================

export interface Note {
  id: string;
  title: string;
  content: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  userId: string;
}

// ============================================================================
// Projects
// ============================================================================

export interface Project {
  id: string;
  name: string;
  description?: string;
  type?: string;
  createdAt: string;
  updatedAt: string;
  userId: string;
}

// ============================================================================
// Settings
// ============================================================================

export interface UserSettings {
  id?: string;
  userId?: string;
  theme: 'light' | 'dark' | 'system';
  colorTheme: 'purple' | 'blue' | 'green' | 'orange' | 'pink' | 'cyan';
  fontSize: 'small' | 'medium' | 'large';
  defaultModel?: string;
  defaultWorkspace?: string;
  language?: string;
  notifications?: boolean;
  [key: string]: any;
}

// ============================================================================
// API Connections
// ============================================================================

export interface Connection {
  id: string;
  name: string;
  type: 'openai' | 'anthropic' | 'azure' | 'custom';
  endpoint: string;
  apiKey?: string;
  enabled: boolean;
  isDefault?: boolean;
  metadata?: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface ConnectionTestResult {
  success: boolean;
  message: string;
  latency?: number;
  error?: string;
}

// ============================================================================
// Models
// ============================================================================

export interface Model {
  id: string;
  name: string;
  provider: string;
  description?: string;
  contextWindow?: number;
  maxTokens?: number;
  pricing?: {
    input: number;
    output: number;
  };
  capabilities?: string[];
  isAvailable: boolean;
}

// ============================================================================
// Plugins & Tools
// ============================================================================

export interface Plugin {
  id: string;
  name: string;
  description?: string;
  type: string;
  enabled: boolean;
  configuration?: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface Tool {
  id: string;
  name: string;
  description?: string;
  type: string;
  enabled: boolean;
  configuration?: Record<string, any>;
}

// ============================================================================
// Usage & Analytics
// ============================================================================

export interface UsageRecord {
  id: string;
  userId: string;
  model: string;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  cost?: number;
  timestamp: string;
}

export interface UsageSummary {
  totalTokens: number;
  totalCost: number;
  requestCount: number;
  period: string;
  breakdown?: {
    model: string;
    tokens: number;
    cost: number;
    requests: number;
  }[];
}

// ============================================================================
// Search
// ============================================================================

export interface SearchQuery {
  query: string;
  filters?: {
    type?: string[];
    dateFrom?: string;
    dateTo?: string;
    userId?: string;
  };
  limit?: number;
  offset?: number;
}

export interface SearchResult {
  id: string;
  type: 'conversation' | 'message' | 'note' | 'knowledge' | 'file';
  title: string;
  content: string;
  snippet?: string;
  score: number;
  metadata?: Record<string, any>;
  createdAt: string;
}

export interface SearchResponse {
  results: SearchResult[];
  total: number;
  query: string;
  executionTime: number;
}

// ============================================================================
// Feedback
// ============================================================================

export interface Feedback {
  id: string;
  messageId?: string;
  rating: number;
  comment?: string;
  type: string;
  userId: string;
  createdAt: string;
}

// ============================================================================
// Common Response Wrapper
// ============================================================================

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: any;
  };
  metadata?: {
    timestamp: string;
    requestId?: string;
    page?: number;
    pageSize?: number;
    total?: number;
  };
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

// ============================================================================
// Error Types
// ============================================================================

export interface ApiError {
  code: string;
  message: string;
  details?: any;
  statusCode?: number;
}

// ============================================================================
// Structured Content Types
// ============================================================================

export interface StructuredContent {
  type: 'table' | 'form' | 'keyvalue' | 'list' | 'chart';
  data: any;
  metadata?: Record<string, any>;
}
