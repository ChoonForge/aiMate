import apiClient from './client';
import { Workspace, Conversation, Message } from './types';

export const workspacesApi = {
  /**
   * Get all workspaces for a user
   */
  async getWorkspaces(userId: string): Promise<Workspace[]> {
    return apiClient.get<Workspace[]>(`/workspaces?userId=${userId}`);
  },

  /**
   * Get a single workspace by ID
   */
  async getWorkspace(workspaceId: string): Promise<Workspace> {
    return apiClient.get<Workspace>(`/workspaces/${workspaceId}`);
  },

  /**
   * Create a new workspace
   */
  async createWorkspace(data: Partial<Workspace>): Promise<Workspace> {
    return apiClient.post<Workspace>('/workspaces', data);
  },

  /**
   * Update an existing workspace
   */
  async updateWorkspace(
    workspaceId: string,
    data: Partial<Workspace>
  ): Promise<Workspace> {
    return apiClient.put<Workspace>(`/workspaces/${workspaceId}`, data);
  },

  /**
   * Delete a workspace
   */
  async deleteWorkspace(workspaceId: string): Promise<void> {
    return apiClient.delete(`/workspaces/${workspaceId}`);
  },

  /**
   * Get all conversations in a workspace
   */
  async getConversations(workspaceId: string): Promise<Conversation[]> {
    return apiClient.get<Conversation[]>(`/workspaces/${workspaceId}/conversations`);
  },

  /**
   * Create a new conversation in a workspace
   */
  async createConversation(
    workspaceId: string,
    data: { title: string; model?: string }
  ): Promise<Conversation> {
    return apiClient.post<Conversation>(
      `/workspaces/${workspaceId}/conversations`,
      data
    );
  },

  /**
   * Update a conversation
   */
  async updateConversation(
    conversationId: string,
    data: Partial<Conversation>
  ): Promise<Conversation> {
    return apiClient.put<Conversation>(`/conversations/${conversationId}`, data);
  },

  /**
   * Delete a conversation
   */
  async deleteConversation(conversationId: string): Promise<void> {
    return apiClient.delete(`/conversations/${conversationId}`);
  },

  /**
   * Archive a conversation
   */
  async archiveConversation(conversationId: string): Promise<Conversation> {
    return apiClient.patch<Conversation>(`/conversations/${conversationId}/archive`, {});
  },

  /**
   * Get all messages in a conversation
   */
  async getMessages(conversationId: string): Promise<Message[]> {
    return apiClient.get<Message[]>(`/conversations/${conversationId}/messages`);
  },

  /**
   * Send a message in a conversation (non-streaming)
   */
  async sendMessage(
    conversationId: string,
    content: string,
    attachments?: string[]
  ): Promise<Message> {
    return apiClient.post<Message>(`/conversations/${conversationId}/messages`, {
      content,
      attachments,
    });
  },
};
