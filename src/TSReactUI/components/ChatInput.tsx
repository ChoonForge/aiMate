import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Textarea } from "./ui/textarea";
import { Input } from "./ui/input";
import { Send, Plus, Wrench, Paperclip, Camera, Globe, FileText, Brain, ChevronRight, Settings2, LayoutGrid, Search, Code, MoreHorizontal, CloudSun, MapPin, MessageSquare, Upload, X, Loader2 } from "lucide-react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "./ui/popover";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Switch } from "./ui/switch";
import { Separator } from "./ui/separator";
import { Label } from "./ui/label";
import { ScrollArea } from "./ui/scroll-area";
import { Slider } from "./ui/slider";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { useDebug } from "./DebugContext";
import { toast } from "sonner@2.0.3";
import * as API from "../utils/api-stubs";
import { KnowledgeSuggestions } from "./KnowledgeSuggestions";
import { AttachedContext } from "./AttachedContext";

interface ChatInputProps {
  onSend: (message: string) => void;
  disabled?: boolean;
}

interface AttachedItem {
  id: string;
  name: string;
  type: "file" | "image" | "document" | "note" | "knowledge" | "chat" | "webpage";
  size?: string;
  preview?: string;
}

export function ChatInput({ onSend, disabled }: ChatInputProps) {
  const { addLog, showcaseMode } = useDebug();
  const [message, setMessage] = useState("");
  const [attachMenuOpen, setAttachMenuOpen] = useState(false);
  const [notesMenuOpen, setNotesMenuOpen] = useState(false);
  const [knowledgeMenuOpen, setKnowledgeMenuOpen] = useState(false);
  const [filesMenuOpen, setFilesMenuOpen] = useState(false);
  const [webpageMenuOpen, setWebpageMenuOpen] = useState(false);
  const [referenceChatMenuOpen, setReferenceChatMenuOpen] = useState(false);
  const [toolsMenuOpen, setToolsMenuOpen] = useState(false);
  const [weatherModalOpen, setWeatherModalOpen] = useState(false);
  const [webpageUrl, setWebpageUrl] = useState("");
  const [isDragging, setIsDragging] = useState(false);
  const [attachedItems, setAttachedItems] = useState<AttachedItem[]>([]);
  const [attachedKnowledge, setAttachedKnowledge] = useState<string[]>([]);
  const [attachedNotes, setAttachedNotes] = useState<string[]>([]);
  const [attachedFiles, setAttachedFiles] = useState<string[]>([]);
  const [attachedChats, setAttachedChats] = useState<string[]>([]);
  const [attachedWebpages, setAttachedWebpages] = useState<string[]>([]);
  
  // Data from API stubs
  const [notes, setNotes] = useState<any[]>([]);
  const [knowledge, setKnowledge] = useState<any[]>([]);
  const [files, setFiles] = useState<any[]>([]);
  const [chats, setChats] = useState<any[]>([]);
  const [webpages, setWebpages] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  
  const [enabledTools, setEnabledTools] = useState({
    webSearch: false,
    codeInterpreter: true,
    fileManagement: false,
    weatherForecast: false,
    echoMCP: true,
  });

  const [weatherSettings, setWeatherSettings] = useState({
    unit: "imperial",
    currentLocation: true,
    showTemp: true,
    showRainfall: true,
    showWind: false,
    useEmojis: true,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (message.trim() && !disabled) {
      // Log the send event
      addLog({
        action: 'Message Sent',
        api: '/api/v1/SendMessage',
        payload: { message: message.trim(), timestamp: new Date().toISOString() },
        type: 'success'
      });
      onSend(message.trim());
      setMessage("");
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  const toggleTool = (tool: keyof typeof enabledTools) => {
    setEnabledTools((prev) => ({ ...prev, [tool]: !prev[tool] }));
  };

  const enabledToolsCount = Object.values(enabledTools).filter(Boolean).length;

  // Load data when menus open
  useEffect(() => {
    if (notesMenuOpen && notes.length === 0) {
      loadNotes();
    }
  }, [notesMenuOpen]);

  useEffect(() => {
    if (knowledgeMenuOpen && knowledge.length === 0) {
      loadKnowledge();
    }
  }, [knowledgeMenuOpen]);

  useEffect(() => {
    if (filesMenuOpen && files.length === 0) {
      loadFiles();
    }
  }, [filesMenuOpen]);

  useEffect(() => {
    if (referenceChatMenuOpen && chats.length === 0) {
      loadChats();
    }
  }, [referenceChatMenuOpen]);

  const loadNotes = async () => {
    setLoading(true);
    const response = await API.getNotes();
    if (response.success && response.data) {
      setNotes(response.data);
      addLog({
        action: 'Loaded Notes',
        api: response.metadata?.endpoint || '/api/v1/GetNotes',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const loadKnowledge = async () => {
    setLoading(true);
    const response = await API.getKnowledge();
    if (response.success && response.data) {
      setKnowledge(response.data);
      addLog({
        action: 'Loaded Knowledge',
        api: response.metadata?.endpoint || '/api/v1/GetKnowledge',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const loadFiles = async () => {
    setLoading(true);
    const response = await API.getFiles();
    if (response.success && response.data) {
      setFiles(response.data);
      addLog({
        action: 'Loaded Files',
        api: response.metadata?.endpoint || '/api/v1/GetFiles',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const loadChats = async () => {
    setLoading(true);
    const response = await API.getChats();
    if (response.success && response.data) {
      setChats(response.data);
      addLog({
        action: 'Loaded Chats',
        api: response.metadata?.endpoint || '/api/v1/GetChats',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const handleAttachNote = async (noteId: string) => {
    setAttachedNotes((prev) => [...prev, noteId]);
    const response = await API.attachNote(noteId, "current_chat_id");
    if (response.success) {
      toast.success("Note attached to chat context");
      addLog({
        action: 'Attached Note',
        api: response.metadata?.endpoint || '/api/v1/AttachNote',
        payload: response.data,
        type: 'success'
      });
      setNotesMenuOpen(false);
      setAttachMenuOpen(false);
    }
  };

  const handleAttachKnowledge = async (knowledgeId: string) => {
    setAttachedKnowledge((prev) => [...prev, knowledgeId]);
    const response = await API.attachKnowledge(knowledgeId, "current_chat_id");
    if (response.success) {
      toast.success("Knowledge attached to chat context");
      addLog({
        action: 'Attached Knowledge',
        api: response.metadata?.endpoint || '/api/v1/AttachKnowledge',
        payload: response.data,
        type: 'success'
      });
      setKnowledgeMenuOpen(false);
      setAttachMenuOpen(false);
    }
  };

  const handleAttachFile = async (fileId: string) => {
    setAttachedFiles((prev) => [...prev, fileId]);
    const response = await API.attachFile(fileId, "current_chat_id");
    if (response.success) {
      toast.success("File attached to chat context");
      addLog({
        action: 'Attached File',
        api: response.metadata?.endpoint || '/api/v1/AttachFile',
        payload: response.data,
        type: 'success'
      });
      setFilesMenuOpen(false);
      setAttachMenuOpen(false);
    }
  };

  const handleReferenceChat = async (chatId: string) => {
    setAttachedChats((prev) => [...prev, chatId]);
    const response = await API.referenceChat(chatId, "current_chat_id");
    if (response.success) {
      toast.success("Chat referenced in context");
      addLog({
        action: 'Referenced Chat',
        api: response.metadata?.endpoint || '/api/v1/ReferenceChat',
        payload: response.data,
        type: 'success'
      });
      setReferenceChatMenuOpen(false);
      setAttachMenuOpen(false);
    }
  };

  const handleAttachWebpage = async () => {
    if (webpageUrl.trim()) {
      const response = await API.attachWebpage(webpageUrl, "current_chat_id");
      if (response.success) {
        // Create a webpage ID and add to attached webpages
        const webpageId = `webpage-${Date.now()}`;
        setAttachedWebpages((prev) => [...prev, webpageId]);
        
        // Add to webpages list for display
        setWebpages((prev) => [...prev, {
          id: webpageId,
          title: response.data?.title || webpageUrl,
          url: webpageUrl,
          description: response.data?.description || ""
        }]);
        
        toast.success("Webpage attached successfully!");
        addLog({
          action: 'Attached Webpage',
          api: response.metadata?.endpoint || '/api/v1/AttachWebpage',
          payload: response.data,
          type: 'success'
        });
        setWebpageUrl("");
        setWebpageMenuOpen(false);
        setAttachMenuOpen(false);
      }
    }
  };

  const handleDragEnter = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.currentTarget === e.target) {
      setIsDragging(false);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    const droppedFiles = Array.from(e.dataTransfer.files);
    
    if (droppedFiles.length > 0) {
      const newItems: AttachedItem[] = droppedFiles.map((file) => {
        const isImage = file.type.startsWith("image/");
        const isDoc = ["application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain"].includes(file.type);
        
        return {
          id: `${Date.now()}-${file.name}`,
          name: file.name,
          type: isImage ? "image" : isDoc ? "document" : "file",
          size: formatFileSize(file.size),
          preview: isImage ? URL.createObjectURL(file) : undefined,
        };
      });

      setAttachedItems((prev) => [...prev, ...newItems]);
      toast.success(`${droppedFiles.length} file(s) attached`);
      
      addLog({
        action: 'Files Dropped',
        api: '/api/v1/DropFiles',
        payload: { fileCount: droppedFiles.length, files: newItems },
        type: 'success'
      });
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + " B";
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + " KB";
    return (bytes / (1024 * 1024)).toFixed(1) + " MB";
  };

  const removeAttachedItem = (id: string) => {
    setAttachedItems((prev) => prev.filter((item) => item.id !== id));
    toast.success("Attachment removed");
  };

  const handleDetachKnowledge = (id: string) => {
    setAttachedKnowledge((prev) => prev.filter((k) => k !== id));
    toast.success("Knowledge detached");
  };

  const handleDetachNote = (id: string) => {
    setAttachedNotes((prev) => prev.filter((n) => n !== id));
    toast.success("Note detached");
  };

  const handleDetachFile = (id: string) => {
    setAttachedFiles((prev) => prev.filter((f) => f !== id));
    toast.success("File detached");
  };

  const handleDetachChat = (id: string) => {
    setAttachedChats((prev) => prev.filter((c) => c !== id));
    toast.success("Chat reference removed");
  };

  const handleDetachWebpage = (id: string) => {
    setAttachedWebpages((prev) => prev.filter((item) => item !== id));
    setWebpages((prev) => prev.filter((item) => item.id !== id));
    toast.success("Webpage removed");
  };

  const handleAttachCollection = (collectionId: string) => {
    // Mock: attach all items from collection
    toast.success(`Attached entire collection (simulated)`);
  };

  return (
    <>
      {/* Weather Forecast Settings Modal */}
      <Dialog open={weatherModalOpen} onOpenChange={setWeatherModalOpen}>
        <DialogContent className="max-w-md bg-gray-900 dark:bg-gray-900 text-white border-gray-700">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-white">
              <CloudSun className="h-5 w-5" />
              Weather Forecast Settings
            </DialogTitle>
            <DialogDescription className="text-gray-400">
              Configure your weather forecast preferences
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-6 py-4">
            {/* Unit of Measure */}
            <div className="space-y-2">
              <Label>Preferred Unit of Measure</Label>
              <Select
                value={weatherSettings.unit}
                onValueChange={(value) =>
                  setWeatherSettings((prev) => ({ ...prev, unit: value }))
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="imperial">Imperial (°F, mph)</SelectItem>
                  <SelectItem value="metric">Metric (°C, km/h)</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Current Location */}
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <MapPin className="h-4 w-4 text-gray-500" />
                <Label>Use Current Location</Label>
              </div>
              <Switch
                checked={weatherSettings.currentLocation}
                onCheckedChange={(checked) =>
                  setWeatherSettings((prev) => ({ ...prev, currentLocation: checked }))
                }
                className="data-[state=checked]:bg-purple-600"
              />
            </div>

            <Separator />

            {/* Display Options */}
            <div className="space-y-4">
              <Label>Display Options</Label>
              
              <div className="flex items-center justify-between">
                <span className="text-sm">Show Temperature</span>
                <Switch
                  checked={weatherSettings.showTemp}
                  onCheckedChange={(checked) =>
                    setWeatherSettings((prev) => ({ ...prev, showTemp: checked }))
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm">Show Rainfall</span>
                <Switch
                  checked={weatherSettings.showRainfall}
                  onCheckedChange={(checked) =>
                    setWeatherSettings((prev) => ({ ...prev, showRainfall: checked }))
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm">Show Wind</span>
                <Switch
                  checked={weatherSettings.showWind}
                  onCheckedChange={(checked) =>
                    setWeatherSettings((prev) => ({ ...prev, showWind: checked }))
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm">Use Emojis</span>
                <Switch
                  checked={weatherSettings.useEmojis}
                  onCheckedChange={(checked) =>
                    setWeatherSettings((prev) => ({ ...prev, useEmojis: checked }))
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <form onSubmit={handleSubmit} className="space-y-2">
        {/* Attached Items Display */}
        {attachedItems.length > 0 && (
          <div className="flex flex-wrap gap-2 p-2 bg-gray-100 dark:bg-gray-900 rounded-2xl">
            {attachedItems.map((item) => (
              <div
                key={item.id}
                className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg group"
              >
                {item.preview ? (
                  <img src={item.preview} alt={item.name} className="h-8 w-8 object-cover rounded" />
                ) : (
                  <div className="flex items-center justify-center h-8 w-8 bg-purple-100 dark:bg-purple-900/30 rounded">
                    {item.type === "document" ? (
                      <FileText className="h-4 w-4 text-purple-600 dark:text-purple-400" />
                    ) : (
                      <Paperclip className="h-4 w-4 text-purple-600 dark:text-purple-400" />
                    )}
                  </div>
                )}
                <div className="flex-1 min-w-0">
                  <div className="text-sm truncate">{item.name}</div>
                  {item.size && <div className="text-xs text-gray-500 dark:text-gray-400">{item.size}</div>}
                </div>
                <button
                  type="button"
                  onClick={() => removeAttachedItem(item.id)}
                  className="opacity-0 group-hover:opacity-100 transition-opacity p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded cursor-pointer"
                >
                  <X className="h-3 w-3" />
                </button>
              </div>
            ))}
          </div>
        )}

        {/* Attached Context Display */}
        <AttachedContext
          attachedKnowledge={attachedKnowledge}
          attachedNotes={attachedNotes}
          attachedFiles={attachedFiles}
          attachedChats={attachedChats}
          attachedWebpages={attachedWebpages}
          knowledgeItems={knowledge}
          noteItems={notes}
          fileItems={files}
          chatItems={chats}
          webpageItems={webpages}
          onDetachKnowledge={handleDetachKnowledge}
          onDetachNote={handleDetachNote}
          onDetachFile={handleDetachFile}
          onDetachChat={handleDetachChat}
          onDetachWebpage={handleDetachWebpage}
        />

        {/* Smart Knowledge Suggestions */}
        <KnowledgeSuggestions
          inputValue={message}
          attachedKnowledge={attachedKnowledge}
          onAttach={handleAttachKnowledge}
          onDetach={handleDetachKnowledge}
          onAttachCollection={handleAttachCollection}
        />

        {/* Drop Zone Overlay */}
        {isDragging && (
          <div className="absolute inset-0 z-50 flex items-center justify-center bg-purple-500/10 dark:bg-purple-500/20 border-2 border-dashed border-purple-500 rounded-3xl backdrop-blur-sm">
            <div className="text-center p-8">
              <Upload className="h-12 w-12 mx-auto mb-3 text-purple-600 dark:text-purple-400" />
              <div className="text-lg font-medium text-purple-600 dark:text-purple-400">Drop files here</div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Documents, images, and other files
              </div>
            </div>
          </div>
        )}

        <div
          className="flex gap-2 items-center bg-gray-100 dark:bg-gray-900 rounded-3xl p-2 pr-3 relative"
          onDragEnter={handleDragEnter}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
        >
          <div className="flex items-center gap-1 pl-1 shrink-0">
            {/* Attach Menu */}
            <Popover 
              open={attachMenuOpen} 
              onOpenChange={setAttachMenuOpen}
              modal={false}
            >
              <PopoverTrigger asChild onClick={() => setAttachMenuOpen(!attachMenuOpen)}>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  className="shrink-0 hover:bg-gray-100 dark:hover:bg-gray-800"
                >
                  <Plus className="h-5 w-5" />
                </Button>
              </PopoverTrigger>
              <PopoverContent 
                className="w-64 p-2" 
                align="start" 
                side="top"
                onInteractOutside={(e) => showcaseMode && e.preventDefault()}
              >
                <div className="space-y-1">
                  {/* Attach Files with Flyout */}
                  <Popover open={filesMenuOpen} onOpenChange={setFilesMenuOpen}>
                    <PopoverTrigger asChild>
                      <button
                        className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors justify-between text-left cursor-pointer"
                      >
                        <div className="flex items-center gap-3">
                          <Paperclip className="h-4 w-4" />
                          <span>Attach Files</span>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </button>
                    </PopoverTrigger>
                    <PopoverContent
                      side="right"
                      align="start"
                      className="w-64 p-0 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    >
                      <div className="px-4 py-2.5 border-b border-gray-800">
                        <div className="text-xs text-gray-400">Recent Files</div>
                      </div>
                      <ScrollArea className="h-80">
                        <div className="p-2 space-y-1">
                          {loading && files.length === 0 ? (
                            <div className="flex flex-col items-center justify-center py-8 gap-2 text-gray-400">
                              <Loader2 className="h-5 w-5 animate-spin" />
                              <span className="text-sm">Loading files...</span>
                            </div>
                          ) : files.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-gray-400">No files found</div>
                          ) : (
                            files.map((file) => (
                              <button
                                key={file.id}
                                className="w-full flex flex-col gap-1 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                onClick={() => handleAttachFile(file.id)}
                              >
                                <span>{file.title}</span>
                                <span className="text-xs text-gray-400">{file.size}</span>
                              </button>
                            ))
                          )}
                        </div>
                      </ScrollArea>
                    </PopoverContent>
                  </Popover>

                  <button
                    className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                    onClick={() => setAttachMenuOpen(false)}
                  >
                    <Camera className="h-4 w-4" />
                    <span>Capture</span>
                  </button>
                  
                  {/* Attach Webpage with Flyout */}
                  <Popover open={webpageMenuOpen} onOpenChange={setWebpageMenuOpen}>
                    <PopoverTrigger asChild>
                      <button
                        className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors justify-between text-left cursor-pointer"
                      >
                        <div className="flex items-center gap-3">
                          <Globe className="h-4 w-4" />
                          <span>Attach Webpage</span>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </button>
                    </PopoverTrigger>
                    <PopoverContent
                      side="right"
                      align="start"
                      className="w-80 p-3 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    >
                      <div className="space-y-3">
                        <div className="text-sm text-gray-400">Enter webpage URL</div>
                        <Input
                          placeholder="https://example.com"
                          value={webpageUrl}
                          onChange={(e) => setWebpageUrl(e.target.value)}
                          className="bg-gray-800 border-gray-700 text-white placeholder:text-gray-500"
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              handleAttachWebpage();
                            }
                          }}
                        />
                        <Button
                          onClick={handleAttachWebpage}
                          className="w-full"
                          size="sm"
                        >
                          Attach
                        </Button>
                      </div>
                    </PopoverContent>
                  </Popover>
                  
                  {/* Attach Notes with Flyout */}
                  <Popover open={notesMenuOpen} onOpenChange={setNotesMenuOpen}>
                    <PopoverTrigger asChild>
                      <button
                        className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors justify-between text-left cursor-pointer"
                      >
                        <div className="flex items-center gap-3">
                          <FileText className="h-4 w-4" />
                          <span>Attach Notes</span>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </button>
                    </PopoverTrigger>
                    <PopoverContent
                      side="right"
                      align="start"
                      className="w-64 p-0 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    >
                      <div className="px-4 py-2.5 border-b border-gray-800">
                        <div className="text-xs text-gray-400">Recent Notes</div>
                      </div>
                      <ScrollArea className="h-80">
                        <div className="p-2 space-y-1">
                          {loading && notes.length === 0 ? (
                            <div className="flex flex-col items-center justify-center py-8 gap-2 text-gray-400">
                              <Loader2 className="h-5 w-5 animate-spin" />
                              <span className="text-sm">Loading notes...</span>
                            </div>
                          ) : notes.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-gray-400">No notes found</div>
                          ) : (
                            notes.map((note) => (
                              <button
                                key={note.id}
                                className="w-full flex flex-col gap-1 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                onClick={() => handleAttachNote(note.id)}
                              >
                                <span>{note.title}</span>
                                <span className="text-xs text-gray-400">{note.date}</span>
                              </button>
                            ))
                          )}
                        </div>
                      </ScrollArea>
                    </PopoverContent>
                  </Popover>

                  {/* Attach Knowledge with Flyout */}
                  <Popover open={knowledgeMenuOpen} onOpenChange={setKnowledgeMenuOpen}>
                    <PopoverTrigger asChild>
                      <button
                        className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors justify-between text-left cursor-pointer"
                      >
                        <div className="flex items-center gap-3">
                          <Brain className="h-4 w-4" />
                          <span>Attach Knowledge</span>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </button>
                    </PopoverTrigger>
                    <PopoverContent
                      side="right"
                      align="start"
                      className="w-64 p-0 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    >
                      <div className="px-4 py-2.5 border-b border-gray-800">
                        <div className="text-xs text-gray-400">Knowledge Base</div>
                      </div>
                      <ScrollArea className="h-80">
                        <div className="p-2 space-y-1">
                          {loading && knowledge.length === 0 ? (
                            <div className="flex flex-col items-center justify-center py-8 gap-2 text-gray-400">
                              <Loader2 className="h-5 w-5 animate-spin" />
                              <span className="text-sm">Loading knowledge...</span>
                            </div>
                          ) : knowledge.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-gray-400">No knowledge found</div>
                          ) : (
                            knowledge.map((item) => (
                              <button
                                key={item.id}
                                className="w-full flex flex-col gap-1 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                onClick={() => handleAttachKnowledge(item.id)}
                              >
                                <span>{item.title}</span>
                                <span className="text-xs text-gray-400">{item.type}</span>
                              </button>
                            ))
                          )}
                        </div>
                      </ScrollArea>
                    </PopoverContent>
                  </Popover>

                  {/* Reference Chat with Flyout */}
                  <Popover open={referenceChatMenuOpen} onOpenChange={setReferenceChatMenuOpen}>
                    <PopoverTrigger asChild>
                      <button
                        className="w-full flex items-center gap-3 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors justify-between text-left cursor-pointer"
                      >
                        <div className="flex items-center gap-3">
                          <MessageSquare className="h-4 w-4" />
                          <span>Reference Chat</span>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400" />
                      </button>
                    </PopoverTrigger>
                    <PopoverContent
                      side="right"
                      align="start"
                      className="w-64 p-0 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                    >
                      <div className="px-4 py-2.5 border-b border-gray-800">
                        <div className="text-xs text-gray-400">Recent Chats</div>
                      </div>
                      <ScrollArea className="h-80">
                        <div className="p-2 space-y-1">
                          {loading && chats.length === 0 ? (
                            <div className="flex flex-col items-center justify-center py-8 gap-2 text-gray-400">
                              <Loader2 className="h-5 w-5 animate-spin" />
                              <span className="text-sm">Loading chats...</span>
                            </div>
                          ) : chats.length === 0 ? (
                            <div className="px-3 py-2 text-sm text-gray-400">No chats found</div>
                          ) : (
                            chats.map((chat) => (
                              <button
                                key={chat.id}
                                className="w-full flex flex-col gap-1 px-3 py-2 text-sm rounded-md hover:bg-gray-800 transition-colors text-left cursor-pointer"
                                onClick={() => handleReferenceChat(chat.id)}
                              >
                                <span>{chat.title}</span>
                                <span className="text-xs text-gray-400">{chat.date}</span>
                              </button>
                            ))
                          )}
                        </div>
                      </ScrollArea>
                    </PopoverContent>
                  </Popover>
                </div>
              </PopoverContent>
            </Popover>

            {/* Tools Menu - Single Level */}
            <Popover open={toolsMenuOpen} onOpenChange={setToolsMenuOpen}>
              <PopoverTrigger asChild onClick={() => setToolsMenuOpen(!toolsMenuOpen)}>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-9 w-9 rounded-full relative hover:bg-gray-200 dark:hover:bg-gray-800 text-gray-500 dark:text-gray-400"
                  type="button"
                >
                  <div className="flex items-center gap-1">
                    <Wrench className="h-5 w-5" />
                    {enabledToolsCount > 0 && (
                      <span className="text-xs font-medium">{enabledToolsCount}</span>
                    )}
                  </div>
                </Button>
              </PopoverTrigger>
                <PopoverContent
                  side="top"
                  align="start"
                  className="w-80 p-2 bg-gray-900 dark:bg-gray-900 text-white border-gray-700"
                  onInteractOutside={(e) => showcaseMode && e.preventDefault()}
                >
                  <div className="space-y-1">
                    {/* Web Search */}
                    <div className="flex items-center justify-between px-3 py-2.5 rounded-md hover:bg-gray-800 transition-colors">
                      <div className="flex items-center gap-3">
                        <Search className="h-4 w-4" />
                        <span className="text-sm">Web Search</span>
                      </div>
                      <Switch
                        checked={enabledTools.webSearch}
                        onCheckedChange={() => toggleTool("webSearch")}
                        className="data-[state=checked]:bg-purple-600"
                      />
                    </div>

                    {/* Code Interpreter */}
                    <div className="flex items-center justify-between px-3 py-2.5 rounded-md hover:bg-gray-800 transition-colors">
                      <div className="flex items-center gap-3">
                        <Code className="h-4 w-4" />
                        <span className="text-sm">Code Interpreter</span>
                      </div>
                      <Switch
                        checked={enabledTools.codeInterpreter}
                        onCheckedChange={() => toggleTool("codeInterpreter")}
                        className="data-[state=checked]:bg-purple-600"
                      />
                    </div>

                    {/* Supreme File Management */}
                    <div className="flex items-center justify-between px-3 py-2.5 rounded-md hover:bg-gray-800 transition-colors">
                      <div className="flex items-center gap-3">
                        <Wrench className="h-4 w-4" />
                        <span className="text-sm">Supreme File Management</span>
                      </div>
                      <Switch
                        checked={enabledTools.fileManagement}
                        onCheckedChange={() => toggleTool("fileManagement")}
                        className="data-[state=checked]:bg-purple-600"
                      />
                    </div>

                    {/* Weather Forecast */}
                    <div className="flex items-center justify-between px-3 py-2.5 rounded-md hover:bg-gray-800 transition-colors">
                      <div className="flex items-center gap-3 flex-1">
                        <CloudSun className="h-4 w-4" />
                        <span className="text-sm">Weather Forecast</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-6 w-6 hover:bg-gray-700"
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            setWeatherModalOpen(true);
                          }}
                        >
                          <Settings2 className="h-3.5 w-3.5" />
                        </Button>
                        <Switch
                          checked={enabledTools.weatherForecast}
                          onCheckedChange={() => toggleTool("weatherForecast")}
                          className="data-[state=checked]:bg-purple-600"
                        />
                      </div>
                    </div>

                    {/* EchoMCP */}
                    <div className="flex items-center justify-between px-3 py-2.5 rounded-md hover:bg-gray-800 transition-colors">
                      <div className="flex items-center gap-3">
                        <Wrench className="h-4 w-4" />
                        <span className="text-sm">EchoMCP</span>
                      </div>
                      <Switch
                        checked={enabledTools.echoMCP}
                        onCheckedChange={() => toggleTool("echoMCP")}
                        className="data-[state=checked]:bg-purple-600"
                      />
                    </div>
                  </div>
                </PopoverContent>
              </Popover>
          </div>

          <Textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Send a Message"
            disabled={disabled}
            className="min-h-[44px] max-h-[200px] resize-none border-0 bg-transparent focus-visible:ring-0 focus-visible:ring-offset-0 placeholder:text-gray-500 dark:placeholder:text-gray-500 flex-1"
            rows={1}
          />

          <Button
            type="submit"
            size="icon"
            disabled={!message.trim() || disabled}
            className="h-10 w-10 shrink-0 rounded-full bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-200 disabled:opacity-50 border border-gray-300 dark:border-gray-700"
          >
            <Send className="h-5 w-5" />
          </Button>
        </div>
      </form>
    </>
  );
}