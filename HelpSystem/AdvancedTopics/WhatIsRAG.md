# What is RAG (Retrieval-Augmented Generation)?

> **Tooltip Summary:** "RAG enhances AI with your documents by retrieving relevant context before generating responses. Powers aiMate's Knowledge Base search."

---

## 🎯 The Problem RAG Solves

### Standard LLM Limitations

**Problem 1: Knowledge Cutoff**
```
You: "What happened at our board meeting last week?"
AI: "I don't have access to your meeting notes."
```

**Problem 2: Hallucinations**
```
You: "Summarize our Q4 sales report."
AI: *Makes up numbers because it doesn't have the report*
```

**Problem 3: No Proprietary Data**
```
You: "What does our company policy say about remote work?"
AI: "I can only provide general information, not your specific policy."
```

### RAG Solution

**With RAG:**
```
You: "What happened at our board meeting last week?"

aiMate:
1. Searches your Knowledge Base for "board meeting"
2. Finds your meeting notes document
3. Retrieves relevant sections
4. Sends context + question to AI
5. AI: "Based on your meeting notes from Nov 15, the key decisions were..."
```

**Result:** ✅ Accurate, ✅ Specific to your data, ✅ No hallucinations

---

## 🔍 How RAG Works

### The RAG Pipeline

```
┌─────────────────────────────────────────────────┐
│ 1. USER QUESTION                                 │
│    "What's our refund policy?"                  │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│ 2. RETRIEVAL (Search Knowledge Base)            │
│    - Convert question to vector embedding       │
│    - Search for similar documents               │
│    - Find: "Customer Refund Policy v2.pdf"      │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│ 3. AUGMENTATION (Add Context to Prompt)         │
│    System: "Use these documents to answer:"     │
│    Context: [Refund policy text extracted]      │
│    Question: "What's our refund policy?"        │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│ 4. GENERATION (AI Creates Response)             │
│    AI reads policy → Generates answer           │
│    "Our refund policy allows returns within..."|
└─────────────────────────────────────────────────┘
```

### Step-by-Step Example

**Setup (One Time):**
1. You upload "Customer_Refund_Policy.pdf" to Knowledge Base
2. aiMate extracts text: "Customers may return items within 30 days..."
3. aiMate creates vector embedding (mathematical representation)
4. Embedding stored in database (pgvector)

**Query Time (Each Question):**
1. You ask: "What's our refund policy?"
2. aiMate converts question to vector embedding
3. Searches database for similar embeddings (semantic search)
4. Finds "Customer_Refund_Policy.pdf" (high similarity score)
5. Retrieves relevant text chunks
6. Sends to AI: `Context: [policy text] Question: [your question]`
7. AI generates accurate answer based on YOUR document

---

## 🧮 Vector Embeddings Explained

### What are Embeddings?

**Text as Numbers:**
```
"refund policy" → [0.42, -0.15, 0.89, ..., 0.33]  (1536 dimensions)
```

**Similar meaning = Similar vectors:**
```
"refund policy"    → [0.42, -0.15, 0.89, ...]
"return guidelines" → [0.41, -0.14, 0.87, ...]  ← Very close!
"pizza toppings"    → [0.91,  0.73, -0.22, ...] ← Very different!
```

### Semantic Search

**Keyword Search (Old Way):**
```
Search: "refund"
Finds: Documents with exact word "refund"
Misses: Documents that say "return" or "money back"
```

**Semantic Search (RAG Way):**
```
Search: "refund"
Finds:
  - "refund policy" ✅
  - "return guidelines" ✅ (similar meaning)
  - "money back guarantee" ✅ (similar concept)
  - "customer satisfaction" ✅ (related topic)
```

**Why it's better:**
- Understands meaning, not just keywords
- Finds relevant info even with different wording
- Handles synonyms, abbreviations, and related concepts

See: [Vector Embeddings Deep Dive](VectorEmbeddings.md)

---

## 💡 RAG vs Fine-Tuning

### Fine-Tuning

**What:** Re-train AI model on your data
**Process:** Months of training on powerful GPUs
**Cost:** $$$$ (thousands to millions)
**When to Update:** Need to retrain for new data
**Best For:** Permanent knowledge, style/tone changes

### RAG

**What:** Provide relevant docs at query time
**Process:** Upload docs, instant availability
**Cost:** $ (storage + retrieval compute)
**When to Update:** Add/remove docs anytime
**Best For:** Dynamic knowledge, frequent updates

### Comparison

| Aspect | Fine-Tuning | RAG |
|--------|-------------|-----|
| **Setup Time** | Weeks-Months | Minutes |
| **Cost** | $10K-$1M+ | $10-$100/month |
| **Update Data** | Retrain model | Upload new docs |
| **Accuracy** | Very high | High |
| **Transparency** | Black box | See sources |
| **Data Privacy** | Training data exposed | Docs stay private |

**aiMate uses RAG because:**
- ✅ Instant updates (upload docs anytime)
- ✅ Affordable (no GPU training needed)
- ✅ Transparent (see which docs were used)
- ✅ Private (your data never trains models)
- ✅ Flexible (add/remove knowledge easily)

