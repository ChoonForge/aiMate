import { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Menu, MoreVertical, Plus, PanelLeftClose, PanelLeft, ChevronDown, Sparkles, MessageSquare } from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from "./ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { SettingsModal } from "./SettingsModal";
import { AboutModal } from "./AboutModal";
import { HelpModal } from "./HelpModal";
import { useDebug } from "./DebugContext";

interface ModelOption {
  id: string;
  name: string;
  color: string;
}

interface ChatHeaderProps {
  onNewChat: () => void;
  onToggleSidebar?: () => void;
  sidebarOpen?: boolean;
  selectedModel?: string;
  onModelChange?: (model: string) => void;
  enabledModels?: Record<string, boolean>;
  availableModels?: ModelOption[];
}

export function ChatHeader({
  onNewChat,
  onToggleSidebar,
  sidebarOpen,
  selectedModel = "simulated",
  onModelChange,
  enabledModels = {
    "simulated": true,
  },
  availableModels,
}: ChatHeaderProps) {
  const { showcaseMode } = useDebug();
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [aboutOpen, setAboutOpen] = useState(false);
  const [helpOpen, setHelpOpen] = useState(false);
  const [moreMenuOpen, setMoreMenuOpen] = useState(false);
  const [modelSelectOpen, setModelSelectOpen] = useState(false);

  // Use provided models or fall back to static list
  const defaultModels: ModelOption[] = [
    { id: "simulated", name: "Simulated", color: "text-gray-400" },
  ];

  const models = availableModels || defaultModels;

  const handleModelChange = (value: string) => {
    if (onModelChange) {
      onModelChange(value);
    }
  };

  return (
    <>
      <SettingsModal open={settingsOpen} onOpenChange={setSettingsOpen} />
      <AboutModal open={aboutOpen} onOpenChange={setAboutOpen} />
      <HelpModal open={helpOpen} onOpenChange={setHelpOpen} />
      
      <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-950">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center gap-3">
            <Button
              variant="ghost"
              size="icon"
              onClick={onToggleSidebar}
              className="shrink-0"
            >
              {sidebarOpen ? (
                <PanelLeftClose className="h-5 w-5" />
              ) : (
                <PanelLeft className="h-5 w-5" />
              )}
            </Button>
            
            {/* Model Selector */}
            <Select value={selectedModel} onValueChange={handleModelChange} open={modelSelectOpen} onOpenChange={setModelSelectOpen}>
              <SelectTrigger className="w-[200px] border-none bg-transparent hover:bg-gray-100 dark:hover:bg-gray-800 focus:ring-0 focus:ring-offset-0">
                <div className="flex items-center gap-2">
                  <Sparkles className={`h-4 w-4 ${models.find(m => m.id === selectedModel)?.color || "text-purple-500"}`} />
                  <SelectValue />
                </div>
              </SelectTrigger>
              <SelectContent>
                {models.filter(model => enabledModels[model.id]).map(model => (
                  <SelectItem key={model.id} value={model.id}>
                    <div className="flex items-center gap-2">
                      <Sparkles className={`h-4 w-4 ${model.color}`} />
                      <span>{model.name}</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={onNewChat}
              className="gap-2 hidden sm:flex"
            >
              <MessageSquare className="h-4 w-4" />
              <span>New Chat</span>
            </Button>
            
            <DropdownMenu open={moreMenuOpen} onOpenChange={setMoreMenuOpen}>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon">
                  <MoreVertical className="h-5 w-5" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => setSettingsOpen(true)}>
                  Settings
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setHelpOpen(true)}>
                  Help & FAQ
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => setAboutOpen(true)}>
                  About
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>
    </>
  );
}