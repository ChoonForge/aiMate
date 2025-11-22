# Tooltips Reference

> **Purpose:** Short 2-line summaries for UI tooltips and info icons throughout aiMate.
> **Usage:** Copy these directly into tooltip components, info icons, or help bubbles.

---

## 📋 How to Use This File

### For Developers

```html
<!-- Example: Tooltip component -->
<InfoIcon tooltip="Chat with AI using streaming responses and markdown formatting. Send messages, attach files, and get real-time answers." />
```

### For Designers

- Use these as placeholder text in mockups
- Character limit: ~140 characters (2 lines at ~70 chars each)
- Format: Direct, actionable, user-focused

### Organization

Tooltips are organized by:
1. Feature area (Chat, Knowledge Base, etc.)
2. UI component (Button, Setting, Field)
3. Alphabetically within each section

---

## 💬 Chat Interface

### Chat Input
**Message Input Field:**
"Type your message here and press Enter to send. Shift+Enter creates a new line without sending."

**Attach Files Button:**
"Attach documents, images, or code files to your message. Drag and drop also works. Max 10MB per file."

**MCP Tools Toggle:**
"Enable AI tools like web search, code execution, and file reading. Tools help AI access external information."

**Voice Input Button:**
"Click to use voice dictation. Speak your message and it will be transcribed automatically."

### Messages

**Copy Message Button:**
"Copy this message to your clipboard. Useful for sharing or saving AI responses elsewhere."

**Edit Message Button:**
"Edit your previous message and regenerate the AI's response. Creates a new conversation branch."

**Delete Message Button:**
"Permanently delete this message from the conversation. Cannot be undone."

**Regenerate Response:**
"Ask the AI to generate a different response to the same message. Useful if the first answer wasn't helpful."

**Rate Response (👍/👎):**
"Rate this response to help improve AI quality. Your feedback is anonymized and used for training."

**Detailed Feedback:**
"Provide detailed feedback on this response with tags and comments. Help us improve AI accuracy and usefulness."

### Streaming

**Streaming Indicator:**
"AI is generating a response in real-time. You'll see words appear as they're created by the model."

**Stop Generation:**
"Stop the AI from generating more text. Useful if the response is going off-track or is too long."

### Models

**Model Selector:**
"Choose which AI model to use for this conversation. Different models have different strengths and costs."

**Model Info Icon:**
"View details about this model: context window size, capabilities, cost per token, and best use cases."

**Switch Model:**
"Change AI model mid-conversation. Previous messages stay; new messages use the selected model."

---

## 🧠 Knowledge Base

### Knowledge Items

**Add Knowledge:**
"Upload documents, web pages, or notes to your knowledge base. AI can search and reference them in conversations."

**Knowledge Type:**
"Choose content type: Document (PDF/Word), Web Page (URL), Note (text), Code (source files), or File (any type)."

**Tags Field:**
"Add tags to organize and find knowledge items. Use commas to separate tags (e.g., project, research, important)."

**Content Field:**
"Enter or paste the content you want to store. Supports markdown formatting and code blocks."

**Semantic Search:**
"Search using natural language, not just keywords. AI finds relevant items based on meaning, not exact matches."

**Vector Embeddings:**
"Knowledge is converted to mathematical vectors for fast semantic search. Powered by pgvector database."

### Collections

**Create Collection:**
"Group related knowledge items into collections. Collections help organize large knowledge bases by topic or project."

**Collection Visibility:**
"Choose who can access this collection: Private (only you), Group (your team), or Organization (everyone)."

---

## 📁 Workspaces & Projects

### Workspaces

**Create Workspace:**
"Workspaces organize conversations by topic or project. Each workspace has its own context, personality, and settings."

**Workspace Type:**
"Choose workspace type: General (default), Project (task-focused), Research (long documents), or Creative (writing/ideas)."

**Workspace Personality:**
"Set the AI personality for this workspace. Choices: Kiwi Mate, Kiwi Professional, Kiwi Dev, Te Reo Māori, Mental Health Guardian, or Standard."

