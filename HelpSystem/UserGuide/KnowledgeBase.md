# Knowledge Base Guide

> **Tooltip Summary:** "Upload documents, web pages, and notes to your knowledge base. AI searches them semantically and answers questions based on YOUR content, not hallucinations."

---

## 📖 What is the Knowledge Base?

The Knowledge Base is aiMate's **memory system** for your documents, notes, and information. It's what makes aiMate truly yours.

### Why It Matters

**Without Knowledge Base:**
```
You: "What does our refund policy say about exchanges?"
AI: "I don't have access to your refund policy.
     I can only provide general information..."
```
❌ Generic answer, possibly wrong

**With Knowledge Base:**
```
You: "What does our refund policy say about exchanges?"
AI: [Searches your knowledge base]
    "According to your 'Refund Policy v2' document uploaded on Nov 15,
     exchanges are accepted within 30 days with receipt.
     See Section 3.2 for details."
```
✅ Accurate, specific, cited

### How It Works (RAG)

**RAG** = Retrieval-Augmented Generation

```
Your Question
    ↓
Search Knowledge Base (semantic + full-text)
    ↓
Find Relevant Documents
    ↓
Send Documents + Question to AI
    ↓
AI Answers Based on YOUR Content
```

**The magic:** AI doesn't hallucinate because it has YOUR actual documents.

See: [What is RAG?](../AdvancedTopics/WhatIsRAG.md) for deep dive

---

## 📁 Content Types

Knowledge Base supports multiple content types:

### 1. Documents

**What:** PDFs, Word docs, text files
**Best for:** Reports, manuals, policies, research papers
**Upload:** Drag & drop or click Upload
**Max size:** 10MB per file

**Supported formats:**
- PDF (.pdf)
- Microsoft Word (.docx)
- Plain text (.txt)
- Markdown (.md)
- CSV (.csv)
- Code files (.py, .js, .cs, etc.)

### 2. Web Pages

**What:** Content from URLs
**Best for:** Articles, blog posts, documentation
**Upload:** Paste URL, aiMate fetches content
**Processing:** Automatically extracts main content (removes ads, navigation)

**Example:**
```
URL: https://example.com/article
→ aiMate fetches, extracts text, saves to knowledge base
```

### 3. Quick Notes

**What:** Short text snippets you type directly
**Best for:** Meeting notes, ideas, reminders, code snippets
**Upload:** Type directly in aiMate
**Size:** No limit, but keep under 5,000 words for best results

**Example:**
```
Title: Team Meeting Notes - Nov 22
Content:
- Q1 goals: Increase retention 15%
- New feature: Mobile app launch Feb 2026
- Budget approved: $50K for marketing
```

### 4. Code Files

**What:** Source code, config files, scripts
**Best for:** Documentation, code review, API references
**Upload:** Drag & drop or paste code
**Syntax:** Automatically detected and highlighted

