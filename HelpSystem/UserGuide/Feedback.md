# Feedback System Guide

> **Tooltip Summary:** "Rate AI responses to improve quality. Your feedback trains better models, identifies issues, and helps prioritize improvements for everyone."

---

## 📖 Why Feedback Matters

Every time you rate an AI response, you're making aiMate better for everyone.

### What Your Feedback Does

**1. Identifies Problems**
```
User rates response: 2/10
Tags: "Inaccurate", "Hallucinated facts"
→ aiMate team investigates
→ Bug found: Model not using knowledge base
→ Fixed for everyone
```

**2. Improves Models**
```
1,000 users rate responses over 2 weeks
→ Export feedback dataset
→ Fine-tune model on highly-rated examples
→ Model quality improves 15%
```

**3. Prioritizes Features**
```
50 users report: "Response too long"
10 users report: "Can't export to PDF"
→ Team prioritizes length control feature
→ Higher impact = built first
```

**4. Tracks Quality Over Time**
```
Week 1 average rating: 6.8/10
Week 4 average rating: 7.9/10
→ Quality improving!
→ Changes are working
```

### Why YOU Should Provide Feedback

**Benefits to you:**
- ✅ Better AI responses (future conversations benefit)
- ✅ Bugs get fixed faster
- ✅ Features you want get prioritized
- ✅ Your voice shapes aiMate's development

**Benefits to community:**
- ✅ Everyone gets better responses
- ✅ Shared learning (your feedback helps others)
- ✅ Open source improvement (feedback data can be shared)

**Time investment:** 5-10 seconds per rating
**Impact:** Massive (especially during alpha testing!)

---

## ⭐ Quick Rating (Thumbs)

The fastest way to provide feedback.

### How to Use

1. **Hover over any AI response**
2. **Click the ⭐ Rate button**
3. **Choose:**
   - 👍 **Good response** (7-10/10)
   - 👎 **Bad response** (1-4/10)
4. **Done!** Takes 2 seconds

### When to Use Quick Rating

**Good responses (👍):**
- ✅ Accurate information
- ✅ Helpful and relevant
- ✅ Well-formatted
- ✅ Appropriate tone
- ✅ Cited sources correctly

**Bad responses (👎):**
- ❌ Factually incorrect
- ❌ Unhelpful or off-topic
- ❌ Poorly formatted
- ❌ Inappropriate tone
- ❌ Hallucinated information

### What Happens

**Your thumbs rating:**
1. Logged to database
2. Aggregated with other ratings
3. Model performance tracked
4. Team reviews low-rated responses weekly

**No detailed comments?** That's okay—volume of ratings matters more than detail for quick patterns.

---

## 📊 Detailed Rating (1-10 Scale)

Provide nuanced feedback with ratings, tags, and comments.

### How to Use

1. **Hover over AI response**
2. **Click ⭐ Rate button**
3. **Click "Detailed Feedback"**
4. **Fill in feedback form:**

   ```
   Rating: [1-10 scale slider]

   Tags: [Select all that apply]
   ☐ Accurate
   ☐ Helpful
   ☐ Well-formatted
   ☐ Good tone
   ☐ Fast response
   ☐ Cited sources

   ☐ Inaccurate
   ☐ Unhelpful
   ☐ Poorly formatted
   ☐ Wrong tone
   ☐ Too slow
   ☐ Hallucinated
   ☐ Ignored context

   Comments: [Optional free-text]
   "Missed the key point about refunds..."

   Expected vs. Actual: [Optional]
   Expected: "Should have mentioned 30-day policy"
   Actual: "Only mentioned 14-day return window"
   ```

5. **Click "Submit"**

### Rating Scale Guide

| Rating | Meaning | When to Use |
|--------|---------|-------------|
| **10** | Perfect | Couldn't be better, exactly what you needed |
| **9** | Excellent | Very good, minor room for improvement |
| **8** | Great | Good response, but could be better |
| **7** | Good | Helpful, but some issues |
| **6** | Okay | Acceptable, but not great |
| **5** | Mediocre | Neither good nor bad, meh |
| **4** | Below Average | Somewhat helpful, significant issues |
| **3** | Poor | Not helpful, major problems |
| **2** | Very Poor | Actively unhelpful or wrong |
| **1** | Terrible | Completely wrong, harmful, or useless |

**Be honest!** Don't inflate ratings. Accurate feedback helps more than nice ratings.

