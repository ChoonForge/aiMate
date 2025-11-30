/**
 * Files Hook
 *
 * Manages file uploads, attachments, and media
 */

import { useState, useCallback, useEffect } from 'react';
import { filesService, FileDto } from '../api/services';
import { AppConfig } from '../utils/config';

export function useFiles(workspaceId?: string) {
  const [files, setFiles] = useState<FileDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);

  // ============================================================================
  // LOAD FILES
  // ============================================================================

  const loadFiles = useCallback(async (wsId?: string) => {
    const targetWorkspaceId = wsId || workspaceId || 'default';

    if (AppConfig.isOfflineMode()) {
      // Mock files in offline mode
      const mockFiles: FileDto[] = [
        {
          id: 'file-1',
          name: 'requirements.pdf',
          fileName: 'requirements.pdf',
          fileType: 'application/pdf',
          fileSize: 2457600,
          url: 'https://mock-files.aimate.nz/file-1',
          uploadedAt: new Date(Date.now() - 86400000 * 2).toISOString(),
          uploadedBy: 'user-1',
          workspaceId: targetWorkspaceId,
        },
        {
          id: 'file-2',
          name: 'design-mockups.fig',
          fileName: 'design-mockups.fig',
          fileType: 'application/figma',
          fileSize: 5242880,
          url: 'https://mock-files.aimate.nz/file-2',
          uploadedAt: new Date(Date.now() - 86400000 * 3).toISOString(),
          uploadedBy: 'user-1',
          workspaceId: targetWorkspaceId,
        },
        {
          id: 'file-3',
          name: 'data-analysis.xlsx',
          fileName: 'data-analysis.xlsx',
          fileType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          fileSize: 1153433,
          url: 'https://mock-files.aimate.nz/file-3',
          uploadedAt: new Date(Date.now() - 86400000).toISOString(),
          uploadedBy: 'user-1',
          workspaceId: targetWorkspaceId,
        },
      ];
      setFiles(mockFiles);
      setLoading(false);
      return mockFiles;
    }

    try {
      setLoading(true);
      const data = await filesService.getFiles(targetWorkspaceId);
      setFiles(data);
      setError(null);
      return data;
    } catch (err) {
      console.error('[useFiles] Failed to load files:', err);
      setError('Failed to load files');
      return [];
    } finally {
      setLoading(false);
    }
  }, [workspaceId]);

  // Load files on mount and when workspaceId changes
  useEffect(() => {
    if (workspaceId) {
      loadFiles(workspaceId);
    }
  }, [workspaceId, loadFiles]);

  // ============================================================================
  // UPLOAD FILE
  // ============================================================================

  const uploadFile = useCallback(async (
    file: File,
    options?: {
      conversationId?: string;
      workspaceId?: string;
      onProgress?: (progress: number) => void;
    }
  ) => {
    if (AppConfig.isOfflineMode()) {
      // Simulate upload in offline mode
      setUploading(true);
      setUploadProgress(0);

      const mockFile: FileDto = {
        id: `file-${Date.now()}`,
        name: file.name,
        fileName: file.name,
        fileType: file.type,
        fileSize: file.size,
        url: URL.createObjectURL(file),
        uploadedAt: new Date().toISOString(),
        uploadedBy: 'user',
        workspaceId: options?.workspaceId || 'default',
      };

      // Simulate progress
      for (let i = 0; i <= 100; i += 10) {
        await new Promise(resolve => setTimeout(resolve, 100));
        setUploadProgress(i);
        options?.onProgress?.(i);
      }

      setUploading(false);
      setUploadProgress(0);
      return mockFile;
    }

    try {
      setUploading(true);
      setUploadProgress(0);
      setError(null);

      const uploadedFile = await filesService.uploadFile(
        options?.workspaceId || 'default',
        file,
        undefined,
        (progress) => {
          setUploadProgress(progress);
          options?.onProgress?.(progress);
        }
      );

      return uploadedFile;
    } catch (err) {
      console.error('[useFiles] Failed to upload file:', err);
      setError('Failed to upload file');
      throw err;
    } finally {
      setUploading(false);
      setUploadProgress(0);
    }
  }, []);

  // ============================================================================
  // UPLOAD MULTIPLE FILES
  // ============================================================================

  const uploadFiles = useCallback(async (
    files: File[],
    options?: {
      conversationId?: string;
      workspaceId?: string;
      onProgress?: (fileIndex: number, progress: number) => void;
    }
  ) => {
    const uploadedFiles: FileDto[] = [];

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      try {
        const uploaded = await uploadFile(file, {
          ...options,
          onProgress: (progress) => options?.onProgress?.(i, progress),
        });
        uploadedFiles.push(uploaded);
      } catch (err) {
        console.error(`[useFiles] Failed to upload file ${file.name}:`, err);
        // Continue with other files
      }
    }

    return uploadedFiles;
  }, [uploadFile]);

  // ============================================================================
  // DELETE FILE
  // ============================================================================

  const deleteFile = useCallback(async (fileId: string, wsId?: string) => {
    const targetWorkspaceId = wsId || workspaceId || 'default';

    // Optimistic update
    setFiles(prev => prev.filter(f => f.id !== fileId));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await filesService.deleteFile(targetWorkspaceId, fileId);
    } catch (err) {
      console.error('[useFiles] Failed to delete file:', err);
      // Revert on failure
      await loadFiles(targetWorkspaceId);
      throw err;
    }
  }, [workspaceId, loadFiles]);

  // ============================================================================
  // GET FILE URL
  // ============================================================================

  const getFileUrl = useCallback(async (fileId: string, workspaceId: string = 'default') => {
    if (AppConfig.isOfflineMode()) {
      return `https://mock-file-url.aimate.nz/${fileId}`;
    }

    try {
      return await filesService.getFileUrl(workspaceId, fileId);
    } catch (err) {
      console.error('[useFiles] Failed to get file URL:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // DOWNLOAD FILE
  // ============================================================================

  const downloadFile = useCallback(async (fileId: string, workspaceId: string = 'default', filename?: string) => {
    if (AppConfig.isOfflineMode()) {
      throw new Error('Cannot download files in offline mode');
    }

    try {
      const blob = await filesService.downloadFile(workspaceId, fileId);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename || `file-${fileId}`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      console.error('[useFiles] Failed to download file:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // UPLOAD IMAGE FROM URL
  // ============================================================================

  const uploadImageFromUrl = useCallback(async (
    url: string,
    options?: {
      conversationId?: string;
      workspaceId?: string;
    }
  ) => {
    if (AppConfig.isOfflineMode()) {
      const mockFile: FileDto = {
        id: `file-${Date.now()}`,
        fileName: url.split('/').pop() || 'image.jpg',
        fileType: 'image/jpeg',
        fileSize: 0,
        url: url,
        uploadedAt: new Date().toISOString(),
        uploadedBy: 'user',
        workspaceId: options?.workspaceId || 'default',
      };
      return mockFile;
    }

    try {
      return await filesService.uploadImageFromUrl(url, options);
    } catch (err) {
      console.error('[useFiles] Failed to upload image from URL:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // VALIDATE FILE
  // ============================================================================

  const validateFile = useCallback((
    file: File,
    options?: {
      maxSize?: number; // in bytes
      allowedTypes?: string[];
    }
  ): { valid: boolean; error?: string } => {
    const maxSize = options?.maxSize || 10 * 1024 * 1024; // 10MB default
    const allowedTypes = options?.allowedTypes || [
      'image/jpeg',
      'image/png',
      'image/gif',
      'image/webp',
      'application/pdf',
      'text/plain',
      'text/markdown',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    ];

    if (file.size > maxSize) {
      return {
        valid: false,
        error: `File size exceeds ${(maxSize / 1024 / 1024).toFixed(0)}MB limit`,
      };
    }

    if (!allowedTypes.includes(file.type)) {
      return {
        valid: false,
        error: `File type ${file.type} is not allowed`,
      };
    }

    return { valid: true };
  }, []);

  return {
    // State
    files,
    loading,
    uploading,
    uploadProgress,
    error,

    // Actions
    refresh: () => loadFiles(workspaceId),
    loadFiles,
    uploadFile,
    uploadFiles,
    deleteFile,
    getFileUrl,
    downloadFile,
    uploadImageFromUrl,
    validateFile,
  };
}
