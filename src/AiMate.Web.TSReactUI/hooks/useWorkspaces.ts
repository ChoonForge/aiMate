import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workspacesApi } from '../api/workspaces';
import { useAuth } from '../context/AuthContext';
import { Workspace, Conversation } from '../api/types';
import { toast } from 'sonner';

/**
 * Hook for managing workspaces
 */
export function useWorkspaces() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  // Get all workspaces for the current user
  const {
    data: workspaces,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['workspaces', user?.id],
    queryFn: () => workspacesApi.getWorkspaces(user!.id),
    enabled: !!user,
  });

  // Create workspace mutation
  const createWorkspace = useMutation({
    mutationFn: (data: Partial<Workspace>) => workspacesApi.createWorkspace(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      toast.success('Workspace created successfully');
    },
    onError: (error: Error) => {
      toast.error(`Failed to create workspace: ${error.message}`);
    },
  });

  // Update workspace mutation
  const updateWorkspace = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Workspace> }) =>
      workspacesApi.updateWorkspace(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      toast.success('Workspace updated');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update workspace: ${error.message}`);
    },
  });

  // Delete workspace mutation
  const deleteWorkspace = useMutation({
    mutationFn: (workspaceId: string) => workspacesApi.deleteWorkspace(workspaceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      toast.success('Workspace deleted');
    },
    onError: (error: Error) => {
      toast.error(`Failed to delete workspace: ${error.message}`);
    },
  });

  return {
    workspaces: workspaces || [],
    isLoading,
    error,
    createWorkspace: createWorkspace.mutate,
    updateWorkspace: updateWorkspace.mutate,
    deleteWorkspace: deleteWorkspace.mutate,
    isCreating: createWorkspace.isPending,
    isUpdating: updateWorkspace.isPending,
    isDeleting: deleteWorkspace.isPending,
  };
}

/**
 * Hook for managing conversations in a workspace
 */
export function useConversations(workspaceId: string | null) {
  const queryClient = useQueryClient();

  // Get all conversations in the workspace
  const {
    data: conversations,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['conversations', workspaceId],
    queryFn: () => workspacesApi.getConversations(workspaceId!),
    enabled: !!workspaceId,
  });

  // Create conversation mutation
  const createConversation = useMutation({
    mutationFn: (data: { title: string; model?: string }) =>
      workspacesApi.createConversation(workspaceId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', workspaceId] });
      toast.success('Conversation created');
    },
    onError: (error: Error) => {
      toast.error(`Failed to create conversation: ${error.message}`);
    },
  });

  // Update conversation mutation
  const updateConversation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Conversation> }) =>
      workspacesApi.updateConversation(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', workspaceId] });
      toast.success('Conversation updated');
    },
    onError: (error: Error) => {
      toast.error(`Failed to update conversation: ${error.message}`);
    },
  });

  // Delete conversation mutation
  const deleteConversation = useMutation({
    mutationFn: (conversationId: string) =>
      workspacesApi.deleteConversation(conversationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', workspaceId] });
      toast.success('Conversation deleted');
    },
    onError: (error: Error) => {
      toast.error(`Failed to delete conversation: ${error.message}`);
    },
  });

  // Archive conversation mutation
  const archiveConversation = useMutation({
    mutationFn: (conversationId: string) =>
      workspacesApi.archiveConversation(conversationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations', workspaceId] });
      toast.success('Conversation archived');
    },
    onError: (error: Error) => {
      toast.error(`Failed to archive conversation: ${error.message}`);
    },
  });

  return {
    conversations: conversations || [],
    isLoading,
    error,
    createConversation: createConversation.mutate,
    updateConversation: updateConversation.mutate,
    deleteConversation: deleteConversation.mutate,
    archiveConversation: archiveConversation.mutate,
    isCreating: createConversation.isPending,
    isUpdating: updateConversation.isPending,
    isDeleting: deleteConversation.isPending,
  };
}

/**
 * Hook for managing messages in a conversation
 */
export function useMessages(conversationId: string | null) {
  const queryClient = useQueryClient();

  // Get all messages in the conversation
  const {
    data: messages,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['messages', conversationId],
    queryFn: () => workspacesApi.getMessages(conversationId!),
    enabled: !!conversationId,
  });

  // Send message mutation (non-streaming)
  const sendMessage = useMutation({
    mutationFn: ({
      content,
      attachments,
    }: {
      content: string;
      attachments?: string[];
    }) => workspacesApi.sendMessage(conversationId!, content, attachments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages', conversationId] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
    onError: (error: Error) => {
      toast.error(`Failed to send message: ${error.message}`);
    },
  });

  return {
    messages: messages || [],
    isLoading,
    error,
    sendMessage: sendMessage.mutate,
    isSending: sendMessage.isPending,
  };
}
