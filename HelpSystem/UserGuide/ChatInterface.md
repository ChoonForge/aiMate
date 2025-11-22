# Chat Interface Guide

> **Tooltip Summary:** "Chat with AI using streaming responses, markdown formatting, file attachments, and multiple models. Master message actions like regenerate, edit, and rate."

---

## 📖 Overview

The chat interface is the heart of aiMate. It's where you:
- Have conversations with AI
- Get real-time streaming responses
- Switch between models
- Attach files and knowledge
- Use MCP tools for enhanced capabilities
- Rate responses to improve quality

This guide covers everything about the chat interface.

---

## 💬 Chat Basics

### Starting a Conversation

**Three Ways to Start:**

1. **From Workspace**
   - Open a workspace in sidebar
   - Click "+ New Chat" button
   - Or press `Ctrl+N`

2. **From Global Search**
   - Press `Ctrl+K`
   - Type your question
   - Hit Enter (creates new chat with your question)

3. **Quick Chat**
   - Click "Quick Chat" in toolbar
   - Ask one-off question without saving

### The Chat Interface Layout

```
┌─────────────────────────────────────────────────┐
│  [Workspace Name] [Model Selector] [Tools] [⚙]  │ ← Header
├─────────────────────────────────────────────────┤
│                                                  │
│  User: Hello!                          [Actions] │
│                                                  │
│  AI: Hi there! How can I help you?     [Actions] │
│                                                  │
│  User: Tell me about...                [Actions] │
│                                                  │
│  AI: [Streaming response...]           [Actions] │
│                                                  │
│  ↓ Scroll for more messages ↓                   │
│                                                  │
├─────────────────────────────────────────────────┤
│  [📎 Attach] [🔧 Tools]                         │ ← Input Bar
│  [Type your message here...             ] [Send]│
│  💬 Message: 0 tokens | 🔢 Total: 1,241 tokens  │ ← Token Counter
└─────────────────────────────────────────────────┘
```

---

## ✍️ Sending Messages

### Basic Message Sending

1. **Type your message** in the input field at the bottom
2. **Press Enter** to send
3. **Watch the AI respond** in real-time (streaming!)

**Keyboard shortcuts:**
- `Enter` - Send message
- `Shift+Enter` - Add new line without sending
- `Ctrl+Enter` - Force send (even with Shift)

### Message Formatting

aiMate supports **Markdown** formatting:

| What You Type | What You Get |
|---------------|--------------|
| `**bold text**` | **bold text** |
| `*italic text*` | *italic text* |
| `` `inline code` `` | `inline code` |
| `[link text](url)` | [link text](url) |
| `# Heading` | Large heading |
| `## Subheading` | Smaller heading |
| `- List item` | • List item |
| `1. Numbered item` | 1. Numbered item |

**Code Blocks:**
````
```python
def hello():
    print("Hello, world!")
```
````

Renders as:
```python
def hello():
    print("Hello, world!")
```

**Supported languages:** Python, JavaScript, TypeScript, Java, C#, Go, Rust, SQL, HTML, CSS, Markdown, JSON, YAML, and 50+ more.

### Multi-Line Messages

**For longer messages:**
1. Press `Shift+Enter` to create new lines
2. Continue typing across multiple lines
3. Press `Enter` when done to send entire message

**Example:**
```
First line
Second line (used Shift+Enter)
Third line (used Shift+Enter)
[Now press Enter to send all three lines]
```

---

## 🎨 AI Responses

### Streaming Responses

**Real-time generation:**
- AI responds token-by-token (word-by-word)
- See response as it's being generated
- No waiting for entire response to complete
- More engaging, feels conversational

**Streaming indicators:**
- Blinking cursor at end of response
- "Generating..." status
- Token count updating in real-time

### Stop Generation

**If AI is going off-track:**
1. Click the **Stop** button (appears during generation)
2. Or press `Esc`
3. AI stops immediately
4. Keep what was generated so far