**Default Model:**
"Set the default AI model for conversations in this workspace. Can be overridden per conversation."

### Projects

**Create Project:**
"Projects organize work across multiple conversations and files. Set budgets, priorities, and track progress."

**Project Budget:**
"Set a token budget for this project to track AI usage costs. Warning shown when approaching limit."

**Project Priority:**
"Set project priority: Low, Normal, High, or Critical. Helps organize and filter projects by importance."

**Project Status:**
"Track project status: Planning, Active, On Hold, Completed, or Archived. Affects visibility and organization."

---

## 🔧 Settings

### General Settings

**Theme:**
"Choose light or dark theme. Dark theme reduces eye strain in low-light environments."

**Language:**
"Set interface language. Currently supports English (NZ), Te Reo Māori, and English (US). More coming soon."

**Notifications:**
"Enable browser notifications for new messages, mentions, and system alerts. Requires browser permission."

**System Prompt:**
"Customize the default AI behavior with a system prompt. Acts as instructions the AI follows in all conversations."

### Model Settings

**Default Model:**
"Set the default AI model used in new conversations. Can be changed per conversation or workspace."

**Temperature:**
"Control AI creativity. Lower (0.0-0.3) = more focused, deterministic. Higher (0.7-1.0) = more creative, varied."

**Max Tokens:**
"Limit response length in tokens. 100 tokens ≈ 75 words. Higher values allow longer responses but cost more."

**Top P:**
"Nucleus sampling parameter (0.0-1.0). Controls response diversity. Default 1.0 works for most cases. Advanced setting."

**Frequency Penalty:**
"Reduce word repetition (-2.0 to 2.0). Higher values make AI avoid repeating tokens. 0 = no penalty (default)."

**Presence Penalty:**
"Encourage topic diversity (-2.0 to 2.0). Higher values make AI explore new topics. 0 = no penalty (default)."

### Connection Settings

**LiteLLM Base URL:**
"URL of your LiteLLM proxy server. Default: http://localhost:4000. Change if using remote or custom deployment."

**API Key:**
"API key for authentication. BYOK (Bring Your Own Key) users enter provider API keys here (OpenAI, Anthropic, etc.)."

**Test Connection:**
"Verify connection to AI provider. Tests authentication, endpoint availability, and lists available models."

**Provider Type:**
"Choose AI provider: OpenAI, Anthropic, Google, Ollama (local), LM Studio (local), or Custom endpoint."

### Interface Settings

**Show Timestamps:**
"Display timestamps on messages. Shows when each message was sent for conversation tracking."

**Code Theme:**
"Choose syntax highlighting theme for code blocks: GitHub (light), Monokai (dark), or VS Code themes."

**Font Size:**
"Adjust chat interface font size. Options: Small (12px), Medium (14px), Large (16px), Extra Large (18px)."

**Compact Mode:**
"Reduce spacing in the UI for more content on screen. Useful on smaller displays or for power users."

**Show Token Counter:**
"Display real-time token count as you type. Helps track context window usage and API costs."

---

## 🔌 MCP Tools & Plugins

### MCP Tools

**What are MCP Tools?:**
"Model Context Protocol tools extend AI capabilities: web search, code execution, file access, and custom functions."

**Enable Tool:**
"Allow AI to use this tool in conversations. Tool must be enabled before AI can call it automatically."

**Tool Settings:**
"Configure tool-specific settings like API keys, rate limits, or custom parameters. Varies by tool."

**Web Search Tool:**
"Let AI search the web for current information. Useful for facts, news, or research beyond AI's training data."

**Code Interpreter:**
"Allow AI to write and execute Python code in a sandbox. Great for calculations, data analysis, or visualizations."

**File Reader:**
"Let AI read files from your system (with permission). Useful for analyzing code, logs, or documents."

**Knowledge Search:**
"AI automatically searches your knowledge base for relevant context when answering questions. Powered by RAG."

### Plugins

