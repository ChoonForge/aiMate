import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { File, Trash2, Download, Eye, Upload } from "lucide-react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "./ui/alert-dialog";
import { toast } from "sonner@2.0.3";
import { useDebug } from "./DebugContext";
import * as API from "../utils/api-stubs";
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
  const [files, setFiles] = useState<FileItem[]>([]);
  const [deleteFileId, setDeleteFileId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // Load files when modal opens
  useEffect(() => {
    if (open && files.length === 0) {
      loadFiles();
    }
  }, [open]);

  const loadFiles = async () => {
    setLoading(true);
    const response = await API.getFiles();
    if (response.success && response.data) {
      // Convert API format to component format
      const formattedFiles = response.data.map(file => ({
        id: file.id,
        name: file.title,
        size: file.size,
        type: file.type,
        uploadedAt: file.uploadDate
      }));
      setFiles(formattedFiles);
      addLog({
        action: 'Loaded Files',
        api: response.metadata?.endpoint || '/api/v1/GetFiles',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const handleUploadFile = () => {
    const input = document.createElement("input");
    input.type = "file";
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        const newFile: FileItem = {
          id: Date.now().toString(),
          name: file.name,
          size: `${(file.size / 1024 / 1024).toFixed(1)} MB`,
          type: file.type.split("/")[0] || "File",
          uploadedAt: new Date().toLocaleDateString(),
        };
        setFiles((prev) => [newFile, ...prev]);
        toast.success(`${file.name} uploaded successfully`);
      }
    };
    input.click();
  };

  const handleDeleteFile = () => {
    if (deleteFileId) {
      setFiles((prev) => prev.filter((file) => file.id !== deleteFileId));
      setDeleteFileId(null);
      toast.success("File deleted successfully");
    }
  };

  const handleDownloadFile = (fileName: string) => {
    toast.info(`Downloading ${fileName}...`);
  };

  const handleViewFile = (fileName: string) => {
    toast.info(`Opening ${fileName}...`);
  };

  const getFileIcon = (type: string) => {
    return <File className="h-5 w-5 text-blue-500" />;
  };

  return (
    <>
      <BaseModal
        open={open}
        onOpenChange={onOpenChange}
        title="Files"
        description="Manage your uploaded files"
        icon={File}
        size="lg"
        showSave={false}
        showDelete={false}
      >
        {/* Upload Button in Content */}
        <div className="flex justify-end mb-4">
          <Button onClick={handleUploadFile} size="sm" className="gap-2">
            <Upload className="h-4 w-4" />
            Upload File
          </Button>
        </div>

        {/* Files List */}
        <div className="space-y-3">
          {files.length === 0 ? (
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
                      onClick={() => handleViewFile(file.name)}
                      title="View"
                    >
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleDownloadFile(file.name)}
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

      <AlertDialog open={!!deleteFileId} onOpenChange={(open) => !open && setDeleteFileId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete File</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this file? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteFile} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
