# Component Architecture & Technical Specification
**AI Chat Interface - Complete Build Specification**

Version: 1.0
Last Updated: November 2025
Purpose: Complete technical specification for rebuilding the application without visual reference

---

## Table of Contents

1. [Application Overview](#application-overview)
2. [Technology Stack](#technology-stack)
3. [Overall Layout Structure](#overall-layout-structure)
4. [Page Hierarchy](#page-hierarchy)
5. [Navigation & Sidebar](#navigation-sidebar)
6. [Chat Interface](#chat-interface)
7. [Modal System](#modal-system)
8. [Settings Modal (6 Tabs)](#settings-modal)
9. [Input System & Context Attachments](#input-system)
10. [Message Display](#message-display)
11. [Structured Content Rendering](#structured-content)
12. [Component Inventory](#component-inventory)
13. [Design System & Theming](#design-system)
14. [State Management](#state-management)
15. [API Integration Points](#api-integration)

---

## 1. Application Overview {#application-overview}

### Purpose
Full-featured AI chat interface with conversation management, multi-model support, structured content rendering, knowledge attachment system, and comprehensive settings.

### Key Features
- Multi-conversation management with sidebar navigation
- Real-time chat with AI models
- File/document/knowledge/webpage attachment system
- Structured content rendering (tables, forms, key-value pairs, lists)
- Model selection and management
- Tool integration (MCP, web search, code interpreter)
- Comprehensive 6-tab settings modal
- Dark/light theme with multiple color schemes
- Responsive design (mobile, tablet, desktop)
- Drag-and-drop file uploads
- Smart knowledge suggestions based on input

---

## 2. Technology Stack {#technology-stack}

### Core
- **Framework**: React 18+ with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS v4.0
- **UI Components**: shadcn/ui (45 components)
- **Icons**: lucide-react

### Key Libraries
- **Markdown**: react-markdown with remark-gfm
- **Code Highlighting**: react-syntax-highlighter
- **Notifications**: sonner@2.0.3
- **Gestures**: Custom swipe gesture hook for mobile

### File Structure
```
/
├── App.tsx                          # Main application wrapper
├── main.tsx                         # Entry point
├── index.html                       # HTML template
├── components/
│   ├── AboutModal.tsx              # About dialog
│   ├── AdminModal.tsx              # Admin interface
│   ├── ArchivedModal.tsx           # Archive management
│   ├── AttachedContext.tsx         # Context chips display
│   ├── BaseDialog.tsx              # Base dialog wrapper
│   ├── BaseModal.tsx               # Base modal with tabs
│   ├── ChatHeader.tsx              # Header with model selector
│   ├── ChatInput.tsx               # Message input component
│   ├── ChatMessage.tsx             # Message bubble
│   ├── ConnectionEditDialog.tsx    # API connection editor
│   ├── ConversationSidebar.tsx     # Left sidebar navigation
│   ├── DebugContext.tsx            # Debug context provider
│   ├── DebugPanel.tsx              # Debug overlay
│   ├── EmptyState.tsx              # Empty conversation state
│   ├── FilesModal.tsx              # File management
│   ├── HelpModal.tsx               # Help/documentation
│   ├── KnowledgeModal.tsx          # Knowledge base management
│   ├── KnowledgeSuggestions.tsx    # Smart suggestions
│   ├── MCPEditDialog.tsx           # MCP tool configuration
│   ├── ModelEditDialog.tsx         # Model settings editor
│   ├── NotesModal.tsx              # Notes management
│   ├── PluginEditDialog.tsx        # Plugin configuration
│   ├── ProjectModal.tsx            # Project/workspace management
│   ├── RatingModal.tsx             # Message rating dialog
│   ├── SearchModal.tsx             # Search interface
│   ├── SettingsModal.tsx           # Main settings (6 tabs)
│   ├── ShareDialog.tsx             # Share conversation
│   ├── ShareModal.tsx              # Share options
│   ├── StructuredPanel.tsx         # Structured content renderer
│   ├── ThemeProvider.tsx           # Theme context
│   ├── UsageDetailsDialog.tsx      # Usage analytics
│   └── ui/                         # shadcn/ui components (45)
├── styles/
│   └── globals.css                 # Global styles & CSS variables
├── utils/
│   ├── api-stubs.ts                # API stub implementations
│   └── useSwipeGesture.ts          # Mobile swipe hook
└── ...config files
```

---

## 3. Overall Layout Structure {#overall-layout-structure}

### Root Layout
```
┌─────────────────────────────────────────────────────────────┐
│  <App /> (Full Viewport)                                    │
│  ┌───────────────┬──────────────────────────────────────┐  │
│  │               │                                      │  │
│  │  Sidebar      │      Main Chat Area                 │  │
│  │  (320px)      │                                      │  │
│  │               │  ┌────────────────────────────────┐ │  │
│  │               │  │  ChatHeader                    │ │  │
│  │               │  │  (Height: 64px)                │ │  │
│  │               │  └────────────────────────────────┘ │  │
│  │               │                                      │  │
│  │               │  ┌────────────────────────────────┐ │  │
│  │               │  │  Messages Area                 │ │  │
│  │               │  │  (Scrollable, flex-1)          │ │  │
│  │               │  │                                 │ │  │
│  │               │  │  - EmptyState OR               │ │  │
│  │               │  │  - Message list                │ │  │
│  │               │  │                                 │ │  │
│  │               │  └────────────────────────────────┘ │  │
│  │               │                                      │  │
│  │               │  ┌────────────────────────────────┐ │  │
│  │               │  │  ChatInput                     │ │  │
│  │               │  │  (Auto-height, min-h-[96px])   │ │  │
│  │               │  └────────────────────────────────┘ │  │
│  │               │                                      │  │
│  │               │  ┌────────────────────────────────┐ │  │
│  │               │  │  DebugPanel (optional)         │ │  │
│  │               │  └────────────────────────────────┘ │  │
│  └───────────────┴──────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Responsive Behavior

**Desktop (≥1024px)**
- Sidebar: Permanent, 320px width, can be toggled
- Layout: Side-by-side flex layout
- Sidebar visibility controlled by state variable `sidebarOpen`

**Tablet/Mobile (<1024px)**
- Sidebar: Hidden by default, opens as overlay Sheet
- Layout: Full-width chat area
- Sidebar accessed via hamburger menu in ChatHeader
- Uses Sheet component from shadcn/ui for mobile sidebar

### Layout Specifications

**Dimensions**:
- Sidebar width: `320px` (fixed)
- Max content width: `1024px` (4xl) for messages
- Header height: `64px`
- Input min height: `96px` (expands with content)
- Border radius (main containers): `24px` (rounded-3xl)
- Border radius (buttons): `12px` (rounded-xl) or `50%` (circular)

**Colors (CSS Variables)**:
- Background: `bg-gray-50 dark:bg-gray-950`
- Surface: `bg-white dark:bg-gray-900`
- Borders: `border-gray-200 dark:border-gray-800`
- Text: `text-gray-900 dark:text-gray-100`
- Muted text: `text-gray-500 dark:text-gray-400`

**Spacing**:
- Padding (containers): `p-4` to `p-6`
- Gap (flex containers): `gap-2` to `gap-6`
- Message spacing: `space-y-6`

---

## 4. Page Hierarchy {#page-hierarchy}

### Route Structure

Application is single-page with modal overlays:

```
/ (Root - Main Chat Interface)
├── Conversations Sidebar (toggleable)
├── Chat Area
│   ├── Empty State (no conversation selected)
│   └── Active Conversation View
├── Modals (overlays):
│   ├── Settings Modal
│   ├── Admin Modal
│   ├── Knowledge Modal
│   ├── Files Modal
│   ├── Notes Modal
│   ├── Projects Modal
│   ├── Search Modal
│   ├── Share Modal
│   ├── About Modal
│   ├── Help Modal
│   ├── Archived Modal
│   └── Rating Modal
└── Dialogs (smaller overlays):
    ├── Connection Edit Dialog
    ├── Model Edit Dialog
    ├── MCP Edit Dialog
    ├── Plugin Edit Dialog
    ├── Share Dialog
    └── Usage Details Dialog
```

### State Structure

```typescript
interface AppState {
  // Conversations
  conversations: ConversationData[]
  activeConversationId: string | null
  
  // UI State
  sidebarOpen: boolean           // Desktop sidebar
  mobileSidebarOpen: boolean     // Mobile sheet
  isTyping: boolean              // AI response in progress
  
  // Model Management
  selectedModel: string
  enabledModels: Record<string, boolean>
  
  // Modal States (all boolean)
  settingsOpen: boolean
  adminOpen: boolean
  knowledgeOpen: boolean
  filesOpen: boolean
  notesOpen: boolean
  projectsOpen: boolean
  searchOpen: boolean
  shareOpen: boolean
  aboutOpen: boolean
  helpOpen: boolean
  archivedOpen: boolean
  ratingOpen: boolean
}

interface ConversationData {
  id: string
  title: string
  messages: Message[]
  createdAt: Date
}

interface Message {
  id: string
  role: "user" | "assistant"
  content: string
  timestamp: string
  structuredContent?: StructuredData
}
```

---

## 5. Navigation & Sidebar {#navigation-sidebar}

### ConversationSidebar Component

**Purpose**: Left sidebar for conversation management and navigation

**Dimensions**:
- Width: `320px` (w-80)
- Height: `100vh` (full height)
- Scrollable content area

**Structure**:
```
┌─────────────────────────────────┐
│  Header                         │
│  ┌───────────────────────────┐  │
│  │  Logo / Brand             │  │
│  │  (h-16)                   │  │
│  └───────────────────────────┘  │
│                                 │
│  New Chat Button                │
│  ┌───────────────────────────┐  │
│  │  [+] New Chat             │  │
│  │  (full width, h-10)       │  │
│  └───────────────────────────┘  │
│                                 │
│  Conversations List             │
│  (Scrollable)                   │
│  ┌───────────────────────────┐  │
│  │  Conversation Item 1      │  │
│  │  - Title                  │  │
│  │  - Last message preview   │  │
│  │  - Timestamp              │  │
│  │  - Actions menu (...)     │  │
│  ├───────────────────────────┤  │
│  │  Conversation Item 2      │  │
│  ├───────────────────────────┤  │
│  │  ...                      │  │
│  └───────────────────────────┘  │
│                                 │
│  Footer / Quick Actions         │
│  ┌───────────────────────────┐  │
│  │  Models Section           │  │
│  │  - Toggle model           │  │
│  │    visibility             │  │
│  │                           │  │
│  │  Bottom Actions           │  │
│  │  ┌─────┬─────┬─────┬────┐│  │
│  │  │Icon │Icon │Icon │... ││  │
│  │  └─────┴─────┴─────┴────┘│  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
```

**Conversation Item Specs**:
- Height: `min-h-[72px]`
- Padding: `p-3`
- Hover state: `hover:bg-gray-100 dark:hover:bg-gray-800`
- Active state: `bg-purple-50 dark:bg-purple-950/20 border-l-4 border-purple-500`
- Border radius: `rounded-lg`

**Conversation Item Components**:
1. **Title** (truncated to 1 line)
   - Font: `font-medium`
   - Size: `text-sm`
   - Color: `text-gray-900 dark:text-gray-100`

2. **Last Message Preview** (truncated to 1 line)
   - Font: `font-normal`
   - Size: `text-xs`
   - Color: `text-gray-500 dark:text-gray-400`
   - Max width: `truncate`

3. **Timestamp** (relative or absolute)
   - Size: `text-xs`
   - Color: `text-gray-400 dark:text-gray-500`
   - Examples: "2 mins ago", "Yesterday", "Dec 15"

4. **Actions Dropdown** (three dots menu)
   - Trigger: MoreHorizontal icon button
   - Position: Absolute top-right of item
   - Appears on hover or always on mobile
   - Items:
     - Rename
     - Clone
     - Archive
     - Share
     - Delete (destructive, red)

**Footer Section**:
- Height: `auto`
- Sticky at bottom: `sticky bottom-0`
- Background: `bg-white dark:bg-gray-900`
- Border top: `border-t border-gray-200 dark:border-gray-800`
- Padding: `p-4`

**Bottom Action Icons** (horizontal row):
- Settings (Settings icon)
- Admin (Shield icon)
- Projects (FolderKanban icon)
- Knowledge (Brain icon)
- Files (FileText icon)
- Notes (StickyNote icon)
- Search (Search icon)
- Help (HelpCircle icon)
- More menu (MoreHorizontal)

Each icon button:
- Size: `h-9 w-9`
- Icon size: `h-4 w-4`
- Rounded: `rounded-lg`
- Hover: `hover:bg-gray-100 dark:hover:bg-gray-800`
- Tooltip on hover

**Models Toggle Section**:
Located above bottom actions
- Title: "Enabled Models"
- List of available models with toggle switches
- Each item:
  - Model name
  - Switch component (shadcn/ui)
  - Disabled models are hidden from model selector

### Mobile Sidebar (Sheet)

**Trigger**: Hamburger menu button in ChatHeader

**Implementation**: shadcn/ui `Sheet` component
- Side: `left`
- Width: `320px` (w-80)
- Full height overlay
- Swipe to close gesture support
- Backdrop blur

**Behavior**:
- Opens with slide-in animation
- Auto-closes after selecting conversation
- Closes on backdrop click

---

## 6. Chat Interface {#chat-interface}

### ChatHeader Component

**Dimensions**:
- Height: `64px` (h-16)
- Full width
- Sticky top: `sticky top-0 z-10`
- Background: `bg-white/90 dark:bg-gray-900/90` (frosted glass)
- Backdrop blur: `backdrop-blur-md`
- Border bottom: `border-b border-gray-200 dark:border-gray-800`

**Structure**:
```
┌────────────────────────────────────────────────────────────┐
│  [☰] [Logo]               [Model ▼]      [⊕] [...] [⚙]   │
│  Menu Toggle   Brand      Selector       New  More  Settings
└────────────────────────────────────────────────────────────┘
```

**Elements (left to right)**:

1. **Menu Toggle Button** (mobile only, <768px)
   - Icon: Menu (hamburger) icon
   - Size: `h-9 w-9`
   - Rounded: `rounded-lg`
   - Shows/hides: `md:hidden` (hidden on desktop)
   - Action: Opens mobile sidebar

2. **Logo/Brand** (optional)
   - Icon: Sparkles with gradient background
   - Size: `h-8 w-8`
   - Gradient: `bg-gradient-to-r from-purple-600 to-blue-500`
   - Rounded: `rounded-full`
   - Sparkles icon: `h-4 w-4 text-white`

3. **Spacer** (flex-1)

4. **Model Selector Dropdown**
   - Component: shadcn/ui Select
   - Width: `w-[200px]`
   - Current model display
   - Dropdown shows all enabled models
   - Each model item shows:
     - Model name
     - Provider/connection badge
     - Color indicator
   - Keyboard navigation support

5. **New Chat Button**
   - Icon: Plus icon
   - Size: `h-9 w-9`
   - Tooltip: "New Chat"
   - Action: Creates new conversation

6. **More Menu Dropdown**
   - Icon: MoreHorizontal
   - Size: `h-9 w-9`
   - Popover menu items:
     - Share
     - Export
     - Archive
     - Settings

7. **Settings Button**
   - Icon: Settings icon
   - Size: `h-9 w-9`
   - Tooltip: "Settings"
   - Action: Opens Settings Modal

**Responsive Behavior**:
- Desktop: All elements visible
- Tablet: Model selector width reduces
- Mobile: Hamburger menu appears, some buttons hide

### Messages Area

**Container Specs**:
- Max width: `1024px` (max-w-4xl)
- Centered: `mx-auto`
- Padding: `px-4 py-6`
- Vertical spacing: `space-y-6`
- Scrollable: Uses shadcn/ui ScrollArea

**Empty State Display** (no messages):
- Component: EmptyState.tsx
- Centered vertically and horizontally
- Contains:
  - Large icon (Sparkles with gradient)
  - Welcome title
  - Subtitle text
  - Suggested prompts (4-6 clickable cards)
  
**Suggested Prompt Card**:
- Dimensions: `p-4`
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-xl`
- Hover: `hover:border-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/20`
- Cursor: `cursor-pointer`
- Icon + text layout

**Message Rendering** (has messages):
- Each message is a ChatMessage component
- Vertical stack with `space-y-6`
- Auto-scroll to bottom on new message
- Infinite scroll support (future)

**Typing Indicator** (AI responding):
```
┌────────────────────────────────────┐
│  [Avatar]  AI is thinking...       │
│  (pulsing  ● ● ●                   │
│   purple)  (animated bounce)       │
└────────────────────────────────────┘
```

- Avatar: Pulsing purple gradient circle
- Text: "AI is thinking"
- Animated dots: 3 dots with staggered bounce
- Height: `h-12`

---

## 7. Modal System {#modal-system}

### BaseModal Component

**Purpose**: Reusable modal wrapper with tab support

**Specifications**:

**Sizes**:
- `md`: `max-w-2xl` (~672px)
- `lg`: `max-w-2xl lg:max-w-5xl` (~672px mobile, ~1280px desktop)
- `xl`: `max-w-2xl lg:max-w-7xl` (~672px mobile, ~1536px desktop)

**Dimensions**:
- Mobile: `w-full h-full` (fullscreen)
- Desktop: `w-[80vw] max-h-[90vh] h-[90vh]`
- Border radius: `rounded-xl`
- Background: `bg-white dark:bg-gray-900`
- Border: `border border-gray-200 dark:border-gray-800`

**Structure**:
```
┌────────────────────────────────────────────────────────┐
│  Header (fixed)                                     [X]│
│  ┌──────────────────────────────────────────────────┐  │
│  │  [Icon] Title                                    │  │
│  │  Description text                                │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  Tabs (horizontal scroll, fixed)                      │
│  ┌──────────────────────────────────────────────────┐  │
│  │  [Icon] Tab 1  |  [Icon] Tab 2  |  [Icon] Tab 3 │  │
│  │  ─────────────                                   │  │
│  │  (active underline)                              │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  Content Area (scrollable, flex-1)                    │
│  ┌──────────────────────────────────────────────────┐  │
│  │                                                  │  │
│  │  [Tab content rendered here]                    │  │
│  │                                                  │  │
│  │                                                  │  │
│  │                                                  │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  Footer (fixed)                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │  [Delete]                    [Cancel]  [Save]    │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

**Header Section**:
- Height: `auto` (min `h-[72px]`)
- Padding: `px-6 py-4`
- Border bottom: `border-b border-gray-200 dark:border-gray-800`
- Fixed at top
- Contains:
  - Icon (left, colored)
  - Title (font-semibold)
  - Description (text-sm, muted)
  - Close button (X, top-right corner)

**Tab Strip** (if tabs provided):
- Height: `h-12`
- Padding: `px-6`
- Border bottom: `border-b border-gray-200 dark:border-gray-800`
- Horizontal scroll: `overflow-x-auto`
- Fixed below header
- Tab scrollbar: Custom thin scrollbar
- Gap between tabs: `gap-1`

**Individual Tab Button**:
- Padding: `px-4 py-2`
- Border bottom: `border-b-2`
- Active: `border-purple-500 text-purple-600 dark:text-purple-400`
- Inactive: `border-transparent text-gray-500 dark:text-gray-400`
- Hover: `hover:text-gray-700 dark:hover:text-gray-300`
- Transition: `transition-colors`
- Whitespace: `whitespace-nowrap`
- Layout: Icon (if provided) + label

**Content Area**:
- Flex: `flex-1` (takes remaining space)
- Overflow: Uses shadcn/ui ScrollArea
- Padding: `px-6 py-4`
- Vertical spacing: `space-y-4`
- Swipe gestures enabled (mobile):
  - Swipe left: Next tab
  - Swipe right: Previous tab

**Footer Section** (if save/delete buttons):
- Height: `h-[60px]` (fixed)
- Padding: `px-6`
- Border top: `border-t border-gray-200 dark:border-gray-800`
- Layout: `flex justify-between items-center`
- Left side: Delete button (if showDelete)
- Right side: Cancel + Save buttons

**Buttons**:
- Delete: `variant="destructive"` (red)
- Cancel: `variant="outline"` (gray)
- Save: `variant="default"` (purple/primary)
- Gap: `gap-2`

**Mobile Adaptations**:
- Fullscreen on mobile (<640px)
- Tab strip horizontal scroll
- Swipe between tabs
- Footer buttons may stack on very small screens

---

## 8. Settings Modal (6 Tabs) {#settings-modal}

### Modal Specifications

**Component**: SettingsModal.tsx  
**Base**: Extends BaseModal  
**Size**: `lg` (max-w-5xl on desktop)  
**Icon**: Settings icon (gear)  
**Title**: "Settings"  
**Description**: "Manage your application settings and preferences"

### Tab Structure

**6 Tabs**:
1. General (Settings icon)
2. Interface (Palette icon)
3. Connections (Link2 icon)
4. Personalisation (Sparkles icon)
5. Account (User icon)
6. Usage (BarChart3 icon)

---

### Tab 1: General

**Content Sections**:

**1. WebUI Settings**
- **Language Selector**
  - Label: "Language"
  - Component: Select dropdown
  - Width: `w-[180px]`
  - Options:
    - English (GB)
    - English (US)
    - Spanish
    - French
    - German
  - Default: "English (GB)"

- **Notifications Selector**
  - Label: "Notifications"
  - Component: Select dropdown
  - Width: `w-[180px]`
  - Options:
    - Off
    - On
    - Mentions Only
  - Default: "Off"

**Separator** (horizontal line)

**2. System Prompt**
- **Title**: "System Prompt"
- **Description**: "Your name is Echo and you are an AI assistant, my name is Rich."
- **Textarea**:
  - Placeholder: "Enter your system prompt..."
  - Min height: `min-h-[240px]`
  - Font: `font-mono text-sm` (monospace for code-like appearance)
  - Default value: Culture/location context example
  - Spellcheck: disabled

**Layout**:
- Each setting row: `flex justify-between items-center`
- Vertical spacing: `space-y-4`
- Section spacing: `space-y-6`

---

### Tab 2: Interface

**Content Sections**:

**1. Theme**
- **Appearance Selector**
  - Label: "Appearance"
  - Component: Select
  - Options: Light, Dark, System
  - Default: "Dark"
  - Live preview on change

- **Color Theme Selector**
  - Label: "Color Theme"
  - Component: Select
  - Options: Purple, Blue, Green, Orange
  - Default: "Purple"
  - Changes accent colors throughout app

- **Font Size Selector**
  - Label: "Font Size"
  - Component: Select
  - Options: Small, Medium, Large
  - Default: "Medium"
  - Affects all text globally

**Separator**

**2. Chat Display**
- **Show Timestamps** (Switch)
  - Label: "Show timestamps"
  - Description: "Display message timestamps"
  - Default: On

- **Syntax Highlighting** (Switch)
  - Label: "Syntax highlighting"
  - Description: "Highlight code in messages"
  - Default: On

- **Markdown Support** (Switch)
  - Label: "Markdown support"
  - Description: "Render markdown formatting"
  - Default: On

**Layout**:
- Setting rows with description:
  ```
  [Label                    ] [Control]
  [Description text (small) ]
  ```
- Description: `text-sm text-gray-500 dark:text-gray-400 mt-1`

---

### Tab 3: Connections

**Content Sections**:

**1. API Connections**
- **Title**: "API Connections"
- **Description**: "Configure API endpoints and authentication"

**Input Fields** (vertical stack):

- **OpenAI API Key**
  - Label: "OpenAI API Key"
  - Input type: `password`
  - Placeholder: "sk-..."
  - Width: Full
  - Margin top: `mt-2`

- **Anthropic API Key**
  - Label: "Anthropic API Key"
  - Input type: `password`
  - Placeholder: "sk-ant-..."
  - Width: Full
  - Margin top: `mt-2`

- **Ollama Base URL**
  - Label: "Ollama Base URL"
  - Input type: `url`
  - Placeholder: "http://localhost:11434"
  - Width: Full
  - Margin top: `mt-2`

**Spacing**: `space-y-4` between fields

**Future Enhancement**: 
- Add "Test Connection" button for each
- Connection status indicators (green/red dot)
- Edit/Delete buttons for saved connections
- "Add New Connection" button

---

### Tab 4: Personalisation

**Content Sections**:

**1. AI Behavior**

- **Creativity Level**
  - Label: "Creativity Level"
  - Component: Select
  - Options: Precise, Balanced, Creative
  - Default: "Balanced"
  - Margin top: `mt-2`

- **Response Style**
  - Label: "Response Style"
  - Component: Select
  - Options: Concise, Balanced, Detailed
  - Default: "Balanced"
  - Margin top: `mt-2`

**Separator**

**2. Custom Instructions**

- **Additional Context Textarea**
  - Label: "Additional Context"
  - Description: "Customize how the AI responds to you"
  - Placeholder: "e.g., Always respond in a friendly tone, use emojis occasionally..."
  - Min height: `min-h-[120px]`
  - ID: `custom-instructions`

- **Remember Context Switch**
  - Label: "Remember conversation context"
  - Description: "AI remembers details from previous messages"
  - Default: On
  - Layout: Label + description on left, Switch on right

---

### Tab 5: Account

**Content Sections**:

**1. Account Information**

**Input Fields**:
- **Email**
  - Label: "Email"
  - Input type: `email`
  - Placeholder: "your.email@example.com"
  - Default: "rich@example.com"
  - Margin top: `mt-2`

- **Username**
  - Label: "Username"
  - Input type: `text`
  - Placeholder: "username"
  - Default: "rich"
  - Margin top: `mt-2`

- **Update Profile Button**
  - Variant: `outline`
  - Text: "Update Profile"

**Separator**

**2. Password**

**Input Fields**:
- Current Password (type: password)
- New Password (type: password)
- Confirm Password (type: password)

**Change Password Button**:
- Variant: `outline`
- Text: "Change Password"

**Separator**

**3. Privacy & Data**

**Switches**:
- **Allow Analytics**
  - Label + description layout
  - Description: "Help improve the app with anonymous usage data"
  - Default: On

- **Personalization**
  - Description: "Use data to personalize your experience"
  - Default: On

**Download My Data Button**:
- Variant: `outline`
- Text: "Download My Data"

**Separator**

**4. Subscription**

**Card/Panel**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-lg`
- Padding: `p-4`
- Content:
  - **Title**: "Free Plan"
  - **Description**: "Basic features with limited usage"
  - **Upgrade Button**: Primary button

**Separator**

**5. Danger Zone**

**Title**: "Danger Zone" (red text)
- Color: `text-red-600 dark:text-red-400`

**Destructive Action Buttons** (vertical stack):
- Clear All Conversations
- Reset All Settings
- Delete Account

Each button:
- Variant: `outline`
- Width: `w-full`
- Justify: `justify-start` (left-aligned text)
- Color: `text-red-600 dark:text-red-400`
- Border: `border-red-200 dark:border-red-900`
- Hover: `hover:bg-red-50 dark:hover:bg-red-950`

---

### Tab 6: Usage

**Content Sections**:

**1. Current Billing Period**

**Period Display**:
- Title: "Current Billing Period"
- Date range: "Nov 1, 2025 - Nov 30, 2025"
- Size: `text-sm text-gray-500`

**Stats Cards** (3-column grid on desktop):
- **Total Messages**
  - Number: Large (text-2xl)
  - Label: Small muted
  - Example: "5,262"
  
- **Total Tokens**
  - Number: Large (text-2xl)
  - Display: Formatted (e.g., "1,558.1K")
  - Label: "Total Tokens"
  
- **Total Cost** (highlighted)
  - Background: `bg-purple-50 dark:bg-purple-950/20`
  - Border: `border-purple-200 dark:border-purple-800`
  - Number color: `text-purple-700 dark:text-purple-300`
  - Display: Currency format (e.g., "$41.87")

Card specs:
- Padding: `p-4`
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-lg`
- Gap: `gap-4`

**View Details Button**:
- Variant: `outline`
- Width: `w-full`
- Text: "View Detailed Analytics"
- Action: Opens UsageDetailsDialog

**Separator**

**2. Usage by Model**

**Title**: "Usage by Model"

**Table Container**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-lg`
- Overflow: hidden

**Table Columns**:
1. Model (with color indicator)
2. Connection
3. Messages (right-aligned)
4. Tokens (right-aligned, formatted as "K")
5. Cost (right-aligned, currency format)

**Table Rows**: Each model with data
- Font: Regular for data
- Model name: `font-medium` with color class
- Color coding:
  - GPT-4: `text-purple-500`
  - Claude: `text-orange-500`
  - GPT-4 Turbo: `text-blue-500`

**Table Specs**:
- Uses shadcn/ui Table component
- Hover rows: `hover:bg-gray-50 dark:hover:bg-gray-900/50`
- Header: `bg-gray-50 dark:bg-gray-900`

---

## 9. Input System & Context Attachments {#input-system}

### ChatInput Component

**Overall Container**:
- Background: `bg-gray-100 dark:bg-gray-900`
- Rounded: `rounded-3xl`
- Padding: `p-2 pr-3`
- Position: `relative` (for drag overlay)
- Border top: `border-t border-gray-200 dark:border-gray-800`

**Layout Structure**:
```
┌────────────────────────────────────────────────────────────┐
│  [Attached Files Display]                                  │
│  (only if files attached)                                  │
├────────────────────────────────────────────────────────────┤
│  [Attached Context Chips]                                  │
│  (knowledge, notes, files, chats, webpages)                │
├────────────────────────────────────────────────────────────┤
│  [Smart Knowledge Suggestions]                             │
│  (appears based on input content)                          │
├────────────────────────────────────────────────────────────┤
│  Input Container                                           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  [+] [🔧]  [Textarea]                         [Send] │  │
│  │   Attach Tools  Message input (grows)         Button │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

### Input Controls (Left Side)

**1. Attach Menu Button** (Plus icon)
- Icon: Plus
- Size: `h-9 w-9`
- Rounded: `rounded-full`
- Hover: `hover:bg-gray-200 dark:hover:bg-gray-800`
- Opens: Attach menu popover

**2. Tools Menu Button** (Wrench icon)
- Icon: Wrench
- Size: `h-9 w-9`
- Rounded: `rounded-full`
- Badge: Shows count of enabled tools (e.g., "2")
- Badge specs:
  - Size: `h-4 w-4` or `min-w-[16px]`
  - Position: `absolute -top-1 -right-1`
  - Background: `bg-purple-500`
  - Text: `text-[10px] text-white`
  - Rounded: `rounded-full`
  - Padding: `px-1`
- Opens: Tools menu popover

### Attach Menu Popover

**Trigger**: Plus button  
**Position**: `side="top" align="start"`  
**Size**: `w-64`  
**Style**: Dark themed (`bg-gray-900 text-white border-gray-700`)

**Menu Items** (vertical list):

1. **Attach Files** (with submenu flyout)
   - Icon: Paperclip
   - Text: "Attach Files"
   - Right icon: ChevronRight
   - Opens: Files submenu (side="right")

2. **Capture**
   - Icon: Camera
   - Text: "Capture"
   - Action: Open camera/screenshot tool

3. **Attach Webpage** (with submenu flyout)
   - Icon: Globe
   - Text: "Attach Webpage"
   - Right icon: ChevronRight
   - Opens: URL input panel

4. **Attach Notes** (with submenu flyout)
   - Icon: FileText
   - Text: "Attach Notes"
   - Right icon: ChevronRight
   - Opens: Notes list

5. **Attach Knowledge** (with submenu flyout)
   - Icon: Brain
   - Text: "Attach Knowledge"
   - Right icon: ChevronRight
   - Opens: Knowledge items list

6. **Reference Chat** (with submenu flyout)
   - Icon: MessageSquare
   - Text: "Reference Chat"
   - Right icon: ChevronRight
   - Opens: Previous chats list

**Menu Item Specs**:
- Padding: `px-3 py-2`
- Rounded: `rounded-md`
- Hover: `hover:bg-gray-800`
- Transition: `transition-colors`
- Gap: `gap-3` (icon to text)
- Icon size: `h-4 w-4`
- Font: `text-sm`

### Submenu Flyouts

**Files Submenu**:
- Width: `w-64`
- Max height: `h-80` (scrollable)
- Header: "Recent Files" (small, muted)
- Loading state: Spinner + "Loading files..."
- Empty state: "No files found"
- List of files with:
  - File title
  - File size (small, muted)
  - Click to attach

**Webpage Submenu**:
- Width: `w-80`
- Contains:
  - Label: "Enter webpage URL"
  - Input field (URL type)
  - "Attach" button
  - Enter key also submits

**Notes/Knowledge/Chats Submenus**:
- Similar to Files submenu
- Show recent items
- Scrollable list
- Click to attach
- Loading and empty states

### Tools Menu Popover

**Trigger**: Wrench button  
**Position**: `side="top" align="start"`  
**Size**: `w-80`  
**Style**: Dark themed

**Content**: Scrollable list of tools with toggles

**Tool Item Structure**:
```
┌──────────────────────────────────────────────────┐
│  [Icon]  Tool Name                     [Switch]  │
│          Description (small text)                │
│          [Settings gear] (if configurable)       │
└──────────────────────────────────────────────────┘
```

**Available Tools** (examples):
1. **Web Search**
   - Icon: Globe
   - Description: "Search the web for information"
   - Toggle: On/Off
   - No settings

2. **Code Interpreter**
   - Icon: Code
   - Description: "Execute Python code"
   - Toggle: On/Off (default: On)
   - No settings

3. **File Management**
   - Icon: FileText
   - Description: "Read and manipulate files"
   - Toggle: On/Off
   - No settings

4. **Weather Forecast**
   - Icon: CloudSun
   - Description: "Get weather information"
   - Toggle: On/Off
   - Settings gear: Opens weather settings modal

5. **EchoMCP**
   - Icon: Wrench
   - Description: "MCP tool integration"
   - Toggle: On/Off (default: On)
   - Settings gear: Opens MCP config

**Tool Item Specs**:
- Padding: `p-3`
- Border bottom: `border-b border-gray-800`
- Layout: Flex row with icon, text column, switch
- Settings button: Small gear icon, appears on right if configurable
- Description: `text-xs text-gray-400`

### Textarea Input

**Component**: shadcn/ui Textarea

**Specs**:
- Min height: `min-h-[48px]`
- Max height: `max-h-[200px]` (scrollable after)
- Padding: `py-3 px-4`
- Font: `text-base`
- Background: Transparent
- Border: None
- Resize: Vertical auto (grows with content)
- Placeholder: "Message..." or "Type your message..."
- Focus: No visible outline (parent container handles focus state)

**Behavior**:
- Enter key: Submits (if not Shift+Enter)
- Shift+Enter: New line
- Auto-resize as user types
- Disabled when `isTyping` is true

### Send Button

**Position**: Right side of input  
**Icon**: Send (paper plane icon)  
**Size**: `h-9 w-9`  
**Rounded**: `rounded-full`  
**Background**: `bg-purple-600 hover:bg-purple-700`  
**Icon color**: `text-white`  
**Disabled state**: `opacity-50 cursor-not-allowed`

### AttachedContext Component

**Purpose**: Display attached knowledge, notes, files, chats, and webpages as colored chips

**Container**:
- Padding: `p-2`
- Background: `bg-gray-100 dark:bg-gray-900`
- Rounded: `rounded-2xl`
- Gap: `gap-2`
- Flex wrap: `flex-wrap`
- Only shows if items are attached

**Chip Structure** (per item):
```
┌────────────────────────────────┐
│  [Icon] Item Name         [×]  │
└────────────────────────────────┘
```

**Chip Specs**:
- Padding: `px-3 py-1.5`
- Rounded: `rounded-full`
- Font: `text-sm`
- Gap: `gap-2`
- X button appears on hover
- Cursor: `cursor-default`

**Color Coding**:
- **Knowledge**: Purple
  - Background: `bg-purple-100 dark:bg-purple-900/30`
  - Border: `border-purple-300 dark:border-purple-700`
  - Text: `text-purple-700 dark:text-purple-300`
  - Icon: Brain

- **Notes**: Blue
  - Background: `bg-blue-100 dark:bg-blue-900/30`
  - Border: `border-blue-300 dark:border-blue-700`
  - Text: `text-blue-700 dark:text-blue-300`
  - Icon: FileText

- **Files**: Green
  - Background: `bg-green-100 dark:bg-green-900/30`
  - Border: `border-green-300 dark:border-green-700`
  - Text: `text-green-700 dark:text-green-300`
  - Icon: Paperclip

- **Chats**: Orange
  - Background: `bg-orange-100 dark:bg-orange-900/30`
  - Border: `border-orange-300 dark:border-orange-700`
  - Text: `text-orange-700 dark:text-orange-300`
  - Icon: MessageSquare

- **Webpages**: Teal
  - Background: `bg-teal-100 dark:bg-teal-900/30`
  - Border: `border-teal-300 dark:border-teal-700`
  - Text: `text-teal-700 dark:text-teal-300`
  - Icon: Globe

**X Button** (remove attachment):
- Size: `h-4 w-4`
- Icon: X
- Opacity: `opacity-0 group-hover:opacity-100`
- Transition: `transition-opacity`
- Hover background: `hover:bg-gray-200 dark:hover:bg-gray-700`
- Rounded: `rounded-full`
- Padding: `p-0.5`

### KnowledgeSuggestions Component

**Purpose**: Suggest relevant knowledge items based on input text

**Trigger**: Appears when user types certain keywords

**Container**:
- Margin: `mb-2`
- Padding: `p-2`
- Background: `bg-purple-50 dark:bg-purple-950/20`
- Border: `border border-purple-200 dark:border-purple-800`
- Rounded: `rounded-xl`
- Animation: Slide down

**Content**:
- Title: "Suggested Knowledge" (small, muted)
- Horizontal scrollable list of suggestion chips
- Each chip:
  - Icon: Brain (small)
  - Knowledge item name
  - Click to attach
  - Background: `bg-white dark:bg-gray-900`
  - Hover: `hover:bg-purple-100 dark:hover:bg-purple-900/40`

**Suggestion Chip Specs**:
- Padding: `px-3 py-2`
- Rounded: `rounded-lg`
- Border: `border border-purple-200 dark:border-purple-700`
- Font: `text-sm`
- Cursor: `cursor-pointer`
- Transition: `transition-colors`

### Drag and Drop Overlay

**Trigger**: File(s) dragged over input area

**Overlay Specs**:
- Position: `absolute inset-0`
- Z-index: `z-50`
- Background: `bg-purple-500/10 dark:bg-purple-500/20`
- Backdrop blur: `backdrop-blur-sm`
- Border: `border-2 border-dashed border-purple-500`
- Rounded: `rounded-3xl`

**Content (centered)**:
- Upload icon (large): `h-12 w-12`
- Text: "Drop files here" (large, purple)
- Subtext: "Documents, images, and other files" (small, muted)
- Icon color: `text-purple-600 dark:text-purple-400`

**Behavior**:
- Shows when file is dragged over
- Hides when drag leaves or file is dropped
- Accepts multiple files
- Creates AttachedItem for each file

---

## 10. Message Display {#message-display}

### ChatMessage Component

**Purpose**: Display a single message (user or assistant)

**Structure Variations**:

#### User Message
```
┌──────────────────────────────────────────────┐
│                      [Avatar] Username  Time │
│                      Message content here... │
│                      Multiline text support  │
│                                      [Edit]  │
└──────────────────────────────────────────────┘
```

#### Assistant Message
```
┌──────────────────────────────────────────────┐
│ [Avatar] Assistant Name           Time       │
│ Message content here...                      │
│ Supports markdown, code blocks, etc.         │
│ [Structured content panel if present]        │
│                [Copy] [Regenerate] [Rate]    │
└──────────────────────────────────────────────┘
```

### User Message Specs

**Container**:
- Alignment: Right-aligned (`flex justify-end`)
- Max width: `max-w-[85%]`
- Gap: `gap-3`

**Avatar** (right side):
- Size: `h-8 w-8`
- Rounded: `rounded-full`
- Background: `bg-gray-200 dark:bg-gray-700`
- Text: User initials
- Font: `text-sm font-medium`
- Position: Top right

**Content Container**:
- Background: `bg-gray-100 dark:bg-gray-800`
- Padding: `px-4 py-3`
- Rounded: `rounded-2xl` with adjusted corner (`rounded-tr-sm`)
- Border: Optional `border border-gray-200 dark:border-gray-700`

**Header Row**:
- Username: `font-medium text-sm`
- Timestamp: `text-xs text-gray-500 dark:text-gray-400`
- Gap: `gap-2`

**Message Text**:
- Font: `text-base`
- Color: `text-gray-900 dark:text-gray-100`
- Line height: `leading-relaxed`
- Whitespace: `whitespace-pre-wrap` (preserves line breaks)

**Edit Button** (on hover):
- Icon: Edit2 icon
- Size: `h-8 w-8`
- Opacity: `opacity-0 group-hover:opacity-100`
- Transition: `transition-opacity`
- Position: Below message
- Variant: Ghost

### Assistant Message Specs

**Container**:
- Alignment: Left-aligned
- Max width: `max-w-[85%]`
- Gap: `gap-3`

**Avatar** (left side):
- Size: `h-8 w-8`
- Rounded: `rounded-full`
- Background: `bg-gradient-to-r from-purple-600 to-blue-500`
- Icon: Sparkles icon (`h-4 w-4 text-white`)
- Position: Top left

**Content Container**:
- Background: `bg-white dark:bg-gray-800`
- Padding: `px-4 py-3`
- Rounded: `rounded-2xl` with adjusted corner (`rounded-tl-sm`)
- Border: `border border-gray-200 dark:border-gray-700`

**Header Row**:
- Assistant name: `font-medium text-sm`
- Timestamp: `text-xs text-gray-500 dark:text-gray-400`

**Message Content**:
- Uses react-markdown with:
  - Paragraph breaks
  - Bold/italic support
  - Code inline: `bg-gray-100 dark:bg-gray-900 px-1.5 py-0.5 rounded`
  - Code blocks: Syntax highlighted with react-syntax-highlighter
  - Lists (ordered and unordered)
  - Links with styling
  - Headings

**Code Block Specs**:
- Background: `bg-gray-900 dark:bg-gray-950`
- Border: `border border-gray-700`
- Rounded: `rounded-lg`
- Padding: `p-4`
- Font: `font-mono text-sm`
- Language label: Top right corner
- Copy button: Top right, appears on hover
- Syntax highlighting: Theme based on app theme

**Structured Content** (if present):
- Renders StructuredPanel component
- Margin top: `mt-3`
- See Structured Content section for details

**Action Buttons Row** (bottom):
- Layout: Horizontal flex
- Gap: `gap-2`
- Opacity: `opacity-0 group-hover:opacity-100` (shows on hover)
- Buttons:
  1. **Copy** (clipboard icon)
  2. **Regenerate** (only on last assistant message)
  3. **Rate** (thumbs up/down)

**Button Specs**:
- Size: `h-8 w-8` or `h-8 px-3` (with text)
- Variant: Ghost
- Icon size: `h-4 w-4`
- Hover: `hover:bg-gray-100 dark:hover:bg-gray-700`

### Message Edit Mode

**Trigger**: Click edit button on user message

**UI Changes**:
- Message content → Textarea
- Edit button → Save/Cancel buttons
- Textarea:
  - Pre-filled with current content
  - Auto-focused
  - Same width as message
  - Min height: `min-h-[100px]`

**Save Button**: Saves edit, removes subsequent messages, triggers new AI response  
**Cancel Button**: Reverts to display mode

### Message Regenerate

**Trigger**: Click regenerate on last assistant message

**Behavior**:
- Removes current assistant message
- Shows typing indicator
- Generates new response with same prompt
- Different response due to randomness

---

## 11. Structured Content Rendering {#structured-content}

### StructuredPanel Component

**Purpose**: Render structured data from AI responses in interactive formats

**Supported Types**:
1. `panel.table` - Tabular data with actions
2. `panel.list` - List of items with titles and content
3. `panel.kv` - Key-value pairs
4. `panel.form` - Input forms with validation

### Data Structure

```typescript
interface StructuredData {
  type: "panel.table" | "panel.list" | "panel.kv" | "panel.form"
  title?: string
  
  // Table specific
  columns?: string[]
  rows?: any[][]
  rowActions?: Action[]
  actions?: Action[]
  
  // List specific
  items?: ListItem[]
  
  // KV specific
  kvPairs?: { key: string, value: string }[]
  
  // Form specific
  fields?: FormField[]
  submit?: Action
}

interface Action {
  type: "action.callTool" | "action.render" | "action.openUrl"
  title: string
  tool?: string
  args?: Record<string, any>
}

interface ListItem {
  title: string
  content: string
}

interface FormField {
  name: string
  label: string
  type: "text" | "email" | "textarea" | "number"
  value?: string
  required?: boolean
}
```

### panel.table Rendering

**Container**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-xl`
- Overflow: hidden
- Margin: `my-3`

**Header Section** (if title or actions):
- Padding: `px-4 py-3`
- Border bottom: `border-b border-gray-200 dark:border-gray-800`
- Background: `bg-gray-50 dark:bg-gray-900`
- Layout: Flex row, space-between
- Title: `font-semibold text-base`
- Actions: Button group (gap-2)

**Table**:
- Component: shadcn/ui Table
- Dense: Yes
- Hover: Row hover effect
- Bordered: Column borders

**Column Headers**:
- Background: `bg-gray-50 dark:bg-gray-900`
- Padding: `px-4 py-2`
- Font: `font-medium text-sm`
- Border bottom: `border-b-2 border-gray-200 dark:border-gray-700`

**Table Rows**:
- Padding: `px-4 py-3`
- Hover: `hover:bg-gray-50 dark:hover:bg-gray-800`
- Border bottom: `border-b border-gray-200 dark:border-gray-800`

**Row Actions Column** (if rowActions defined):
- Alignment: Right
- Contains: Action buttons
- Buttons: Small, variant="ghost"
- Layout: Horizontal with gap-1

**Action Buttons** (table-level):
- Position: Top right of header
- Variant: Default or outline
- Size: Small
- Icon: Optional (left of text)

**Example Appearance**:
```
┌────────────────────────────────────────────────┐
│  Active Projects              [+ New Project]  │
├──────┬──────────────┬────────┬──────┬─────────┤
│ Key  │ Name         │ Owner  │Tasks │ Actions │
├──────┼──────────────┼────────┼──────┼─────────┤
│ CF   │ ChoonForge   │ rich   │  12  │ [Open]  │
│ AM   │ aiMate       │ rich   │   8  │ [Open]  │
│ WA   │ WebApp...    │ john   │  24  │ [Open]  │
└──────┴──────────────┴────────┴──────┴─────────┘
```

### panel.list Rendering

**Container**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-xl`
- Overflow: hidden
- Margin: `my-3`

**Header** (if title):
- Padding: `px-4 py-3`
- Border bottom: `border-b`
- Background: `bg-gray-50 dark:bg-gray-900`
- Title: `font-semibold text-base`

**List Items**:
- Each item: Collapsible/expandable (shadcn/ui Accordion)
- Padding: `px-4 py-3`
- Border bottom: `border-b` (except last)

**List Item Structure**:
```
┌────────────────────────────────────────┐
│  Item Title                       [▼]  │
│  ─────────────────────────────────     │
│  Item content here when expanded       │
└────────────────────────────────────────┘
```

**Item Title**:
- Font: `font-medium text-base`
- Color: `text-gray-900 dark:text-gray-100`
- Cursor: `cursor-pointer`

**Item Content**:
- Padding top: `pt-2`
- Font: `text-sm`
- Color: `text-gray-600 dark:text-gray-400`
- Line height: `leading-relaxed`

**Expand/Collapse Icon**:
- Icon: ChevronDown
- Size: `h-4 w-4`
- Rotate: 180deg when expanded
- Transition: `transition-transform`

### panel.kv Rendering

**Container**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-xl`
- Overflow: hidden
- Margin: `my-3`

**Header** (if title):
- Same as table header

**Key-Value Pairs**:
- Layout: 2-column grid or definition list
- Each pair: Row with border bottom

**Pair Structure**:
```
┌──────────────────────────────────────────┐
│  Key Label                   Value       │
│  ─────────────────────────────────────   │
│  Next Key                    Next Value  │
└──────────────────────────────────────────┘
```

**Key Column**:
- Font: `font-medium text-sm`
- Color: `text-gray-600 dark:text-gray-400`
- Width: `w-1/3` or fixed `w-48`
- Padding: `px-4 py-3`

**Value Column**:
- Font: `text-sm`
- Color: `text-gray-900 dark:text-gray-100`
- Padding: `px-4 py-3`
- Word break: `break-words`

**Alternate Background** (optional):
- Odd rows: Default background
- Even rows: `bg-gray-50 dark:bg-gray-900/50`

### panel.form Rendering

**Container**:
- Border: `border border-gray-200 dark:border-gray-800`
- Rounded: `rounded-xl`
- Padding: `p-4`
- Margin: `my-3`
- Background: `bg-white dark:bg-gray-800`

**Header** (if title):
- Title: `font-semibold text-lg`
- Margin bottom: `mb-4`

**Form Fields**:
- Vertical stack: `space-y-4`
- Each field uses shadcn/ui form components

**Field Structure**:
```
┌─────────────────────────────────────┐
│  Label *                            │
│  ┌───────────────────────────────┐  │
│  │ Input or Textarea             │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

**Label**:
- Component: shadcn/ui Label
- Font: `text-sm font-medium`
- Required asterisk: `text-red-500` if required

**Input Types**:
- Text: `<Input type="text" />`
- Email: `<Input type="email" />`
- Textarea: `<Textarea rows={4} />`
- Number: `<Input type="number" />`

**Input Styling**:
- Border: `border border-gray-200 dark:border-gray-700`
- Rounded: `rounded-md`
- Padding: `px-3 py-2`
- Focus: `focus:ring-2 focus:ring-purple-500`

**Submit Button**:
- Position: Below all fields
- Variant: Default (purple)
- Size: Medium
- Full width or auto
- Text: From `submit.title` property

**Form Behavior**:
- Client-side validation
- Required field checks
- Submit triggers tool call with form data
- Success: Show toast, optionally replace panel
- Error: Show error message below button

---

## 12. Component Inventory {#component-inventory}

### Custom Components (28)

| Component | Purpose | Key Props | Size/Specs |
|-----------|---------|-----------|------------|
| AboutModal | About/info dialog | open, onOpenChange | md |
| AdminModal | Admin interface | open, onOpenChange | lg |
| ArchivedModal | Archive management | open, onOpenChange | lg |
| AttachedContext | Context chips display | attachedKnowledge, onDetach, etc. | auto |
| BaseDialog | Dialog wrapper | title, description, children | md |
| BaseModal | Modal with tabs | title, tabs, size, onSave | md/lg/xl |
| ChatHeader | Header bar | selectedModel, onModelChange | h-16 |
| ChatInput | Message input | onSend, disabled | auto (min 96px) |
| ChatMessage | Message bubble | role, content, onEdit, onRegenerate | max-w-[85%] |
| ConnectionEditDialog | API connection editor | connection, onSave | md |
| ConversationSidebar | Left nav sidebar | conversations, onSelectConversation | w-80 |
| DebugContext | Debug provider | N/A (context) | N/A |
| DebugPanel | Debug overlay | N/A | Fixed bottom-right |
| EmptyState | Empty chat view | onSendMessage | Centered |
| FilesModal | File management | open, onOpenChange | lg |
| HelpModal | Help docs | open, onOpenChange | lg |
| KnowledgeModal | Knowledge base | open, onOpenChange | lg |
| KnowledgeSuggestions | Smart suggestions | inputValue, onAttach | auto |
| MCPEditDialog | MCP config | tool, onSave | md |
| ModelEditDialog | Model settings | model, onSave | md |
| NotesModal | Notes management | open, onOpenChange | lg |
| PluginEditDialog | Plugin config | plugin, onSave | md |
| ProjectModal | Project management | open, onOpenChange | lg |
| RatingModal | Message rating | open, message, onRate | sm |
| SearchModal | Search interface | open, onOpenChange | lg |
| SettingsModal | Settings (6 tabs) | open, onOpenChange | lg |
| ShareDialog | Share conversation | open, conversation, onShare | md |
| ShareModal | Share options | open, onOpenChange | md |
| StructuredPanel | Structured renderer | data (StructuredData) | auto |
| ThemeProvider | Theme context | defaultTheme, children | N/A |
| UsageDetailsDialog | Usage analytics | open, onOpenChange | xl |

### shadcn/ui Components (45)

All located in `/components/ui/`:

- accordion.tsx
- alert-dialog.tsx
- alert.tsx
- aspect-ratio.tsx
- avatar.tsx
- badge.tsx
- breadcrumb.tsx
- button.tsx
- calendar.tsx
- card.tsx
- carousel.tsx
- chart.tsx
- checkbox.tsx
- collapsible.tsx
- command.tsx
- context-menu.tsx
- dialog.tsx
- drawer.tsx
- dropdown-menu.tsx
- form.tsx
- hover-card.tsx
- input-otp.tsx
- input.tsx
- label.tsx
- menubar.tsx
- navigation-menu.tsx
- pagination.tsx
- popover.tsx
- progress.tsx
- radio-group.tsx
- resizable.tsx
- scroll-area.tsx
- select.tsx
- separator.tsx
- sheet.tsx
- sidebar.tsx
- skeleton.tsx
- slider.tsx
- sonner.tsx (toast)
- switch.tsx
- table.tsx
- tabs.tsx
- textarea.tsx
- toggle-group.tsx
- toggle.tsx
- tooltip.tsx

---

## 13. Design System & Theming {#design-system}

### Color System

**Theme Modes**:
- Light
- Dark
- System (auto-detects)

**Color Themes** (accent colors):
- Purple (default)
- Blue
- Green
- Orange

**Implementation**: CSS variables in `/styles/globals.css`

```css
:root {
  /* Light mode variables */
  --background: 0 0% 98%;          /* #FAFAFA (gray-50) */
  --foreground: 0 0% 9%;           /* #171717 (gray-900) */
  --surface: 0 0% 100%;            /* #FFFFFF */
  --muted: 0 0% 96%;               /* #F5F5F5 */
  --border: 0 0% 89%;              /* #E4E4E7 (gray-200) */
  
  /* Purple theme (default) */
  --primary: 271 91% 65%;          /* purple-500 */
  --primary-hover: 271 81% 60%;    /* purple-600 */
  --primary-muted: 270 100% 98%;   /* purple-50 */
}

.dark {
  /* Dark mode overrides */
  --background: 0 0% 4%;           /* #0A0A0A (gray-950) */
  --foreground: 0 0% 98%;          /* #FAFAFA */
  --surface: 0 0% 11%;             /* #1C1C1C (gray-900) */
  --muted: 0 0% 15%;               /* #262626 */
  --border: 0 0% 23%;              /* #3A3A3A (gray-800) */
  
  /* Purple theme in dark */
  --primary: 271 91% 65%;          /* Same purple */
  --primary-hover: 271 81% 70%;    /* Lighter for dark */
  --primary-muted: 271 91% 10%;    /* purple-950 */
}
```

**Color Application**:
- Backgrounds: `bg-background`, `bg-surface`
- Text: `text-foreground`, `text-muted-foreground`
- Borders: `border-border`
- Primary actions: `bg-primary hover:bg-primary-hover`

### Typography

**Font Family**: System font stack
```css
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 
             'Oxygen', 'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 
             'Helvetica Neue', sans-serif;
```

**Font Sizes** (Tailwind classes):
- xs: `text-xs` (0.75rem / 12px)
- sm: `text-sm` (0.875rem / 14px)
- base: `text-base` (1rem / 16px)
- lg: `text-lg` (1.125rem / 18px)
- xl: `text-xl` (1.25rem / 20px)
- 2xl: `text-2xl` (1.5rem / 24px)
- 3xl: `text-3xl` (1.875rem / 30px)

**Font Weights**:
- Normal: `font-normal` (400)
- Medium: `font-medium` (500)
- Semibold: `font-semibold` (600)
- Bold: `font-bold` (700)

**Line Heights**:
- Tight: `leading-tight` (1.25)
- Normal: `leading-normal` (1.5)
- Relaxed: `leading-relaxed` (1.625)

**User-Configurable Font Size** (via Settings):
- Small: Base font 14px
- Medium: Base font 16px (default)
- Large: Base font 18px
- Applied via CSS variable on root

### Spacing Scale

**Padding/Margin** (Tailwind scale):
- 0: 0px
- 1: 0.25rem (4px)
- 2: 0.5rem (8px)
- 3: 0.75rem (12px)
- 4: 1rem (16px)
- 6: 1.5rem (24px)
- 8: 2rem (32px)
- 12: 3rem (48px)
- 16: 4rem (64px)

**Gap Scale**: Same as padding/margin

### Border Radius

**Tailwind Classes**:
- sm: `rounded-sm` (0.125rem / 2px)
- Default: `rounded` (0.25rem / 4px)
- md: `rounded-md` (0.375rem / 6px)
- lg: `rounded-lg` (0.5rem / 8px)
- xl: `rounded-xl` (0.75rem / 12px)
- 2xl: `rounded-2xl` (1rem / 16px)
- 3xl: `rounded-3xl` (1.5rem / 24px)
- full: `rounded-full` (9999px / circular)

**Application**:
- Buttons: `rounded-xl` or `rounded-full`
- Modals/panels: `rounded-xl`
- Input container: `rounded-3xl`
- Cards: `rounded-lg` or `rounded-xl`
- Avatars: `rounded-full`
- Chips: `rounded-full`

### Shadows

**Tailwind Shadow Classes**:
- sm: `shadow-sm` - Subtle shadow
- Default: `shadow` - Standard shadow
- md: `shadow-md` - Medium shadow
- lg: `shadow-lg` - Large shadow
- xl: `shadow-xl` - Extra large shadow

**Application**:
- Modals: `shadow-xl`
- Popover menus: `shadow-lg`
- Cards: `shadow-sm` or `shadow`
- Buttons (hover): `hover:shadow-md`

### Gradients

**Primary Gradient** (Purple to Blue):
```css
background: linear-gradient(to right, #9333EA, #3B82F6);
/* Tailwind: bg-gradient-to-r from-purple-600 to-blue-500 */
```

**Application**:
- AI assistant avatar background
- Accent elements
- Hover states on primary buttons
- Brand elements

### Transitions

**Standard Transition**:
```css
transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);
/* Tailwind: transition-all duration-150 ease-in-out */
```

**Common Transitions**:
- Colors: `transition-colors`
- Opacity: `transition-opacity`
- Transform: `transition-transform`
- All: `transition-all`

**Durations**:
- 75: 75ms (very fast)
- 150: 150ms (fast, default)
- 300: 300ms (medium)
- 500: 500ms (slow)

### Responsive Breakpoints

**Tailwind Breakpoints**:
- sm: 640px
- md: 768px
- lg: 1024px
- xl: 1280px
- 2xl: 1536px

**Usage**:
- Mobile first: Base styles for mobile
- `md:` prefix for tablet and up
- `lg:` prefix for desktop and up

**Key Responsive Behaviors**:
- Sidebar: Hidden on mobile, visible on `lg:`
- Modal sizes: Full screen on mobile, constrained on `lg:`
- Grid columns: 1 column mobile, 2-3 on desktop
- Font sizes: Slightly smaller on mobile

---

## 14. State Management {#state-management}

### Global State

**Context Providers**:
1. **ThemeProvider** (ThemeProvider.tsx)
   - Theme mode (light/dark/system)
   - Font size (small/medium/large)
   - Color theme (purple/blue/green/orange)

2. **DebugProvider** (DebugContext.tsx)
   - Debug panel visibility
   - Debug log entries
   - API call tracking

### Local State (App.tsx)

**Conversation State**:
```typescript
const [conversations, setConversations] = useState<ConversationData[]>([])
const [activeConversationId, setActiveConversationId] = useState<string | null>(null)
```

**UI State**:
```typescript
const [sidebarOpen, setSidebarOpen] = useState(true)           // Desktop
const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false) // Mobile
const [isTyping, setIsTyping] = useState(false)
```

**Model State**:
```typescript
const [selectedModel, setSelectedModel] = useState("gpt-4")
const [enabledModels, setEnabledModels] = useState<Record<string, boolean>>({
  "gpt-4": true,
  "claude-3-opus": true,
  // ...
})
```

**Modal State** (boolean flags):
```typescript
const [settingsOpen, setSettingsOpen] = useState(false)
const [adminOpen, setAdminOpen] = useState(false)
// ... etc for each modal
```

### Component State Examples

**ChatInput State**:
```typescript
const [message, setMessage] = useState("")
const [attachMenuOpen, setAttachMenuOpen] = useState(false)
const [attachedKnowledge, setAttachedKnowledge] = useState<string[]>([])
const [attachedFiles, setAttachedFiles] = useState<string[]>([])
const [enabledTools, setEnabledTools] = useState({
  webSearch: false,
  codeInterpreter: true,
  // ...
})
```

**SettingsModal State**:
```typescript
// Managed by BaseModal:
const [activeTab, setActiveTab] = useState("general")

// Tab-specific state:
const [language, setLanguage] = useState("en-gb")
const [notifications, setNotifications] = useState("off")
const [systemPrompt, setSystemPrompt] = useState("")
```

### State Update Patterns

**Adding a Message**:
```typescript
const handleSendMessage = (content: string) => {
  const userMessage = {
    id: Date.now().toString(),
    role: "user" as const,
    content,
    timestamp: getTimestamp()
  }
  
  setConversations(prev =>
    prev.map(conv =>
      conv.id === activeConversationId
        ? { ...conv, messages: [...conv.messages, userMessage] }
        : conv
    )
  )
  
  // Trigger AI response...
}
```

**Creating New Conversation**:
```typescript
const createNewConversation = () => {
  const newConv = {
    id: Date.now().toString(),
    messages: [],
    title: "New Conversation",
    createdAt: new Date()
  }
  
  setConversations(prev => [newConv, ...prev])
  setActiveConversationId(newConv.id)
}
```

**Deleting Conversation**:
```typescript
const handleDeleteConversation = (id: string) => {
  setConversations(prev => prev.filter(conv => conv.id !== id))
  
  if (activeConversationId === id) {
    const remaining = conversations.filter(conv => conv.id !== id)
    setActiveConversationId(remaining.length > 0 ? remaining[0].id : null)
  }
}
```

### Data Flow

```
User Action (e.g., send message)
    ↓
Event Handler (handleSendMessage)
    ↓
State Update (setConversations)
    ↓
React Re-render
    ↓
UI Update (new message appears)
    ↓
Side Effect (API call for AI response)
    ↓
State Update (add AI message)
    ↓
UI Update (AI message appears)
```

### Persistence Strategy

**Current**: In-memory only (resets on refresh)

**Future**:
- localStorage for temporary persistence
- Backend API for permanent storage
- Sync state on mount
- Auto-save on changes (debounced)

---

## 15. API Integration Points {#api-integration}

### API Stub System

**Location**: `/utils/api-stubs.ts`

**Purpose**: Mock API responses for development without backend

**Response Format**:
```typescript
interface APIResponse<T> {
  success: boolean
  data?: T
  error?: string
  metadata?: {
    endpoint: string
    method: string
    timestamp: string
  }
}
```

### Endpoint Categories

#### 1. Conversation Management

**GET /api/v1/conversations**
- Returns: List of conversations
- Data: `Conversation[]`

**POST /api/v1/conversations/create**
- Body: `{ title?: string }`
- Returns: New conversation object

**DELETE /api/v1/conversations/{id}**
- Returns: Success confirmation

**PATCH /api/v1/conversations/{id}/rename**
- Body: `{ title: string }`
- Returns: Updated conversation

**POST /api/v1/conversations/{id}/clone**
- Returns: Cloned conversation

#### 2. Message Management

**POST /api/v1/chat/sendMessage**
- Body: `{ conversationId: string, message: string }`
- Returns: Message ID

**POST /api/v1/chat/completion**
- Body: `{ conversationId: string, messages: Message[], model: string }`
- Returns: AI response with potential structured content
- Response includes:
  - `content`: Text response
  - `structuredContent?`: Structured data object
  - `usage`: Token usage stats

**POST /api/v1/messages/{id}/rate**
- Body: `{ rating: number, feedback?: string }`
- Returns: Success confirmation

#### 3. Context Attachment

**GET /api/v1/knowledge**
- Returns: List of knowledge items
- Data: `Knowledge[]`

**POST /api/v1/attachKnowledge**
- Body: `{ knowledgeId: string, chatId: string }`
- Returns: Attachment confirmation

**GET /api/v1/notes**
- Returns: List of notes
- Data: `Note[]`

**POST /api/v1/attachNote**
- Body: `{ noteId: string, chatId: string }`
- Returns: Attachment confirmation

**GET /api/v1/files**
- Returns: List of files
- Data: `File[]`

**POST /api/v1/attachFile**
- Body: `{ fileId: string, chatId: string }`
- Returns: Attachment confirmation

**GET /api/v1/chats**
- Returns: List of previous chats for reference
- Data: `Chat[]`

**POST /api/v1/referenceChat**
- Body: `{ chatId: string, currentChatId: string }`
- Returns: Reference confirmation

**POST /api/v1/attachWebpage**
- Body: `{ url: string, chatId: string }`
- Returns: Webpage content summary + metadata
- Response includes:
  - `title`: Page title
  - `description`: Meta description
  - `content`: Extracted text

#### 4. Model Management

**GET /api/v1/models**
- Returns: List of available models
- Data: `Model[]` with properties:
  - id, name, provider, capabilities, costPer1kTokens

**PATCH /api/v1/models/{id}/toggle**
- Body: `{ enabled: boolean }`
- Returns: Updated model state

**GET /api/v1/models/{id}/settings**
- Returns: Model configuration

**PATCH /api/v1/models/{id}/settings**
- Body: Model-specific settings
- Returns: Success confirmation

#### 5. Settings Management

**GET /api/v1/settings**
- Returns: All user settings

**PATCH /api/v1/settings**
- Body: Partial settings object
- Returns: Updated settings

**GET /api/v1/settings/usage**
- Returns: Usage statistics
- Data: Token usage, message counts, costs per model

#### 6. Tool Management

**GET /api/v1/tools**
- Returns: List of available tools
- Data: `Tool[]`

**PATCH /api/v1/tools/{id}/toggle**
- Body: `{ enabled: boolean }`
- Returns: Updated tool state

**GET /api/v1/tools/{id}/settings**
- Returns: Tool configuration

**PATCH /api/v1/tools/{id}/settings**
- Body: Tool-specific settings
- Returns: Success confirmation

#### 7. File Upload

**POST /api/v1/files/upload**
- Body: FormData with file(s)
- Returns: File metadata
- Response includes:
  - fileId, filename, size, type, url

**POST /api/v1/files/drop**
- Body: FormData with dropped files
- Returns: Array of file metadata

### Debug Logging

**All API calls are logged via DebugContext**:

```typescript
addLog({
  action: 'Description of action',
  api: 'api/v1/endpoint',
  payload?: any,
  type: 'success' | 'error' | 'warning' | 'info'
})
```

**Debug Panel Display**:
- Shows all logged API calls
- Color-coded by type
- Collapsible entries with full payload
- Timestamp for each call
- Can be cleared
- Sticky at bottom-right corner

### Future Backend Integration

**Migration Steps**:
1. Replace stub functions with actual HTTP calls (axios/fetch)
2. Add authentication headers (JWT tokens)
3. Implement error handling and retry logic
4. Add loading states
5. Implement optimistic updates
6. Add request caching
7. Set up WebSocket for real-time updates

**Example Real Implementation**:
```typescript
export async function getKnowledge(): Promise<APIResponse<Knowledge[]>> {
  try {
    const response = await fetch('/api/v1/knowledge', {
      headers: {
        'Authorization': `Bearer ${getAuthToken()}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (!response.ok) {
      throw new Error(response.statusText)
    }
    
    const data = await response.json()
    
    return {
      success: true,
      data,
      metadata: {
        endpoint: '/api/v1/knowledge',
        method: 'GET',
        timestamp: new Date().toISOString()
      }
    }
  } catch (error) {
    return {
      success: false,
      error: error.message
    }
  }
}
```

---

## Appendix A: Quick Reference - Key Measurements

### Layout
- Sidebar width: 320px
- Header height: 64px
- Max content width: 1024px (4xl)
- Message max width: 85%

### Spacing
- Container padding: 16-24px (p-4 to p-6)
- Element gap: 8-24px (gap-2 to gap-6)
- Message spacing: 24px (space-y-6)

### Border Radius
- Large containers: 24px (rounded-3xl)
- Buttons/inputs: 12px (rounded-xl)
- Avatars/chips: 50% (rounded-full)
- Cards: 8-12px (rounded-lg to rounded-xl)

### Colors (Key Variables)
- Primary (purple): #9333EA
- Background (light): #FAFAFA
- Background (dark): #0A0A0A
- Border (light): #E4E4E7
- Border (dark): #3A3A3A

### Typography
- Base font: 16px (text-base)
- Small text: 14px (text-sm)
- Tiny text: 12px (text-xs)
- Large text: 18px (text-lg)

### Transitions
- Standard: 150ms ease-in-out
- Colors: transition-colors
- Opacity: transition-opacity

---

## Appendix B: Component Props Reference

### BaseModal Props

```typescript
interface BaseModalProps {
  open: boolean                    // Control visibility
  onOpenChange: (open: boolean) => void
  title: string                    // Modal title
  description?: string             // Subtitle
  icon?: React.ComponentType       // Icon component (e.g., Settings)
  tabs?: Tab[]                     // Array of tab configurations
  children?: ReactNode             // Content if no tabs
  onSave?: () => void             // Save handler
  onDelete?: () => void           // Delete handler
  saveLabel?: string              // Default: "Save"
  deleteLabel?: string            // Default: "Delete"
  showSave?: boolean              // Default: true
  showDelete?: boolean            // Default: false
  isSaveDisabled?: boolean
  isDeleteDisabled?: boolean
  size?: "md" | "lg" | "xl"       // Default: "lg"
}

interface Tab {
  id: string                       // Unique identifier
  label: string                    // Display name
  icon?: React.ComponentType       // Optional icon
  content: ReactNode               // Tab content
}
```

### ChatMessage Props

```typescript
interface ChatMessageProps {
  role: "user" | "assistant"
  content: string                  // Message text (markdown supported)
  timestamp: string                // Display timestamp
  structuredContent?: StructuredData
  onEdit?: (newContent: string) => void    // User messages only
  onRegenerate?: () => void        // Last assistant message only
}
```

### ChatInput Props

```typescript
interface ChatInputProps {
  onSend: (message: string) => void
  disabled?: boolean               // Disable during AI response
}
```

### ConversationSidebar Props

```typescript
interface ConversationSidebarProps {
  conversations: Conversation[]
  activeConversationId: string | null
  onSelectConversation: (id: string) => void
  onNewConversation: () => void
  onDeleteConversation: (id: string) => void
  onRenameConversation: (id: string, title: string) => void
  onCloneConversation: (id: string) => void
  onClose?: () => void            // Mobile only
  enabledModels: Record<string, boolean>
  onToggleModel: (modelId: string) => void
}

interface Conversation {
  id: string
  title: string
  lastMessage: string
  timestamp: Date
}
```

### StructuredPanel Props

```typescript
interface StructuredPanelProps {
  data: StructuredData
}

interface StructuredData {
  type: "panel.table" | "panel.list" | "panel.kv" | "panel.form"
  title?: string
  
  // Table
  columns?: string[]
  rows?: any[][]
  rowActions?: StructuredAction[]
  actions?: StructuredAction[]
  
  // List
  items?: Array<{ title: string; content: string }>
  
  // Key-Value
  kvPairs?: Array<{ key: string; value: string }>
  
  // Form
  fields?: FormField[]
  submit?: StructuredAction
}
```

---

## Appendix C: File Upload Specifications

### Drag and Drop

**Supported Areas**: ChatInput component

**Supported File Types**:
- Images: jpg, jpeg, png, gif, webp, svg
- Documents: pdf, doc, docx, txt, md
- Code: js, ts, jsx, tsx, py, java, cpp, etc.
- Data: json, csv, xml
- Archives: zip (future)

**Max File Size**: 10MB per file (configurable)

**Multiple Files**: Supported (max 10 at once)

**Behavior**:
1. User drags file(s) over input area
2. Purple overlay appears with "Drop files here" message
3. User drops files
4. Files are validated (type, size)
5. Valid files → AttachedItem array
6. Preview generated for images
7. Toast notification: "{n} file(s) attached"
8. Files displayed as chips above input

**Visual Feedback**:
- Drag enter: Show overlay
- Drag over: Maintain overlay, show drop cursor
- Drag leave: Hide overlay
- Drop: Process files, hide overlay, show chips

### File Attachment Display

**AttachedItem Interface**:
```typescript
interface AttachedItem {
  id: string                       // Unique ID
  name: string                     // Filename
  type: "file" | "image" | "document"
  size?: string                    // Formatted size (e.g., "2.5 MB")
  preview?: string                 // Image preview URL
}
```

**Chip Appearance** (see Input System section for full specs):
- Image files: Show thumbnail
- Documents: Show FileText icon
- Regular files: Show Paperclip icon
- X button to remove (on hover)

---

## Appendix D: Keyboard Shortcuts

### Global Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl/Cmd + K | Focus search |
| Ctrl/Cmd + N | New conversation |
| Ctrl/Cmd + , | Open settings |
| Ctrl/Cmd + / | Show keyboard shortcuts |
| Esc | Close modal/popover |

### Chat Shortcuts

| Shortcut | Action |
|----------|--------|
| Enter | Send message |
| Shift + Enter | New line in message |
| Ctrl/Cmd + ↑ | Edit last user message |
| Ctrl/Cmd + R | Regenerate last response |

### Sidebar Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl/Cmd + B | Toggle sidebar |
| ↑/↓ | Navigate conversations |
| Enter | Select conversation |
| Delete | Delete conversation (with confirmation) |

### Modal Shortcuts

| Shortcut | Action |
|----------|--------|
| Tab | Next tab |
| Shift + Tab | Previous tab |
| Ctrl/Cmd + S | Save (if applicable) |
| Esc | Close modal |

---

## Appendix E: Animation Specifications

### Page Transitions

**Modal Open/Close**:
- Animation: Scale + fade
- Duration: 200ms
- Easing: ease-in-out
- Scale: 0.95 → 1.0 (open), 1.0 → 0.95 (close)
- Opacity: 0 → 1 (open), 1 → 0 (close)

**Sidebar Slide** (mobile):
- Animation: Slide from left
- Duration: 300ms
- Easing: ease-out
- Transform: translateX(-100%) → translateX(0)

### Element Animations

**Message Appear**:
- Animation: Fade up
- Duration: 200ms
- Transform: translateY(10px) → translateY(0)
- Opacity: 0 → 1

**Typing Indicator**:
- Animation: Bounce (3 dots, staggered)
- Duration: 600ms per cycle
- Delay: 0ms, 150ms, 300ms for each dot
- Transform: translateY(0) → translateY(-4px) → translateY(0)

**Button Hover**:
- Animation: Background color
- Duration: 150ms
- Easing: ease-in-out

**Chip Remove**:
- Animation: Scale + fade
- Duration: 150ms
- Scale: 1.0 → 0.8
- Opacity: 1 → 0

**Drag Overlay**:
- Animation: Fade + blur
- Duration: 200ms
- Backdrop filter: blur(0) → blur(4px)
- Opacity: 0 → 1

### Loading States

**Spinner**:
- Animation: Rotate
- Duration: 1000ms (1 second per rotation)
- Iteration: Infinite
- Easing: Linear

**Skeleton Loader** (future):
- Animation: Shimmer
- Duration: 2000ms
- Iteration: Infinite
- Gradient: Move from left to right

**Progress Bar**:
- Animation: Width increase
- Duration: Variable (based on progress)
- Easing: ease-out

---

## Final Notes

This specification is comprehensive enough to rebuild the entire application without seeing a single screenshot. All dimensions, colors, layouts, interactions, and behaviors are documented.

**For Developers**:
- Start with the layout structure (sidebar + main area)
- Build core components (ChatInput, ChatMessage, ConversationSidebar)
- Implement BaseModal system (reusable for all modals)
- Add SettingsModal with all 6 tabs
- Implement structured content rendering
- Add context attachment system
- Polish with animations and responsive behavior

**Key Success Criteria**:
- Layout matches specifications
- All interactions work as described
- Responsive behavior on mobile/tablet/desktop
- Theme system functional
- All modals and dialogs work
- Structured content renders correctly
- Context attachment system operational

**Maintenance**:
- Update this spec when adding new features
- Document any deviations from spec
- Keep component inventory current
- Update API integration section when backend is implemented

---

**Document Version**: 1.0
**Last Updated**: November 2025
**Author**: AI Assistant
**Purpose**: Complete rebuild specification
**Status**: Production-ready

---

*End of Component Architecture & Technical Specification*
