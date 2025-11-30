/**
 * Memories Panel
 *
 * UI for viewing and managing persistent user memories
 */

import { useState } from "react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Badge } from "./ui/badge";
import { ScrollArea } from "./ui/scroll-area";
import { Separator } from "./ui/separator";
import {
  Brain,
  Plus,
  Trash2,
  Search,
  Tag,
  Calendar,
  AlertCircle,
  CheckCircle2,
  X,
} from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "./ui/alert-dialog";
import { useMemories, Memory } from "../hooks/useMemories";
import { toast } from "sonner";

const CATEGORY_LABELS: Record<Memory['category'], string> = {
  preference: 'Preference',
  fact: 'Fact',
  context: 'Context',
  custom: 'Custom',
};

const CATEGORY_COLORS: Record<Memory['category'], string> = {
  preference: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  fact: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  context: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
  custom: 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200',
};

export function MemoriesPanel() {
  const {
    memories,
    loading,
    addMemory,
    deleteMemory,
    clearAllMemories,
    searchMemories,
  } = useMemories();

  const [searchQuery, setSearchQuery] = useState('');
  const [newMemory, setNewMemory] = useState('');
  const [newCategory, setNewCategory] = useState<Memory['category']>('fact');
  const [showAddForm, setShowAddForm] = useState(false);

  const filteredMemories = searchQuery
    ? searchMemories(searchQuery)
    : memories;

  const handleAddMemory = () => {
    if (!newMemory.trim()) return;

    const memory = addMemory(newMemory.trim(), newCategory);
    if (memory) {
      toast.success('Memory added');
      setNewMemory('');
      setShowAddForm(false);
    } else {
      toast.error('Memory already exists');
    }
  };

  const handleDeleteMemory = (id: string) => {
    deleteMemory(id);
    toast.success('Memory deleted');
  };

  const handleClearAll = () => {
    clearAllMemories();
    toast.success('All memories cleared');
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-NZ', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Brain className="h-5 w-5 text-purple-600 dark:text-purple-400" />
          <h3 className="text-lg font-semibold">Memories</h3>
          <Badge variant="secondary">{memories.length}</Badge>
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowAddForm(!showAddForm)}
          >
            <Plus className="h-4 w-4 mr-1" />
            Add
          </Button>

          {memories.length > 0 && (
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button variant="outline" size="sm" className="text-red-600">
                  <Trash2 className="h-4 w-4 mr-1" />
                  Clear All
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Clear all memories?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This will permanently delete all {memories.length} memories.
                    This action cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={handleClearAll}>
                    Clear All
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
        </div>
      </div>

      {/* Add Memory Form */}
      {showAddForm && (
        <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg space-y-3">
          <div className="flex gap-2">
            <Input
              placeholder="Enter a memory (e.g., 'User prefers dark mode')"
              value={newMemory}
              onChange={(e) => setNewMemory(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleAddMemory()}
              className="flex-1"
            />
            <Select value={newCategory} onValueChange={(v) => setNewCategory(v as Memory['category'])}>
              <SelectTrigger className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="fact">Fact</SelectItem>
                <SelectItem value="preference">Preference</SelectItem>
                <SelectItem value="context">Context</SelectItem>
                <SelectItem value="custom">Custom</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" size="sm" onClick={() => setShowAddForm(false)}>
              Cancel
            </Button>
            <Button size="sm" onClick={handleAddMemory} disabled={!newMemory.trim()}>
              <CheckCircle2 className="h-4 w-4 mr-1" />
              Add Memory
            </Button>
          </div>
        </div>
      )}

      {/* Search */}
      {memories.length > 5 && (
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-500" />
          <Input
            placeholder="Search memories..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
      )}

      {/* Memories List */}
      {loading ? (
        <div className="text-center py-8 text-gray-500">
          Loading memories...
        </div>
      ) : filteredMemories.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          {searchQuery ? (
            <p>No memories matching "{searchQuery}"</p>
          ) : (
            <div className="space-y-2">
              <Brain className="h-12 w-12 mx-auto text-gray-400" />
              <p>No memories yet</p>
              <p className="text-sm">
                Memories are automatically extracted from your conversations,
                or you can add them manually.
              </p>
            </div>
          )}
        </div>
      ) : (
        <ScrollArea className="h-[400px]">
          <div className="space-y-2 pr-4">
            {filteredMemories.map((memory) => (
              <div
                key={memory.id}
                className="p-3 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors group"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm">{memory.content}</p>
                    <div className="flex items-center gap-2 mt-2">
                      <Badge className={CATEGORY_COLORS[memory.category]}>
                        <Tag className="h-3 w-3 mr-1" />
                        {CATEGORY_LABELS[memory.category]}
                      </Badge>
                      <span className="text-xs text-gray-500 flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        {formatDate(memory.createdAt)}
                      </span>
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="opacity-0 group-hover:opacity-100 transition-opacity h-8 w-8 text-red-600 hover:text-red-700 hover:bg-red-50 dark:hover:bg-red-900/20"
                    onClick={() => handleDeleteMemory(memory.id)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </ScrollArea>
      )}

      {/* Info Footer */}
      <Separator />
      <div className="flex items-start gap-2 text-xs text-gray-500">
        <AlertCircle className="h-4 w-4 shrink-0 mt-0.5" />
        <p>
          Memories are stored locally in your browser. They help personalize
          your conversations by remembering facts and preferences you've shared.
        </p>
      </div>
    </div>
  );
}
