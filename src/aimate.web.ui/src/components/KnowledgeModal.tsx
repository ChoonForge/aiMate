import { useState, useEffect, useMemo, useRef } from "react";
import { FileText, Globe, FileType, Video, Headphones, Code, Image as ImageIcon, Search, Filter, Plus, Download, ExternalLink, Trash2, Eye, EyeOff, X, ChevronDown, Grid3x3, List, FolderOpen, Tag, Clock, User, Hash, Sparkles, Calendar, TrendingUp, Layers, ArrowUpDown, Check, BarChart3, Brain, File, GitBranch, Link2, Upload, Settings, Share2, History, Loader2, Star, Zap, RefreshCw } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { Badge } from "./ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Checkbox } from "./ui/checkbox";
import { Label } from "./ui/label";
import { Separator } from "./ui/separator";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "./ui/collapsible";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "./ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { ScrollArea } from "./ui/scroll-area";
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "./ui/alert-dialog";
import { Progress } from "./ui/progress";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator, DropdownMenuTrigger } from "./ui/dropdown-menu";
import { useDebug } from "./DebugContext";
import { toast } from "sonner";
import { useAppData } from "../context/AppDataContext";
import { useSwipeGesture } from "../utils/useSwipeGesture";

interface KnowledgeItem {
  id: string;
  title: string;
  type: "Web" | "PDF" | "Document" | "Video" | "Audio" | "Code" | "Image";
  date: string;
  tags: string[];
  summary?: string;
  entities?: string[];
  collection?: string;
  size?: string;
  usageCount?: number;
  lastUsed?: string;
  source?: string;
  version?: number;
  updatedBy?: string;
}

interface Collection {
  id: string;
  name: string;
  description: string;
  itemCount: number;
  color: string;
}

interface ArtifactItem {
  id: string;
  title: string;
  content: string;
  date: string;
  type?: string;
  version?: number;
  tags?: string[];
}

interface KnowledgeModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function KnowledgeModal({ open, onOpenChange }: KnowledgeModalProps) {
  const { addLog } = useDebug();
  const { knowledge } = useAppData();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [activeTab, setActiveTab] = useState("knowledge");
  const [searchQuery, setSearchQuery] = useState("");
  const [filterType, setFilterType] = useState("all");
  const [filterCollection, setFilterCollection] = useState("all");
  const [sortBy, setSortBy] = useState("recent");
  const [collections, setCollections] = useState<Collection[]>([]);
  const [artifacts, setArtifacts] = useState<ArtifactItem[]>([
    {
      id: "1",
      title: "API Response Analysis",
      content: "Detailed analysis of the API response...",
      date: "Nov 3, 2025",
      type: "Analysis",
      version: 2,
      tags: ["API", "Analysis"]
    },
    {
      id: "2",
      title: "Code Optimization Ideas",
      content: "Several approaches to optimize performance...",
      date: "Nov 2, 2025",
      type: "Code",
      version: 1,
      tags: ["Performance", "Optimization"]
    },
  ]);
  const [deleteItemId, setDeleteItemId] = useState<string | null>(null);
  const [deleteArtifactId, setDeleteArtifactId] = useState<string | null>(null);
  const [showImportMenu, setShowImportMenu] = useState(false);
  const [selectedItems, setSelectedItems] = useState<string[]>([]);

  // Map documents from hook to KnowledgeItem format
  const items: KnowledgeItem[] = useMemo(() => {
    return knowledge.documents.map((doc) => ({
      id: doc.id,
      title: doc.title || doc.fileName,
      type: mapFileTypeToKnowledgeType(doc.fileType),
      date: formatDate(doc.createdAt),
      tags: doc.tags || [],
      summary: `${doc.fileName} - ${doc.status === 'ready' ? `${doc.chunkCount} chunks indexed` : doc.status}`,
      entities: [],
      collection: undefined,
      size: formatFileSize(doc.fileSize),
      usageCount: 0,
      lastUsed: formatDate(doc.updatedAt),
      source: doc.fileName,
      version: 1,
    }));
  }, [knowledge.documents]);

