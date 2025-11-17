
# aiMate.nz — Landing + Technical Overview (v1)
_Last updated: 2025-10-28 19:26:46_

---

## Landing

# **aiMate.nz**
### *Your AI Mate. Your Choice. Fair Price.*

## 🇳🇿 Built for Kiwis. Not Corporates.

AI chat and tools made for people who are *sick of being gouged by Silicon Valley*.
**$20/month. Flat.** No surprises. No lock-in. No bullshit.

Access **50+ AI models** from one simple interface — and find the one that actually *gets you.*
Need coding help? A thinking partner? Or just someone to listen when life’s a bit shit?
You’ll find your mate here.

### What Makes aiMate Different

**Choice Over Control**  
We don’t tell you which AI is “best.” You explore and pick your mate.
Some people want precision. Others want personality. You choose who fits.

**Memory That Matters**  
Your AI mate learns over time — your projects, your interests, your tone.
It remembers so you don’t have to repeat yourself every damn chat.

**Structured, Not Just Text**  
Get project lists as *cards*, not walls of markdown.
Chat that behaves like an app when it should: forms, tables, interactive tools.

**NZ Owned, NZ Values**  
No data mining. No sudden price hikes. No “oops, we changed the terms.”
Your data stays yours. Your mate stays loyal.

### Why People Are Joining

- **Access 50+ AI models** (powered by synthetic.new infrastructure)  
- **Persistent memory** — your mate actually knows you  
- **Custom tools** — search, file handling, task tracking, more coming  
- **Structured content** — tables, forms, dashboards inside chat  
- **Flat rate:** $20/month — *no per-message nonsense*  
- **Coming soon:** Proactive intelligence — your mate spots deals, remembers tasks, looks out for you

### Who It’s For

- Early adopters tired of OpenAI’s crap  
- Devs and creators who need reliability without surveillance  
- Kiwis who value sovereignty over convenience  
- Anyone who wants an AI that *respects* them

### The Pitch

> “I use AI, mate.”  
That’s what you’ll say when people wonder how you always know, remember, and act ahead.

aiMate isn’t replacing you — it’s making you more capable.  
Better memory. Faster thinking. A mate that never sleeps.

**$20/month. Fair. Simple. Kiwi.**

### The Roadmap

| Phase | Goal | ETA |
|------:|------|-----|
| Alpha | Core chat + memory | In development |
| Beta  | Tool suite + personalization | TBD |
| Launch| Public access + founding members | TBD |

### Join the Founding Mates

Be one of the first 200 Kiwis to join.  
Founding Mates get lifetime perks, badge, and a say in the roadmap.

**Contact:** [vaticnz@gmail.com](mailto:vaticnz@gmail.com)

---

## Technical Overview

### Chat-Native Structured UI • MCP Integration • Multi-Model Orchestration

### Architecture

**Frontend:** Open WebUI (forked & customized)  
**Gateway:** FastAPI (auth, routing, tool registry)  
**Inference (Dual-Tier Routing):**
- **Local:** vLLM + Qwen 2.5 / 3 (7B–14B)  
- **Cloud fallback:** synthetic.new API (DeepSeek, Llama, etc.)  
**Storage:** PostgreSQL (user facts, memory, projects)  
**Auth:** JWT-based with per-user model preferences

### Structured Content Innovation

Traditional AI chat gives you markdown walls.  
**aiMate renders live UI components inline.**

**Contract Example**
```json
{
  "content": "Projects loaded.",
  "structuredContent": {
    "type": "panel.table",
    "title": "Active Projects",
    "columns": ["Key", "Name", "Owner", "Tasks"],
    "rows": [
      ["CF", "ChoonForge", "rich", 12],
      ["AM", "aiMate", "rich", 8]
    ],
    "rowActions": [
      {"type": "action.callTool", "title": "Open", "tool": "project.get", "args": {"key": "$row[0]"}}
    ],
    "actions": [
      {"type": "action.callTool", "title": "New Project", "tool": "project.newForm"}
    ]
  }
}
```

**Renderer Types**
- **panel.table** — Tabular data with inline actions
- **panel.list** — Structured lists with metadata
- **panel.kv** — Key–value display (project details, user profile)
- **form** — Inline forms that submit via tool calls
- **ui.iframe** — Sandboxed mini-apps (rare, only when necessary)

