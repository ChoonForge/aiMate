# API Reference Overview

> **Tooltip Summary:** "Complete REST API for integrating aiMate with your applications. OpenAI-compatible endpoints with JWT authentication and streaming support."

---

## 🚀 Introduction

The aiMate API provides programmatic access to all platform features:

- **OpenAI-Compatible** - Drop-in replacement for OpenAI API
- **RESTful Design** - HTTP/JSON, standard status codes
- **JWT Authentication** - Secure token-based auth
- **Streaming Support** - Real-time SSE responses
- **Comprehensive** - 14 controllers, 90+ endpoints
- **Well-Documented** - Swagger/OpenAPI spec included

**Base URL:** `https://api.aimate.co.nz/api/v1`
**Swagger Docs:** `https://api.aimate.co.nz/swagger`

---

## 🔐 Authentication

### JWT Tokens

All API requests (except `/auth/register` and `/auth/login`) require authentication.

#### Getting a Token

```bash
# Register
curl -X POST https://api.aimate.co.nz/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!",
    "name": "John Doe"
  }'

# Login
curl -X POST https://api.aimate.co.nz/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-23T12:00:00Z",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "tier": "BYOK"
}
```

#### Using the Token

Include in `Authorization` header:

```bash
curl -X GET https://api.aimate.co.nz/api/v1/workspaces \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Token Expiry:** 24 hours (configurable)
**Refresh:** Re-login to get new token

See: [Authentication API](Authentication.md)

---

## 📡 API Controllers

### 1. Authentication (`/api/v1/auth`)

Manage user authentication and registration.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/register` | Register new user |
| POST | `/login` | Login and receive JWT token |

**See:** [Authentication API](Authentication.md)

---

### 2. Chat Completions (`/api/v1/chat`)

OpenAI-compatible chat API with streaming support.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/completions` | Chat completion (non-streaming) |
| POST | `/completions/stream` | Chat completion (streaming SSE) |

**Example:**
```bash
curl -X POST https://api.aimate.co.nz/api/v1/chat/completions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4",
    "messages": [
      {"role": "user", "content": "Hello!"}
    ],
    "temperature": 0.7,
    "stream": false
  }'
```

**See:** [Chat API](ChatAPI.md)

---

### 3. Workspaces (`/api/v1/workspaces`)

Manage workspaces and conversations.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/workspaces` | List all workspaces |
| GET | `/workspaces/{id}` | Get workspace by ID |
| POST | `/workspaces` | Create workspace |
| PUT | `/workspaces/{id}` | Update workspace |
| DELETE | `/workspaces/{id}` | Delete workspace |
| GET | `/workspaces/{id}/conversations` | Get conversations in workspace |
| POST | `/workspaces/{id}/conversations` | Create conversation |

**See:** [Workspaces API](WorkspacesAPI.md)

---

### 4. Knowledge Base (`/api/v1/knowledge`)

CRUD operations and semantic search for knowledge items.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/knowledge` | List knowledge items |
| GET | `/knowledge/{id}` | Get knowledge item by ID |
| POST | `/knowledge` | Create knowledge item |
| PUT | `/knowledge/{id}` | Update knowledge item |
| DELETE | `/knowledge/{id}` | Delete knowledge item |
| GET | `/knowledge/analytics` | Get usage analytics |

**Example:**
```bash
# Create knowledge item
curl -X POST https://api.aimate.co.nz/api/v1/knowledge \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Company Refund Policy",
    "content": "Customers may return items within 30 days...",
    "type": "Document",
    "tags": ["policy", "customer-service"]
  }'
```

**See:** [Knowledge API](KnowledgeAPI.md)

---

### 5. Search (`/api/v1/search`)

Global search across conversations, messages, and knowledge.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/search` | Global search |
| GET | `/search/conversations` | Search conversations |
| GET | `/search/messages` | Search messages |
| GET | `/search/knowledge` | Full-text search knowledge |
| GET | `/search/knowledge/semantic` | Semantic search knowledge |

**Example:**
```bash
# Semantic search
curl -X GET "https://api.aimate.co.nz/api/v1/search/knowledge/semantic?query=refund+policy&limit=5" \
  -H "Authorization: Bearer $TOKEN"
```

**See:** [Search API](SearchAPI.md)

---

### 6. Files (`/api/v1/files`)

Upload, download, and manage files.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/files/upload` | Upload file |
| GET | `/files/{id}/download` | Download file |
| DELETE | `/files/{id}` | Delete file |
| GET | `/files` | List files in workspace |
| GET | `/files/{id}` | Get file info |

**Example:**
```bash
# Upload file
curl -X POST "https://api.aimate.co.nz/api/v1/files/upload?workspaceId=123" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@document.pdf"
```

**See:** [Files API](FilesAPI.md)

---

### 7. Feedback (`/api/v1/feedback`)

Submit and manage message feedback.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/feedback/messages/{messageId}` | Create/update feedback |
| GET | `/feedback/messages/{messageId}` | Get message feedback |
| DELETE | `/feedback/messages/{messageId}` | Delete feedback |
| GET | `/feedback/templates` | Get tag templates |
| GET | `/feedback/export` | Export feedback data |
| POST | `/feedback/bulk` | Bulk feedback import |

