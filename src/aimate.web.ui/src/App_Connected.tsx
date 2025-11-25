import { useState, useRef, useEffect } from "react";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConversationSidebar, Conversation } from "./components/ConversationSidebar";
import { ChatHeader } from "./components/ChatHeader";
import { ChatMessage } from "./components/ChatMessage";
import { EmptyState } from "./components/EmptyState";
import { ChatInput } from "./components/ChatInput";
import { DebugPanel } from "./components/DebugPanel";
import { ThemeProvider } from "./components/ThemeProvider";
import { DebugProvider, useDebug } from "./components/DebugContext";
import { AuthProvider, useAuth } from "./context/AuthContext";
import { useWorkspaces, useConversations, useMessages } from "./hooks/useWorkspaces";
import { useChatStream } from "./hooks/useChat";
import { ScrollArea } from "./components/ui/scroll-area";
import { Sheet, SheetContent, SheetTitle } from "./components/ui/sheet";
import { Sparkles } from "lucide-react";
import { Toaster } from "./components/ui/sonner";
import { toast } from "sonner";

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  const { user, isAuthenticated } = useAuth();
  const { addLog } = useDebug();
  const scrollRef = useRef<HTMLDivElement>(null);

  // UI State
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

  // Backend Data - Get workspaces
  const { workspaces, isLoading: workspacesLoading, createWorkspace } = useWorkspaces();

  // Select first workspace or create default
  const [activeWorkspaceId, setActiveWorkspaceId] = useState<string | null>(null);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);

  useEffect(() => {
    if (workspaces && workspaces.length > 0 && !activeWorkspaceId) {
      setActiveWorkspaceId(workspaces[0].id);
    }
  }, [workspaces, activeWorkspaceId]);

  // Get conversations for active workspace
  const {
    conversations,
    isLoading: conversationsLoading,
    createConversation,
    deleteConversation,
    updateConversation,
  } = useConversations(activeWorkspaceId);

  // Get messages for active conversation
  const {
    messages,
    isLoading: messagesLoading,
  } = useMessages(activeConversationId);

  // Chat streaming
  const [streamingMessage, setStreamingMessage] = useState("");
  const { streamMessage, isStreaming, streamedContent } = useChatStream({
    onChunk: (chunk) => {
      // Update streaming message as chunks arrive
      setStreamingMessage(prev => prev + (chunk.choices[0]?.delta?.content || ''));
    },
    onComplete: (fullContent) => {
      addLog({
        action: 'AI response complete',
        api: 'api/v1/chat/completions/stream',
        payload: { length: fullContent.length },
        type: 'success'
      });
      setStreamingMessage("");
    },
  });

  // Auto-scroll
  const scrollToBottom = () => {
    if (scrollRef.current) {
      scrollRef.current.scrollIntoView({ behavior: "smooth" });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isStreaming, streamingMessage]);

  // Auto-close sidebar on resize
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 1024 && sidebarOpen) {
        setSidebarOpen(false);
      }
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [sidebarOpen]);

  // Create new conversation
  const handleCreateConversation = () => {
    if (!activeWorkspaceId) {
      toast.error("No active workspace");
      return;
    }

    createConversation({
      title: "New Conversation",
      model: selectedModel,
    });

    setMobileSidebarOpen(false);

    addLog({
      action: 'New conversation created',
      api: 'api/v1/workspaces/{id}/conversations',
      payload: { workspaceId: activeWorkspaceId },
      type: 'success'
    });
  };

  // Send message with streaming
  const handleSendMessage = async (content: string) => {
    if (!activeConversationId) {
      // Create new conversation first
      handleCreateConversation();
      // Wait for it to be created, then send message
      setTimeout(() => handleSendMessage(content), 500);
      return;
    }

    addLog({
      action: 'User message sent',
      api: 'api/v1/chat/completions/stream',
      payload: { message: content, conversationId: activeConversationId },
      type: 'info'
    });

    // Prepare chat messages for API
    const chatMessages = [
      ...(messages || []).map(m => ({
        role: m.role as 'user' | 'assistant' | 'system',
        content: m.content,
      })),
      {
        role: 'user' as const,
        content,
      },
    ];

    // Stream the response
    await streamMessage({
      model: selectedModel,
      messages: chatMessages,
      temperature: 0.7,
      stream: true,
    });
  };

  // Handle message edit
  const handleEditMessage = (messageId: string, newContent: string) => {
    // For now, just show a toast - full implementation would need backend support
    toast.info("Message editing requires backend implementation");
  };

  // Handle regenerate
  const handleRegenerateResponse = () => {
    if (!messages || messages.length === 0) return;

    const lastUserMessage = [...messages].reverse().find(m => m.role === 'user');
    if (lastUserMessage) {
      handleSendMessage(lastUserMessage.content);
    }
  };

  // Handle conversation actions
  const handleDeleteConversation = (id: string) => {
    deleteConversation(id);
    if (activeConversationId === id) {
      setActiveConversationId(null);
    }

    addLog({
      action: 'Conversation deleted',
      api: 'api/v1/conversations/{id}',
      payload: { conversationId: id },
      type: 'warning'
    });
  };

  const handleRenameConversation = (id: string, newTitle: string) => {
    updateConversation({
      id,
      data: { title: newTitle },
    });
  };

  const handleCloneConversation = (id: string) => {
    toast.info("Clone conversation requires backend implementation");
  };

  // Toggle sidebar
  const toggleSidebar = () => {
    if (window.innerWidth < 768) {
      setMobileSidebarOpen((prev) => !prev);
    } else {
      setSidebarOpen((prev) => !prev);
    }
  };

  // Toggle model
  const handleToggleModel = (modelId: string) => {
    setEnabledModels((prev) => {
      const newState = { ...prev, [modelId]: !prev[modelId] };
      if (modelId === selectedModel && !newState[modelId]) {
        const firstEnabled = Object.keys(newState).find(key => newState[key]);
        if (firstEnabled) {
          setSelectedModel(firstEnabled);
        }
      }
      return newState;
    });
  };

  // Convert backend conversations to sidebar format
  const conversationList: Conversation[] = (conversations || []).map((conv) => ({
    id: conv.id,
    title: conv.title,
    lastMessage: "Click to view messages",
    timestamp: new Date(conv.createdAt),
  }));

  // Show loading state
  if (!isAuthenticated) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4">Authentication Required</h2>
          <p className="text-gray-600 dark:text-gray-400">
            Please log in to use the application
          </p>
        </div>
      </div>
    );
  }

  if (workspacesLoading || conversationsLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="flex items-center gap-2">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600"></div>
          <span>Loading...</span>
        </div>
      </div>
    );
  }

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
                onNewConversation={handleCreateConversation}
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
                onNewConversation={handleCreateConversation}
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
              onNewChat={handleCreateConversation}
              onToggleSidebar={toggleSidebar}
              sidebarOpen={sidebarOpen || mobileSidebarOpen}
              selectedModel={selectedModel}
              onModelChange={setSelectedModel}
              enabledModels={enabledModels}
            />

            <div className="flex-1 overflow-hidden">
              <ScrollArea className="h-full">
                {!messages || messages.length === 0 ? (
                  <EmptyState onSendMessage={handleSendMessage} />
                ) : (
                  <div className="max-w-4xl mx-auto px-4 py-6 space-y-6">
                    {messages.map((message, index) => (
                      <ChatMessage
                        key={message.id}
                        role={message.role === 'system' ? 'assistant' : message.role}
                        content={message.content}
                        timestamp={new Date(message.createdAt).toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                        structuredContent={message.metadata?.structuredContent}
                        onEdit={
                          message.role === "user"
                            ? (newContent) => handleEditMessage(message.id, newContent)
                            : undefined
                        }
                        onRegenerate={
                          message.role === "assistant" &&
                          index === messages.length - 1 &&
                          !isStreaming
                            ? handleRegenerateResponse
                            : undefined
                        }
                      />
                    ))}

                    {/* Streaming message */}
                    {isStreaming && streamingMessage && (
                      <ChatMessage
                        role="assistant"
                        content={streamingMessage}
                        timestamp={new Date().toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                      />
                    )}

                    {/* Typing indicator */}
                    {isStreaming && !streamingMessage && (
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
                <ChatInput onSend={handleSendMessage} disabled={isStreaming} />
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
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ThemeProvider defaultTheme="dark">
          <DebugProvider>
            <App />
          </DebugProvider>
        </ThemeProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default AppWrapper;
