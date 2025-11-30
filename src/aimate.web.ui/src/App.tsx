import { useState, useRef, useEffect } from "react";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConversationSidebar, Conversation } from "./components/ConversationSidebar";
import { ChatHeader } from "./components/ChatHeader";
import { ChatMessage } from "./components/ChatMessage";
import { EmptyState } from "./components/EmptyState";
import { ChatInput } from "./components/ChatInput";
import { DebugPanel } from "./components/DebugPanel";
import { ShowcaseModeIndicator } from "./components/ShowcaseModeIndicator";
import { ThemeProvider } from "./components/ThemeProvider";
import { DebugProvider, useDebug } from "./components/DebugContext";
import { AuthProvider } from "./context/AuthContext";
import { AdminSettingsProvider } from "./context/AdminSettingsContext";
import { UserSettingsProvider } from "./context/UserSettingsContext";
import { AppDataProvider, useAppData } from "./context/AppDataContext";
import { AppConfig } from "./utils/config";
import { ScrollArea } from "./components/ui/scroll-area";
import { Sheet, SheetContent, SheetTitle } from "./components/ui/sheet";
import { Sparkles } from "lucide-react";
import { Toaster } from "./components/ui/sonner";
import { toast } from "sonner";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function ChatApp() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const [selectedModel, setSelectedModel] = useState<string>("gpt-4-turbo");
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);

  const { addLog } = useDebug();
  const scrollRef = useRef<HTMLDivElement>(null);

  // Get all our data from context
  const { chat, conversations, workspaces, admin } = useAppData();

  // Load messages when conversation changes
  useEffect(() => {
    if (activeConversationId) {
      chat.loadMessages(activeConversationId);
      addLog({
        action: 'Loading messages',
        category: 'chat:messages',
        api: 'api/v1/chat/messages',
        payload: { conversationId: activeConversationId },
        type: 'info'
      });
    }
  }, [activeConversationId, chat.loadMessages, addLog]);

  // Auto-scroll to bottom when messages change
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [chat.messages, chat.streaming]);

  // Handle responsive sidebar
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 1024 && sidebarOpen) {
        setSidebarOpen(false);
      }
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [sidebarOpen]);

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleNewConversation = async () => {
    try {
      const newConv = await conversations.createConversation({
        title: "New Conversation",
        workspaceId: workspaces.currentWorkspace?.id,
      });

      setActiveConversationId(newConv.id);
      setMobileSidebarOpen(false);

      addLog({
        action: 'New conversation created',
        api: 'api/v1/conversations',
        payload: { conversationId: newConv.id },
        type: 'success',
        category: 'chat:conversation'
      });

      toast.success("New conversation created");
    } catch (err) {
      console.error('Failed to create conversation:', err);
      toast.error("Failed to create conversation");
    }
  };

  const handleSendMessage = async (content: string) => {
    // Determine which conversation to use
    let targetConversationId = activeConversationId;

    if (!targetConversationId) {
      // Create new conversation first
      try {
        const newConv = await conversations.createConversation({
          title: content.substring(0, 50) + (content.length > 50 ? "..." : ""),
          workspaceId: workspaces.currentWorkspace?.id,
        });
        setActiveConversationId(newConv.id);
        targetConversationId = newConv.id;
      } catch (err) {
        toast.error("Failed to create conversation");
        return;
      }
    }

    try {
      addLog({
        action: 'Sending message',
        api: 'api/v1/chat/send',
        payload: { message: content, conversationId: targetConversationId, model: selectedModel },
        type: 'info',
        category: 'chat:message'
      });

      await chat.sendMessage(content, {
        conversationId: targetConversationId,
        workspaceId: workspaces.currentWorkspace?.id,
        model: selectedModel,
      });

      addLog({
        action: 'Message sent successfully',
        api: 'api/v1/chat/send',
        type: 'success',
        category: 'chat:message'
      });

      // Update conversation title if this is the first message
      const conv = conversations.conversations.find(c => c.id === targetConversationId);
      if (conv && conv.messageCount === 0) {
        await conversations.updateConversation(targetConversationId, {
          title: content.substring(0, 50) + (content.length > 50 ? "..." : ""),
        });
      }
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        console.error('Failed to send message:', err);
        addLog({
          action: 'Failed to send message',
          api: 'api/v1/chat/send',
          payload: { error: String(err) },
          type: 'error',
          category: 'chat:message'
        });
        toast.error("Failed to send message");
      }
    }
  };

  const handleEditMessage = async (messageId: string, newContent: string) => {
    try {
      await chat.editMessage(messageId, newContent);
      toast.success("Message updated");

      // Regenerate response after editing
      setTimeout(() => {
        handleRegenerateResponse();
      }, 100);
    } catch (err) {
      console.error('Failed to edit message:', err);
      toast.error("Failed to edit message");
    }
  };

  const handleRegenerateResponse = async () => {
    if (!chat.messages.length) return;

    // Find the last assistant message
    const lastAssistantMsg = [...chat.messages].reverse().find(m => m.role === 'assistant');
    if (!lastAssistantMsg) return;

    try {
      addLog({
        action: 'Regenerating response',
        api: 'api/v1/chat/regenerate',
        payload: { messageId: lastAssistantMsg.id },
        type: 'info',
        category: 'chat:regenerate'
      });

      await chat.regenerateMessage(lastAssistantMsg.id);

      toast.success("Response regenerated");
    } catch (err) {
      console.error('Failed to regenerate:', err);
      toast.error("Failed to regenerate response");
    }
  };

  const handleDeleteConversation = async (id: string) => {
    try {
      await conversations.deleteConversation(id);

      if (activeConversationId === id) {
        const remaining = conversations.conversations.filter(c => c.id !== id);
        setActiveConversationId(remaining.length > 0 ? remaining[0].id : null);
      }

      addLog({
        action: 'Conversation deleted',
        api: 'api/v1/conversations',
        payload: { conversationId: id },
        type: 'warning',
        category: 'chat:conversation'
      });

      toast.success("Conversation deleted");
    } catch (err) {
      console.error('Failed to delete conversation:', err);
      toast.error("Failed to delete conversation");
    }
  };

  const handleRenameConversation = async (id: string, newTitle: string) => {
    try {
      await conversations.updateConversation(id, { title: newTitle });
      toast.success("Conversation renamed");
    } catch (err) {
      console.error('Failed to rename conversation:', err);
      toast.error("Failed to rename conversation");
    }
  };

  const handleCloneConversation = async (id: string) => {
    try {
      const conv = conversations.conversations.find(c => c.id === id);
      if (!conv) return;

      const cloned = await conversations.createConversation({
        title: `${conv.title} (Copy)`,
        workspaceId: conv.workspaceId,
        tags: conv.tags,
      });

      setActiveConversationId(cloned.id);

      addLog({
        action: 'Conversation cloned',
        api: 'api/v1/conversations',
        payload: { originalId: id, clonedId: cloned.id },
        type: 'success',
        category: 'chat:conversation'
      });

      toast.success("Conversation duplicated");
    } catch (err) {
      console.error('Failed to clone conversation:', err);
      toast.error("Failed to duplicate conversation");
    }
  };

  const toggleSidebar = () => {
    if (window.innerWidth < 768) {
      setMobileSidebarOpen((prev) => !prev);
    } else {
      setSidebarOpen((prev) => !prev);
    }
  };

  // ============================================================================
  // PREPARE DATA FOR UI
  // ============================================================================

  const conversationList: Conversation[] = conversations.conversations.map((conv) => ({
    id: conv.id,
    title: conv.title,
    lastMessage: `${conv.messageCount} messages`,
    timestamp: new Date(conv.lastMessageAt || conv.createdAt),
  }));

  // Get available models from admin
  // Filter to only show models with valid enabled connections (unless in offline mode with no connections)
  const allModels = admin?.models || [];
  const enabledConnectionNames = new Set(
    admin?.connections?.filter(c => c.isActive || c.enabled).map(c => c.name) || []
  );
  const availableModels = allModels.filter(model => {
    // Always show models that have a matching enabled connection
    if (enabledConnectionNames.has(model.provider)) return true;
    // In offline mode, show all models as fallback (for testing)
    if (AppConfig.isOfflineMode() && enabledConnectionNames.size === 0) return true;
    // Otherwise filter out models without valid connections
    return false;
  });
  const enabledModels = availableModels.reduce((acc, model) => {
    acc[model.id] = model.isActive || false;
    return acc;
  }, {} as Record<string, boolean>);

  const handleToggleModel = async (modelId: string) => {
    if (!admin) return;

    try {
      await admin.toggleModel(modelId);
      toast.success("Model status updated");
    } catch (err) {
      console.error('Failed to toggle model:', err);
      toast.error("Failed to update model");
    }
  };

  // ============================================================================
  // RENDER
  // ============================================================================

  return (
    <div className="flex h-screen bg-gray-50 dark:bg-gray-950">
      {/* Desktop Sidebar */}
      {sidebarOpen && (
        <div className="hidden md:block w-80 shrink-0">
          <ConversationSidebar
            conversations={conversationList}
            activeConversationId={activeConversationId}
            onSelectConversation={setActiveConversationId}
            onNewConversation={handleNewConversation}
            onDeleteConversation={handleDeleteConversation}
            onRenameConversation={handleRenameConversation}
            onCloneConversation={handleCloneConversation}
            enabledModels={enabledModels}
            onToggleModel={handleToggleModel}
            hasMore={conversations.hasMore}
            onLoadMore={conversations.loadMore}
            loading={conversations.loading}
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
            onNewConversation={handleNewConversation}
            onDeleteConversation={handleDeleteConversation}
            onRenameConversation={handleRenameConversation}
            onCloneConversation={handleCloneConversation}
            onClose={() => setMobileSidebarOpen(false)}
            enabledModels={enabledModels}
            onToggleModel={handleToggleModel}
            hasMore={conversations.hasMore}
            onLoadMore={conversations.loadMore}
            loading={conversations.loading}
          />
        </SheetContent>
      </Sheet>

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col min-w-0">
        <ChatHeader
          onNewChat={handleNewConversation}
          onToggleSidebar={toggleSidebar}
          sidebarOpen={sidebarOpen || mobileSidebarOpen}
          selectedModel={selectedModel}
          onModelChange={setSelectedModel}
          enabledModels={enabledModels}
          availableModels={availableModels.map(m => ({
            id: m.id,
            name: m.name,
            provider: m.provider,
            contextWindow: m.contextWindow,
            capabilities: m.capabilities,
            color: '#888888', // Default color
          }))}
        />

        <div className="flex-1 overflow-hidden">
          <ScrollArea className="h-full">
            {chat.messages.length === 0 && !chat.loading ? (
              <EmptyState onSendMessage={handleSendMessage} />
            ) : (
              <div className="max-w-4xl mx-auto px-4 py-6 space-y-6">
                {chat.messages.map((message, index) => (
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
                        index === chat.messages.length - 1 &&
                        !chat.streaming
                        ? handleRegenerateResponse
                        : undefined
                    }
                  />
                ))}

                {chat.streaming && (
                  <div className="flex gap-3 items-center">
                    <div className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center shrink-0">
                      <Sparkles className="w-4 h-4 text-white" />
                    </div>
                    <div className="flex-1 bg-white dark:bg-gray-900 rounded-2xl px-4 py-3 border border-gray-200 dark:border-gray-800">
                      <div className="flex gap-1">
                        <div className="w-2 h-2 bg-purple-500 rounded-full animate-bounce" style={{ animationDelay: "0ms" }} />
                        <div className="w-2 h-2 bg-purple-500 rounded-full animate-bounce" style={{ animationDelay: "150ms" }} />
                        <div className="w-2 h-2 bg-purple-500 rounded-full animate-bounce" style={{ animationDelay: "300ms" }} />
                      </div>
                    </div>
                  </div>
                )}

                <div ref={scrollRef} />
              </div>
            )}
          </ScrollArea>
        </div>

        <ChatInput
          onSend={handleSendMessage}
          disabled={chat.streaming}
        />

        <DebugPanel />
      </div>

      <ShowcaseModeIndicator />
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider defaultTheme="dark">
        <DebugProvider>
          <AuthProvider>
            <AdminSettingsProvider>
              <UserSettingsProvider>
                <AppDataProvider isAdmin={true}>
                  <Toaster />
                  <ChatApp />
                </AppDataProvider>
              </UserSettingsProvider>
            </AdminSettingsProvider>
          </AuthProvider>
        </DebugProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;