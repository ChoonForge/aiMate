import { useState, useMemo } from "react";
import { useDebug } from "./DebugContext";
import { useAdminSettings } from "../context/AdminSettingsContext";
import { ConnectionEditDialog } from "./ConnectionEditDialog";
import { ModelEditDialog } from "./ModelEditDialog";
import { MCPEditDialog } from "./MCPEditDialog";
import { PluginEditDialog } from "./PluginEditDialog";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { ShieldCheck, Users, Database, FileText, BarChart3, Pencil, Sparkles, Search, Code, Wrench, CloudSun, Plus, Settings, Layers, Download, X, Upload, Eye, Trash2, Edit2 } from "lucide-react";
import { ScrollArea } from "./ui/scroll-area";
import { Switch } from "./ui/switch";
import { Label } from "./ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Separator } from "./ui/separator";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { Slider } from "./ui/slider";
import { BaseModal } from "./BaseModal";

interface AdminModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  enabledModels?: Record<string, boolean>;
  onToggleModel?: (modelId: string) => void;
}

export function AdminModal({ open, onOpenChange, enabledModels, onToggleModel }: AdminModalProps) {
  const tabs = useMemo(() => [
    {
      id: "general",
      label: "General",
      icon: ShieldCheck,
      content: <GeneralTab />,
    },
    {
      id: "interface",
      label: "Interface",
      icon: Layers,
      content: <InterfaceTab />,
    },
    {
      id: "users",
      label: "Users & Groups",
      icon: Users,
      content: <UsersGroupsTab />,
    },
    {
      id: "connections",
      label: "Connections",
      icon: Database,
      content: <ConnectionsTab />,
    },
    {
      id: "models",
      label: "Models",
      icon: BarChart3,
      content: <ModelsTab enabledModels={enabledModels} onToggleModel={onToggleModel} />,
    },
    {
      id: "plugins",
      label: "Plugins",
      icon: FileText,
      content: <PluginsTab />,
    },
    {
      id: "mcp",
      label: "MCP",
      icon: Database,
      content: <MCPTab />,
    },
    {
      id: "documents",
      label: "Documents",
      icon: FileText,
      content: <DocumentsTab />,
    },
    {
      id: "websearch",
      label: "Web Search",
      icon: Search,
      content: <WebSearchTab />,
    },
    {
      id: "codeexecution",
      label: "Code Execution",
      icon: Code,
      content: <CodeExecutionTab />,
    },
    {
      id: "audio",
      label: "Audio",
      icon: CloudSun,
      content: <AudioTab />,
    },
    {
      id: "images",
      label: "Images",
      icon: Sparkles,
      content: <ImagesTab />,
    },
  ], [enabledModels, onToggleModel]);

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title="Admin Panel"
      description="Administrator settings and configurations"
      icon={ShieldCheck}
      size="lg"
      showSave={false}
      showDelete={false}
      tabs={tabs}
    />
  );
}

