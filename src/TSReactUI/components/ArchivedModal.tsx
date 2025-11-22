import { useState, useMemo } from "react";
import { Button } from "./ui/button";
import { Archive, MessageSquare, FileText, FolderKanban, File, Trash2, RotateCcw } from "lucide-react";
import { BaseModal } from "./BaseModal";

interface ArchivedItem {
  id: string;
  title: string;
  date: string;
}

interface ArchivedModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ArchivedModal({ open, onOpenChange }: ArchivedModalProps) {
  const [archivedChats, setArchivedChats] = useState<ArchivedItem[]>([
    { id: "1", title: "Old conversation about React", date: "Oct 15, 2025" },
    { id: "2", title: "Discussion on TypeScript", date: "Oct 10, 2025" },
  ]);

  const [archivedFiles, setArchivedFiles] = useState<ArchivedItem[]>([
    { id: "1", title: "old-presentation.pdf", date: "Oct 20, 2025" },
  ]);

  const [archivedProjects, setArchivedProjects] = useState<ArchivedItem[]>([
    { id: "1", title: "Legacy Website Redesign", date: "Sep 30, 2025" },
  ]);

  const [archivedNotes, setArchivedNotes] = useState<ArchivedItem[]>([
    { id: "1", title: "Meeting notes from Q3", date: "Oct 5, 2025" },
  ]);

  const handleRestore = (type: string, id: string) => {
    // Logic to restore item based on type
    console.log(`Restoring ${type} with id ${id}`);
  };

  const handleDelete = (type: string, id: string) => {
    // Logic to permanently delete item
    switch (type) {
      case "chats":
        setArchivedChats((prev) => prev.filter((item) => item.id !== id));
        break;
      case "files":
        setArchivedFiles((prev) => prev.filter((item) => item.id !== id));
        break;
      case "projects":
        setArchivedProjects((prev) => prev.filter((item) => item.id !== id));
        break;
      case "notes":
        setArchivedNotes((prev) => prev.filter((item) => item.id !== id));
        break;
    }
  };

  const renderItems = (items: ArchivedItem[], type: string, iconClass: string, IconComponent: any) => (
    <div className="space-y-3">
      {items.length === 0 ? (
        <div className="text-center py-8 text-gray-500 dark:text-gray-400">
          No archived {type} yet.
        </div>
      ) : (
        items.map((item) => (
          <div
            key={item.id}
            className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
          >
            <div className="flex items-start justify-between">
              <div className="flex gap-3 flex-1">
                <IconComponent className={`h-5 w-5 ${iconClass} shrink-0 mt-1`} />
                <div>
                  <h4 className="font-medium mb-1">{item.title}</h4>
                  <span className="text-sm text-gray-600 dark:text-gray-400">
                    Archived on {item.date}
                  </span>
                </div>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-blue-600 hover:text-blue-700 dark:text-blue-400"
                  onClick={() => handleRestore(type, item.id)}
                  title="Restore"
                >
                  <RotateCcw className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-red-600 hover:text-red-700 dark:text-red-400"
                  onClick={() => handleDelete(type, item.id)}
                  title="Delete Permanently"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  );

  const tabs = useMemo(() => [
    {
      id: "chats",
      label: "Chats",
      icon: MessageSquare,
      content: renderItems(archivedChats, "chats", "text-blue-500", MessageSquare),
    },
    {
      id: "files",
      label: "Files",
      icon: File,
      content: renderItems(archivedFiles, "files", "text-green-500", File),
    },
    {
      id: "projects",
      label: "Projects",
      icon: FolderKanban,
      content: renderItems(archivedProjects, "projects", "text-purple-500", FolderKanban),
    },
    {
      id: "notes",
      label: "Notes",
      icon: FileText,
      content: renderItems(archivedNotes, "notes", "text-orange-500", FileText),
    },
  ], [archivedChats, archivedFiles, archivedProjects, archivedNotes]);

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title="Archived Items"
      description="View and manage archived items"
      icon={Archive}
      size="lg"
      showSave={false}
      showDelete={false}
      tabs={tabs}
    />
  );
}
