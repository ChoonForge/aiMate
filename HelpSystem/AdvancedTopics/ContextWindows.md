# Context Windows: Understanding AI Memory

> **Tooltip Summary:** "Context windows limit how much text an AI can 'remember' at once. Think of it as the AI's short-term memory capacity."

---

## 🧠 What is a Context Window?

A **context window** is the maximum amount of text an AI model can process at once. It's measured in **tokens** (roughly words + punctuation).

**Analogy:**
> Imagine reading a book but you can only remember the last 10 pages. When you turn to page 11, you forget page 1. That's a context window.

---

## 📏 How Big are Context Windows?

| Model | Context Window | What It Holds |
|-------|----------------|---------------|
| **GPT-3.5** | 16K tokens | ~12,000 words (newspaper article) |
| **GPT-4** | 8K-32K tokens | ~6,000-24,000 words (short story) |
| **GPT-4 Turbo** | 128K tokens | ~96,000 words (short book) |
| **Claude 3** | 200K tokens | ~150,000 words (novel) |
| **Gemini 1.5 Pro** | 1M tokens | ~750,000 words (entire codebase!) |
| **Llama 3.3** | 128K tokens | ~96,000 words |
| **Qwen 2.5** | 32K-128K tokens | ~24,000-96,000 words |

**What's a token?**
- English: ~1 token = 0.75 words
- Code: ~1 token = 0.5 words (more symbols)
- Other languages: varies

---

## 🔢 Context Window Components

When you chat with AI in aiMate, the context window contains:

```
[System Prompt]           ← Personality instructions (500-1000 tokens)
[Knowledge Base Context]  ← Retrieved documents (0-4000 tokens)
[Conversation History]    ← Previous messages (varies)
[Current Message]         ← Your latest message (varies)
[Response Buffer]         ← Room for AI's response (varies)
```

### Example Calculation

GPT-4 (32K tokens):
```
System Prompt:           800 tokens
Knowledge (3 docs):    3,000 tokens
Conversation (10 msgs): 2,500 tokens
Current Message:         200 tokens
Response Buffer:       2,000 tokens
-----------------------------------
Total Used:            8,500 tokens
Remaining:            23,500 tokens
```

---

## ⚠️ What Happens When You Hit the Limit?

### Truncation Strategies

1. **Drop Oldest Messages** (default in aiMate)
   ```
   Message 1 (dropped)
   Message 2 (dropped)
   Message 3 ← Context starts here
   Message 4
   Message 5
   Current Message
   ```

2. **Summarize History**
   ```
   [Summary of messages 1-5]
   Message 6 (full)
   Message 7 (full)
   Current Message
   ```

3. **Sliding Window**
   ```
   Always keep:
   - System prompt
   - Last N messages
   - Current message
   ```

### aiMate's Approach

aiMate uses **intelligent context management**:

1. **Always Keeps:**
   - System prompt (personality)
   - Current message
   - Last 5 messages minimum

2. **Smart Truncation:**
   - Drop oldest messages first
   - Preserve recent context
   - Keep knowledge base context (if recent)

3. **User Control:**
   - See token count in real-time
   - Manual context reset (start fresh)
   - Workspace isolation (separate contexts)

---

## 💡 Why Context Windows Matter

### Use Case: Code Review

**Small Context (8K tokens):**
```
❌ Can't fit entire file
❌ AI can't see function definitions
❌ Misses important context
→ Poor suggestions
```

**Large Context (128K tokens):**
```
✅ Whole file + imports
✅ Related functions visible
✅ Full context available
→ Accurate review
```

### Use Case: Long Documents

**Small Context:**
```
❌ Document: 50,000 words
❌ Context: 12,000 words
→ Can only analyze ~1/4 of document
```

**Large Context:**
```
✅ Document: 50,000 words
✅ Context: 150,000 words
→ Analyze entire document + have room for responses
```

---

## 🎯 Strategies for Managing Context

### 1. Break Up Long Tasks

**Instead of:**
```
"Review this entire 100-page document and give me a summary."
```

**Do this:**
```
# Upload document to Knowledge Base
# Then:
"Search my knowledge for sales data and summarize."
→ aiMate retrieves only relevant sections (RAG)
```

See: [What is RAG?](WhatIsRAG.md)

### 2. Use Workspaces

**Bad:** One chat for everything
```
Chat 1: Work project + personal tasks + coding help
→ Context gets muddy, irrelevant history
```

**Good:** Separate workspaces
```
Workspace "ProjectX": Only ProjectX context
Workspace "Personal": Only personal context
Workspace "Coding": Only code context
→ Each has clean, focused context
```

See: [Workspaces](../UserGuide/Workspaces.md)

### 3. Reset When Needed

**When to reset context:**
- Switching topics completely
- AI seems confused by old messages
- Token count is too high
- Starting a new sub-task

**How to reset in aiMate:**
- Start a new conversation
- Use `/reset` command
- Create a new workspace

### 4. Optimize Your Input

**Inefficient (wastes tokens):**
```
"Hey, so I was thinking, you know, about that thing we discussed earlier,
the one with the code, you remember right? So anyway, I wanted to ask
if maybe you could help me with something similar..."
(60+ tokens for a simple question)
```

**Efficient:**
```
"Help me refactor this function to use async/await."
(10 tokens, clear intent)
```

### 5. Use Structured Content

