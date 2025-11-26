# aiMate REST API Documentation

**Version:** v1
**Base URL:** `https://your-domain.com/api/v1`
**Format:** JSON
**Authentication:** Bearer Token (API Key)

---

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Rate Limiting](#rate-limiting)
4. [Error Handling](#error-handling)
5. [Chat API](#chat-api)
6. [Workspace API](#workspace-api)
7. [Conversation API](#conversation-api)
8. [Feedback API](#feedback-api)
9. [Knowledge Base API](#knowledge-base-api)
10. [Tools API](#tools-api)
11. [Search API](#search-api)
12. [Notes API](#notes-api)
13. [Settings API](#settings-api)
14. [Usage API](#usage-api)
15. [Admin API](#admin-api)
16. [Code Examples](#code-examples)

---

## Overview

The aiMate REST API provides programmatic access to all aiMate functionality. It's designed to be:

- **OpenAI-Compatible** - Chat endpoints follow OpenAI's format
- **RESTful** - Standard HTTP methods and status codes
- **JSON-First** - All requests and responses use JSON
- **Versioned** - `/api/v1` prefix for stability
- **Well-Documented** - Clear errors and validation messages

### Supported Features

- ✅ Chat completions (streaming and non-streaming)
- ✅ Workspace management
- ✅ Feedback and ratings
- ✅ Knowledge base search
- ✅ MCP tool execution
- ✅ Admin operations

### API Tiers

| Tier | Access | Rate Limit |
|------|--------|------------|
| **Free** | Web UI only | N/A |
| **BYOK ($10/mo)** | API + Web UI | 60 req/min, 10K req/day |
| **Developer ($30/mo)** | Full API access | 120 req/min, 50K req/day |

---

## Authentication

All API requests require authentication via API key.

### Generating an API Key

**Via Web UI:**
1. Navigate to Settings → API Keys
2. Click "Generate New Key"
3. Copy the key (shown only once!)
4. Use in `Authorization` header

**Via Admin Panel** (for admins):
```http
POST /api/v1/admin/api-keys
Content-Type: application/json

{
  "userId": "user-guid",
  "name": "My API Key",
  "description": "For my app"
}
```

### Using API Keys

**Format:** `sk-aimate-{32-character-random-string}`

**Include in requests:**
```http
Authorization: Bearer sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### Security Best Practices

- ✅ Never commit API keys to version control
- ✅ Rotate keys regularly (every 90 days recommended)
- ✅ Use environment variables for storage
- ✅ Revoke unused keys immediately
- ✅ Use separate keys for dev/staging/production

### API Key Management

**List Your Keys:**
```http
GET /api/v1/admin/api-keys
Authorization: Bearer YOUR_KEY
```

**Revoke a Key:**
```http
DELETE /api/v1/admin/api-keys/{keyId}
Authorization: Bearer YOUR_KEY
```

---

## Rate Limiting

Rate limits are enforced per API key.

### Limits by Tier

| Tier | Requests/Minute | Requests/Day |
|------|----------------|--------------|
| BYOK | 60 | 10,000 |
| Developer | 120 | 50,000 |

### Rate Limit Headers

Every response includes:

```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1640000000
```

### Handling Rate Limits

**429 Too Many Requests:**
```json
{
  "error": {
    "message": "Rate limit exceeded",
    "type": "rate_limit_error",
    "retry_after": 60
  }
}
```

**Best Practices:**
- Implement exponential backoff
- Cache responses when possible
- Use webhooks instead of polling
- Monitor rate limit headers

---

## Error Handling

### Standard Error Format

All errors return this structure:

```json
{
  "error": {
    "message": "Human-readable error message",
    "type": "error_type",
    "code": "ERROR_CODE",
    "details": {}
  }
}
```

### HTTP Status Codes

| Code | Meaning | Common Causes |
|------|---------|---------------|
| `200` | OK | Request succeeded |
| `201` | Created | Resource created successfully |
| `400` | Bad Request | Invalid request body or parameters |
| `401` | Unauthorized | Missing or invalid API key |
| `403` | Forbidden | Valid key but insufficient permissions |
| `404` | Not Found | Resource doesn't exist |
| `422` | Unprocessable Entity | Validation failed |
| `429` | Too Many Requests | Rate limit exceeded |
| `500` | Internal Server Error | Server-side error |
| `503` | Service Unavailable | Temporary maintenance or overload |

### Error Types

```typescript
type ErrorType =
  | "authentication_error"    // Invalid or missing API key
  | "authorization_error"     // Insufficient permissions
  | "rate_limit_error"        // Too many requests
  | "validation_error"        // Invalid input
  | "not_found_error"         // Resource not found
  | "internal_error"          // Server-side error
  | "service_unavailable";    // Temporary outage
```

### Example Error Responses

**401 Unauthorized:**
```json
{
  "error": {
    "message": "Invalid API key",
    "type": "authentication_error",
    "code": "INVALID_API_KEY"
  }
}
```

**422 Validation Error:**
```json
{
  "error": {
    "message": "Validation failed",
    "type": "validation_error",
    "code": "VALIDATION_FAILED",
    "details": {
      "name": ["Name is required"],
      "email": ["Invalid email format"]
    }
  }
}
```

---

## Chat API

OpenAI-compatible chat completions API.

### POST /api/v1/chat/completions

Create a chat completion (non-streaming).

**Request:**
```http
POST /api/v1/chat/completions
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "model": "gpt-4",
  "messages": [
    {
      "role": "system",
      "content": "You are a helpful assistant."
    },
    {
      "role": "user",
      "content": "What is the capital of New Zealand?"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 500,
  "top_p": 1.0,
  "frequency_penalty": 0.0,
  "presence_penalty": 0.0
}
```

**Response:**
```json
{
  "id": "chatcmpl-123",
  "object": "chat.completion",
  "created": 1677652288,
  "model": "gpt-4",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "The capital of New Zealand is Wellington."
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 20,
    "completion_tokens": 10,
    "total_tokens": 30
  }
}
```

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `model` | string | Yes | Model ID (e.g., "gpt-4", "claude-3-5-sonnet-20241022") |
| `messages` | array | Yes | Array of message objects |
| `temperature` | number | No | 0.0 to 2.0, default 0.7 |
| `max_tokens` | integer | No | Maximum tokens to generate |
| `top_p` | number | No | Nucleus sampling, 0.0 to 1.0 |
| `frequency_penalty` | number | No | -2.0 to 2.0, default 0.0 |
| `presence_penalty` | number | No | -2.0 to 2.0, default 0.0 |

### POST /api/v1/chat/completions/stream

Create a streaming chat completion.

**Request:**
```http
POST /api/v1/chat/completions/stream
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "model": "gpt-4",
  "messages": [
    {
      "role": "user",
      "content": "Count to 5."
    }
  ],
  "stream": true
}
```

**Response (Server-Sent Events):**
```
data: {"id":"chatcmpl-123","choices":[{"delta":{"content":"1"},"index":0}]}

data: {"id":"chatcmpl-123","choices":[{"delta":{"content":", 2"},"index":0}]}

data: {"id":"chatcmpl-123","choices":[{"delta":{"content":", 3"},"index":0}]}

data: {"id":"chatcmpl-123","choices":[{"delta":{"content":", 4"},"index":0}]}

data: {"id":"chatcmpl-123","choices":[{"delta":{"content":", 5"},"index":0}]}

data: [DONE]
```

**Client-Side Handling:**
```javascript
const response = await fetch('/api/v1/chat/completions/stream', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer YOUR_API_KEY',
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    model: 'gpt-4',
    messages: [{ role: 'user', content: 'Hello!' }],
    stream: true
  })
});

const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { done, value } = await reader.read();
  if (done) break;

  const chunk = decoder.decode(value);
  const lines = chunk.split('\n').filter(line => line.trim() !== '');

  for (const line of lines) {
    if (line === 'data: [DONE]') break;
    if (line.startsWith('data: ')) {
      const json = JSON.parse(line.slice(6));
      const content = json.choices[0]?.delta?.content || '';
      console.log(content); // Process each chunk
    }
  }
}
```

---

## Workspace API

Manage workspaces and conversations.

### GET /api/v1/workspaces

Get all workspaces for the authenticated user.

**Request:**
```http
GET /api/v1/workspaces
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Project",
    "description": "Project workspace",
    "type": "Project",
    "personality": "KiwiDev",
    "createdAt": "2025-01-18T10:00:00Z",
    "updatedAt": "2025-01-18T12:30:00Z"
  }
]
```

### GET /api/v1/workspaces/{id}

Get a specific workspace by ID.

**Request:**
```http
GET /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Project",
  "description": "Project workspace",
  "type": "Project",
  "personality": "KiwiDev",
  "conversations": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "title": "Discussion about API",
      "createdAt": "2025-01-18T10:00:00Z"
    }
  ],
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T12:30:00Z"
}
```

### POST /api/v1/workspaces

Create a new workspace.

**Request:**
```http
POST /api/v1/workspaces
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "name": "New Workspace",
  "description": "Description of workspace",
  "type": "Project",
  "personality": "KiwiMate"
}
```

**Response:**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "name": "New Workspace",
  "description": "Description of workspace",
  "type": "Project",
  "personality": "KiwiMate",
  "createdAt": "2025-01-18T14:00:00Z",
  "updatedAt": "2025-01-18T14:00:00Z"
}
```

### PUT /api/v1/workspaces/{id}

Update an existing workspace.

**Request:**
```http
PUT /api/v1/workspaces/770e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "name": "Updated Workspace Name",
  "description": "Updated description"
}
```

**Response:**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "name": "Updated Workspace Name",
  "description": "Updated description",
  "type": "Project",
  "personality": "KiwiMate",
  "createdAt": "2025-01-18T14:00:00Z",
  "updatedAt": "2025-01-18T14:30:00Z"
}
```

### DELETE /api/v1/workspaces/{id}

Delete a workspace.

**Request:**
```http
DELETE /api/v1/workspaces/770e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```http
204 No Content
```

---

## Conversation API

Manage conversations within workspaces. Full CRUD operations available.

### GET /api/v1/workspaces/{workspaceId}/conversations

Get all conversations in a workspace.

**Request:**
```http
GET /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
[
  {
    "id": 660000,
    "workspaceId": 550000,
    "title": "Project Discussion",
    "modelId": "gpt-4",
    "isPinned": false,
    "isArchived": false,
    "createdAt": "2025-01-18T10:00:00Z",
    "updatedAt": "2025-01-18T12:30:00Z",
    "messageCount": 15
  }
]
```

### GET /api/v1/workspaces/{workspaceId}/conversations/{conversationId}

Get a specific conversation by ID.

**Request:**
```http
GET /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "id": 660000,
  "workspaceId": 550000,
  "title": "Project Discussion",
  "modelId": "gpt-4",
  "isPinned": false,
  "isArchived": false,
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T12:30:00Z",
  "messageCount": 15
}
```

### POST /api/v1/workspaces/{workspaceId}/conversations

Create a new conversation in a workspace.

**Request:**
```http
POST /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "title": "New Discussion"
}
```

**Response:**
```json
{
  "id": 770000,
  "workspaceId": 550000,
  "title": "New Discussion",
  "modelId": null,
  "isPinned": false,
  "isArchived": false,
  "createdAt": "2025-01-18T14:00:00Z",
  "updatedAt": "2025-01-18T14:00:00Z",
  "messageCount": 0
}
```

### PUT /api/v1/workspaces/{workspaceId}/conversations/{conversationId}

Update an existing conversation.

**Request:**
```http
PUT /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "title": "Updated Title",
  "modelId": "gpt-4",
  "isPinned": true,
  "isArchived": false
}
```

**Response:**
```json
{
  "id": 660000,
  "workspaceId": 550000,
  "title": "Updated Title",
  "modelId": "gpt-4",
  "isPinned": true,
  "isArchived": false,
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T14:30:00Z",
  "messageCount": 15
}
```

**Update Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `title` | string | No | Conversation title |
| `modelId` | string | No | AI model ID for this conversation |
| `isPinned` | boolean | No | Pin/unpin the conversation |
| `isArchived` | boolean | No | Archive/unarchive the conversation |

### DELETE /api/v1/workspaces/{workspaceId}/conversations/{conversationId}

Delete a conversation.

**Request:**
```http
DELETE /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "success": true,
  "message": "Conversation deleted successfully"
}
```

---

## Feedback API

Rate and provide feedback on AI responses.

### POST /api/v1/feedback/messages/{messageId}

Create or update feedback for a message.

**Request:**
```http
POST /api/v1/feedback/messages/880e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "userId": "user-guid",
  "rating": 8,
  "textFeedback": "Great response, very helpful!",
  "tags": [
    {
      "key": "quality",
      "value": "high",
      "color": "success",
      "sentiment": "positive"
    },
    {
      "key": "accuracy",
      "value": "accurate",
      "color": "success",
      "sentiment": "positive"
    }
  ],
  "modelId": "gpt-4",
  "responseTimeMs": 2500
}
```

**Response:**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440000",
  "rating": 8,
  "createdAt": "2025-01-18T15:00:00Z",
  "updatedAt": "2025-01-18T15:00:00Z"
}
```

### PUT /api/v1/feedback/{feedbackId}

Update existing feedback for a message.

**Request:**
```http
PUT /api/v1/feedback/990e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "rating": 9,
  "textFeedback": "Even better on second thought!",
  "tags": [
    {
      "key": "quality",
      "value": "very-high",
      "color": "success",
      "sentiment": "positive"
    }
  ],
  "modelId": "gpt-4",
  "responseTimeMs": 2200
}
```

**Response:**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440000",
  "rating": 9,
  "createdAt": "2025-01-18T15:00:00Z",
  "updatedAt": "2025-01-18T15:05:00Z"
}
```

