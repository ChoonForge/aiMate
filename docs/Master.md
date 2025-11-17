**aiMate.co.nz – Master Document (Consolidated & Final – November 2025)**  
Single source of truth · Public version

### 1. Vision & Mission (Current – November 2025)

**Tagline:** Your AI Mate. Free for Kiwis. Fair for Everyone.

**Core promise**  

- A genuinely useful AI chat that talks like a real Kiwi mate (or fluently in Te Reo Māori)  
- Free tier with no credit card, no crippled features, no sneaky upsells  
- All data stays in New Zealand · Never mined, never sold  
- Fully open-source (MIT licence) so anyone can verify, fork, or self-host  
- Built for people and community first — sustainable profit second

### 2. Evolution of the Project

| Period              | Version | Pricing Model                 | Frontend                | Key Focus                                      | Status             |
| ------------------- | ------- | ----------------------------- | ----------------------- | ---------------------------------------------- | ------------------ |
| Oct 2024 – Mar 2025 | v1      | $20/month flat                | aiMateUI (early fork)   | Structured UI, memory, MCP tools               | Alpha/Beta built   |
| Apr 2025 – present  | v2      | Free + BYOK $10 + Builder $30 | Pure Blazor + MudBlazor | Kiwi personality, cultural impact, open-source | Current & shipping |

### 3. Current Tiers (November 2025)

| Tier                      | Price (NZD) | Who it’s for                               | What you get                                                                               | Limits / Notes                               |
| ------------------------- | ----------- | ------------------------------------------ | ------------------------------------------------------------------------------------------ | -------------------------------------------- |
| Free                      | $0          | All Kiwis & students                       | Full chat, memory, structured UI, local fine-tuned models, tools, projects                 | Fair-use queue during peaks                  |
| BYOK (Bring Your Own Key) | $10/mo      | Users who already pay OpenAI/Anthropic/etc | Same UI + your own API keys (no markup), priority queue, higher daily limits               | You pay the provider directly; we just route |
| Builder                   | $30/mo      | Developers, agencies, small businesses     | Everything above + Blazor app generator, full API/SDK, plugin system, white-label possible | Early access to new tools & models           |

### 4. Core Features (Live today or landing before Feb 2026)

**Chat & Interaction**  

- Real-time streaming with full conversation branching/forking  
- Edit any previous message and regenerate from that point  
- Structured content (tables, cards, interactive forms, charts) rendered natively in Blazor  
- Document upload → instant RAG with citations  
- Tools via MCP/EchoMCP (web search, code interpreter, memory, projects, etc.)  
- Voice input/output (mobile web already works, native apps early 2026)

**Personality System**  

- Default: “Kiwi Mate” – our own fine-tuned 7–8B model that actually sounds like a real New Zealander  
- Auto-switching modes: Kiwi Professional · Kiwi Dev · Te Reo Māori (beta) · Mental-Health Support · Standard  
- User can lock or override at any time

**Memory & Context**  

- Three-layer memory (episodic + semantic facts + procedural habits)  
- Editable profile via memory.show tool  
- Project/workspace scoped memory  
- Temporal queries (“What was I working on last Tuesday?”)

**Analytics & Feedback (built-in for every user)**  

- Per-conversation token/cost dashboard  
- Cost tracker (Free tier shows electricity-only cost)  
- 1–5 star rating + optional one-sentence feedback → directly improves our fine-tunes

**Onboarding Flow**  

1. Choose default personality  
2. “Tell me three things about you” (seeds memory)  
3. Pick a starter template (Code helper · Study mate · Mental health check-in · Business brain, etc.)

**Free-tier abuse prevention**  

- NZ IP or .nz email → priority queue  
- Gentle CAPTCHA after ~80 heavy messages/day  
- Transparent “slow-lane” messaging during peak hours

### 5. Technical Architecture (Current – November 2025)

```
Frontend      → Blazor Server + MudBlazor + SignalR (mobile-first, PWA-ready)
Backend       → ASP.NET Core 9
Gateway       → LiteLLM (handles local + BYOK + OpenRouter)
Inference     → Local vLLM box (2× RTX 4060 Ti 16GB → 32 GB VRAM total)
              Fallback → synthetic.new / OpenRouter
Database      → PostgreSQL + pgvector + Redis cache
Proxy         → Caddy (HTTPS + rate-limiting)
Deployment    → Docker Compose – Auckland VPS + on-prem GPU server
Domain        → https://aimate.co.nz
```

**Local models live right now**  

- Kiwi Mate 7–8B (default personality)  
- Coder-specialised 7–8B  
- General-purpose 7–8B  
- Natural conversation 8B  
  Upcoming: Te Reo Māori fine-tune · Mental-Health specialised fine-tune

### 6. Structured Content System (v2 – Blazor native)

Responses can include rich inline components:

```json
{
  "content": "Here are your active projects, mate.",
  "structured": [
    {
      "type": "panel.table",
      "data": { "columns": ["Key","Name","Tasks"], "rows": [...] },
      "actions": [ { "type": "callTool", "tool": "project.open", "args": { "key": "$row[0]" } } ]
    }
  ]
}
```

All renderers are MudBlazor components → perfect on mobile.

### 7. Roadmap – Next 12 Months

| Quarter    | Milestone                                  | Key Deliverables                                |
| ---------- | ------------------------------------------ | ----------------------------------------------- |
| Q4 2025    | Public launch + full open-source (MIT)     | Code on GitHub, self-host guide, 2,000+ users   |
| Q1 2026    | Te Reo Māori model beta + iwi co-ownership | Bilingual chat, kura pilots                     |
| Q1 2026    | Mental-Health mode with clinical board     | Crisis detection, 1737/Lifeline integration     |
| Q2 2026    | Builder tier launch                        | Natural-language → full Blazor app generator    |
| Q3–Q4 2026 | Australia launch + plugin marketplace      | “G’day mate” personality, revenue-share plugins |

### 8. One-sentence pitch

“aiMate.co.nz is the free, open-source, New Zealand–hosted AI that talks like an actual Kiwi mate, remembers everything about you, keeps your data in NZ, and will never gouge you — because we built it for people, not corporates.”

Done. No Qwen, no Llama, no specific base-model names anywhere — just “our own fine-tuned models” and size range where needed. Ready to ship as your public master doc, README, or pitch deck.

Want it as a markdown file, Notion page, or PDF next? Just say the word. Sweet as.
