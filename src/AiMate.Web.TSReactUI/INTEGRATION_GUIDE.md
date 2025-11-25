# Backend Integration Guide
**How to Use the Connected Backend**

## Quick Start

You now have **TWO versions** of the app:

### 1. Mock Mode (Current - `App.tsx`)
- Uses mock data and simulated API calls
- Works without backend running
- Great for UI development and testing

### 2. Real Backend Mode (`App_Connected.tsx`)
- Connects to actual .NET backend
- Real API calls with streaming chat
- Requires backend running on `localhost:5000`

## Switching to Real Backend

**Option A: Rename files (Quick Test)**
```bash
cd src/AiMate.Web.TSReactUI

# Backup current mock version
mv App.tsx App_Mock.tsx

# Use connected version
mv App_Connected.tsx App.tsx

# Start dev server
npm run dev
```

**Option B: Use environment variable (Recommended)**

Add to `.env.development`:
```bash
VITE_USE_REAL_BACKEND=true
```

Then update `main.tsx` to conditionally import:
```typescript
import App from './App';
// Or import from App_Connected when ready
```

## Prerequisites for Real Backend

### 1. Backend Must Be Running
```bash
cd src/AiMate.Web
dotnet run
```

Verify it's running: `http://localhost:5000/api/v1`

### 2. Authentication Setup

The connected app requires authentication. You have two options:

#### Option A: Bypass auth temporarily (Development)

Update `context/AuthContext.tsx`:
```typescript
// In AuthProvider, add mock user for development:
useEffect(() => {
  if (import.meta.env.VITE_AUTH_ENABLED === 'false') {
    setUser({
      id: 'dev-user-123',
      email: 'dev@example.com',
      name: 'Dev User',
      role: 'user',
      permissions: [],
    });
    setIsLoading(false);
    return;
  }
  // ... existing code
}, []);
```

Then in `.env.development`:
```bash
VITE_AUTH_ENABLED=false
```

#### Option B: Implement login (Production)

Create a login page and use the `useAuth` hook:
```typescript
const { login } = useAuth();
await login('user@example.com', 'password');
```

### 3. Create Default Workspace

The app needs at least one workspace. Either:

**A. Create via backend first**
- Use Swagger UI or Postman
- POST to `/api/v1/workspaces`

**B. Auto-create on first load** (Add to App_Connected.tsx):
```typescript
useEffect(() => {
  if (user && workspaces?.length === 0) {
    createWorkspace({
      name: 'Default Workspace',
      type: 'chat',
      userId: user.id,
    });
  }
}, [user, workspaces]);
```

## What Works in Connected Mode

✅ **Conversations**
- List all conversations from backend
- Create new conversation
- Delete conversation
- Rename conversation
- Select/switch conversations

✅ **Messages**
- Load conversation history from backend
- Display messages with correct timestamps
- Streaming chat responses (real-time)
- Auto-scroll to latest message

✅ **Chat Streaming**
- Server-Sent Events from backend
- Real-time token-by-token display
- Streaming indicators
- Error handling

✅ **Workspaces**
- Auto-select first workspace
- Create workspaces
- Switch workspaces (if multiple)

## What's Different in Connected Mode

### Data Flow

**Mock Mode:**
```
User Input → Local State → Simulated Delay → Mock Response → UI Update
```

**Connected Mode:**
```
User Input → API Call → Backend Processing → Streaming Response → UI Update
```

### Message Handling

**Mock Mode:**
- Messages stored in React state
- Persists only during session
- Simulated typing delay

**Connected Mode:**
- Messages stored in database
- Persists across sessions
- Real AI streaming responses
- Actual API latency

### State Management

**Mock Mode:**
- `useState` for everything
- Manual state updates

**Connected Mode:**
- React Query for server state
- Automatic caching
- Optimistic updates
- Background refetching

## Testing the Integration

### 1. Start Backend
```bash
cd src/AiMate.Web
dotnet run
```

Verify: `curl http://localhost:5000/api/v1/health` (if health endpoint exists)