### GET /api/v1/feedback/messages/{messageId}

Get feedback for a specific message.

**Request:**
```http
GET /api/v1/feedback/messages/880e8400-e29b-41d4-a716-446655440000
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440000",
  "messageId": "880e8400-e29b-41d4-a716-446655440000",
  "userId": "user-guid",
  "rating": 9,
  "textFeedback": "Even better on second thought!",
  "tags": [
    {
      "key": "quality",
      "value": "very-high",
      "color": "success",
      "sentiment": "positive"
    }
  ],
  "modelId": "gpt-4",
  "responseTimeMs": 2200,
  "createdAt": "2025-01-18T15:00:00Z",
  "updatedAt": "2025-01-18T15:05:00Z"
}
```

### GET /api/v1/feedback/stats/models/{modelId}

Get aggregate statistics for a model.

**Request:**
```http
GET /api/v1/feedback/stats/models/gpt-4
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "modelId": "gpt-4",
  "totalFeedback": 1250,
  "averageRating": 7.8,
  "ratingDistribution": {
    "1": 10,
    "2": 15,
    "3": 25,
    "4": 50,
    "5": 80,
    "6": 120,
    "7": 200,
    "8": 350,
    "9": 250,
    "10": 150
  },
  "averageResponseTime": 2800,
  "positivePercentage": 82.5,
  "negativePercentage": 5.2,
  "neutralPercentage": 12.3
}
```

