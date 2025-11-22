# First Steps with aiMate

> **Tooltip Summary:** "Get started with aiMate in 10 minutes. Create your first workspace, chat with AI, and upload knowledge for smarter responses."

---

## 🎯 What You'll Learn

By the end of this guide (10-15 minutes), you'll:

1. ✅ Register an account and log in
2. ✅ Create your first workspace
3. ✅ Have your first AI conversation
4. ✅ Upload a document to the knowledge base
5. ✅ Ask questions about your documents
6. ✅ Understand the basics of workspaces and AI personalities

**Prerequisites:** aiMate installed and running (see [Installation Guide](Installation.md))

---

## 📝 Step 1: Register & Login (2 minutes)

### First Time Setup

1. **Open aiMate** in your browser:
   - Docker: http://localhost:5000
   - Local dev: https://localhost:5001

2. **Click "Register"** (if you don't have an account)

   ![Registration screen placeholder]

3. **Fill in your details:**
   ```
   Name: Your Name
   Email: you@example.com
   Password: (at least 16 characters, mix of upper/lower/numbers/symbols)
   ```

4. **Click "Create Account"**

   ✅ You should see: "Account created successfully"

5. **You're automatically logged in!**

### If You Already Have an Account

1. **Click "Login"**
2. **Enter your email and password**
3. **Click "Sign In"**

**Troubleshooting:**
- "Email already exists" → Use the Login form instead
- "Invalid password" → Check caps lock, password is case-sensitive
- Can't access? → Check [Troubleshooting](../Troubleshooting/AuthenticationErrors.md)

---

## 🏠 Step 2: Create Your First Workspace (2 minutes)

**What's a workspace?** Think of it as a project folder for related conversations. Each workspace has its own personality, settings, and context.

### Create a Workspace

1. **Look for the sidebar** on the left

   ![Sidebar with Workspaces placeholder]

2. **Click "+ New Workspace"** button

3. **Fill in the workspace details:**

   ```
   Name: My First Workspace

   Type: General
   (Other options: Work, Research, Creative)

   Personality: Kiwi Mate
   (Other options: Kiwi Professional, Kiwi Dev, Te Reo Māori,
    Mental Health Guardian, Standard)

   Description: Learning how to use aiMate
   ```

4. **Click "Create"**

   ✅ You should see: Your new workspace in the sidebar

5. **Click on your workspace** to open it

**What just happened?**
- You created a container for conversations
- Set the AI personality to "Kiwi Mate" (casual, friendly, uses NZ slang)
- You're now "inside" this workspace—all chats here will use this personality

**Pro Tip:** Create separate workspaces for different topics:
- "Work Projects" (Kiwi Professional personality)
- "Coding Help" (Kiwi Dev personality)
- "Personal" (Kiwi Mate personality)

---

## 💬 Step 3: Your First Conversation (3 minutes)

Now let's chat with AI!

### Start a Chat

1. **You should see a chat interface** with a text input at the bottom

   ![Chat interface placeholder]

2. **Type your first message:**
   ```
   Hello! Tell me about yourself and what you can help me with.
   ```

3. **Press Enter** (or click the Send button)

4. **Watch the response stream in real-time!**

   ✅ You should see: AI responding token-by-token

   The response will be in "Kiwi Mate" style—casual and friendly!

### Try Different Message Types

**Ask a question:**
```
What's the capital of New Zealand?
```

**Request code:**
```
Write a Python function to calculate fibonacci numbers
```

**Ask for an explanation:**
```
Explain photosynthesis in simple terms
```

### Explore Message Actions

Hover over any AI response to see action buttons:

- 📋 **Copy** - Copy response to clipboard
- 🔄 **Regenerate** - Get a different response to the same question
- ⭐ **Rate** - Give feedback (thumbs up/down or detailed rating)
- ✏️ **Edit** - Edit your message and get a new response
- 🗑️ **Delete** - Remove this message

**Try regenerating a response:**
1. Hover over the AI's response
2. Click the 🔄 Regenerate button
3. Watch it generate a different answer!

**Pro Tip:** Regenerating is great when:
- The answer wasn't quite what you wanted
- You want to see alternative approaches
- The response was too long/short

---

## 📚 Step 4: Upload Your First Document (3 minutes)

One of aiMate's superpowers is **RAG** (Retrieval-Augmented Generation)—the ability to answer questions about YOUR documents.

### Upload a Document

1. **Click "Knowledge Base"** in the sidebar (or press Ctrl+Shift+K)

   ![Knowledge Base button placeholder]

2. **Click "+ Add Knowledge"** button

3. **Fill in the details:**

   ```
   Title: aiMate Feature List

   Type: Document

   Content: [Paste some text or upload a file]
   ```

   **Sample content to paste:**
   ```
   aiMate Features:
   - Real-time chat with AI streaming
   - Multiple AI models (GPT-4, Claude, Llama, etc.)
   - Knowledge base with semantic search
   - Workspaces for organizing conversations
   - AI personalities (Kiwi Mate, Professional, Dev, Te Reo Māori)
   - File uploads and attachments
   - MCP tools (web search, file reading)
   - Feedback system for improving responses
   ```

4. **Add tags** (optional but recommended):
   ```
   Tags: features, documentation, getting-started
   ```

5. **Click "Save"**

   ✅ You should see: Your knowledge item in the list

**Upload a file instead:**
1. Click "Upload File" instead of typing content
2. Choose a PDF, Word doc, or text file (max 10MB)
3. aiMate will extract the text automatically

**What just happened?**
- Your document was saved to the knowledge base
- It was converted to **vector embeddings** (mathematical representation)
- Now it's searchable by meaning, not just keywords!

---

## 🔍 Step 5: Ask Questions About Your Document (2 minutes)

Now let's use RAG to answer questions based on YOUR document.

### Search Your Knowledge

1. **Go back to your chat** (click on "My First Workspace")

2. **Start a new conversation** (click "+ New Chat" or Ctrl+N)

3. **Ask a question about what you uploaded:**
   ```
   What AI models does aiMate support?
   ```

4. **Enable knowledge search** (if not automatic):
   - Look for "Attach Knowledge" button
   - Or enable "Auto-search knowledge" in settings

5. **Send the message**

   ✅ You should see: AI answers based on YOUR document, with citations!

**Example response:**
```
According to your knowledge base, aiMate supports multiple AI models including:
- GPT-4
- Claude
- Llama

[Source: aiMate Feature List]
```

**What just happened?**
- aiMate searched your knowledge base for relevant info
- Found the "aiMate Feature List" document
- Sent that context to the AI along with your question
- AI answered based on YOUR data (not hallucinating!)

### Try More Questions

```
What personalities are available in aiMate?
```

```
Does aiMate have a feedback system?
```

```
What is the maximum file upload size?
```

Each answer should reference your uploaded document!

**Pro Tip:** The more documents you upload, the smarter aiMate becomes about YOUR topics.

---

## 🎨 Step 6: Explore AI Personalities (2 minutes)

aiMate has different personalities for different contexts.

### Try Different Personalities

1. **Go to Workspace Settings**
   - Click the ⚙️ icon next to your workspace name
   - Or right-click workspace → "Settings"

2. **Change the personality:**
   - Try: **Kiwi Professional** (business-appropriate)
   - Or: **Kiwi Dev** (technical, concise, code-focused)
   - Or: **Te Reo Māori** (bilingual, cultural awareness)

3. **Save and go back to chat**

4. **Ask the same question again:**
   ```
   Hello! Tell me about yourself.
   ```

5. **Notice the difference?**
   - Kiwi Mate: "Kia ora! I'm your AI mate..."
   - Kiwi Professional: "Hello. I'm an AI assistant designed to..."
   - Kiwi Dev: "I'm a dev-focused AI. Stack?"

### Personality Quick Reference

| Personality | Best For | Tone |
|-------------|----------|------|
| **Kiwi Mate** | General use, casual chat | Friendly, uses slang |
| **Kiwi Professional** | Work, business | Professional but approachable |
| **Kiwi Dev** | Coding, technical | Concise, technical, code-first |
| **Te Reo Māori** | Learning, cultural topics | Bilingual, culturally aware |
| **Mental Health Guardian** | Wellbeing, support | Empathetic, resource-focused |
| **Standard** | Default, international | Neutral, generic AI |

**Create multiple workspaces with different personalities:**
- "Work" → Kiwi Professional
- "Coding Projects" → Kiwi Dev
- "Personal Learning" → Kiwi Mate

---

## 🚀 Step 7: Explore More Features (Optional)

You've mastered the basics! Here are more features to explore:

### Switch AI Models

1. **Look for the model selector** (usually top of chat)
2. **Click to see available models:**
   - GPT-4 (best quality, slower, more expensive)
   - GPT-3.5 Turbo (fast, cheap, good for simple tasks)
   - Claude 3 Opus (excellent for long context, nuanced reasoning)
   - Llama 3.3 (open source, fast)
   - And more...

3. **Try the same question with different models** to compare!

**Pro Tip:** Use cheaper/faster models (GPT-3.5, Llama) for drafts and brainstorming, then switch to GPT-4/Claude for final polish.

### Attach Files to Messages

1. **In the chat input, look for the 📎 attachment button**
2. **Click to attach:**
   - Files from your computer
   - Items from knowledge base
   - Recent files

3. **Send a message with an attachment:**
   ```
   Summarize this document for me
   [Attached: your file]
   ```

### Use MCP Tools

MCP tools extend AI capabilities with real-time data.

**Try web search:**
1. Enable "Web Search" tool (look for tools icon)
2. Ask:
   ```
   What's the weather in Auckland today?
   ```
3. AI will search the web and give current info!

**Available tools:**
- 🌐 Web Search - Current information from the internet
- 📄 File Reader - Read files from your system (with permission)
- 🔍 Knowledge Search - Automatic knowledge base search

### Rate Responses

Help improve aiMate by rating responses:

1. **Hover over any AI response**
2. **Click the ⭐ Rate button**
3. **Choose:**
   - Quick rating: 👍 (good) or 👎 (bad)
   - Detailed rating: 1-10 scale with tags and comments

4. **Your feedback helps:**
   - Improve aiMate's responses
   - Train better models
   - Prioritize features

---

## 🎓 What You've Learned

Congratulations! You now know how to:

- ✅ Register and log in to aiMate
- ✅ Create workspaces for organizing conversations
- ✅ Chat with AI using different personalities
- ✅ Upload documents to the knowledge base
- ✅ Ask questions about your documents (RAG)
- ✅ Switch between AI models
- ✅ Use basic message actions (copy, regenerate, rate)
- ✅ Explore MCP tools for real-time data

---

## 🔜 Next Steps

### Recommended Learning Path

1. **[Quick Reference](QuickReference.md)** - Learn keyboard shortcuts (5 min)
2. **[Chat Interface](../UserGuide/ChatInterface.md)** - Deep dive on chat features (15 min)
3. **[Knowledge Base](../UserGuide/KnowledgeBase.md)** - Master semantic search (15 min)
4. **[Workspaces](../UserGuide/Workspaces.md)** - Organize like a pro (10 min)
5. **[Settings](../UserGuide/Settings.md)** - Customize your experience (10 min)

### Advanced Topics (When Ready)

- **[What are LLMs?](../AdvancedTopics/WhatAreLLMs.md)** - Understand how AI works
- **[Context Windows](../AdvancedTopics/ContextWindows.md)** - Manage AI memory limits
- **[What is RAG?](../AdvancedTopics/WhatIsRAG.md)** - Deep dive on knowledge search
- **[Prompt Engineering](../AdvancedTopics/PromptEngineeringBasics.md)** - Write better prompts

---

## 💡 Pro Tips for New Users

### 1. Start Simple
Don't overwhelm yourself with all features at once. Master:
1. Chat
2. Workspaces
3. Knowledge base

Everything else builds on these three.

### 2. Organize from Day One
Create workspaces early:
- Prevents conversation clutter
- Each workspace has clean context
- Easier to find past chats

### 3. Upload Documents Liberally
The more you add to knowledge base:
- Smarter AI responses about YOUR topics
- No need to paste the same info repeatedly
- Build your personal AI research assistant

### 4. Use Keyboard Shortcuts
See [Quick Reference](QuickReference.md) for full list:
- `Ctrl+K` - Search everything
- `Ctrl+N` - New conversation
- `Ctrl+Enter` - Send message
- `Shift+Enter` - New line without sending

### 5. Provide Feedback
When AI gives a great (or terrible) response:
- Rate it! (⭐ button)
- This helps improve aiMate for everyone

### 6. Don't Fear Experimenting
- Can't break anything by trying features
- Conversations auto-save
- Can always delete mistakes
- Regenerate responses if you don't like them

---

## ❓ Common Questions

**Q: How many workspaces can I create?**
A: Unlimited! Create as many as you need for different topics/projects.

**Q: Can I share workspaces with others?**
A: Not in v1.0 (single-user mode). Sharing comes in v1.5+.

**Q: What's the difference between workspaces and conversations?**
A: Workspace = container (project folder). Conversation = individual chat within that workspace.

**Q: Do I need to select a model every time?**
A: No. Set a default model in Settings, then override per-conversation if needed.

**Q: Can I use aiMate offline?**
A: Not yet. Requires internet connection to reach AI models. Local models (Ollama) coming soon.

**Q: Is my data private?**
A: Yes! Self-hosted = your data stays on your server. BYOK users: data goes direct to your chosen AI provider.

**Q: What file types can I upload?**
A: PDF, Word (.docx), text (.txt), markdown (.md), code files. More formats coming.

**Q: How much does it cost?**
A: Free tier available. BYOK tier = bring your own API keys. See pricing page for details.

**Q: Can I delete my data?**
A: Yes! Delete conversations, workspaces, or entire account in Settings.

**Q: I found a bug! How do I report it?**
A: Click "Feedback" or email support@aimate.co.nz. We appreciate bug reports!

---

## 🆘 Need Help?

**Stuck on something?**
- Check [Troubleshooting Guide](../Troubleshooting/CommonIssues.md)
- Search the help docs (Ctrl+K in help system)
- Email: hello@aimate.co.nz
- GitHub: [Report an issue](https://github.com/ChoonForge/aiMate/issues)

**Want to learn more?**
- [Video Tutorials](link-when-available) - Coming soon!
- [User Guide](../UserGuide/) - Deep dives on all features
- [Advanced Topics](../AdvancedTopics/) - Understand how AI works

---

## 🎉 Welcome to aiMate!

You're all set! Start chatting, upload your documents, and explore.

Remember: aiMate is **your AI workspace**. The more you use it, the more useful it becomes.

**Happy chatting!** 🚀

---

**Next:** [Quick Reference - Keyboard Shortcuts](QuickReference.md)