  const tabs = [
    { id: "knowledge", label: "Knowledge", icon: FileText },
    { id: "collections", label: "Collections", icon: FolderOpen },
    { id: "artifacts", label: "Artifacts", icon: Sparkles },
    { id: "analytics", label: "Analytics", icon: BarChart3 },
  ];

  // Swipe gesture handlers
  const handleSwipeLeft = () => {
    const currentIndex = tabs.findIndex(tab => tab.id === activeTab);
    if (currentIndex < tabs.length - 1) {
      setActiveTab(tabs[currentIndex + 1].id);
    }
  };

  const handleSwipeRight = () => {
    const currentIndex = tabs.findIndex(tab => tab.id === activeTab);
    if (currentIndex > 0) {
      setActiveTab(tabs[currentIndex - 1].id);
    }
  };

  const swipeRef = useSwipeGesture({
    onSwipeLeft: handleSwipeLeft,
    onSwipeRight: handleSwipeRight,
  });

  // Load collections when modal opens
  useEffect(() => {
    if (open) {
      loadCollections();
      addLog({
        category: 'knowledge:modal',
        action: 'Loaded Knowledge',
        api: '/api/v1/knowledge',
        payload: knowledge.documents,
        type: 'success'
      });
    }
  }, [open, knowledge.documents]);

  const loadCollections = () => {
    setCollections([
      { id: "1", name: "API Documentation", description: "All API related docs", itemCount: 15, color: "purple" },
      { id: "2", name: "Meeting Notes", description: "Team meeting notes", itemCount: 8, color: "blue" },
      { id: "3", name: "Design Specs", description: "Product design specifications", itemCount: 12, color: "green" },
      { id: "4", name: "Frontend Development", description: "React, TypeScript, CSS", itemCount: 23, color: "orange" },
    ]);
  };

  const handleDeleteItem = async () => {
    if (deleteItemId) {
      try {
        await knowledge.deleteDocument(deleteItemId);
        toast.success("Knowledge item deleted");
        addLog({
          category: 'knowledge:modal',
          action: 'Deleted Document',
          api: '/api/v1/knowledge',
          payload: { id: deleteItemId },
          type: 'success'
        });
      } catch (err) {
        toast.error("Failed to delete document");
        addLog({
          category: 'knowledge:modal',
          action: 'Delete Failed',
          api: '/api/v1/knowledge',
          payload: { id: deleteItemId, error: err },
          type: 'error'
        });
      }
      setDeleteItemId(null);
    }
  };

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    try {
      await knowledge.uploadDocument(file);
      toast.success(`Uploaded ${file.name}`);
      addLog({
        category: 'knowledge:modal',
        action: 'Uploaded Document',
        api: '/api/v1/knowledge/upload',
        payload: { fileName: file.name, size: file.size },
        type: 'success'
      });
    } catch (err) {
      toast.error(`Failed to upload ${file.name}`);
      addLog({
        category: 'knowledge:modal',
        action: 'Upload Failed',
        api: '/api/v1/knowledge/upload',
        payload: { fileName: file.name, error: err },
        type: 'error'
      });
    }