### GET /api/v1/feedback/stats/tags

Get tag frequency statistics.

**Request:**
```http
GET /api/v1/feedback/stats/tags
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "tags": [
    {
      "key": "quality",
      "value": "high",
      "count": 450,
      "percentage": 36.0
    },
    {
      "key": "accuracy",
      "value": "accurate",
      "count": 380,
      "percentage": 30.4
    }
  ],
  "totalTags": 1250
}
```

---

## Knowledge Base API

Search and manage knowledge base items.

### GET /api/v1/knowledge

Get knowledge articles.

**Request:**
```http
GET /api/v1/knowledge?userId=user-guid
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
[
  {
    "id": "article-1",
    "title": "How to use aiMate",
    "content": "Full article content...",
    "type": "Documentation",
    "category": "Guides",
    "tags": ["tutorial", "getting-started"],
    "visibility": "Public",
    "isPublished": true,
    "isFeatured": false,
    "viewCount": 150,
    "referenceCount": 12,
    "createdAt": "2025-01-10T10:00:00Z",
    "updatedAt": "2025-01-15T12:00:00Z",
    "lastViewedAt": "2025-01-18T14:30:00Z"
  }
]
```

### GET /api/v1/knowledge/analytics

Get knowledge base analytics.

**Request:**
```http
GET /api/v1/knowledge/analytics?userId=user-guid
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "totalArticles": 45,
  "totalViews": 2500,
  "totalReferences": 180,
  "mostViewed": [
    {
      "id": "article-1",
      "title": "How to use aiMate",
      "viewCount": 150
    }
  ],
  "mostReferenced": [
    {
      "id": "article-2",
      "title": "API Documentation",
      "referenceCount": 45
    }
  ],
  "tagCounts": {
    "tutorial": 15,
    "api": 12,
    "guide": 20
  },
  "typeCounts": {
    "Documentation": 25,
    "Article": 20
  }
}
```