function GeneralTab() {
  const { debugEnabled, setDebugEnabled } = useDebug();
  const { settings, updateGeneral } = useAdminSettings();

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">General Settings</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label htmlFor="admin-enabled">Enable Admin Panel</Label>
            <Switch
              id="admin-enabled"
              checked={settings.general.adminEnabled}
              onCheckedChange={(checked) => updateGeneral({ adminEnabled: checked })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="flex items-center justify-between">
            <Label htmlFor="user-registration">Allow User Registration</Label>
            <Switch
              id="user-registration"
              checked={settings.general.userRegistration}
              onCheckedChange={(checked) => updateGeneral({ userRegistration: checked })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="flex items-center justify-between">
            <Label htmlFor="api-access">Enable API Access</Label>
            <Switch
              id="api-access"
              checked={settings.general.apiAccess}
              onCheckedChange={(checked) => updateGeneral({ apiAccess: checked })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Developer Settings</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="debug-mode">Debug Mode</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Display debug console with API calls and payloads
              </p>
            </div>
            <Switch
              id="debug-mode"
              checked={debugEnabled}
              onCheckedChange={setDebugEnabled}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function InterfaceTab() {
  const { settings, updateInterface } = useAdminSettings();
  const iface = settings.interface;

  const addBanner = () => {
    updateInterface({
      banners: [...iface.banners, { id: Date.now().toString(), type: "Type", text: "Test", enabled: true }]
    });
  };

  const removeBanner = (id: string) => {
    updateInterface({
      banners: iface.banners.filter(b => b.id !== id)
    });
  };

  const updateBanner = (id: string, updates: Partial<typeof iface.banners[0]>) => {
    updateInterface({
      banners: iface.banners.map(b => b.id === id ? { ...b, ...updates } : b)
    });
  };

  const addPromptSuggestion = () => {
    updateInterface({
      promptSuggestions: [...iface.promptSuggestions, {
        id: Date.now().toString(),
        title: "Title (e.g. Tell me a fun fact)",
        subtitle: "Subtitle (e.g. about the Roman Empire)",
        prompt: "Prompt (e.g. Tell me a fun fact about the Roman Empire)"
      }]
    });
  };

  const removePromptSuggestion = (id: string) => {
    updateInterface({
      promptSuggestions: iface.promptSuggestions.filter(p => p.id !== id)
    });
  };

  const updatePromptSuggestion = (id: string, updates: Partial<typeof iface.promptSuggestions[0]>) => {
    updateInterface({
      promptSuggestions: iface.promptSuggestions.map(p => p.id === id ? { ...p, ...updates } : p)
    });
  };

  return (
    <div className="space-y-6">
      {/* Task Model */}
      <div>
        <div className="flex items-center gap-2 mb-3">
          <h3 className="font-semibold">Task Model</h3>
          <button className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <circle cx="12" cy="12" r="10" strokeWidth="2" />
              <text x="12" y="16" textAnchor="middle" className="text-xs" fill="currentColor">?</text>
            </svg>
          </button>
        </div>

        <div className="grid grid-cols-2 gap-4 mb-4">
          <div className="space-y-2">
            <Label>Local Task Model</Label>
            <Select value={iface.localTaskModel} onValueChange={(v) => updateInterface({ localTaskModel: v })}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Current Model">Current Model</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>External Task Model</Label>
            <Select value={iface.externalTaskModel} onValueChange={(v) => updateInterface({ externalTaskModel: v })}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Current Model">Current Model</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      <Separator />

      {/* Title Generation */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold">Title Generation</h3>
          <Switch
            checked={iface.titleGeneration}
            onCheckedChange={(v) => updateInterface({ titleGeneration: v })}
            className="data-[state=checked]:bg-purple-600"
          />
        </div>
        <div className="space-y-2">
          <Label>Title Generation Prompt</Label>
          <Textarea
            value={iface.titleGenerationPrompt}
            onChange={(e) => updateInterface({ titleGenerationPrompt: e.target.value })}
            className="min-h-[60px]"
            placeholder="Leave empty to use the default prompt, or enter a custom prompt"
          />
        </div>
      </div>

      {/* Toggle Sections */}
      <div className="flex items-center justify-between">
        <Label>Follow Up Generation</Label>
        <Switch
          checked={iface.followUpGeneration}
          onCheckedChange={(v) => updateInterface({ followUpGeneration: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <div className="flex items-center justify-between">
        <Label>Tags Generation</Label>
        <Switch
          checked={iface.tagsGeneration}
          onCheckedChange={(v) => updateInterface({ tagsGeneration: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <div className="flex items-center justify-between">
        <Label>Retrieval Query Generation</Label>
        <Switch
          checked={iface.retrievalQueryGeneration}
          onCheckedChange={(v) => updateInterface({ retrievalQueryGeneration: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <div className="flex items-center justify-between">
        <Label>Web Search Query Generation</Label>
        <Switch
          checked={iface.webSearchQueryGeneration}
          onCheckedChange={(v) => updateInterface({ webSearchQueryGeneration: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      {/* Query Generation Prompt */}
      <div className="space-y-2">
        <Label>Query Generation Prompt</Label>
        <Textarea
          value={iface.queryGenerationPrompt}
          onChange={(e) => updateInterface({ queryGenerationPrompt: e.target.value })}
          className="min-h-[60px]"
          placeholder="Leave empty to use the default prompt, or enter a custom prompt"
        />
      </div>

      <div className="flex items-center justify-between">
        <Label>Autocomplete Generation</Label>
        <Switch
          checked={iface.autocompleteGeneration}
          onCheckedChange={(v) => updateInterface({ autocompleteGeneration: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      {/* Image Prompt Generation Prompt */}
      <div className="space-y-2">
        <Label>Image Prompt Generation Prompt</Label>
        <Textarea
          value={iface.imagePromptGenerationPrompt}
          onChange={(e) => updateInterface({ imagePromptGenerationPrompt: e.target.value })}
          className="min-h-[60px]"
          placeholder="Leave empty to use the default prompt, or enter a custom prompt"
        />
      </div>

      {/* Tools Function Calling Prompt */}
      <div className="space-y-2">
        <Label>Tools Function Calling Prompt</Label>
        <Textarea
          value={iface.toolsFunctionCallingPrompt}
          onChange={(e) => updateInterface({ toolsFunctionCallingPrompt: e.target.value })}
          className="min-h-[60px]"
          placeholder="Leave empty to use the default prompt, or enter a custom prompt"
        />
      </div>

      <Separator />

      {/* UI Section */}
      <div>
        <h3 className="font-semibold mb-4">UI</h3>

        {/* Banners */}
        <div className="space-y-3 mb-4">
          <div className="flex items-center justify-between">
            <Label>Banners</Label>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 cursor-pointer"
              onClick={addBanner}
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>

          {iface.banners.map((banner) => (
            <div key={banner.id} className="flex gap-2 items-center p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
              <div className="flex gap-2 flex-1">
                <Select value={banner.type} onValueChange={(v) => updateBanner(banner.id, { type: v })}>
                  <SelectTrigger className="w-24">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Type">Type</SelectItem>
                    <SelectItem value="Info">Info</SelectItem>
                    <SelectItem value="Warning">Warning</SelectItem>
                  </SelectContent>
                </Select>
                <Input
                  value={banner.text}
                  onChange={(e) => updateBanner(banner.id, { text: e.target.value })}
                  className="flex-1"
                />
              </div>
              <div className="flex gap-1">
                <Switch
                  checked={banner.enabled}
                  onCheckedChange={(v) => updateBanner(banner.id, { enabled: v })}
                  className="data-[state=checked]:bg-purple-600"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 cursor-pointer"
                  onClick={() => removeBanner(banner.id)}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
        </div>

        {/* Default Prompt Suggestions */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <Label>Default Prompt Suggestions</Label>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 cursor-pointer"
              onClick={addPromptSuggestion}
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>

          {iface.promptSuggestions.map((suggestion) => (
            <div key={suggestion.id} className="space-y-2 p-3 bg-gray-100 dark:bg-gray-800 rounded-lg relative">
              <Button
                variant="ghost"
                size="icon"
                className="h-6 w-6 absolute top-2 right-2 cursor-pointer"
                onClick={() => removePromptSuggestion(suggestion.id)}
              >
                <X className="h-3 w-3" />
              </Button>
              <Input
                value={suggestion.title}
                onChange={(e) => updatePromptSuggestion(suggestion.id, { title: e.target.value })}
                className="text-sm"
              />
              <Input
                value={suggestion.subtitle}
                onChange={(e) => updatePromptSuggestion(suggestion.id, { subtitle: e.target.value })}
                className="text-xs text-gray-500 dark:text-gray-400"
              />
              <Textarea
                value={suggestion.prompt}
                onChange={(e) => updatePromptSuggestion(suggestion.id, { prompt: e.target.value })}
                className="min-h-[60px] text-sm"
              />
            </div>
          ))}
        </div>
      </div>

      <Separator />

      {/* Note */}
      <p className="text-sm text-gray-500 dark:text-gray-400">
        Adjusting these settings will apply changes universally to all users.
      </p>

      {/* Import/Export Buttons */}
      <div className="flex gap-2">
        <Button variant="outline" className="cursor-pointer">
          <Upload className="h-4 w-4 mr-2" />
          Import Prompt Suggestions
        </Button>
        <Button variant="outline" className="cursor-pointer">
          <Download className="h-4 w-4 mr-2" />
          Export Prompt Suggestions ({iface.promptSuggestions.length})
        </Button>
      </div>
    </div>
  );
}

function UsersGroupsTab() {
  const [view, setView] = useState<"users" | "groups" | "roles">("users");
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editType, setEditType] = useState<"user" | "group" | "role">("user");
  const [selectedItem, setSelectedItem] = useState<any>(null);

  const [users, setUsers] = useState([
    { id: "1", name: "Rich", email: "rich@example.com", role: "Admin", group: "Admins" },
    { id: "2", name: "John Doe", email: "john@example.com", role: "User", group: "Users" },
    { id: "3", name: "Jane Smith", email: "jane@example.com", role: "User", group: "Power Users" },
  ]);

  const [groups, setGroups] = useState([
    { 
      id: "1", 
      name: "Admins", 
      users: 1,
      maxMessages: "Unlimited",
      canAddTools: true,
      canCreateProjects: true,
      canUseWebSearch: true,
      canUseCodeInterpreter: true,
    },
    { 
      id: "2", 
      name: "Power Users", 
      users: 1,
      maxMessages: "500/day",
      canAddTools: true,
      canCreateProjects: true,
      canUseWebSearch: true,
      canUseCodeInterpreter: false,
    },
    { 
      id: "3", 
      name: "Users", 
      users: 1,
      maxMessages: "100/day",
      canAddTools: false,
      canCreateProjects: false,
      canUseWebSearch: true,
      canUseCodeInterpreter: false,
    },
  ]);

  const handleEdit = (item: any, type: "user" | "group") => {
    setSelectedItem(item);
    setEditType(type);
    setEditDialogOpen(true);
  };

  const handleAdd = (type: "user" | "group") => {
    setSelectedItem(null);
    setEditType(type);
    setEditDialogOpen(true);
  };

  const handleSave = (item: any) => {
    if (editType === "user") {
      if (selectedItem) {
        setUsers(users.map((u) => (u.id === item.id ? item : u)));
      } else {
        setUsers([...users, { ...item, id: Date.now().toString() }]);
      }
    } else {
      if (selectedItem) {
        setGroups(groups.map((g) => (g.id === item.id ? item : g)));
      } else {
        setGroups([...groups, { ...item, id: Date.now().toString(), users: 0 }]);
      }
    }
    setEditDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (editType === "user") {
      setUsers(users.filter((u) => u.id !== id));
    } else {
      setGroups(groups.filter((g) => g.id !== id));
    }
    setEditDialogOpen(false);
  };

  return (
    <>
      {/* User/Group Edit Dialog */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] p-0 gap-0 flex flex-col">
          <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800">
            <DialogTitle>
              {selectedItem ? `Edit ${editType === "user" ? "User" : "Group"}` : `Add ${editType === "user" ? "User" : "Group"}`}
            </DialogTitle>
          </DialogHeader>

          <ScrollArea className="flex-1 overflow-hidden">
            <div className="px-6 py-4 space-y-4">
              {editType === "user" ? (
                <UserForm item={selectedItem} onSave={handleSave} />
              ) : (
                <GroupForm item={selectedItem} onSave={handleSave} />
              )}
            </div>
          </ScrollArea>

          <div className="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-between">
            {selectedItem ? (
              <Button variant="destructive" onClick={() => handleDelete(selectedItem.id)} className="cursor-pointer">
                Delete
              </Button>
            ) : (
              <div />
            )}
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setEditDialogOpen(false)} className="cursor-pointer">
                Cancel
              </Button>
              <Button onClick={() => {
                // Form submission will be handled by the form components
              }} className="cursor-pointer">
                Save
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <div className="space-y-4">
        {/* View Toggle */}
        <div className="flex gap-2 p-1 bg-gray-100 dark:bg-gray-800 rounded-lg w-fit">
          <Button
            variant={view === "users" ? "default" : "ghost"}
            size="sm"
            onClick={() => setView("users")}
            className="cursor-pointer"
          >
            Users
          </Button>
          <Button
            variant={view === "groups" ? "default" : "ghost"}
            size="sm"
            onClick={() => setView("groups")}
            className="cursor-pointer"
          >
            Groups
          </Button>
        </div>

        {view === "users" ? (
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold">User Management</h3>
              <Button onClick={() => handleAdd("user")} size="sm" className="cursor-pointer">
                <Plus className="h-4 w-4 mr-1" />
                Add User
              </Button>
            </div>
            <div className="space-y-2">
              {users.map((user) => (
                <div
                  key={user.id}
                  className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
                      <span className="text-purple-600 dark:text-purple-400 font-medium">
                        {user.name.charAt(0)}
                      </span>
                    </div>
                    <div>
                      <h4 className="font-medium">{user.name}</h4>
                      <p className="text-sm text-gray-500 dark:text-gray-400">{user.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-sm">
                      <span className="text-gray-500 dark:text-gray-400">Role: </span>
                      <span className="font-medium">{user.role}</span>
                    </div>
                    <div className="text-sm">
                      <span className="text-gray-500 dark:text-gray-400">Group: </span>
                      <span className="font-medium">{user.group}</span>
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 cursor-pointer"
                      onClick={() => handleEdit(user, "user")}
                    >
                      <Edit2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-semibold">Group Management</h3>
              <Button onClick={() => handleAdd("group")} size="sm" className="cursor-pointer">
                <Plus className="h-4 w-4 mr-1" />
                Add Group
              </Button>
            </div>
            <div className="space-y-2">
              {groups.map((group) => (
                <div
                  key={group.id}
                  className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                >
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center gap-3">
                      <Users className="h-5 w-5 text-purple-500" />
                      <div>
                        <h4 className="font-medium">{group.name}</h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">{group.users} users</p>
                      </div>
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 cursor-pointer"
                      onClick={() => handleEdit(group, "group")}
                    >
                      <Edit2 className="h-4 w-4" />
                    </Button>
                  </div>
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    <div>
                      <span className="text-gray-500 dark:text-gray-400">Messages: </span>
                      <span>{group.maxMessages}</span>
                    </div>
                    <div>
                      <span className="text-gray-500 dark:text-gray-400">Add Tools: </span>
                      <span>{group.canAddTools ? "✓" : "✗"}</span>
                    </div>
                    <div>
                      <span className="text-gray-500 dark:text-gray-400">Create Projects: </span>
                      <span>{group.canCreateProjects ? "✓" : "✗"}</span>
                    </div>
                    <div>
                      <span className="text-gray-500 dark:text-gray-400">Web Search: </span>
                      <span>{group.canUseWebSearch ? "✓" : "✗"}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </>
  );
}

function UserForm({ item, onSave }: { item: any; onSave: (item: any) => void }) {
  const [formData, setFormData] = useState(item || {
    name: "",
    email: "",
    role: "User",
    group: "Users",
  });

  return (
    <div className="space-y-4">
      <div>
        <Label htmlFor="user-name">Name</Label>
        <Input
          id="user-name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="User name"
          className="mt-2"
        />
      </div>

      <div>
        <Label htmlFor="user-email">Email</Label>
        <Input
          id="user-email"
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          placeholder="user@example.com"
          className="mt-2"
        />
      </div>

      <div>
        <Label htmlFor="user-role">Role</Label>
        <Select value={formData.role} onValueChange={(role) => setFormData({ ...formData, role })}>
          <SelectTrigger className="mt-2">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="Admin">Admin</SelectItem>
            <SelectItem value="User">User</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div>
        <Label htmlFor="user-group">Group</Label>
        <Select value={formData.group} onValueChange={(group) => setFormData({ ...formData, group })}>
          <SelectTrigger className="mt-2">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="Admins">Admins</SelectItem>
            <SelectItem value="Power Users">Power Users</SelectItem>
            <SelectItem value="Users">Users</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Password</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="user-password">New Password</Label>
            <Input
              id="user-password"
              type="password"
              placeholder="Leave empty to keep current password"
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="user-confirm-password">Confirm Password</Label>
            <Input
              id="user-confirm-password"
              type="password"
              placeholder="Confirm new password"
              className="mt-2"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function GroupForm({ item, onSave }: { item: any; onSave: (item: any) => void }) {
  const [formData, setFormData] = useState(item || {
    name: "",
    maxMessages: "100/day",
    canAddTools: false,
    canCreateProjects: false,
    canUseWebSearch: true,
    canUseCodeInterpreter: false,
    canUploadFiles: true,
    canAccessKnowledge: true,
  });

  return (
    <div className="space-y-4">
      <div>
        <Label htmlFor="group-name">Group Name</Label>
        <Input
          id="group-name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Group name"
          className="mt-2"
        />
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Limits</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="max-messages">Max Messages</Label>
            <Select 
              value={formData.maxMessages} 
              onValueChange={(maxMessages) => setFormData({ ...formData, maxMessages })}
            >
              <SelectTrigger className="mt-2">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Unlimited">Unlimited</SelectItem>
                <SelectItem value="500/day">500 per day</SelectItem>
                <SelectItem value="100/day">100 per day</SelectItem>
                <SelectItem value="50/day">50 per day</SelectItem>
                <SelectItem value="10/day">10 per day</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Capabilities</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label htmlFor="can-add-tools">Add Tools</Label>
            <Switch 
              id="can-add-tools"
              checked={formData.canAddTools}
              onCheckedChange={(canAddTools) => setFormData({ ...formData, canAddTools })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="can-create-projects">Create Projects</Label>
            <Switch 
              id="can-create-projects"
              checked={formData.canCreateProjects}
              onCheckedChange={(canCreateProjects) => setFormData({ ...formData, canCreateProjects })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="can-use-websearch">Use Web Search</Label>
            <Switch 
              id="can-use-websearch"
              checked={formData.canUseWebSearch}
              onCheckedChange={(canUseWebSearch) => setFormData({ ...formData, canUseWebSearch })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="can-use-code">Use Code Interpreter</Label>
            <Switch 
              id="can-use-code"
              checked={formData.canUseCodeInterpreter}
              onCheckedChange={(canUseCodeInterpreter) => setFormData({ ...formData, canUseCodeInterpreter })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="can-upload">Upload Files</Label>
            <Switch 
              id="can-upload"
              checked={formData.canUploadFiles}
              onCheckedChange={(canUploadFiles) => setFormData({ ...formData, canUploadFiles })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="can-knowledge">Access Knowledge Base</Label>
            <Switch 
              id="can-knowledge"
              checked={formData.canAccessKnowledge}
              onCheckedChange={(canAccessKnowledge) => setFormData({ ...formData, canAccessKnowledge })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function ConnectionsTab() {
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedConnection, setSelectedConnection] = useState<any>(null);
  const [connections, setConnections] = useState([
    {
      id: "1",
      name: "OpenAI API",
      type: "OpenAI",
      url: "https://chat.firebird.co.nz/lmstudio/v1",
      enabled: true,
      isGroup: false,
    },
    {
      id: "2",
      name: "Anthropic API",
      url: "https://api.anthropic.com/v1",
      enabled: false,
      isGroup: false,
    },
    {
      id: "3",
      name: "Local Docker",
      url: "http://host.docker.internal:9099",
      enabled: false,
      isGroup: false,
      hasLayer: true,
    },
    {
      id: "4",
      name: "OpenRouter",
      url: "https://openrouter.ai/api/v1",
      enabled: false,
      isGroup: false,
    },
  ]);

  const handleEdit = (connection: any) => {
    setSelectedConnection(connection);
    setEditDialogOpen(true);
  };

  const handleAdd = () => {
    setSelectedConnection(null);
    setEditDialogOpen(true);
  };

  const handleSave = (connection: any) => {
    if (selectedConnection) {
      setConnections(
        connections.map((c) => (c.id === connection.id ? connection : c))
      );
    } else {
      setConnections([...connections, connection]);
    }
  };

  const handleDelete = (id: string) => {
    setConnections(connections.filter((c) => c.id !== id));
  };

  const toggleConnection = (id: string) => {
    setConnections(
      connections.map((c) =>
        c.id === id ? { ...c, enabled: !c.enabled } : c
      )
    );
  };

  return (
    <>
      <ConnectionEditDialog
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        connection={selectedConnection}
        onSave={handleSave}
        onDelete={selectedConnection ? handleDelete : undefined}
      />
      <div className="space-y-6">
        {/* OpenAI API Section */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">OpenAI API</h3>
            <Switch
              checked={connections[0]?.enabled}
              onCheckedChange={() => toggleConnection("1")}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="space-y-2">
            <div className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <span className="text-sm">
                Manage OpenAI API Connections
              </span>
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8"
                onClick={handleAdd}
              >
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            {connections
              .filter((c) => c.type === "OpenAI")
              .map((connection) => (
                <div
                  key={connection.id}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                >
                  <span className="text-sm">{connection.url}</span>
                  <Button
                    size="icon"
                    variant="ghost"
                    className="h-8 w-8"
                    onClick={() => handleEdit(connection)}
                  >
                    <Settings className="h-4 w-4" />
                  </Button>
                </div>
              ))}
          </div>
        </div>

        <Separator />

        {/* Ollama API Section */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Ollama API</h3>
            <Switch className="data-[state=checked]:bg-purple-600" />
          </div>
          <div className="space-y-2">
            <div className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <span className="text-sm">
                Manage Ollama API Connections
              </span>
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8"
                onClick={handleAdd}
              >
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            <div className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <span className="text-sm">http://10.0.0.50:1234/v1</span>
              <div className="flex items-center gap-2">
                <Button size="icon" variant="ghost" className="h-8 w-8">
                  <Download className="h-4 w-4" />
                </Button>
                <Button size="icon" variant="ghost" className="h-8 w-8">
                  <Settings className="h-4 w-4" />
                </Button>
              </div>
            </div>
            <p className="text-sm text-purple-500 dark:text-purple-400 px-3">
              Trouble accessing Ollama?{" "}
              <button className="underline">Click here for help.</button>
            </p>
          </div>
        </div>

        <Separator />

        {/* Direct Connections Section */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Direct Connections</h3>
            <Switch className="data-[state=checked]:bg-purple-600" />
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">
            Direct Connections allow users to connect to their own OpenAI
            compatible API endpoints.
          </p>
        </div>

        <Separator />

        {/* Cache Base Model List */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Cache Base Model List</h3>
            <Switch className="data-[state=checked]:bg-purple-600" />
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Base Model List Cache speeds up access by fetching base models only
            at startup or on settings save—faster, but may not show recent base
            model changes.
          </p>
        </div>
      </div>
    </>
  );
}

interface ModelsTabProps {
  enabledModels?: Record<string, boolean>;
  onToggleModel?: (modelId: string) => void;
}

function ModelsTab({ enabledModels, onToggleModel }: ModelsTabProps) {
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedModel, setSelectedModel] = useState<any>(null);
  const [models, setModels] = useState([
    { id: "gpt-4", name: "GPT-4", color: "text-purple-500", description: "Most capable model, best for complex tasks", connection: "OpenAI" },
    { id: "gpt-4-turbo", name: "GPT-4 Turbo", color: "text-blue-500", description: "Faster GPT-4 with improved performance", connection: "OpenAI" },
    { id: "gpt-3.5-turbo", name: "GPT-3.5 Turbo", color: "text-green-500", description: "Fast and efficient for most tasks", connection: "OpenAI" },
    { id: "claude-3-opus", name: "Claude 3 Opus", color: "text-orange-500", description: "Anthropic's most capable model", connection: "Anthropic" },
    { id: "claude-3-sonnet", name: "Claude 3 Sonnet", color: "text-amber-500", description: "Balanced speed and capability", connection: "Anthropic" },
    { id: "structured-gpt", name: "Structured GPT", color: "text-cyan-500", description: "Model with structured content support (tables, forms, panels)", connection: "OpenAI" },
  ]);

  const handleEdit = (model: any) => {
    setSelectedModel(model);
    setEditDialogOpen(true);
  };

  const handleAdd = () => {
    setSelectedModel(null);
    setEditDialogOpen(true);
  };

  const handleSave = (model: any) => {
    if (selectedModel) {
      setModels(models.map((m) => (m.id === model.id ? model : m)));
    } else {
      setModels([...models, model]);
    }
  };

  const handleDelete = (id: string) => {
    setModels(models.filter((m) => m.id !== id));
  };

  return (
    <>
      <ModelEditDialog
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        model={selectedModel}
        onSave={handleSave}
        onDelete={selectedModel ? handleDelete : undefined}
      />
      <div className="space-y-4">
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">AI Models Configuration</h3>
            <Button onClick={handleAdd} size="sm" className="cursor-pointer">
              <Plus className="h-4 w-4 mr-1" />
              Add Model
            </Button>
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
            Enable or disable models to show/hide them in the main dropdown
          </p>
          <div className="space-y-2">
            {models.map((model) => (
              <div
                key={model.id}
                className="p-3 border border-gray-200 dark:border-gray-800 rounded-lg hover:border-gray-300 dark:hover:border-gray-700 transition-colors"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3 flex-1">
                    <Sparkles className={`h-4 w-4 ${model.color}`} />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h4 className="font-medium text-sm">{model.name}</h4>
                        <span className="text-xs text-gray-500 dark:text-gray-400">via {model.connection}</span>
                      </div>
                      <p className="text-xs text-gray-500 dark:text-gray-400">
                        {model.description}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-7 w-7 cursor-pointer"
                      onClick={() => handleEdit(model)}
                    >
                      <Pencil className="h-3.5 w-3.5" />
                    </Button>
                    <Switch
                      checked={enabledModels?.[model.id] ?? true}
                      onCheckedChange={() => onToggleModel?.(model.id)}
                      className="data-[state=checked]:bg-purple-600"
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </>
  );
}

function PluginsTab() {
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedPlugin, setSelectedPlugin] = useState<any>(null);
  const [plugins, setPlugins] = useState([
    { 
      id: "web-search", 
      name: "Web Search", 
      icon: Search,
      description: "Search the web for real-time information",
      enabled: false,
      parameters: [
        { name: "api_key", type: "string", value: "", required: true },
        { name: "max_results", type: "number", value: "10", required: false },
      ]
    },
    { 
      id: "code-interpreter", 
      name: "Code Interpreter", 
      icon: Code,
      description: "Execute Python code and analyze data",
      enabled: true,
      parameters: [
        { name: "timeout", type: "number", value: "30", required: false },
      ]
    },
    { 
      id: "file-management", 
      name: "Supreme File Management", 
      icon: Wrench,
      description: "Advanced file operations and management",
      enabled: false,
      parameters: []
    },
    { 
      id: "weather-forecast", 
      name: "Weather Forecast", 
      icon: CloudSun,
      description: "Get weather forecasts and conditions",
      enabled: false,
      parameters: [
        { name: "api_key", type: "string", value: "", required: true },
        { name: "units", type: "string", value: "metric", required: false },
      ]
    },
    { 
      id: "echo-mcp", 
      name: "EchoMCP", 
      icon: Wrench,
      description: "Model Context Protocol integration",
      enabled: true,
      parameters: []
    },
  ]);

  const handleEdit = (plugin: any) => {
    setSelectedPlugin(plugin);
    setEditDialogOpen(true);
  };

  const handleAdd = () => {
    setSelectedPlugin(null);
    setEditDialogOpen(true);
  };

  const handleSave = (plugin: any) => {
    if (selectedPlugin) {
      setPlugins(plugins.map((p) => (p.id === plugin.id ? { ...plugin, icon: p.icon } : p)));
    } else {
      setPlugins([...plugins, { ...plugin, icon: Wrench }]);
    }
  };

  const handleDelete = (id: string) => {
    setPlugins(plugins.filter((p) => p.id !== id));
  };

  const togglePlugin = (id: string) => {
    setPlugins(plugins.map((p) => (p.id === id ? { ...p, enabled: !p.enabled } : p)));
  };

  return (
    <>
      <PluginEditDialog
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        plugin={selectedPlugin}
        onSave={handleSave}
        onDelete={selectedPlugin ? handleDelete : undefined}
      />
      <div className="space-y-4">
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Available Plugins</h3>
            <Button onClick={handleAdd} size="sm" className="cursor-pointer">
              <Plus className="h-4 w-4 mr-1" />
              Add Plugin
            </Button>
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
            Manage available tools and plugins for the AI assistant
          </p>
          <div className="space-y-3">
            {plugins.map((plugin) => {
              const Icon = plugin.icon;
              return (
                <div key={plugin.id} className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3 flex-1">
                      <Icon className="h-5 w-5 text-gray-500 dark:text-gray-400" />
                      <div className="flex-1">
                        <h4 className="font-medium">{plugin.name}</h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">{plugin.description}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        size="icon"
                        variant="ghost"
                        className="h-8 w-8 cursor-pointer"
                        onClick={() => handleEdit(plugin)}
                      >
                        <Settings className="h-4 w-4" />
                      </Button>
                      <Switch 
                        checked={plugin.enabled} 
                        onCheckedChange={() => togglePlugin(plugin.id)}
                        className="data-[state=checked]:bg-purple-600" 
                      />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </>
  );
}

function MCPTab() {
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedConnector, setSelectedConnector] = useState<any>(null);
  const [connectors, setConnectors] = useState([
    {
      id: "1",
      name: "EchoMCP",
      type: "MCP Streamable HTTP",
      url: "https://echo.firebird.co.nz",
      auth: "Bearer",
      authToken: "••••••••••••••••••••",
      mcpId: "EchoMCP",
      description: "Echo MCP Connector",
      visibility: "private" as const,
      groups: [],
      enabled: true,
    },
  ]);

  const handleEdit = (connector: any) => {
    setSelectedConnector(connector);
    setEditDialogOpen(true);
  };

  const handleAdd = () => {
    setSelectedConnector(null);
    setEditDialogOpen(true);
  };

  const handleSave = (connector: any) => {
    if (selectedConnector) {
      setConnectors(
        connectors.map((c) => (c.id === connector.id ? connector : c))
      );
    } else {
      setConnectors([...connectors, connector]);
    }
  };

  const handleDelete = (id: string) => {
    setConnectors(connectors.filter((c) => c.id !== id));
  };

  const toggleConnector = (id: string) => {
    setConnectors(
      connectors.map((c) =>
        c.id === id ? { ...c, enabled: !c.enabled } : c
      )
    );
  };

  return (
    <>
      <MCPEditDialog
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        connector={selectedConnector}
        onSave={handleSave}
        onDelete={selectedConnector ? handleDelete : undefined}
      />
      <div className="space-y-6">
        {/* MCP Connectors Section */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Model Context Protocol</h3>
            <Button onClick={handleAdd} size="sm" className="cursor-pointer">
              <Plus className="h-4 w-4 mr-1" />
              Add Connector
            </Button>
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
            Configure MCP servers and connectors for extended functionality
          </p>

          <div className="space-y-2">
            {connectors.length === 0 ? (
              <div className="p-8 border border-gray-200 dark:border-gray-800 rounded-lg text-center">
                <Database className="h-12 w-12 text-gray-400 mx-auto mb-3" />
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  No MCP connectors configured
                </p>
                <Button
                  onClick={handleAdd}
                  variant="outline"
                  size="sm"
                  className="mt-3 cursor-pointer"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add Your First Connector
                </Button>
              </div>
            ) : (
              connectors.map((connector) => (
                <div
                  key={connector.id}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors border border-gray-200 dark:border-gray-800"
                >
                  <div className="flex items-center gap-3 flex-1">
                    <div className="flex items-center gap-3 flex-1">
                      <Database className="h-5 w-5 text-purple-500" />
                      <div>
                        <h4 className="font-medium">{connector.name}</h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {connector.url}
                        </p>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Switch
                      checked={connector.enabled}
                      onCheckedChange={() => toggleConnector(connector.id)}
                      className="data-[state=checked]:bg-purple-600"
                    />
                    <Button
                      size="icon"
                      variant="ghost"
                      className="h-8 w-8 cursor-pointer"
                      onClick={() => handleEdit(connector)}
                    >
                      <Settings className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        <Separator />

        <div className="p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
          <p className="text-sm text-blue-800 dark:text-blue-200">
            <strong>About MCP:</strong> The Model Context Protocol allows AI models to interact with external tools and services. 
            Configure connectors to extend your AI assistant's capabilities.
          </p>
        </div>
      </div>
    </>
  );
}

function DocumentsTab() {
  const [contentExtractionEngine, setContentExtractionEngine] = useState("Default");
  const [pdfExtractImages, setPdfExtractImages] = useState(false);
  const [bypassEmbedding, setBypassEmbedding] = useState(false);
  const [textSplitter, setTextSplitter] = useState("RecursiveCharacter");
  const [chunkSize, setChunkSize] = useState("1500");
  const [chunkOverlap, setChunkOverlap] = useState("100");
  const [embeddingModelEngine, setEmbeddingModelEngine] = useState("Default");
  const [embeddingModel, setEmbeddingModel] = useState("sentence-transformers/all-MiniLM-L6-v2");
  const [fullContextMode, setFullContextMode] = useState(false);
  const [hybridSearch, setHybridSearch] = useState(true);
  const [rerankingEngine, setRerankingEngine] = useState("Default");
  const [rerankingModel, setRerankingModel] = useState("ms-marco-TinyBERT-L-2-v2");
  const [topKReranker, setTopKReranker] = useState("5");
  const [relevanceThreshold, setRelevanceThreshold] = useState("0");
  const [bm25Weight, setBm25Weight] = useState([0.5]);
  const [ragTemplate, setRagTemplate] = useState("Use the following context to answer the question:\n\nContext: {{CONTEXT}}\n\nQuestion: {{QUERY}}");

  return (
    <div className="space-y-6">
      {/* Content Extraction Engine */}
      <div>
        <h3 className="font-semibold mb-3">Content Extraction</h3>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Content Extraction Engine</Label>
            <Select value={contentExtractionEngine} onValueChange={setContentExtractionEngine}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Default">Default</SelectItem>
                <SelectItem value="Tika">Apache Tika</SelectItem>
                <SelectItem value="Custom">Custom</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between">
            <Label>PDF Extract Images</Label>
            <Switch 
              checked={pdfExtractImages} 
              onCheckedChange={setPdfExtractImages}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label>Bypass Embedding</Label>
            <Switch 
              checked={bypassEmbedding} 
              onCheckedChange={setBypassEmbedding}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>

      <Separator />

      {/* Text Splitting */}
      <div>
        <h3 className="font-semibold mb-3">Text Splitting</h3>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Text Splitter</Label>
            <Select value={textSplitter} onValueChange={setTextSplitter}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="RecursiveCharacter">Recursive Character</SelectItem>
                <SelectItem value="Character">Character</SelectItem>
                <SelectItem value="Token">Token</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Chunk Size</Label>
              <Input value={chunkSize} onChange={(e) => setChunkSize(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Chunk Overlap</Label>
              <Input value={chunkOverlap} onChange={(e) => setChunkOverlap(e.target.value)} />
            </div>
          </div>
        </div>
      </div>

      <Separator />

      {/* Embedding Model */}
      <div>
        <h3 className="font-semibold mb-3">Embedding Model</h3>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Embedding Model Engine</Label>
            <Select value={embeddingModelEngine} onValueChange={setEmbeddingModelEngine}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Default">Default</SelectItem>
                <SelectItem value="OpenAI">OpenAI</SelectItem>
                <SelectItem value="HuggingFace">Hugging Face</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>Embedding Model</Label>
            <div className="flex gap-2">
              <Input 
                value={embeddingModel} 
                onChange={(e) => setEmbeddingModel(e.target.value)}
                className="flex-1"
              />
              <Button size="icon" variant="outline" className="cursor-pointer">
                <Download className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      </div>

      <Separator />

      {/* Query Settings */}
      <div>
        <h3 className="font-semibold mb-3">Query Settings</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label>Full Context Mode</Label>
            <Switch 
              checked={fullContextMode} 
              onCheckedChange={setFullContextMode}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <Label>Hybrid Search</Label>
            <Switch 
              checked={hybridSearch} 
              onCheckedChange={setHybridSearch}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>

      <Separator />

      {/* Reranking */}
      <div>
        <h3 className="font-semibold mb-3">Reranking</h3>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Reranking Engine</Label>
            <Select value={rerankingEngine} onValueChange={setRerankingEngine}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Default">Default</SelectItem>
                <SelectItem value="Cohere">Cohere</SelectItem>
                <SelectItem value="Custom">Custom</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>Reranking Model</Label>
            <Input value={rerankingModel} onChange={(e) => setRerankingModel(e.target.value)} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Top K (Reranker)</Label>
              <Input value={topKReranker} onChange={(e) => setTopKReranker(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Relevance Threshold</Label>
              <Input value={relevanceThreshold} onChange={(e) => setRelevanceThreshold(e.target.value)} />
            </div>
          </div>

          <p className="text-xs text-gray-500 dark:text-gray-400">
            Note: If you set a minimum score, the search will only return documents with a score greater than or equal to the minimum score.
          </p>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <Label>BM25 Weight</Label>
              <span className="text-sm">Custom</span>
            </div>
            <div className="space-y-2">
              <Slider
                value={bm25Weight}
                onValueChange={setBm25Weight}
                min={0}
                max={1}
                step={0.1}
                className="w-full"
              />
              <div className="flex justify-between text-xs text-gray-500 dark:text-gray-400">
                <span>semantic</span>
                <span className="font-medium">{bm25Weight[0]}</span>
                <span>lexical</span>
              </div>
            </div>
          </div>

          <div className="space-y-2">
            <Label>RAG Template</Label>
            <Textarea
              value={ragTemplate}
              onChange={(e) => setRagTemplate(e.target.value)}
              className="min-h-[100px] font-mono text-sm"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function WebSearchTab() {
  const { settings, updateWebSearch } = useAdminSettings();
  const ws = settings.webSearch;

  return (
    <div className="space-y-6">
      {/* Enable Web Search */}
      <div className="flex items-center justify-between">
        <Label>Enable Web Search</Label>
        <Switch
          checked={ws.webSearchEnabled}
          onCheckedChange={(v) => updateWebSearch({ webSearchEnabled: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <Separator />

      {/* Search Engine */}
      <div className="space-y-2">
        <Label>Search Engine</Label>
        <Select value={ws.searchEngine} onValueChange={(v) => updateWebSearch({ searchEngine: v })}>
          <SelectTrigger>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="google">Google PSE</SelectItem>
            <SelectItem value="searxng">SearXNG</SelectItem>
            <SelectItem value="serpapi">SerpAPI</SelectItem>
            <SelectItem value="brave">Brave Search</SelectItem>
            <SelectItem value="serper">Serper</SelectItem>
            <SelectItem value="serply">Serply</SelectItem>
            <SelectItem value="duckduckgo">DuckDuckGo</SelectItem>
            <SelectItem value="tavity">Tavity</SelectItem>
            <SelectItem value="searchapi">SearchApi</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Separator />

      {/* Query Suggestions */}
      <div className="flex items-center justify-between">
        <Label>Enable Query Suggestions</Label>
        <Switch
          checked={ws.enableQuerySuggestions}
          onCheckedChange={(v) => updateWebSearch({ enableQuerySuggestions: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      {/* Web Loader */}
      <div className="flex items-center justify-between">
        <Label>Enable Web Loader for All URLs</Label>
        <Switch
          checked={ws.enableWebLoader}
          onCheckedChange={(v) => updateWebSearch({ enableWebLoader: v })}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <Separator />

      {/* Result Count */}
      <div className="space-y-2">
        <Label>Result Count</Label>
        <Input
          type="number"
          value={ws.resultCount}
          onChange={(e) => updateWebSearch({ resultCount: e.target.value })}
          min="1"
          max="20"
        />
      </div>

      <Separator />

      {/* API Keys Section */}
      <div className="space-y-4">
        <h3 className="font-semibold">API Keys</h3>

        {ws.searchEngine === "searxng" && (
          <div className="space-y-2">
            <Label>SearXNG Instance URL</Label>
            <Input
              type="url"
              value={ws.searxngUrl}
              onChange={(e) => updateWebSearch({ searxngUrl: e.target.value })}
              placeholder="https://searxng.example.com"
            />
          </div>
        )}

        {ws.searchEngine === "serpapi" && (
          <div className="space-y-2">
            <Label>SerpAPI API Key</Label>
            <Input
              type="password"
              value={ws.serpApiKey}
              onChange={(e) => updateWebSearch({ serpApiKey: e.target.value })}
              placeholder="Enter your SerpAPI key"
            />
          </div>
        )}

        {ws.searchEngine === "google" && (
          <>
            <div className="space-y-2">
              <Label>Google Search API Key</Label>
              <Input
                type="password"
                value={ws.googleSearchApiKey}
                onChange={(e) => updateWebSearch({ googleSearchApiKey: e.target.value })}
                placeholder="Enter your Google API key"
              />
            </div>
            <div className="space-y-2">
              <Label>Google Search Engine ID</Label>
              <Input
                value={ws.googleSearchEngineId}
                onChange={(e) => updateWebSearch({ googleSearchEngineId: e.target.value })}
                placeholder="Enter your Search Engine ID"
              />
            </div>
          </>
        )}

        {ws.searchEngine === "brave" && (
          <div className="space-y-2">
            <Label>Brave Search API Key</Label>
            <Input
              type="password"
              value={ws.braveSearchApiKey}
              onChange={(e) => updateWebSearch({ braveSearchApiKey: e.target.value })}
              placeholder="Enter your Brave Search API key"
            />
          </div>
        )}

        {ws.searchEngine === "serper" && (
          <div className="space-y-2">
            <Label>Serper API Key</Label>
            <Input
              type="password"
              value={ws.serperApiKey}
              onChange={(e) => updateWebSearch({ serperApiKey: e.target.value })}
              placeholder="Enter your Serper API key"
            />
          </div>
        )}

        {ws.searchEngine === "serply" && (
          <div className="space-y-2">
            <Label>Serply API Key</Label>
            <Input
              type="password"
              value={ws.serplyApiKey}
              onChange={(e) => updateWebSearch({ serplyApiKey: e.target.value })}
              placeholder="Enter your Serply API key"
            />
          </div>
        )}

        {ws.searchEngine === "tavity" && (
          <div className="space-y-2">
            <Label>Tavity API Key</Label>
            <Input
              type="password"
              value={ws.tavityApiKey}
              onChange={(e) => updateWebSearch({ tavityApiKey: e.target.value })}
              placeholder="Enter your Tavity API key"
            />
          </div>
        )}

        {ws.searchEngine === "searchapi" && (
          <div className="space-y-2">
            <Label>SearchAPI API Key</Label>
            <Input
              type="password"
              value={ws.searchapiApiKey}
              onChange={(e) => updateWebSearch({ searchapiApiKey: e.target.value })}
              placeholder="Enter your SearchAPI key"
            />
          </div>
        )}
      </div>
    </div>
  );
}

function CodeExecutionTab() {
  const { settings, updateCodeExecution } = useAdminSettings();

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Code Execution</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label>Enable Code Execution</Label>
            <Switch
              checked={settings.codeExecution.codeExecutionEnabled}
              onCheckedChange={(v) => updateCodeExecution({ codeExecutionEnabled: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
            <p className="text-sm text-yellow-800 dark:text-yellow-200">
              Warning: Enabling code execution can be dangerous. Only enable in secure environments.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

function AudioTab() {
  const { settings, updateAudio } = useAdminSettings();
  const audio = settings.audio;

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Audio Settings</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label>Enable Voice Input</Label>
            <Switch
              checked={audio.voiceInputEnabled}
              onCheckedChange={(v) => updateAudio({ voiceInputEnabled: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="flex items-center justify-between">
            <Label>Enable Text-to-Speech</Label>
            <Switch
              checked={audio.textToSpeechEnabled}
              onCheckedChange={(v) => updateAudio({ textToSpeechEnabled: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div>
            <Label>Voice Model</Label>
            <Select value={audio.voiceModel} onValueChange={(v) => updateAudio({ voiceModel: v })}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="alloy">Alloy</SelectItem>
                <SelectItem value="echo">Echo</SelectItem>
                <SelectItem value="nova">Nova</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>
    </div>
  );
}

function ImagesTab() {
  const { settings, updateImages } = useAdminSettings();
  const images = settings.images;

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Image Generation</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label>Enable Image Generation</Label>
            <Switch
              checked={images.imageGenerationEnabled}
              onCheckedChange={(v) => updateImages({ imageGenerationEnabled: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div>
            <Label>Image Model</Label>
            <Select value={images.imageModel} onValueChange={(v) => updateImages({ imageModel: v })}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="dall-e-3">DALL-E 3</SelectItem>
                <SelectItem value="dall-e-2">DALL-E 2</SelectItem>
                <SelectItem value="stable-diffusion">Stable Diffusion</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>
    </div>
  );
}

function UsersTab() {
  const users = [
    { id: 1, name: "John Doe", email: "john.doe@example.com", role: "Admin", status: "Active" },
    { id: 2, name: "Jane Smith", email: "jane.smith@example.com", role: "User", status: "Active" },
    { id: 3, name: "Bob Johnson", email: "bob.johnson@example.com", role: "User", status: "Inactive" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">User Management</h3>
        <Button>Add User</Button>
      </div>

      <div className="space-y-3">
        {users.map((user) => (
          <div
            key={user.id}
            className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
          >
            <div className="flex items-center justify-between">
              <div>
                <h4 className="font-medium">{user.name}</h4>
                <p className="text-sm text-gray-500 dark:text-gray-400">{user.email}</p>
              </div>
              <div className="flex items-center gap-4">
                <div className="text-right">
                  <div className="text-sm font-medium">{user.role}</div>
                  <div className={`text-xs ${user.status === "Active" ? "text-green-600 dark:text-green-400" : "text-gray-500"}`}>
                    {user.status}
                  </div>
                </div>
                <Button variant="outline" size="sm">
                  Edit
                </Button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function DatabaseTab() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="font-semibold mb-4">Database Management</h3>
        <div className="space-y-4">
          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <div className="flex items-center justify-between mb-2">
              <h4 className="font-medium">Database Size</h4>
              <span className="text-sm text-gray-500 dark:text-gray-400">256 MB</span>
            </div>
            <div className="w-full bg-gray-200 dark:bg-gray-800 rounded-full h-2">
              <div className="bg-purple-500 h-2 rounded-full" style={{ width: "45%" }}></div>
            </div>
          </div>

          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <div className="flex items-center justify-between">
              <div>
                <h4 className="font-medium">Backup Status</h4>
                <p className="text-sm text-gray-500 dark:text-gray-400">Last backup: 2 hours ago</p>
              </div>
              <Button variant="outline">Backup Now</Button>
            </div>
          </div>

          <Separator />

          <div>
            <h4 className="font-medium mb-3 text-red-600 dark:text-red-400">Danger Zone</h4>
            <div className="space-y-2">
              <Button
                variant="outline"
                className="w-full justify-start text-red-600 dark:text-red-400 border-red-200 dark:border-red-900"
              >
                Clear All Data
              </Button>
              <Button
                variant="outline"
                className="w-full justify-start text-red-600 dark:text-red-400 border-red-200 dark:border-red-900"
              >
                Reset Database
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function LogsTab() {
  const logs = [
    { id: 1, type: "Info", message: "User login successful", time: "2 minutes ago" },
    { id: 2, type: "Warning", message: "High memory usage detected", time: "15 minutes ago" },
    { id: 3, type: "Error", message: "API request failed", time: "1 hour ago" },
    { id: 4, type: "Info", message: "Database backup completed", time: "2 hours ago" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">System Logs</h3>
        <Button variant="outline" size="sm">Clear Logs</Button>
      </div>

      <div className="space-y-2">
        {logs.map((log) => (
          <div
            key={log.id}
            className="p-3 border border-gray-200 dark:border-gray-800 rounded-lg"
          >
            <div className="flex items-start justify-between">
              <div className="flex items-start gap-3">
                <span
                  className={`text-xs px-2 py-1 rounded ${
                    log.type === "Error"
                      ? "bg-red-100 dark:bg-red-900 text-red-600 dark:text-red-400"
                      : log.type === "Warning"
                      ? "bg-yellow-100 dark:bg-yellow-900 text-yellow-600 dark:text-yellow-400"
                      : "bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400"
                  }`}
                >
                  {log.type}
                </span>
                <p className="text-sm">{log.message}</p>
              </div>
              <span className="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                {log.time}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function AnalyticsTab() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="font-semibold mb-4">System Analytics</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <h4 className="text-sm text-gray-500 dark:text-gray-400 mb-1">Total Users</h4>
            <p className="text-3xl font-semibold">1,234</p>
            <p className="text-xs text-green-600 dark:text-green-400 mt-1">+12% this month</p>
          </div>

          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <h4 className="text-sm text-gray-500 dark:text-gray-400 mb-1">Active Sessions</h4>
            <p className="text-3xl font-semibold">89</p>
            <p className="text-xs text-green-600 dark:text-green-400 mt-1">+5% this week</p>
          </div>

          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <h4 className="text-sm text-gray-500 dark:text-gray-400 mb-1">API Calls</h4>
            <p className="text-3xl font-semibold">45.6K</p>
            <p className="text-xs text-red-600 dark:text-red-400 mt-1">-3% this week</p>
          </div>

          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <h4 className="text-sm text-gray-500 dark:text-gray-400 mb-1">Storage Used</h4>
            <p className="text-3xl font-semibold">256 MB</p>
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">of 1 GB</p>
          </div>
        </div>
      </div>
    </div>
  );
}