### Feedback Tags

**Positive Tags:**
- ✅ **Accurate** - Information is correct
- ✅ **Helpful** - Answered your question
- ✅ **Well-formatted** - Easy to read, good structure
- ✅ **Good tone** - Appropriate personality/style
- ✅ **Fast response** - Generated quickly
- ✅ **Cited sources** - Referenced knowledge base correctly
- ✅ **Creative** - Novel ideas or approaches
- ✅ **Concise** - No fluff, straight to point

**Negative Tags:**
- ❌ **Inaccurate** - Factually wrong
- ❌ **Unhelpful** - Didn't answer question
- ❌ **Poorly formatted** - Hard to read
- ❌ **Wrong tone** - Inappropriate style
- ❌ **Too slow** - Took too long to generate
- ❌ **Hallucinated** - Made up facts
- ❌ **Ignored context** - Didn't use provided info
- ❌ **Too long** - Unnecessarily verbose
- ❌ **Too short** - Insufficient detail
- ❌ **Repetitive** - Repeated itself

**Select multiple tags** - most responses have both good and bad aspects!

### Comments (Optional but Valuable)

**Good comments:**
```
✅ "Mentioned 14-day returns but our policy is 30 days. Check knowledge base."
✅ "Code example worked perfectly but explanation was too technical for beginners."
✅ "Missed the most important point: we need manager approval for refunds over $100."
```

**Not helpful:**
```
❌ "Bad"
❌ "This sucks"
❌ "Try again"
```

**Be specific:** What was wrong? What would make it better?

### Expected vs. Actual (Advanced)

**Use when AI missed something specific:**

```
Expected: "Should have recommended GPT-4 for this complex reasoning task"
Actual: "Recommended GPT-3.5, which gave poor results"
```

```
Expected: "Should have cited our refund policy from knowledge base"
Actual: "Provided generic refund information instead"
```

**Why this helps:** Shows exactly where AI went wrong, makes fixing easier.

---

## 🏷️ Feedback Tag Templates

Admins can create **tag templates** for consistent feedback collection.

### What Are Tag Templates?

Pre-defined sets of tags for specific use cases.

**Example: "Code Review Template"**
```
Tags:
✅ Correct syntax
✅ Follows best practices
✅ Well-commented
✅ Handles edge cases
❌ Bugs present
❌ Security issues
❌ Performance problems
❌ Poor naming
```

**Example: "Customer Support Template"**
```
Tags:
✅ Polite and professional
✅ Resolved issue
✅ Cited correct policy
✅ Empathetic tone
❌ Unhelpful
❌ Wrong information
❌ Rude or dismissive
❌ Ignored customer context
```

### Using Templates

**When rating:**
1. Click "Use Template" dropdown
2. Select relevant template
3. Tags auto-populate
4. Select applicable tags
5. Add rating and comments

**Benefits:**
- Faster feedback (pre-selected tags)
- Consistent categorization
- Easier to analyze trends
- Custom for your use case

---

## 📈 Feedback Analytics

See how AI is performing over time.

### Your Feedback Dashboard

**Access:** Settings → Feedback → My Ratings

**Shows:**
- Total ratings given
- Average rating you've given
- Most common tags used
- Your most/least rated models
- Feedback contribution rank (leaderboard!)

**Use to:**
- See which models you prefer
- Track your feedback contribution
- Compare your ratings to community average

### Model Performance Dashboard

**Access:** Admin → Feedback Analytics (Admin only)

**Shows:**
- Average rating per model
- Ratings over time (trending up/down?)
- Most common positive/negative tags
- Response time vs. rating correlation
- User satisfaction by workspace/topic

**Charts:**
```
GPT-4:      ████████░░ 8.2/10 (1,234 ratings)
Claude 3:   █████████░ 8.5/10 (987 ratings)
GPT-3.5:    ██████░░░░ 6.8/10 (2,156 ratings)
Llama 3.3:  ███████░░░ 7.4/10 (543 ratings)
```

**Trend:**
```
Week 1: 7.2/10
Week 2: 7.5/10
Week 3: 7.8/10
Week 4: 8.1/10 ↗️ Improving!
```

### Tag Statistics

**See which tags appear most:**

```
Positive:
1. Helpful (1,234 times)
2. Accurate (987 times)
3. Well-formatted (765 times)

Negative:
1. Too long (543 times)
2. Hallucinated (421 times)
3. Ignored context (387 times)
```

