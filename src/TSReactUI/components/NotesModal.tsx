import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { FileText, Plus, Pencil, Trash2 } from "lucide-react";
import { ScrollArea } from "./ui/scroll-area";
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
import { toast } from "sonner@2.0.3";
import { useDebug } from "./DebugContext";
import * as API from "../utils/api-stubs";

interface Note {
  id: string;
  title: string;
  content: string;
  date: string;
}

interface NotesModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function NotesModal({ open, onOpenChange }: NotesModalProps) {
  const { addLog } = useDebug();
  const [notes, setNotes] = useState<Note[]>([]);
  const [editingNote, setEditingNote] = useState<Note | null>(null);
  const [deleteNoteId, setDeleteNoteId] = useState<string | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [formData, setFormData] = useState({ title: "", content: "" });
  const [loading, setLoading] = useState(false);

  // Load notes when modal opens
  useEffect(() => {
    if (open && notes.length === 0) {
      loadNotes();
    }
  }, [open]);

  const loadNotes = async () => {
    setLoading(true);
    const response = await API.getNotes();
    if (response.success && response.data) {
      // Convert API format to component format
      const formattedNotes = response.data.map(note => ({
        id: note.id,
        title: note.title,
        content: note.content,
        date: note.date
      }));
      setNotes(formattedNotes);
      addLog({
        action: 'Loaded Notes',
        api: response.metadata?.endpoint || '/api/v1/GetNotes',
        payload: response.data,
        type: 'success'
      });
    }
    setLoading(false);
  };

  const handleCreateNote = () => {
    setFormData({ title: "", content: "" });
    setEditingNote(null);
    setIsCreating(true);
  };

  const handleEditNote = (note: Note) => {
    setFormData({ title: note.title, content: note.content });
    setEditingNote(note);
    setIsCreating(true);
  };

  const handleSaveNote = () => {
    if (!formData.title.trim()) {
      toast.error("Please add a title for your note");
      return;
    }
    
    if (!formData.content.trim()) {
      toast.error("Please add content to your note");
      return;
    }

    if (editingNote) {
      // Update existing note
      setNotes((prev) =>
        prev.map((note) =>
          note.id === editingNote.id
            ? { ...note, title: formData.title, content: formData.content, date: new Date().toLocaleDateString() }
            : note
        )
      );
      toast.success("Note saved!");
    } else {
      // Create new note
      const newNote: Note = {
        id: Date.now().toString(),
        title: formData.title,
        content: formData.content,
        date: new Date().toLocaleDateString(),
      };
      setNotes((prev) => [newNote, ...prev]);
      toast.success("Note created!");
    }

    setIsCreating(false);
    setFormData({ title: "", content: "" });
    setEditingNote(null);
  };

  const handleDeleteNote = () => {
    if (deleteNoteId) {
      setNotes((prev) => prev.filter((note) => note.id !== deleteNoteId));
      setDeleteNoteId(null);
    }
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-3xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col overflow-auto">
          <DialogHeader className="px-6 py-4 border-b border-gray-200 dark:border-gray-800">
            <div className="flex items-center justify-between pr-8">
              <DialogTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Notes
              </DialogTitle>
              <Button onClick={handleCreateNote} size="sm" className="gap-2">
                <Plus className="h-4 w-4" />
                New Note
              </Button>
            </div>
            <DialogDescription className="sr-only">
              Manage your notes
            </DialogDescription>
          </DialogHeader>

          {isCreating ? (
            <div className="flex-1 overflow-hidden flex flex-col">
              <div className="px-6 py-4 flex-1 overflow-y-auto space-y-4">
                <div>
                  <label className="text-sm font-medium mb-2 block">Title</label>
                  <Input
                    placeholder="Note title"
                    value={formData.title}
                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  />
                </div>
                <div className="flex-1">
                  <label className="text-sm font-medium mb-2 block">Content</label>
                  <Textarea
                    placeholder="Note content"
                    value={formData.content}
                    onChange={(e) => setFormData({ ...formData, content: e.target.value })}
                    className="min-h-[300px]"
                  />
                </div>
              </div>
              <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex gap-2 justify-end">
                <Button variant="outline" onClick={() => setIsCreating(false)}>
                  Cancel
                </Button>
                <Button onClick={handleSaveNote}>
                  {editingNote ? "Update" : "Create"} Note
                </Button>
              </div>
            </div>
          ) : (
            <ScrollArea className="flex-1">
              <div className="px-6 py-4 space-y-3">
                {notes.length === 0 ? (
                  <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                    No notes yet. Create your first note!
                  </div>
                ) : (
                  notes.map((note) => (
                    <div
                      key={note.id}
                      className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <h4 className="font-medium mb-1">{note.title}</h4>
                          <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-2 mb-2">
                            {note.content}
                          </p>
                          <span className="text-xs text-gray-500">{note.date}</span>
                        </div>
                        <div className="flex gap-1 ml-4">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => handleEditNote(note)}
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-red-600 hover:text-red-700 dark:text-red-400"
                            onClick={() => setDeleteNoteId(note.id)}
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
          )}
        </DialogContent>
      </Dialog>

      <AlertDialog open={!!deleteNoteId} onOpenChange={(open) => !open && setDeleteNoteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Note</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this note? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteNote} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}