---

## 🏗️ aiMate's RAG Implementation

### Architecture

```
┌─────────────────────────────────────────────────┐
│  KNOWLEDGE BASE (Your Documents)                │
│  - PDFs, Word docs, Web pages, Notes, Code     │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  TEXT EXTRACTION                                │
│  - Extract text from files                     │
│  - Clean and normalize                         │
│  - Split into chunks (500-1000 tokens)         │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  EMBEDDING GENERATION                           │
│  - Convert text chunks to vectors              │
│  - Use OpenAI text-embedding-3-small           │
│  - 1536 dimensions per vector                  │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  VECTOR STORAGE (pgvector)                      │
│  - PostgreSQL with pgvector extension          │
│  - Indexed for fast similarity search          │
│  - Stores: text chunk + vector + metadata      │
└─────────────────────────────────────────────────┘

         [QUERY TIME]

┌─────────────────────────────────────────────────┐
│  USER QUESTION                                  │
│  "What's our remote work policy?"               │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  QUERY EMBEDDING                                │
│  - Convert question to vector                  │
│  - Same embedding model as documents           │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  SIMILARITY SEARCH                              │
│  - Find top K most similar vectors             │
│  - Uses cosine similarity                      │
│  - Returns: chunks + similarity scores         │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  CONTEXT ASSEMBLY                               │
│  - Rank results by relevance                   │
│  - Take top 3-5 chunks                         │
│  - Format for AI prompt                        │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│  LLM GENERATION                                 │
│  - Send: context + question to AI              │
│  - AI generates answer using YOUR docs         │
│  - Include source citations                    │
└─────────────────────────────────────────────────┘
```

### Components

**1. Knowledge Base (Frontend)**
- Upload docs, add notes, save web pages
- Tag and organize content
- Manual search and browse

**2. Knowledge API (Backend)**
- CRUD operations for knowledge items
- File processing and text extraction
- Embedding generation (via OpenAI API)

**3. Vector Database (pgvector)**
- Stores embeddings in PostgreSQL
- Fast similarity search (nearest neighbors)
- Scales to millions of documents

**4. Search Service**
- Semantic search (vector similarity)
- Full-text search (PostgreSQL full-text)
- Hybrid search (combines both)

**5. Chat Integration**
- Automatic knowledge retrieval during chat
- Manual knowledge attachment
- Source citation in responses

---

## 📊 RAG Performance Tuning

### Chunk Size

**Too Small (100-200 tokens):**
```
❌ Loses context
❌ Fragments sentences
❌ More chunks to search (slower)
```

**Too Large (2000+ tokens):**
```
❌ Too much irrelevant info in each chunk
❌ Hits context window limit faster
❌ Less precise retrieval
```

**Just Right (500-1000 tokens):**
```
✅ Preserves context
✅ Complete thoughts/paragraphs
✅ Efficient search
```

**aiMate default:** 800 tokens with 100-token overlap

### Retrieval Count

**Top K Results:**

```
K=1:  Very narrow, might miss context
K=3:  Balanced (aiMate default)
K=5:  More context, but might add noise
K=10: Very broad, uses lots of tokens
```

**Trade-offs:**
- More results = better coverage, higher token cost
- Fewer results = cheaper, but might miss info

### Embedding Models

| Model | Dimensions | Speed | Quality | Cost |
|-------|------------|-------|---------|------|
| **text-embedding-3-small** | 1536 | Fast | Good | $ |
| **text-embedding-3-large** | 3072 | Medium | Better | $$ |
| **text-embedding-ada-002** | 1536 | Fast | Good | $ |

**aiMate uses:** text-embedding-3-small (best balance)

### Search Strategies

**1. Semantic Only (default)**
```
Convert query to embedding → Find similar vectors
```
- Best for: Conceptual questions
- Example: "How do I request time off?" matches "PTO policy"

**2. Full-Text Only**
```
PostgreSQL full-text search on keywords
```
- Best for: Exact term matching
- Example: "API-KEY-12345" needs exact match

**3. Hybrid (best)**
```
Combine semantic + full-text results
```
- Best for: Most queries
- Gives relevance from both approaches

**aiMate supports all three!**

---

## 🎯 RAG Use Cases

### 1. Customer Support

**Upload:**
- Product manuals
- FAQs
- Support tickets history
- Return policies

**Ask:**
- "How do I reset my password?"
- "What's covered under warranty?"
- "How do I process a return?"

**Benefit:** Instant, accurate answers from official docs

### 2. Internal Knowledge Base

**Upload:**
- HR policies
- Engineering docs
- Meeting notes
- Project requirements

**Ask:**
- "What's our vacation policy?"
- "How does the auth service work?"
- "What was decided in the last sprint planning?"

**Benefit:** Institutional knowledge accessible to all

### 3. Research & Analysis

**Upload:**
- Research papers
- Industry reports
- Market data
- Competitor analysis

**Ask:**
- "What are the key trends in AI?"
- "Compare our pricing to competitors"
- "Summarize findings from study XYZ"

**Benefit:** Synthesize insights from many documents

