# Backend Integration Plan
**TSReactUI → AiMate.Web Backend Integration**

Version: 1.0
Created: 2025-11-25
Purpose: Comprehensive plan to connect the React frontend to the existing .NET backend

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Integration Architecture](#integration-architecture)
4. [Phase 1: API Client Setup](#phase-1-api-client-setup)
5. [Phase 2: Authentication & Authorization](#phase-2-authentication--authorization)
6. [Phase 3: Core API Integration](#phase-3-core-api-integration)
7. [Phase 4: Real-time Features](#phase-4-real-time-features)
8. [Phase 5: File Upload & Management](#phase-5-file-upload--management)
9. [Phase 6: Advanced Features](#phase-6-advanced-features)
10. [Testing Strategy](#testing-strategy)
11. [Deployment Considerations](#deployment-considerations)

---

## 1. Executive Summary {#executive-summary}

### Current Situation
- **Backend**: Solid .NET 9.0 backend with 22+ API controllers, JWT authentication, comprehensive business logic
- **Frontend**: React TypeScript UI with shadcn/ui components, currently using mock API stubs
- **Goal**: Replace mock stubs with real backend integration while maintaining the existing UI/UX

### Key Challenges
1. Frontend expects different API structure than backend provides
2. Authentication flow needs to be implemented
3. Real-time chat streaming requires WebSocket or SSE implementation
4. File uploads need multipart/form-data handling
5. Type definitions need to match backend DTOs

### Success Criteria
- All 45+ React components connected to real backend APIs
- Authentication working with JWT tokens
- Real-time chat streaming functional
- File uploads working correctly
- Type-safe API calls throughout the application

---

## 2. Current State Analysis {#current-state-analysis}

### Backend API Structure

The backend provides well-structured RESTful APIs:

**API Controllers (22 total)**:
- `ChatApiController` - Chat completions (OpenAI-compatible)
- `WorkspaceApiController` - Workspace/conversation management
- `KnowledgeApiController` - Knowledge base operations
- `FileApiController` - File upload/management
- `SettingsApiController` - User settings
- `PluginApiController` - Plugin management
- `ConnectionApiController` - API connection management
- `AdminApiController` - Admin operations
- `ProjectsApiController` - Project management
- `NotesApiController` - Notes management
- `SearchApiController` - Search functionality
- `UsageApiController` - Usage tracking
- `FeedbackApiController` - User feedback
- `ToolsApiController` - Tool integration
- `AttachmentsApiController` - File attachments
- `StructuredContentApiController` - Structured data rendering
- `GroupApiController` - User groups
- `OrganizationApiController` - Organization management
- `CodeFilesApiController` - Code file management
- `CodeCompilationApiController` - Code execution
- `MonacoConfigApiController` - Monaco editor config
- `FeedbackSystemApiController` - Feedback system

**Authentication**:
- JWT Bearer token authentication
- Custom permission-based authorization policies
- API key support for developer tier

**CORS Configuration**:
- `ApiCorsPolicy` - Allows any origin (configured in Program.cs:287)
- Ready for frontend integration

### Frontend Expectations

**API Stubs Define These Endpoints**:
```typescript
/api/v1/GetChats?UserID={userId}
/api/v1/GetNotes?UserID={userId}
/api/v1/GetKnowledge?UserID={userId}
/api/v1/GetFiles?UserID={userId}
/api/v1/AttachFile
/api/v1/AttachNote
/api/v1/AttachKnowledge
/api/v1/ReferenceChat
/api/v1/AttachWebpage
/api/v1/TestConnection
```

**Backend Actually Provides**:
```
/api/v1/workspaces (GET, POST, PUT, DELETE)
/api/v1/workspaces/{id}/conversations
/api/v1/knowledge (GET, POST, PUT, DELETE)
/api/v1/files (GET, POST, DELETE)
/api/v1/settings
/api/v1/connections
/api/v1/chat/completions
/api/v1/chat/completions/stream
... and many more
```

### Gap Analysis

| Frontend Expects | Backend Provides | Action Required |
|-----------------|------------------|-----------------|
| `/api/v1/GetChats` | `/api/v1/workspaces` | Create adapter or update frontend |
| `/api/v1/GetNotes` | `/api/v1/notes` (likely) | Map endpoints |
| `/api/v1/AttachFile` | `/api/v1/attachments` | Map endpoints |
| Mock authentication | JWT authentication | Implement auth flow |
| Synchronous responses | Streaming for chat | Implement SSE/WebSocket |
| Simple types | Complex DTOs | Generate TypeScript types |

---

## 3. Integration Architecture {#integration-architecture}

### Recommended Approach: API Client Layer

Create a dedicated API client layer that:
1. Handles authentication token management
2. Provides type-safe API calls
3. Maps between frontend expectations and backend reality
4. Handles errors consistently
5. Supports both REST and streaming APIs

### Directory Structure
```
src/AiMate.Web.TSReactUI/
├── api/                          # NEW: API client layer
│   ├── client.ts                 # Base API client with axios/fetch
│   ├── auth.ts                   # Authentication API
│   ├── workspaces.ts             # Workspace/chat API
│   ├── knowledge.ts              # Knowledge base API
│   ├── files.ts                  # File management API
│   ├── settings.ts               # Settings API
│   ├── connections.ts            # Connection management
│   ├── chat.ts                   # Chat streaming API
│   └── types.ts                  # TypeScript type definitions
├── hooks/                        # NEW: React hooks for API
│   ├── useAuth.ts                # Authentication hook
│   ├── useWorkspaces.ts          # Workspaces data hook
│   ├── useChat.ts                # Chat streaming hook
│   └── useQuery.ts               # Generic query hook
├── context/                      # NEW: Global state
│   ├── AuthContext.tsx           # Auth state management
│   └── ApiContext.tsx            # API client context
├── utils/
│   ├── api-stubs.ts              # DEPRECATED: To be removed
│   └── storage.ts                # NEW: Token storage
└── components/                   # Existing components
```

---

## 4. Phase 1: API Client Setup {#phase-1-api-client-setup}

### 4.1 Install Dependencies

```bash
cd src/AiMate.Web.TSReactUI
npm install axios
npm install @tanstack/react-query
npm install --save-dev @types/node
```

**Why these libraries?**
- **axios**: Robust HTTP client with interceptors for auth
- **@tanstack/react-query**: Handles caching, loading states, and refetching
- **@types/node**: For environment variable types

### 4.2 Create Base API Client

**File: `api/client.ts`**

```typescript
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: `${API_BASE_URL}/api/v1`,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor - add auth token
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('auth_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - handle errors
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid
          localStorage.removeItem('auth_token');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }

  // For Server-Sent Events (chat streaming)
  createEventSource(url: string, token: string): EventSource {
    const fullUrl = `${API_BASE_URL}/api/v1/${url}`;
    // Note: EventSource doesn't support custom headers, so pass token as query param
    return new EventSource(`${fullUrl}?access_token=${token}`);
  }

  // For file uploads
  async uploadFile<T>(url: string, file: File, onProgress?: (progress: number) => void): Promise<T> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await this.client.post<T>(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          onProgress(percentCompleted);
        }
      },
    });

    return response.data;
  }
}

export const apiClient = new ApiClient();
export default apiClient;
```

### 4.3 Environment Configuration

**File: `.env.development`** (create this file)
```bash
VITE_API_BASE_URL=http://localhost:5000
VITE_AUTH_ENABLED=true
```

**File: `.env.production`** (create this file)
```bash
VITE_API_BASE_URL=https://your-production-domain.com
VITE_AUTH_ENABLED=true
```

**Update `vite.config.ts`** to ensure env vars are loaded:
```typescript
// Already configured, but verify the proxy settings:
export default defineConfig({
  // ... existing config
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
```

---

## 5. Phase 2: Authentication & Authorization {#phase-2-authentication--authorization}

### 5.1 Generate TypeScript Types from Backend

**Option A: Use NSwag or similar tools**
```bash
# Install NSwag
dotnet tool install -g NSwag.MSBuild

# Generate TypeScript client
cd src/AiMate.Web
nswag run nswag.json
```

**Option B: Manual type definitions**

**File: `api/types.ts`**

```typescript
// User & Authentication
export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: string;
}

export interface ApiKey {
  id: string;
  name: string;
  key: string;
  createdAt: string;
  expiresAt?: string;
}

// Workspaces & Conversations
export interface Workspace {
  id: string;
  name: string;
  description?: string;
  type: string;
  personality: string;
  createdAt: string;
  updatedAt: string;
}

export interface Conversation {
  id: string;
  workspaceId: string;
  title: string;
  model: string;
  messageCount: number;
  lastMessageAt: string;
  createdAt: string;
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
  };
  attachments?: Attachment[];
  createdAt: string;
}

// Chat Completion
export interface ChatCompletionRequest {
  model: string;
  messages: Array<{
    role: string;
    content: string;
  }>;
  temperature?: number;
  maxTokens?: number;
  stream?: boolean;
}

export interface ChatCompletionResponse {
  id: string;
  model: string;
  choices: Array<{
    message: {
      role: string;
      content: string;
    };
    finishReason: string;
  }>;
  usage: {
    promptTokens: number;
    completionTokens: number;
    totalTokens: number;
  };
}

// Knowledge & Files
export interface KnowledgeItem {
  id: string;
  title: string;
  content: string;
  type: string;
  tags: string[];
  source?: string;
  createdAt: string;
  updatedAt: string;
}

export interface FileItem {
  id: string;
  name: string;
  size: number;
  mimeType: string;
  url: string;
  uploadedAt: string;
}

export interface Attachment {
  id: string;
  type: 'file' | 'note' | 'knowledge' | 'webpage' | 'chat';
  referenceId: string;
  metadata?: any;
}

// Settings
export interface UserSettings {
  theme: 'light' | 'dark' | 'system';
  colorTheme: 'purple' | 'blue' | 'green' | 'orange' | 'pink' | 'cyan';
  fontSize: 'small' | 'medium' | 'large';
  defaultModel: string;
  defaultWorkspace: string;
}

// API Connections
export interface Connection {
  id: string;
  name: string;
  type: 'openai' | 'anthropic' | 'custom';
  endpoint: string;
  apiKey: string;
  enabled: boolean;
  createdAt: string;
}

// Common response wrapper
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
    requestId: string;
  };
}
```

### 5.2 Authentication API

**File: `api/auth.ts`**

```typescript
import apiClient from './client';
import { LoginRequest, LoginResponse, User, ApiKey } from './types';

export const authApi = {
  // Login with email/password
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    // Store token
    localStorage.setItem('auth_token', response.token);
    localStorage.setItem('refresh_token', response.refreshToken);
    return response;
  },

  // Logout
  async logout(): Promise<void> {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    // Optionally call backend to invalidate token
    await apiClient.post('/auth/logout');
  },

  // Get current user
  async getCurrentUser(): Promise<User> {
    return apiClient.get<User>('/auth/me');
  },

  // Refresh token
  async refreshToken(): Promise<LoginResponse> {
    const refreshToken = localStorage.getItem('refresh_token');
    const response = await apiClient.post<LoginResponse>('/auth/refresh', {
      refreshToken,
    });
    localStorage.setItem('auth_token', response.token);
    return response;
  },

  // API Key management
  async getApiKeys(): Promise<ApiKey[]> {
    return apiClient.get<ApiKey[]>('/auth/api-keys');
  },

  async createApiKey(name: string): Promise<ApiKey> {
    return apiClient.post<ApiKey>('/auth/api-keys', { name });
  },

  async deleteApiKey(keyId: string): Promise<void> {
    return apiClient.delete(`/auth/api-keys/${keyId}`);
  },
};
```

### 5.3 Authentication Context

**File: `context/AuthContext.tsx`**

```typescript
import React, { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../api/auth';
import { User } from '../api/types';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check if user is already logged in
    const token = localStorage.getItem('auth_token');
    if (token) {
      refreshUser();
    } else {
      setIsLoading(false);
    }
  }, []);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await authApi.login({ email, password });
      setUser(response.user);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    setIsLoading(true);
    try {
      await authApi.logout();
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  const refreshUser = async () => {
    setIsLoading(true);
    try {
      const userData = await authApi.getCurrentUser();
      setUser(userData);
    } catch (error) {
      // Token invalid or expired
      setUser(null);
      localStorage.removeItem('auth_token');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
```

---

## 6. Phase 3: Core API Integration {#phase-3-core-api-integration}

### 6.1 Workspace/Chat API

**File: `api/workspaces.ts`**

```typescript
import apiClient from './client';
import { Workspace, Conversation, Message } from './types';

export const workspacesApi = {
  // Get all workspaces for user
  async getWorkspaces(userId: string): Promise<Workspace[]> {
    return apiClient.get<Workspace[]>(`/workspaces?userId=${userId}`);
  },

  // Get single workspace
  async getWorkspace(workspaceId: string): Promise<Workspace> {
    return apiClient.get<Workspace>(`/workspaces/${workspaceId}`);
  },

  // Create workspace
  async createWorkspace(data: Partial<Workspace>): Promise<Workspace> {
    return apiClient.post<Workspace>('/workspaces', data);
  },

  // Update workspace
  async updateWorkspace(workspaceId: string, data: Partial<Workspace>): Promise<Workspace> {
    return apiClient.put<Workspace>(`/workspaces/${workspaceId}`, data);
  },

  // Delete workspace
  async deleteWorkspace(workspaceId: string): Promise<void> {
    return apiClient.delete(`/workspaces/${workspaceId}`);
  },

  // Get conversations in workspace
  async getConversations(workspaceId: string): Promise<Conversation[]> {
    return apiClient.get<Conversation[]>(`/workspaces/${workspaceId}/conversations`);
  },

  // Create conversation
  async createConversation(workspaceId: string, title: string): Promise<Conversation> {
    return apiClient.post<Conversation>(`/workspaces/${workspaceId}/conversations`, {
      title,
    });
  },

  // Get messages in conversation
  async getMessages(conversationId: string): Promise<Message[]> {
    return apiClient.get<Message[]>(`/conversations/${conversationId}/messages`);
  },

  // Send message (non-streaming)
  async sendMessage(conversationId: string, content: string): Promise<Message> {
    return apiClient.post<Message>(`/conversations/${conversationId}/messages`, {
      content,
    });
  },
};
```

### 6.2 React Query Hooks

**File: `hooks/useWorkspaces.ts`**

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workspacesApi } from '../api/workspaces';
import { useAuth } from '../context/AuthContext';

export function useWorkspaces() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  // Get all workspaces
  const { data: workspaces, isLoading, error } = useQuery({
    queryKey: ['workspaces', user?.id],
    queryFn: () => workspacesApi.getWorkspaces(user!.id),
    enabled: !!user,
  });

  // Create workspace mutation
  const createWorkspace = useMutation({
    mutationFn: workspacesApi.createWorkspace,
    onSuccess: () => {
      // Invalidate and refetch workspaces
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
    },
  });

  // Delete workspace mutation
  const deleteWorkspace = useMutation({
    mutationFn: workspacesApi.deleteWorkspace,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
    },
  });

  return {
    workspaces,
    isLoading,
    error,
    createWorkspace: createWorkspace.mutate,
    deleteWorkspace: deleteWorkspace.mutate,
  };
}

export function useConversations(workspaceId: string | null) {
  const queryClient = useQueryClient();

  const { data: conversations, isLoading } = useQuery({
    queryKey: ['conversations', workspaceId],
    queryFn: () => workspacesApi.getConversations(workspaceId!),
    enabled: !!workspaceId,
  });

  const createConversation = useMutation({
    mutationFn: ({ title }: { title: string }) =>
      workspacesApi.createConversation(workspaceId!, title),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', workspaceId] });
    },
  });

  return {
    conversations,
    isLoading,
    createConversation: createConversation.mutate,
  };
}
```

### 6.3 Update App.tsx with Providers

**File: `App.tsx`** (update)

```typescript
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './components/ThemeProvider';
import { DebugProvider } from './components/DebugContext';
// ... other imports

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ThemeProvider defaultTheme="system" storageKey="aiMate-theme">
          <DebugProvider>
            {/* Your existing app content */}
          </DebugProvider>
        </ThemeProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
```

---

## 7. Phase 4: Real-time Chat Streaming {#phase-4-real-time-features}

### 7.1 Chat Streaming API

**File: `api/chat.ts`**

```typescript
import { ChatCompletionRequest, ChatCompletionResponse } from './types';

export interface StreamChunk {
  id: string;
  delta: {
    content: string;
  };
  finishReason?: string;
}

export class ChatStreamClient {
  private baseUrl: string;
  private token: string;

  constructor(baseUrl: string = 'http://localhost:5000') {
    this.baseUrl = baseUrl;
    this.token = localStorage.getItem('auth_token') || '';
  }

  async *streamCompletion(
    request: ChatCompletionRequest
  ): AsyncGenerator<StreamChunk, void, unknown> {
    const response = await fetch(`${this.baseUrl}/api/v1/chat/completions/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.token}`,
      },
      body: JSON.stringify({ ...request, stream: true }),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const reader = response.body?.getReader();
    if (!reader) {
      throw new Error('No response body');
    }

    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();

      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');

      // Keep the last incomplete line in the buffer
      buffer = lines.pop() || '';

      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = line.slice(6);

          if (data === '[DONE]') {
            return;
          }

          try {
            const chunk = JSON.parse(data);
            yield chunk as StreamChunk;
          } catch (error) {
            console.error('Failed to parse chunk:', error);
          }
        }
      }
    }
  }

  // Non-streaming completion
  async createCompletion(request: ChatCompletionRequest): Promise<ChatCompletionResponse> {
    const response = await fetch(`${this.baseUrl}/api/v1/chat/completions`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.token}`,
      },
      body: JSON.stringify({ ...request, stream: false }),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }
}

export const chatStreamClient = new ChatStreamClient();
```

### 7.2 Chat Streaming Hook

**File: `hooks/useChat.ts`**

```typescript
import { useState, useCallback } from 'react';
import { chatStreamClient, StreamChunk } from '../api/chat';
import { ChatCompletionRequest } from '../api/types';

export function useChat() {
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const streamMessage = useCallback(
    async (
      request: ChatCompletionRequest,
      onChunk: (chunk: StreamChunk) => void,
      onComplete: () => void
    ) => {
      setIsStreaming(true);
      setError(null);

      try {
        for await (const chunk of chatStreamClient.streamCompletion(request)) {
          onChunk(chunk);
        }
        onComplete();
      } catch (err) {
        setError(err as Error);
        console.error('Chat streaming error:', err);
      } finally {
        setIsStreaming(false);
      }
    },
    []
  );

  return {
    streamMessage,
    isStreaming,
    error,
  };
}
```

---

## 8. Phase 5: File Upload & Management {#phase-5-file-upload--management}

### 8.1 File API

**File: `api/files.ts`**

```typescript
import apiClient from './client';
import { FileItem } from './types';

export const filesApi = {
  // Get all files for user
  async getFiles(userId: string): Promise<FileItem[]> {
    return apiClient.get<FileItem[]>(`/files?userId=${userId}`);
  },

  // Upload file
  async uploadFile(
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<FileItem> {
    return apiClient.uploadFile<FileItem>('/files/upload', file, onProgress);
  },

  // Delete file
  async deleteFile(fileId: string): Promise<void> {
    return apiClient.delete(`/files/${fileId}`);
  },

  // Get file URL
  getFileUrl(fileId: string): string {
    return `${apiClient['client'].defaults.baseURL}/files/${fileId}/download`;
  },
};
```

### 8.2 File Upload Hook

**File: `hooks/useFileUpload.ts`**

```typescript
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { filesApi } from '../api/files';
import { toast } from 'sonner';

export function useFileUpload() {
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => filesApi.uploadFile(file, setUploadProgress),
    onSuccess: (data) => {
      toast.success(`File "${data.name}" uploaded successfully`);
      queryClient.invalidateQueries({ queryKey: ['files'] });
      setUploadProgress(0);
    },
    onError: (error: Error) => {
      toast.error(`Upload failed: ${error.message}`);
      setUploadProgress(0);
    },
  });

  return {
    upload: uploadMutation.mutate,
    isUploading: uploadMutation.isPending,
    uploadProgress,
  };
}
```

---

## 9. Phase 6: Remaining APIs {#phase-6-advanced-features}

### 6.1 Knowledge API

**File: `api/knowledge.ts`**

```typescript
import apiClient from './client';
import { KnowledgeItem } from './types';

export const knowledgeApi = {
  async getKnowledge(userId: string): Promise<KnowledgeItem[]> {
    return apiClient.get<KnowledgeItem[]>(`/knowledge?userId=${userId}`);
  },

  async createKnowledge(data: Partial<KnowledgeItem>): Promise<KnowledgeItem> {
    return apiClient.post<KnowledgeItem>('/knowledge', data);
  },

  async updateKnowledge(id: string, data: Partial<KnowledgeItem>): Promise<KnowledgeItem> {
    return apiClient.put<KnowledgeItem>(`/knowledge/${id}`, data);
  },

  async deleteKnowledge(id: string): Promise<void> {
    return apiClient.delete(`/knowledge/${id}`);
  },
};
```

### 6.2 Settings API

**File: `api/settings.ts`**

```typescript
import apiClient from './client';
import { UserSettings } from './types';

export const settingsApi = {
  async getSettings(userId: string): Promise<UserSettings> {
    return apiClient.get<UserSettings>(`/settings?userId=${userId}`);
  },

  async updateSettings(userId: string, settings: Partial<UserSettings>): Promise<UserSettings> {
    return apiClient.put<UserSettings>(`/settings/${userId}`, settings);
  },
};
```

### 6.3 Connections API

**File: `api/connections.ts`**

```typescript
import apiClient from './client';
import { Connection } from './types';

export const connectionsApi = {
  async getConnections(): Promise<Connection[]> {
    return apiClient.get<Connection[]>('/connections');
  },

  async createConnection(data: Partial<Connection>): Promise<Connection> {
    return apiClient.post<Connection>('/connections', data);
  },

  async updateConnection(id: string, data: Partial<Connection>): Promise<Connection> {
    return apiClient.put<Connection>(`/connections/${id}`, data);
  },

  async deleteConnection(id: string): Promise<void> {
    return apiClient.delete(`/connections/${id}`);
  },

  async testConnection(id: string): Promise<{ success: boolean; message: string }> {
    return apiClient.post(`/connections/${id}/test`, {});
  },
};
```

---

## 10. Migration Steps {#testing-strategy}

### Step-by-Step Implementation

#### Week 1: Foundation
- [ ] Install dependencies (axios, react-query)
- [ ] Create base API client with interceptors
- [ ] Set up environment variables
- [ ] Create TypeScript type definitions
- [ ] Implement authentication context
- [ ] Create login/logout functionality
- [ ] Test authentication flow

#### Week 2: Core Features
- [ ] Implement workspace API
- [ ] Create workspace hooks
- [ ] Update ConversationSidebar to use real data
- [ ] Implement conversation CRUD
- [ ] Test workspace/conversation management

#### Week 3: Chat & Streaming
- [ ] Implement chat streaming API
- [ ] Create chat streaming hook
- [ ] Update ChatInput to send real messages
- [ ] Update ChatMessage to display streamed responses
- [ ] Test real-time chat

#### Week 4: Files & Attachments
- [ ] Implement file upload API
- [ ] Create file upload hook
- [ ] Update FilesModal with real data
- [ ] Implement attachment system
- [ ] Test file operations

#### Week 5: Settings & Configuration
- [ ] Implement settings API
- [ ] Update SettingsModal with real data
- [ ] Implement connection management
- [ ] Update ConnectionEditDialog
- [ ] Test settings persistence

#### Week 6: Polish & Testing
- [ ] Remove api-stubs.ts
- [ ] Implement error boundaries
- [ ] Add loading states throughout
- [ ] Write integration tests
- [ ] Performance optimization
- [ ] Documentation

---

## 11. Deployment Considerations {#deployment-considerations}

### Backend Configuration

**Update Program.cs CORS for production**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        if (builder.Environment.IsProduction())
        {
            policy.WithOrigins("https://your-frontend-domain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});
```

### Frontend Build Configuration

**Update package.json**:
```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "build:prod": "tsc && vite build --mode production",
    "preview": "vite preview"
  }
}
```

### Hosting Options

**Option 1: Separate Deployments**
- Backend: Azure App Service / AWS Elastic Beanstalk
- Frontend: Vercel / Netlify / Azure Static Web Apps

**Option 2: Backend Serves Frontend**
- Build React app
- Copy `dist/` to `AiMate.Web/wwwroot`
- Configure .NET to serve static files

**File: Update `Program.cs`**:
```csharp
// Serve React app
app.UseDefaultFiles();
app.UseStaticFiles();

// Fallback to index.html for client-side routing
app.MapFallbackToFile("index.html");
```

---

## 12. Testing Checklist

### Unit Tests
- [ ] API client interceptors
- [ ] Auth token management
- [ ] Type conversions
- [ ] Error handling

### Integration Tests
- [ ] Login/logout flow
- [ ] Workspace CRUD operations
- [ ] Message sending/receiving
- [ ] File uploads
- [ ] Settings persistence

### E2E Tests
- [ ] User registration → login → chat
- [ ] Create workspace → conversation → messages
- [ ] Upload file → attach to message
- [ ] Change settings → verify persistence

---

## 13. Success Metrics

Track these metrics to measure integration success:

1. **API Response Times**: < 200ms for CRUD, < 2s for AI responses
2. **Error Rate**: < 1% of API calls
3. **User Experience**: Loading states show within 100ms
4. **Chat Latency**: Streaming starts within 500ms
5. **File Upload**: Support files up to 10MB with progress

---

## Conclusion

This integration plan provides a comprehensive roadmap for connecting the TSReactUI frontend to the AiMate.Web backend. Follow the phases sequentially, test thoroughly at each stage, and maintain type safety throughout the application.

**Next Steps**:
1. Review this plan with the team
2. Set up development environment
3. Begin Phase 1: API Client Setup
4. Track progress using the migration checklist

For questions or clarifications, refer to:
- Backend API documentation in each controller's XML comments
- Frontend component specs in `COMPONENT_ARCHITECTURE_SPEC.md`
- This integration plan

Good luck with the integration! 🚀
