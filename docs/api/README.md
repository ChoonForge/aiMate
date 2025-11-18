# aiMate API Documentation

**Welcome to the aiMate API!**

This directory contains comprehensive documentation for integrating with aiMate programmatically.

---

## 📚 Documentation Index

### Core API Documentation

- **[REST_API.md](REST_API.md)** - Complete REST API reference
  - All endpoints documented
  - Request/response formats
  - Code examples in Python, JavaScript/TypeScript, C#, and cURL
  - Error handling and rate limiting
  - Authentication guide

### Coming Soon

- **WebSocket_API.md** - Real-time WebSocket/SignalR integration
- **SDK_Documentation.md** - Official SDK guides
- **Webhooks.md** - Webhook setup and payloads
- **Migration_Guide.md** - Migrating from OpenAI/Claude APIs

---

## 🚀 Quick Start

### 1. Get an API Key

**Via Web UI:**
1. Log in to aiMate
2. Navigate to Settings → API Keys
3. Click "Generate New Key"
4. Copy the key (shown only once!)

**Format:** `sk-aimate-{32-character-string}`

### 2. Make Your First Request

**cURL:**
```bash
curl -X POST https://your-domain.com/api/v1/chat/completions \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4",
    "messages": [{"role": "user", "content": "Hello!"}]
  }'
```

**Python:**
```python
import requests

response = requests.post(
    "https://your-domain.com/api/v1/chat/completions",
    headers={"Authorization": "Bearer YOUR_API_KEY"},
    json={
        "model": "gpt-4",
        "messages": [{"role": "user", "content": "Hello!"}]
    }
)

print(response.json())
```

**JavaScript:**
```javascript
const response = await fetch('https://your-domain.com/api/v1/chat/completions', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer YOUR_API_KEY',
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    model: 'gpt-4',
    messages: [{role: 'user', content: 'Hello!'}]
  })
});

const data = await response.json();
console.log(data);
```

### 3. Handle Responses

**Success (200):**
```json
{
  "id": "chatcmpl-123",
  "object": "chat.completion",
  "choices": [{
    "message": {
      "role": "assistant",
      "content": "Hello! How can I help you today?"
    }
  }],
  "usage": {
    "total_tokens": 25
  }
}
```

**Error (401):**
```json
{
  "error": {
    "message": "Invalid API key",
    "type": "authentication_error"
  }
}
```

---

## 🔑 Authentication

All API requests require a Bearer token (API key) in the Authorization header:

```http
Authorization: Bearer sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

**Security Best Practices:**
- ✅ Never commit API keys to git
- ✅ Use environment variables
- ✅ Rotate keys every 90 days
- ✅ Use different keys for dev/staging/production
- ✅ Revoke unused keys immediately

---

## 📊 Rate Limits

| Tier | Requests/Minute | Requests/Day |
|------|----------------|--------------|
| BYOK ($10/mo) | 60 | 10,000 |
| Developer ($30/mo) | 120 | 50,000 |

**Check remaining quota:**
```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1640000000
```

**Handle 429 errors:**
```python
if response.status_code == 429:
    retry_after = int(response.headers.get('Retry-After', 60))
    time.sleep(retry_after)
    # Retry request
```

---

## 🎯 Common Use Cases

### 1. Chat Application

Build a chat interface that uses aiMate for AI responses:

```typescript
async function sendMessage(message: string): Promise<string> {
  const response = await fetch('/api/v1/chat/completions', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${API_KEY}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      model: 'gpt-4',
      messages: [{ role: 'user', content: message }]
    })
  });

  const data = await response.json();
  return data.choices[0].message.content;
}
```

### 2. Streaming Responses

Get real-time token-by-token responses:

```javascript
async function streamResponse(message) {
  const response = await fetch('/api/v1/chat/completions/stream', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${API_KEY}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      model: 'gpt-4',
      messages: [{ role: 'user', content: message }],
      stream: true
    })
  });

  const reader = response.body.getReader();
  const decoder = new TextDecoder();

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    const chunk = decoder.decode(value);
    // Process each chunk
    console.log(chunk);
  }
}
```

### 3. Workspace Management

Organize conversations in workspaces:

```python
# Create workspace
workspace = requests.post(
    f"{BASE_URL}/workspaces",
    headers={"Authorization": f"Bearer {API_KEY}"},
    json={
        "name": "My Project",
        "description": "AI assistance for my project",
        "type": "Project",
        "personality": "KiwiDev"
    }
).json()

# Use workspace
workspace_id = workspace["id"]
```

### 4. Feedback Collection

Collect user feedback on AI responses:

```python
requests.post(
    f"{BASE_URL}/feedback/messages/{message_id}",
    headers={"Authorization": f"Bearer {API_KEY}"},
    json={
        "rating": 8,
        "textFeedback": "Great response!",
        "tags": [
            {"key": "quality", "value": "high", "sentiment": "positive"}
        ]
    }
)
```

### 5. Knowledge Base Search

Search your knowledge base semantically:

```python
results = requests.post(
    f"{BASE_URL}/knowledge/search",
    headers={"Authorization": f"Bearer {API_KEY}"},
    json={
        "query": "How do I configure workspaces?",
        "limit": 5,
        "threshold": 0.7
    }
).json()

for result in results["results"]:
    print(f"{result['title']} (score: {result['score']})")
