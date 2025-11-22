import { useState } from "react";
import { Button } from "./ui/button";
import { ScrollArea } from "./ui/scroll-area";
import { MessageSquare, Plus, Trash2, X, Search, FileText, FolderKanban, Sparkles, Settings, Archive, ShieldCheck, LogOut, ChevronUp, Moon, Sun, Info, ChevronDown, ChevronRight, Brain, MoreVertical, Share, Download, Edit, Pin, Copy, FolderInput, Check, File, Layers, Users, Database } from "lucide-react";
import { useTheme } from "./ThemeProvider";
import { useDebug } from "./DebugContext";
import { SettingsModal } from "./SettingsModal";
import { AdminModal } from "./AdminModal";
import { AboutModal } from "./AboutModal";
import { ProjectModal, Project } from "./ProjectModal";
import { SearchModal } from "./SearchModal";
import { NotesModal } from "./NotesModal";
import { KnowledgeModal } from "./KnowledgeModal";
import { ShareModal } from "./ShareModal";
import { FilesModal } from "./FilesModal";
import { ArchivedModal } from "./ArchivedModal";
import { Input } from "./ui/input";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "./ui/collapsible";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "./ui/popover";
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { toast } from "sonner";

export interface Conversation {
  id: string;
  title: string;
  lastMessage: string;
  timestamp: Date;
}

interface ConversationSidebarProps {
  conversations: Conversation[];
  activeConversationId: string | null;
  onSelectConversation: (id: string) => void;
  onNewConversation: () => void;
  onDeleteConversation: (id: string) => void;
  onRenameConversation?: (id: string, newTitle: string) => void;
  onCloneConversation?: (id: string) => void;
  onClose?: () => void;
  enabledModels?: Record<string, boolean>;
  onToggleModel?: (modelId: string) => void;
}