**When to use:**
- Response is too long
- AI misunderstood your question
- You got enough information already

### Response Quality

**What affects response quality:**

1. **Model Choice** (GPT-4 > GPT-3.5)
2. **Prompt Clarity** (specific > vague)
3. **Context** (more relevant context = better answers)
4. **Temperature** (lower = more focused, higher = more creative)

See: [Prompt Engineering Basics](../AdvancedTopics/PromptEngineeringBasics.md)

---

## 🔄 Message Actions

Hover over any message to see action buttons:

### Copy Message

**📋 Copy Button**
- Copies message text to clipboard
- Preserves formatting
- Use: Share AI responses elsewhere

**Keyboard:** `Ctrl+C` with message selected

### Regenerate Response

**🔄 Regenerate Button**
- Ask AI to generate a different response
- Same question, different answer
- Use: When first answer wasn't helpful

**Keyboard:** `Ctrl+R` (regenerates last response)

**What happens:**
1. AI generates new response to same question
2. Old response is preserved (you can compare)
3. Can regenerate multiple times

**Pro Tip:** Regenerate 2-3 times to see different approaches, then pick the best one.

### Edit Message

**✏️ Edit Button**
- Edit your previous message
- AI generates new response to edited question
- Creates conversation "branch"

**Keyboard:** `Ctrl+E` or `Ctrl+↑` (edit last message)

**Example:**
```
Original: "Explain photosynthesis"
Edit to: "Explain photosynthesis in simple terms for a 10-year-old"
→ Get more appropriate response
```

### Delete Message

**🗑️ Delete Button**
- Permanently remove message from conversation
- Deletes your message AND AI's response (if applicable)
- Cannot be undone

**Keyboard:** `Ctrl+D` with message selected

**Use cases:**
- Remove mistakes or typos
- Clean up test messages
- Remove sensitive information

**Warning:** Deleting messages affects conversation context. AI won't remember deleted content.

### Rate Response

**⭐ Rate Button**
- Provide feedback on AI response
- Help improve aiMate
- Your feedback is anonymized

**Two rating modes:**

**1. Quick Rating (Thumbs)**
- 👍 Good response
- 👎 Bad response
- Takes 1 second

**2. Detailed Rating**
- 1-10 scale
- Feedback tags (Accuracy, Helpfulness, Tone, etc.)
- Written comments
- Expected vs. Actual comparison

**Keyboard:** `F` (when hovering over message)

**Why rate?**
- Helps prioritize model improvements
- Identifies which models work best for which tasks
- Your feedback used for fine-tuning
- Makes aiMate better for everyone

See: [Feedback System Guide](Feedback.md)

---

## 🎯 Model Selection

### Choosing an AI Model

**Model Selector** (top of chat):
- Click to see available models
- Switch mid-conversation
- Each model has different strengths

### Available Models

| Model | Best For | Speed | Cost | Context |
|-------|----------|-------|------|---------|
| **GPT-4 Turbo** | Complex reasoning, best quality | Slow | $$$ | 128K |
| **GPT-4** | High quality, balanced | Medium | $$ | 8K-32K |
| **GPT-3.5 Turbo** | Quick tasks, drafts | Fast | $ | 16K |
| **Claude 3 Opus** | Long context, nuanced reasoning | Slow | $$$ | 200K |
| **Claude 3 Sonnet** | Balanced quality and speed | Medium | $$ | 200K |
| **Claude 3 Haiku** | Fast, cheap, good enough | Fast | $ | 200K |
| **Llama 3.3 70B** | Open source, balanced | Medium | $$ | 128K |
| **Llama 3.3 8B** | Fast, local-capable | Fast | $ | 128K |
| **Qwen 2.5 7B** | Instruction-following, structured | Fast | $ | 32K |

**Context** = How much text the model can "remember" at once (see [Context Windows](../AdvancedTopics/ContextWindows.md))