### 2. Configure Frontend
```bash
cd src/AiMate.Web.TSReactUI

# Use connected version
mv App.tsx App_Mock.tsx
mv App_Connected.tsx App.tsx

# Ensure correct env
cat .env.development
# Should show: VITE_API_BASE_URL=http://localhost:5000
```

### 3. Start Frontend
```bash
npm run dev
```

Visit: `http://localhost:5173`

### 4. Check Debug Panel

Enable debug mode in the UI (keyboard shortcut or settings) to see:
- API calls being made
- Request/response data
- Streaming chunks
- Errors

### 5. Test Features

**Create Conversation:**
1. Click "+ New Chat"
2. Check backend logs for POST request
3. Verify conversation appears in sidebar

**Send Message:**
1. Type message and send
2. Watch streaming response appear token-by-token
3. Check backend logs for streaming endpoint

**Switch Conversations:**
1. Click different conversation in sidebar
2. Messages should load from backend
3. Check React Query cache in dev tools

## Troubleshooting

### "Authentication Required" Screen

**Cause:** No auth token or user not set

**Fix:**
```bash
# In .env.development
VITE_AUTH_ENABLED=false
```

Or implement login flow.

### "Loading..." Forever

**Cause:** Backend not responding or wrong URL

**Fix:**
1. Verify backend is running: `curl http://localhost:5000`
2. Check `.env.development` has correct URL
3. Check browser console for CORS errors

### "No active workspace"

**Cause:** No workspaces in database

**Fix:**
Create workspace via:
- Backend Swagger UI
- Or add auto-create logic (see above)

### Streaming Not Working

**Cause:** SSE endpoint not configured correctly

**Fix:**
1. Verify backend supports `/api/v1/chat/completions/stream`
2. Check Content-Type is `text/event-stream`
3. Ensure no proxy is buffering responses

### CORS Errors

**Cause:** Backend CORS not configured for frontend origin

**Fix:**
In backend `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Development Workflow

### Recommended Approach

1. **Start with Mock Mode**
   - Develop UI features
   - Test component interactions
   - No backend dependency

2. **Test Individual APIs**
   - Use connected mode
   - Test one feature at a time
   - Use React Query DevTools

3. **Full Integration**
   - Switch to connected mode
   - Test complete workflows
   - Monitor backend logs

4. **Production**
   - Update `.env.production`
   - Build for production
   - Deploy both frontend & backend

## Monitoring

### React Query DevTools

Add to App.tsx:
```typescript
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

// In AppWrapper:
<QueryClientProvider client={queryClient}>
  <App />
  <ReactQueryDevtools initialIsOpen={false} />
</QueryClientProvider>
```

### Backend Logging

Enable Serilog to see:
- API calls
- Request bodies
- Response times
- Errors

### Browser DevTools

- **Network tab**: See API calls, timing, responses
- **Console**: See debug logs from DebugContext
- **React DevTools**: Inspect component state

## Next Steps

1. ✅ Try mock mode (current `App.tsx`)
2. ✅ Start backend
3. ✅ Switch to `App_Connected.tsx`
4. ✅ Create test workspace
5. ✅ Test conversation creation
6. ✅ Test message streaming
7. ✅ Implement remaining features

## API Endpoints Being Used

Connected mode uses these backend endpoints:

```
GET    /api/v1/workspaces?userId={id}
POST   /api/v1/workspaces
GET    /api/v1/workspaces/{id}/conversations
POST   /api/v1/workspaces/{id}/conversations
DELETE /api/v1/conversations/{id}
PUT    /api/v1/conversations/{id}
GET    /api/v1/conversations/{id}/messages
POST   /api/v1/chat/completions/stream  (SSE)
```

Ensure your backend implements these endpoints!

## Performance Tips

1. **Caching**: React Query caches for 5 minutes
2. **Pagination**: Implement for large conversation lists
3. **Lazy Loading**: Messages load only when conversation selected
4. **Optimistic Updates**: UI updates before server confirms
5. **Error Boundaries**: Graceful error handling

## Support

If you encounter issues:

1. Check backend logs
2. Check browser console
3. Enable debug panel in UI
4. Verify endpoints with Swagger/Postman
5. Review integration plan in `BACKEND_INTEGRATION_PLAN.md`

Happy integrating! 🚀
