/**
 * Chat Hook
 *
 * Manages chat messages, streaming, and real-time communication.
 * In offline mode with an active LM server connection, calls the server directly.
 */

import { useState, useCallback, useRef, useMemo, useEffect } from 'react';
import { messagesService, MessageDto, SendMessageDto, knowledgeService } from '../api/services';
import { AppConfig } from '../utils/config';
import { useAdminSettings } from '../context/AdminSettingsContext';
import { toast } from 'sonner';

// Retry configuration for LM server connections
const RETRY_CONFIG = {
  maxRetries: 3,
  baseDelayMs: 1000,
  maxDelayMs: 10000,
};

// Helper to wait with exponential backoff
const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

// Calculate delay with exponential backoff and jitter
const getRetryDelay = (attempt: number): number => {
  const exponentialDelay = RETRY_CONFIG.baseDelayMs * Math.pow(2, attempt);
  const jitter = Math.random() * 500;
  return Math.min(exponentialDelay + jitter, RETRY_CONFIG.maxDelayMs);
};

// Check if error is retryable (network errors, 5xx, but not auth errors)
const isRetryableError = (error: unknown): boolean => {
  if (error instanceof TypeError) return true; // Network error
  if (error instanceof Error) {
    const message = error.message.toLowerCase();
    if (message.includes('network') || message.includes('fetch')) return true;
    if (message.includes('5') && message.includes('http')) return true; // 5xx errors
    if (message.includes('timeout')) return true;
  }
  return false;
};

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
      knowledgeIds?: string[];
      memoryContext?: string;
    }
  ) => {
    const isOffline = AppConfig.isOfflineMode();
    const activeConnection = getActiveLmConnection();

    console.log('[useChat] sendMessage called:', {
      content: content.substring(0, 50) + '...',
      options,
      isOfflineMode: isOffline,
      activeConnection: activeConnection ? {
        name: activeConnection.name,
        url: activeConnection.url,
        enabled: activeConnection.enabled,
      } : 'NONE',
    });

    // PRIORITY 1: If we have an active LM server connection, use it directly (regardless of offline mode)
    if (activeConnection?.url) {
      console.log('[useChat] Using LM server connection (connection-first mode)');

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
        model: options?.model || activeConnection.name || 'LM Server',
      };

      const chatUrl = activeConnection.url.replace(/\/$/, '') + '/chat/completions';
      console.log('[useChat] CALLING LM SERVER:', {
        connectionName: activeConnection.name,
        baseUrl: activeConnection.url,
        chatUrl: chatUrl,
        model: options?.model || 'default',
      });

      try {
        const headers: Record<string, string> = {
          'Content-Type': 'application/json',
        };
        if (activeConnection.apiKey) {
          headers['Authorization'] = `Bearer ${activeConnection.apiKey}`;
        }

        // Fetch document chunks if knowledge IDs are provided
        let knowledgeContext = '';
        if (options?.knowledgeIds && options.knowledgeIds.length > 0) {
          console.log('[useChat] Fetching knowledge chunks for:', options.knowledgeIds);
          try {
            const chunkPromises = options.knowledgeIds.map(id =>
              knowledgeService.getDocumentChunks(id)
            );
            const allChunks = await Promise.all(chunkPromises);

            // Flatten and format chunks
            const chunks = allChunks.flat();
            if (chunks.length > 0) {
              knowledgeContext = '\n\n--- ATTACHED KNOWLEDGE CONTEXT ---\n' +
                chunks.map((chunk, i) =>
                  `[Document Chunk ${i + 1}]:\n${chunk.content}`
                ).join('\n\n') +
                '\n--- END KNOWLEDGE CONTEXT ---\n\n';
              console.log('[useChat] Injecting', chunks.length, 'knowledge chunks');
            }
          } catch (err) {
            console.error('[useChat] Failed to fetch knowledge chunks:', err);
            // Continue without knowledge context on error
          }
        }

        // Build messages array for the API (prepend system prompt + knowledge context + memory context if provided)
        const systemContent = [
          options?.systemPrompt || '',
          options?.memoryContext || '',
          knowledgeContext,
        ].filter(Boolean).join('\n');

        const chatMessages = [
          ...(systemContent ? [{ role: 'system' as const, content: systemContent }] : []),
          ...messages.map(m => ({ role: m.role, content: m.content })),
          { role: 'user' as const, content }
        ];

        // Retry logic with exponential backoff
        let response: Response | null = null;
        let lastError: Error | null = null;

        for (let attempt = 0; attempt <= RETRY_CONFIG.maxRetries; attempt++) {
          try {
            if (attempt > 0) {
              const delay = getRetryDelay(attempt - 1);
              console.log(`[useChat] Retry attempt ${attempt}/${RETRY_CONFIG.maxRetries} after ${delay}ms`);
              toast.info(`Reconnecting to LM server... (attempt ${attempt}/${RETRY_CONFIG.maxRetries})`);
              await wait(delay);
            }

            response = await fetch(chatUrl, {
              method: 'POST',
              headers,
              body: JSON.stringify({
                model: options?.model || 'default',
                messages: chatMessages,
                stream: true,
              }),
              signal: AbortSignal.timeout(30000), // 30 second timeout
            });

            if (!response.ok) {
              const error = new Error(`HTTP ${response.status}: ${response.statusText}`);
              // Don't retry auth errors
              if (response.status === 401 || response.status === 403) {
                throw error;
              }
              lastError = error;
              if (attempt < RETRY_CONFIG.maxRetries && isRetryableError(error)) {
                continue;
              }
              throw error;
            }

            // Success - break out of retry loop
            if (attempt > 0) {
              toast.success('Reconnected to LM server');
            }
            break;
          } catch (err) {
            lastError = err instanceof Error ? err : new Error(String(err));
            if (attempt < RETRY_CONFIG.maxRetries && isRetryableError(err)) {
              continue;
            }
            throw lastError;
          }
        }

        if (!response) {
          throw lastError || new Error('Failed to connect after retries');
        }

        // Handle streaming response with mid-stream error recovery
        const reader = response.body?.getReader();
        const decoder = new TextDecoder();
        let fullContent = '';
        let messageAdded = false;
        let streamInterrupted = false;

        if (reader) {
          try {
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
          } catch (streamErr) {
            // Mid-stream error - connection dropped
            console.error('[useChat] Stream interrupted:', streamErr);
            streamInterrupted = true;

            if (fullContent.length > 0) {
              // We got partial content - append interruption notice
              const partialContent = fullContent + '\n\n⚠️ *[Response interrupted - connection lost]*';
              setMessages(prev => prev.map(msg =>
                msg.id === assistantMsg.id
                  ? { ...msg, content: partialContent }
                  : msg
              ));
              toast.warning('Connection lost during response', {
                description: 'Partial response saved. Use Continue to resume.',
              });
            } else {
              // No content received
              toast.error('Connection lost', {
                description: 'Please try again.',
              });
              throw streamErr;
            }
          }
        }

        if (!streamInterrupted) {
          console.log('[useChat] LM server streaming complete');
        }
        return { ...assistantMsg, content: fullContent };
      } catch (err) {
        console.error('[useChat] LM server call failed:', err);

        // Map errors to user-friendly messages
        let userMessage = 'Unable to connect to the AI server.';
        let toastMessage = 'Connection failed';
        const selectedModel = options?.model || 'default';

        if (err instanceof Error) {
          const msg = err.message.toLowerCase();
          if (msg.includes('401') || msg.includes('403')) {
            userMessage = 'Authentication failed. Please check your API key in Admin → Connections.';
            toastMessage = 'Authentication failed';
          } else if (msg.includes('404')) {
            // Could be model not found or endpoint not found
            userMessage = `The model "${selectedModel}" may not exist on this server, or the endpoint was not found.`;
            toastMessage = 'Model or endpoint not found';

            // Try to fetch available models to help the user
            try {
              const modelsUrl = activeConnection.url.replace(/\/$/, '') + '/models';
              const modelsRes = await fetch(modelsUrl, {
                headers: activeConnection.apiKey ? { 'Authorization': `Bearer ${activeConnection.apiKey}` } : {},
                signal: AbortSignal.timeout(5000),
              });
              if (modelsRes.ok) {
                const modelsData = await modelsRes.json();
                const modelIds = modelsData.data?.map((m: { id: string }) => m.id) || [];
                if (modelIds.length > 0) {
                  userMessage = `Model "${selectedModel}" not found. Available models: ${modelIds.slice(0, 5).join(', ')}${modelIds.length > 5 ? ` (+${modelIds.length - 5} more)` : ''}`;
                }
              }
            } catch {
              // Ignore - just use the original error message
            }
          } else if (msg.includes('timeout')) {
            userMessage = 'The request timed out. The server may be busy or unreachable.';
            toastMessage = 'Request timed out';
          } else if (msg.includes('network') || msg.includes('fetch') || msg.includes('failed')) {
            userMessage = 'Network error. Please check your internet connection and that the LM server is running.';
            toastMessage = 'Network error';
          } else if (msg.includes('5')) {
            userMessage = 'The AI server encountered an error. Please try again or check server logs.';
            toastMessage = 'Server error';
          }
        }

        toast.error(toastMessage, {
          description: err instanceof Error ? err.message : undefined,
        });

        setMessages(prev => [...prev, { ...assistantMsg, content: `⚠️ ${userMessage}` }]);
        setError(userMessage);
        return { ...assistantMsg, content: userMessage };
      } finally {
        setStreaming(false);
      }
    }

    // PRIORITY 2: No active LM connection - check offline mode
    if (isOffline) {
      console.log('[useChat] No active LM connection, using mock responses (offline mode)');

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
        model: options?.model || 'Mock',
      };

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
  }, [messages, getActiveLmConnection]);

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

  // ============================================================================
  // CONTINUE MESSAGE
  // ============================================================================

  const continueMessage = useCallback(async (options?: { model?: string; systemPrompt?: string }) => {
    const activeConnection = adminSettings.settings.connections?.find(c => c.enabled && c.url);

    if (!activeConnection?.url) {
      toast.error('No active LM connection');
      return;
    }

    // Get the last assistant message to continue from
    const lastAssistantMsg = [...messages].reverse().find(m => m.role === 'assistant');
    if (!lastAssistantMsg) {
      toast.error('No message to continue');
      return;
    }

    setStreaming(true);

    try {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };
      if (activeConnection.apiKey) {
        headers['Authorization'] = `Bearer ${activeConnection.apiKey}`;
      }

      // Build messages with a continuation prompt
      const chatMessages = [
        ...(options?.systemPrompt ? [{ role: 'system' as const, content: options.systemPrompt }] : []),
        ...messages.map(m => ({ role: m.role, content: m.content })),
        { role: 'user' as const, content: 'Please continue from where you left off.' }
      ];

      const chatUrl = activeConnection.url.replace(/\/$/, '') + '/chat/completions';
      const response = await fetch(chatUrl, {
        method: 'POST',
        headers,
        body: JSON.stringify({
          model: options?.model || 'default',
          messages: chatMessages,
          stream: true,
        }),
        signal: AbortSignal.timeout(30000),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      // Handle streaming response - append to last assistant message
      const reader = response.body?.getReader();
      const decoder = new TextDecoder();
      let continuedContent = lastAssistantMsg.content;

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
                  continuedContent += delta;
                  setMessages(prev => prev.map(msg =>
                    msg.id === lastAssistantMsg.id
                      ? { ...msg, content: continuedContent }
                      : msg
                  ));
                }
              } catch {
                // Skip invalid JSON chunks
              }
            }
          }
        }
      }

        console.log('[useChat] Continue message complete');
      toast.success('Message continued');
    } catch (err) {
      console.error('[useChat] Continue failed:', err);
      toast.error('Failed to continue message');
    } finally {
      setStreaming(false);
    }
  }, [messages, adminSettings.settings.connections]);

  return useMemo(() => ({
    messages,
    streaming,
    loading,
    error,

    // Actions
    loadMessages,
    sendMessage,
    continueMessage,
    regenerateMessage,
    editMessage,
    deleteMessage,
    submitFeedback,
    cancelStreaming,
    clearMessages,
  }), [messages, streaming, loading, error, loadMessages, sendMessage, continueMessage, regenerateMessage, editMessage, deleteMessage, submitFeedback, cancelStreaming, clearMessages]);
}