### Model Selection Strategy

**For Different Tasks:**

```
Complex reasoning (math, logic, analysis):
→ GPT-4 Turbo or Claude 3 Opus

Long documents (summarize 50-page PDF):
→ Claude 3 Opus (200K context)

Quick questions (simple facts, definitions):
→ GPT-3.5 Turbo or Llama 3.3 8B

Creative writing (stories, brainstorming):
→ GPT-4 (higher temperature)

Code generation (write functions):
→ GPT-4 Turbo or Qwen 2.5

Cost-sensitive (lots of messages):
→ GPT-3.5 Turbo or Claude 3 Haiku

Privacy-sensitive (keep data local):
→ Llama 3.3 (can run locally with Ollama)
```

### Switching Models Mid-Conversation

**You can change models anytime:**

1. Click model selector
2. Choose different model
3. Continue conversation
4. **Previous messages stay**, new messages use new model

**Use case:**
```
1. Use GPT-3.5 for brainstorming (fast, cheap)
2. Switch to GPT-4 for final polished version
3. Save money while maintaining quality
```

### Model Settings

**Click ⚙️ next to model name** to adjust:

- **Temperature** (0.0-2.0)
  - 0.0 = Very focused, deterministic
  - 0.7 = Balanced (default)
  - 1.5+ = Very creative, random

- **Max Tokens** (response length limit)
  - 100 = Short answer
  - 500 = Medium (default)
  - 2000 = Long, detailed response

- **Top P** (nucleus sampling, advanced)
- **Frequency/Presence Penalty** (reduce repetition, advanced)

See: [Temperature & Sampling](../AdvancedTopics/TemperatureAndSampling.md)

---

## 📎 File Attachments

### Attaching Files to Messages

**Three ways:**

1. **Drag & Drop**
   - Drag file from computer
   - Drop into chat input area
   - Auto-attaches and sends

2. **Click Attach Button**
   - Click 📎 button in input bar
   - Browse for file
   - Select and send

3. **Paste**
   - Copy file in file explorer
   - Paste (`Ctrl+V`) in chat
   - Confirms and attaches

### Supported File Types

**Documents:**
- PDF (.pdf)
- Microsoft Word (.docx)
- Plain text (.txt)
- Markdown (.md)
- CSV/Excel (.csv, .xlsx)

**Code:**
- Python (.py)
- JavaScript (.js, .ts)
- C# (.cs)
- Java (.java)
- And all other text-based code files

**Images:** (Coming soon)
- JPEG, PNG, GIF
- For vision-capable models (GPT-4V, Claude 3)

**File Size Limit:** 10MB per file

### Using Attachments

**Example prompts:**

```
"Summarize this document"
[Attached: research_paper.pdf]
```

```
"Review this code for bugs"
[Attached: app.py]
```

```
"What are the key points from this meeting?"
[Attached: meeting_notes.docx]
```

**AI will:**
1. Extract text from file
2. Include file content in context
3. Answer based on file content
4. Cite specific sections if relevant

### Attach from Knowledge Base

**Instead of uploading same file repeatedly:**

1. Upload to Knowledge Base once
2. Click "Attach Knowledge" in chat
3. Select item(s) from knowledge base
4. Send message

**Benefits:**
- Don't re-upload same files
- Knowledge Base has semantic search (better retrieval)
- Tagged and organized

See: [Knowledge Base Guide](KnowledgeBase.md)

---

## 🔧 MCP Tools

MCP (Model Context Protocol) tools extend AI capabilities with real-time data and actions.

### Available Tools

**Built-in Tools:**

1. **🌐 Web Search**
   - Search the internet for current information
   - Use when AI's knowledge is outdated
   - Example: "What's the weather in Auckland today?"

2. **📄 File Reader**
   - Read files from your system (with permission)
   - Useful for large codebases
   - Example: "Read config.json and explain the settings"

