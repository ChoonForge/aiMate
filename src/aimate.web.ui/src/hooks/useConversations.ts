/**
 * Conversations Hook
 * 
 * Manages conversation list, creation, and organization
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { conversationsService, ConversationDto, CreateConversationDto } from '../api/services';
import { AppConfig } from '../utils/config';

// Module-level cache to persist data across StrictMode remounts
let globalConversationsInitialized = false;
let globalConversationsCache: ConversationDto[] = [];

export function useConversations(workspaceId?: string) {
  const [conversations, setConversations] = useState<ConversationDto[]>(() => globalConversationsCache);
  const [loading, setLoading] = useState(!globalConversationsInitialized);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const isInitializedRef = useRef(globalConversationsInitialized);

  // ============================================================================
  // LOAD CONVERSATIONS
  // ============================================================================

  const loadConversations = useCallback(async (wsId?: string, forceReload = false) => {
    // Skip reload if already initialized in offline mode (preserve created conversations)
    // Use global flag to survive StrictMode remounts
    if (AppConfig.isOfflineMode() && globalConversationsInitialized && !forceReload) {
      console.log('[useConversations] Skipping reload - already initialized');
      return;
    }

    console.log('[useConversations] Loading conversations...');

    if (AppConfig.isOfflineMode()) {
      // Use mock conversations in offline mode
      const mockConvs: ConversationDto[] = [
        {
          id: 'conv-1',
          title: 'aiMate.nz Development Discussion',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 3600000).toISOString(),
          messageCount: 24,
          isPinned: true,
          isArchived: false,
          modelId: 'gpt-4',
          tags: ['development', 'planning'],
          createdAt: new Date(Date.now() - 86400000 * 3).toISOString(),
          updatedAt: new Date(Date.now() - 3600000).toISOString(),
        },
        {
          id: 'conv-2',
          title: 'Crisis Detection Testing',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 7200000).toISOString(),
          messageCount: 12,
          isPinned: false,
          isArchived: false,
          modelId: 'gpt-3.5-turbo',
          tags: ['testing', 'safety'],
          createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
          updatedAt: new Date(Date.now() - 7200000).toISOString(),
        },
        {
          id: 'conv-3',
          title: 'API Integration Planning',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 14400000).toISOString(),
          messageCount: 8,
          isPinned: false,
          isArchived: false,
          modelId: null,
          tags: ['api', 'backend'],
          createdAt: new Date(Date.now() - 86400000).toISOString(),
          updatedAt: new Date(Date.now() - 14400000).toISOString(),
        },
        {
          id: 'conv-4',
          title: 'Structured Content Examples',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 21600000).toISOString(),
          messageCount: 5,
          isPinned: false,
          isArchived: false,
          modelId: 'claude-3-opus',
          tags: ['demo', 'ui'],
          createdAt: new Date(Date.now() - 86400000 * 4).toISOString(),
          updatedAt: new Date(Date.now() - 21600000).toISOString(),
        },
        {
          id: 'conv-5',
          title: 'React Hooks Deep Dive',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 28800000).toISOString(),
          messageCount: 18,
          isPinned: false,
          isArchived: false,
          modelId: null,
          tags: ['react', 'learning'],
          createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
          updatedAt: new Date(Date.now() - 28800000).toISOString(),
        },
        {
          id: 'conv-6',
          title: 'TypeScript Best Practices',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 43200000).toISOString(),
          messageCount: 15,
          isPinned: false,
          isArchived: false,
          modelId: 'gpt-4',
          tags: ['typescript', 'code-quality'],
          createdAt: new Date(Date.now() - 86400000 * 6).toISOString(),
          updatedAt: new Date(Date.now() - 43200000).toISOString(),
        },
        {
          id: 'conv-7',
          title: 'Safety Infrastructure Design',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 57600000).toISOString(),
          messageCount: 32,
          isPinned: true,
          isArchived: false,
          modelId: 'claude-3-sonnet',
          tags: ['safety', 'architecture'],
          createdAt: new Date(Date.now() - 86400000 * 7).toISOString(),
          updatedAt: new Date(Date.now() - 57600000).toISOString(),
        },
        {
          id: 'conv-8',
          title: 'NZ Resource Verification',
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - 72000000).toISOString(),
          messageCount: 9,
          isPinned: false,
          isArchived: false,
          modelId: null,
          tags: ['nz', 'resources'],
          createdAt: new Date(Date.now() - 86400000 * 8).toISOString(),
          updatedAt: new Date(Date.now() - 72000000).toISOString(),
        },
      ];

      // Add more mock conversations for pagination testing
      for (let i = 9; i <= 50; i++) {
        mockConvs.push({
          id: `conv-${i}`,
          title: `Conversation ${i}`,
          workspaceId: wsId || 'default',
          lastMessageAt: new Date(Date.now() - (86400000 * i)).toISOString(),
          messageCount: Math.floor(Math.random() * 30) + 1,
          isPinned: false,
          isArchived: false,
          modelId: null,
          tags: ['test'],
          createdAt: new Date(Date.now() - (86400000 * i)).toISOString(),
          updatedAt: new Date(Date.now() - (86400000 * i)).toISOString(),
        });
      }

      setConversations(mockConvs);
      globalConversationsCache = mockConvs;
      setHasMore(mockConvs.length >= pageSize);
      setLoading(false);
      isInitializedRef.current = true;
      globalConversationsInitialized = true;
      return;
    }

    try {
      setLoading(true);
      const data = await conversationsService.getConversations(wsId);
      setConversations(data);
      globalConversationsCache = data;
      setHasMore(data.length >= pageSize);
      setError(null);
      isInitializedRef.current = true;
      globalConversationsInitialized = true;
    } catch (err) {
      console.error('[useConversations] Failed to load conversations:', err);
      setError('Failed to load conversations');
      setConversations([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // ============================================================================
  // CREATE CONVERSATION
  // ============================================================================

  const createConversation = useCallback(async (data: CreateConversationDto) => {
    // Always create locally first (works with or without backend)
    const newConv: ConversationDto = {
      id: `conv-${Date.now()}`,
      title: data.title || 'New Conversation',
      workspaceId: data.workspaceId || 'default',
      lastMessageAt: new Date().toISOString(),
      messageCount: 0,
      isPinned: false,
      isArchived: false,
      modelId: data.modelId || null,
      tags: data.tags || [],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    setConversations(prev => [newConv, ...prev]);

    // If not offline, also try to sync with backend (but don't fail if it errors)
    if (!AppConfig.isOfflineMode()) {
      try {
        const backendConv = await conversationsService.createConversation(data);
        // Update with backend ID if successful
        setConversations(prev => prev.map(c =>
          c.id === newConv.id ? { ...c, id: backendConv.id } : c
        ));
        return backendConv;
      } catch (err) {
        console.warn('[useConversations] Failed to sync conversation to backend:', err);
        // Return local conversation - it still works
      }
    }

    return newConv;
  }, []);

  // ============================================================================
  // UPDATE CONVERSATION
  // ============================================================================

  const updateConversation = useCallback(async (
    conversationId: string,
    updates: Partial<ConversationDto>
  ) => {
    // Optimistic update
    setConversations(prev => prev.map(conv =>
      conv.id === conversationId ? { ...conv, ...updates } : conv
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await conversationsService.updateConversation(conversationId, updates);
    } catch (err) {
      console.error('[useConversations] Failed to update conversation:', err);
      loadConversations(workspaceId); // Revert
      throw err;
    }
  }, [workspaceId, loadConversations]);

  // ============================================================================
  // DELETE CONVERSATION
  // ============================================================================

  const deleteConversation = useCallback(async (conversationId: string) => {
    // Optimistic update
    setConversations(prev => prev.filter(conv => conv.id !== conversationId));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await conversationsService.deleteConversation(conversationId);
    } catch (err) {
      console.error('[useConversations] Failed to delete conversation:', err);
      await loadConversations(workspaceId); // Revert
      throw err;
    }
  }, [workspaceId, loadConversations]);

  // ============================================================================
  // ARCHIVE CONVERSATION
  // ============================================================================

  const archiveConversation = useCallback(async (conversationId: string) => {
    // Optimistic update - remove from list
    setConversations(prev => prev.filter(conv => conv.id !== conversationId));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await conversationsService.archiveConversation(conversationId);
    } catch (err) {
      console.error('[useConversations] Failed to archive conversation:', err);
      await loadConversations(workspaceId); // Revert
      throw err;
    }
  }, [workspaceId, loadConversations]);

  // ============================================================================
  // PIN/UNPIN CONVERSATION
  // ============================================================================

  const togglePin = useCallback(async (conversationId: string) => {
    const conv = conversations.find(c => c.id === conversationId);
    if (!conv) return;

    // Optimistic update
    setConversations(prev => prev.map(c =>
      c.id === conversationId ? { ...c, isPinned: !c.isPinned } : c
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      if (conv.isPinned) {
        await conversationsService.unpinConversation(conversationId);
      } else {
        await conversationsService.pinConversation(conversationId);
      }
    } catch (err) {
      console.error('[useConversations] Failed to toggle pin:', err);
      await loadConversations(workspaceId); // Revert
      throw err;
    }
  }, [conversations, workspaceId, loadConversations]);

  // ============================================================================
  // SEARCH CONVERSATIONS
  // ============================================================================

  const searchConversations = useCallback(async (query: string) => {
    if (AppConfig.isOfflineMode()) {
      return conversations.filter(conv =>
        conv.title.toLowerCase().includes(query.toLowerCase()) ||
        conv.tags?.some(tag => tag.toLowerCase().includes(query.toLowerCase()))
      );
    }

    try {
      return await conversationsService.searchConversations(query);
    } catch (err) {
      console.error('[useConversations] Failed to search conversations:', err);
      throw err;
    }
  }, [conversations]);

  // ============================================================================
  // EXPORT CONVERSATION
  // ============================================================================

  const exportConversation = useCallback(async (
    conversationId: string,
    format: 'json' | 'markdown' | 'pdf' = 'json'
  ) => {
    if (AppConfig.isOfflineMode()) {
      throw new Error('Cannot export in offline mode');
    }

    try {
      const blob = await conversationsService.exportConversation(conversationId, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `conversation-${conversationId}.${format}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      console.error('[useConversations] Failed to export conversation:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // INITIALIZATION & CACHE SYNC
  // ============================================================================

  useEffect(() => {
    loadConversations(workspaceId);
  }, [workspaceId]); // Only reload when workspaceId changes

  // Sync module-level cache whenever conversations change
  useEffect(() => {
    globalConversationsCache = conversations;
  }, [conversations]);

  // ============================================================================
  // LOAD MORE (PAGINATION)
  // ============================================================================

  const loadMore = useCallback(async () => {
    if (!hasMore || loading) return;

    console.log('[useConversations] Loading more conversations, page:', page + 1);

    if (AppConfig.isOfflineMode()) {
      // In offline mode, all conversations are already loaded
      setHasMore(false);
      return;
    }

    try {
      setLoading(true);
      const data = await conversationsService.getConversations(workspaceId, {
        page: page + 1,
        pageSize,
      });

      setConversations(prev => [...prev, ...data]);
      setPage(prev => prev + 1);
      setHasMore(data.length >= pageSize);
    } catch (err) {
      console.error('[useConversations] Failed to load more conversations:', err);
      setError('Failed to load more conversations');
    } finally {
      setLoading(false);
    }
  }, [hasMore, loading, page, pageSize, workspaceId]);

  return {
    conversations,
    loading,
    error,
    hasMore,

    // Actions
    refresh: () => loadConversations(workspaceId),
    loadMore,
    createConversation,
    updateConversation,
    deleteConversation,
    archiveConversation,
    togglePin,
    searchConversations,
    exportConversation,
  };
}