**Action items:** Fix top 3 negative tags first.

### Feedback Heatmap

**Identify problem areas:**

```
Topic: Code Review
└─ GPT-4: 8.9/10 ✅
└─ GPT-3.5: 5.2/10 ❌ (Issue found!)

Topic: Creative Writing
└─ GPT-4: 8.7/10 ✅
└─ Claude 3: 9.1/10 ✅ (Best for this!)

Topic: Data Analysis
└─ Claude 3: 8.8/10 ✅
└─ GPT-3.5: 6.1/10 ❌ (Use Claude instead)
```

**Insight:** Recommend best model per task type automatically.

---

## 💾 Exporting Feedback Data

Feedback data can be exported for analysis or model training.

### Export Formats

**Available formats:**
- **CSV** - For spreadsheet analysis
- **Excel (.xlsx)** - With formatting and charts
- **JSON** - For programmatic processing
- **Dataset (JSONL)** - For model fine-tuning

### What's Exported

**Per feedback entry:**
```json
{
  "id": "feedback-12345",
  "timestamp": "2025-11-22T14:30:00Z",
  "userId": "user-abc",
  "messageId": "msg-xyz",
  "rating": 8,
  "tags": ["Helpful", "Accurate", "Well-formatted"],
  "comments": "Great answer but could be more concise",
  "model": "gpt-4",
  "temperature": 0.7,
  "promptTokens": 245,
  "completionTokens": 512,
  "userMessage": "Explain photosynthesis",
  "aiResponse": "Photosynthesis is the process...",
  "knowledgeUsed": ["Biology_101.pdf"],
  "expectedVsActual": {
    "expected": null,
    "actual": null
  }
}
```

### How to Export

**Single workspace:**
1. Workspace → Settings → Export Feedback
2. Choose format
3. Download

**All feedback (Admin):**
1. Admin → Feedback → Export All
2. Set filters (date range, rating range, tags)
3. Choose format
4. Download

### Privacy & Anonymization

**Options when exporting:**
- **Include user IDs** (for internal analysis)
- **Anonymize** (remove all identifying info)
- **Aggregate only** (no individual responses, just stats)

**For model training:** Always use anonymized exports.

**For debugging:** Include user IDs only if necessary, delete after use.

---

## 🎯 Best Practices

### Give Feedback Consistently

**Don't just rate extremes:**
```
❌ Only rate 10/10 and 1/10 responses
✅ Rate everything, use full 1-10 scale
```

**Why:** Middle ratings (5-8) are most useful for improving model.

### Rate Across Models

**Compare models:**
```
Ask same question to GPT-4 and Claude 3
Rate both
→ Team sees which model handles this task better
→ Can recommend model automatically
```

### Be Specific in Comments

**Vague:**
```
❌ "This is wrong"
❌ "Not good"
```

**Specific:**
```
✅ "Said our refund policy is 14 days but it's actually 30 days (see HR Policy v2)"
✅ "Code example has syntax error on line 3: missing closing parenthesis"
```

### Use Expected vs. Actual for Hallucinations

**When AI makes up facts:**
```
Expected: "Should say we're based in Auckland"
Actual: "Said we're based in Wellington (wrong!)"
→ Clear hallucination flagged
→ Team can investigate why
```

### Tag Thoughtfully

**Select relevant tags, not all tags:**
```
❌ Selecting every positive tag (not believable)
✅ 2-3 tags that really apply
```

**Be honest about negatives:**
```
Response was helpful BUT had formatting issues
→ Select: ✅ Helpful, ❌ Poorly formatted
```

### Provide Context in Comments

**Include what you were trying to do:**
```
"I was trying to debug a Python script. The AI suggested adding
a print statement which helped, but didn't address the root cause
(variable scope issue)."
```

**Helps team understand use case and improve for similar scenarios.**

---

## 🚀 Advanced: Feedback-Driven Workflows

### Creating Fine-Tuning Datasets

**Collect highly-rated examples:**

1. Export feedback with rating ≥ 9
2. Filter for specific use case (e.g., "code review")
3. Format as JSONL:
   ```jsonl
   {"prompt": "Review this code...", "completion": "This code..."}
   ```
4. Use for model fine-tuning

**Result:** Model learns from best examples.

### A/B Testing Models

**Compare two models systematically:**

