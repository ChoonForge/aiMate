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

interface ChatHeaderProps {
  onNewChat: () => void;
  onToggleSidebar?: () => void;
  sidebarOpen?: boolean;
  selectedModel?: string;
  onModelChange?: (model: string) => void;
  enabledModels?: Record<string, boolean>;
}

export function ChatHeader({ 
  onNewChat, 
  onToggleSidebar, 
  sidebarOpen,
  selectedModel = "gpt-4",
  onModelChange,
  enabledModels = {
    "gpt-4": true,
    "gpt-4-turbo": true,
    "gpt-3.5-turbo": true,
    "claude-3-opus": true,
    "claude-3-sonnet": true,
    "structured-gpt": true,
  }
}: ChatHeaderProps) {
  const { showcaseMode } = useDebug();
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [aboutOpen, setAboutOpen] = useState(false);
  const [helpOpen, setHelpOpen] = useState(false);
  const [moreMenuOpen, setMoreMenuOpen] = useState(false);
  const [modelSelectOpen, setModelSelectOpen] = useState(false);

  const models = [
    { id: "gpt-4", name: "GPT-4", color: "text-purple-500" },
    { id: "gpt-4-turbo", name: "GPT-4 Turbo", color: "text-blue-500" },
    { id: "gpt-3.5-turbo", name: "GPT-3.5 Turbo", color: "text-green-500" },
    { id: "claude-3-opus", name: "Claude 3 Opus", color: "text-orange-500" },
    { id: "claude-3-sonnet", name: "Claude 3 Sonnet", color: "text-amber-500" },
    { id: "structured-gpt", name: "Structured GPT", color: "text-cyan-500" },
  ];

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