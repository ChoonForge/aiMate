import { useState, useRef, useEffect } from "react";
import { ConversationSidebar, Conversation } from "./components/ConversationSidebar";
import { ChatHeader } from "./components/ChatHeader";
import { ChatMessage } from "./components/ChatMessage";
import { EmptyState } from "./components/EmptyState";
import { ChatInput } from "./components/ChatInput";
import { DebugPanel } from "./components/DebugPanel";
import { ShowcaseModeIndicator } from "./components/ShowcaseModeIndicator";
import { ThemeProvider } from "./components/ThemeProvider";
import { DebugProvider, useDebug } from "./components/DebugContext";
import { ScrollArea } from "./components/ui/scroll-area";
import { Sheet, SheetContent, SheetTitle } from "./components/ui/sheet";
import { Sparkles } from "lucide-react";
import { Toaster } from "./components/ui/sonner";

interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: string;
  structuredContent?: any;
}

interface ConversationData {
  id: string;
  messages: Message[];
  title: string;
  createdAt: Date;
}

const mockResponses = [
  "I'm here to help! What would you like to know? Here are some **key points**:\n\n- I can answer questions\n- Provide explanations\n- Help with code like `console.log('hello')`\n\nLet me know how I can assist!",
  "That's a great question. Let me think about that...\n\n```javascript\nconst answer = 'Here is some sample code';\nconsole.log(answer);\n```",
  "I'd be happy to help you with that. Here's what I think:\n\n1. First consideration\n2. Second point\n3. Final thought",
  "Thanks for asking! Based on what you've mentioned, here are my insights:\n\n- **Important**: Take note of this\n- *Consideration*: Keep this in mind\n- Regular point here",
  "Interesting! Let me provide some insights on that with a code example:\n\n```python\ndef example():\n    return \"This is Python code\"\n```\n\nHope this helps!",
  "I understand what you're asking. Here's my response with *formatted* text and **bold** sections.",
];

const mockConversations: ConversationData[] = [
  {
    id: "1",
    title: "How do I center a div in CSS?",
    createdAt: new Date(Date.now() - 1000 * 60 * 30), // 30 mins ago
    messages: [
      {
        id: "1-1",
        role: "user",
        content: "How do I center a div in CSS?",
        timestamp: "2:30 PM",
      },
      {
        id: "1-2",
        role: "assistant",
        content: "Great question! There are several ways to center a div in CSS. Here are the most common methods:\n\n**Using Flexbox (Modern & Recommended):**\n```css\n.container {\n  display: flex;\n  justify-content: center;\n  align-items: center;\n}\n```\n\n**Using Grid:**\n```css\n.container {\n  display: grid;\n  place-items: center;\n}\n```\n\n**Traditional Method:**\n```css\n.centered {\n  margin: 0 auto;\n  width: 50%; /* Must have a width */\n}\n```\n\nThe flexbox method is usually the best choice for modern projects!",
        timestamp: "2:31 PM",
      },
      {
        id: "1-3",
        role: "user",
        content: "Thanks! What if I want to center it vertically as well?",
        timestamp: "2:32 PM",
      },
      {
        id: "1-4",
        role: "assistant",
        content: "Perfect follow-up! The **Flexbox method** I showed you actually centers both horizontally AND vertically when you use both properties:\n\n```css\n.container {\n  display: flex;\n  justify-content: center;  /* Horizontal */\n  align-items: center;      /* Vertical */\n  height: 100vh;            /* Full viewport height */\n}\n```\n\nYou can also use the **Grid** method which is even simpler:\n```css\n.container {\n  display: grid;\n  place-items: center;\n  height: 100vh;\n}\n```\n\nBoth achieve perfect centering in just a few lines! 🎯",
        timestamp: "2:32 PM",
      },
    ],
  },
  {
    id: "2",
    title: "Explain React hooks",
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
    messages: [
      {
        id: "2-1",
        role: "user",
        content: "Can you explain React hooks to me?",
        timestamp: "12:15 PM",
      },
      {
        id: "2-2",
        role: "assistant",
        content: "Of course! **React Hooks** are functions that let you use React features in functional components. Here are the most important ones:\n\n**useState** - Manages component state:\n```jsx\nconst [count, setCount] = useState(0);\n```\n\n**useEffect** - Handles side effects:\n```jsx\nuseEffect(() => {\n  // Code runs after render\n  document.title = `Count: ${count}`;\n}, [count]); // Dependencies array\n```\n\n**useRef** - Creates mutable references:\n```jsx\nconst inputRef = useRef(null);\n```\n\n**Key Benefits:**\n- ✅ Cleaner code than class components\n- ✅ Easier to reuse stateful logic\n- ✅ Better code organization\n\nWould you like me to explain any specific hook in detail?",
        timestamp: "12:16 PM",
      },
    ],
  },
];