    // Reset the file input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleRefresh = () => {
    knowledge.refresh();
    toast.success("Refreshing knowledge base...");
  };

  const handleDeleteArtifact = () => {
    if (deleteArtifactId) {
      setArtifacts((prev) => prev.filter((item) => item.id !== deleteArtifactId));
      toast.success("Artifact deleted");
      setDeleteArtifactId(null);
    }
  };

  const handleImport = (source: string) => {
    toast.success(`Importing from ${source}...`);
    setShowImportMenu(false);
  };

  const handleBulkOperation = (operation: string) => {
    toast.success(`${operation} ${selectedItems.length} items`);
    setSelectedItems([]);
  };

  const toggleItemSelection = (id: string) => {
    setSelectedItems(prev => 
      prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
    );
  };

  // Filter and search items
  const filteredItems = items.filter(item => {
    const matchesSearch = searchQuery === "" || 
      item.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.tags.some(tag => tag.toLowerCase().includes(searchQuery.toLowerCase())) ||
      item.summary?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.entities?.some(e => e.toLowerCase().includes(searchQuery.toLowerCase()));
    
    const matchesType = filterType === "all" || item.type === filterType;
    const matchesCollection = filterCollection === "all" || item.collection === filterCollection;
    
    return matchesSearch && matchesType && matchesCollection;
  });

  // Sort items
  const sortedItems = [...filteredItems].sort((a, b) => {
    switch (sortBy) {
      case "recent":
        return new Date(b.date).getTime() - new Date(a.date).getTime();
      case "alphabetical":
        return a.title.localeCompare(b.title);
      case "usage":
        return (b.usageCount || 0) - (a.usageCount || 0);
      default:
        return 0;
    }
  });

  const getIcon = (type: KnowledgeItem["type"]) => {
    switch (type) {
      case "Web":
        return <Globe className="h-5 w-5 text-blue-500" />;
      case "PDF":
        return <FileText className="h-5 w-5 text-red-500" />;
      case "Document":
        return <File className="h-5 w-5 text-green-500" />;
      case "Video":
        return <FileText className="h-5 w-5 text-purple-500" />;
      case "Audio":
        return <FileText className="h-5 w-5 text-orange-500" />;
      case "Code":
        return <FileText className="h-5 w-5 text-cyan-500" />;
      case "Image":
        return <FileText className="h-5 w-5 text-pink-500" />;
    }
  };

  const getCollectionColor = (color: string) => {
    const colors: { [key: string]: string } = {
      purple: "bg-purple-100 dark:bg-purple-950 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-800",
      blue: "bg-blue-100 dark:bg-blue-950 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-800",
      green: "bg-green-100 dark:bg-green-950 text-green-700 dark:text-green-300 border-green-200 dark:border-green-800",
      orange: "bg-orange-100 dark:bg-orange-950 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-800",
    };
    return colors[color] || colors.purple;
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-[95vw] sm:max-w-[90vw] lg:max-w-[85vw] xl:max-w-7xl max-h-[95vh] sm:max-h-[90vh] h-[95vh] sm:h-[90vh] p-0 gap-0 flex flex-col">
          <DialogHeader className="px-4 sm:px-6 py-3 sm:py-4 border-b border-gray-200 dark:border-gray-800">
            <DialogTitle className="flex items-center gap-2">
              <Brain className="h-4 w-4 sm:h-5 sm:w-5" />
              Knowledge Base
            </DialogTitle>
            <DialogDescription className="sr-only">
              Manage your knowledge base with advanced search, collections, and analytics
            </DialogDescription>
          </DialogHeader>

          {/* Horizontal Scrolling Tabs */}
          <div className="flex-shrink-0 border-b border-gray-200 dark:border-gray-800 overflow-x-auto tab-scrollbar">
            <div className="flex px-3 sm:px-6 gap-1 min-w-max">
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={(e) => {
                      setActiveTab(tab.id);
                      e.currentTarget.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
                    }}
                    className={`flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 border-b-2 transition-colors whitespace-nowrap cursor-pointer text-sm ${
                      activeTab === tab.id
                        ? "border-purple-500 text-purple-600 dark:text-purple-400"
                        : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                    }`}
                  >
                    <Icon className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                    <span className="hidden xs:inline">{tab.label}</span>
                    <span className="xs:hidden">{tab.label.split(' ')[0]}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Tab Content */}
          {activeTab === "knowledge" && (
            <div ref={swipeRef} className="flex-1 flex flex-col overflow-hidden">
              {/* Search and Filters */}
              <div className="px-3 sm:px-6 py-3 sm:py-4 border-b border-gray-200 dark:border-gray-800 space-y-2 sm:space-y-3">
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <Search className="absolute left-2 sm:left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                      placeholder="Search knowledge..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="pl-8 sm:pl-10 text-sm"
                    />
                  </div>
                  <DropdownMenu open={showImportMenu} onOpenChange={setShowImportMenu}>
                    <DropdownMenuTrigger asChild>
                      <Button variant="outline" className="gap-2" size="sm">
                        <Upload className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                        <span className="hidden sm:inline">Import</span>
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="w-56">
                      <DropdownMenuItem onClick={() => handleImport("Google Drive")}>
                        <Globe className="h-4 w-4 mr-2" />
                        Google Drive
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleImport("Notion")}>
                        <FileText className="h-4 w-4 mr-2" />
                        Notion
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleImport("GitHub")}>
                        <GitBranch className="h-4 w-4 mr-2" />
                        GitHub
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleImport("Confluence")}>
                        <Link2 className="h-4 w-4 mr-2" />
                        Confluence
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem onClick={() => handleImport("Web Scraper")}>
                        <Globe className="h-4 w-4 mr-2" />
                        Web Scraper
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleImport("Email")}>
                        <FileText className="h-4 w-4 mr-2" />
                        Email Forward
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleImport("RSS")}>
                        <Download className="h-4 w-4 mr-2" />
                        RSS Feed
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleRefresh}
                    disabled={knowledge.loading}
                  >
                    <RefreshCw className={`h-3.5 w-3.5 sm:h-4 sm:w-4 ${knowledge.loading ? 'animate-spin' : ''}`} />
                  </Button>
                  <Button
                    className="gap-2"
                    size="sm"
                    onClick={() => fileInputRef.current?.click()}
                    disabled={knowledge.uploading}
                  >
                    {knowledge.uploading ? (
                      <Loader2 className="h-3.5 w-3.5 sm:h-4 sm:w-4 animate-spin" />
                    ) : (
                      <Plus className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                    )}
                    <span className="hidden sm:inline">{knowledge.uploading ? 'Uploading...' : 'Add'}</span>
                  </Button>
                </div>

                {/* Filters */}
                <div className="flex gap-2 flex-wrap">
                  <Select value={filterType} onValueChange={setFilterType}>
                    <SelectTrigger className="w-[110px] sm:w-[140px] text-sm">
                      <Filter className="h-4 w-4 mr-2" />
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Types</SelectItem>
                      <SelectItem value="Web">Web</SelectItem>
                      <SelectItem value="PDF">PDF</SelectItem>
                      <SelectItem value="Document">Document</SelectItem>
                      <SelectItem value="Video">Video</SelectItem>
                      <SelectItem value="Audio">Audio</SelectItem>
                      <SelectItem value="Code">Code</SelectItem>
                    </SelectContent>
                  </Select>

                  <Select value={filterCollection} onValueChange={setFilterCollection}>
                    <SelectTrigger className="w-[130px] sm:w-[180px] text-sm">
                      <FolderOpen className="h-4 w-4 mr-2" />
                      <SelectValue placeholder="All Collections" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Collections</SelectItem>
                      {collections.map(col => (
                        <SelectItem key={col.id} value={col.name}>{col.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>

                  <Select value={sortBy} onValueChange={setSortBy}>
                    <SelectTrigger className="w-[110px] sm:w-[140px] text-sm">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="recent">Recent</SelectItem>
                      <SelectItem value="alphabetical">A-Z</SelectItem>
                      <SelectItem value="usage">Most Used</SelectItem>
                    </SelectContent>
                  </Select>

                  {selectedItems.length > 0 && (
                    <div className="flex gap-2 ml-auto">
                      <Badge variant="secondary">{selectedItems.length} selected</Badge>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="outline" size="sm">
                            Bulk Actions
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent>
                          <DropdownMenuItem onClick={() => handleBulkOperation("Tag")}>
                            <Tag className="h-4 w-4 mr-2" />
                            Add Tags
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleBulkOperation("Move to collection")}>
                            <FolderOpen className="h-4 w-4 mr-2" />
                            Move to Collection
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleBulkOperation("Share")}>
                            <Share2 className="h-4 w-4 mr-2" />
                            Share
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem onClick={() => handleBulkOperation("Delete")} className="text-red-600">
                            <Trash2 className="h-4 w-4 mr-2" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  )}
                </div>
              </div>

              {/* Knowledge Items List */}
              <ScrollArea className="flex-1">
                <div className="px-3 sm:px-6 py-3 sm:py-4 space-y-2 sm:space-y-3">
                  {knowledge.loading ? (
                    <div className="flex flex-col items-center justify-center py-12 gap-3 text-gray-500 dark:text-gray-400">
                      <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
                      <p>Loading knowledge...</p>
                    </div>
                  ) : sortedItems.length === 0 ? (
                    <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                      {searchQuery ? "No results found. Try different keywords." : "No knowledge items yet. Import or add your first item!"}
                    </div>
                  ) : (
                    sortedItems.map((item) => (
                      <div
                        key={item.id}
                        className={`p-2.5 sm:p-3 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors ${
                          selectedItems.includes(item.id) ? "border-purple-500 bg-purple-50 dark:bg-purple-950/20" : "border-gray-200 dark:border-gray-800"
                        }`}
                      >
                        <div className="flex items-start gap-2 sm:gap-3">
                          <input
                            type="checkbox"
                            checked={selectedItems.includes(item.id)}
                            onChange={() => toggleItemSelection(item.id)}
                            className="mt-0.5 sm:mt-1 flex-shrink-0"
                          />
                          <div className="flex-shrink-0">{getIcon(item.type)}</div>
                          <div className="flex-1 min-w-0">
                            <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-2">
                              <div className="flex-1 min-w-0">
                                <h4 className="font-medium mb-0.5 sm:mb-1 truncate text-sm sm:text-base">{item.title}</h4>
                                {item.summary && (
                                  <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mb-1.5 sm:mb-2 line-clamp-2">
                                    {item.summary}
                                  </p>
                                )}
                                
                                {/* Tags and Entities */}
                                <div className="flex flex-wrap gap-1 mb-1.5 sm:mb-2">
                                  {item.tags.slice(0, 2).map(tag => (
                                    <Badge key={tag} variant="secondary" className="text-xs h-5">
                                      <Tag className="h-2.5 w-2.5 sm:h-3 sm:w-3 mr-0.5 sm:mr-1" />
                                      {tag}
                                    </Badge>
                                  ))}
                                  {item.entities?.slice(0, 2).map(entity => (
                                    <Badge key={entity} variant="outline" className="text-xs h-5">
                                      {entity}
                                    </Badge>
                                  ))}
                                  {item.tags.length > 2 && (
                                    <Badge variant="secondary" className="text-xs h-5">
                                      +{item.tags.length - 2}
                                    </Badge>
                                  )}
                                </div>

                                {/* Metadata */}
                                <div className="flex flex-wrap gap-2 sm:gap-3 text-xs text-gray-500 dark:text-gray-400">
                                  <span className="flex items-center gap-1">
                                    <Clock className="h-2.5 w-2.5 sm:h-3 sm:w-3" />
                                    {item.date}
                                  </span>
                                  {item.usageCount !== undefined && (
                                    <span className="flex items-center gap-1">
                                      <Eye className="h-2.5 w-2.5 sm:h-3 sm:w-3" />
                                      <span className="hidden sm:inline">Used {item.usageCount} times</span>
                                      <span className="sm:hidden">{item.usageCount}</span>
                                    </span>
                                  )}
                                  {item.version && item.version > 1 && (
                                    <span className="flex items-center gap-1">
                                      <GitBranch className="h-2.5 w-2.5 sm:h-3 sm:w-3" />
                                      v{item.version}
                                    </span>
                                  )}
                                  {item.collection && (
                                    <span className="flex items-center gap-1 hidden sm:flex">
                                      <FolderOpen className="h-3 w-3" />
                                      {item.collection}
                                    </span>
                                  )}
                                </div>
                              </div>

                              {/* Actions */}
                              <div className="flex gap-0.5 sm:gap-1 flex-shrink-0">
                                <Button variant="ghost" size="icon" className="h-7 w-7 sm:h-8 sm:w-8 hidden sm:flex">
                                  <Eye className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                                <Button variant="ghost" size="icon" className="h-7 w-7 sm:h-8 sm:w-8 hidden sm:flex">
                                  <History className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                                <Button variant="ghost" size="icon" className="h-7 w-7 sm:h-8 sm:w-8">
                                  <Share2 className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  className="h-7 w-7 sm:h-8 sm:w-8 text-red-600 hover:text-red-700 dark:text-red-400"
                                  onClick={() => setDeleteItemId(item.id)}
                                >
                                  <Trash2 className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </ScrollArea>
            </div>
          )}

          {/* Collections Tab */}
          {activeTab === "collections" && (
            <div ref={swipeRef} className="flex-1 flex flex-col overflow-hidden">
              <div className="px-3 sm:px-6 py-3 sm:py-4 border-b border-gray-200 dark:border-gray-800">
                <Button className="gap-2" size="sm">
                  <Plus className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                  New Collection
                </Button>
              </div>
              <ScrollArea className="flex-1">
                <div className="px-3 sm:px-6 py-3 sm:py-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 sm:gap-4">
                  {collections.map((collection) => (
                    <div
                      key={collection.id}
                      className={`p-4 sm:p-5 border-2 rounded-lg cursor-pointer hover:shadow-lg transition-all ${getCollectionColor(collection.color)}`}
                    >
                      <div className="flex items-start justify-between mb-2 sm:mb-3">
                        <FolderOpen className="h-5 w-5 sm:h-6 sm:w-6" />
                        <Badge variant="secondary" className="text-xs">{collection.itemCount} items</Badge>
                      </div>
                      <h3 className="font-semibold mb-1 text-sm sm:text-base">{collection.name}</h3>
                      <p className="text-xs sm:text-sm opacity-80 mb-3 sm:mb-4 line-clamp-2">{collection.description}</p>
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" className="flex-1 text-xs sm:text-sm">
                          View Items
                        </Button>
                        <Button size="sm" variant="ghost" className="px-2 sm:px-3">
                          <Settings className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </div>
          )}

          {/* Artifacts Tab */}
          {activeTab === "artifacts" && (
            <div ref={swipeRef} className="flex-1 flex flex-col overflow-hidden">
              <ScrollArea className="flex-1">
                <div className="px-3 sm:px-6 py-3 sm:py-4 space-y-2 sm:space-y-3">
                  {artifacts.length === 0 ? (
                    <div className="text-center py-8 text-gray-500 dark:text-gray-400 text-sm">
                      No artifacts yet. Save content using the Brain icon under chat bubbles!
                    </div>
                  ) : (
                    artifacts.map((artifact) => (
                      <div
                        key={artifact.id}
                        className="p-2.5 sm:p-3 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
                      >
                        <div className="flex items-start gap-2 sm:gap-3">
                          <Sparkles className="h-4 w-4 sm:h-5 sm:w-5 text-purple-500 flex-shrink-0 mt-0.5" />
                          <div className="flex-1 min-w-0">
                            <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-2 mb-1.5 sm:mb-2">
                              <div className="flex-1 min-w-0">
                                <h4 className="font-medium mb-0.5 sm:mb-1 text-sm sm:text-base">{artifact.title}</h4>
                                <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mb-1.5 sm:mb-2 line-clamp-2">
                                  {artifact.content}
                                </p>
                              </div>
                              <div className="flex gap-0.5 sm:gap-1 flex-shrink-0">
                                <Button variant="ghost" size="icon" className="h-7 w-7 sm:h-8 sm:w-8">
                                  <Eye className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                                <Button variant="ghost" size="icon" className="h-7 w-7 sm:h-8 sm:w-8">
                                  <Download className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  className="h-7 w-7 sm:h-8 sm:w-8 text-red-600 hover:text-red-700 dark:text-red-400"
                                  onClick={() => setDeleteArtifactId(artifact.id)}
                                >
                                  <Trash2 className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                              </div>
                            </div>

                            {/* Artifact metadata */}
                            <div className="flex flex-wrap gap-1 sm:gap-2">
                              {artifact.type && (
                                <Badge variant="secondary" className="text-xs h-5">{artifact.type}</Badge>
                              )}
                              {artifact.tags?.slice(0, 2).map(tag => (
                                <Badge key={tag} variant="outline" className="text-xs h-5">
                                  {tag}
                                </Badge>
                              ))}
                              {artifact.version && artifact.version > 1 && (
                                <Badge variant="outline" className="text-xs h-5">
                                  v{artifact.version}
                                </Badge>
                              )}
                              <span className="text-xs text-gray-500 dark:text-gray-500 ml-auto">
                                {artifact.date}
                              </span>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </ScrollArea>
            </div>
          )}

          {/* Analytics Tab */}
          {activeTab === "analytics" && (
            <div ref={swipeRef} className="flex-1 flex flex-col overflow-hidden">
              <ScrollArea className="flex-1">
                <div className="px-3 sm:px-6 py-3 sm:py-4 space-y-4 sm:space-y-6">
                  {/* Summary Cards */}
                  <div className="grid grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                    <div className="p-3 sm:p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                      <div className="flex items-center gap-1.5 sm:gap-2 mb-1.5 sm:mb-2">
                        <FileText className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-purple-500" />
                        <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">Total Items</span>
                      </div>
                      <p className="text-lg sm:text-2xl font-semibold">{items.length}</p>
                      <p className="text-xs text-green-500 mt-0.5 sm:mt-1">↑ 23 this mo</p>
                    </div>

                    <div className="p-3 sm:p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                      <div className="flex items-center gap-1.5 sm:gap-2 mb-1.5 sm:mb-2">
                        <Eye className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-blue-500" />
                        <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">Views</span>
                      </div>
                      <p className="text-lg sm:text-2xl font-semibold">2,847</p>
                      <p className="text-xs text-green-500 mt-0.5 sm:mt-1">↑ 12%</p>
                    </div>

                    <div className="p-3 sm:p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                      <div className="flex items-center gap-1.5 sm:gap-2 mb-1.5 sm:mb-2">
                        <FolderOpen className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-green-500" />
                        <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">Collections</span>
                      </div>
                      <p className="text-lg sm:text-2xl font-semibold">{collections.length}</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 sm:mt-1 hidden sm:block">Avg 14/col</p>
                    </div>

                    <div className="p-3 sm:p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                      <div className="flex items-center gap-1.5 sm:gap-2 mb-1.5 sm:mb-2">
                        <TrendingUp className="h-3.5 w-3.5 sm:h-4 sm:w-4 text-orange-500" />
                        <span className="text-xs sm:text-sm text-gray-500 dark:text-gray-400">Engage</span>
                      </div>
                      <p className="text-lg sm:text-2xl font-semibold">87%</p>
                      <p className="text-xs text-green-500 mt-0.5 sm:mt-1">↑ 5%</p>
                    </div>
                  </div>

                  <Separator />

                  {/* Most Used Items */}
                  <div>
                    <h3 className="text-sm sm:text-base font-semibold mb-2 sm:mb-3 flex items-center gap-2">
                      <Star className="h-4 w-4 sm:h-5 sm:w-5 text-yellow-500" />
                      Most Referenced Documents
                    </h3>
                    <div className="space-y-1.5 sm:space-y-2">
                      {items.slice(0, 5).map((item, index) => (
                        <div key={item.id} className="flex items-center gap-2 sm:gap-3 p-2 sm:p-3 border border-gray-200 dark:border-gray-800 rounded-lg">
                          <span className="font-semibold text-gray-400 w-4 sm:w-6 text-sm">{index + 1}</span>
                          {getIcon(item.type)}
                          <div className="flex-1 min-w-0">
                            <p className="font-medium truncate text-xs sm:text-sm">{item.title}</p>
                            <p className="text-xs text-gray-500 dark:text-gray-400">
                              <span className="hidden sm:inline">Used {item.usageCount || 0} times • Last used {item.lastUsed || item.date}</span>
                              <span className="sm:hidden">{item.usageCount || 0} uses</span>
                            </p>
                          </div>
                          <Progress value={(item.usageCount || 0) * 2} className="w-16 sm:w-24 hidden sm:block" />
                        </div>
                      ))}
                    </div>
                  </div>

                  <Separator />

                  {/* Unused Knowledge */}
                  <div>
                    <h3 className="text-sm sm:text-base font-semibold mb-2 sm:mb-3 flex items-center gap-2">
                      <Zap className="h-4 w-4 sm:h-5 sm:w-5 text-gray-400" />
                      <span className="hidden sm:inline">Unused Knowledge (Consider Archiving)</span>
                      <span className="sm:hidden">Unused</span>
                    </h3>
                    <div className="space-y-1.5 sm:space-y-2">
                      {items.filter(i => (i.usageCount || 0) === 0).slice(0, 3).map((item) => (
                        <div key={item.id} className="flex items-center justify-between gap-2 p-2 sm:p-3 border border-gray-200 dark:border-gray-800 rounded-lg">
                          <div className="flex items-center gap-2 sm:gap-3 flex-1 min-w-0">
                            {getIcon(item.type)}
                            <div className="flex-1 min-w-0">
                              <p className="font-medium truncate text-xs sm:text-sm">{item.title}</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400">
                                <span className="hidden sm:inline">Added {item.date} • Never used</span>
                                <span className="sm:hidden">{item.date}</span>
                              </p>
                            </div>
                          </div>
                          <Button variant="outline" size="sm" className="text-xs">Archive</Button>
                        </div>
                      ))}
                    </div>
                  </div>

                  <Separator />

                  {/* Knowledge Health */}
                  <div>
                    <h3 className="font-semibold mb-3">Knowledge Base Health</h3>
                    <div className="space-y-3">
                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Coverage Quality</span>
                          <span className="font-semibold">92%</span>
                        </div>
                        <Progress value={92} className="h-2" />
                        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                          Good coverage across topics
                        </p>
                      </div>

                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Freshness Score</span>
                          <span className="font-semibold">78%</span>
                        </div>
                        <Progress value={78} className="h-2" />
                        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                          3 documents need updating
                        </p>
                      </div>

                      <div>
                        <div className="flex justify-between text-sm mb-1">
                          <span>Organization</span>
                          <span className="font-semibold">85%</span>
                        </div>
                        <Progress value={85} className="h-2" />
                        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                          Most items properly tagged
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </ScrollArea>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Dialogs */}
      <AlertDialog open={!!deleteItemId} onOpenChange={(open) => !open && setDeleteItemId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Knowledge Item</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this item? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteItem} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={!!deleteArtifactId} onOpenChange={(open) => !open && setDeleteArtifactId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Artifact</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this artifact? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteArtifact} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Hidden file input for uploads */}
      <input
        ref={fileInputRef}
        type="file"
        className="hidden"
        onChange={handleFileUpload}
        accept=".pdf,.doc,.docx,.txt,.md,.json,.csv"
      />
    </>
  );
}

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

function mapFileTypeToKnowledgeType(fileType: string): KnowledgeItem['type'] {
  const type = fileType.toLowerCase();
  if (type.includes('pdf')) return 'PDF';
  if (type.includes('doc') || type.includes('text') || type.includes('markdown')) return 'Document';
  if (type.includes('video') || type.includes('mp4') || type.includes('webm')) return 'Video';
  if (type.includes('audio') || type.includes('mp3') || type.includes('wav')) return 'Audio';
  if (type.includes('image') || type.includes('png') || type.includes('jpg') || type.includes('jpeg')) return 'Image';
  if (type.includes('code') || type.includes('json') || type.includes('javascript') || type.includes('typescript')) return 'Code';
  if (type.includes('http') || type.includes('url') || type.includes('web')) return 'Web';
  return 'Document';
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
    return dateString;
  }
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
}
