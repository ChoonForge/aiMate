# Quick Reference - Keyboard Shortcuts & Commands

> **Tooltip Summary:** "Master aiMate with keyboard shortcuts. Work faster with quick access to search, chat, and navigation—all without touching the mouse."

---

## ⌨️ Essential Shortcuts

### Global Actions

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+K` (or `Cmd+K`) | **Global Search** | Search conversations, messages, files, knowledge |
| `Ctrl+N` | **New Conversation** | Start fresh chat in current workspace |
| `Ctrl+,` | **Settings** | Open settings panel |
| `Ctrl+/` | **Command Palette** | Quick access to all commands |
| `F1` or `?` | **Help** | Context-sensitive help |
| `Esc` | **Close Modal** | Close any open dialog or panel |

### Chat Interface

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Enter` | **Send Message** | Send your message to AI |
| `Shift+Enter` | **New Line** | Add line break without sending |
| `Ctrl+Enter` | **Force Send** | Send even with Shift modifier |
| `Ctrl+↑` | **Previous Message** | Cycle through your message history (up) |
| `Ctrl+↓` | **Next Message** | Cycle through your message history (down) |
| `Ctrl+L` | **Clear Input** | Clear the message input field |

### Message Actions

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+C` | **Copy** | Copy selected message to clipboard |
| `Ctrl+R` | **Regenerate** | Regenerate the last AI response |
| `Ctrl+E` | **Edit Message** | Edit your last message and regenerate |
| `Ctrl+D` | **Delete** | Delete selected message |
| `Ctrl+S` | **Save** | Save conversation (auto-saves anyway) |

### Navigation

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+B` | **Toggle Sidebar** | Show/hide workspace sidebar |
| `Ctrl+Shift+K` | **Knowledge Base** | Open knowledge base panel |
| `Ctrl+Shift+F` | **Files** | Open file manager |
| `Ctrl+Shift+W` | **Workspaces** | Switch workspace (quick switcher) |
| `Ctrl+Tab` | **Next Workspace** | Cycle to next workspace |
| `Ctrl+Shift+Tab` | **Previous Workspace** | Cycle to previous workspace |

### Selection & Editing

| Shortcut | Action | Description |
|----------|--------|-------------|
| `Ctrl+A` | **Select All** | Select all text in input |
| `Ctrl+Z` | **Undo** | Undo last edit |
| `Ctrl+Y` | **Redo** | Redo last undone edit |
| `Ctrl+X` | **Cut** | Cut selected text |
| `Ctrl+V` | **Paste** | Paste from clipboard |

---

## 🎯 Quick Actions Menu

**Right-click** on messages, workspaces, or knowledge items for context menus:

### On Messages
- Copy Message
- Regenerate Response
- Edit and Regenerate
- Rate Response
- Delete Message
- Copy as Markdown
- Export Conversation

### On Workspaces
- Rename Workspace
- Change Personality
- Workspace Settings
- Archive Workspace
- Delete Workspace
- Export All Conversations

### On Knowledge Items
- Edit Content
- Add Tags
- View Details
- Delete Item
- Export as File

---

## 💬 Chat Commands

Type these in the chat input for quick actions:

### Workspace Commands

| Command | Action |
|---------|--------|
| `/new workspace` | Create new workspace |
| `/switch [name]` | Switch to workspace |
| `/rename [name]` | Rename current workspace |
| `/settings` | Open workspace settings |

### Knowledge Commands

| Command | Action |
|---------|--------|
| `/upload` | Upload document to knowledge base |
| `/search [query]` | Search knowledge base |
| `/knowledge` | Open knowledge base panel |

### Model Commands

| Command | Action |
|---------|--------|
| `/model [name]` | Switch AI model |
| `/models` | List available models |
| `/temp [0-2]` | Set temperature (creativity) |

### Utility Commands

| Command | Action |
|---------|--------|
| `/clear` | Clear current conversation |
| `/export` | Export conversation |
| `/help` | Open help documentation |
| `/feedback` | Submit feedback |
| `/about` | About aiMate |

---

## 🔧 Settings Quick Access

### User Settings Sections