3. **🔍 Knowledge Search**
   - Automatically search your knowledge base
   - RAG (Retrieval-Augmented Generation)
   - Happens automatically when relevant

4. **🧮 Calculator** (Coming soon)
   - Precise calculations
   - Example: "What's 15.7% of $23,456.89?"

5. **💻 Code Interpreter** (Coming soon)
   - Execute Python code in sandbox
   - Example: "Plot this data as a chart"

### Enabling Tools

**Per-Conversation:**
1. Click 🔧 Tools button in header
2. Toggle tools on/off
3. Enabled tools show checkmark

**Or:** Auto-enable in Settings → Tools

### Tool Usage Examples

**Web Search:**
```
You: What's the current price of Bitcoin?

AI: [Uses web search tool]
    "As of November 22, 2025, Bitcoin is trading at $42,315.
     Source: coinmarketcap.com"
```

**Knowledge Search:**
```
You: What does our refund policy say about returns after 30 days?

AI: [Searches your knowledge base]
    "According to your 'Refund Policy v2' document, returns
     after 30 days are handled on a case-by-case basis..."
```

**Tool Indicators:**
- 🔧 Icon next to response when tool was used
- "Used: Web Search" note
- Click to see tool call details

### Tool Settings

**Configure per-tool:**
- Web Search: Choose search engine (Google, Bing, Brave)
- Knowledge Search: Set max results, relevance threshold
- File Reader: Set allowed directories (security)

See: [MCP Tools Guide](MCPTools.md)

---

## 💬 Conversation Management

### Conversation Actions

**Right-click conversation** (in sidebar) for:
- Rename
- Archive
- Delete
- Export (markdown, PDF)
- Share (coming soon)
- Pin to top

### Conversation Metadata

**View conversation details:**
- Total messages
- Total tokens used
- Models used
- Created/modified dates
- Attached files

**Click ℹ️ icon** in conversation header

### Conversation History

**Navigate through messages:**
- Scroll up/down
- Click message to jump to that point
- "Jump to latest" button (when scrolled up)

**Load older messages:**
- Pull down at top of chat
- Or click "Load More Messages"

### Archiving Conversations

**When a conversation is finished:**
1. Right-click conversation → Archive
2. Or press `Ctrl+Shift+A`
3. Moved to "Archived" section

**Benefits:**
- Cleaner workspace
- Archived chats still searchable
- Can un-archive anytime

### Exporting Conversations

**Save conversations externally:**
1. Right-click conversation → Export
2. Choose format:
   - Markdown (.md)
   - PDF (.pdf)
   - JSON (raw data)
3. Download file

**Use cases:**
- Share with colleagues
- Archive important chats
- Include in reports
- Backup conversations

---

## 📊 Token Counter

**Bottom of chat input:**
```
💬 Message: 45 tokens | 🔢 Total: 3,241 / 32,000 tokens
```

### What Are Tokens?

**Tokens** = Pieces of text the AI processes

Roughly:
- 1 word ≈ 1.3 tokens
- 1 character ≈ 0.25 tokens

Examples:
- "Hello world" = 2 tokens
- "aiMate is great!" = 4 tokens

See: [Tokens & Pricing](../AdvancedTopics/TokensAndPricing.md)

### Why It Matters

**Two limits:**