**Install Plugin:**
"Add new functionality to aiMate with plugins. Plugins can add tools, UI components, or integrations."

**Plugin Settings:**
"Configure plugin-specific options. Each plugin has its own settings panel. Changes apply immediately."

**Enable/Disable:**
"Turn plugin on or off without uninstalling. Disabled plugins don't load but settings are preserved."

**Uninstall Plugin:**
"Permanently remove this plugin. Settings and data will be deleted. Cannot be undone."

**Plugin Visibility:**
"Control who can use this plugin: Private (only you), Group (your team), Organization (all users), or Public (everyone)."

---

## 👥 Admin Settings

### User Management

**User Tier:**
"Set user access level: Free (basic), BYOK (bring own keys), Developer (API access), or Admin (full control)."

**API Key Limit:**
"Maximum API keys this user can create. Free: 1, BYOK: 3, Developer: 10, Admin: Unlimited."

**Connection Limit:**
"Maximum provider connections (BYOK). Free: 0, BYOK: 3, Developer: 10, Admin: Unlimited."

**Usage Quota:**
"Monthly token limit for this user. Prevents runaway costs. Set to 0 for unlimited (admin only)."

### System Overview

**Total Users:**
"Number of registered users on this aiMate instance. Includes all tiers (Free, BYOK, Developer, Admin)."

**Active Conversations:**
"Number of conversations with activity in the last 7 days. Indicates platform engagement."

**Total Messages:**
"All-time message count across all users and conversations. Includes both user messages and AI responses."

**Token Usage (Today):**
"Tokens consumed today across all users and models. Useful for tracking costs and system load."

**LiteLLM Status:**
"Connection status to LiteLLM proxy. Green = healthy, Yellow = degraded, Red = down. Click to test."

**Database Size:**
"Current database storage usage. Includes conversations, knowledge items, files, and metadata. Monitor for growth."

### Model Configuration

**Add Model:**
"Register a new AI model with aiMate. Requires LiteLLM configuration to expose the model via API."

**Model Name:**
"Display name for this model in the UI. Use clear names like 'GPT-4 Turbo' or 'Claude 3 Opus'."

**Model ID:**
"LiteLLM model identifier. Must match config in litellm_config.yaml. Examples: 'gpt-4', 'claude-3-opus-20240229'."

**Context Window:**
"Maximum tokens this model can process. Examples: GPT-4=32K, Claude 3=200K. Affects how much context AI remembers."

**Capabilities:**
"Check features this model supports: Streaming, Function Calling, JSON Mode, Vision (images), or Code Execution."

**Cost Per Token:**
"Input and output token costs in USD. Used to estimate user spending. Check provider pricing pages for rates."

**Model Visibility:**
"Who can use this model: Everyone, Developer+, Admin only. Controls access based on user tier."

### MCP Server Configuration

**Add MCP Server:**
"Connect to an MCP server to enable tools. Requires server URL and optional authentication credentials."

**Server URL:**
"WebSocket or HTTP URL of the MCP server. Examples: ws://localhost:3000, https://mcp.example.com."

**Authentication:**
"MCP server auth method: None, API Key, Bearer Token, or OAuth. Depends on server configuration."

**Auto-Discover Tools:**
"Automatically detect and register tools from this MCP server. Tools appear in the tools list when enabled."

**Health Check:**
"Test connection to MCP server. Verifies URL, auth, and lists available tools. Run before enabling."

---

## 📊 Feedback & Analytics

### Message Feedback

**Rating (1-10):**
"Rate this response from 1 (poor) to 10 (excellent). Helps track AI quality and model performance over time."

**Feedback Tags:**
"Categorize feedback with tags: Accuracy, Helpfulness, Formatting, Tone, Relevance. Select all that apply."

**Text Feedback:**
"Provide detailed comments on what was good or bad. Helps developers understand context behind ratings."

**Expected vs Actual:**
"What did you expect vs what did you get? Structured feedback helps identify specific issues with responses."

### Feedback Analytics