function App() {
  const [conversations, setConversations] = useState<ConversationData[]>(mockConversations);
  const [activeConversationId, setActiveConversationId] = useState<string | null>("1");
  const [isTyping, setIsTyping] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const [selectedModel, setSelectedModel] = useState("gpt-4");
  const [enabledModels, setEnabledModels] = useState<Record<string, boolean>>({
    "gpt-4": true,
    "gpt-4-turbo": true,
    "gpt-3.5-turbo": true,
    "claude-3-opus": true,
    "claude-3-sonnet": true,
    "structured-gpt": true,
  });
  const { addLog } = useDebug();
  const scrollRef = useRef<HTMLDivElement>(null);

  const activeConversation = conversations.find(
    (conv) => conv.id === activeConversationId
  );
  const messages = activeConversation?.messages || [];

  const scrollToBottom = () => {
    if (scrollRef.current) {
      scrollRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isTyping]);

  // Auto-close sidebar on window resize below breakpoint
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 1024 && sidebarOpen) {
        setSidebarOpen(false);
      }
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [sidebarOpen]);

  const getTimestamp = () => {
    const now = new Date();
    return now.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  };

  const generateConversationTitle = (firstMessage: string) => {
    return firstMessage.length > 50
      ? firstMessage.substring(0, 50) + "..."
      : firstMessage;
  };

  const createNewConversation = () => {
    const newConv: ConversationData = {
      id: Date.now().toString(),
      messages: [],
      title: "New Conversation",
      createdAt: new Date(),
    };
    setConversations((prev) => [newConv, ...prev]);
    setActiveConversationId(newConv.id);
    setMobileSidebarOpen(false);
    
    addLog({
      action: 'New conversation created',
      api: 'api/v1/conversations/create',
      payload: { conversationId: newConv.id },
      type: 'success'
    });
  };

  const handleSendMessage = async (content: string) => {
    if (!activeConversationId) {
      createNewConversation();
      // Wait for state to update, then send message
      setTimeout(() => handleSendMessage(content), 10);
      return;
    }

    const userMessage: Message = {
      id: Date.now().toString(),
      role: "user",
      content,
      timestamp: getTimestamp(),
    };
    
    addLog({
      action: 'User message sent',
      api: 'api/v1/chat/sendMessage',
      payload: { message: content, conversationId: activeConversationId },
      type: 'info'
    });

    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === activeConversationId) {
          const updatedMessages = [...conv.messages, userMessage];
          const title =
            conv.messages.length === 0
              ? generateConversationTitle(content)
              : conv.title;
          return { ...conv, messages: updatedMessages, title };
        }
        return conv;
      })
    );

    setIsTyping(true);
    
    addLog({
      action: 'Requesting AI response',
      api: 'api/v1/chat/completion',
      payload: { model: selectedModel, conversationId: activeConversationId },
      type: 'info'
    });

    // Simulate AI response delay
    setTimeout(() => {
      let aiMessage: Message;

      // Check for structured content triggers
      const lowerContent = content.trim().toLowerCase();
      
      if (lowerContent === "gettable") {
        const structuredData = {
          type: "panel.table",
          title: "Active Projects",
          columns: ["Key", "Name", "Owner", "Tasks"],
          rows: [
            ["CF", "ChoonForge", "rich", 12],
            ["AM", "aiMate", "rich", 8],
            ["WA", "WebApp Builder", "john", 24],
            ["DS", "Design System", "jane", 15],
            ["AP", "API Gateway", "bob", 32],
            ["DB", "Database Migration", "alice", 7],
            ["ML", "ML Pipeline", "rich", 19],
            ["UI", "UI Component Library", "sarah", 11],
            ["BE", "Backend Services", "mike", 28],
            ["FE", "Frontend Framework", "emma", 5]
          ],
          rowActions: [
            {
              type: "action.callTool",
              title: "Open",
              tool: "project.get",
              args: {
                key: "$row[0]"
              }
            }
          ],
          actions: [
            {
              type: "action.callTool",
              title: "New Project",
              tool: "project.newForm"
            }
          ],
          items: null,
          itemsList: null,
          fields: null,
          submit: null,
          onSuccess: null
        };

        aiMessage = {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: `Projects loaded.\n\n\`\`\`structured\n${JSON.stringify(structuredData)}\n\`\`\``,
          timestamp: getTimestamp(),
          structuredContent: structuredData,
        };
      } else if (lowerContent === "getlist") {
        const structuredData = {
          type: "panel.list",
          title: "Frequently Asked Questions",
          items: [
            {
              title: "How do I reset my password?",
              content: "Click on the 'Forgot Password' link on the login page. Enter your email address, and we'll send you instructions to reset your password."
            },
            {
              title: "Can I export my chat history?",
              content: "Yes! Go to Settings > Data & Privacy > Export Data. You can download your entire chat history in JSON format."
            },
            {
              title: "What models are available?",
              content: "We offer multiple AI models including GPT-4, GPT-4 Turbo, Claude 3 Opus, Claude 3 Sonnet, and our specialized Structured GPT for data visualization."
            },
            {
              title: "How do I enable dark mode?",
              content: "Dark mode is enabled by default. You can toggle between light and dark mode in Settings > Appearance > Theme."
            },
            {
              title: "Can I use this on mobile?",
              content: "Absolutely! Our interface is fully responsive and works seamlessly on mobile devices, tablets, and desktops."
            },
            {
              title: "How do I create a new project?",
              content: "Navigate to the Projects section in the sidebar, then click the 'New Project' button. Fill in the project details and click Save."
            }
          ]
        };
        
        aiMessage = {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: `FAQs loaded.\n\n\`\`\`structured\n${JSON.stringify(structuredData)}\n\`\`\``,
          timestamp: getTimestamp(),
          structuredContent: structuredData,
        };
      } else if (lowerContent === "getkv") {
        const structuredData = {
          type: "panel.kv",
          title: "Project Details",
          kvPairs: [
            { key: "Project Key", value: "CF" },
            { key: "Project Name", value: "ChoonForge" },
            { key: "Owner", value: "rich" },
            { key: "Total Tasks", value: "12" },
            { key: "Status", value: "Active" },
            { key: "Created Date", value: "2024-01-15" },
            { key: "Last Updated", value: "2024-03-10" },
            { key: "Priority", value: "High" },
            { key: "Budget", value: "$50,000" },
            { key: "Completion", value: "65%" }
          ]
        };
        
        aiMessage = {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: `Project details loaded.\n\n\`\`\`structured\n${JSON.stringify(structuredData)}\n\`\`\``,
          timestamp: getTimestamp(),
          structuredContent: structuredData,
        };
      } else if (lowerContent === "getform") {
        const structuredData = {
          type: "panel.form",
          title: "Edit Project: ChoonForge",
          fields: [
            { name: "projectKey", label: "Project Key", type: "text", value: "CF", required: true },
            { name: "projectName", label: "Project Name", type: "text", value: "ChoonForge", required: true },
            { name: "owner", label: "Owner", type: "text", value: "rich", required: true },
            { name: "email", label: "Email", type: "email", value: "rich@example.com", required: true },
            { name: "status", label: "Status", type: "text", value: "Active", required: false },
            { name: "priority", label: "Priority", type: "text", value: "High", required: false },
            { name: "budget", label: "Budget", type: "text", value: "$50,000", required: false },
            { name: "description", label: "Description", type: "textarea", value: "A comprehensive project management system with advanced features for team collaboration.", required: false }
          ],
          submit: {
            type: "action.callTool",
            title: "Save",
            tool: "project.update",
            args: {}
          }
        };
        
        aiMessage = {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: `Project form loaded.\n\n\`\`\`structured\n${JSON.stringify(structuredData)}\n\`\`\``,
          timestamp: getTimestamp(),
          structuredContent: structuredData,
        };
      } else {
        aiMessage = {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content:
            mockResponses[Math.floor(Math.random() * mockResponses.length)],
          timestamp: getTimestamp(),
        };
      }

      setConversations((prev) =>
        prev.map((conv) =>
          conv.id === activeConversationId
            ? { ...conv, messages: [...conv.messages, aiMessage] }
            : conv
        )
      );
      setIsTyping(false);
      
      addLog({
        action: 'AI response received',
        api: 'api/v1/chat/completion',
        payload: { messageLength: aiMessage.content.length },
        type: 'success'
      });
    }, 1000 + Math.random() * 1000);
  };

  const handleEditMessage = (messageId: string, newContent: string) => {
    if (!activeConversationId) return;

    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === activeConversationId) {
          const messageIndex = conv.messages.findIndex((m) => m.id === messageId);
          if (messageIndex === -1) return conv;

          const updatedMessages = [...conv.messages];
          updatedMessages[messageIndex] = {
            ...updatedMessages[messageIndex],
            content: newContent,
          };

          // Remove all messages after the edited one
          const truncatedMessages = updatedMessages.slice(0, messageIndex + 1);

          return { ...conv, messages: truncatedMessages };
        }
        return conv;
      })
    );

    // Regenerate AI response
    setTimeout(() => {
      handleRegenerateResponse();
    }, 500);
  };

  const handleRegenerateResponse = () => {
    if (!activeConversationId) return;

    const conversation = conversations.find((c) => c.id === activeConversationId);
    if (!conversation || conversation.messages.length === 0) return;

    // Remove last AI message if exists
    setConversations((prev) =>
      prev.map((conv) => {
        if (conv.id === activeConversationId) {
          const messages = [...conv.messages];
          if (messages[messages.length - 1]?.role === "assistant") {
            messages.pop();
          }
          return { ...conv, messages };
        }
        return conv;
      })
    );

    setIsTyping(true);

    // Generate new response
    setTimeout(() => {
      const aiMessage: Message = {
        id: Date.now().toString(),
        role: "assistant",
        content:
          mockResponses[Math.floor(Math.random() * mockResponses.length)],
        timestamp: getTimestamp(),
      };

      setConversations((prev) =>
        prev.map((conv) =>
          conv.id === activeConversationId
            ? { ...conv, messages: [...conv.messages, aiMessage] }
            : conv
        )
      );
      setIsTyping(false);
    }, 1000 + Math.random() * 1000);
  };

  const handleDeleteConversation = (id: string) => {
    setConversations((prev) => prev.filter((conv) => conv.id !== id));
    if (activeConversationId === id) {
      const remaining = conversations.filter((conv) => conv.id !== id);
      setActiveConversationId(remaining.length > 0 ? remaining[0].id : null);
    }
    
    addLog({
      action: 'Conversation deleted',
      api: 'api/v1/conversations/delete',
      payload: { conversationId: id },
      type: 'warning'
    });
  };

  const handleRenameConversation = (id: string, newTitle: string) => {
    setConversations((prev) =>
      prev.map((conv) =>
        conv.id === id ? { ...conv, title: newTitle } : conv
      )
    );
  };

  const handleCloneConversation = (id: string) => {
    const conversation = conversations.find((conv) => conv.id === id);
    if (conversation) {
      const clonedConv: ConversationData = {
        id: Date.now().toString(),
        messages: [...conversation.messages],
        title: `${conversation.title} (Copy)`,
        createdAt: new Date(),
      };
      setConversations((prev) => [clonedConv, ...prev]);
      setActiveConversationId(clonedConv.id);
      
      addLog({
        action: 'Conversation cloned',
        api: 'api/v1/conversations/clone',
        payload: { originalId: id, clonedId: clonedConv.id, messageCount: conversation.messages.length },
        type: 'success'
      });
    }
  };

  const conversationList: Conversation[] = conversations.map((conv) => ({
    id: conv.id,
    title: conv.title,
    lastMessage:
      conv.messages[conv.messages.length - 1]?.content || "No messages yet",
    timestamp: conv.createdAt,
  }));

  const toggleSidebar = () => {
    if (window.innerWidth < 768) {
      setMobileSidebarOpen((prev) => !prev);
    } else {
      setSidebarOpen((prev) => !prev);
    }
  };

  const handleToggleModel = (modelId: string) => {
    setEnabledModels((prev) => {
      const newState = { ...prev, [modelId]: !prev[modelId] };
      // If disabling the currently selected model, switch to first enabled model
      if (modelId === selectedModel && !newState[modelId]) {
        const firstEnabled = Object.keys(newState).find(key => newState[key]);
        if (firstEnabled) {
          setSelectedModel(firstEnabled);
        }
      }
      return newState;
    });
  };

  return (
    <ThemeProvider defaultTheme="dark">
      <DebugProvider>
        <Toaster />
        <div className="flex h-screen bg-gray-50 dark:bg-gray-950">
      {/* Desktop Sidebar */}
      {sidebarOpen && (
        <div className="hidden md:block w-80 shrink-0">
          <ConversationSidebar
            conversations={conversationList}
            activeConversationId={activeConversationId}
            onSelectConversation={setActiveConversationId}
            onNewConversation={createNewConversation}
            onDeleteConversation={handleDeleteConversation}
            onRenameConversation={handleRenameConversation}
            onCloneConversation={handleCloneConversation}
            enabledModels={enabledModels}
            onToggleModel={handleToggleModel}
          />
        </div>
      )}

      {/* Mobile Sidebar */}
      <Sheet open={mobileSidebarOpen} onOpenChange={setMobileSidebarOpen}>
        <SheetContent side="left" className="p-0 w-80" aria-describedby={undefined}>
          <SheetTitle className="sr-only">Navigation Menu</SheetTitle>
          <ConversationSidebar
            conversations={conversationList}
            activeConversationId={activeConversationId}
            onSelectConversation={(id) => {
              setActiveConversationId(id);
              setMobileSidebarOpen(false);
            }}
            onNewConversation={createNewConversation}
            onDeleteConversation={handleDeleteConversation}
            onRenameConversation={handleRenameConversation}
            onCloneConversation={handleCloneConversation}
            onClose={() => setMobileSidebarOpen(false)}
            enabledModels={enabledModels}
            onToggleModel={handleToggleModel}
          />
        </SheetContent>
      </Sheet>

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col min-w-0">
        <ChatHeader
          onNewChat={createNewConversation}
          onToggleSidebar={toggleSidebar}
          sidebarOpen={sidebarOpen || mobileSidebarOpen}
          selectedModel={selectedModel}
          onModelChange={setSelectedModel}
          enabledModels={enabledModels}
        />

        <div className="flex-1 overflow-hidden">
          <ScrollArea className="h-full">
            {messages.length === 0 ? (
              <EmptyState onSendMessage={handleSendMessage} />
            ) : (
              <div className="max-w-4xl mx-auto px-4 py-6 space-y-6">
                {messages.map((message, index) => (
                  <ChatMessage
                    key={message.id}
                    role={message.role}
                    content={message.content}
                    timestamp={message.timestamp}
                    structuredContent={message.structuredContent}
                    onEdit={
                      message.role === "user"
                        ? (newContent) => handleEditMessage(message.id, newContent)
                        : undefined
                    }
                    onRegenerate={
                      message.role === "assistant" &&
                      index === messages.length - 1 &&
                      !isTyping
                        ? handleRegenerateResponse
                        : undefined
                    }
                  />
                ))}

                {isTyping && (
                  <div className="flex gap-3 items-center">
                    <div className="relative h-8 w-8 shrink-0 rounded-full bg-gradient-to-r from-purple-600 to-blue-500 flex items-center justify-center animate-pulse">
                      <Sparkles className="h-4 w-4 text-white" />
                      <div className="absolute inset-0 rounded-full bg-purple-400 opacity-75 animate-ping"></div>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-gray-500 dark:text-gray-400">AI is thinking</span>
                      <div className="flex gap-1">
                        <span className="w-1 h-1 bg-gray-400 dark:bg-gray-600 rounded-full animate-bounce [animation-delay:-0.3s]"></span>
                        <span className="w-1 h-1 bg-gray-400 dark:bg-gray-600 rounded-full animate-bounce [animation-delay:-0.15s]"></span>
                        <span className="w-1 h-1 bg-gray-400 dark:bg-gray-600 rounded-full animate-bounce"></span>
                      </div>
                    </div>
                  </div>
                )}

                <div ref={scrollRef} />
              </div>
            )}
          </ScrollArea>
        </div>

        <div className="border-t border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950">
          <div className="max-w-4xl mx-auto px-4 py-4">
            <ChatInput onSend={handleSendMessage} disabled={isTyping} />
          </div>
        </div>
        
        <DebugPanel />
      </div>
    </div>
      </DebugProvider>
    </ThemeProvider>
  );
}

function AppWrapper() {
  return (
    <ThemeProvider defaultTheme="dark">
      <DebugProvider>
        <App />
      </DebugProvider>
    </ThemeProvider>
  );
}

export default AppWrapper;