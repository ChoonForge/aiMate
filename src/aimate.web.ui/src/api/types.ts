/**
 * aiMate REST API TypeScript Definitions
 * 
 * Complete type definitions for the aiMate REST API v1
 * Matches the backend DTOs exactly for type safety across the stack
 */

// ============================================================================
// COMMON TYPES
// ============================================================================

export interface ApiErrorResponse {
  error: {
    message: string;
    type: string;
    code: string;
    details?: Record<string, string[]>;
  };
}

export interface ApiSuccessResponse {
  success: boolean;
  message: string;
}

export interface PaginationParams {
  page?: number;
  perPage?: number;
  limit?: number;
}

// ============================================================================
// AUTHENTICATION & API KEYS
// ============================================================================

export interface LoginRequest {
  email?: string;
  password?: string;
  provider?: string;
  token?: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  refreshToken?: string;
  expiresIn?: number;
}

export interface User {
  id: string;
  username: string;
  email: string;
  userTier: 'Free' | 'BYOK' | 'Developer' | 'Enterprise';
  permissions?: string[];
}

export interface ApiKeyDto {
  id: string;
  userId: string;
  name: string;
  hashedKey: string;
  createdAt: string;
  lastUsedAt: string | null;
  isRevoked: boolean;
  requestsPerMinute: number;
  requestsPerDay: number;
}

export type ApiKey = ApiKeyDto;

export interface CreateApiKeyDto {
  userId: string;
  name: string;
  description?: string;
}

export interface ApiKeyResponseDto {
  id: string;
  apiKey: string; // Only returned once!
  createdAt: string;
}

// ============================================================================
// CHAT API
// ============================================================================

export interface ChatMessage {
  role: 'system' | 'user' | 'assistant';
  content: string;
}

export type Message = ChatMessage;

export interface MessageDto {
  id: string;
  conversationId: string;
  role: 'system' | 'user' | 'assistant';
  content: string;
  timestamp: string;
  model?: string;
  createdAt: string;
  updatedAt?: string;
  metadata?: {
    model?: string;
    tokenCount?: number;
    cost?: number;
    [key: string]: any;
  };
  attachments?: Array<{ id: string; name: string; type: string; url: string }>;
  structuredContent?: any;
  feedback?: FeedbackDto;
}

export interface SendMessageDto {
  conversationId?: string;
  workspaceId?: string;
  content: string;
  model?: string;
  parentMessageId?: string;
  attachmentIds?: string[];
  systemPrompt?: string;
  stream?: boolean;
}

export interface UpdateMessageDto {
  content?: string;
  isLiked?: boolean;
}

export interface ChatCompletionRequest {
  model: string;
  messages: ChatMessage[];
  temperature?: number;
  max_tokens?: number;
  top_p?: number;
  frequency_penalty?: number;
  presence_penalty?: number;
  stream?: boolean;
}

export interface ChatCompletionChoice {
  index: number;
  message: ChatMessage;
  finish_reason: 'stop' | 'length' | 'content_filter' | null;
}

export interface ChatCompletionUsage {
  prompt_tokens: number;
  completion_tokens: number;
  total_tokens: number;
}

export interface ChatCompletionResponse {
  id: string;
  object: 'chat.completion';
  created: number;
  model: string;
  choices: ChatCompletionChoice[];
  usage: ChatCompletionUsage;
}

export interface StreamChunkDelta {
  content?: string;
  role?: string;
}

export interface StreamChunkChoice {
  index: number;
  delta: StreamChunkDelta;
  finish_reason: 'stop' | 'length' | 'content_filter' | null;
}

export interface StreamChunk {
  id: string;
  object: 'chat.completion.chunk';
  created: number;
  model: string;
  choices: StreamChunkChoice[];
}

// ============================================================================
// WORKSPACES & CONVERSATIONS
// ============================================================================

export interface WorkspaceDto {
  id: string;
  name: string;
  description?: string;
  type: 'Project' | 'Research' | 'Support' | 'Personal' | 'Other';
  personality: string;
  createdAt: string;
  updatedAt: string;
  icon?: string;
  isDefault?: boolean;
  color?: string;
  conversationCount?: number;
}

export type Workspace = WorkspaceDto;

export interface CreateWorkspaceDto {
  name: string;
  description?: string;
  type: 'Project' | 'Research' | 'Support' | 'Personal' | 'Other';
  personality?: string;
  icon?: string;
  color?: string;
}

export interface UpdateWorkspaceDto {
  name?: string;
  description?: string;
  personality?: string;
  icon?: string;
  color?: string;
}

export interface ConversationDto {
  id: string;
  workspaceId: string;
  title: string;
  modelId: string | null;
  isPinned: boolean;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  messageCount: number;
  lastMessageAt?: string;
  tags?: string[];
}