| Tab | Shortcut | What's There |
|-----|----------|--------------|
| **General** | `Ctrl+, → G` | Theme, language, notifications |
| **Models** | `Ctrl+, → M` | Default model, temperature, tokens |
| **Connections** | `Ctrl+, → C` | API keys, LiteLLM config, BYOK |
| **Tools** | `Ctrl+, → T` | MCP tools, plugins |
| **Advanced** | `Ctrl+, → A` | Debug mode, data management |
| **Account** | `Ctrl+, → U` | Profile, password, API keys |

---

## 🚀 Power User Tips

### Workflow Optimization

**1. Keyboard-Only Navigation**
```
Ctrl+K → Type search → Enter → Start chatting
(Never touch mouse!)
```

**2. Quick Workspace Switch**
```
Ctrl+Shift+W → Type workspace name → Enter
(Fuzzy search works: "cod" finds "Coding Projects")
```

**3. Rapid Message Editing**
```
Ctrl+↑ → Edit previous message → Enter
(Faster than scrolling and clicking edit)
```

**4. Knowledge Search from Chat**
```
Type message mentioning topic → Ctrl+Shift+K
(Opens knowledge with search pre-filled)
```

### Multi-Select Actions

Hold `Shift` to select multiple items:

- **Messages:** Select range of messages → Delete/Export batch
- **Knowledge:** Select multiple items → Bulk tag/delete
- **Workspaces:** Select multiple → Bulk archive

### Custom Shortcuts (Coming Soon)

**Settings → Keyboard → Customize**
- Remap any shortcut
- Create custom commands
- Import/export shortcut profiles

---

## 📋 Markdown Formatting in Chat

Use markdown for formatted messages:

| Syntax | Result |
|--------|--------|
| `**bold**` | **bold** |
| `*italic*` | *italic* |
| `` `code` `` | `code` |
| `[link](url)` | [link](url) |
| `# Heading` | Heading |
| `- List item` | • List item |
| `1. Numbered` | 1. Numbered |
| `` ```code block``` `` | Syntax-highlighted code |

**Pro Tip:** Use code blocks for better AI code responses:
```
Write a Python function:
```python
# AI will understand you want Python specifically
```
(Just type the opening backticks, language name, and AI will format the response correctly)

---

## 🎨 Personality Quick Switch

**From Chat Interface:**

| Keys | Personality |
|------|-------------|
| `Alt+1` | Kiwi Mate |
| `Alt+2` | Kiwi Professional |
| `Alt+3` | Kiwi Dev |
| `Alt+4` | Te Reo Māori |
| `Alt+5` | Mental Health Guardian |
| `Alt+6` | Standard |

**Or:** Right-click workspace → Change Personality

---

## 🔍 Advanced Search Operators

**In Global Search (Ctrl+K):**

| Operator | Example | Finds |
|----------|---------|-------|
| `@` | `@workspace:coding` | Items in "coding" workspace |
| `#` | `#important` | Items tagged "important" |
| `type:` | `type:knowledge` | Only knowledge items |
| `from:` | `from:me` | Only your messages |
| `date:` | `date:today` | Items from today |
| `has:` | `has:attachment` | Messages with files |

**Combine operators:**
```
type:knowledge #python date:this-week
→ Knowledge items tagged "python" from this week
```

---

## ⚡ Speed Tricks

### 1. Message Input Shortcuts

**While typing:**
- `Tab` - Autocomplete (if suggestions available)
- `Ctrl+Space` - Show autocomplete suggestions
- `Ctrl+K` (in input) - Insert knowledge reference
- `Ctrl+@` - Mention workspace/user

### 2. File Attachment Shortcuts

**Drag & Drop:**
- Drag file to chat → Auto-attach
- Drag to knowledge base → Auto-upload
- Drag multiple files → Batch upload

**Quick Attach:**
- `Ctrl+U` - Upload from computer
- `Ctrl+Shift+U` - Attach from knowledge base
- `Ctrl+Alt+U` - Attach recent files

### 3. Response Actions

**Hover over any message:**
- `C` - Copy
- `R` - Regenerate
- `E` - Edit
- `D` - Delete
- `S` - Star/favorite
- `F` - Provide feedback