```

---

## 🔧 Advanced Features

### OpenAI Compatibility

aiMate is **drop-in compatible** with OpenAI's API. Just change the base URL:

```python
# OpenAI
import openai
openai.api_base = "https://your-aimate-domain.com/api/v1"
openai.api_key = "sk-aimate-xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"

# Use exactly like OpenAI
response = openai.ChatCompletion.create(
    model="gpt-4",
    messages=[{"role": "user", "content": "Hello!"}]
)
```

### Multi-Model Support

Switch between models seamlessly:

```python
models = ["gpt-4", "claude-3-5-sonnet-20241022", "gemini-pro"]

for model in models:
    response = chat_completion(message, model=model)
    print(f"{model}: {response}")
```

### Personality Modes

Use different AI personalities:

```python
personalities = {
    "casual": "KiwiMate",
    "professional": "KiwiProfessional",
    "technical": "KiwiDev",
    "bilingual": "TeReoMaori",
    "supportive": "MentalHealthGuardian"
}

# Set workspace personality
workspace = create_workspace(
    name="Support Chat",
    personality=personalities["supportive"]
)
```

---

## 🐛 Debugging

### Enable Logging

**Python:**
```python
import logging

logging.basicConfig(level=logging.DEBUG)
```

**JavaScript:**
```javascript
const DEBUG = true;

if (DEBUG) {
  console.log('Request:', request);
  console.log('Response:', response);
}
```

### Common Issues

**401 Unauthorized:**
- Check API key format: `sk-aimate-{32-chars}`
- Verify key hasn't been revoked
- Ensure Authorization header is set

**429 Rate Limited:**
- Check `X-RateLimit-*` headers
- Implement exponential backoff
- Consider upgrading tier

**422 Validation Error:**
- Check required fields
- Verify data types
- Review parameter constraints

**500 Internal Error:**
- Check logs in aiMate admin panel
- Verify LiteLLM service is running
- Check database connectivity

---

## 📖 API Reference

**Full documentation:** [REST_API.md](REST_API.md)

**Endpoints:**
- `POST /api/v1/chat/completions` - Chat completion
- `POST /api/v1/chat/completions/stream` - Streaming chat
- `GET /api/v1/workspaces` - List workspaces
- `POST /api/v1/workspaces` - Create workspace
- `POST /api/v1/feedback/messages/{id}` - Submit feedback
- `POST /api/v1/knowledge/search` - Search knowledge base
- `GET /api/v1/tools` - List MCP tools
- `POST /api/v1/tools/execute` - Execute tool

---

## 🔐 Security

### API Key Security

**DO:**
- ✅ Store in environment variables
- ✅ Use secrets management (AWS Secrets Manager, etc.)
- ✅ Rotate regularly
- ✅ Use separate keys per environment
- ✅ Revoke immediately when compromised

**DON'T:**
- ❌ Hardcode in source code
- ❌ Commit to version control
- ❌ Share keys in chat/email
- ❌ Use same key across projects
- ❌ Expose in client-side code

### Request Security

**HTTPS Only:**
```python
# ✅ CORRECT
BASE_URL = "https://aimate.co.nz/api/v1"

# ❌ WRONG - Never use HTTP
BASE_URL = "http://aimate.co.nz/api/v1"
```

**Validate Responses:**
```python
if response.status_code == 200:
    data = response.json()
    # Validate data structure
    assert "choices" in data
    assert len(data["choices"]) > 0
else:
    raise Exception(f"API Error: {response.status_code}")
```

---

## 📈 Monitoring

### Track Usage

```python
def track_api_call(endpoint, status_code, duration):
    # Log to your monitoring system
    metrics.increment(f"api.calls.{endpoint}")
    metrics.histogram(f"api.duration.{endpoint}", duration)

    if status_code >= 400:
        metrics.increment(f"api.errors.{status_code}")
```

### Monitor Rate Limits

```python
def check_rate_limit(response):
    remaining = int(response.headers.get('X-RateLimit-Remaining', 0))
    limit = int(response.headers.get('X-RateLimit-Limit', 60))

    usage_percent = ((limit - remaining) / limit) * 100

    if usage_percent > 80:
        logger.warning(f"Rate limit at {usage_percent}%")
```

---

## 🆘 Support

**Need help?**

- 📖 **Full API Docs:** [REST_API.md](REST_API.md)
- 💬 **Discussions:** https://github.com/ChoonForge/aiMate/discussions
- 🐛 **Bug Reports:** https://github.com/ChoonForge/aiMate/issues
- 📧 **Email:** api@aimate.co.nz
- 🌐 **Website:** https://aimate.co.nz

**Report API issues with:**
- Request/response examples
- Error messages
- Status codes
- Timestamp of issue
- Your API key ID (not the key itself!)

---

## 📜 License

The aiMate API follows the same MIT license as the main project.

**Free to use commercially, no restrictions.**

---

## 🎯 Next Steps

1. **Read the full API reference:** [REST_API.md](REST_API.md)
2. **Generate your API key** in the web UI
3. **Try the examples** in your language of choice
4. **Join the community** for support and updates
5. **Build something awesome!** 🚀

---

**Built with ❤️ from New Zealand** 🇳🇿

*Making AI accessible to everyone, one API call at a time.*
