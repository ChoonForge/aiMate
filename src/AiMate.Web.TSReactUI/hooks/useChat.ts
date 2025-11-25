import { useState, useCallback, useRef } from 'react';
import { chatStreamClient } from '../api/chat';
import { ChatCompletionRequest, StreamChunk } from '../api/types';
import { toast } from 'sonner';

interface UseChatStreamOptions {
  onChunk?: (chunk: StreamChunk) => void;
  onComplete?: (fullContent: string) => void;
  onError?: (error: Error) => void;
}

/**
 * Hook for streaming chat completions
 */
export function useChatStream(options: UseChatStreamOptions = {}) {
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const [streamedContent, setStreamedContent] = useState('');
  const abortControllerRef = useRef<AbortController | null>(null);

  const { onChunk, onComplete, onError } = options;

  /**
   * Start streaming a chat completion
   */
  const streamMessage = useCallback(
    async (request: ChatCompletionRequest) => {
      setIsStreaming(true);
      setError(null);
      setStreamedContent('');

      // Create abort controller for cancellation
      abortControllerRef.current = new AbortController();

      let fullContent = '';

      try {
        for await (const chunk of chatStreamClient.streamCompletion(request)) {
          // Check if aborted
          if (abortControllerRef.current?.signal.aborted) {
            break;
          }

          // Extract content from chunk
          const content = chunk.choices[0]?.delta?.content || '';
          fullContent += content;
          setStreamedContent(fullContent);

          // Call onChunk callback
          if (onChunk) {
            onChunk(chunk);
          }

          // Check for finish reason
          if (chunk.choices[0]?.finishReason) {
            break;
          }
        }

        // Stream complete
        if (onComplete) {
          onComplete(fullContent);
        }
      } catch (err) {
        const error = err as Error;
        setError(error);
        console.error('Chat streaming error:', error);

        if (onError) {
          onError(error);
        } else {
          toast.error(`Chat error: ${error.message}`);
        }
      } finally {
        setIsStreaming(false);
        abortControllerRef.current = null;
      }

      return fullContent;
    },
    [onChunk, onComplete, onError]
  );

  /**
   * Cancel ongoing stream
   */
  const cancelStream = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      setIsStreaming(false);
      toast.info('Stream cancelled');
    }
  }, []);

  /**
   * Reset state
   */
  const reset = useCallback(() => {
    setStreamedContent('');
    setError(null);
    setIsStreaming(false);
  }, []);

  return {
    streamMessage,
    cancelStream,
    reset,
    isStreaming,
    error,
    streamedContent,
  };
}

/**
 * Hook for non-streaming chat completions
 */
export function useChat() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const sendMessage = useCallback(async (request: ChatCompletionRequest) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await chatStreamClient.createCompletion(request);
      return response;
    } catch (err) {
      const error = err as Error;
      setError(error);
      console.error('Chat error:', error);
      toast.error(`Chat error: ${error.message}`);
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, []);

  return {
    sendMessage,
    isLoading,
    error,
  };
}