**Supported languages:** 50+ (Python, JavaScript, C#, Java, Go, Rust, SQL, etc.)

---

## 📤 Uploading to Knowledge Base

### Method 1: Drag & Drop (Fastest)

1. **Open Knowledge Base** (Ctrl+Shift+K or click in sidebar)
2. **Drag files from your computer**
3. **Drop into Knowledge Base area**
4. **Files auto-upload and process**

✅ Upload multiple files at once!

### Method 2: Upload Button

1. **Click "+ Add Knowledge"** in Knowledge Base
2. **Click "Upload File"** tab
3. **Browse and select file(s)**
4. **Add title, tags (optional)**
5. **Click "Save"**

### Method 3: Paste URL

1. **Click "+ Add Knowledge"**
2. **Click "Web Page"** tab
3. **Paste URL:**
   ```
   https://example.com/article-to-save
   ```
4. **aiMate fetches content automatically**
5. **Review and save**

### Method 4: Type Directly (Quick Notes)

1. **Click "+ Add Knowledge"**
2. **Click "Quick Note"** tab
3. **Fill in:**
   ```
   Title: Meeting Notes
   Content: [Type your content here]
   Tags: meeting, team, q1-planning
   ```
4. **Click "Save"**

### What Happens After Upload?

1. **Text extraction** (for PDFs/Word docs)
2. **Chunking** (split into ~800 token pieces)
3. **Embedding generation** (convert to vectors for semantic search)
4. **Indexing** (add to search database)
5. **Ready to search!** (usually takes 5-30 seconds)

---

## 🔍 Searching Your Knowledge

### Semantic Search (Recommended)

**What:** Search by meaning, not just keywords

**How to use:**
1. Type your question naturally in search box
2. aiMate finds relevant content by meaning
3. Results ranked by relevance

**Examples:**

```
Search: "customer return process"
Finds:
- "Refund Policy" (contains "exchanges and returns")
- "Customer Service Guide" (has return workflow)
- "FAQ" (mentions returning items)
→ Even though exact phrase "customer return process" isn't in documents!
```

```
Search: "how to reset password"
Finds:
- "User Manual" (section on password reset)
- "Troubleshooting Guide" (password recovery steps)
- "FAQ" (login issues)
→ Understands intent, not just keyword matching
```

**Why it's better:**
- ✅ Finds content by concept
- ✅ Handles synonyms ("refund" finds "money back")
- ✅ Works with questions ("How do I...?")
- ✅ Understands context

### Full-Text Search

**What:** Search for exact words/phrases

**How to use:**
1. Click "Full-Text" tab in search
2. Type keywords
3. Get exact matches

**When to use:**
- Looking for specific term (API-KEY-12345)
- Exact quote or phrase
- Technical identifiers
- Names or codes

**Examples:**

```
Search: "ValueError"
→ Finds only documents with exact word "ValueError"
```

```
Search: "Article 3.2"
→ Finds documents mentioning this specific article
```

### Search Filters

**Narrow results:**

1. **By Type**
   - Documents only
   - Web pages only
   - Quick notes only
   - Code files only

2. **By Tags**
   - Select one or more tags
   - Show only tagged items

3. **By Date**
   - Today
   - This week
   - This month
   - Custom range

4. **By Workspace**
   - Current workspace only
   - Specific workspace
   - All workspaces

**Combine filters:**
```
Type: Document
Tags: policy, customer-service
Date: This month
→ Recent customer service policy documents
```

### Search Results

**Each result shows:**
- **Title** and excerpt (matching text)
- **Relevance score** (how well it matches)
- **Source** (which knowledge item)
- **Chunk location** (which part of document)
- **Preview** (click to expand)

**Actions on results:**
- 👁️ **View** - See full item
- 💬 **Ask AI** - Start chat about this item
- 📋 **Copy** - Copy excerpt
- 🔗 **Attach** - Add to current chat
- ✏️ **Edit** - Modify item
- 🗑️ **Delete** - Remove item

---

## 🏷️ Organizing with Tags

**Tags** are keywords that help organize and find knowledge items.

### Adding Tags

**When creating/editing:**
```
Tags: project-x, design, approved
```

**Separate with commas** or press Enter after each tag.

### Tag Best Practices

**DO:**
- ✅ Use consistent naming: `project-x` not `projectx` or `project x`
- ✅ Be specific: `q1-2026` better than `q1`
- ✅ Include category: `doc-policy`, `note-meeting`, `code-api`
- ✅ Add priority: `urgent`, `important`, `reference`
- ✅ Use hierarchical tags: `work/project-x/design`

**DON'T:**
- ❌ Over-tag (5-10 tags max per item)
- ❌ Use vague tags: `stuff`, `misc`, `notes`
- ❌ Duplicate information (title already says "policy")

### Tag Autocomplete

**aiMate suggests tags:**
- Based on content (auto-detected topics)
- From existing tags (consistency)
- From similar documents

Type to see suggestions, click to add.

### Filtering by Tags

**In Knowledge Base:**
1. Click "Tags" filter
2. Select one or more tags
3. See only tagged items

**In Search:**
```
#policy #customer-service
→ Items with both tags
```

### Managing Tags

**View all tags:**
- Knowledge Base → Tags tab
- See tag cloud (size = usage)
- Rename tags (updates all items)
- Merge tags (combine duplicates)
- Delete unused tags

---

## 💬 Using Knowledge in Chat

### Automatic Knowledge Search

**When enabled** (Settings → Tools → Knowledge Search):

1. You ask a question in chat
2. aiMate automatically searches knowledge base
3. Finds relevant items
4. Includes them in AI context
5. AI answers based on YOUR documents

**Example:**
```
You: What's our vacation policy?

[aiMate searches knowledge base]
[Finds: "HR Policies 2025.pdf"]

AI: According to your HR Policies (Section 5),
    employees receive 20 days annual leave plus 11 public holidays.
    See Section 5.3 for carryover rules.

Source: HR Policies 2025.pdf (uploaded Nov 1)
```

**No manual searching needed!**

### Manual Knowledge Attachment

**Attach specific items to chat:**

1. Click 📎 Attach in chat input
2. Select "From Knowledge Base"
3. Search or browse for item
4. Select item(s)
5. Send message

**When to use:**
- Want to reference specific document
- Multiple relevant documents to include
- Building context for complex question

### Citations

**AI responses include sources:**
```
According to your "Product Manual v2" document...
[Source: Product Manual v2.pdf, page 23]
```

**Click citation to:**
- Jump to source document
- See full context
- Verify information

### Knowledge Context Window

**How much knowledge fits in context?**

Typical chat context (32K tokens):
```
System prompt:        800 tokens
Conversation history: 2,500 tokens
Knowledge chunks:     4,000 tokens (3-5 documents)
Your message:         200 tokens
AI response buffer:   2,000 tokens
-----------------------------------
Total:                9,500 tokens
Remaining:            22,500 tokens
```

**Knowledge retrieval:**
- aiMate fetches top 3-5 most relevant chunks
- Each chunk ~800-1,000 tokens
- Total ~4,000 tokens of your content in context

**If you need more:**
1. Use model with larger context (Claude 3 = 200K tokens)
2. Upload document as file attachment (whole doc in context)
3. Ask multiple specific questions (one topic at a time)

See: [Context Windows](../AdvancedTopics/ContextWindows.md)

---

## 📊 Knowledge Base Management

### Viewing All Items

**Knowledge Base shows:**
- List view (default)
- Grid view (thumbnails)
- Table view (sortable columns)

**Sort by:**
- Relevance (after search)
- Date added (newest/oldest)
- Title (A-Z)
- Size (largest/smallest)
- Usage count (most/least referenced)

### Editing Knowledge Items

1. **Click item** to open
2. **Click Edit** button
3. **Modify:**
   - Title
   - Content (for notes; re-upload for files)
   - Tags
   - Type (convert note → document)
4. **Save changes**

**Editing triggers:**
- Re-chunking
- New embeddings
- Re-indexing

Takes 5-30 seconds to process.

### Deleting Items

**Single item:**
1. Right-click item → Delete
2. Or open item → Delete button
3. Confirm deletion

**Bulk delete:**
1. Select multiple items (Shift+Click)
2. Click Delete Selected
3. Confirm

**⚠️ Warning:** Deleted items cannot be recovered!

### Exporting Items

**Export formats:**
- Original file (PDF, .docx, etc.)
- Plain text (.txt)
- Markdown (.md)
- JSON (with metadata)

**How to export:**
1. Right-click item → Export
2. Choose format
3. Download file

**Bulk export:**
- Select multiple items
- Export → ZIP file with all items

### Duplicates Detection

**aiMate warns when:**
- Uploading same file twice (by hash)
- Very similar content detected
- Same URL already saved

**Options:**
- Skip duplicate
- Update existing item
- Save as new version

---

## 📈 Knowledge Analytics

**See how knowledge is used:**

### Usage Stats

**Per item:**
- Times referenced in chat
- Search result appearances
- Last accessed date
- Average relevance score

**View:** Click item → Analytics tab

### Search Analytics

**Track searches:**
- Most common queries
- Successful vs. unsuccessful searches
- Low-relevance results (need better docs?)
- Missing content gaps

**Use to:**
- Identify content gaps
- Improve tagging
- Optimize document structure

### Knowledge Graph (Coming Soon)

**Visualize connections:**
- Which documents relate to which
- Concept clusters
- Topic networks
- Citation graphs

---

## 🎯 Best Practices

### Document Organization

**Create a structure:**

```
Company Docs/
├── Policies/
│   ├── HR Policies 2025.pdf #policy #hr
│   ├── Code of Conduct.pdf #policy #compliance
│   └── Remote Work Policy.pdf #policy #hr #remote
├── Procedures/
│   ├── Onboarding Checklist.md #procedure #hr
│   ├── Deploy Process.md #procedure #dev
│   └── Incident Response.pdf #procedure #security
└── Reference/
    ├── API Documentation.md #reference #dev
    ├── Brand Guidelines.pdf #reference #marketing
    └── Troubleshooting FAQ.md #reference #support
```

**Use consistent tagging structure.**

### Naming Conventions

**Good titles:**
- ✅ "HR Policies 2025 - Employee Handbook"
- ✅ "API Reference - Authentication v2.0"
- ✅ "Meeting Notes - Q1 Planning - Nov 22"

**Bad titles:**
- ❌ "Document (1).pdf"
- ❌ "untitled-2"
- ❌ "stuff"

**Include:**
- Descriptive name
- Version (if applicable)
- Date (if time-sensitive)

### Keep Content Fresh

**Regular maintenance:**

1. **Monthly review:**
   - Delete outdated documents
   - Update changed policies
   - Add new content

2. **Version management:**
   - Keep only current version
   - Archive old versions (export first)
   - Tag with version: `v2.0`, `2025-edition`

3. **Mark stale content:**
   - Add tag: `needs-update`
   - Review quarterly

### Optimize for Search

**Make documents searchable:**

1. **Use clear headings**
   ```markdown
   # Refund Policy
   ## Eligibility
   ## Process
   ## Exceptions
   ```

2. **Include keywords naturally**
   - Don't keyword-stuff
   - Use terms users would search for
   - Define acronyms first time: "API (Application Programming Interface)"

3. **Add context**
   - Summary at top of document
   - Clear introduction
   - Conclusion with key points

4. **Break up long docs**
   - Large PDFs: Split by chapter/section
   - Better retrieval granularity
   - Faster processing

### Chunk-Friendly Content

**Knowledge is split into ~800 token chunks.**

**Optimize chunking:**

1. **Use clear section breaks**
   - Headers (`# Section Name`)
   - Page breaks
   - Visual separators

2. **Keep related info together**
   - Don't split tables across pages
   - Keep procedure steps in one section
   - Group related concepts

3. **Self-contained sections**
   - Each section should make sense alone
   - Include context ("As mentioned in Introduction...")
   - Repeat key terms

**Why:** AI might retrieve one chunk, not whole document. Make chunks useful standalone.

---

## 💡 Advanced Features

### Collections (Saved Filters)

**Group items dynamically:**

Instead of:
```
Manual collection: "Project X Docs"
→ Must manually add each doc
→ Doesn't update automatically
```

Use saved filters:
```
Saved filter: "Project X Docs"
Query: #project-x type:document
→ Automatically includes all matching items
→ Updates as you add new docs
```

**Create saved filter:**
1. Search with filters
2. Click "Save Filter"
3. Name it (e.g., "Project X Docs")
4. Access from sidebar like a collection

### Scheduled Updates (Web Pages)

**Keep web content fresh:**

1. Add web page to knowledge
2. Enable "Auto-update"
3. Set schedule (daily, weekly, monthly)
4. aiMate re-fetches and updates automatically

**Use for:**
- News sources
- Competitor websites
- Documentation that changes
- Status pages

### Collaborative Knowledge (Coming Soon)

**Share knowledge in organizations:**

- Team-shared knowledge bases
- Permission levels (read/edit/admin)
- Collaborative tagging
- Suggested additions from team

### Knowledge Import/Export

**Backup entire knowledge base:**

1. Knowledge Base → Settings
2. Export All → ZIP
3. Download

**Restore from backup:**

1. Knowledge Base → Settings
2. Import → Select ZIP
3. Choose merge or replace

**Migration between aiMate instances:**
- Export from old instance
- Import to new instance
- All tags, metadata preserved

---

## 🚀 Pro Tips

### 1. Upload Liberally

**More knowledge = smarter AI**

- Upload anything you reference regularly
- Better to have it and not use it than need it and not have it
- Storage is cheap, context is valuable

### 2. Tag Everything

**3 minutes tagging saves 30 minutes searching later**

- Tag as you upload (while context is fresh)
- Use autocomplete for consistency
- Include category, project, status tags

### 3. Quick Note Everything

**Meeting? Create a quick note:**
```
Title: Meeting - Product Roadmap - Nov 22
Content:
Attendees: John, Sarah, Mike
Decisions:
- Launch mobile app Feb 2026
- Focus on retention Q1
- Budget $50K marketing

Action items:
- Mike: Spec mobile app by Dec 1
- Sarah: Draft marketing plan by Dec 10
```

**Now AI knows this forever.**

### 4. Use Knowledge + Chat Combo

**Workflow:**
1. Upload all project docs to knowledge
2. Start chat: "Summarize the key risks in our project docs"
3. AI searches, finds all relevant docs, synthesizes

**Saves hours of reading/synthesizing manually.**

### 5. Verify with Citations

**Always check AI's sources:**
- Click citation links
- Verify context is correct
- If AI misunderstood, refine question

**Don't blindly trust—verify.**

### 6. Optimize Over Time

**Track what works:**
- Which searches succeed/fail?
- Which documents are never used? (delete them)
- Which topics need more content?
- Which tags are most useful?

**Iterate on your knowledge structure.**

---

## 🆘 Troubleshooting

### Upload Failed

**"File too large"**
- Max 10MB per file
- Split large PDFs by chapter
- Compress images in document

**"Unsupported format"**
- Check file extension (.pdf, .docx, .txt supported)
- Try converting to PDF
- Copy-paste content as quick note

**"Processing failed"**
- File might be corrupted
- Try re-downloading original
- Check file opens on your computer

### Search Returns No Results

**Check:**
1. Spelling (use semantic search, it's forgiving)
2. Filters (remove filters, search again)
3. Workspace filter (search all workspaces)
4. Content actually exists (verify item is uploaded)

**Try:**
- Different search terms (synonyms)
- Broader query ("customer" instead of "customer satisfaction")
- Full-text search (exact keywords)

### AI Doesn't Use Knowledge

**Verify:**
1. Knowledge Search tool enabled (Settings → Tools)
2. Items actually in knowledge base (check list)
3. Question is relevant to content
4. Items have been processed (wait 30 seconds after upload)

**Try:**
- Manually attach knowledge items
- Ask more specific question
- Reference document: "Based on our refund policy..."

### Slow Search

**If search takes >5 seconds:**
- Too many items (10,000+): Add filters first
- Complex semantic search: Try full-text
- Server overload: Check Admin Dashboard
- Large documents: Split into smaller chunks

### Wrong Results

**If search finds irrelevant items:**

**Short-term fix:**
- Use full-text search (more precise)
- Add more specific keywords
- Use filters (tags, date, type)

**Long-term fix:**
- Improve tagging (better categorization)
- Better document titles
- Remove outdated/duplicate content
- Better document structure (clear sections)

---

## 📚 Related Guides

**User Guides:**
- [Chat Interface](ChatInterface.md) - Use knowledge in chat
- [Search](Search.md) - Global search across everything
- [Settings](Settings.md) - Configure knowledge search

**Advanced Topics:**
- [What is RAG?](../AdvancedTopics/WhatIsRAG.md) - How knowledge search works
- [Vector Embeddings](../AdvancedTopics/VectorEmbeddings.md) - Semantic search explained
- [Knowledge Base Best Practices](../AdvancedTopics/KnowledgeBaseBestPractices.md) - Optimization guide

---

## 🎉 You're a Knowledge Master!

You now know:
- ✅ What the Knowledge Base is and why it matters
- ✅ How to upload documents, web pages, and notes
- ✅ Semantic vs. full-text search
- ✅ How to tag and organize effectively
- ✅ How to use knowledge in chat (RAG)
- ✅ Best practices for structure and naming
- ✅ Advanced features and pro tips
- ✅ Troubleshooting common issues

**Now go build your AI's memory!** 🧠💾

Upload your docs, tag them well, and watch AI give you accurate, cited answers based on YOUR content.

---

**Previous:** [Chat Interface](ChatInterface.md) | **Next:** [Workspaces Guide](Workspaces.md)