### POST /api/v1/knowledge/search

Semantic search across knowledge base.

**Request:**
```http
POST /api/v1/knowledge/search
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "query": "How do I create a workspace?",
  "limit": 10,
  "threshold": 0.7
}
```

**Response:**
```json
{
  "results": [
    {
      "id": "article-5",
      "title": "Creating Workspaces",
      "content": "To create a workspace...",
      "score": 0.92,
      "relevance": "high"
    }
  ],
  "totalResults": 3,
  "queryTime": 45
}
```

---

## Tools API

Execute MCP tools programmatically.

### GET /api/v1/tools

List available MCP tools.

**Request:**
```http
GET /api/v1/tools
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
[
  {
    "name": "web_search",
    "description": "Search the web for information",
    "category": "Search",
    "parameters": {
      "query": {
        "type": "string",
        "description": "Search query",
        "required": true
      },
      "max_results": {
        "type": "integer",
        "description": "Maximum number of results",
        "required": false,
        "default": 5
      }
    }
  },
  {
    "name": "read_file",
    "description": "Read a workspace file",
    "category": "Files",
    "parameters": {
      "fileId": {
        "type": "string",
        "description": "File ID to read",
        "required": true
      }
    }
  }
]
```

### POST /api/v1/tools/execute

Execute a specific tool.

**Request:**
```http
POST /api/v1/tools/execute
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "tool": "web_search",
  "parameters": {
    "query": "New Zealand AI news",
    "max_results": 5
  }
}
```

**Response:**
```json
{
  "success": true,
  "toolName": "web_search",
  "result": {
    "results": [
      {
        "title": "AI Innovation in NZ",
        "url": "https://example.com/article",
        "snippet": "New Zealand leads in AI..."
      }
    ]
  },
  "executionTime": 1200,
  "timestamp": "2025-01-18T16:00:00Z"
}
```

