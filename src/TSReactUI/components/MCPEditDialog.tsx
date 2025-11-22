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
import { X, RefreshCw, Eye, EyeOff, Lock } from "lucide-react";
import { ScrollArea } from "./ui/scroll-area";
import { Separator } from "./ui/separator";

interface MCPConnector {
  id: string;
  name: string;
  type: string;
  url: string;
  auth: string;
  authToken: string;
  mcpId: string;
  description: string;
  visibility: "private" | "public";
  groups: string[];
  enabled: boolean;
}

interface MCPEditDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  connector?: MCPConnector | null;
  onSave: (connector: MCPConnector) => void;
  onDelete?: (id: string) => void;
}

export function MCPEditDialog({
  open,
  onOpenChange,
  connector,
  onSave,
  onDelete,
}: MCPEditDialogProps) {
  const [formData, setFormData] = useState<MCPConnector>(
    connector || {
      id: Date.now().toString(),
      name: "",
      type: "MCP Streamable HTTP",
      url: "",
      auth: "Bearer",
      authToken: "",
      mcpId: "",
      description: "",
      visibility: "private",
      groups: [],
      enabled: true,
    }
  );

  const [showAuthToken, setShowAuthToken] = useState(false);
  const [showImportExport, setShowImportExport] = useState(false);

  const handleSave = () => {
    onSave(formData);
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (connector && onDelete) {
      onDelete(connector.id);
      onOpenChange(false);
    }
  };

  const handleImport = () => {
    // TODO: Implement import functionality
    console.log("Import MCP connector");
  };

  const handleExport = () => {
    // TODO: Implement export functionality
    console.log("Export MCP connector");
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col bg-gray-900">
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-800 flex flex-row items-center justify-between">
          <DialogTitle>
            {connector ? "Edit Connection" : "Add Connection"}
          </DialogTitle>
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={handleImport}
              className="text-gray-400 hover:text-gray-200 cursor-pointer"
            >
              Import
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={handleExport}
              className="text-gray-400 hover:text-gray-200 cursor-pointer"
            >
              Export
            </Button>
          </div>
          <DialogDescription className="sr-only">
            Configure MCP connector settings
          </DialogDescription>
        </DialogHeader>

        <ScrollArea className="flex-1 overflow-hidden">
          <div className="px-6 py-4 space-y-4">
            <div className="flex items-center justify-between">
              <Label htmlFor="connection-type" className="text-gray-400">Type</Label>
              <span className="text-sm text-gray-300">
                {formData.type}
              </span>
            </div>

            <div className="space-y-2">
              <Label htmlFor="url" className="text-gray-300">URL</Label>
              <div className="flex gap-2">
                <Input
                  id="url"
                  value={formData.url}
                  onChange={(e) =>
                    setFormData({ ...formData, url: e.target.value })
                  }
                  placeholder="https://echo.firebird.co.nz"
                  className="flex-1 bg-gray-800 border-gray-700 text-gray-100"
                />
                <Button size="icon" variant="outline" className="border-gray-700 hover:bg-gray-800 cursor-pointer">
                  <RefreshCw className="h-4 w-4" />
                </Button>
                <Switch
                  checked={formData.enabled}
                  onCheckedChange={(enabled) =>
                    setFormData({ ...formData, enabled })
                  }
                  className="data-[state=checked]:bg-green-600"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="auth" className="text-gray-300">Auth</Label>
              <Select
                value={formData.auth}
                onValueChange={(auth) => setFormData({ ...formData, auth })}
              >
                <SelectTrigger className="bg-gray-800 border-gray-700 text-gray-100">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-gray-800 border-gray-700">
                  <SelectItem value="None">None</SelectItem>
                  <SelectItem value="Bearer">Bearer</SelectItem>
                  <SelectItem value="API Key">API Key</SelectItem>
                  <SelectItem value="OAuth">OAuth</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {formData.auth !== "None" && (
              <div className="space-y-2">
                <Label htmlFor="auth-token" className="text-gray-300">Auth Token</Label>
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <Input
                      id="auth-token"
                      type={showAuthToken ? "text" : "password"}
                      value={formData.authToken}
                      onChange={(e) =>
                        setFormData({ ...formData, authToken: e.target.value })
                      }
                      placeholder="••••••••••••••••••••••••••••••••"
                      className="bg-gray-800 border-gray-700 text-gray-100 pr-10"
                    />
                  </div>
                  <Button
                    size="icon"
                    variant="outline"
                    onClick={() => setShowAuthToken(!showAuthToken)}
                    className="border-gray-700 hover:bg-gray-800 cursor-pointer"
                  >
                    {showAuthToken ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>
            )}

            <Separator className="bg-gray-800" />

            <div className="space-y-2">
              <Label htmlFor="mcp-id" className="text-gray-300">ID</Label>
              <Input
                id="mcp-id"
                value={formData.mcpId}
                onChange={(e) =>
                  setFormData({ ...formData, mcpId: e.target.value })
                }
                placeholder="EchoMCP"
                className="bg-gray-800 border-gray-700 text-gray-100"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="name" className="text-gray-300">Name</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
                placeholder="EchoMCP"
                className="bg-gray-800 border-gray-700 text-gray-100"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description" className="text-gray-300">Description</Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                placeholder="Echo MCP Connector"
                className="bg-gray-800 border-gray-700 text-gray-100"
              />
            </div>

            <Separator className="bg-gray-800" />

            <div className="space-y-3">
              <h4 className="font-medium text-gray-200">Visibility</h4>
              <div className="p-4 bg-gray-800 rounded-lg border border-gray-700">
                <div className="flex items-start gap-3">
                  <Lock className="h-5 w-5 text-gray-400 mt-0.5" />
                  <div className="flex-1">
                    <Select
                      value={formData.visibility}
                      onValueChange={(visibility: "private" | "public") =>
                        setFormData({ ...formData, visibility })
                      }
                    >
                      <SelectTrigger className="bg-gray-900 border-gray-700 text-gray-100">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent className="bg-gray-800 border-gray-700">
                        <SelectItem value="private">Private</SelectItem>
                        <SelectItem value="public">Public</SelectItem>
                      </SelectContent>
                    </Select>
                    <p className="text-sm text-gray-400 mt-2">
                      Only select users and groups with permission can access
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <h4 className="font-medium text-gray-200">Groups</h4>
              <Select>
                <SelectTrigger className="bg-gray-800 border-gray-700 text-gray-400">
                  <SelectValue placeholder="Select a group" />
                </SelectTrigger>
                <SelectContent className="bg-gray-800 border-gray-700">
                  <SelectItem value="group1">Group 1</SelectItem>
                  <SelectItem value="group2">Group 2</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-sm text-gray-400 px-3">
                No groups with access, add a group to grant access
              </p>
            </div>

            <Separator className="bg-gray-800" />

            <div className="p-4 bg-yellow-900/20 border border-yellow-700/50 rounded-lg">
              <p className="text-sm text-yellow-200">
                <strong>Warning:</strong> MCP support is experimental and its specification changes
                often, which can lead to incompatibilities. OpenAPI specification support
                is directly maintained by the Open WebUI team, making it the more
                reliable option for compatibility.{" "}
                <button className="text-yellow-400 underline hover:text-yellow-300 cursor-pointer">
                  Read more →
                </button>
              </p>
            </div>
          </div>
        </ScrollArea>

        <div className="flex-shrink-0 px-6 py-4 border-t border-gray-800 flex justify-between">
          {connector && onDelete ? (
            <Button 
              variant="destructive" 
              onClick={handleDelete}
              className="bg-red-900/20 text-red-400 hover:bg-red-900/30 border border-red-800 cursor-pointer"
            >
              Delete
            </Button>
          ) : (
            <div />
          )}
          <div className="flex gap-2">
            <Button onClick={handleSave} className="bg-white text-gray-900 hover:bg-gray-200 cursor-pointer">
              Save
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