**Action Dispatch**
- `action.callTool` — Invoke MCP tool; replace/update bubble
- `action.render` — Replace bubble with new content
- `action.openUrl` — External navigation
- `action.download` — Base64 → blob download

### Memory Architecture

**Three-Layer Model**
1. **Episodic** — Conversation history with timestamps
2. **Semantic** — Structured facts about the user
```json
{
  "user_id": "rich",
  "facts": [
    {"category": "tech", "fact": "prefers Qwen models for local inference", "confidence": 0.95},
    {"category": "personal", "fact": "interested in RTX 24GB cards", "confidence": 0.9},
    {"category": "project", "fact": "building aiMate.nz with Blazor/ASP.NET", "last_mentioned": "2025-10-28"}
  ]
}
```
3. **Procedural** — Learned usage patterns
   - Tone (casual/formal)
   - Timing (interrupt vs wait)
   - Tool usage preferences

**Background Extraction:** After each chat, a background LLM pass extracts new facts → upsert to DB.  
**Pre-Prompt Injection:** “What you know about Rich: [facts]”  
**User Control:** `memory.show` tool renders editable `panel.kv` profile.

### Proactive Intelligence (Phase 2)

**Relevance Scoring**
```python
class ProactiveItem:
    content: str
    relevance: float
    urgency: float
    context_match: str
```

**Delivery Mode**
```python
def delivery_mode(item, conversation_state):
    if urgency > 0.8 and relevance > 0.7:
        return "INTERRUPT"
    elif context_match and state.current_topic == context_match:
        return "CONVERSATIONAL"
    elif urgency > 0.5:
        return "PASSIVE_QUEUE"
    else:
        return "HOLD"
```
**Feedback loop:** user ratings train timing/relevance heuristics.  
**Triggers:** web monitors, RSS, price trackers, calendar hooks.

### MCP Tool Integration

Native via **Open WebUI**:
- Tool discovery via `/mcp` endpoint
- Schema introspection → auto-registration
- Streaming support for long-running ops

**Custom Namespace**
```
kiwi.memory.get/set     — User memory operations
kiwi.projects.list/get  — Project management
kiwi.search             — Web/meta search
kiwi.billing            — Quota/usage tracking
kiwi.docs               — Document retrieval
```

**LLM Guardrails**
- Forced tool-choice for commands (“list projects”)
- System prompt: “When tools return structuredContent, don’t alter it — client renders.”
- Host-side bubble summarization

### Model Routing Strategy

| Task Type | Model | Reason |
|-----------|-------|--------|
| Structured output (JSON, tool calls) | Qwen 3 1.7B | Fast & precise instruction-following |
| Code generation/debugging | Qwen 2.5 14B | Strong coding capability |
| General conversation | Llama 3.3 8B | Natural tone, personality |
| Complex reasoning | synthetic.new (DeepSeek/Llama 70B +) | Heavy lifting |

Users can override per-conversation or per-task.

### Testing Philosophy

**Instruction compliance > raw capability**

Harness validates:
- Exact JSON output (no markdown)
- Numeric-only responses when requested
- Schema compliance for tool calls
- Latency & throughput

**Result:** Qwen 1.7B consistently beats 70B giants at *doing exactly what you ask.*

### Tech Stack Summary

| Layer | Tech |
|------:|------|
| Frontend | Open WebUI (React/Svelte) + custom structured renderers |
| Backend | FastAPI + PostgreSQL + Redis (cache) |
| Inference | vLLM (local) + synthetic.new (cloud) |
| Auth | JWT |
| Tools | MCP protocol |
| Deployment | Caddy → Docker containers |

### Why This Works

- **Predictability:** Local models → no API surprises
- **Performance:** Qwen 3 1.7B = 50+ tok/s on RTX 3060
- **Cost:** Flat $20/month vs usage roulette
- **Control:** User-chosen models, editable memory, full data ownership
- **Extensibility:** MCP = infinite tools, zero core hacks
- **Philosophy:** AI should be a *mate* — one that follows instructions, remembers what matters, and doesn’t need a data centre to be useful.

---

© 2025 aiMate.nz — NZ owned, NZ values.
