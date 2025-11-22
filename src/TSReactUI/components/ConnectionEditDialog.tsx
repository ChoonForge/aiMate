import { useState } from "react";
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
import { Label } from "./ui/label";
import { Switch } from "./ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { X, RefreshCw, Plus } from "lucide-react";
import { ScrollArea } from "./ui/scroll-area";
import { Separator } from "./ui/separator";

interface Connection {
  id: string;
  name: string;
  type: string;
  url: string;
  auth: string;
  headers: string;
  prefixId: string;
  providerType: string;
  modelIds: string[];
  tags: string[];
  enabled: boolean;
}

interface ConnectionEditDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  connection?: Connection | null;
  onSave: (connection: Connection) => void;
  onDelete?: (id: string) => void;
}

export function ConnectionEditDialog({
  open,
  onOpenChange,
  connection,
  onSave,
  onDelete,
}: ConnectionEditDialogProps) {
  const [formData, setFormData] = useState<Connection>(
    connection || {
      id: Date.now().toString(),
      name: "",
      type: "Local",
      url: "",
      auth: "None",
      headers: "",
      prefixId: "",
      providerType: "OpenAI",
      modelIds: [],
      tags: [],
      enabled: true,
    },
  );

  const [newModelId, setNewModelId] = useState("");
  const [newTag, setNewTag] = useState("");

  const handleSave = () => {
    onSave(formData);
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (connection && onDelete) {
      onDelete(connection.id);
      onOpenChange(false);
    }
  };

  const addModelId = () => {
    if (newModelId.trim()) {
      setFormData({
        ...formData,
        modelIds: [...formData.modelIds, newModelId.trim()],
      });
      setNewModelId("");
    }
  };

  const removeModelId = (index: number) => {
    setFormData({
      ...formData,
      modelIds: formData.modelIds.filter((_, i) => i !== index),
    });
  };

  const addTag = () => {
    if (newTag.trim()) {
      setFormData({
        ...formData,
        tags: [...formData.tags, newTag.trim()],
      });
      setNewTag("");
    }
  };

  const removeTag = (index: number) => {
    setFormData({
      ...formData,
      tags: formData.tags.filter((_, i) => i !== index),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0 gap-0 flex flex-col">
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex flex-row items-center justify-between">
          <DialogTitle>
            {connection ? "Edit Connection" : "Add Connection"}
          </DialogTitle>
          <DialogDescription className="sr-only">
            Configure API connection settings
          </DialogDescription>
        </DialogHeader>

        <ScrollArea className="flex-1 overflow-hidden">
          <div className="px-6 py-4 space-y-4">
            <div className="flex items-center justify-between">
              <Label htmlFor="connection-type">
                Connection Type
              </Label>
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {formData.type}
              </span>
            </div>

            <div className="space-y-2">
              <Label htmlFor="url">URL</Label>
              <div className="flex gap-2">
                <Input
                  id="url"
                  value={formData.url}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      url: e.target.value,
                    })
                  }
                  placeholder="https://chat.firebird.co.nz/lmstudio/v1"
                  className="flex-1"
                />
                <Button size="icon" variant="outline">
                  <RefreshCw className="h-4 w-4" />
                </Button>
                <Switch
                  checked={formData.enabled}
                  onCheckedChange={(enabled) =>
                    setFormData({ ...formData, enabled })
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="auth">Auth</Label>
              <Select
                value={formData.auth}
                onValueChange={(auth) =>
                  setFormData({ ...formData, auth })
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="None">None</SelectItem>
                  <SelectItem value="API Key">
                    API Key
                  </SelectItem>
                  <SelectItem value="OAuth">OAuth</SelectItem>
                  <SelectItem value="Bearer Token">
                    Bearer Token
                  </SelectItem>
                </SelectContent>
              </Select>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                No authentication
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="headers">Headers</Label>
              <Textarea
                id="headers"
                value={formData.headers}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    headers: e.target.value,
                  })
                }
                placeholder="Enter additional headers in JSON format"
                className="min-h-[80px] font-mono text-sm"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="prefix-id">Prefix ID</Label>
              <Input
                id="prefix-id"
                value={formData.prefixId}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    prefixId: e.target.value,
                  })
                }
                placeholder="Prefix ID"
              />
            </div>

            <div className="flex items-center justify-between">
              <Label htmlFor="provider-type">
                Provider Type
              </Label>
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {formData.providerType}
              </span>
            </div>

            <Separator />

            <div className="space-y-2">
              <Label>Model IDs</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                Leave empty to include all models from
                "https://chat.firebird.co.nz/lmstudio/v1/models"
                endpoint
              </p>

              {formData.modelIds.length > 0 && (
                <div className="space-y-2">
                  {formData.modelIds.map((modelId, index) => (
                    <div
                      key={index}
                      className="flex items-center gap-2 p-2 bg-gray-100 dark:bg-gray-800 rounded"
                    >
                      <span className="flex-1 text-sm">
                        {modelId}
                      </span>
                      <Button
                        size="icon"
                        variant="ghost"
                        className="h-6 w-6"
                        onClick={() => removeModelId(index)}
                      >
                        <X className="h-3 w-3" />
                      </Button>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex gap-2">
                <Input
                  value={newModelId}
                  onChange={(e) =>
                    setNewModelId(e.target.value)
                  }
                  placeholder="Add a model ID"
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      addModelId();
                    }
                  }}
                />
                <Button
                  size="icon"
                  variant="outline"
                  onClick={addModelId}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
            </div>

            <Separator />

            <div className="space-y-2">
              <Label>Tags</Label>
              {formData.tags.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {formData.tags.map((tag, index) => (
                    <div
                      key={index}
                      className="flex items-center gap-1 px-2 py-1 bg-gray-100 dark:bg-gray-800 rounded text-sm"
                    >
                      <span>{tag}</span>
                      <button
                        onClick={() => removeTag(index)}
                        className="hover:text-red-500"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex gap-2">
                <Input
                  value={newTag}
                  onChange={(e) => setNewTag(e.target.value)}
                  placeholder="Add Tags"
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      addTag();
                    }
                  }}
                />
                <Button
                  size="icon"
                  variant="outline"
                  onClick={addTag}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        </ScrollArea>

        <div className="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-between">
          {connection && onDelete ? (
            <Button
              variant="destructive"
              onClick={handleDelete}
            >
              Delete
            </Button>
          ) : (
            <div />
          )}
          <div className="flex gap-2">
            <Button
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button onClick={handleSave}>Save</Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}