1. **Context Window** (model's memory)
   - GPT-4: 32,000 tokens max
   - When exceeded, oldest messages dropped
   - Current total shown in counter

2. **API Costs** (if using BYOK)
   - Charged per token
   - Input tokens + output tokens
   - Monitor to control costs

### Token Counter Colors

- 🟢 **Green** (< 50% used) - Plenty of space
- 🟡 **Yellow** (50-80% used) - Monitor usage
- 🔴 **Red** (> 80% used) - Consider resetting conversation

### Managing Token Usage

**When approaching limit:**

1. **Start new conversation** (resets context)
2. **Summarize current chat** ("Summarize our discussion")
3. **Delete unnecessary messages**
4. **Switch to larger context model** (Claude 3 = 200K tokens)

See: [Context Windows](../AdvancedTopics/ContextWindows.md)

---

## 🎨 Personalities

aiMate has AI personalities that change tone and behavior.

### Available Personalities

**Set per-workspace** (not per-conversation):

1. **Kiwi Mate** (Default)
   - Casual, friendly
   - Uses NZ slang ("sweet as", "chur", "yeah nah")
   - Best for: Everyday chat

2. **Kiwi Professional**
   - Business-appropriate
   - Still Kiwi, but polished
   - Best for: Work emails, reports

3. **Kiwi Dev**
   - Technical, concise
   - Code-focused
   - Minimal chat, maximum code
   - Best for: Programming help

4. **Te Reo Māori**
   - Bilingual (English + Te Reo)
   - Cultural awareness
   - Best for: Learning Te Reo, cultural topics

5. **Mental Health Guardian**
   - Empathetic, supportive
   - Resource-focused
   - Safety-first approach
   - Best for: Wellbeing, crisis support

6. **Standard**
   - Generic AI assistant
   - Neutral tone
   - Best for: International users, formal contexts

### Changing Personality

**Per-Workspace:**
1. Right-click workspace → Settings
2. Select personality
3. Save
4. All new conversations use this personality

**Or:** Settings → General → Default Personality

### Personality Examples

**Same question, different personalities:**

**Q:** "How do I center a div in CSS?"

**Kiwi Mate:**
> "Sweet! Centering divs is easy as. Chuck this in your CSS: `display: flex; justify-content: center; align-items: center;` and you're sorted!"

**Kiwi Professional:**
> "The modern approach is using Flexbox. Add these properties to the parent container: `display: flex;`, `justify-content: center;`, and `align-items: center;`."

**Kiwi Dev:**
> "Flexbox. Parent: `display: flex; justify-content: center; align-items: center;` Done."

See: [AI Personalities Guide](Personalities.md)

---

## ⚙️ Chat Settings

**Click ⚙️ in chat header** for quick settings:

### Per-Conversation Settings

- Model selection
- Temperature
- Max tokens
- System prompt override
- Tool enablement

### Workspace Settings

- Default model
- Default personality
- Auto-save settings
- Context window management

### Global Chat Settings

**Settings → General → Chat:**

- Show timestamps
- Show token counter
- Enable markdown rendering
- Code theme (syntax highlighting)
- Font size
- Compact mode

---

## 🚀 Advanced Features

### System Prompt Override

**Customize AI behavior per-conversation:**

1. Settings → Advanced → System Prompt
2. Enter custom instructions
3. AI follows these instructions for this chat

**Example custom prompt:**
```
You are a senior software architect reviewing code.
Be extremely critical and focus on:
- Performance issues
- Security vulnerabilities
- Code smells
Always suggest specific improvements with code examples.
```

### Conversation Templates

**Save common conversation setups:**

1. Create conversation with desired settings
2. Right-click → "Save as Template"
3. Name it (e.g., "Code Review Template")
4. Create new chats from template

### Multi-Turn Editing

**Edit multiple messages:**
1. Enable "Edit Mode" (Settings)
2. Click edit on any message (not just last one)
3. Change creates conversation branch
4. Can view/switch between branches

### Conversation Branching

**Explore multiple conversation paths:**
```
Original conversation:
  User: Should I use React or Vue?
  AI: React has larger ecosystem...

Branch 1 (edit to ask about Vue):
  User: Should I use React or Vue? Focus on Vue.
  AI: Vue has simpler learning curve...

Branch 2 (edit to ask about Svelte):
  User: Should I use React or Vue? Also consider Svelte.
  AI: Svelte has best performance...
```

**View branches:**
- Click "Branches" in conversation header
- Switch between branches
- Compare different approaches

---

## 💡 Tips & Best Practices

### Writing Effective Prompts

**DO:**
- ✅ Be specific: "Write a Python function to sort a list" not "help with sorting"
- ✅ Provide context: "I'm a beginner learning Python"
- ✅ Give examples: "Like this: [example], but for [your case]"
- ✅ Set constraints: "In 3 bullet points" or "Under 100 words"
- ✅ Iterate: Start broad, then narrow with follow-ups

**DON'T:**
- ❌ Be vague: "Help me" (with what?)
- ❌ Assume context: AI doesn't remember past conversations (unless in same chat)
- ❌ Ask multiple unrelated questions at once
- ❌ Expect mind-reading: Explain what you need

See: [Prompt Engineering Basics](../AdvancedTopics/PromptEngineeringBasics.md)

### Optimizing for Speed

**Get faster responses:**
1. Use smaller models (GPT-3.5, Llama 8B)
2. Set lower max tokens (shorter responses)
3. Disable unnecessary tools
4. Keep conversations shorter (less context to process)

### Optimizing for Quality

**Get better responses:**
1. Use larger models (GPT-4, Claude Opus)
2. Provide more context
3. Include relevant files/knowledge
4. Be specific in your questions
5. Use follow-up questions to refine

### Saving Costs (BYOK Users)

**Reduce token usage:**
1. Use cheaper models for simple tasks
2. Reset conversations when switching topics
3. Delete unnecessary messages
4. Use Knowledge Base (searches only relevant chunks)
5. Monitor token counter

---

## 🆘 Troubleshooting

### Common Issues

**Messages won't send:**
- Check internet connection
- Verify LiteLLM is running (Admin Dashboard)
- Check if model is available
- Look for error message at top of chat

**Streaming stops mid-response:**
- Network timeout (refresh page)
- Model rate limit hit (wait 60 seconds)
- Server error (check Admin Dashboard → Logs)

**AI gives wrong answers:**
- Try regenerating
- Switch to better model (GPT-4)
- Provide more context
- Check if question is ambiguous
- Use web search tool for current info

**Attachments won't upload:**
- Check file size (< 10MB)
- Verify file type is supported
- Check disk space on server
- Try renaming file (remove special characters)

**Token counter at 100%:**
- Start new conversation
- Delete old messages
- Switch to model with larger context window

### Getting Help

**Still stuck?**
- Check [Common Issues](../Troubleshooting/CommonIssues.md)
- Check [Chat Problems](../Troubleshooting/ChatProblems.md)
- Submit feedback (click Feedback button)
- Email: support@aimate.co.nz

---

## 📚 Related Guides

**User Guides:**
- [Workspaces](Workspaces.md) - Organize conversations
- [Knowledge Base](KnowledgeBase.md) - Upload and search documents
- [MCP Tools](MCPTools.md) - Extend AI capabilities
- [Feedback System](Feedback.md) - Rate and improve responses
- [Settings](Settings.md) - Customize aiMate

**Advanced Topics:**
- [What are LLMs?](../AdvancedTopics/WhatAreLLMs.md) - How AI works
- [Context Windows](../AdvancedTopics/ContextWindows.md) - Managing memory
- [Prompt Engineering](../AdvancedTopics/PromptEngineeringBasics.md) - Better prompts
- [Temperature & Sampling](../AdvancedTopics/TemperatureAndSampling.md) - Control creativity

---

## 🎉 You're a Chat Master!

You now know:
- ✅ How to send messages and use markdown
- ✅ All message actions (copy, regenerate, edit, rate, delete)
- ✅ How to choose and switch models
- ✅ How to attach files and use tools
- ✅ How to manage conversations
- ✅ How to monitor and manage tokens
- ✅ Best practices for great AI conversations

**Go forth and chat!** 💬🚀

---

**Previous:** [Quick Reference](../GettingStarted/QuickReference.md) | **Next:** [Knowledge Base Guide](KnowledgeBase.md)
