/**
 * Chat Hook
 *
 * Manages chat messages, streaming, and real-time communication.
 * In offline mode with an active LM server connection, calls the server directly.
 */

import { useState, useCallback, useRef, useMemo, useEffect } from 'react';
import { messagesService, MessageDto, SendMessageDto } from '../api/services';
import { AppConfig } from '../utils/config';
import { useAdminSettings } from '../context/AdminSettingsContext';

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: string;
  model?: string;
  tokenCount?: number;
  cost?: number;
  attachments?: Array<{ id: string; name: string; type: string; url: string }>;
  structuredContent?: any;
  feedback?: 'positive' | 'negative';
}

// Module-level cache to persist messages per conversation in offline mode
const conversationMessagesCache = new Map<string, ChatMessage[]>();

export function useChat(conversationId?: string) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [streaming, setStreaming] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);
  const adminSettings = useAdminSettings();

  // Get active LM server connection (for offline mode)
  const getActiveLmConnection = useCallback(() => {
    const connections = adminSettings.settings.connections;
    return connections.find(c => c.enabled && c.url);
  }, [adminSettings.settings.connections]);
  
  // Debug: Log streaming state changes
  useEffect(() => {
    console.log('[useChat] Streaming state changed to:', streaming);
  }, [streaming]);

  // Sync messages to cache whenever they change (in offline mode)
  useEffect(() => {
    if (AppConfig.isOfflineMode() && conversationId && messages.length > 0) {
      conversationMessagesCache.set(conversationId, messages);
    }
  }, [conversationId, messages]);

  // ============================================================================
  // LOAD MESSAGES
  // ============================================================================

  const loadMessages = useCallback(async (convId: string) => {
    console.log('[useChat] Loading messages for conversation:', convId);
    console.log('[useChat] Current streaming state before load:', streaming);
    
    // Reset streaming state when loading new conversation
    setStreaming(false);
    console.log('[useChat] Streaming state reset to false');
    setError(null);
    
    if (AppConfig.isOfflineMode()) {
      // Check cache first
      const cachedMessages = conversationMessagesCache.get(convId);
      if (cachedMessages) {
        console.log('[useChat] Loading messages from cache:', cachedMessages.length, 'messages');
        setMessages(cachedMessages);
        return;
      }
      
      // Use mock messages for the default conversation only
      if (convId === 'conv-1') {
        console.log('[useChat] Using mock messages for default conversation (offline mode)');
        const mockMessages: ChatMessage[] = [
          {
            id: '1',
            role: 'user',
            content: 'Hello! Can you help me understand how aiMate.nz works?',
            timestamp: new Date(Date.now() - 120000).toISOString(),
          },
          {
            id: '2',
            role: 'assistant',
            content: 'Kia ora! I\'d be happy to help you understand aiMate.nz. It\'s a sovereign AI platform designed specifically for New Zealanders, with a strong focus on safety and local resources.\n\nKey features include:\n- Crisis detection with verified NZ resources\n- Bring Your Own Key (BYOK) for privacy\n- Local data sovereignty\n- Multiple AI models to choose from\n\nWhat would you like to know more about?',
            timestamp: new Date(Date.now() - 115000).toISOString(),
            model: 'GPT-4 Turbo',
            tokenCount: 156,
            cost: 0.0045,
          },
        ];
        setMessages(mockMessages);
        conversationMessagesCache.set(convId, mockMessages);
      } else {
        // New conversation - start with empty messages
        console.log('[useChat] New conversation - empty messages');
        setMessages([]);
      }
      return;
    }

    try {
      setLoading(true);
      const data = await messagesService.getMessages(convId);
      setMessages(data.map(msg => ({
        id: msg.id,
        role: msg.role as 'user' | 'assistant' | 'system',
        content: msg.content,
        timestamp: msg.timestamp,
        model: msg.metadata?.model,
        tokenCount: msg.metadata?.tokenCount,
        cost: msg.metadata?.cost,
        attachments: msg.attachments,
        structuredContent: msg.structuredContent,
      })));
    } catch (err) {
      console.error('[useChat] Failed to load messages:', err);
      setError('Failed to load messages');
      setMessages([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // ============================================================================
  // SEND MESSAGE (Streaming)
  // ============================================================================

  const sendMessage = useCallback(async (
    content: string,
    options?: {
      conversationId?: string;
      workspaceId?: string;
      model?: string;
      attachments?: string[];
      systemPrompt?: string;
    }
  ) => {
    console.log('[useChat] sendMessage called:', { content, options });
    
    if (AppConfig.isOfflineMode()) {
      console.log('[useChat] Sending message in offline mode');

      // Check if we have an active LM server connection
      const activeConnection = getActiveLmConnection();

      const userMsg: ChatMessage = {
        id: `msg-${Date.now()}`,
        role: 'user',
        content,
        timestamp: new Date().toISOString(),
      };

      setMessages(prev => [...prev, userMsg]);
      setStreaming(true);

      const assistantMsg: ChatMessage = {
        id: `msg-${Date.now() + 1}`,
        role: 'assistant',
        content: '',
        timestamp: new Date().toISOString(),
        model: options?.model || 'LM Server',
      };

      // If we have an active LM server connection, use it!
      if (activeConnection?.url) {
        console.log('[useChat] Using LM server connection:', activeConnection.name, activeConnection.url);

        try {
          const chatUrl = activeConnection.url.replace(/\/$/, '') + '/chat/completions';
          const headers: Record<string, string> = {
            'Content-Type': 'application/json',
          };
          if (activeConnection.apiKey) {
            headers['Authorization'] = `Bearer ${activeConnection.apiKey}`;
          }

          // Build messages array for the API
          const chatMessages = [
            ...messages.map(m => ({ role: m.role, content: m.content })),
            { role: 'user' as const, content }
          ];

          const response = await fetch(chatUrl, {
            method: 'POST',
            headers,
            body: JSON.stringify({
              model: options?.model || 'default',
              messages: chatMessages,
              stream: true,
            }),
          });

          if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
          }

          // Handle streaming response
          const reader = response.body?.getReader();
          const decoder = new TextDecoder();
          let fullContent = '';
          let messageAdded = false;

          if (reader) {
            while (true) {
              const { done, value } = await reader.read();
              if (done) break;

              const chunk = decoder.decode(value, { stream: true });
              const lines = chunk.split('\n');

              for (const line of lines) {
                if (line.startsWith('data: ')) {
                  const data = line.slice(6);
                  if (data === '[DONE]') continue;

                  try {
                    const parsed = JSON.parse(data);
                    const delta = parsed.choices?.[0]?.delta?.content || '';
                    if (delta) {
                      fullContent += delta;

                      if (!messageAdded) {
                        setMessages(prev => [...prev, { ...assistantMsg, content: fullContent }]);
                        messageAdded = true;
                      } else {
                        setMessages(prev => prev.map(msg =>
                          msg.id === assistantMsg.id
                            ? { ...msg, content: fullContent }
                            : msg
                        ));
                      }
                    }
                  } catch {
                    // Skip invalid JSON chunks
                  }
                }
              }
            }
          }

          console.log('[useChat] LM server streaming complete');
          return { ...assistantMsg, content: fullContent };
        } catch (err) {
          console.error('[useChat] LM server call failed:', err);
          // Fall back to error message
          const errorMsg = `Failed to reach LM server: ${err instanceof Error ? err.message : 'Unknown error'}`;
          setMessages(prev => [...prev, { ...assistantMsg, content: errorMsg }]);
          return { ...assistantMsg, content: errorMsg };
        } finally {
          setStreaming(false);
        }
      }

      // No active connection - fall back to mock responses
      console.log('[useChat] No active LM connection, using mock responses');

      // Check for structured content commands
      let mockResponse = '';
      let structuredData = null;
      const contentLower = content.toLowerCase().trim();

      if (contentLower === 'gettable') {
        mockResponse = "Here's a table of New Zealand AI safety metrics:";
        structuredData = {
          type: 'table',
          data: {
            columns: ['Metric', 'Value', 'Status', 'Last Updated'],
            rows: [
              ['Crisis Detection Rate', '99.2%', 'Operational', '2024-11-28'],
              ['Response Time (avg)', '1.2s', 'Good', '2024-11-28'],
              ['NZ Resources Verified', '847', 'Active', '2024-11-27'],
              ['Safety Blocks Active', '23', 'Monitoring', '2024-11-28'],
              ['User Trust Score', '94.5%', 'Excellent', '2024-11-28'],
            ]
          }
        };
      } else if (contentLower === 'getlist') {
        mockResponse = "Here are the verified NZ crisis support services:";
        structuredData = {
          type: 'list',
          data: {
            items: [
              { id: '1', title: 'Lifeline Aotearoa', description: 'Free 24/7 counselling and support', meta: '0800 543 354' },
              { id: '2', title: 'Suicide Crisis Helpline', description: 'Immediate crisis support', meta: '0508 828 865' },
              { id: '3', title: '1737 - Need to talk?', description: 'Free call or text service', meta: 'Text or call 1737' },
              { id: '4', title: 'Youthline', description: 'Support for young people', meta: '0800 376 633' },
              { id: '5', title: 'Samaritans', description: 'Confidential support service', meta: '0800 726 666' },
              { id: '6', title: 'Depression Helpline', description: 'Support for depression', meta: '0800 111 757' },
            ]
          }
        };
      } else if (contentLower === 'getkv') {
        mockResponse = "Here are the current system configuration values:";
        structuredData = {
          type: 'key-value',
          data: {
            pairs: [
              { key: 'Platform Name', value: 'aiMate.nz' },
              { key: 'Environment', value: 'Development (Offline Mode)' },
              { key: 'Region', value: 'New Zealand (Sovereign)' },
              { key: 'Safety Level', value: 'Maximum' },
              { key: 'Crisis Detection', value: 'Enabled' },
              { key: 'Resource Verification', value: 'Active' },
              { key: 'API Version', value: 'v1.0.0' },
              { key: 'Last Deploy', value: '2024-11-28' },
            ]
          }
        };
      } else if (contentLower === 'getform') {
        mockResponse = "Here's an example feedback form structure:";
        structuredData = {
          type: 'form',
          data: {
            title: 'aiMate.nz Safety Feedback',
            description: 'Help us improve our safety infrastructure',
            fields: [
              { id: 'name', label: 'Your Name', type: 'text', required: false, placeholder: 'Optional' },
              { id: 'category', label: 'Feedback Category', type: 'select', required: true, options: ['Safety Feature', 'Crisis Detection', 'NZ Resources', 'General'] },
              { id: 'rating', label: 'Overall Experience', type: 'rating', required: true, max: 5 },
              { id: 'feedback', label: 'Your Feedback', type: 'textarea', required: true, placeholder: 'Tell us about your experience...' },
              { id: 'contact', label: 'Email (optional)', type: 'email', required: false, placeholder: 'your@email.com' },
            ],
            submitLabel: 'Submit Feedback'
          }
        };
      } else {
        mockResponse = `No LM server connection configured. Add a connection in Admin > Connections to enable AI chat.\n\nYou said: "${content}"`;
      }

      // Simulate streaming text
      try {
        for (let i = 0; i < mockResponse.length; i++) {
          await new Promise(resolve => setTimeout(resolve, 20));

          if (i === 0) {
            setMessages(prev => [...prev, { ...assistantMsg, content: mockResponse.substring(0, 1), structuredContent: structuredData }]);
          } else {
            setMessages(prev => prev.map(msg =>
              msg.id === assistantMsg.id
                ? { ...msg, content: mockResponse.substring(0, i + 1), structuredContent: structuredData }
                : msg
            ));
          }
        }

        return { ...assistantMsg, content: mockResponse, structuredContent: structuredData };
      } finally {
        setStreaming(false);
      }
    }

    try {
      setStreaming(true);
      setError(null);
      
      // Add user message immediately
      const userMsg: ChatMessage = {
        id: `temp-${Date.now()}`,
        role: 'user',
        content,
        timestamp: new Date().toISOString(),
      };
      setMessages(prev => [...prev, userMsg]);
      
      // Create assistant message placeholder (don't add to messages yet)
      const assistantId = `temp-${Date.now() + 1}`;
      const assistantMsg: ChatMessage = {
        id: assistantId,
        role: 'assistant',
        content: '',
        timestamp: new Date().toISOString(),
        model: options?.model,
      };
      
      // Track if we've added the message yet
      let messageAdded = false;
      
      // Create abort controller for cancellation
      abortControllerRef.current = new AbortController();
      
      // Send message with streaming
      const payload: SendMessageDto = {
        content,
        conversationId: options?.conversationId,
        workspaceId: options?.workspaceId,
        model: options?.model,
        attachmentIds: options?.attachments,
        systemPrompt: options?.systemPrompt,
        stream: true,
      };
      
      await messagesService.sendMessage(
        payload,
        (chunk) => {
          if (!messageAdded) {
            // Add message with first chunk
            setMessages(prev => [...prev, { ...assistantMsg, content: chunk }]);
            messageAdded = true;
          } else {
            // Update assistant message with streamed content
            setMessages(prev => prev.map(msg => 
              msg.id === assistantId 
                ? { ...msg, content: msg.content + chunk }
                : msg
            ));
          }
        },
        abortControllerRef.current.signal
      );
      
      setStreaming(false);
      return assistantMsg;
      
    } catch (err: any) {
      if (err.name === 'AbortError') {
        console.log('[useChat] Message sending cancelled');
      } else {
        console.error('[useChat] Failed to send message:', err);
        setError('Failed to send message');
      }
      setStreaming(false);
      throw err;
    }
  }, []);

  // ============================================================================
  // REGENERATE MESSAGE
  // ============================================================================

  const regenerateMessage = useCallback(async (messageId: string) => {
    if (AppConfig.isOfflineMode()) {
      const newMsg: ChatMessage = {
        id: `msg-${Date.now()}`,
        role: 'assistant',
        content: 'This is a regenerated response in offline mode.',
        timestamp: new Date().toISOString(),
      };
      setMessages(prev => [...prev, newMsg]);
      return newMsg;
    }

    try {
      setStreaming(true);
      const regenerated = await messagesService.regenerateMessage(messageId);
      
      const newMsg: ChatMessage = {
        id: regenerated.id,
        role: regenerated.role as 'assistant',
        content: regenerated.content,
        timestamp: regenerated.timestamp,
        model: regenerated.metadata?.model,
        tokenCount: regenerated.metadata?.tokenCount,
        cost: regenerated.metadata?.cost,
      };
      
      setMessages(prev => [...prev, newMsg]);
      return newMsg;
    } catch (err) {
      console.error('[useChat] Failed to regenerate message:', err);
      throw err;
    } finally {
      setStreaming(false);
    }
  }, []);

  // ============================================================================
  // EDIT MESSAGE
  // ============================================================================

  const editMessage = useCallback(async (messageId: string, newContent: string) => {
    // Optimistic update
    setMessages(prev => prev.map(msg => 
      msg.id === messageId ? { ...msg, content: newContent } : msg
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await messagesService.editMessage(messageId, { content: newContent });
    } catch (err) {
      console.error('[useChat] Failed to edit message:', err);
      // Revert on error would require storing original
      throw err;
    }
  }, []);

  // ============================================================================
  // DELETE MESSAGE
  // ============================================================================

  const deleteMessage = useCallback(async (messageId: string) => {
    // Optimistic update
    setMessages(prev => prev.filter(msg => msg.id !== messageId));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await messagesService.deleteMessage(messageId);
    } catch (err) {
      console.error('[useChat] Failed to delete message:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // FEEDBACK
  // ============================================================================

  const submitFeedback = useCallback(async (
    messageId: string, 
    rating: 'positive' | 'negative',
    comment?: string
  ) => {
    // Optimistic update
    setMessages(prev => prev.map(msg => 
      msg.id === messageId ? { ...msg, feedback: rating } : msg
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await messagesService.submitFeedback(messageId, rating, comment);
    } catch (err) {
      console.error('[useChat] Failed to submit feedback:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // CANCEL STREAMING
  // ============================================================================

  const cancelStreaming = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
      setStreaming(false);
    }
  }, []);

  // ============================================================================
  // CLEAR MESSAGES
  // ============================================================================

  const clearMessages = useCallback(() => {
    setMessages([]);
  }, []);

  return useMemo(() => ({
    messages,
    streaming,
    loading,
    error,
    
    // Actions
    loadMessages,
    sendMessage,
    regenerateMessage,
    editMessage,
    deleteMessage,
    submitFeedback,
    cancelStreaming,
    clearMessages,
  }), [messages, streaming, loading, error, loadMessages, sendMessage, regenerateMessage, editMessage, deleteMessage, submitFeedback, cancelStreaming, clearMessages]);
}
