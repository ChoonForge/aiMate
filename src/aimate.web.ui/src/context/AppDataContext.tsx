/**
 * App Data Context
 * 
 * Centralized context for all app data using hooks
 */

import React, { createContext, useContext, ReactNode, useMemo } from 'react';
import {
  useChat,
  useConversations,
  useWorkspaces,
  useKnowledge,
  useProjects,
  useFiles,
  useAdmin,
  useSettings,
  useUsage,
  useTools,
} from '../hooks';

interface AppDataContextType {
  // Chat
  chat: ReturnType<typeof useChat>;
  
  // Conversations
  conversations: ReturnType<typeof useConversations>;
  
  // Workspaces
  workspaces: ReturnType<typeof useWorkspaces>;
  
  // Knowledge
  knowledge: ReturnType<typeof useKnowledge>;
  
  // Projects
  projects: ReturnType<typeof useProjects>;
  
  // Files
  files: ReturnType<typeof useFiles>;

  // Tools (MCP)
  tools: ReturnType<typeof useTools>;

  // Admin (optional - only for admin users)
  admin?: ReturnType<typeof useAdmin>;
  
  // Settings
  settings: ReturnType<typeof useSettings>;
  
  // Usage
  usage: ReturnType<typeof useUsage>;
}

const AppDataContext = createContext<AppDataContextType | undefined>(undefined);

interface AppDataProviderProps {
  children: ReactNode;
  isAdmin?: boolean;
}

export function AppDataProvider({ children, isAdmin = false }: AppDataProviderProps) {
  const workspaces = useWorkspaces();
  const conversations = useConversations(workspaces.currentWorkspace?.id);
  const chat = useChat();
  const knowledge = useKnowledge(workspaces.currentWorkspace?.id);
  const projects = useProjects(workspaces.currentWorkspace?.id);
  const files = useFiles(workspaces.currentWorkspace?.id);
  const tools = useTools();
  const admin = isAdmin ? useAdmin() : undefined;
  const settings = useSettings();
  const usage = useUsage();

  const value = useMemo(() => ({
    chat,
    conversations,
    workspaces,
    knowledge,
    projects,
    files,
    tools,
    admin,
    settings,
    usage,
  }), [chat, conversations, workspaces, knowledge, projects, files, tools, admin, settings, usage]);

  return (
    <AppDataContext.Provider value={value}>
      {children}
    </AppDataContext.Provider>
  );
}

export function useAppData() {
  const context = useContext(AppDataContext);
  if (context === undefined) {
    throw new Error('useAppData must be used within an AppDataProvider');
  }
  return context;
}
