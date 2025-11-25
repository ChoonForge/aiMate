import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Input } from "./ui/input";
import { Search } from "lucide-react";
import { ScrollArea } from "./ui/scroll-area";
import { Switch } from "./ui/switch";
import { Label } from "./ui/label";
import { Separator } from "./ui/separator";

interface SearchModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SearchModal({ open, onOpenChange }: SearchModalProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [filters, setFilters] = useState({
    includeChats: true,
    includeFiles: true,
    includeArtifacts: false,
    includeNotes: true,
  });

  const toggleFilter = (key: keyof typeof filters) => {
    setFilters((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col overflow-auto">
        <DialogHeader className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <DialogTitle className="flex items-center gap-2">
            <Search className="h-5 w-5" />
            Search
          </DialogTitle>
          <DialogDescription className="sr-only">
            Search across chats, files, artifacts, and notes
          </DialogDescription>
        </DialogHeader>

        {/* Search Input */}
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              placeholder="Search..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {/* Filter Toggles */}
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center justify-between">
              <Label htmlFor="include-chats">Include Chats</Label>
              <Switch
                id="include-chats"
                checked={filters.includeChats}
                onCheckedChange={() => toggleFilter("includeChats")}
                className="data-[state=checked]:bg-purple-600"
              />
            </div>
            <div className="flex items-center justify-between">
              <Label htmlFor="include-files">Include Files</Label>
              <Switch
                id="include-files"
                checked={filters.includeFiles}
                onCheckedChange={() => toggleFilter("includeFiles")}
                className="data-[state=checked]:bg-purple-600"
              />
            </div>
            <div className="flex items-center justify-between">
              <Label htmlFor="include-artifacts">Include Artifacts</Label>
              <Switch
                id="include-artifacts"
                checked={filters.includeArtifacts}
                onCheckedChange={() => toggleFilter("includeArtifacts")}
                className="data-[state=checked]:bg-purple-600"
              />
            </div>
            <div className="flex items-center justify-between">
              <Label htmlFor="include-notes">Include Notes</Label>
              <Switch
                id="include-notes"
                checked={filters.includeNotes}
                onCheckedChange={() => toggleFilter("includeNotes")}
                className="data-[state=checked]:bg-purple-600"
              />
            </div>
          </div>
        </div>

        {/* Search Results */}
        <ScrollArea className="flex-1">
          <div className="px-6 py-4">
            {searchQuery.trim() === "" ? (
              <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                Enter a search query to see results
              </div>
            ) : (
              <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                No results found for "{searchQuery}"
              </div>
            )}
          </div>
        </ScrollArea>
      </DialogContent>
    </Dialog>
  );
}