**See:** [Feedback API](FeedbackAPI.md)

---

### 8. Projects (`/api/v1/projects`)

Project management within workspaces.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/projects` | List all projects |
| GET | `/projects/{id}` | Get project by ID |
| POST | `/projects` | Create project |
| PUT | `/projects/{id}` | Update project |
| DELETE | `/projects/{id}` | Delete project |

---

### 9. Notes (`/api/v1/notes`)

Note-taking with tags and collections.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/notes` | List all notes |
| GET | `/notes/{id}` | Get note by ID |
| POST | `/notes` | Create note |
| PUT | `/notes/{id}` | Update note |
| DELETE | `/notes/{id}` | Delete note |
| GET | `/notes/tags` | Get all tags |

---

### 10. Settings (`/api/v1/settings`)

User settings management.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/settings` | Get user settings |
| PUT | `/settings` | Update user settings |

---

### 11. Connections (`/api/v1/connections`)

Manage provider connections (BYOK).

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/connections` | List connections |
| GET | `/connections/{id}` | Get connection by ID |
| POST | `/connections` | Create connection |
| PUT | `/connections/{id}` | Update connection |
| DELETE | `/connections/{id}` | Delete connection |
| POST | `/connections/{id}/test` | Test connection |
| GET | `/connections/limits` | Get tier limits |

---

### 12. Plugins (`/api/v1/plugins`)

Plugin management and execution.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/plugins` | List installed plugins |
| GET | `/plugins/{id}` | Get plugin details |
| POST | `/plugins/install` | Install plugin |
| DELETE | `/plugins/{id}` | Uninstall plugin |
| PUT | `/plugins/{id}/settings` | Update plugin settings |
| POST | `/plugins/{id}/enable` | Enable plugin |
| POST | `/plugins/{id}/disable` | Disable plugin |

---

### 13. Tools (`/api/v1/tools`)

MCP tool execution.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/tools` | List available tools |
| POST | `/tools/execute` | Execute a tool |
| GET | `/tools/{name}` | Get tool details |

---

### 14. Admin (`/api/v1/admin`)

Administrative operations (Admin tier only).

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/admin` | Get dashboard data |
| GET | `/admin/users` | Get all users |
| GET | `/admin/stats` | System statistics |
| GET | `/admin/logs` | System logs |
| POST | `/admin/api-keys` | Create API key |
| GET | `/admin/api-keys` | List all API keys |
| DELETE | `/admin/api-keys/{id}` | Revoke API key |
| GET | `/admin/models` | Get model configs |
| PUT | `/admin/models/{id}` | Update model config |
| GET | `/admin/mcp-servers` | Get MCP servers |
| POST | `/admin/mcp-servers/test` | Test MCP connection |
| GET | `/admin/litellm/status` | LiteLLM status |
| GET | `/admin/system/uptime` | System uptime |

**See:** [Admin API](AdminAPI.md)

---

## 📊 Rate Limits

API requests are rate-limited by user tier:

| Tier | Requests/Minute | Burst |
|------|-----------------|-------|
| **Free** | 60 | 10 |
| **BYOK** | 60 | 10 |
| **Developer** | 120 | 20 |
| **Admin** | 200 | 50 |

**Headers:**
```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1700000000
```

**429 Too Many Requests:**
```json
{
  "error": "Rate limit exceeded",
  "retryAfter": 30
}
```

**Best Practices:**
- Check `X-RateLimit-Remaining` header
- Implement exponential backoff for 429 errors
- Cache responses when possible
- Upgrade tier if consistently hitting limits

See: [Rate Limits](RateLimits.md)

---

## ❗ Error Handling

### HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| **200** | OK | Request successful |
| **201** | Created | Resource created |
| **204** | No Content | Delete successful |
| **400** | Bad Request | Invalid request parameters |
| **401** | Unauthorized | Missing or invalid token |
| **403** | Forbidden | Insufficient permissions |
| **404** | Not Found | Resource doesn't exist |
| **429** | Too Many Requests | Rate limit exceeded |
| **500** | Internal Server Error | Server error |
| **503** | Service Unavailable | Temporary outage |

### Error Response Format

```json
{
  "error": {
    "code": "INVALID_PARAMETER",
    "message": "The 'model' field is required",
    "details": {
      "field": "model",
      "reason": "missing_required_field"
    }
  }
}
```

See: [Error Handling](ErrorHandling.md)

---

## 🌊 Streaming Responses

For real-time chat, use Server-Sent Events (SSE):

```bash
curl -X POST https://api.aimate.co.nz/api/v1/chat/completions/stream \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4",
    "messages": [{"role": "user", "content": "Hello"}],
    "stream": true
  }'
```

**Response (SSE):**
```
data: {"choices":[{"delta":{"content":"Hello"}}]}

data: {"choices":[{"delta":{"content":"!"}}]}

data: {"choices":[{"delta":{"content":" How"}}]}