### 4. Code Documentation

**Upload:**
- API documentation
- Code comments (extracted)
- Architecture diagrams (as text)
- Changelogs

**Ask:**
- "How do I call the user authentication API?"
- "What changed in version 2.0?"
- "Explain the database schema"

**Benefit:** Docs always in sync with questions

### 5. Legal & Compliance

**Upload:**
- Contracts
- Legal policies
- Compliance requirements
- Regulatory documents

**Ask:**
- "What are our GDPR obligations?"
- "Summarize the NDA terms"
- "What's required for SOC2 compliance?"

**Benefit:** Quick reference without reading 100-page docs

---

## 🚧 RAG Limitations

### Current Challenges

**1. Chunking Artifacts**
```
Document: "...end of page 1. Page 2 starts with..."
Chunk boundary: [... end of page 1.] | [Page 2 starts...]
→ Context split mid-thought
```

**2. Multi-Hop Reasoning**
```
Doc A: "Alice is Bob's manager"
Doc B: "Bob manages the sales team"
Question: "Who manages the sales team's manager?"
→ Needs to connect Doc A + Doc B (hard for RAG)
```

**3. Contradiction Handling**
```
Doc 1 (old): "Policy is X"
Doc 2 (new): "Policy is now Y"
→ Might retrieve both, confuse AI
```

**4. No Understanding of Images**
```
PDF with charts, diagrams, tables
→ Text extraction misses visual info
```

**aiMate improvements:**
- Smart chunking (paragraph-aware)
- Timestamp-based ranking (prefer recent docs)
- OCR for scanned documents (planned)
- Multimodal embeddings for images (future)

---

## 💪 Best Practices

### DO ✅

**1. Organize with Tags**
```
Tag docs: "hr", "policy", "engineering", "2024"
Filter searches by tag for precision
```

**2. Use Clear Titles**
```
Good: "Remote Work Policy - Updated Nov 2024"
Bad: "Document (1).pdf"
```

**3. Keep Docs Updated**
```
Delete outdated versions
Mark current version clearly
Regular cleanup (quarterly)
```

**4. Test Your Knowledge Base**
```
Ask questions you expect it to answer
Verify citations are correct
Refine docs if AI gets confused
```

**5. Provide Context in Questions**
```
Good: "According to our Q4 report, what were sales in October?"
Bad: "What were sales?" (too vague)
```

### DON'T ❌

**1. Upload Duplicate Content**
```
Same doc in PDF + Word + text
→ Retrieves all copies, wastes tokens
```

**2. Upload Entire Books**
```
500-page reference book
→ Chunk explosion, poor retrieval
Better: Upload specific chapters as separate docs
```

**3. Use RAG for Simple Facts**
```
"What's 15% of 200?"
→ Doesn't need RAG, AI can calculate
```

**4. Expect Perfect Accuracy**
```
RAG improves accuracy but isn't 100%
Always verify critical information
```

**5. Upload Sensitive Data Carelessly**
```
Check your tier's data privacy settings
Use local models for highly sensitive info
```

---

## 📚 Learn More

### Related Topics

- [Vector Embeddings](VectorEmbeddings.md) - How semantic search works
- [Knowledge Base Guide](../UserGuide/KnowledgeBase.md) - Using the Knowledge Base
- [Knowledge Base Best Practices](KnowledgeBaseBestPractices.md) - Optimize your KB
- [What are LLMs?](WhatAreLLMs.md) - Understanding AI models

### External Resources

- [Vector Search Guide](https://www.pinecone.io/learn/vector-search/)
- [RAG Paper (Original)](https://arxiv.org/abs/2005.11401)
- [Embeddings Guide](https://platform.openai.com/docs/guides/embeddings)
- [pgvector Documentation](https://github.com/pgvector/pgvector)

---

## ❓ FAQ

**Q: Is RAG expensive?**
A: Moderate cost. Embedding generation (~$0.0001/1K tokens) + vector storage + retrieval compute. Much cheaper than fine-tuning!

**Q: How many documents can I upload?**
A: No hard limit. PostgreSQL + pgvector scales to millions. Practical limit: storage space and search speed.

**Q: Can I use RAG with local models?**
A: Yes! Use Ollama/LM Studio for chat + OpenAI for embeddings. Or fully local with models like BAAI/bge-small.

**Q: Does RAG work in other languages?**
A: Yes, embedding models support 100+ languages. Quality varies by language.

**Q: How do I know which docs were used?**
A: aiMate shows source citations in AI responses. Click to view full document.

**Q: Can I combine multiple knowledge bases?**
A: Yes, via collections. Search across all or filter by collection/tags.

**Q: What if my doc is too large?**
A: aiMate auto-chunks large docs. For very large (1000+ pages), consider splitting by chapter/section.

**Q: Does RAG slow down responses?**
A: Slightly (~500ms for retrieval). Worth it for accurate, grounded answers. Caching helps repeat queries.

---

**RAG is aiMate's secret weapon for accurate, up-to-date, personalized AI chat!** 🚀

**Next:** [Vector Embeddings Explained](VectorEmbeddings.md)