---

## 🎯 Focus Mode

**Distraction-Free Chatting:**

1. Press `Ctrl+Shift+F` (Focus Mode)
2. Hides sidebar, shows only chat
3. Press again to restore

**Or:** Settings → Interface → Enable Focus Mode

---

## 📱 Mobile Shortcuts

**On Mobile/Tablet:**

### Gestures

| Gesture | Action |
|---------|--------|
| **Swipe Left** (on message) | Show actions (copy, delete, rate) |
| **Swipe Right** (on sidebar) | Open workspace |
| **Long Press** (message) | Select message |
| **Pull Down** (in chat) | Load older messages |
| **Pull Up** (in chat) | Scroll to latest |

### Mobile Menu

Tap `☰` (menu icon) for:
- Workspaces
- Knowledge Base
- Settings
- Help

---

## 🆘 Troubleshooting Shortcuts

| Issue | Quick Fix |
|-------|-----------|
| **Shortcut not working** | Check if focus is in text input (click input first) |
| **Modal won't close** | Press `Esc` multiple times |
| **Can't find command** | Press `Ctrl+/` for command palette |
| **Search not finding** | Check filters in search panel |
| **Sidebar disappeared** | Press `Ctrl+B` to toggle back |

---

## 📚 Learn More

**Detailed Guides:**
- [Chat Interface](../UserGuide/ChatInterface.md) - Full chat feature documentation
- [Workspaces](../UserGuide/Workspaces.md) - Workspace management
- [Knowledge Base](../UserGuide/KnowledgeBase.md) - Knowledge management
- [Settings](../UserGuide/Settings.md) - All settings explained

**Advanced:**
- [Prompt Engineering](../AdvancedTopics/PromptEngineeringBasics.md) - Write better prompts
- [Keyboard Customization](../UserGuide/Settings.md#keyboard) - Custom shortcuts (coming soon)

---

## 💡 Printable Cheat Sheet

**Essential Shortcuts Only:**

```
┌─────────────────────────────────────────────────────────┐
│              aiMate Quick Reference                      │
├─────────────────────────────────────────────────────────┤
│ GLOBAL                                                  │
│   Ctrl+K        Search                                  │
│   Ctrl+N        New Conversation                        │
│   Ctrl+,        Settings                                │
│   Esc           Close Modal                             │
├─────────────────────────────────────────────────────────┤
│ CHAT                                                    │
│   Enter         Send Message                            │
│   Shift+Enter   New Line                                │
│   Ctrl+R        Regenerate                              │
│   Ctrl+↑↓       Message History                         │
├─────────────────────────────────────────────────────────┤
│ NAVIGATION                                              │
│   Ctrl+B        Toggle Sidebar                          │
│   Ctrl+Shift+K  Knowledge Base                          │
│   Ctrl+Shift+W  Switch Workspace                        │
├─────────────────────────────────────────────────────────┤
│ MESSAGE ACTIONS (Hover over message)                    │
│   C             Copy                                    │
│   R             Regenerate                              │
│   E             Edit                                    │
│   D             Delete                                  │
│   F             Feedback                                │
└─────────────────────────────────────────────────────────┘
```

**Print this page:** `Ctrl+P` → Save as PDF or print

---

## 🎓 Mastery Checklist

Track your progress:

- [ ] Used `Ctrl+K` to search
- [ ] Created conversation with `Ctrl+N`
- [ ] Sent message with `Enter`
- [ ] Added line break with `Shift+Enter`
- [ ] Regenerated response with `Ctrl+R`
- [ ] Edited message with `Ctrl+↑`
- [ ] Switched workspace with `Ctrl+Shift+W`
- [ ] Toggled sidebar with `Ctrl+B`
- [ ] Used command palette `Ctrl+/`
- [ ] Tried a chat command (e.g., `/model`)
- [ ] Used markdown formatting in chat
- [ ] Attached file with drag-and-drop
- [ ] Rated a response with hover action
- [ ] Customized a shortcut (when available)

**Completed all?** You're an aiMate power user! 🚀

---

**Next:** [Chat Interface Guide](../UserGuide/ChatInterface.md) - Deep dive on chat features
