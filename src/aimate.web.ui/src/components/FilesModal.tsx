import { useState, useEffect, useRef, useMemo } from "react";
import { Upload, Trash2, FileText, Download, Eye, Loader2, RefreshCw } from "lucide-react";
import { Button } from "./ui/button";
import { ScrollArea } from "./ui/scroll-area";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "./ui/alert-dialog";
import { toast } from "sonner";
import { useDebug } from "./DebugContext";
import { useAppData } from "../context/AppDataContext";
import { BaseModal } from "./BaseModal";

interface FileItem {
  id: string;
  name: string;
  size: string;
  type: string;
  uploadedAt: string;
}

interface FilesModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FilesModal({ open, onOpenChange }: FilesModalProps) {
  const { addLog } = useDebug();
  const { files: filesHook, workspaces } = useAppData();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [deleteFileId, setDeleteFileId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Map hook files to component format
  const files: FileItem[] = useMemo(() => {
    return filesHook.files.map((file) => ({
      id: file.id,
      name: file.fileName || file.name || 'Unknown',
      size: formatFileSize(file.fileSize),
      type: getFileTypeLabel(file.fileType),
      uploadedAt: formatDate(file.uploadedAt),
    }));
  }, [filesHook.files]);

  // Load files when modal opens
  useEffect(() => {
    if (open) {
      filesHook.refresh();
      addLog({
        category: 'files:modal',
        action: 'Loaded Files',
        api: '/api/v1/files',
        payload: { count: filesHook.files.length },
        type: 'success'
      });
    }
  }, [open]);

  const handleUploadFile = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = event.target.files;
    if (!selectedFiles || selectedFiles.length === 0) return;

    const file = selectedFiles[0];

    // Validate file
    const validation = filesHook.validateFile(file);
    if (!validation.valid) {
      toast.error(validation.error || 'Invalid file');
      return;
    }

    try {
      await filesHook.uploadFile(file, {
        workspaceId: workspaces.currentWorkspace?.id,
      });

      toast.success(`${file.name} uploaded successfully`);
      addLog({
        category: 'files:modal',
        action: 'Uploaded File',
        api: '/api/v1/files/upload',
        payload: { fileName: file.name, size: file.size },
        type: 'success'
      });

      // Refresh file list
      filesHook.refresh();
    } catch (err) {
      toast.error(`Failed to upload ${file.name}`);
      addLog({
        category: 'files:modal',
        action: 'Upload Failed',
        api: '/api/v1/files/upload',
        payload: { fileName: file.name, error: err },
        type: 'error'
      });
    }

    // Reset file input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDeleteFile = async () => {
    if (!deleteFileId) return;

    setDeleting(true);
    try {
      await filesHook.deleteFile(deleteFileId);
      toast.success("File deleted successfully");
      addLog({
        category: 'files:modal',
        action: 'Deleted File',
        api: '/api/v1/files',
        payload: { fileId: deleteFileId },
        type: 'success'
      });
    } catch (err) {
      toast.error("Failed to delete file");
      addLog({
        category: 'files:modal',
        action: 'Delete Failed',
        api: '/api/v1/files',
        payload: { fileId: deleteFileId, error: err },
        type: 'error'
      });
    } finally {
      setDeleteFileId(null);
      setDeleting(false);
    }
  };

  const handleDownloadFile = async (fileId: string, fileName: string) => {
    try {
      toast.info(`Downloading ${fileName}...`);
      await filesHook.downloadFile(fileId, workspaces.currentWorkspace?.id, fileName);
      toast.success(`Downloaded ${fileName}`);
    } catch (err) {
      toast.error(`Failed to download ${fileName}`);
    }
  };

  const handleViewFile = async (fileId: string, fileName: string) => {
    try {
      const url = await filesHook.getFileUrl(fileId, workspaces.currentWorkspace?.id);
      window.open(url, '_blank');
    } catch (err) {
      toast.error(`Failed to open ${fileName}`);
    }
  };

  const handleRefresh = () => {
    filesHook.refresh();
    toast.success("Refreshing files...");
  };

  const getFileIcon = (type: string) => {
    return <FileText className="h-5 w-5 text-blue-500" />;
  };

  return (
    <>
      <BaseModal
        open={open}
        onOpenChange={onOpenChange}
        title="Files"
        description="Manage your uploaded files"
        icon={FileText}
        size="lg"
        showSave={false}
        showDelete={false}
      >
        {/* Upload Button in Content */}
        <div className="flex justify-end gap-2 mb-4">
          <Button
            variant="outline"
            size="sm"
            onClick={handleRefresh}
            disabled={filesHook.loading}
          >
            <RefreshCw className={`h-4 w-4 ${filesHook.loading ? 'animate-spin' : ''}`} />
          </Button>
          <Button
            onClick={() => fileInputRef.current?.click()}
            size="sm"
            className="gap-2"
            disabled={filesHook.uploading}
          >
            {filesHook.uploading ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Upload className="h-4 w-4" />
            )}
            {filesHook.uploading ? `Uploading (${filesHook.uploadProgress}%)` : 'Upload File'}
          </Button>
        </div>

        {/* Files List */}
        <div className="space-y-3">
          {filesHook.loading ? (
            <div className="flex flex-col items-center justify-center py-12 gap-3 text-gray-500 dark:text-gray-400">
              <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
              <p>Loading files...</p>
            </div>
          ) : files.length === 0 ? (
            <div className="text-center py-8 text-gray-500 dark:text-gray-400">
              No files yet. Upload your first file!
            </div>
          ) : (
            files.map((file) => (
              <div
                key={file.id}
                className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
              >
                <div className="flex items-start justify-between">
                  <div className="flex gap-3 flex-1">
                    {getFileIcon(file.type)}
                    <div>
                      <h4 className="font-medium mb-1">{file.name}</h4>
                      <div className="flex gap-2 text-sm text-gray-600 dark:text-gray-400">
                        <span>{file.type}</span>
                        <span>•</span>
                        <span>{file.size}</span>
                        <span>•</span>
                        <span>{file.uploadedAt}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleViewFile(file.id, file.name)}
                      title="View"
                    >
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleDownloadFile(file.id, file.name)}
                      title="Download"
                    >
                      <Download className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-red-600 hover:text-red-700 dark:text-red-400"
                      onClick={() => setDeleteFileId(file.id)}
                      title="Delete"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </BaseModal>

      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        className="hidden"
        onChange={handleUploadFile}
      />

      <AlertDialog open={!!deleteFileId} onOpenChange={(open) => !open && setDeleteFileId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete File</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this file? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteFile}
              disabled={deleting}
              className="bg-red-600 hover:bg-red-700"
            >
              {deleting ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Deleting...
                </>
              ) : (
                "Delete"
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}

// Helper functions
function formatFileSize(bytes: number): string {
  if (!bytes || bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}

function formatDate(dateString: string): string {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-NZ', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  } catch {
    return dateString || 'Unknown';
  }
}

function getFileTypeLabel(fileType: string): string {
  if (!fileType) return 'File';
  if (fileType.includes('pdf')) return 'PDF';
  if (fileType.includes('image')) return 'Image';
  if (fileType.includes('video')) return 'Video';
  if (fileType.includes('audio')) return 'Audio';
  if (fileType.includes('word') || fileType.includes('document')) return 'Document';
  if (fileType.includes('excel') || fileType.includes('spreadsheet')) return 'Spreadsheet';
  if (fileType.includes('text')) return 'Text';
  return 'File';
}
