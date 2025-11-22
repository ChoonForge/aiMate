# What are Large Language Models (LLMs)?

> **Tooltip Summary:** "LLMs are AI systems trained on vast text data to understand and generate human-like language. They power aiMate's chat capabilities."

---

## 📖 Introduction

Large Language Models (LLMs) are the technology behind aiMate's AI chat capabilities. Understanding how they work helps you use aiMate more effectively.

---

## 🤖 What is an LLM?

An **Large Language Model** is an artificial intelligence system that:

- **Learns from massive text datasets** - Billions of words from books, websites, code, and conversations
- **Understands language patterns** - Grammar, context, facts, and reasoning
- **Generates human-like text** - Responses that feel natural and contextually appropriate
- **Performs many tasks** - Writing, analysis, coding, translation, and more

**Think of an LLM as:**
> A very smart autocomplete that doesn't just predict the next word, but understands meaning, context, and intent.

---

## 🧠 How Do LLMs Work?

### Training Process

1. **Pre-training** (The Learning Phase)
   - Model reads billions of text documents
   - Learns patterns: "After 'Hello' comes a greeting"
   - Learns facts: "Paris is the capital of France"
   - Learns reasoning: "If A > B and B > C, then A > C"
   - Takes weeks/months on massive computer clusters

2. **Fine-tuning** (The Specialization Phase)
   - Trained on specific tasks (chat, code, analysis)
   - Learns to follow instructions
   - Learns conversation style and safety guardrails
   - Takes days on smaller datasets

3. **Inference** (The Usage Phase)
   - You type a message
   - Model processes it and generates a response
   - Happens in seconds (the part you see in aiMate)

### The Neural Network

LLMs use **transformer neural networks**:

```
Your Input → Tokenization → Embeddings → Transformer Layers → Output Prediction
```

- **Tokenization:** "Hello world" → `["Hello", " world"]`
- **Embeddings:** Convert words to numbers (vectors)
- **Transformer Layers:** Process context and meaning (100+ layers deep)
- **Output:** Most likely next words

---

## 🔢 Key Concepts

### Parameters

**What:** The "knobs" the model adjusts during training
**Analogy:** Like neurons in a brain

| Model Size | Parameters | Capability |
|------------|------------|------------|
| Small | 7B-14B | Fast, good for simple tasks |
| Medium | 30B-70B | Balanced speed and quality |
| Large | 175B+ | Highest quality, slower |

**aiMate supports all sizes** - choose based on your needs.

### Tokens

**What:** Pieces of text the model processes
**Examples:**
- "Hello" = 1 token
- "world" = 1 token
- "ChatGPT" = 2 tokens ("Chat" + "GPT")

**Why it matters:**
- Models have token limits (context windows)
- API costs charged per token
- Affects how much context the AI can "remember"

See: [Context Windows](ContextWindows.md) | [Tokens & Pricing](TokensAndPricing.md)

### Context Window

**What:** How much text the model can "see" at once
**Analogy:** Short-term memory

Typical sizes:
- **4K tokens:** ~3,000 words (a few pages)
- **32K tokens:** ~24,000 words (a short book chapter)
- **128K tokens:** ~96,000 words (a novel)
- **1M tokens:** Entire codebases!

**In aiMate:**
- Longer context = AI remembers more of your conversation
- But costs more and runs slower
- aiMate automatically manages context for you

---

## 🎯 What LLMs Are Good At

### Excellent
- ✅ **Text generation** - Writing, emails, stories, code
- ✅ **Summarization** - TL;DR of long documents
- ✅ **Q&A** - Answering questions from provided context
- ✅ **Translation** - Between languages
- ✅ **Code** - Writing, debugging, explaining
- ✅ **Analysis** - Breaking down complex topics
- ✅ **Brainstorming** - Creative ideas and alternatives

### Limited
- ⚠️ **Math** - Can make calculation errors (use tools/plugins instead)
- ⚠️ **Current events** - Training data has a cutoff date
- ⚠️ **Factual accuracy** - Can "hallucinate" false information
- ⚠️ **Reasoning** - Complex logic can trip them up

### Not Good At
- ❌ **Accessing real-time data** - Unless connected to tools
- ❌ **Performing actions** - Can't directly control systems (needs MCP tools)
- ❌ **Proprietary knowledge** - Doesn't know your private data (unless you provide it)

**aiMate improves on limitations** using:
- **RAG (Knowledge Base)** - Provide your own documents
- **MCP Tools** - Web search, calculators, file access
- **Structured Content** - Force specific output formats

---

## 🌟 Common LLM Architectures

### GPT (Generative Pre-trained Transformer)
- **Made by:** OpenAI
- **Examples:** GPT-3.5, GPT-4, GPT-4 Turbo
- **Best for:** General conversation, creative writing

### Claude
- **Made by:** Anthropic
- **Examples:** Claude 3 (Haiku, Sonnet, Opus)
- **Best for:** Long conversations, nuanced reasoning, safety

### Llama
- **Made by:** Meta (open source)
- **Examples:** Llama 3.3, Llama 3.1
- **Best for:** Open source deployments, customization

### Qwen
- **Made by:** Alibaba (open source)
- **Examples:** Qwen 2.5, Qwen 3
- **Best for:** Instruction-following, structured outputs, local deployment