1. Create two identical workspaces
2. Workspace A: GPT-4
3. Workspace B: Claude 3 Opus
4. Ask same questions in both
5. Rate both responses
6. Export and compare statistics

**Find:** Which model is better for YOUR use cases.

### Feedback Loops

**Iterative improvement:**

```
Week 1:
└─ Collect feedback
└─ Identify top 3 issues

Week 2:
└─ Fix issues
└─ Deploy changes

Week 3:
└─ Collect feedback again
└─ Verify ratings improved

Week 4:
└─ Repeat
```

**Continuous improvement based on real user feedback.**

### Feedback-Driven Feature Requests

**Common pattern in comments?**

```
10 users: "Wish I could export conversation as PDF"
→ Team adds PDF export feature
→ Users notified when implemented
```

**Your feedback literally shapes the product roadmap.**

---

## 🔔 Feedback Notifications (Coming Soon)

**Get notified when:**
- Your feedback leads to a fix
- Feature you requested is implemented
- Model you rated highly gets updated
- Issue you reported is resolved

**Opt-in:** Settings → Notifications → Feedback Updates

---

## 💡 Pro Tips

### 1. Rate Immediately

**While it's fresh:**
- ✅ Rate right after you see response
- ❌ Don't wait—you'll forget details

**Why:** Fresh memory = more accurate rating.

### 2. Rate Both Good and Bad

**Don't only report problems:**
```
Good responses need feedback too!
→ Helps identify what works
→ Reinforces positive patterns
```

### 3. Use Regenerate + Rate

**If response is bad:**
1. Regenerate (get new response)
2. Rate both responses
3. Compare: which was better?

**Team sees:** Consistency issues, or second try usually better?

### 4. Screenshot for Visual Issues

**Formatting problems?**
- Take screenshot
- Attach to feedback comment
- Easier to show than describe

### 5. Check Citations Before Rating

**If AI cites sources:**
- Click citation
- Verify it's correct
- Rate accordingly

**AI cited wrong source:** Tag "Hallucinated" even if response seems good.

### 6. Suggest Improvements

**Don't just say what's wrong:**
```
❌ "Too long"
✅ "Too long—could be 3 bullet points instead of 3 paragraphs"
```

**Actionable feedback > vague complaints.**

---

## 🆘 Troubleshooting

### Can't Submit Feedback

**"Feedback failed to save":**
- Check internet connection
- Try again in 30 seconds
- If persists, report bug

**"Feedback button missing":**
- Hover over AI response (not your message)
- Check Settings → Feedback → Enable feedback

### Feedback Not Showing in Analytics

**Wait 5 minutes:**
- Analytics update every 5 minutes
- Refresh dashboard

**Wrong workspace filter:**
- Check if filtering to wrong workspace
- Select "All workspaces"

### Want to Change Rating

**Edit existing feedback:**
1. Click response again
2. Click "Edit Feedback"
3. Update rating/tags/comments
4. Save

**Or delete and re-rate:**
1. Click "Delete Feedback"
2. Rate again with new rating

---

## 📚 Related Guides

**User Guides:**
- [Chat Interface](ChatInterface.md) - Using chat features
- [Settings](Settings.md) - Configure feedback settings

**Admin Guides:**
- [Feedback Analytics](../AdminGuide/FeedbackAnalytics.md) - Review all feedback
- [Model Configuration](../AdminGuide/ModelConfiguration.md) - Adjust models based on feedback

**Advanced Topics:**
- [What are LLMs?](../AdvancedTopics/WhatAreLLMs.md) - Understanding AI models
- [Model Fine-Tuning](../AdvancedTopics/ModelFineTuning.md) - Using feedback for training (coming soon)

---

## 🎉 Every Rating Counts!

You now know:
- ✅ Why feedback matters (improve AI for everyone)
- ✅ Quick rating (thumbs up/down, 2 seconds)
- ✅ Detailed rating (1-10 scale, tags, comments)
- ✅ Feedback tags and templates
- ✅ Analytics and tracking
- ✅ Exporting data
- ✅ Best practices for useful feedback
- ✅ Advanced workflows (A/B testing, fine-tuning datasets)

**Start rating today!** Your feedback makes aiMate better for everyone. 🌟

**Especially during alpha testing—your feedback is GOLD.**

Every rating helps identify bugs, improve models, and prioritize features. Thank you for contributing! 🙏

---

**Previous:** [Knowledge Base](KnowledgeBase.md) | **Next:** [Settings Guide](Settings.md)