data: [DONE]
```

**Parsing SSE:**
```javascript
const eventSource = new EventSource('...');
eventSource.onmessage = (event) => {
  if (event.data === '[DONE]') {
    eventSource.close();
  } else {
    const data = JSON.parse(event.data);
    console.log(data.choices[0].delta.content);
  }
};
```

---

## 📦 SDKs & Libraries

### Official SDKs

**Python** (Coming Soon)
```python
from aimate import Client

client = Client(api_key='YOUR_TOKEN')
response = client.chat.completions.create(
    model='gpt-4',
    messages=[{'role': 'user', 'content': 'Hello!'}]
)
```

**TypeScript/JavaScript** (Coming Soon)
```typescript
import { AiMateClient } from '@aimate/sdk';

const client = new AiMateClient({ apiKey: 'YOUR_TOKEN' });
const response = await client.chat.completions.create({
  model: 'gpt-4',
  messages: [{ role: 'user', content: 'Hello!' }]
});
```

**C# / .NET** (Available Now)
```csharp
var client = new AiMateClient(apiKey: "YOUR_TOKEN");
var response = await client.Chat.CreateCompletionAsync(new {
    model = "gpt-4",
    messages = new[] {
        new { role = "user", content = "Hello!" }
    }
});
```

### Community Libraries

- **OpenAI SDK** - Use OpenAI's official SDK, just change base URL
- **LangChain** - Supported via OpenAI compatibility
- **LlamaIndex** - Supported via OpenAI compatibility

---

## 🔗 OpenAI Compatibility

aiMate's Chat API is **OpenAI-compatible**. Swap base URL:

**Before (OpenAI):**
```python
from openai import OpenAI
client = OpenAI(api_key='sk-...')
```

**After (aiMate):**
```python
from openai import OpenAI
client = OpenAI(
    api_key='YOUR_AIMATE_TOKEN',
    base_url='https://api.aimate.co.nz/api/v1/chat'
)
```

**Compatible Endpoints:**
- `/chat/completions` ✅
- `/chat/completions/stream` ✅ (SSE)
- `/models` ⚠️ (aiMate format differs slightly)

---

## 📚 Code Examples

See: [Code Examples](CodeExamples.md) for:

- Python examples (requests, OpenAI SDK)
- JavaScript/TypeScript examples (fetch, axios)
- C# examples (HttpClient, RestSharp)
- cURL examples
- Streaming examples
- Error handling patterns
- Pagination examples

---

## 🧪 Testing

### Postman Collection

Download: [aiMate.postman_collection.json](./postman/aiMate_collection.json)

**Includes:**
- Pre-configured requests for all endpoints
- Environment variables for token/base URL
- Example requests with sample data
- Tests for status codes and responses

### Swagger/OpenAPI

**Interactive Docs:** https://api.aimate.co.nz/swagger

**Features:**
- Try API calls directly in browser
- View request/response schemas
- Download OpenAPI spec (JSON/YAML)
- Generate client code

---

## 🔒 Security

### Best Practices

1. **Store Tokens Securely**
   - Use environment variables
   - Never commit to version control
   - Rotate regularly

2. **Use HTTPS**
   - Always use `https://` for production
   - `http://` only for local development

3. **Validate Inputs**
   - Sanitize user input before sending to API
   - Check file uploads for malicious content

4. **Handle Errors Gracefully**
   - Don't expose error details to end users
   - Log errors server-side

5. **Respect Rate Limits**
   - Implement backoff and retry logic
   - Cache responses when appropriate

See: [Security Best Practices](../SECURITY.md)

---

## 📖 Further Reading

### API Details

- [Authentication](Authentication.md) - JWT tokens and API keys
- [Chat API](ChatAPI.md) - OpenAI-compatible chat
- [Workspaces API](WorkspacesAPI.md) - Workspace and conversation management
- [Knowledge API](KnowledgeAPI.md) - Knowledge base CRUD and search
- [Search API](SearchAPI.md) - Global search endpoints
- [Files API](FilesAPI.md) - File upload and management
- [Feedback API](FeedbackAPI.md) - Rating and feedback system
- [Admin API](AdminAPI.md) - Administrative operations

### Supporting Docs

- [Rate Limits](RateLimits.md) - Request limits by tier
- [Error Handling](ErrorHandling.md) - Status codes and error formats
- [Code Examples](CodeExamples.md) - Sample code in multiple languages
- [Pagination](Pagination.md) - Handling large result sets
- [Webhooks](Webhooks.md) - Event notifications (coming soon)

---

## 🆘 Support

**API Issues?**
- Check [Swagger docs](https://api.aimate.co.nz/swagger) for endpoint details
- Review [Error Handling](ErrorHandling.md) for common errors
- Search [GitHub Discussions](https://github.com/ChoonForge/aiMate/discussions)

**Still stuck?**
- Email: api@aimate.co.nz
- GitHub Issues: Report bugs or request features

---

**Happy Coding!** 🚀

*The aiMate API makes AI integration simple, powerful, and developer-friendly.*