---

## Search API

Search across conversations, messages, and knowledge base.

### GET /api/v1/search/conversations

Search conversations by title or metadata.

**Request:**
```http
GET /api/v1/search/conversations?q=project&limit=10
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "results": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "title": "Project Discussion",
      "workspaceId": "550e8400-e29b-41d4-a716-446655440000",
      "relevance": 0.95
    }
  ],
  "totalResults": 1
}
```

### GET /api/v1/search/messages

Search messages by content.

**Request:**
```http
GET /api/v1/search/messages?q=API documentation&limit=10
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "results": [
    {
      "id": "msg-123",
      "content": "Here is the API documentation...",
      "conversationId": "660e8400-e29b-41d4-a716-446655440000",
      "createdAt": "2025-01-18T10:00:00Z",
      "relevance": 0.92
    }
  ],
  "totalResults": 5
}
```

### GET /api/v1/search/knowledge/semantic

Semantic search in knowledge base using AI embeddings.

**Request:**
```http
GET /api/v1/search/knowledge/semantic?q=how to use workspaces&limit=5&threshold=0.7
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "results": [
    {
      "id": "article-1",
      "title": "Workspace Management Guide",
      "content": "Workspaces are...",
      "score": 0.89,
      "relevance": "high"
    }
  ],
  "totalResults": 3
}
```

### GET /api/v1/search/knowledge

Full-text search in knowledge base.

**Request:**
```http
GET /api/v1/search/knowledge?q=API&limit=10
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "results": [
    {
      "id": "article-2",
      "title": "REST API Documentation",
      "snippet": "The REST API provides programmatic access...",
      "viewCount": 150
    }
  ],
  "totalResults": 8
}
```

### GET /api/v1/search

Global search across all content types.

**Request:**
```http
GET /api/v1/search?q=workspace&limit=20
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "conversations": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "title": "Workspace Discussion"
    }
  ],
  "messages": [
    {
      "id": "msg-123",
      "content": "Let's discuss workspace features..."
    }
  ],
  "knowledge": [
    {
      "id": "article-1",
      "title": "Workspace Guide"
    }
  ]
}
```

---

## Notes API

Create and manage personal notes.

### GET /api/v1/notesapi

Get all notes for the authenticated user.

**Request:**
```http
GET /api/v1/notesapi
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
[
  {
    "id": "note-1",
    "title": "Meeting Notes",
    "content": "Discussion about new features...",
    "createdAt": "2025-01-18T10:00:00Z",
    "updatedAt": "2025-01-18T12:30:00Z"
  }
]
```

### GET /api/v1/notesapi/{id}

Get a specific note by ID.

**Request:**
```http
GET /api/v1/notesapi/note-1
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "id": "note-1",
  "title": "Meeting Notes",
  "content": "Discussion about new features...",
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T12:30:00Z"
}
```

### POST /api/v1/notesapi

Create a new note.

**Request:**
```http
POST /api/v1/notesapi
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "title": "Quick Note",
  "content": "Important information to remember..."
}
```

**Response:**
```json
{
  "id": "note-2",
  "title": "Quick Note",
  "content": "Important information to remember...",
  "createdAt": "2025-01-18T14:00:00Z",
  "updatedAt": "2025-01-18T14:00:00Z"
}
```

### PUT /api/v1/notesapi/{id}

Update an existing note.

**Request:**
```http
PUT /api/v1/notesapi/note-1
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "title": "Updated Meeting Notes",
  "content": "Updated content..."
}
```

**Response:**
```json
{
  "id": "note-1",
  "title": "Updated Meeting Notes",
  "content": "Updated content...",
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T14:30:00Z"
}
```

### DELETE /api/v1/notesapi/{id}

Delete a note.