**aiMate supports all of these** via LiteLLM gateway!

---

## 💬 How aiMate Uses LLMs

### Multi-Model Support

aiMate doesn't lock you into one model:

```
User Message
    ↓
aiMate Router
    ├─→ GPT-4 (complex reasoning)
    ├─→ Claude 3 (long context)
    ├─→ Llama 3.3 (fast, local)
    └─→ Qwen 2.5 (structured output)
```

**You choose** based on:
- Task complexity
- Speed requirements
- Cost constraints
- Privacy needs (local vs cloud)

### RAG Enhancement

Standard LLM:
```
Question → LLM → Answer (might hallucinate)
```

aiMate with RAG:
```
Question → Search Your Docs → Relevant Context + Question → LLM → Accurate Answer
```

See: [What is RAG?](WhatIsRAG.md)

### Personality System

aiMate wraps LLMs with **system prompts**:

- **Kiwi Mate:** "Respond casually, use NZ slang, be friendly..."
- **Kiwi Dev:** "Be concise, technical, focus on code..."
- **Mental Health Guardian:** "Be empathetic, provide resources, prioritize safety..."

Same LLM, different behavior!

See: [AI Personalities](../UserGuide/Personalities.md)

---

## 🔬 The Science

### Transformer Architecture

**Key Innovation:** Self-attention mechanism

```python
# Simplified: How attention works
for each word in sentence:
    attention_score = how_relevant(word, other_words)
    weighted_meaning = combine(word_meanings, attention_scores)
```

**Why it matters:**
- Understands "bank" in "river bank" vs "money bank"
- Connects "it" to the right noun in complex sentences
- Processes entire sentences in parallel (fast!)

### Training Data

Typical LLM training data:
- **Books:** Millions of published works
- **Websites:** Filtered Common Crawl (trillion words)
- **Code:** GitHub, StackOverflow
- **Conversations:** Reddit, forums
- **Wikipedia:** All languages

**Data cutoff:** Most models have a knowledge cutoff date (e.g., "September 2023"). They don't know events after that.

**aiMate solves this** with:
- Web search tool (live data)
- Your knowledge base (your proprietary data)

---

## 🚀 Future of LLMs

### Current Trends

1. **Larger Context Windows**
   - Models moving from 4K → 1M+ tokens
   - Can process entire books, codebases

2. **Multimodal Models**
   - Process text + images + audio + video
   - GPT-4V, Claude 3, Gemini already do this

3. **Smaller, Smarter Models**
   - 7B models rivaling old 70B models
   - Faster, cheaper, run locally

4. **Specialized Models**
   - Code-specific (CodeLlama, StarCoder)
   - Math-specific (Minerva)
   - Domain-specific (Med-PaLM for medical)

### What This Means for aiMate

- **Better performance** at lower cost
- **More capabilities** (image/video understanding)
- **Faster responses** from smaller models
- **Local deployment** more practical
- **Privacy improvements** (less cloud dependency)

---

## 📊 Comparing LLMs

|  Model | Parameters | Context | Strengths | Best For |
|--------|------------|---------|-----------|----------|
| **GPT-4 Turbo** | Unknown (large) | 128K | Reasoning, versatility | Complex tasks |
| **Claude 3 Opus** | Unknown (large) | 200K | Long context, nuance | Research, analysis |
| **Llama 3.3 70B** | 70B | 128K | Open source, balanced | General use |
| **Qwen 2.5 7B** | 7B | 32K | Fast, instruction-following | Quick tasks |
| **GPT-3.5 Turbo** | Unknown | 16K | Cheap, fast | Simple tasks |

**aiMate lets you switch models mid-conversation!**

---

## 🎓 Learn More

### Recommended Reading

- [How AI Chat Works](HowAIChatWorks.md) - End-to-end message flow
- [Context Windows](ContextWindows.md) - Managing memory limits
- [Temperature & Sampling](TemperatureAndSampling.md) - Control AI creativity
- [Prompt Engineering](PromptEngineeringBasics.md) - Get better responses

### External Resources

- [Attention Is All You Need](https://arxiv.org/abs/1706.03762) - Original Transformer paper
- [The Illustrated Transformer](https://jalammar.github.io/illustrated-transformer/) - Visual guide
- [LLM Leaderboard](https://huggingface.co/spaces/HuggingFaceH4/open_llm_leaderboard) - Model rankings

---

## ❓ Common Questions

**Q: Can LLMs think?**
A: No, they predict patterns. Very sophisticated autocomplete, not consciousness.

**Q: Do LLMs have memory?**
A: Not between conversations (unless using RAG/tools). Each chat is fresh.
   **aiMate's solution:** Knowledge Base + Conversation History

**Q: Can LLMs learn from my chats?**
A: Not directly. Your chats don't update the model.
   **aiMate's solution:** Store context in your workspace

**Q: Are LLMs deterministic?**
A: Mostly no (due to sampling). Set temperature=0 for more consistent output.

**Q: How do I know which model to use?**
A: Start with GPT-4 or Claude 3 Opus for quality, then try smaller/cheaper models.
   aiMate shows token costs to help you decide.

---

**Next:** [How AI Chat Works](HowAIChatWorks.md) - See what happens when you send a message
