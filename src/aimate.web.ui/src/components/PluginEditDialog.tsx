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
import { Label } from "./ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { ScrollArea } from "./ui/scroll-area";
import { Switch } from "./ui/switch";
import { Separator } from "./ui/separator";
import { Plus, X } from "lucide-react";

interface Plugin {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  parameters?: Array<{
    name: string;
    type: string;
    value: string;
    required: boolean;
  }>;
}

interface PluginEditDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  plugin?: Plugin | null;
  onSave: (plugin: Plugin) => void;
  onDelete?: (id: string) => void;
}

export function PluginEditDialog({
  open,
  onOpenChange,
  plugin,
  onSave,
  onDelete,
}: PluginEditDialogProps) {
  const [formData, setFormData] = useState<Plugin>(
    plugin || {
      id: Date.now().toString(),
      name: "",
      description: "",
      enabled: true,
      parameters: [],
    }
  );

  const [parameters, setParameters] = useState<Array<{
    name: string;
    type: string;
    value: string;
    required: boolean;
  }>>(plugin?.parameters || []);

  useEffect(() => {
    if (plugin) {
      setFormData(plugin);
      setParameters(plugin.parameters || []);
    } else {
      setFormData({
        id: Date.now().toString(),
        name: "",
        description: "",
        enabled: true,
        parameters: [],
      });
      setParameters([]);
    }
  }, [plugin, open]);

  const handleSave = () => {
    onSave({ ...formData, parameters });
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (plugin && onDelete) {
      onDelete(plugin.id);
      onOpenChange(false);
    }
  };

  const addParameter = () => {
    setParameters([
      ...parameters,
      { name: "", type: "string", value: "", required: false },
    ]);
  };

  const removeParameter = (index: number) => {
    setParameters(parameters.filter((_, i) => i !== index));
  };

  const updateParameter = (index: number, field: string, value: any) => {
    setParameters(
      parameters.map((param, i) =>
        i === index ? { ...param, [field]: value } : param
      )
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col">
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <DialogTitle>
            {plugin ? "Edit Plugin" : "Add Plugin"}
          </DialogTitle>
          <DialogDescription className="sr-only">
            Configure plugin settings and parameters
          </DialogDescription>
        </DialogHeader>

        <ScrollArea className="flex-1 overflow-hidden">
          <div className="px-6 py-4 space-y-6">
            {/* Basic Info */}
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="plugin-name">Plugin Name</Label>
                <Input
                  id="plugin-name"
                  value={formData.name}
                  onChange={(e) =>
                    setFormData({ ...formData, name: e.target.value })
                  }
                  placeholder="e.g., Weather Forecast"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="plugin-description">Description</Label>
                <Textarea
                  id="plugin-description"
                  value={formData.description}
                  onChange={(e) =>
                    setFormData({ ...formData, description: e.target.value })
                  }
                  placeholder="Brief description of the plugin"
                  className="min-h-[80px]"
                />
              </div>

              <div className="flex items-center justify-between">
                <Label htmlFor="plugin-enabled">Enabled</Label>
                <Switch
                  id="plugin-enabled"
                  checked={formData.enabled}
                  onCheckedChange={(enabled) =>
                    setFormData({ ...formData, enabled })
                  }
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
            </div>

            <Separator />

            {/* Parameters */}
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <Label>Parameters</Label>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={addParameter}
                  className="cursor-pointer"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add Parameter
                </Button>
              </div>

              {parameters.length === 0 ? (
                <div className="p-4 border border-dashed border-gray-300 dark:border-gray-700 rounded-lg text-center text-sm text-gray-500 dark:text-gray-400">
                  No parameters defined. Click "Add Parameter" to add one.
                </div>
              ) : (
                <div className="space-y-3">
                  {parameters.map((param, index) => (
                    <div
                      key={index}
                      className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg space-y-3"
                    >
                      <div className="flex items-center justify-between">
                        <span className="text-sm font-medium">
                          Parameter {index + 1}
                        </span>
                        <Button
                          size="icon"
                          variant="ghost"
                          onClick={() => removeParameter(index)}
                          className="h-6 w-6 cursor-pointer"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>

                      <div className="grid grid-cols-2 gap-3">
                        <div className="space-y-2">
                          <Label>Name</Label>
                          <Input
                            value={param.name}
                            onChange={(e) =>
                              updateParameter(index, "name", e.target.value)
                            }
                            placeholder="parameter_name"
                          />
                        </div>

                        <div className="space-y-2">
                          <Label>Type</Label>
                          <Select
                            value={param.type}
                            onValueChange={(value) =>
                              updateParameter(index, "type", value)
                            }
                          >
                            <SelectTrigger>
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="string">String</SelectItem>
                              <SelectItem value="number">Number</SelectItem>
                              <SelectItem value="boolean">Boolean</SelectItem>
                              <SelectItem value="array">Array</SelectItem>
                              <SelectItem value="object">Object</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                      </div>

                      <div className="space-y-2">
                        <Label>Default Value</Label>
                        <Input
                          value={param.value}
                          onChange={(e) =>
                            updateParameter(index, "value", e.target.value)
                          }
                          placeholder="default value"
                        />
                      </div>

                      <div className="flex items-center gap-2">
                        <Switch
                          checked={param.required}
                          onCheckedChange={(checked) =>
                            updateParameter(index, "required", checked)
                          }
                          className="data-[state=checked]:bg-purple-600"
                        />
                        <Label>Required</Label>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </ScrollArea>

        <div className="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-between">
          {plugin && onDelete ? (
            <Button variant="destructive" onClick={handleDelete} className="cursor-pointer">
              Delete
            </Button>
          ) : (
            <div />
          )}
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => onOpenChange(false)} className="cursor-pointer">
              Cancel
            </Button>
            <Button onClick={handleSave} className="cursor-pointer">
              Save
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