**Request:**
```http
DELETE /api/v1/notesapi/note-1
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```http
204 No Content
```

---

## Settings API

Manage user settings and preferences.

### GET /api/v1/settings

Get user settings.

**Request:**
```http
GET /api/v1/settings
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "theme": "dark",
  "notifications": {
    "email": true,
    "push": false
  },
  "language": "en",
  "timezone": "Pacific/Auckland",
  "defaultModel": "gpt-4"
}
```

### POST /api/v1/settings

Update user settings (legacy POST method).

**Request:**
```http
POST /api/v1/settings?userId=user-guid
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "theme": "light",
  "notifications": {
    "email": false,
    "push": true
  },
  "defaultModel": "claude-3-5-sonnet-20241022"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Settings updated successfully"
}
```

### PUT /api/v1/settings

Update user settings (REST compliant).

**Request:**
```http
PUT /api/v1/settings?userId=user-guid
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "theme": "light",
  "notifications": {
    "email": false,
    "push": true
  },
  "language": "en",
  "defaultModel": "claude-3-5-sonnet-20241022",
  "defaultPersonality": "KiwiDev"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Settings updated successfully"
}
```

### DELETE /api/v1/settings

Reset user settings to defaults.

**Request:**
```http
DELETE /api/v1/settings?userId=user-guid
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "username": "john.doe",
  "email": "john@example.com",
  "userTier": "Developer",
  "defaultPersonality": "KiwiDev"
}
```

---

## Usage API

Get usage statistics and metrics.

### GET /api/v1/usage

Get usage statistics for the authenticated user.

**Request:**
```http
GET /api/v1/usage?period=month
Authorization: Bearer YOUR_API_KEY
```

**Response:**
```json
{
  "period": "month",
  "totalRequests": 2450,
  "requestsRemaining": 7550,
  "tokensUsed": 125000,
  "costs": {
    "currency": "USD",
    "amount": 12.50
  },
  "breakdown": {
    "chatCompletions": {
      "count": 1200,
      "tokensUsed": 95000
    },
    "searchRequests": {
      "count": 800,
      "tokensUsed": 20000
    },
    "other": {
      "count": 450,
      "tokensUsed": 10000
    }
  },
  "resetDate": "2025-02-18T00:00:00Z"
}
```

---

## Admin API

Administrative operations (requires admin role).

### GET /api/v1/admin/api-keys

List all API keys (admin only).

**Request:**
```http
GET /api/v1/admin/api-keys
Authorization: Bearer YOUR_ADMIN_API_KEY
```

**Response:**
```json
[
  {
    "id": "key-1",
    "userId": "user-guid",
    "name": "Production Key",
    "hashedKey": "bcrypt-hash",
    "createdAt": "2025-01-10T10:00:00Z",
    "lastUsedAt": "2025-01-18T15:30:00Z",
    "isRevoked": false,
    "requestsPerMinute": 60,
    "requestsPerDay": 10000
  }
]
```

### POST /api/v1/admin/api-keys

Create an API key for a user (admin only).

**Request:**
```http
POST /api/v1/admin/api-keys
Authorization: Bearer YOUR_ADMIN_API_KEY
Content-Type: application/json

