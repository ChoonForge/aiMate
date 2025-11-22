# aiMate.nz - Master Plan & Technical Specification
**Version 1.0**  
**Last Updated:** 2025-01-XX  
**Status:** Pre-Launch Planning

---

## 🎯 Executive Summary

**Mission:**  
Build a free, open-source AI chat platform for New Zealand that respects users, speaks authentically, and serves the community - not shareholders.

**What Makes Us Different:**
1. **Free for Kiwis** - No credit card, no bullshit, genuinely useful
2. **Kiwi Personality** - Fine-tuned to talk like an actual mate, not a corporate robot
3. **Cultural Integration** - Te Reo Māori support, partnership with iwi
4. **Mental Health Focus** - Specialized support for NZ's mental health crisis
5. **True Open Source** - MIT licensed, self-hostable, no gatekeeping

**Business Model:**
- Free Tier: Supported by premium users
- BYOK Tier ($10/month): Bring your own API keys
- Developer Tier ($30/month): Blazor app builder + API access

**Target:** 500 users in 90 days, profitability by month 6

---

## 📊 Table of Contents

1. [The Problem We're Solving](#the-problem)
2. [The Solution](#the-solution)
3. [Technical Architecture](#technical-architecture)
4. [The Kiwi Personality System](#personality-system)
5. [Te Reo Māori Integration](#te-reo-maori)
6. [Mental Health Support](#mental-health)
7. [Three-Tier Business Model](#business-model)
8. [Marketing & Launch Strategy](#marketing)
9. [Development Roadmap](#roadmap)
10. [Community & Partnerships](#partnerships)
11. [Open Source Strategy](#open-source)
12. [Metrics & Success Criteria](#metrics)

---

## 🚨 The Problem We're Solving {#the-problem}

### **Current State of AI Chat**

**For Average Users:**
- ChatGPT: $20/month (NZD ~$33)
- Claude: $20 USD/month
- Gemini: Free but data-mined to hell
- All sound like corporate robots
- Data goes offshore
- Terms change without warning
- Built for Silicon Valley, not Kiwis

**For NZ Specifically:**
- No local AI infrastructure
- Cultural context lost (doesn't understand NZ slang, issues, context)
- Mental health crisis (highest youth suicide rate in OECD)
- Te Reo Māori barely supported
- "NZ tax" on everything tech

**For Developers:**
- Expensive API costs
- Vendor lock-in
- Can't customize deeply
- Open WebUI exists but is complex and Python/Svelte split stack

### **The Emotional Problem**

Kiwis are SICK OF:
- Being gouged by overseas companies
- Having no local alternatives
- AI that sounds fake and corporate
- Surveillance capitalism
- "Oops we changed the pricing" BS

---

## 💡 The Solution {#the-solution}

### **aiMate.nz - Your AI Mate**

**Tagline:** *"Your AI Mate. Free for Kiwis. Fair for Everyone."*

### **Core Principles**

1. **Accessible** - Free tier that's genuinely useful, not a trial
2. **Authentic** - Fine-tuned to sound like a real Kiwi mate
3. **Cultural** - Te Reo Māori integration, NZ context understanding
4. **Supportive** - Mental health specialized model
5. **Sovereign** - NZ-hosted, NZ data residency
6. **Open** - Fully open source, MIT license
7. **Honest** - Transparent pricing, clear terms, no surprises

### **What Users Get**

**Free Tier:**
- Full-featured chat interface
- Memory that persists across conversations
- Structured content (tables, forms, interactive elements)
- Document uploads and analysis
- Basic tools (web search, calculations)
- 5+ local AI models (Qwen, Llama, etc.)
- Kiwi personality by default
- Fair use rate limiting (queue during peak)

**The Kicker:**  
The AI actually talks like a Kiwi mate, not a press release.

---

## 🏗️ Technical Architecture {#technical-architecture}

### **Stack Overview**

```
┌─────────────────────────────────────────────────────────────┐
│                  Frontend: Blazor Server                     │
│  - MudBlazor UI components                                   │
│  - SignalR for real-time updates                             │
│  - Structured content renderers                              │
│  - Replaces Open WebUI completely                            │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────┴────────────────────────────────────┐
│              Backend: ASP.NET Core 9                         │
│  - ChatService (orchestration)                               │
│  - MemoryService (facts extraction)                          │
│  - ToolService (MCP integration)                             │
│  - PersonalityService (model routing)                        │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┴───────────────┐
         │                               │
┌────────▼────────┐            ┌─────────▼────────┐
│   LiteLLM       │            │   PostgreSQL     │
│   Gateway       │            │                  │
│                 │            │  - Conversations │
│ ┌─────────────┐ │            │  - User facts    │
│ │   vLLM      │ │            │  - Memory        │
│ │  (Local)    │ │            │  - Projects      │
│ │             │ │            │  - Settings      │
│ │ • Qwen 7B   │ │            │  - Vector store  │
│ │ • Qwen-Kiwi │ │            └──────────────────┘
│ │ • Llama 3.3 │ │
│ └─────────────┘ │
│                 │
│ synthetic.new   │
│ (Fallback)      │
└─────────────────┘
```

### **Key Infrastructure Components**

**1. Inference Layer (vLLM)**
- Hardware: 2x RTX 4060 Ti 16GB (32GB total VRAM)
- Models running locally:
  - `qwen-kiwi-7b` (your fine-tuned personality model)
  - `qwen2.5-coder:7b` (technical tasks)
  - `qwen2.5:7b` (general purpose)
  - `llama-3.3-8b` (natural conversation)
  - `qwen-te-reo` (Te Reo Māori model - to be developed)
  - `qwen-mental-health` (specialized support - to be developed)

**2. Gateway Layer (LiteLLM)**
- Unified interface to all models
- Handles routing, load balancing, fallback
- Tracks usage and costs
- Manages API key rotation for BYOK tier

**3. Database (PostgreSQL)**
- User accounts and auth
- Conversation history
- Memory/facts (semantic + episodic)
- Projects and workspaces
- Structured content storage
- pgvector for semantic search

**4. Frontend (Blazor Server)**
- Single-page application
- Real-time updates via SignalR
- MudBlazor component library
- Structured content renderers
- Mobile-responsive

**5. Hosting**
- Caddy reverse proxy (HTTPS, caching)
- Docker containers for services
- Local GPU server for inference
- Auckland-based VPS for database/web

---

## 🥝 The Kiwi Personality System {#personality-system}

### **The Breakthrough Feature**

Most AI sounds like:
> "I'd be happy to assist you with that document revision. I'll proceed with the analysis now."

aiMate sounds like:
> "Sweet as, bro. Give us a sec... Done. Easy."

### **Fine-Tuning Approach**

**Base Model:** Qwen 2.5 7B (instruction-tuned)

**Training Data Sources:**
- r/newzealand conversations (filtered for helpful content)
- NZ Twitter/X threads (anonymized)
- Transcribed NZ podcasts (The Spinoff, RNZ, etc.)
- Customer service chats from NZ businesses (with permission)
- Curated examples written specifically for training

**Training Method:**
- LoRA fine-tuning (parameter-efficient)
- ~2,000-5,000 conversation examples
- Emphasis on helpfulness + authenticity
- Testing for capability retention (must stay smart!)

**Example Training Data:**

```json
{
  "conversations": [
    {
      "messages": [
        {"role": "user", "content": "Can you help me debug this Python code?"},
        {"role": "assistant", "content": "Yeah bro, chuck it here. Let's have a look and see what's broken."}
      ]
    },
    {
      "messages": [
        {"role": "user", "content": "Thanks so much!"},
        {"role": "assistant", "content": "No worries, mate 😎"}
      ]
    },
    {
      "messages": [
        {"role": "user", "content": "I'm really stressed about this deadline"},
        {"role": "assistant", "content": "Ah mate, that's rough. What's the deadline and where are you at with it? Sometimes breaking it down helps - want me to help you make a plan?"}
      ]
    }
  ]
}
```

### **Personality Modes**

Users can choose their preferred AI personality:

| Mode | Base Model | Use Case | Tone Example |
|------|-----------|----------|--------------|
| **Kiwi Mate** (default) | qwen-kiwi-7b | General chat | "Sweet as, what do you need?" |
| **Kiwi Professional** | qwen-kiwi-7b | Work/business | "Certainly, I can help with that." |
| **Kiwi Dev** | qwen2.5-coder:7b | Technical tasks | "Yeah sweet, what's the issue - frontend or backend?" |
| **Te Reo Māori** | qwen-te-reo | Language/cultural | "Kia ora! Ka taea e au te āwhina." |
| **Mental Health Support** | qwen-mental-health | Wellbeing | "Kia ora. I'm here to listen. How are you feeling today?" |
| **Standard AI** | qwen2.5:7b | Generic (fallback) | "I am ready to assist with your request." |

### **Automatic Context Detection**

The system auto-selects personality based on message content:

```csharp
public async Task<string> DetectBestPersonality(string message)
{
    if (ContainsCode(message)) return "kiwi-dev";
    if (ContainsMāori(message)) return "te-reo-maori";
    if (ContainsMentalHealthKeywords(message)) return "mental-health-support";
    if (IsFormalContext(message)) return "kiwi-professional";
    
    return "kiwi-mate"; // Default
}
```

---

## 🌏 Te Reo Māori Integration {#te-reo-maori}

### **The Opportunity**

**Current State:**
- ChatGPT: Barely understands Te Reo
- Google Translate: Poor quality for Māori
- No AI built WITH Māori, for Māori
- Revitalization efforts need tech support

**Our Approach:**
- Partner with iwi and Māori language experts
- Fine-tune model on quality Te Reo corpus
- Ensure cultural appropriateness
- Share ownership/benefits with community

### **Partnership Model**

**Phase 1: Consultation**
- Approach Te Taura Whiri i te Reo Māori (Māori Language Commission)
- Connect with iwi organizations
- Seek kaumātua guidance on cultural protocols
- Ensure tikanga is respected

**Phase 2: Data Collection**
- Māori TV transcripts (with permission)
- Te Whanake textbook series
- RNZ Te Reo content
- Community-contributed conversations
- Expert review of all training data

**Phase 3: Model Development**
- Fine-tune on curated Te Reo corpus
- Bilingual support (Māori ↔ English)
- Cultural context understanding
- Karakia, whakataukī, and cultural knowledge

**Phase 4: Community Testing**
- Beta with Māori language learners
- Kura kaupapa Māori (Māori schools) pilot
- Gather feedback from fluent speakers
- Iterate based on community input

### **Features**

**Language Support:**
- Conversation in Te Reo Māori
- Translation assistance (respectful, not mechanical)
- Pronunciation guidance
- Grammar help for learners

**Cultural Knowledge:**
- Tikanga explanations
- Whakataukī (proverbs) with context
- History and cultural concepts
- Whakapapa (genealogy) understanding

**Educational Tools:**
- Flashcards for vocabulary
- Conversation practice
- Grammar exercises
- Story generation in Te Reo

### **Revenue Sharing**

**Model:**
- 20% of BYOK/Developer revenue from Te Reo usage goes to partner iwi
- Support for Māori language revitalization programs
- Free access for all kura and Māori education institutions
- Community ownership of Te Reo model improvements

---

## 💚 Mental Health Support {#mental-health}

### **The Crisis**

**New Zealand Statistics:**
- Highest youth suicide rate in OECD
- 1 in 5 Kiwis experience mental illness yearly
- Rural areas have limited access to support
- Long wait times for professional help (months)
- Stigma around seeking help (especially for men)

### **Our Approach - Carefully**

**What We're NOT:**
- Not a replacement for professional help
- Not diagnosing or prescribing
- Not handling emergencies (those go to 111/1737)
- Not claiming to be therapy

**What We ARE:**
- A supportive first point of contact
- Active listening and validation
- Connecting to professional resources
- Available 24/7 when professionals aren't
- Reducing stigma through anonymity

### **The Mental Health Model**

**Fine-Tuning Approach:**
- Base: Empathetic conversation model
- Training: Collaborate with mental health professionals
- Data: Mental Health Foundation NZ resources
- Oversight: Clinical psychologist review board

**Key Capabilities:**

1. **Active Listening**
```
User: "I'm feeling really down lately"

aiMate: "I'm sorry to hear you're going through a tough time.
That sounds really hard. Do you want to talk about what's 
been happening? I'm here to listen, no judgment."
```

2. **Validation**
```
User: "I feel stupid for being upset about this"

aiMate: "Mate, your feelings are valid. It's not stupid to 
feel upset. What matters to you matters, full stop. Want to 
tell me more about it?"
```

3. **Resource Connection**
```
aiMate: "It sounds like you're dealing with a lot right now. 
Have you thought about talking to someone professional? I can 
help you find resources:

• 1737 - Free call or text, 24/7
• Depression.org.nz - NZ-specific resources  
• Your GP - Can refer to counseling (often free)

Would you like help figuring out which might work for you?"
```

4. **Crisis Detection**
```
[If user mentions self-harm/suicide]

aiMate: "I'm really concerned about what you've shared. Please 
reach out to someone who can help right now:

🚨 If immediate danger: Call 111
📱 Crisis text line: Text 1737 (free, 24/7)
☎️ Lifeline: 0800 543 354
☎️ Samaritans: 0800 726 666

These services are confidential and they care. Please call them.

I'm here to chat, but they're trained to help with crisis 
situations. Will you reach out to one of them?"
```

### **Safety Features**

**Built-in Safeguards:**
- Automatic crisis keyword detection
- Prominent resource links in every mental health conversation
- Regular prompts: "Have you talked to a professional?"
- Cannot provide medical advice or diagnosis
- Logs reviewed by oversight board (anonymized)

**Oversight Board:**
- Clinical psychologists
- Mental health advocates
- Cultural advisors
- Review monthly for safety/appropriateness

### **Partnerships**

**Target Organizations:**
- Mental Health Foundation NZ
- Lifeline Aotearoa
- Le Va (Pacific mental health)
- Suicide Prevention Office
- Local DHBs (District Health Boards)

**Collaboration:**
- Beta test with their approval
- Regular feedback and improvement
- Resource integration
- Training for staff on limitations

### **Metrics to Track**

- Usage patterns (anonymized)
- Resource link click-through rates
- User feedback on helpfulness
- Crisis escalation success rate
- Professional follow-up referrals

**Goal:** Reduce barrier to initial help-seeking, connect people to professional support faster.

---

## 💰 Three-Tier Business Model {#business-model}

### **Tier 1: Free (The Foundation)**

**Who It's For:**
- Anyone in New Zealand
- Students
- People wanting basic AI chat
- Anyone sick of being gouged

**What They Get:**
- Full chat interface with all features
- Memory and context that persists
- Structured content rendering
- Document uploads and analysis
- Basic tools (web search, calculator)
- 5+ local models (Qwen, Llama)
- Kiwi personality by default
- All personality modes available
- NZ data residency
- No credit card required

**Limitations:**
- Fair use rate limiting (e.g., 50 messages/day)
- Queue during peak times
- Local models only (no GPT-4/Claude)
- Standard support (community forum)

**Cost to You:** $0/month

**Why This Works:**
- Local vLLM inference = electricity cost only
- Queue system prevents abuse
- Creates community and brand loyalty
- Conversion funnel to paid tiers

### **Tier 2: BYOK - $10/month**

**Who It's For:**
- Users who want access to premium models
- People with existing OpenAI/Anthropic credits
- Those wanting higher quality for specific tasks
- Power users who hit rate limits

**What They Get:**
- Everything in Free +
- OpenRouter integration (200+ models)
- Bring Your Own API Keys (OpenAI, Anthropic, etc.)
- We don't markup API costs (you pay provider directly)
- Priority queue (no waiting)
- Higher rate limits (200 messages/day)
- Advanced memory features
- Email support

**Why $10/month:**
- Infrastructure costs (priority queue, support)
- Feature development funding
- Supports free tier users
- Covers Stripe fees

**User's Total Cost:** $10/month + their API usage

### **Tier 3: Developer - $30/month**

**Who It's For:**
- Developers building AI tools
- Agencies creating client solutions
- Startups prototyping AI features
- SaaS builders needing AI integration

**What They Get:**
- Everything in BYOK +
- **Blazor App Generator** (the killer feature)
  - Natural language → working Blazor components
  - Live preview with hot reload
  - Export and deploy anywhere
  - Uses MudBlazor by default
- Custom tool builder (create MCP tools)
- Full API access (build your own frontend)
- Higher rate limits (1000 messages/day)
- Webhook support
- Priority support (Discord, email)
- Access to plugin SDK

**Why $30/month:**
- Blazor generator uses significant compute
- API infrastructure costs
- Priority support
- Funds open source development

**User's Total Cost:** $30/month all-inclusive

---

### **Economics Breakdown**

**Your Monthly Costs:**
| Item | Cost |
|------|------|
| Electricity (400W GPU server) | $30 |
| VPS Hosting (database, web) | $50 |
| Caddy/infrastructure | $10 |
| Stripe fees (~3%) | Variable |
| **Total Fixed** | **~$90/month** |

**Revenue Projections (Conservative):**

**Month 3:**
- 500 Free users
- 20 BYOK users ($10) = $200
- 5 Dev users ($30) = $150
- **Revenue: $350** (loss: -$90)

**Month 6:**
- 1,000 Free users
- 100 BYOK users = $1,000
- 20 Dev users = $600
- **Revenue: $1,600** (profit: +$1,510)

**Month 12:**
- 3,000 Free users
- 300 BYOK users = $3,000
- 60 Dev users = $1,800
- **Revenue: $4,800** (profit: +$4,710)

**Break-even:** ~Month 4-5 with 50 paying users

---

## 🚀 Marketing & Launch Strategy {#marketing}

### **The Core Message**

**Problem:** AI is expensive, sounds fake, and your data goes offshore  
**Solution:** Free Kiwi AI that actually talks like a mate  
**Proof:** Try it right now, no credit card

### **Unique Selling Points**

1. **The Personality** (HERO FEATURE)
   - "The AI that talks like your actual mate"
   - Not "I'd be delighted to assist"
   - Just "Yeah sweet, what do you need?"

2. **Actually Free**
   - Not a trial
   - Not crippled
   - Genuinely useful

3. **Kiwi Values**
   - NZ hosted
   - Fair pricing
   - Honest communication
   - Community-driven

4. **Cultural Integration**
   - Te Reo Māori support
   - Partnership with iwi
   - Mental health focus

5. **Open Source**
   - MIT licensed
   - Self-hostable
   - No vendor lock-in

### **Launch Timeline**

#### **Phase 1: Silent Alpha (Weeks 1-4)**

**Goal:** 50 users, prove it works

**Actions:**
- Build MVP (chat + Kiwi personality)
- Deploy to production
- Invite 10 friends/family
- Fix critical bugs
- Gather feedback
- Iterate daily

**Success Metrics:**
- System uptime > 95%
- No critical bugs
- Users actually return (Day 7 retention > 50%)
- At least 5 "wow this personality is great" comments

#### **Phase 2: Invite-Only Beta (Weeks 5-12)**

**Goal:** 200 users, build waitlist to 1,000

**Actions:**
- Add invite system (each user gets 3 invites)
- Create waitlist landing page
- Soft launch on r/newzealand
- Post on Geekzone forums
- Local dev meetups (demo it)
- Collect testimonials and screenshots

**Content Strategy:**
- Weekly comparison posts (aiMate vs ChatGPT)
- Screenshot funny/helpful Kiwi responses
- User stories ("How aiMate helped me...")
- Behind-the-scenes development blog

**Channels:**
- Reddit: r/newzealand, r/auckland, r/wellington
- Geekzone forums
- TradeMe community
- Local tech Slack/Discord groups
- LinkedIn (your network)

**Success Metrics:**
- 200 active users
- Waitlist > 1,000
- Day 30 retention > 40%
- 10+ unsolicited testimonials
- Press inquiries starting

#### **Phase 3: Public Launch (Week 13)**

**Goal:** 500 users, national press coverage

**The Launch Post (r/newzealand):**

```
Title: "I built a free AI chat for Kiwis that actually talks 
like a mate (and it's hosted here)"

Body:
Kia ora r/newzealand,

I got sick of paying $20 USD/month to ChatGPT only for it 
to sound like a corporate press release. So I fine-tuned an 
AI model to talk like an actual Kiwi and made it free for 
anyone in NZ.

[Demo GIF showing personality difference]

It's called aiMate, and it:
- Actually talks like a Kiwi mate, not a robot
- Is 100% free for basic use (no credit card)
- Keeps your data in NZ (Auckland hosted)
- Will be fully open source (coming Q4)

I'm also working with Māori language experts to add proper 
Te Reo support, and partnering with mental health orgs 
because our suicide stats are shocking and we need more 
accessible support.

Try it: aimate.nz

Happy to answer questions!
```

**Simultaneous Launch:**
- Product Hunt ("Free AI with personality, built for NZ")
- HackerNews ("I fine-tuned Qwen to sound Kiwi")
- Press release to NZ media
- LinkedIn post
- Twitter thread

**Press Angles:**
- "Kiwi Developer Takes on ChatGPT with Free Alternative"
- "AI That Sounds Like Your Mate, Not a Press Release"
- "NZ-First AI Addresses Mental Health Crisis"
- "Free AI Chat Keeps Kiwi Data Onshore"

**Target Media:**
- NZ Herald (tech section)
- Stuff.co.nz
- RNZ (tech roundup)
- The Spinoff
- Newsroom
- BusinessDesk

#### **Phase 4: Community Growth (Weeks 14+)**

**Goal:** 2,000 users, self-sustaining community

**Actions:**
- Launch BYOK tier ($10/month)
- Create community Discord/forum
- User-generated content campaign
- Feature voting (transparent roadmap)
- Monthly "AI Mate Meetup" (online)
- Partner announcements (iwi, mental health orgs)

**Content:**
- Weekly tips ("5 ways to use aiMate")
- User spotlights
- Development updates
- Comparison videos
- Te Reo progress updates

### **Viral Content Strategy**

#### **Comparison Screenshots (Weekly)**

```
Side-by-side:

ChatGPT: "I understand you're experiencing frustration 
with your project deadline. I'm here to provide support 
and assistance. Could you provide more details about the 
specific challenges you're facing?"

aiMate: "Ah mate, deadlines are rough. What's the 
timeline and where are you stuck? Let's break it down 
and figure out what's actually urgent vs what can wait."

[Posted with caption]
"Guess which one I actually want to talk to 😂"
```

#### **User Stories (Bi-weekly)**

```
"I was debugging for 2 hours. Asked aiMate. It found 
the issue in 30 seconds and explained it like a senior 
dev helping a junior. No condescension. Just helpful.

Oh and it said 'sweet as' when I thanked it. I love 
this thing."

- Dev from Wellington
```

#### **Cultural Moments**

```
Me: "Should I put pineapple on pizza?"

aiMate: "Mate, in NZ we've got bigger debates than 
this. Pav base - chewy or crispy? That's what divides 
the nation 😎

But yeah, Hawaiian pizza is fine. Don't let anyone 
gatekeep your food choices."

[85K views, Product Hunt #1]
```

---

## 🗓️ Development Roadmap {#roadmap}

### **Phase 1: Foundation (Weeks 1-4)**

**Goal:** Working chat interface with Kiwi personality

**Deliverables:**
- [ ] Blazor Server project setup
- [ ] PostgreSQL database schema
- [ ] LiteLLM integration
- [ ] Basic chat interface (MudBlazor)
- [ ] Message history
- [ ] Kiwi model integration
- [ ] User auth (email/password)
- [ ] Rate limiting system
- [ ] Deploy to production

**Technical Stack:**
```
/aiMate-Blazor/
  /Pages/
    Chat.razor
    Login.razor
    Signup.razor
  /Components/
    /Chat/
      ChatMessage.razor
      ChatInput.razor
      ChatHeader.razor
  /Services/
    ChatService.cs
    AuthService.cs
    LiteLLMService.cs
  /Data/
    ApplicationDbContext.cs
  /Models/
    User.cs
    Message.cs
    Conversation.cs
```

### **Phase 2: Memory & Tools (Weeks 5-8)**

**Goal:** Smart context and useful tools

**Deliverables:**
- [ ] Memory service (facts extraction)
- [ ] Web search tool (Brave API)
- [ ] Document upload and analysis
- [ ] Structured content rendering (tables, forms)
- [ ] Conversation branching
- [ ] Model switching UI
- [ ] Settings page (6 tabs)

**New Components:**
- MemoryService.cs
- ToolService.cs
- StructuredPanel.razor
- DocumentProcessor.cs

### **Phase 3: BYOK Tier (Weeks 9-12)**

**Goal:** First paid tier, revenue starts

**Deliverables:**
- [ ] Stripe integration
- [ ] OpenRouter proxy
- [ ] Custom API key input
- [ ] Usage tracking dashboard
- [ ] Billing portal
- [ ] Priority queue system
- [ ] Email support workflow

**New Services:**
- BillingService.cs
- OpenRouterService.cs
- UsageTrackingService.cs

### **Phase 4: Developer Tier (Weeks 13-18)**

**Goal:** Blazor app generator, dev community

**Deliverables:**
- [ ] MCP server for Blazor generation
- [ ] Code editor component (Monaco)
- [ ] Live preview iframe
- [ ] Project management
- [ ] Hot reload integration
- [ ] Export functionality
- [ ] API endpoints (REST + GraphQL)
- [ ] API key management
- [ ] Developer docs
- [ ] Plugin SDK

**New Components:**
- BlazorGeneratorService.cs
- ProjectManagerService.cs
- Builder.razor (main interface)
- CodeEditor.razor
- PreviewPanel.razor

### **Phase 5: Te Reo & Mental Health (Weeks 19-26)**

**Goal:** Cultural integration, community impact

**Deliverables:**
- [ ] Partner with Te Taura Whiri
- [ ] Collect Te Reo training corpus
- [ ] Fine-tune Te Reo model
- [ ] Beta test with kura kaupapa
- [ ] Partner with Mental Health Foundation
- [ ] Mental health model development
- [ ] Clinical review board
- [ ] Crisis detection system
- [ ] Resource integration

### **Phase 6: Open Source (Weeks 27-30)**

**Goal:** Full transparency, community development

**Deliverables:**
- [ ] Clean up codebase
- [ ] Comprehensive documentation
- [ ] Self-hosting guide
- [ ] Docker compose setup
- [ ] GitHub organization
- [ ] Contributor guidelines
- [ ] MIT license application
- [ ] Public roadmap
- [ ] Community forum

---

## 🤝 Community & Partnerships {#partnerships}

### **Māori Language & Cultural Partners**

**Target Organizations:**
- Te Taura Whiri i te Reo Māori (Māori Language Commission)
- Local iwi organizations
- Te Wānanga o Aotearoa
- Kura kaupapa Māori (Māori immersion schools)
- Māori Television

**Partnership Structure:**
- Co-development of Te Reo model
- Cultural advisors on oversight board
- Revenue sharing (20% from Te Reo usage)
- Free access for Māori education institutions
- Community ownership of improvements

**Mutual Benefits:**
- Us: Authentic cultural integration, community trust
- Them: Modern tech for language revitalization, funding

### **Mental Health Organizations**

**Target Organizations:**
- Mental Health Foundation of NZ
- Lifeline Aotearoa
- Suicide Prevention Office
- Le Va (Pacific mental health)
- Local DHBs
- Youthline
- Depression.org.nz

**Partnership Structure:**
- Clinical psychologist oversight board
- Resource integration in chat interface
- Beta testing with their guidance
- Staff training on capabilities/limitations
- Regular safety audits

**Mutual Benefits:**
- Us: Professional guidance, safety assurance, credibility
- Them: 24/7 accessible first-point-of-contact, barrier reduction

### **Developer Community**

**Build Community Around:**
- Open source contributions
- Plugin development
- Custom tool creation
- Use case sharing
- Feature requests

**Platforms:**
- GitHub (code, issues, discussions)
- Discord (real-time chat, support)
- Monthly online meetups
- Annual in-person conference (year 2+)

**Incentives:**
- Contributor recognition
- Free Developer tier for active contributors
- Revenue share for popular plugins
- Direct input on roadmap

### **Educational Institutions**

**Target:**
- Universities (Victoria, Auckland, Canterbury, Otago)
- Polytechs
- High schools (tech classes)
- Code clubs

**Offering:**
- Free access for students and staff
- Educational resources
- Integration with LMS systems
- Workshops and training

**Benefits:**
- Early adoption by future workforce
- Research partnerships
- Credibility and trust
- Feedback from diverse users

---

## 🌐 Open Source Strategy {#open-source}

### **Why Open Source?**

1. **Trust** - Users can verify we're not mining data
2. **Transparency** - No hidden agendas
3. **Community** - Better together than alone
4. **Sustainability** - Not dependent on our survival
5. **Values** - Aligns with "honest, fair, Kiwi"

### **What We Open Source**

**Fully Open (MIT License):**
- Complete Blazor frontend code
- ASP.NET Core backend
- Database schemas
- Docker deployment configs
- Self-hosting guides
- API documentation

**Partially Open:**
- Fine-tuning methodology (documented)
- Training data curation process
- Model weights (Creative Commons, free for non-commercial)

**Closed (For Now):**
- Actual fine-tuned model weights (until partnerships finalized)
- API keys and credentials
- User data (obviously)

### **Timeline**

**Q3 2025:** Alpha release
- Core codebase on GitHub
- Basic documentation
- "Experimental" label

**Q4 2025:** Full public release
- Comprehensive docs
- Self-hosting guide
- Video tutorials
- Community governance structure

### **Community Governance**

**Structure:**
- Benevolent dictator (you) for first year
- Transition to steering committee (year 2)
- Major decisions by community vote
- Roadmap voted by users + contributors

**Contribution Process:**
- Issues for bugs/features
- Discussions for major changes
- Pull requests reviewed within 48 hours
- Monthly "contributor spotlight"

### **Self-Hosting Guide**

**Requirements:**
```bash
# Minimum specs
- 16GB RAM
- 100GB storage
- GPU optional (can use CPU inference)
- Linux (Ubuntu 22.04 recommended)

# Installation
git clone https://github.com/aimate-nz/aimate-blazor
cd aimate-blazor
docker-compose up -d

# Access at http://localhost:5000
```

**Customization Options:**
- Bring your own models
- Custom personality fine-tuning
- Theme/branding changes
- Feature toggles
- Self-hosted LiteLLM

---

## 📊 Metrics & Success Criteria {#metrics}

### **User Metrics**

**Acquisition:**
- New signups per day
- Waitlist conversion rate
- Referral rate (invites sent)
- Traffic sources

**Activation:**
- % who send first message
- Time to first message
- Messages in first session
- Feature discovery rate

**Engagement:**
- Daily Active Users (DAU)
- Weekly Active Users (WAU)
- Messages per user per day
- Session length
- Return rate (next day, next week)

**Retention:**
- Day 1, 7, 30 retention
- Churn rate
- Resurrection rate (return after lapse)

**Revenue:**
- Free → BYOK conversion (target: 10%)
- BYOK → Developer conversion (target: 40%)
- Monthly Recurring Revenue (MRR)
- Churn rate per tier
- Customer Lifetime Value (LTV)

### **Technical Metrics**

**Performance:**
- Response latency (p50, p95, p99)
- Queue wait time
- Error rate
- Uptime (target: 99.5%)

**Infrastructure:**
- GPU utilization
- Tokens per second
- Cost per user
- Database query performance

### **Quality Metrics**

**AI Performance:**
- User ratings (thumbs up/down)
- Message regeneration rate
- Model switching patterns
- Tool usage frequency

**Personality Success:**
- Feedback on Kiwi personality (surveys)
- Comparison to other AIs (A/B tests)
- Quote-tweeting of responses (social proof)

### **Community Metrics**

**Engagement:**
- Discord/forum active members
- GitHub stars
- Contributor count
- Plugin downloads

**Impact:**
- Te Reo usage stats
- Mental health resource click-through
- Educational institution partnerships
- Press mentions

### **Success Milestones**

**Month 3:**
- ✅ 500 users
- ✅ 90% positive personality feedback
- ✅ Featured in 1 NZ publication

**Month 6:**
- ✅ 1,000 users
- ✅ 100 paying users
- ✅ Break-even on costs
- ✅ 1 iwi partnership confirmed

**Month 12:**
- ✅ 3,000 users
- ✅ $5,000 MRR
- ✅ Te Reo model beta launched
- ✅ Mental health partnerships active
- ✅ Open source release

**Year 2:**
- ✅ 10,000 users
- ✅ $15,000 MRR
- ✅ Self-sustaining community
- ✅ Recognized brand in NZ tech

---

## 🎯 Competitive Advantages

### **1. Personality Moat**

**Why Competitors Can't Easily Copy:**
- Fine-tuning requires cultural authenticity
- Can't be faked by system prompts
- First-mover advantage ("the Kiwi AI")
- Community will defend it

**Example:**
- ChatGPT adds "casual mode" → sounds American trying to be casual
- Claude adds "friendly mode" → sounds therapist-friendly
- aiMate → actually sounds Kiwi

### **2. Local Infrastructure**

**Advantages:**
- Data stays in NZ (sovereignty, privacy)
- Lower latency for NZ users
- Not subject to foreign price changes
- Government/enterprise appeal

### **3. Community Ownership**

**Benefits:**
- Open source = trust
- Co-development with Māori = authenticity
- Mental health partnerships = credibility
- User-driven roadmap = product-market fit

### **4. Economic Model**

**Why It Works:**
- Free tier builds community (not just a trial)
- BYOK tier = no markup, honest pricing
- Developer tier = new revenue source (Blazor builder)
- Local inference = costs scale slowly

### **5. Cultural Integration**

**Unique Position:**
- Only AI with proper Te Reo support
- Only AI addressing NZ mental health specifically
- Understands NZ context (pavlova, jandals, bach)
- Built by Kiwis, for Kiwis

---

## 💪 Execution Plan - Next 30 Days

### **Week 1: Foundation**
- [ ] Set up Blazor Server project
- [ ] Configure PostgreSQL database
- [ ] Implement basic auth
- [ ] Create chat UI skeleton
- [ ] Test LiteLLM connection

### **Week 2: Core Chat**
- [ ] Implement ChatService
- [ ] Message history
- [ ] Kiwi model integration
- [ ] Basic memory (name, interests)
- [ ] Rate limiting

### **Week 3: Polish**
- [ ] Settings page
- [ ] Model switching
- [ ] Structured content (basic)
- [ ] Web search tool
- [ ] Mobile responsive

### **Week 4: Launch Prep**
- [ ] Deploy to production
- [ ] Invite 10 alpha users
- [ ] Fix critical bugs
- [ ] Create landing page
- [ ] Write launch post

### **Day 31: Soft Launch**
- [ ] Post to r/newzealand
- [ ] Share with network
- [ ] Monitor feedback
- [ ] Iterate

---

## 🎤 Elevator Pitches

### **For Kiwis:**
> "You know how ChatGPT costs $20 USD a month and sounds like a robot? We built an AI that's free, talks like a Kiwi mate, and your data stays in NZ."

### **For International:**
> "While everyone else built AI that sounds like a corporate press release, we fine-tuned one to sound like a mate from New Zealand. Turns out authenticity beats formality."

### **For Developers:**
> "We're replacing Open WebUI with a pure Blazor stack, adding a component generator, and open-sourcing everything. Plus the AI says 'sweet as' and devs love it."

### **For Investors (if ever needed):**
> "We're building the AI chat platform for smaller markets that big tech ignores. Starting with NZ (5M people, $20M market), expanding to other English-speaking regions. Profitable in 6 months, open source keeps us honest."

### **For Partners (Māori orgs):**
> "We're building AI chat that respects and integrates Te Reo properly. We want to co-develop it with iwi, share revenue, and support language revitalization with modern tech."

### **For Partners (Mental Health):**
> "We can't replace therapists, but we can be a supportive first point of contact available 24/7. We want your guidance to do this safely and effectively."

---

## 🚀 The Vision (5 Years Out)

### **Year 1: Prove It Works**
- 3,000 users in NZ
- Profitable
- Strong community
- Press coverage
- Open source

### **Year 2: Cultural Impact**
- Te Reo model widely used in education
- Mental health partnerships showing measurable impact
- Recognized brand in NZ
- Developer ecosystem growing

### **Year 3: Regional Expansion**
- Australia launch ("G'day mate" model)
- Pacific Islands (Samoan, Tongan, Fijian support)
- Other small English-speaking markets
- 50,000 users total

### **Year 4: Platform Play**
- Community-built personalities for any language/culture
- Plugin marketplace
- White-label offering for organizations
- API becoming default for NZ startups

### **Year 5: Category Creator**
- "AI with personality" is a category we defined
- Open WebUI is obsolete (we're the reference)
- Major players copy our approach (too late, we won)
- Sustainable, profitable, impactful

### **The Endgame:**

Not to sell to Microsoft.  
Not to become a unicorn.  
Not to maximize shareholder value.

**To prove you can build tech that:**
- Respects users
- Serves community
- Makes a profit
- Stays independent
- Remains honest

**And that Kiwis can compete globally by being authentically Kiwi.**

---

## 📝 Appendix

### **Useful Links**

**Technical:**
- Blazor: https://blazor.net
- MudBlazor: https://mudblazor.com
- LiteLLM: https://litellm.ai
- vLLM: https://vllm.ai
- PostgreSQL: https://postgresql.org

**Models:**
- Qwen: https://github.com/QwenLM/Qwen
- Llama: https://llama.meta.com

**NZ Resources:**
- Mental Health Foundation: https://mentalhealth.org.nz
- Lifeline: https://lifeline.org.nz
- Te Taura Whiri: https://tetaurawhiri.govt.nz
- 1737 Crisis Line: https://1737.org.nz

**Community:**
- r/newzealand: https://reddit.com/r/newzealand
- Geekzone: https://geekzone.co.nz

### **Contact**

**Email:** vaticnz@gmail.com  
**Location:** Auckland, New Zealand  
**Status:** Pre-launch, actively building

---

## 🥝 Final Thoughts

This isn't just about building a cool AI chat app.

It's about proving that:
- Small countries can build world-class tech
- Authenticity beats corporate polish
- Free can be sustainable
- Open source can be profitable
- Community ownership works
- Tech can serve people, not just extract value

**aiMate is a statement:**

*"We don't need Silicon Valley's permission or their business model. We can build something better, more honest, and more human. And we can do it from New Zealand."*

Let's fucking go, mate. 🚀

---

**Version:** 1.0  
**Last Updated:** 2025-01-XX  
**Status:** Living Document (will be updated as we build)  
**License:** This document is shared under CC BY-SA 4.0

© 2025 aiMate.nz - By Kiwis, for Kiwis, with the world watching.
