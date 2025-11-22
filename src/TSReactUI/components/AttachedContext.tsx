import { useState } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { 
  Brain, 
  FileText, 
  X, 
  ChevronDown, 
  ChevronUp, 
  Paperclip,
  MessageSquare,
  File,
  Globe
} from "lucide-react";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "./ui/collapsible";

interface AttachedContextProps {
  attachedKnowledge: string[];
  attachedNotes: string[];
  attachedFiles: string[];
  attachedChats: string[];
  attachedWebpages: string[];
  knowledgeItems: any[];
  noteItems: any[];
  fileItems: any[];
  chatItems: any[];
  webpageItems: any[];
  onDetachKnowledge: (id: string) => void;
  onDetachNote: (id: string) => void;
  onDetachFile: (id: string) => void;
  onDetachChat: (id: string) => void;
  onDetachWebpage: (id: string) => void;
}

interface AttachedItem {
  id: string;
  title: string;
  type: 'knowledge' | 'note' | 'file' | 'chat' | 'webpage';
  onDetach: () => void;
}

export function AttachedContext({ 
  attachedKnowledge,
  attachedNotes,
  attachedFiles,
  attachedChats,
  attachedWebpages,
  knowledgeItems,
  noteItems,
  fileItems,
  chatItems,
  webpageItems,
  onDetachKnowledge,
  onDetachNote,
  onDetachFile,
  onDetachChat,
  onDetachWebpage
}: AttachedContextProps) {
  const [isOpen, setIsOpen] = useState(true);

  const totalAttached = attachedKnowledge.length + attachedNotes.length + attachedFiles.length + attachedChats.length + attachedWebpages.length;

  if (totalAttached === 0) {
    return null;
  }

  const getItemDetails = (id: string, items: any[], defaultType: string) => {
    const item = items.find(i => i.id === id);
    return item || { id, title: `Item ${id}`, type: defaultType };
  };

  // Combine all attached items into one array
  const allAttachedItems: AttachedItem[] = [
    ...attachedKnowledge.map(id => ({
      id,
      title: getItemDetails(id, knowledgeItems, "Document").title,
      type: 'knowledge' as const,
      onDetach: () => onDetachKnowledge(id)
    })),
    ...attachedNotes.map(id => ({
      id,
      title: getItemDetails(id, noteItems, "Note").title,
      type: 'note' as const,
      onDetach: () => onDetachNote(id)
    })),
    ...attachedFiles.map(id => ({
      id,
      title: getItemDetails(id, fileItems, "File").title,
      type: 'file' as const,
      onDetach: () => onDetachFile(id)
    })),
    ...attachedWebpages.map(id => ({
      id,
      title: getItemDetails(id, webpageItems, "Webpage").title,
      type: 'webpage' as const,
      onDetach: () => onDetachWebpage(id)
    })),
    ...attachedChats.map(id => ({
      id,
      title: getItemDetails(id, chatItems, "Chat").title,
      type: 'chat' as const,
      onDetach: () => onDetachChat(id)
    }))
  ];

  const getTypeConfig = (type: 'knowledge' | 'note' | 'file' | 'chat' | 'webpage') => {
    switch (type) {
      case 'knowledge':
        return {
          icon: Brain,
          colorClass: 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-800',
          iconColor: 'text-purple-600 dark:text-purple-400'
        };
      case 'note':
        return {
          icon: FileText,
          colorClass: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-800',
          iconColor: 'text-blue-600 dark:text-blue-400'
        };
      case 'file':
        return {
          icon: File,
          colorClass: 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 border-green-200 dark:border-green-800',
          iconColor: 'text-green-600 dark:text-green-400'
        };
      case 'webpage':
        return {
          icon: Globe,
          colorClass: 'bg-cyan-100 dark:bg-cyan-900/30 text-cyan-700 dark:text-cyan-300 border-cyan-200 dark:border-cyan-800',
          iconColor: 'text-cyan-600 dark:text-cyan-400'
        };
      case 'chat':
        return {
          icon: MessageSquare,
          colorClass: 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 border-orange-200 dark:border-orange-800',
          iconColor: 'text-orange-600 dark:text-orange-400'
        };
    }
  };

  return (
    <div className="mb-3">
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
        <div className="flex items-center justify-between mb-2">
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="sm" className="gap-2 p-0 h-auto hover:bg-transparent">
              <Brain className="h-4 w-4 text-purple-500" />
              <span className="text-sm font-medium">
                Smart Context
              </span>
              {isOpen ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </Button>
          </CollapsibleTrigger>
          
          <span className="text-xs text-gray-500 dark:text-gray-400">
            {totalAttached} attached
          </span>
        </div>

        <CollapsibleContent>
          <div className="flex flex-wrap gap-2 p-3 bg-gray-50 dark:bg-gray-900/50 border border-gray-200 dark:border-gray-800 rounded-lg">
            {allAttachedItems.map((item) => {
              const config = getTypeConfig(item.type);
              const Icon = config.icon;
              
              return (
                <Badge 
                  key={`${item.type}-${item.id}`} 
                  variant="outline" 
                  className={`gap-1.5 pr-1 border ${config.colorClass}`}
                >
                  <Icon className={`h-3 w-3 ${config.iconColor}`} />
                  <span className="text-xs max-w-[200px] truncate">{item.title}</span>
                  <button
                    onClick={item.onDetach}
                    className="ml-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full p-0.5 transition-colors"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              );
            })}
          </div>
        </CollapsibleContent>
      </Collapsible>
    </div>
  );
}
