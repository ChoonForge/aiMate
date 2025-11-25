import { ChatCompletionRequest, ChatCompletionResponse, StreamChunk } from './types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export class ChatStreamClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  /**
   * Get auth token from localStorage
   */
  private getToken(): string {
    return localStorage.getItem('auth_token') || '';
  }

  /**
   * Stream chat completion using Server-Sent Events
   * Returns an async generator that yields chunks as they arrive
   */
  async *streamCompletion(
    request: ChatCompletionRequest
  ): AsyncGenerator<StreamChunk, void, unknown> {
    const token = this.getToken();

    const response = await fetch(`${this.baseUrl}/api/v1/chat/completions/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ ...request, stream: true }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const reader = response.body?.getReader();
    if (!reader) {
      throw new Error('Response body is not readable');
    }

    const decoder = new TextDecoder();
    let buffer = '';

    try {
      while (true) {
        const { done, value } = await reader.read();

        if (done) break;

        // Decode the chunk and add to buffer
        buffer += decoder.decode(value, { stream: true });

        // Split by newlines to get individual SSE messages
        const lines = buffer.split('\n');

        // Keep the last incomplete line in the buffer
        buffer = lines.pop() || '';

        for (const line of lines) {
          // SSE format: "data: {json}"
          if (line.startsWith('data: ')) {
            const data = line.slice(6).trim();

            // Check for end of stream
            if (data === '[DONE]') {
              return;
            }

            try {
              const chunk = JSON.parse(data) as StreamChunk;
              yield chunk;
            } catch (error) {
              console.error('Failed to parse SSE chunk:', error, 'Data:', data);
            }
          }
        }
      }
    } finally {
      reader.releaseLock();
    }
  }

  /**
   * Create a non-streaming chat completion
   */
  async createCompletion(
    request: ChatCompletionRequest
  ): Promise<ChatCompletionResponse> {
    const token = this.getToken();

    const response = await fetch(`${this.baseUrl}/api/v1/chat/completions`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ ...request, stream: false }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    return response.json();
  }
}

export const chatStreamClient = new ChatStreamClient();