**Average Rating:**
"Mean rating across all user feedback for this model or time period. Higher is better (target: 7+)."

**Feedback by Tag:**
"Distribution of feedback tags. Shows which categories (Accuracy, Tone, etc.) are strong or need improvement."

**Model Comparison:**
"Compare average ratings across different models. Helps identify which models perform best for your use case."

**Export Feedback:**
"Download feedback data as CSV or Excel. Useful for analysis, reporting, or fine-tuning datasets."

---

## 🔍 Search

### Global Search

**Search All (Ctrl+K):**
"Search across conversations, messages, files, and knowledge base. Use natural language or keywords."

**Search Filters:**
"Narrow results by type: Chats, Messages, Files, Knowledge, Notes. Or date range: Today, Week, Month, All Time."

**Semantic Search:**
"AI-powered search finds results by meaning, not just keywords. Example: 'budget meeting' finds 'financial planning discussion'."

**Result Preview:**
"See context around search matches. Click result to jump directly to that conversation, message, or knowledge item."

### Search Shortcuts

**Recent Files:**
"Quickly access recently uploaded files. Shown in attachment menu for easy re-attachment to new messages."

**Frequent Searches:**
"Your most common search queries, saved for one-click access. Clear history in settings if desired."

---

## 🛠️ Troubleshooting

### Error Messages

**Authentication Error:**
"Login failed or session expired. Try logging out and back in. If issue persists, reset password or contact admin."

**Rate Limit Exceeded:**
"Too many requests in a short time. Wait 60 seconds and try again. Upgrade tier for higher limits."

**Model Not Available:**
"Selected model isn't configured or provider is down. Try a different model or contact admin to add this model."

**File Upload Failed:**
"File upload error. Check file size (<10MB), type (supported formats), and internet connection. Retry upload."

**Knowledge Search Failed:**
"Semantic search error. Database may be down or pgvector not configured. Contact admin if issue persists."

### Performance

**Slow Response:**
"AI is taking longer than usual. Large models (70B+) or long context can slow responses. Try a smaller/faster model."

**Streaming Interrupted:**
"Real-time streaming stopped mid-response. Network issue or server timeout. Click regenerate to try again."

**High Token Usage:**
"Approaching context window limit. Reset conversation or start fresh to clear context and reduce token count."

---

## 📱 Mobile & Accessibility

### Mobile Tips

**Touch Gestures:**
"Swipe left on a message to see actions (copy, edit, delete). Long-press to select multiple messages."

**Responsive Layout:**
"UI adapts to screen size. Sidebar collapses on mobile. Tap menu icon (☰) to access navigation."

### Accessibility

**Screen Reader:**
"ARIA labels and semantic HTML ensure screen reader compatibility. Press Tab to navigate, Enter to activate."

**Keyboard Navigation:**
"Full keyboard support. Tab to move focus, Enter to activate, Escape to close. See Quick Reference for shortcuts."

**High Contrast:**
"Enable in OS settings or browser. aiMate respects system preferences for high contrast and reduced motion."

**Font Scaling:**
"Zoom in/out with Ctrl/Cmd + or -. Or adjust font size in Settings > Interface > Font Size."

---

## 🔐 Security & Privacy

### Data Privacy

**Data Storage:**
"All conversations and knowledge stored locally or on your aiMate instance. Not shared with third parties unless using cloud AI providers."

**API Keys:**
"API keys are encrypted in the database using .NET Data Protection API. Never logged or exposed in UI."

**BYOK (Bring Your Own Key):**
"Use your own API keys from OpenAI, Anthropic, etc. Your data goes directly to provider, not through aiMate platform."

**Local Models:**
"Use Ollama or LM Studio for fully local AI. No data leaves your machine. Perfect for sensitive or proprietary information."

### Account Security

**Password Strength:**
"Minimum 16 characters with mix of upper, lower, numbers, and symbols. Use a password manager for best security."

**Two-Factor Authentication:**
"Add extra security layer with 2FA. Requires authenticator app (Google Authenticator, Authy) to log in. Coming soon."