{
  "userId": "user-guid",
  "name": "New API Key",
  "description": "For external integration"
}
```

**Response:**
```json
{
  "id": "key-2",
  "apiKey": "sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "createdAt": "2025-01-18T16:30:00Z"
}
```

**⚠️ Important:** The `apiKey` is only returned once. Store it securely!

### DELETE /api/v1/admin/api-keys/{keyId}

Revoke an API key (admin only).

**Request:**
```http
DELETE /api/v1/admin/api-keys/key-1
Authorization: Bearer YOUR_ADMIN_API_KEY
```

**Response:**
```http
204 No Content
```

---

## Additional APIs

The following APIs are also available in the system:

### Connection API (`/api/v1/connection`)

Manage connection status and connection-related operations.

- **GET** `/` - Get connection status
- **POST** `/` - Establish connection

### Code Files API (`/api/v1/codefiles`)

Manage code files and code snippets.

- **GET** `/` - List code files
- **POST** `/` - Create/upload code file
- **GET** `/{id}` - Get specific code file
- **PUT** `/{id}` - Update code file
- **DELETE** `/{id}` - Delete code file

### Projects API (`/api/v1/projects`)

Manage projects and project organization.

- **GET** `/` - List projects
- **POST** `/` - Create project
- **GET** `/{id}` - Get specific project
- **PUT** `/{id}` - Update project
- **DELETE** `/{id}` - Delete project

### Organization API (`/api/v1/organization`)

Manage organization settings and members.

- **GET** `/` - Get organization info
- **POST** `/` - Create organization
- **PUT** `/` - Update organization

### Group API (`/api/v1/group`)

Manage user groups and group permissions.

- **GET** `/` - List groups
- **POST** `/` - Create group
- **GET** `/{id}` - Get specific group
- **PUT** `/{id}` - Update group
- **DELETE** `/{id}` - Delete group

### Plugin API (`/api/v1/plugins`)

Manage plugins and extensions.

**Plugin Management:**
- **GET** `/` - List all loaded plugins
- **GET** `/{id}` - Get specific plugin details
- **POST** `/{id}/toggle` - Toggle plugin enabled state
- **POST** `/reload` - Reload all plugins

**Plugin Settings:**
- **GET** `/{id}/settings` - Get plugin settings schema and values
- **POST** `/{id}/settings` - Save plugin settings
- **PUT** `/{id}/settings` - Update plugin settings (REST compliant)
- **DELETE** `/{id}/settings` - Delete plugin settings (admin only)

**Plugin Tools & Actions:**
- **GET** `/tools` - Get available tools from all plugins
- **POST** `/actions` - Get message actions for a message

### Code Compilation API (`/api/v1/codecompilation`)

Compile and execute code.

- **POST** `/` - Compile code
- **POST** `/execute` - Execute compiled code

### Attachments API (`/api/v1/attachments`)

Manage file attachments.

- **GET** `/` - List attachments
- **POST** `/` - Upload attachment
- **GET** `/{id}` - Get attachment
- **DELETE** `/{id}` - Delete attachment

### File API (`/api/v1/file`)

Manage files and file operations.

- **GET** `/` - List files
- **POST** `/` - Upload file
- **GET** `/{id}` - Get file
- **PUT** `/{id}` - Update file metadata
- **DELETE** `/{id}` - Delete file

### Structured Content API (`/api/v1/structuredcontent`)

Manage structured content and templates.

- **GET** `/` - List content items
- **POST** `/` - Create content
- **GET** `/{id}` - Get specific content
- **PUT** `/{id}` - Update content
- **DELETE** `/{id}` - Delete content

### Feedback System API (`/api/v1`)

System-level feedback operations and error logging.

**User Feedback:**
- **POST** `/system-feedback` - Submit user feedback
- **GET** `/system-feedback` - Get user feedback
- **GET** `/system-feedback/filter` - Get filtered feedback (admin)
- **GET** `/system-feedback/{id}` - Get specific feedback
- **PUT** `/system-feedback/{id}/status` - Update feedback status (admin)
- **PUT** `/system-feedback/{id}/assign` - Assign feedback to admin
- **DELETE** `/system-feedback/{id}` - Delete feedback
- **GET** `/system-feedback/stats` - Get feedback statistics (admin)

**Feedback Templates:**
- **GET** `/feedback/templates` - Get feedback templates
- **POST** `/feedback/templates` - Create template (admin)
- **PUT** `/feedback/templates/{id}` - Update template (admin)
- **DELETE** `/feedback/templates/{id}` - Delete template (admin)

**Error Logging:**
- **POST** `/errors/log` - Log frontend error (public, rate-limited)
- **GET** `/errors` - Get error logs (admin)
- **GET** `/errors/{id}` - Get specific error (admin)
- **PUT** `/errors/{id}/resolve` - Mark error as resolved (admin)
- **DELETE** `/errors/{id}` - Delete error log (admin)
- **GET** `/errors/stats` - Get error statistics (admin)

### Monaco Config API (`/api/v1/monacoconfig`)

Get and manage Monaco editor configuration.

- **GET** `/` - Get Monaco editor config
- **POST** `/` - Update Monaco editor config

---

## Code Examples

### cURL

**Chat Completion:**
```bash
curl -X POST https://your-domain.com/api/v1/chat/completions \
  -H "Authorization: Bearer sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4",
    "messages": [
      {
        "role": "user",
        "content": "Hello, aiMate!"
      }
    ],
    "temperature": 0.7
  }'
```

**Streaming:**
```bash
curl -N -X POST https://your-domain.com/api/v1/chat/completions/stream \
  -H "Authorization: Bearer sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4",
    "messages": [
      {"role": "user", "content": "Count to 10"}
    ],
    "stream": true
  }'
```

### Python

```python
import requests

API_KEY = "sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
BASE_URL = "https://your-domain.com/api/v1"

def chat_completion(message):
    headers = {
        "Authorization": f"Bearer {API_KEY}",
        "Content-Type": "application/json"
    }

    payload = {
        "model": "gpt-4",
        "messages": [
            {"role": "user", "content": message}
        ],
        "temperature": 0.7
    }

    response = requests.post(
        f"{BASE_URL}/chat/completions",
        headers=headers,
        json=payload
    )

    return response.json()

# Usage
result = chat_completion("What is the capital of New Zealand?")
print(result["choices"][0]["message"]["content"])
```

**Streaming:**
```python
import requests
import json

def chat_completion_stream(message):
    headers = {
        "Authorization": f"Bearer {API_KEY}",
        "Content-Type": "application/json"
    }

    payload = {
        "model": "gpt-4",
        "messages": [{"role": "user", "content": message}],
        "stream": True
    }

    response = requests.post(
        f"{BASE_URL}/chat/completions/stream",
        headers=headers,
        json=payload,
        stream=True
    )

    for line in response.iter_lines():
        if line:
            line = line.decode('utf-8')
            if line.startswith('data: '):
                if line == 'data: [DONE]':
                    break
                data = json.loads(line[6:])
                content = data['choices'][0]['delta'].get('content', '')
                if content:
                    print(content, end='', flush=True)

# Usage
chat_completion_stream("Count to 10")
```

### JavaScript/TypeScript

```typescript
const API_KEY = "sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
const BASE_URL = "https://your-domain.com/api/v1";