export function ConversationSidebar({
  conversations,
  activeConversationId,
  onSelectConversation,
  onNewConversation,
  onDeleteConversation,
  onRenameConversation,
  onCloneConversation,
  onClose,
  enabledModels,
  onToggleModel,
}: ConversationSidebarProps) {
  const { theme, setTheme } = useTheme();
  const { showcaseMode } = useDebug();
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [adminOpen, setAdminOpen] = useState(false);
  const [aboutOpen, setAboutOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [notesOpen, setNotesOpen] = useState(false);
  const [knowledgeOpen, setKnowledgeOpen] = useState(false);
  const [projectModalOpen, setProjectModalOpen] = useState(false);
  const [shareModalOpen, setShareModalOpen] = useState(false);
  const [shareConversationId, setShareConversationId] = useState<string | null>(null);
  const [renamingId, setRenamingId] = useState<string | null>(null);
  const [renameValue, setRenameValue] = useState("");
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [selectedProject, setSelectedProject] = useState<Project | undefined>();
  const [projectModalMode, setProjectModalMode] = useState<"create" | "view">("create");
  const [projectsExpanded, setProjectsExpanded] = useState(false);
  const [filesOpen, setFilesOpen] = useState(false);
  const [archivedOpen, setArchivedOpen] = useState(false);
  const [pinnedConversations, setPinnedConversations] = useState<string[]>([]);
  const [archivedConversationsLocal, setArchivedConversationsLocal] = useState<string[]>([]);
  const [projects, setProjects] = useState<Project[]>([
    { id: "1", name: "Website Redesign", description: "Redesigning the company website", createdAt: new Date(2024, 0, 15) },
    { id: "2", name: "Mobile App", description: "iOS and Android app development", createdAt: new Date(2024, 1, 3) },
    { id: "3", name: "Marketing Campaign", description: "Q1 marketing initiatives", createdAt: new Date(2024, 2, 20) },
  ]);
  
  const formatDate = (date: Date) => {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) return "Today";
    if (days === 1) return "Yesterday";
    if (days < 7) return `${days} days ago`;
    return date.toLocaleDateString();
  };

  const handleNewProject = () => {
    setProjectModalMode("create");
    setSelectedProject(undefined);
    setProjectModalOpen(true);
  };

  const handleCreateProject = (newProject: Project) => {
    setProjects((prev) => [newProject, ...prev]);
    // Close the create modal and open the view modal
    setProjectModalOpen(false);
    setTimeout(() => {
      setSelectedProject(newProject);
      setProjectModalMode("view");
      setProjectModalOpen(true);
    }, 100);
  };

  const handleViewProject = (project: Project) => {
    setProjectModalMode("view");
    setSelectedProject(project);
    setProjectModalOpen(true);
  };

  const handleDownload = (conversationId: string) => {
    const conversation = conversations.find(c => c.id === conversationId);
    toast.success(`Downloading "${conversation?.title}"...`);
  };

  const handlePin = (conversationId: string) => {
    setPinnedConversations(prev => {
      if (prev.includes(conversationId)) {
        toast.success("Conversation unpinned");
        return prev.filter(id => id !== conversationId);
      } else {
        toast.success("Conversation pinned");
        return [...prev, conversationId];
      }
    });
  };

  const handleClone = (conversationId: string) => {
    const conversation = conversations.find(c => c.id === conversationId);
    onCloneConversation?.(conversationId);
    toast.success(`Cloned "${conversation?.title}"`);
  };

  const handleMove = (conversationId: string) => {
    const conversation = conversations.find(c => c.id === conversationId);
    toast.info(`Move "${conversation?.title}" to folder...`);
  };

  const handleArchive = (conversationId: string) => {
    const conversation = conversations.find(c => c.id === conversationId);
    setArchivedConversationsLocal(prev => [...prev, conversationId]);
    toast.success(`"${conversation?.title}" archived`);
  };

  const renderConversationItem = (conversation: Conversation) => (
    <div
      key={conversation.id}
      className={`group relative rounded-lg transition-colors ${
        activeConversationId === conversation.id
          ? "bg-gray-100 dark:bg-gray-800"
          : "hover:bg-gray-50 dark:hover:bg-gray-900"
      }`}
    >
      {renamingId === conversation.id ? (
        <div className="p-3 flex items-center gap-2">
          <MessageSquare className="h-4 w-4 shrink-0 text-gray-500" />
          <Input
            value={renameValue}
            onChange={(e) => setRenameValue(e.target.value)}
            className="h-8 text-sm"
            autoFocus
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                onRenameConversation?.(conversation.id, renameValue);
                setRenamingId(null);
              } else if (e.key === "Escape") {
                setRenamingId(null);
              }
            }}
          />
          <Button
            size="icon"
            variant="ghost"
            className="h-8 w-8 shrink-0"
            onClick={() => {
              onRenameConversation?.(conversation.id, renameValue);
              setRenamingId(null);
            }}
          >
            <Check className="h-4 w-4" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="h-8 w-8 shrink-0"
            onClick={() => setRenamingId(null)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ) : (
        <>
          <button
            onClick={() => onSelectConversation(conversation.id)}
            className="w-full text-left p-3 pr-10"
          >
            <div className="flex items-start gap-2">
              <MessageSquare className="h-4 w-4 mt-1 shrink-0 text-gray-500" />
              <div className="flex-1 min-w-0">
                <div className="font-medium truncate">
                  {conversation.title}
                </div>
              </div>
            </div>
          </button>

          <Popover>
            <PopoverTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-2 top-2 opacity-0 group-hover:opacity-100 transition-opacity h-8 w-8"
                onClick={(e) => e.stopPropagation()}
              >
                <MoreVertical className="h-4 w-4" />
              </Button>
            </PopoverTrigger>
            <PopoverContent 
              side="right" 
              align="start" 
              className="w-48 p-1"
              onInteractOutside={(e) => showcaseMode && e.preventDefault()}
            >
              <div className="space-y-1">
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                  onClick={() => {
                    setShareConversationId(conversation.id);
                    setShareModalOpen(true);
                  }}
                >
                  <Share className="h-4 w-4" />
                  <span>Share</span>
                </button>
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                  onClick={() => handleDownload(conversation.id)}
                >
                  <Download className="h-4 w-4" />
                  <span>Download</span>
                </button>
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                  onClick={() => {
                    setRenameValue(conversation.title);
                    setRenamingId(conversation.id);
                  }}
                >
                  <Edit className="h-4 w-4" />
                  <span>Rename</span>
                </button>
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                  onClick={() => handlePin(conversation.id)}
                >
                  <Pin className="h-4 w-4" />
                  <span>{pinnedConversations.includes(conversation.id) ? 'Unpin' : 'Pin'}</span>
                </button>
                <Popover>
                  <PopoverTrigger asChild>
                    <button 
                      className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left cursor-pointer"
                    >
                      <FolderInput className="h-4 w-4" />
                      <span>Move to Project</span>
                      <ChevronRight className="h-4 w-4 ml-auto text-gray-400" />
                    </button>
                  </PopoverTrigger>
                  <PopoverContent 
                    side="right" 
                    align="start" 
                    className="w-64 p-0 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    onInteractOutside={(e) => showcaseMode && e.preventDefault()}
                  >
                    <div className="px-4 py-2.5 border-b border-gray-800">
                      <div className="text-xs text-gray-400">Projects</div>
                    </div>
                    <ScrollArea className="h-80">
                      <div className="p-2 space-y-1">
                        {projects.length === 0 ? (
                          <div className="px-3 py-2 text-sm text-gray-400">No projects available</div>
                        ) : (
                          projects.map((project) => (
                            <button
                              key={project.id}
                              className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                              onClick={() => {
                                toast.success(`Moved to "${project.name}"`);
                              }}
                            >
                              <FolderKanban className="h-4 w-4" />
                              <span>{project.name}</span>
                            </button>
                          ))
                        )}
                      </div>
                    </ScrollArea>
                  </PopoverContent>
                </Popover>
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                  onClick={() => handleArchive(conversation.id)}
                >
                  <Archive className="h-4 w-4" />
                  <span>Archive</span>
                </button>
                <button 
                  className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left text-red-600 dark:text-red-400"
                  onClick={() => setDeleteConfirmId(conversation.id)}
                >
                  <Trash2 className="h-4 w-4" />
                  <span>Delete</span>
                </button>
              </div>
            </PopoverContent>
          </Popover>
        </>
      )}
    </div>
  );

  const pinnedList = conversations.filter(c => pinnedConversations.includes(c.id));
  const recentList = conversations.filter(c => !pinnedConversations.includes(c.id));

  return (
    <>
      <SettingsModal open={settingsOpen} onOpenChange={setSettingsOpen} />
      <AdminModal 
        open={adminOpen} 
        onOpenChange={setAdminOpen}
        enabledModels={enabledModels}
        onToggleModel={onToggleModel}
      />
      <AboutModal open={aboutOpen} onOpenChange={setAboutOpen} />
      <SearchModal open={searchOpen} onOpenChange={setSearchOpen} />
      <NotesModal open={notesOpen} onOpenChange={setNotesOpen} />
      <KnowledgeModal open={knowledgeOpen} onOpenChange={setKnowledgeOpen} />
      <ProjectModal
        open={projectModalOpen}
        onOpenChange={setProjectModalOpen}
        project={selectedProject}
        mode={projectModalMode}
        onCreateProject={handleCreateProject}
      />
      
      <div className="flex flex-col h-full bg-white dark:bg-gray-950 border-r border-gray-200 dark:border-gray-800">
      {/* Logo and Close Button */}
      <div className="flex-shrink-0 flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-800">
        <div className="flex items-center gap-2">
          <Sparkles className="h-6 w-6 text-purple-500" />
          <span className="text-xl font-bold text-gray-900 dark:text-white">
            aimate
          </span>
        </div>
        {onClose && (
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        )}
      </div>

      {/* Navigation Links */}
      <nav className="flex-shrink-0 p-3 space-y-1 border-b border-gray-200 dark:border-gray-800">
        <Button
          onClick={onNewConversation}
          variant="ghost"
          className="w-full justify-start gap-3"
        >
          <MessageSquare className="h-4 w-4" />
          Start a new chat
        </Button>
        <Button
          variant="ghost"
          className="w-full justify-start gap-3"
          onClick={() => setSearchOpen(true)}
        >
          <Search className="h-4 w-4" />
          Search
        </Button>
        <Button
          variant="ghost"
          className="w-full justify-start gap-3"
          onClick={() => setNotesOpen(true)}
        >
          <FileText className="h-4 w-4" />
          Notes
        </Button>
        <Button
          variant="ghost"
          className="w-full justify-start gap-3"
          onClick={() => setKnowledgeOpen(true)}
        >
          <Brain className="h-4 w-4" />
          Knowledge
        </Button>
        <Button
          variant="ghost"
          className="w-full justify-start gap-3"
          onClick={() => setFilesOpen(true)}
        >
          <File className="h-4 w-4" />
          Files
        </Button>
        <Collapsible open={projectsExpanded} onOpenChange={setProjectsExpanded}>
          <CollapsibleTrigger asChild>
            <Button
              variant="ghost"
              className="w-full justify-start gap-3"
            >
              <FolderKanban className="h-4 w-4" />
              Projects
              {projectsExpanded ? (
                <ChevronDown className="h-4 w-4 ml-auto" />
              ) : (
                <ChevronRight className="h-4 w-4 ml-auto" />
              )}
            </Button>
          </CollapsibleTrigger>
          <CollapsibleContent className="space-y-1 mt-1">
            <Button
              variant="ghost"
              size="sm"
              className="w-full justify-start gap-3 pl-10 text-sm"
              onClick={handleNewProject}
            >
              <Plus className="h-3 w-3" />
              New Project
            </Button>
            {projects.map((project) => (
              <div key={project.id} className="relative group">
                <Button
                  variant="ghost"
                  size="sm"
                  className="w-full justify-start gap-3 pl-10 text-sm pr-8"
                  onClick={() => handleViewProject(project)}
                >
                  <FolderKanban className="h-3 w-3" />
                  {project.name}
                </Button>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="absolute right-1 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transition-opacity h-6 w-6"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <MoreVertical className="h-3 w-3" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent 
                    side="right" 
                    align="start" 
                    className="w-40 p-1"
                    onInteractOutside={(e) => showcaseMode && e.preventDefault()}
                  >
                    <div className="space-y-1">
                      <button className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left">
                        <Archive className="h-4 w-4" />
                        <span>Archive</span>
                      </button>
                      <button className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left text-red-600 dark:text-red-400">
                        <Trash2 className="h-4 w-4" />
                        <span>Delete</span>
                      </button>
                    </div>
                  </PopoverContent>
                </Popover>
              </div>
            ))}
          </CollapsibleContent>
        </Collapsible>
      </nav>

      {/* Conversations Section with ScrollArea */}
      <div className="flex-1 overflow-hidden flex flex-col">
        <ScrollArea className="flex-1">
        <div className="p-2 space-y-4">
          {conversations.length === 0 ? (
            <div className="text-center py-8 px-4 text-sm text-gray-500 dark:text-gray-400">
              No conversations yet. Start a new chat!
            </div>
          ) : (
            <>
              {/* Pinned Conversations */}
              {pinnedList.length > 0 && (
                <div className="space-y-1">
                  <div className="px-2 py-1">
                    <h2 className="text-xs font-semibold text-gray-500 dark:text-gray-400">Pinned</h2>
                  </div>
                  {pinnedList.map(renderConversationItem)}
                </div>
              )}

              {/* Recent Conversations */}
              {recentList.length > 0 && (
                <div className="space-y-1">
                  {pinnedList.length > 0 && (
                    <div className="px-2 py-1">
                      <h2 className="text-xs font-semibold text-gray-500 dark:text-gray-400">Recent</h2>
                    </div>
                  )}
                  {recentList.map(renderConversationItem)}
                </div>
              )}
            </>
          )}
        </div>
        </ScrollArea>
      </div>

      {/* User Footer */}
      <div className="flex-shrink-0 border-t border-gray-200 dark:border-gray-800 p-3">
        <DropdownMenu modal={false}>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              className="w-full justify-start gap-3 h-auto py-2 px-3"
            >
              <Avatar className="h-8 w-8">
                <AvatarImage src="https://api.dicebear.com/7.x/avataaars/svg?seed=user" />
                <AvatarFallback className="bg-purple-500 text-white">JD</AvatarFallback>
              </Avatar>
              <div className="flex-1 text-left overflow-hidden">
                <div className="text-sm font-medium truncate">John Doe</div>
                <div className="text-xs text-gray-500 dark:text-gray-400 truncate">
                  john.doe@example.com
                </div>
              </div>
              <ChevronUp className="h-4 w-4 text-gray-500" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent
            side="top"
            align="start"
            className="w-56"
            sideOffset={8}
            onInteractOutside={(e) => showcaseMode && e.preventDefault()}
          >
            <DropdownMenuItem 
              className="gap-2 cursor-pointer"
              onClick={() => setSettingsOpen(true)}
            >
              <Settings className="h-4 w-4" />
              Settings
            </DropdownMenuItem>
            <DropdownMenuItem 
              className="gap-2 cursor-pointer"
              onClick={() => setArchivedOpen(true)}
            >
              <Archive className="h-4 w-4" />
              Archived
            </DropdownMenuItem>
            <DropdownMenuItem 
              className="gap-2 cursor-pointer"
              onClick={() => setAdminOpen(true)}
            >
              <ShieldCheck className="h-4 w-4" />
              Admin Panel
            </DropdownMenuItem>
            <DropdownMenuItem 
              className="gap-2 cursor-pointer"
              onClick={() => setAboutOpen(true)}
            >
              <Info className="h-4 w-4" />
              About
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem className="gap-2 cursor-pointer text-red-600 dark:text-red-400">
              <LogOut className="h-4 w-4" />
              Logout
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Share Modal */}
      <ShareModal 
        open={shareModalOpen} 
        onOpenChange={setShareModalOpen}
        conversationTitle={conversations.find(c => c.id === shareConversationId)?.title}
      />

      {/* Files Modal */}
      <FilesModal 
        open={filesOpen}
        onOpenChange={setFilesOpen}
      />

      {/* Archived Modal */}
      <ArchivedModal 
        open={archivedOpen}
        onOpenChange={setArchivedOpen}
      />

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteConfirmId !== null} onOpenChange={(open) => !open && setDeleteConfirmId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete conversation?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete this conversation.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeleteConfirmId(null)}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => {
                if (deleteConfirmId) {
                  onDeleteConversation(deleteConfirmId);
                  setDeleteConfirmId(null);
                }
              }}
              className="bg-red-500 hover:bg-red-600"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
    </>
  );
}