export type Conversation = ConversationDto;

export interface CreateConversationDto {
  title?: string;
  workspaceId?: string;
  tags?: string[];
  modelId?: string;
}

export interface UpdateConversationDto {
  title?: string;
  modelId?: string | null;
  isPinned?: boolean;
  isArchived?: boolean;
  tags?: string[];
}

// ============================================================================
// FEEDBACK
// ============================================================================

export interface FeedbackTag {
  key: string;
  value: string;
  color: string;
  sentiment: 'positive' | 'negative' | 'neutral';
}

export interface FeedbackDto {
  id: string;
  messageId: string;
  userId: string;
  rating: number;
  textFeedback?: string;
  tags: FeedbackTag[];
  modelId: string;
  responseTimeMs: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFeedbackDto {
  userId: string;
  rating: number;
  textFeedback?: string;
  tags?: FeedbackTag[];
  modelId: string;
  responseTimeMs: number;
}

export interface UpdateFeedbackDto {
  rating?: number;
  textFeedback?: string;
}

export interface FeedbackStatsDto {
  modelId: string;
  totalFeedback: number;
  averageRating: number;
  ratingDistribution: Record<string, number>;
  averageResponseTime: number;
  positivePercentage: number;
  negativePercentage: number;
  neutralPercentage: number;
}

export interface FeedbackTemplateOptionDto {
  value: string;
  color: string;
  sentiment: 'positive' | 'negative' | 'neutral';
  icon: string;
  displayOrder: number;
}

export interface FeedbackTemplateDto {
  id: string;
  category: string;
  label: string;
  description: string;
  isActive: boolean;
  isRequired?: boolean;
  options: FeedbackTemplateOptionDto[];
  createdAt?: string;
}

export interface CreateFeedbackTemplateDto {
  category: string;
  label: string;
  description: string;
  options: FeedbackTemplateOptionDto[];
  isRequired?: boolean;
}

// ============================================================================
// KNOWLEDGE BASE
// ============================================================================

export interface KnowledgeArticleDto {
  id: string;
  title: string;
  content: string;
  type: 'Documentation' | 'Article' | 'Tutorial' | 'Guide' | 'Reference';
  category: string;
  tags: string[];
  visibility: 'Public' | 'Private' | 'Organization';
  isPublished: boolean;
  isFeatured: boolean;
  viewCount: number;
  referenceCount: number;
  createdAt: string;
  updatedAt: string;
  lastViewedAt?: string;
}

export interface CreateKnowledgeArticleDto {
  title: string;
  content: string;
  type: 'Documentation' | 'Article' | 'Tutorial' | 'Guide' | 'Reference';
  category: string;
  tags: string[];
  visibility: 'Public' | 'Private' | 'Organization';
  isPublished: boolean;
  isFeatured: boolean;
}

export interface KnowledgeAnalyticsDto {
  totalArticles: number;
  totalViews: number;
  totalReferences: number;
  mostViewed: Array<{ id: string; title: string; viewCount: number }>;
  mostReferenced: Array<{ id: string; title: string; referenceCount: number }>;
  tagCounts: Record<string, number>;
}

export interface SemanticSearchRequest {
  query: string;
  workspaceId?: string;
  limit?: number;
  threshold?: number;
  minScore?: number;
  filters?: Record<string, any>;
}

export interface SemanticSearchResult {
  id: string;
  title: string;
  content: string;
  score: number;
  relevance: 'high' | 'medium' | 'low';
}

export interface SemanticSearchResponse {
  results: SemanticSearchResult[];
  totalResults: number;
  queryTime: number;
}

export interface KnowledgeDocumentDto {
  id: string;
  workspaceId: string;
  title: string;
  fileName: string;
  fileType: string;
  fileSize: number;
  status: 'processing' | 'ready' | 'error';
  chunkCount: number;
  tags?: string[];
  createdAt: string;
  updatedAt: string;
}

export interface UploadDocumentDto {
  file: File;
  workspaceId: string;
  tags?: string[];
}

// ============================================================================
// NOTES
// ============================================================================

export interface NoteDto {
  id: string;
  title: string;
  content: string;
  contentType: 'Plain' | 'Markdown' | 'RichText';
  tags: string[];
  collection?: string;
  category?: string;
  color?: string;
  ownerId: string;
  visibility: 'Private' | 'Shared' | 'Public';
  isFavorite: boolean;
  isPinned: boolean;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNoteDto {
  title: string;
  content: string;
  contentType?: 'Plain' | 'Markdown' | 'RichText';
  tags?: string[];
  collection?: string;
  category?: string;
  color?: string;
}

export interface UpdateNoteDto {
  title?: string;
  content?: string;
  tags?: string[];
  collection?: string;
  category?: string;
  color?: string;
  isFavorite?: boolean;
  isPinned?: boolean;
  isArchived?: boolean;
}

// ============================================================================
// PROJECTS
// ============================================================================

export interface ProjectDto {
  id: string;
  workspaceId: string;
  key: string;
  name: string;
  description?: string;
  ownerId: string;
  owner: string;
  ownerEmail: string;
  status: 'Planning' | 'In Progress' | 'On Hold' | 'Completed' | 'Cancelled';
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  budget?: number;
  startDate?: string;
  dueDate?: string;
  progressPercent: number;
  tags: string[];
  teamMembers: string[];
  notes?: string;
  createdAt: string;
  updatedAt: string;
  conversationIds?: string[];
  documentIds?: string[];
  collaborators?: string[];
  icon?: string;
  color?: string;
}

export interface CreateProjectDto {
  key: string;
  name: string;
  description?: string;
  owner: string;
  ownerEmail: string;
  status: 'Planning' | 'In Progress' | 'On Hold' | 'Completed' | 'Cancelled';
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  budget?: number;
  startDate?: string;
  dueDate?: string;
  progressPercent?: number;
  tags?: string[];
  teamMembers?: string[];
  notes?: string;
  workspaceId?: string;
  icon?: string;
  color?: string;
}

export interface UpdateProjectDto {
  name?: string;
  description?: string;
  status?: 'Planning' | 'In Progress' | 'On Hold' | 'Completed' | 'Cancelled';
  priority?: 'Low' | 'Medium' | 'High' | 'Critical';
  budget?: number;
  startDate?: string;
  dueDate?: string;
  progressPercent?: number;
  tags?: string[];
  teamMembers?: string[];
  notes?: string;
  conversationIds?: string[];
  documentIds?: string[];
  collaborators?: string[];
}

// ============================================================================
// SETTINGS
// ============================================================================

export interface NotificationSettings {
  email: boolean;
  push: boolean;
  inApp: boolean;
}

export interface UserSettingsDto {
  username: string;
  email: string;
  userTier: 'Free' | 'BYOK' | 'Developer' | 'Enterprise';
  theme: 'light' | 'dark' | 'auto';
  notifications: NotificationSettings;
  language: string;
  timezone: string;
  defaultModel: string;
  defaultPersonality: string;
  autoSaveInterval: number;
  showLineNumbers: boolean;
  fontSize: number;
  fontFamily: string;
}

export interface UpdateSettingsDto {
  theme?: 'light' | 'dark' | 'auto';
  notifications?: NotificationSettings;
  language?: string;
  timezone?: string;
  defaultModel?: string;
  defaultPersonality?: string;
  autoSaveInterval?: number;
  showLineNumbers?: boolean;
  fontSize?: number;
  fontFamily?: string;
}

// ============================================================================
// USAGE ANALYTICS
// ============================================================================

export interface UsageByModel {
  model: string;
  connection: string;
  messages: number;
  tokens: number;
  cost: number;
  color: string;
}

export interface UsageStatsDto {
  totalMessages: number;
  totalTokens: number;
  totalCost: number;
  billingPeriodStart: string;
  billingPeriodEnd: string;
  usageByModel: UsageByModel[];
}

// ============================================================================
// ADMIN
// ============================================================================

export interface AdminDashboardDto {
  totalUsers: number;
  activeUsers: number;
  totalWorkspaces: number;
  totalConversations: number;
  totalMessages: number;
  systemHealth: 'Healthy' | 'Degraded' | 'Critical';
  uptime: number;
  lastBackup: string;
  totalTokensUsed: number;
  totalCostTracked: number;
}

export interface ModelDto {
  id: string;
  name: string;
  provider: string;
  isActive: boolean;
  isAvailable?: boolean;
  contextWindow: number;
  maxTokens?: number;
  costPer1kInputTokens?: number;
  costPer1kOutputTokens?: number;
  capabilities?: string[];
  usageCount?: number;
  lastUsed?: string;
  color?: string;
}

export type ModelOption = ModelDto;

export interface McpServerDto {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  status: 'Connected' | 'Disconnected' | 'Error';
  lastConnected?: string;
  tools: string[];
  version: string;
  enabled?: boolean;
}

export interface LogEntryDto {
  timestamp: string;
  level: 'DEBUG' | 'INFO' | 'WARN' | 'ERROR' | 'CRITICAL';
  source: string;
  message: string;
  details?: Record<string, any>;
}

// ============================================================================
// FILES & ATTACHMENTS
// ============================================================================

export interface FileDto {
  id: string;
  workspaceId: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  description?: string;
  url: string;
  uploadedAt: string;
  uploadedBy: string;
  updatedAt?: string;
  name?: string; // Alias for fileName
}

export interface FileUploadResponse {
  id: string;
  workspaceId: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  description?: string;
  url: string;
  uploadedAt: string;
  uploadedBy: string;
}

export interface AttachmentDto {
  id: string;
  fileName: string;
  url: string;
  uploadedAt: string;
}

export interface UpdateFileDto {
  description?: string;
}

// ============================================================================
// SEARCH
// ============================================================================

export interface SearchResultDto<T = any> {
  results: T[];
  totalResults: number;
}

export interface ConversationSearchResult {
  id: number;
  title: string;
  workspaceId: string;
  relevance: number;
}

export interface MessageSearchResult {
  id: string;
  content: string;
  conversationId: string;
  createdAt: string;
  relevance: number;
}

export interface KnowledgeSearchResult {
  id: string;
  title: string;
  content?: string;
  score?: number;
}

export interface GlobalSearchResultDto {
  conversations: ConversationSearchResult[];
  messages: MessageSearchResult[];
  knowledge: KnowledgeSearchResult[];
}

// ============================================================================
// TOOLS & MCP
// ============================================================================

export interface ToolParameter {
  type: 'string' | 'integer' | 'number' | 'boolean' | 'array' | 'object';
  description: string;
  required: boolean;
  default?: any;
}

export interface ToolDto {
  name: string;
  description: string;
  category: string;
  parameters: Record<string, ToolParameter>;
}

export interface ToolExecutionRequest {
  tool: string;
  parameters: Record<string, any>;
}

export interface ToolExecutionResponse {
  success: boolean;
  toolName: string;
  result: any;
  executionTime: number;
  timestamp: string;
}

// ============================================================================
// CODE MANAGEMENT
// ============================================================================

export interface CodeFileDto {
  id: string;
  fileName: string;
  language: string;
  path: string;
  content: string;
  size?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCodeFileDto {
  fileName: string;
  language: string;
  content: string;
  path?: string;
}

export interface CompileRequest {
  code: string;
  language: string;
}

export interface CompileMessage {
  line?: number;
  column?: number;
  message: string;
  severity: 'error' | 'warning' | 'info';
}

export interface CompileResponse {
  success: boolean;
  messages: CompileMessage[];
  warnings: CompileMessage[];
  errors: CompileMessage[];
  compiledCode?: string;
}

// ============================================================================
// PLUGINS
// ============================================================================

export interface PluginSettingSchema {
  type: 'select' | 'number' | 'string' | 'boolean';
  options?: string[];
  min?: number;
  max?: number;
  description: string;
}

export interface PluginDto {
  id: string;
  name: string;
  version: string;
  description: string;
  isEnabled: boolean;
  author: string;
  settings: Record<string, any>;
}

export interface PluginSettingsDto {
  schema: Record<string, PluginSettingSchema>;
  values: Record<string, any>;
}

export interface UpdatePluginSettingsDto {
  [key: string]: any;
}

// ============================================================================
// CONNECTIONS (BYOK)
// ============================================================================

export interface ConnectionDto {
  id: string;
  provider: 'OpenAI' | 'Anthropic' | 'Google' | 'Azure' | 'Custom';
  name: string;
  isActive: boolean;
  apiKeyPrefix: string;
  createdAt: string;
  lastUsed?: string;
  models: string[];
  updatedAt?: string;
  enabled?: boolean;
  url?: string;
}

export interface CreateConnectionDto {
  provider: 'OpenAI' | 'Anthropic' | 'Google' | 'Azure' | 'Custom';
  name: string;
  apiKey: string;
  isActive: boolean;
  url?: string;
}

export interface UpdateConnectionDto {
  name?: string;
  isActive?: boolean;
  apiKey?: string;
  url?: string;
}

export interface ConnectionTestResponse {
  success: boolean;
  message: string;
  provider: string;
  availableModels: string[];
}

// ============================================================================
// ORGANIZATIONS & GROUPS
// ============================================================================

export interface OrganizationDto {
  id: string;
  name: string;
  description?: string;
  memberCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrganizationDto {
  name: string;
  description?: string;
}

export interface GroupDto {
  id: string;
  organizationId: string;
  name: string;
  description?: string;
  memberCount: number;
  createdAt: string;
}

export interface CreateGroupDto {
  organizationId: string;
  name: string;
  description?: string;
}

// ============================================================================
// STRUCTURED CONTENT
// ============================================================================

export interface StructuredContentFieldSchema {
  name: string;
  type: 'text' | 'richtext' | 'number' | 'boolean' | 'array' | 'date';
  required: boolean;
  description?: string;
}

export interface StructuredContentTemplateDto {
  id: string;
  name: string;
  category: string;
  description: string;
  fields: StructuredContentFieldSchema[];
}

export interface ParseStructuredContentRequest {
  templateId: string;
  data: Record<string, any>;
}

export interface ParseStructuredContentResponse {
  success: boolean;
  parsed: Record<string, any>;
}