interface ChatMessage {
  role: "system" | "user" | "assistant";
  content: string;
}

interface ChatCompletionRequest {
  model: string;
  messages: ChatMessage[];
  temperature?: number;
  max_tokens?: number;
}

async function chatCompletion(message: string): Promise<string> {
  const response = await fetch(`${BASE_URL}/chat/completions`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${API_KEY}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      model: "gpt-4",
      messages: [{ role: "user", content: message }],
      temperature: 0.7
    } as ChatCompletionRequest)
  });

  const data = await response.json();
  return data.choices[0].message.content;
}

// Usage
chatCompletion("What is the capital of New Zealand?")
  .then(response => console.log(response));
```

**Streaming:**
```typescript
async function chatCompletionStream(
  message: string,
  onChunk: (chunk: string) => void
): Promise<void> {
  const response = await fetch(`${BASE_URL}/chat/completions/stream`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${API_KEY}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      model: "gpt-4",
      messages: [{ role: "user", content: message }],
      stream: true
    })
  });

  const reader = response.body!.getReader();
  const decoder = new TextDecoder();

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    const chunk = decoder.decode(value);
    const lines = chunk.split('\n').filter(line => line.trim() !== '');

    for (const line of lines) {
      if (line === 'data: [DONE]') return;
      if (line.startsWith('data: ')) {
        const data = JSON.parse(line.slice(6));
        const content = data.choices[0]?.delta?.content || '';
        if (content) onChunk(content);
      }
    }
  }
}

// Usage
chatCompletionStream("Count to 10", (chunk) => {
  console.log(chunk);
});
```

### C#

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class AiMateClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://your-domain.com/api/v1";

    public AiMateClient(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {apiKey}"
        );
    }

    public async Task<string> ChatCompletionAsync(string message)
    {
        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "user", content = message }
            },
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{BaseUrl}/chat/completions",
            request
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }

    public async Task<List<Workspace>> GetWorkspacesAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/workspaces");
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<Workspace>>()
            ?? new List<Workspace>();
    }
}

// Usage
var client = new AiMateClient("sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
var response = await client.ChatCompletionAsync("Hello!");
Console.WriteLine(response);
```

---

## Best Practices

### Error Handling

```typescript
async function safeChatCompletion(message: string): Promise<string> {
  try {
    const response = await fetch(`${BASE_URL}/chat/completions`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${API_KEY}`,
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        model: "gpt-4",
        messages: [{ role: "user", content: message }]
      })
    });

    if (!response.ok) {
      const error = await response.json();

      if (response.status === 429) {
        // Rate limit - implement retry with backoff
        const retryAfter = error.error.retry_after;
        await sleep(retryAfter * 1000);
        return safeChatCompletion(message); // Retry
      }

      if (response.status === 401) {
        throw new Error("Invalid API key");
      }

      throw new Error(error.error.message);
    }

    const data = await response.json();
    return data.choices[0].message.content;
  } catch (error) {
    console.error("API Error:", error);
    throw error;
  }
}
```

### Rate Limit Handling

```python
import time

def chat_with_retry(message, max_retries=3):
    for attempt in range(max_retries):
        try:
            return chat_completion(message)
        except requests.exceptions.HTTPError as e:
            if e.response.status_code == 429:
                retry_after = int(e.response.headers.get('Retry-After', 60))
                print(f"Rate limited. Retrying in {retry_after}s...")
                time.sleep(retry_after)
            else:
                raise

    raise Exception("Max retries exceeded")
```

### Caching Responses

```typescript
const responseCache = new Map<string, { response: string; timestamp: number }>();
const CACHE_TTL = 5 * 60 * 1000; // 5 minutes

async function cachedChatCompletion(message: string): Promise<string> {
  const cached = responseCache.get(message);

  if (cached && Date.now() - cached.timestamp < CACHE_TTL) {
    return cached.response;
  }

  const response = await chatCompletion(message);

  responseCache.set(message, {
    response,
    timestamp: Date.now()
  });

  return response;
}
```

---

## Complete CRUD Operations

For a comprehensive guide to all CRUD operations across all API resources, including:
- 12 resources with complete CRUD implementation
- 7 resources with partial CRUD (missing operations identified)
- Implementation templates for adding missing endpoints
- Standard CRUD patterns and best practices

**See:** [API_CRUD_OPERATIONS.md](../API_CRUD_OPERATIONS.md)

---

## Support

**Need help?**
- 📖 Documentation: https://docs.aimate.co.nz
- 💬 Discussions: https://github.com/ChoonForge/aiMate/discussions
- 🐛 Issues: https://github.com/ChoonForge/aiMate/issues
- 📧 Email: api@aimate.co.nz

**Built with ❤️ from New Zealand** 🇳🇿