**Session Timeout:**
"Sessions expire after 24 hours of inactivity for security. Re-login required. Change in Settings > Security."

**API Key Rotation:**
"Regularly regenerate API keys for security. Old key is immediately invalidated. Update clients with new key."

---

## 📚 Learning & Help

### Getting Started

**First Steps:**
"New to aiMate? Start here for installation, first chat, and basic setup. Get productive in 10 minutes."

**Video Tutorials:**
"Watch step-by-step video guides on key features. New videos added weekly. Subscribe for updates."

**Keyboard Shortcuts:**
"Learn keyboard shortcuts to work faster. Press ? in app to see full list. Customize in Settings."

### Advanced Topics

**Understanding LLMs:**
"Learn how AI works under the hood. Understand tokens, context windows, temperature, and prompt engineering."

**What is RAG?:**
"Retrieval-Augmented Generation explained. How aiMate's Knowledge Base extends AI capabilities with your data."

**Prompt Engineering:**
"Write better prompts to get better responses. Learn techniques, examples, and best practices."

### Community

**GitHub Discussions:**
"Ask questions, share tips, and connect with other aiMate users. Monitored by developers."

**Bug Reports:**
"Found a bug? Report it on GitHub Issues with steps to reproduce. Include screenshots if possible."

**Feature Requests:**
"Have an idea? Submit feature requests on GitHub. Vote on existing requests to prioritize development."

---

## 🎓 Usage Tips

### Efficiency

**Workspaces for Organization:**
"Use separate workspaces for different projects or topics. Keeps context clean and relevant."

**Knowledge Base for Documents:**
"Upload large documents to Knowledge Base instead of pasting in chat. Saves tokens and enables semantic search."

**Shortcuts Save Time:**
"Master keyboard shortcuts. Ctrl+K (search), Ctrl+N (new chat), Ctrl+Enter (send) speed up workflow."

### Quality

**Clear Prompts:**
"Be specific and concise. Good: 'Summarize this in 3 bullet points.' Bad: 'Can you maybe look at this?'"

**Provide Context:**
"Include relevant background information in your message or attach files. AI can't read your mind!"

**Iterate on Responses:**
"If first response isn't perfect, ask follow-up questions or regenerate. AI learns what you want through conversation."

### Cost Optimization

**Choose Right Model:**
"Simple tasks? Use smaller/cheaper models (GPT-3.5, Llama 3.3 8B). Complex reasoning? Use GPT-4 or Claude 3 Opus."

**Watch Token Usage:**
"Monitor token counter. Long context = higher cost. Reset conversations when switching topics."

**Use Local Models:**
"Ollama and LM Studio offer free local inference. Slower but $0 cost. Great for drafts and simple tasks."

---

## 📝 Developer Tips

### API Usage

**Authentication:**
"Include JWT token in Authorization header: 'Bearer YOUR_TOKEN'. Get token from POST /api/v1/auth/login."

**Rate Limits:**
"60 req/min (Free), 120 req/min (Developer), 200 req/min (Admin). Headers show remaining quota. See docs for details."

**Streaming:**
"Use /api/v1/chat/completions/stream for real-time responses. Returns Server-Sent Events (SSE). Parse data: lines."

**Error Handling:**
"Check HTTP status codes. 401=auth, 403=permission, 429=rate limit, 500=server error. See API docs for details."

### Integration

**Webhooks:**
"Receive events (new message, feedback submitted) via webhooks. Configure in Settings > Integrations."

**Custom Tools (MCP):**
"Build custom tools using MCP protocol. Python/TypeScript SDKs available. See Advanced Topics > MCP for guide."

**Plugin Development:**
"Extend aiMate with plugins. C# plugins have full access to services, database, and UI. See Developer Guide."

---

## 🌟 Pro Tips

### Power User Tricks

**Conversation Branching:**
"Edit a previous message to create a new branch. Explore multiple approaches without losing original conversation."

**Model Comparison:**
"Open two chats with same prompt, different models. Compare responses side-by-side to find best model for task."