**aiMate's structured content doesn't use context:**
```
Table with 100 rows in UI ← Rendered by frontend
AI only sends table spec   ← ~200 tokens
```

vs.

```
Markdown table with 100 rows ← 2000+ tokens in context!
```

---

## 📊 Monitoring Your Context

### In aiMate

**Token Counter (bottom of chat):**
```
💬 Message: 45 tokens | 🔢 Total: 3,241 / 32,000 tokens
```

- **Message:** Current input size
- **Total:** All context so far
- **Limit:** Model's max context

**Visual Indicator:**
- 🟢 Green: < 50% used (safe)
- 🟡 Yellow: 50-80% used (monitor)
- 🔴 Red: > 80% used (consider resetting)

### Tips

1. **Watch the counter** before long pastes
2. **Check after uploading** knowledge items
3. **Reset if yellow/red** and switching topics
4. **Use shorter prompts** when near limit

---

## 🚀 Future: Infinite Context?

### Current Limitations

- Context windows have hard limits
- Cost increases with context size
- Processing slows with more tokens

### Emerging Solutions

1. **Longer Windows**
   - Gemini 1.5: 1M tokens
   - Claude 3: 200K tokens
   - Trend: Doubling every year

2. **Better Compression**
   - Summarize old messages efficiently
   - Semantic compression (keep meaning, drop fluff)

3. **External Memory**
   - RAG (aiMate's Knowledge Base)
   - Vector databases (automatic retrieval)
   - Tool-based memory (MCP tools)

4. **Hybrid Approaches**
   - Short context for recent chat
   - Long-term memory in database
   - Retrieve on demand

**aiMate is ready for all of these!**

---

## 🧮 Context Math

### Estimating Token Usage

**Rules of Thumb:**
- 1 word ≈ 1.3 tokens (English)
- 1 character ≈ 0.25 tokens
- 1 code line ≈ 3-5 tokens

**Quick Estimates:**
- 1 page (single-spaced) ≈ 500 words ≈ 650 tokens
- 1 email ≈ 200 words ≈ 260 tokens
- 1 code file (200 lines) ≈ 600-1000 tokens

### Example: Chat Conversation

```
System Prompt:           800 tokens
Message 1 (user):         50 tokens
Message 2 (AI):          200 tokens
Message 3 (user):        100 tokens
Message 4 (AI):          300 tokens
Message 5 (user):         80 tokens
Message 6 (AI):          250 tokens
Message 7 (user, code):  400 tokens
-----------------------------------
Total:                 2,180 tokens

Remaining (GPT-4 32K): 29,820 tokens
```

Can still fit:
- ~40 more exchanges at this rate
- Or 1 large document (20K tokens)
- Or 100 small queries

---

## 🎓 Best Practices

### DO ✅

1. **Choose models based on need**
   - Simple task? Use 8K-16K models (cheaper)
   - Long documents? Use 128K+ models

2. **Leverage RAG**
   - Upload docs to Knowledge Base
   - Let aiMate retrieve only what's needed

3. **Use separate conversations**
   - One topic per conversation
   - Clean context, better results

4. **Monitor token usage**
   - Keep an eye on the counter
   - Reset before hitting limits

5. **Write concisely**
   - Clear, direct prompts
   - Less fluff = more room for context

### DON'T ❌

1. **Don't paste massive documents**
   - Upload to Knowledge Base instead
   - Use RAG for retrieval

2. **Don't reuse one conversation forever**
   - Old context pollutes new queries
   - Start fresh when switching topics

3. **Don't ignore warnings**
   - Red token counter = time to reset
   - Truncation can drop important context

4. **Don't use large models unnecessarily**
   - 128K context ≠ always better
   - Slower and costs more

5. **Don't forget model limits**
   - Check context size before switching models
   - GPT-3.5 (16K) vs GPT-4 (32K) vs Claude 3 (200K)

---

## 📚 Related Topics

- [What are LLMs?](WhatAreLLMs.md) - Understanding AI models
- [Tokens & Pricing](TokensAndPricing.md) - How usage is billed
- [What is RAG?](WhatIsRAG.md) - Extend context with external knowledge
- [Workspaces](../UserGuide/Workspaces.md) - Organize separate contexts
- [Knowledge Base](../UserGuide/KnowledgeBase.md) - Upload and search documents

---

## ❓ FAQ

**Q: What happens if I exceed the context window?**
A: aiMate automatically drops oldest messages. You'll see a warning in the UI.

**Q: Can I increase the context window?**
A: Choose a model with a larger window (e.g., Claude 3 at 200K instead of GPT-3.5 at 16K).

**Q: Does context cost more?**
A: Yes! More tokens = higher API costs. See [Tokens & Pricing](TokensAndPricing.md).

**Q: How do I see what's in my context?**
A: Click the "Token Counter" to see a breakdown of system prompt, history, and current context.

**Q: Does the Knowledge Base use context?**
A: Only the retrieved chunks (usually 2-4K tokens). The rest stays in the database.

**Q: Can I manually control what's in context?**
A: Not directly, but you can:
   - Reset conversations
   - Use separate workspaces
   - Clear conversation history

**Q: What's the largest context window available?**
A: Currently Gemini 1.5 Pro at 1 million tokens (~750K words). aiMate supports it!

---

**Understanding context windows helps you:**
- Choose the right model
- Structure conversations effectively
- Avoid truncation issues
- Optimize costs

**Master this and you'll master AI chat!** 🚀
