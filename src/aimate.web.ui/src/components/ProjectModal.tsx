import { useState, useEffect, useRef } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import { FolderKanban, Upload, Download, Eye, Trash2, File, MessageSquare, MoreVertical, Pin, Share, Edit, Archive, Copy, ListRestart, Search, X, GripVertical, Check, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { ScrollArea } from "./ui/scroll-area";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "./ui/popover";
import { Checkbox } from "./ui/checkbox";
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
import { useSwipeGesture } from "../utils/useSwipeGesture";
import { useAppData } from "../context/AppDataContext";
import { useDebug } from "./DebugContext";

export interface Project {
  id: string;
  name: string;
  description: string;
  createdAt: Date;
}

interface ProjectFile {
  id: string;
  name: string;
  size: string;
  uploadedAt: string;
}

interface ProjectChat {
  id: string;
  title: string;
  lastMessage: string;
  timestamp: string;
  pinned?: boolean;
}

interface ProjectModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  project?: Project;
  mode: "create" | "view";
  onCreateProject?: (project: Project) => void;
}

export function ProjectModal({ open, onOpenChange, project, mode, onCreateProject }: ProjectModalProps) {
  const { projects: projectsHook } = useAppData();
  const { addLog } = useDebug();

  const [activeTab, setActiveTab] = useState("general");
  const [name, setName] = useState(project?.name || "");
  const [description, setDescription] = useState(project?.description || "");
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [files, setFiles] = useState<ProjectFile[]>(
    mode === "view" ? [
      { id: "1", name: "requirements.pdf", size: "2.4 MB", uploadedAt: "Nov 3, 2025" },
      { id: "2", name: "mockups.fig", size: "5.2 MB", uploadedAt: "Nov 2, 2025" },
      { id: "3", name: "data.xlsx", size: "1.1 MB", uploadedAt: "Nov 1, 2025" },
    ] : []
  );
  const [chats, setChats] = useState<ProjectChat[]>(
    mode === "view" ? [
      { id: "c1", title: "Project brainstorming session", lastMessage: "Let's think about the core features...", timestamp: "2 hours ago", pinned: true },
      { id: "c2", title: "Technical architecture discussion", lastMessage: "We should use a microservices approach...", timestamp: "Yesterday" },
      { id: "c3", title: "Design review", lastMessage: "The mockups look great, but...", timestamp: "Nov 2" },
    ] : []
  );
  const [deleteFileId, setDeleteFileId] = useState<string | null>(null);
  const [pinnedChats, setPinnedChats] = useState<string[]>(["c1"]);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedChats, setSelectedChats] = useState<string[]>([]);
  const [draggedChat, setDraggedChat] = useState<string | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  // Reset to first tab when modal opens
  useEffect(() => {
    if (open) {
      setActiveTab("general");
    }
  }, [open]);

  const handleSave = async () => {
    if (!name.trim()) {
      toast.error("Project name is required");
      return;
    }

    setSaving(true);
    try {
      if (mode === "create") {
        // Generate a project key from the name
        const projectKey = name.trim().toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 10) || 'PROJ';

        const createdProject = await projectsHook.createProject({
          key: projectKey,
          name: name.trim(),
          description: description.trim(),
          owner: 'User', // Will be filled by backend
          ownerEmail: 'user@example.com', // Will be filled by backend
          status: 'Planning',
          priority: 'Medium',
        });

        // Also call the callback if provided (for UI updates)
        if (onCreateProject && createdProject) {
          const localProject: Project = {
            id: createdProject.id,
            name: createdProject.name,
            description: createdProject.description,
            createdAt: new Date(createdProject.createdAt),
          };
          onCreateProject(localProject);
        }

        addLog({
          category: 'projects:modal',
          action: 'Created Project',
          api: '/api/v1/projects',
          payload: { name: name.trim() },
          type: 'success'
        });

        toast.success("Project created!");
      } else if (project) {
        await projectsHook.updateProject(project.id, {
          name: name.trim(),
          description: description.trim(),
        });

        addLog({
          category: 'projects:modal',
          action: 'Updated Project',
          api: '/api/v1/projects',
          payload: { id: project.id, name: name.trim() },
          type: 'success'
        });

        toast.success("Project saved!");
      }
      onOpenChange(false);
    } catch (err) {
      toast.error("Failed to save project");
      addLog({
        category: 'projects:modal',
        action: 'Save Failed',
        api: '/api/v1/projects',
        payload: { error: err },
        type: 'error'
      });
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!project) return;

    setDeleting(true);
    try {
      await projectsHook.deleteProject(project.id);

      addLog({
        category: 'projects:modal',
        action: 'Deleted Project',
        api: '/api/v1/projects',
        payload: { id: project.id },
        type: 'success'
      });

      toast.success("Project deleted successfully");
      onOpenChange(false);
    } catch (err) {
      toast.error("Failed to delete project");
      addLog({
        category: 'projects:modal',
        action: 'Delete Failed',
        api: '/api/v1/projects',
        payload: { id: project.id, error: err },
        type: 'error'
      });
    } finally {
      setDeleting(false);
    }
  };

  const handleAddFile = () => {
    // Trigger file picker
    const input = document.createElement("input");
    input.type = "file";
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        const newFile: ProjectFile = {
          id: Date.now().toString(),
          name: file.name,
          size: `${(file.size / 1024 / 1024).toFixed(1)} MB`,
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

  const handlePinChat = (chatId: string) => {
    if (pinnedChats.includes(chatId)) {
      setPinnedChats(pinnedChats.filter(id => id !== chatId));
    } else {
      setPinnedChats([...pinnedChats, chatId]);
    }
  };

  const handleMoveToMainList = (chatId: string) => {
    const chat = chats.find(c => c.id === chatId);
    if (chat) {
      toast.success(`"${chat.title}" moved to main chat list`);
      setChats(chats.filter(c => c.id !== chatId));
    }
  };

  const handleDeleteChat = (chatId: string) => {
    const chat = chats.find(c => c.id === chatId);
    if (chat) {
      toast.success(`"${chat.title}" deleted`);
      setChats(chats.filter(c => c.id !== chatId));
    }
  };

  const handleArchiveChat = (chatId: string) => {
    const chat = chats.find(c => c.id === chatId);
    if (chat) {
      toast.success(`"${chat.title}" archived`);
    }
  };

  const handleDragStart = (e: React.DragEvent, chatId: string) => {
    setDraggedChat(chatId);
    e.dataTransfer.effectAllowed = "move";
  };

  const handleDragOver = (e: React.DragEvent, index: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
    setDragOverIndex(index);
  };

  const handleDragLeave = () => {
    setDragOverIndex(null);
  };

  const handleDrop = (e: React.DragEvent, targetIndex: number) => {
    e.preventDefault();
    if (!draggedChat) return;

    const draggedIndex = chats.findIndex(c => c.id === draggedChat);
    if (draggedIndex === -1 || draggedIndex === targetIndex) {
      setDraggedChat(null);
      setDragOverIndex(null);
      return;
    }

    const newChats = [...chats];
    const [removed] = newChats.splice(draggedIndex, 1);
    newChats.splice(targetIndex, 0, removed);
    setChats(newChats);
    setDraggedChat(null);
    setDragOverIndex(null);
    toast.success("Chat reordered");
  };

  const handleDragEnd = () => {
    setDraggedChat(null);
    setDragOverIndex(null);
  };

  const toggleChatSelection = (chatId: string) => {
    setSelectedChats(prev => 
      prev.includes(chatId) 
        ? prev.filter(id => id !== chatId)
        : [...prev, chatId]
    );
  };

  const toggleSelectAll = () => {
    if (selectedChats.length === filteredChats.length) {
      setSelectedChats([]);
    } else {
      setSelectedChats(filteredChats.map(c => c.id));
    }
  };

  const handleBulkDelete = () => {
    setChats(chats.filter(c => !selectedChats.includes(c.id)));
    toast.success(`${selectedChats.length} chat(s) deleted`);
    setSelectedChats([]);
  };

  const handleBulkArchive = () => {
    toast.success(`${selectedChats.length} chat(s) archived`);
    setSelectedChats([]);
  };

  const handleBulkMoveToMain = () => {
    setChats(chats.filter(c => !selectedChats.includes(c.id)));
    toast.success(`${selectedChats.length} chat(s) moved to main list`);
    setSelectedChats([]);
  };

  const filteredChats = chats.filter(chat =>
    chat.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    chat.lastMessage.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const tabs = ["general", ...(mode === "view" ? ["files", "chats"] : [])];

  // Swipe gesture handlers
  const handleSwipeLeft = () => {
    const currentIndex = tabs.findIndex(tab => tab === activeTab);
    if (currentIndex < tabs.length - 1) {
      setActiveTab(tabs[currentIndex + 1]);
    }
  };

  const handleSwipeRight = () => {
    const currentIndex = tabs.findIndex(tab => tab === activeTab);
    if (currentIndex > 0) {
      setActiveTab(tabs[currentIndex - 1]);
    }
  };

  const swipeRef = useSwipeGesture({
    onSwipeLeft: handleSwipeLeft,
    onSwipeRight: handleSwipeRight,
  });

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-2xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col">
          <DialogHeader className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
            <DialogTitle className="flex items-center gap-2">
              <FolderKanban className="h-5 w-5 text-purple-500" />
              {mode === "create" ? "New Project" : project?.name}
            </DialogTitle>
            <DialogDescription>
              {mode === "create"
                ? "Create a new project to organize your work"
                : "View and manage project details"}
            </DialogDescription>
          </DialogHeader>

          {/* Tabs */}
          <div className="border-b border-gray-200 dark:border-gray-800">
            <div className="flex px-6 gap-1">
              <button
                onClick={() => setActiveTab("general")}
                className={`px-4 py-2 border-b-2 transition-colors cursor-pointer ${
                  activeTab === "general"
                    ? "border-purple-500 text-purple-600 dark:text-purple-400"
                    : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                }`}
              >
                General
              </button>
              {mode === "view" && (
                <>
                  <button
                    onClick={() => setActiveTab("chats")}
                    className={`px-4 py-2 border-b-2 transition-colors cursor-pointer ${
                      activeTab === "chats"
                        ? "border-purple-500 text-purple-600 dark:text-purple-400"
                        : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                    }`}
                  >
                    Chats
                  </button>
                  <button
                    onClick={() => setActiveTab("files")}
                    className={`px-4 py-2 border-b-2 transition-colors cursor-pointer ${
                      activeTab === "files"
                        ? "border-purple-500 text-purple-600 dark:text-purple-400"
                        : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                    }`}
                  >
                    Files
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Tab Content */}
          <div className="flex-1 overflow-auto">
            {activeTab === "general" ? (
              <div ref={swipeRef} className="px-6 py-4 space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="project-name">Project Name</Label>
                  <Input
                    id="project-name"
                    placeholder="Enter project name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    disabled={mode === "view"}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="project-description">Description</Label>
                  <Textarea
                    id="project-description"
                    placeholder="Enter project description (optional)"
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    disabled={mode === "view"}
                    className="min-h-[100px]"
                  />
                </div>

                {mode === "view" && project && (
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    Created: {project.createdAt.toLocaleDateString()}
                  </div>
                )}
              </div>
            ) : activeTab === "chats" ? (
              <div ref={swipeRef} className="h-full flex flex-col">
                {/* Search and Bulk Actions Bar */}
                <div className="flex-shrink-0 px-6 py-3 border-b border-gray-200 dark:border-gray-800 space-y-3">
                  {/* Search */}
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                      placeholder="Search chats..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="pl-9 pr-9"
                    />
                    {searchQuery && (
                      <button
                        onClick={() => setSearchQuery("")}
                        className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 cursor-pointer"
                      >
                        <X className="h-4 w-4" />
                      </button>
                    )}
                  </div>

                  {/* Bulk Actions */}
                  {selectedChats.length > 0 && (
                    <div className="flex items-center justify-between p-2 bg-purple-50 dark:bg-purple-950/20 rounded-lg border border-purple-200 dark:border-purple-800">
                      <span className="text-sm">
                        {selectedChats.length} selected
                      </span>
                      <div className="flex gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={handleBulkMoveToMain}
                          className="h-8 cursor-pointer"
                        >
                          Move to Main
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={handleBulkArchive}
                          className="h-8 cursor-pointer"
                        >
                          Archive
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={handleBulkDelete}
                          className="h-8 text-red-600 hover:text-red-700 dark:text-red-400 cursor-pointer"
                        >
                          Delete
                        </Button>
                      </div>
                    </div>
                  )}
                </div>

                <ScrollArea className="flex-1">
                  <div className="px-6 py-4 space-y-2">
                    {/* Select All */}
                    {filteredChats.length > 0 && (
                      <div className="flex items-center gap-2 pb-2 border-b border-gray-200 dark:border-gray-800 mb-2">
                        <Checkbox
                          checked={selectedChats.length === filteredChats.length && filteredChats.length > 0}
                          onCheckedChange={toggleSelectAll}
                          className="cursor-pointer"
                        />
                        <span className="text-sm text-gray-500 dark:text-gray-400">
                          Select all
                        </span>
                      </div>
                    )}

                    {filteredChats.length === 0 ? (
                      <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                        {searchQuery ? "No chats match your search" : "No chats in this project yet"}
                      </div>
                    ) : (
                      filteredChats.map((chat, index) => (
                        <div
                          key={chat.id}
                          draggable
                          onDragStart={(e) => handleDragStart(e, chat.id)}
                          onDragOver={(e) => handleDragOver(e, index)}
                          onDragLeave={handleDragLeave}
                          onDrop={(e) => handleDrop(e, index)}
                          onDragEnd={handleDragEnd}
                          className={`group relative flex items-start gap-3 p-3 rounded-lg transition-colors cursor-move ${
                            draggedChat === chat.id 
                              ? "opacity-50" 
                              : "hover:bg-gray-100 dark:hover:bg-gray-800"
                          } ${
                            dragOverIndex === index 
                              ? "border-t-2 border-purple-500" 
                              : ""
                          } ${
                            selectedChats.includes(chat.id)
                              ? "bg-purple-50 dark:bg-purple-950/20 border border-purple-200 dark:border-purple-800"
                              : ""
                          }`}
                        >
                          <Checkbox
                            checked={selectedChats.includes(chat.id)}
                            onCheckedChange={() => toggleChatSelection(chat.id)}
                            className="shrink-0 mt-1 cursor-pointer"
                            onClick={(e) => e.stopPropagation()}
                          />
                          <GripVertical className="h-5 w-5 text-gray-400 shrink-0 mt-0.5 opacity-0 group-hover:opacity-100 transition-opacity" />
                          <MessageSquare className="h-5 w-5 text-gray-500 dark:text-gray-400 shrink-0 mt-0.5" />
                          <div className="flex-1 min-w-0">
                            <div className="flex items-start justify-between gap-2">
                              <div className="flex items-center gap-2 flex-1 min-w-0">
                                <h4 className="font-medium truncate">{chat.title}</h4>
                                {pinnedChats.includes(chat.id) && (
                                  <Pin className="h-3 w-3 text-purple-500 shrink-0" />
                                )}
                              </div>
                              <span className="text-xs text-gray-500 dark:text-gray-400 shrink-0">{chat.timestamp}</span>
                            </div>
                            <p className="text-sm text-gray-500 dark:text-gray-400 truncate">{chat.lastMessage}</p>
                          </div>
                          <Popover>
                            <PopoverTrigger asChild>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                              >
                                <MoreVertical className="h-4 w-4" />
                              </Button>
                            </PopoverTrigger>
                            <PopoverContent
                              side="bottom"
                              align="end"
                              className="w-48 p-1 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                            >
                              <div className="space-y-1">
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => handlePinChat(chat.id)}
                                >
                                  <Pin className="h-4 w-4" />
                                  <span>{pinnedChats.includes(chat.id) ? 'Unpin' : 'Pin'}</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => toast.info("Share chat")}
                                >
                                  <Share className="h-4 w-4" />
                                  <span>Share</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => toast.info("Rename chat")}
                                >
                                  <Edit className="h-4 w-4" />
                                  <span>Rename</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => toast.info("Copy chat")}
                                >
                                  <Copy className="h-4 w-4" />
                                  <span>Duplicate</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => handleMoveToMainList(chat.id)}
                                >
                                  <ListRestart className="h-4 w-4" />
                                  <span>Move to Main List</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                  onClick={() => handleArchiveChat(chat.id)}
                                >
                                  <Archive className="h-4 w-4" />
                                  <span>Archive</span>
                                </button>
                                <button 
                                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left text-red-400 cursor-pointer"
                                  onClick={() => handleDeleteChat(chat.id)}
                                >
                                  <Trash2 className="h-4 w-4" />
                                  <span>Delete</span>
                                </button>
                              </div>
                            </PopoverContent>
                          </Popover>
                        </div>
                      ))
                    )}
                  </div>
                </ScrollArea>
              </div>
            ) : (
              <div ref={swipeRef} className="h-full flex flex-col">
                <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
                  <Button onClick={handleAddFile} className="gap-2 cursor-pointer">
                    <Upload className="h-4 w-4" />
                    Add File
                  </Button>
                </div>
                <ScrollArea className="flex-1">
                  <div className="px-6 py-4 space-y-3">
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
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-3">
                              <File className="h-5 w-5 text-gray-500" />
                              <div>
                                <h4 className="font-medium">{file.name}</h4>
                                <div className="flex gap-2 text-sm text-gray-500 dark:text-gray-400">
                                  <span>{file.size}</span>
                                  <span>•</span>
                                  <span>{file.uploadedAt}</span>
                                </div>
                              </div>
                            </div>
                            <div className="flex gap-1">
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 cursor-pointer"
                                onClick={() => handleViewFile(file.name)}
                                title="View"
                              >
                                <Eye className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 cursor-pointer"
                                onClick={() => handleDownloadFile(file.name)}
                                title="Download"
                              >
                                <Download className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-red-600 hover:text-red-700 dark:text-red-400 cursor-pointer"
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
                </ScrollArea>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex gap-2 justify-end">
            {mode === "view" && activeTab === "general" && (
              <Button
                variant="destructive"
                onClick={handleDelete}
                disabled={deleting}
                className="cursor-pointer"
              >
                {deleting ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Deleting...
                  </>
                ) : (
                  "Delete"
                )}
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={saving || deleting}
              className="cursor-pointer"
            >
              {mode === "view" ? "Close" : "Cancel"}
            </Button>
            {mode === "create" && (
              <Button
                onClick={handleSave}
                disabled={saving}
                className="cursor-pointer"
              >
                {saving ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Creating...
                  </>
                ) : (
                  "Create Project"
                )}
              </Button>
            )}
          </div>
        </DialogContent>
      </Dialog>

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