**Template Prompts:**
"Save common prompts as notes or Knowledge Base items. Copy/paste for consistent quality (e.g., code review template)."

**Feedback for Fine-Tuning:**
"Detailed feedback helps create fine-tuning datasets. Export your feedback data to train custom models later."

### Hidden Features

**Markdown Support:**
"Use markdown in messages: **bold**, *italic*, `code`, [links](url), lists, tables. Renders beautifully."

**Code Block Languages:**
"Specify language for syntax highlighting: ```python, ```javascript, ```sql. 50+ languages supported."

**Math Rendering:**
"Use LaTeX for math: $E=mc^2$ (inline) or $$...$$  (block). Great for equations, formulas, statistics."

**Mermaid Diagrams:**
"Create diagrams with mermaid syntax in code blocks: ```mermaid graph TD; A-->B. Renders as flowcharts, sequences, etc."

---

## 🔄 Updates & Versioning

**Version Info:**
"Current aiMate version, build date, and changelog. Click to see what's new in this release."

**Auto-Update:**
"Docker deployments auto-pull latest images. Local installs: git pull && dotnet build. Backup data first!"

**Release Notes:**
"Full changelog of features, fixes, and improvements. Subscribe to GitHub releases for email notifications."

**Breaking Changes:**
"Major version bumps (1.x → 2.x) may have breaking changes. Read migration guide before updating."

---

## 📧 Support Channels

**In-App Help (F1):**
"Context-sensitive help based on current screen. Quick answers without leaving the app."

**Documentation:**
"Comprehensive guides on all features, APIs, and advanced topics. Start here for self-service help."

**GitHub Discussions:**
"Community Q&A forum. Search existing discussions or start a new one. Developers monitor daily."

**Email Support:**
"hello@aimate.co.nz for account issues, billing questions, or private matters. Response within 48 hours."

**Emergency Support:**
"Critical production issues? Email emergency@aimate.co.nz (Admin/Enterprise only). 4-hour response SLA."

---

## ✅ Best Practices Summary

**For Best Results:**
"Use specific prompts, organize with workspaces, leverage Knowledge Base, monitor tokens, provide feedback."

**For Best Security:**
"Strong passwords, rotate API keys, use 2FA, review permissions regularly, keep software updated."

**For Best Performance:**
"Choose right model for task, reset conversations when needed, use local models when possible."

**For Best Cost:**
"Optimize prompts, use cheaper models for simple tasks, monitor usage dashboard, set budgets."

---

**Total Tooltips:** 150+
**Coverage:** All major features and settings
**Format:** 2 lines max (~140 characters)
**Last Updated:** November 22, 2025

---

## 🎨 Implementation Notes

### For UI Developers

**HTML Example:**
```html
<div class="relative group">
  <InfoIcon class="text-gray-400 hover:text-gray-600" />
  <div class="tooltip">
    Chat with AI using streaming responses and markdown formatting.
    Send messages, attach files, and get real-time answers.
  </div>
</div>
```

**React/Blazor Example:**
```jsx
<Tooltip content="Chat with AI using streaming responses and markdown formatting. Send messages, attach files, and get real-time answers.">
  <InfoIcon />
</Tooltip>
```

### Styling Guidelines

- **Font:** System default, 12-14px
- **Width:** Max 300px
- **Background:** Dark (#1a1a1a) with 95% opacity
- **Text Color:** White (#ffffff)
- **Border:** None or subtle 1px border
- **Shadow:** Medium drop shadow for depth
- **Padding:** 8-12px
- **Arrow:** Optional, pointing to trigger element

### Accessibility

- **ARIA:** `aria-describedby` linking tooltip to trigger
- **Keyboard:** Show on focus (Tab), hide on blur
- **Screen Readers:** Tooltip text announced automatically
- **Contrast:** Meet WCAG 2.1 AA (4.5:1 minimum)

---

**This file is the single source of truth for all tooltip text in aiMate.**
**Update this file when adding features, then sync UI components.**
