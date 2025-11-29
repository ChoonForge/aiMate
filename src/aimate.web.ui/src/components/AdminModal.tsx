import { useState, useMemo, useEffect } from "react";
import { useDebug, useUIEventLogger } from "./DebugContext";
import { useAdminSettings } from "../context/AdminSettingsContext";
import { useAdmin } from "../hooks/useAdmin";
import { AppConfig } from "../utils/config";
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
  const { logUIEvent } = useUIEventLogger();
  const { addLog } = useDebug();

  useEffect(() => {
    if (open) {
      logUIEvent('Admin Panel opened', 'ui:admin:open');
      addLog({
        action: 'Admin Panel opened',
        category: 'admin:open',
        api: 'api/v1/GetAdminSettings',
        type: 'info'
      });
    }
  }, [open, logUIEvent, addLog]);

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
  const { debugEnabled, setDebugEnabled, addLog, debugSettings, updateDebugSettings } = useDebug();
  const { settings, updateGeneral } = useAdminSettings();
  const [offlineMode, setOfflineModeState] = useState(AppConfig.isOfflineMode());

  const handleOfflineModeChange = (checked: boolean) => {
    AppConfig.setOfflineMode(checked);
    setOfflineModeState(checked);
    addLog({
      action: `Offline mode: ${checked ? 'enabled' : 'disabled'}`,
      category: 'admin:general:offline',
      api: 'api/v1/admin/settings/general',
      payload: { offlineMode: checked },
      type: 'info'
    });
  };

  const handleAdminEnabledChange = (checked: boolean) => {
    updateGeneral({ adminEnabled: checked });
    addLog({
      action: `Admin panel: ${checked ? 'enabled' : 'disabled'}`,
      category: 'admin:general:admin',
      api: 'api/v1/admin/settings/general',
      payload: { adminEnabled: checked },
      type: 'info'
    });
  };

  const handleUserRegistrationChange = (checked: boolean) => {
    updateGeneral({ userRegistration: checked });
    addLog({
      action: `User registration: ${checked ? 'enabled' : 'disabled'}`,
      category: 'admin:general:registration',
      api: 'api/v1/admin/settings/general',
      payload: { userRegistration: checked },
      type: 'info'
    });
  };

  const handleApiAccessChange = (checked: boolean) => {
    updateGeneral({ apiAccess: checked });
    addLog({
      action: `API access: ${checked ? 'enabled' : 'disabled'}`,
      category: 'admin:general:api',
      api: 'api/v1/admin/settings/general',
      payload: { apiAccess: checked },
      type: 'info'
    });
  };

  const handleDebugModeChange = (checked: boolean) => {
    setDebugEnabled(checked);
    addLog({
      action: `Debug mode: ${checked ? 'enabled' : 'disabled'}`,
      category: 'admin:general:debug',
      api: 'api/v1/admin/settings/general',
      payload: { debugEnabled: checked },
      type: checked ? 'success' : 'warning'
    });
  };

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
              onCheckedChange={handleAdminEnabledChange}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="flex items-center justify-between">
            <Label htmlFor="user-registration">Allow User Registration</Label>
            <Switch
              id="user-registration"
              checked={settings.general.userRegistration}
              onCheckedChange={handleUserRegistrationChange}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
          <div className="flex items-center justify-between">
            <Label htmlFor="api-access">Enable API Access</Label>
            <Switch
              id="api-access"
              checked={settings.general.apiAccess}
              onCheckedChange={handleApiAccessChange}
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
              <Label htmlFor="offline-mode">Offline Mode</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Use mock data instead of live APIs (persists in browser storage)
              </p>
            </div>
            <Switch
              id="offline-mode"
              checked={offlineMode}
              onCheckedChange={handleOfflineModeChange}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
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
              onCheckedChange={handleDebugModeChange}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>

      {debugEnabled && (
        <>
          <Separator />

          <div>
            <h3 className="font-semibold mb-3">Debug Configuration</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <Label htmlFor="show-payloads">Show Request Payloads</Label>
                <Switch
                  id="show-payloads"
                  checked={debugSettings.showPayloads}
                  onCheckedChange={(v) => updateDebugSettings({ showPayloads: v })}
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
              <div className="flex items-center justify-between">
                <Label htmlFor="show-responses">Show API Responses</Label>
                <Switch
                  id="show-responses"
                  checked={debugSettings.showResponses}
                  onCheckedChange={(v) => updateDebugSettings({ showResponses: v })}
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
              <div className="flex items-center justify-between">
                <Label htmlFor="show-timestamps">Show Timestamps</Label>
                <Switch
                  id="show-timestamps"
                  checked={debugSettings.showTimestamps}
                  onCheckedChange={(v) => updateDebugSettings({ showTimestamps: v })}
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>
              <div className="flex items-center justify-between">
                <Label htmlFor="log-ui-events">Log UI Events</Label>
                <Switch
                  id="log-ui-events"
                  checked={debugSettings.logUIEvents}
                  onCheckedChange={(v) => updateDebugSettings({ logUIEvents: v })}
                  className="data-[state=checked]:bg-purple-600"
                />
              </div>

              <Separator />

              <div>
                <Label className="mb-2 block">Log Levels</Label>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <Label htmlFor="log-info" className="text-sm">Info</Label>
                    <Switch
                      id="log-info"
                      checked={debugSettings.logLevels.info}
                      onCheckedChange={(v) => updateDebugSettings({ logLevels: { ...debugSettings.logLevels, info: v } })}
                      className="data-[state=checked]:bg-purple-600"
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <Label htmlFor="log-success" className="text-sm">Success</Label>
                    <Switch
                      id="log-success"
                      checked={debugSettings.logLevels.success}
                      onCheckedChange={(v) => updateDebugSettings({ logLevels: { ...debugSettings.logLevels, success: v } })}
                      className="data-[state=checked]:bg-purple-600"
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <Label htmlFor="log-warning" className="text-sm">Warning</Label>
                    <Switch
                      id="log-warning"
                      checked={debugSettings.logLevels.warning}
                      onCheckedChange={(v) => updateDebugSettings({ logLevels: { ...debugSettings.logLevels, warning: v } })}
                      className="data-[state=checked]:bg-purple-600"
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <Label htmlFor="log-error" className="text-sm">Error</Label>
                    <Switch
                      id="log-error"
                      checked={debugSettings.logLevels.error}
                      onCheckedChange={(v) => updateDebugSettings({ logLevels: { ...debugSettings.logLevels, error: v } })}
                      className="data-[state=checked]:bg-purple-600"
                    />
                  </div>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="max-logs">Maximum Log Entries</Label>
                <Input
                  id="max-logs"
                  type="number"
                  value={debugSettings.maxLogs}
                  onChange={(e) => updateDebugSettings({ maxLogs: parseInt(e.target.value) || 200 })}
                  min={50}
                  max={1000}
                />
              </div>
            </div>
          </div>
        </>
      )}
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
          <button className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 cursor-pointer"
            onClick={() => alert('Task Models are used for automatic tasks like title generation and query rewriting.')}
            title="Task Models are used for automatic tasks like title generation and query rewriting."
          >
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
        <Button
          variant="outline"
          className="cursor-pointer"
          onClick={() => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.json';
            input.onchange = (e) => {
              const file = (e.target as HTMLInputElement).files?.[0];
              if (file) {
                const reader = new FileReader();
                reader.onload = (event) => {
                  try {
                    const suggestions = JSON.parse(event.target?.result as string);
                    updateInterface({ promptSuggestions: suggestions });
                  } catch {
                    alert('Invalid JSON file');
                  }
                };
                reader.readAsText(file);
              }
            };
            input.click();
          }}
        >
          <Upload className="h-4 w-4 mr-2" />
          Import Prompt Suggestions
        </Button>
        <Button
          variant="outline"
          className="cursor-pointer"
          onClick={() => {
            const element = document.createElement('a');
            element.setAttribute('href', `data:text/json;charset=utf-8,${encodeURIComponent(JSON.stringify(iface.promptSuggestions, null, 2))}`);
            element.setAttribute('download', `prompt-suggestions-${new Date().toISOString().split('T')[0]}.json`);
            element.style.display = 'none';
            document.body.appendChild(element);
            element.click();
            document.body.removeChild(element);
          }}
        >
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
  const { users: apiUsers } = useAdmin();

  const users = apiUsers.map(u => ({
    id: u.id,
    name: u.username,
    email: u.email,
    role: u.userTier,
    group: u.userTier === 'Admin' ? 'Admins' : u.userTier === 'BYOK' ? 'Power Users' : 'Users',
  }));

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
        // setUsers(users.map((u) => (u.id === item.id ? item : u)));
        console.warn('User editing not yet implemented - users are managed via API');
      } else {
        // setUsers([...users, { ...item, id: Date.now().toString() }]);
        console.warn('User creation not yet implemented - users are managed via API');
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
      // setUsers(users.filter((u) => u.id !== id));
      console.warn('User deletion not yet implemented - users are managed via API');
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
                // Get form data from form component via ref or state
                const formElement = document.querySelector('form');
                if (formElement) {
                  const formData = new FormData(formElement);
                  handleSave(Object.fromEntries(formData));
                }
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
  const { addLog } = useDebug();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedConnection, setSelectedConnection] = useState<any>(null);
  const { connections, toggleConnection, createConnection, updateConnection, deleteConnection: deleteConnectionApi, testConnection } = useAdmin();

  // Map ConnectionDto to dialog Connection format
  const mapToDialogConnection = (conn: any) => ({
    id: conn.id,
    name: conn.name,
    type: conn.provider || 'Custom',
    url: conn.url || '',
    auth: 'None',
    headers: '',
    prefixId: '',
    providerType: conn.provider || 'Custom',
    modelIds: conn.models || [],
    tags: [],
    enabled: conn.isActive ?? conn.enabled ?? true,
  });

  const handleEdit = (connection: any) => {
    setSelectedConnection(mapToDialogConnection(connection));
    setEditDialogOpen(true);
    addLog({
      action: `Editing connection: ${connection.name}`,
      category: 'admin:connections:edit',
      payload: { connectionId: connection.id, name: connection.name },
      type: 'info'
    });
  };

  const handleAdd = () => {
    setSelectedConnection(null);
    setEditDialogOpen(true);
    addLog({
      action: 'Adding new connection',
      category: 'admin:connections:add',
      type: 'info'
    });
  };

  const handleSave = async (connection: any) => {
    try {
      if (selectedConnection) {
        await updateConnection(connection.id, connection);
        addLog({
          action: `Connection updated: ${connection.name}`,
          category: 'admin:connections:update',
          api: 'api/v1/admin/connections',
          payload: connection,
          type: 'success'
        });
      } else {
        await createConnection(connection);
        addLog({
          action: `Connection created: ${connection.name}`,
          category: 'admin:connections:create',
          api: 'api/v1/admin/connections',
          payload: connection,
          type: 'success'
        });
      }
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[ConnectionsTab] Failed to save connection:', err);
      addLog({
        action: `Failed to save connection: ${connection.name}`,
        category: 'admin:connections:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      const conn = connections.find(c => c.id === id);
      await deleteConnectionApi(id);
      addLog({
        action: `Connection deleted: ${conn?.name || id}`,
        category: 'admin:connections:delete',
        api: 'api/v1/admin/connections',
        payload: { connectionId: id },
        type: 'warning'
      });
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[ConnectionsTab] Failed to delete connection:', err);
      addLog({
        action: 'Failed to delete connection',
        category: 'admin:connections:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleToggle = async (id: string) => {
    try {
      const conn = connections.find(c => c.id === id);
      const newState = !conn?.enabled;
      await toggleConnection(id);
      addLog({
        action: `${conn?.name}: ${newState ? 'enabled' : 'disabled'}`,
        category: 'admin:connections:toggle',
        api: 'api/v1/admin/connections/toggle',
        payload: { connectionId: id, enabled: newState, name: conn?.name },
        type: 'info'
      });
    } catch (err) {
      console.error('[ConnectionsTab] Failed to toggle connection:', err);
      addLog({
        action: 'Failed to toggle connection',
        category: 'admin:connections:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
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
              .filter((c) => c.provider === "OpenAI")
              .map((connection) => (
                <div
                  key={connection.id}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                >
                  <div className="flex-1">
                    <div className="text-sm font-medium">{connection.name}</div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">{connection.apiKeyPrefix}</div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Switch
                      checked={connection.isActive}
                      onCheckedChange={() => handleToggle(connection.id)}
                      className="data-[state=checked]:bg-purple-600"
                    />
                    <Button
                      size="icon"
                      variant="ghost"
                      className="h-8 w-8"
                      onClick={() => handleEdit(connection)}
                    >
                      <Settings className="h-4 w-4" />
                    </Button>
                  </div>
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

        {/* Direct Connections Section - LM Server / Custom */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold">Direct Connections (LM Server)</h3>
            <Button
              size="icon"
              variant="ghost"
              className="h-8 w-8"
              onClick={handleAdd}
            >
              <Plus className="h-4 w-4" />
            </Button>
          </div>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">
            Connect to LM Studio, Ollama, or any OpenAI-compatible API endpoint.
          </p>
          <div className="space-y-2">
            {connections.map((connection) => (
              <div
                key={connection.id}
                className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium">{connection.name}</div>
                  <div className="text-xs text-gray-500 dark:text-gray-400 truncate">
                    {(connection as any).url || connection.apiKeyPrefix || 'No URL configured'}
                  </div>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <Switch
                    checked={connection.isActive ?? connection.enabled}
                    onCheckedChange={() => handleToggle(connection.id)}
                    className="data-[state=checked]:bg-purple-600"
                  />
                  <Button
                    size="icon"
                    variant="ghost"
                    className="h-8 w-8"
                    onClick={() => handleEdit(connection)}
                  >
                    <Settings className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
            {connections.length === 0 && (
              <p className="text-sm text-gray-500 dark:text-gray-400 text-center py-4">
                No connections configured. Click + to add one.
              </p>
            )}
          </div>
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
  const { addLog } = useDebug();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedModel, setSelectedModel] = useState<any>(null);
  const { models, toggleModel, createModel, updateModel, deleteModel: deleteModelApi } = useAdmin();

  const handleEdit = (model: any) => {
    setSelectedModel(model);
    setEditDialogOpen(true);
    addLog({
      action: `Editing model: ${model.name}`,
      category: 'admin:models:edit',
      payload: { modelId: model.id, name: model.name },
      type: 'info'
    });
  };

  const handleAdd = () => {
    setSelectedModel(null);
    setEditDialogOpen(true);
    addLog({
      action: 'Adding new model',
      category: 'admin:models:add',
      type: 'info'
    });
  };

  const handleSave = async (model: any) => {
    try {
      if (selectedModel) {
        await updateModel(model.id, model);
        addLog({
          action: `Model updated: ${model.name}`,
          category: 'admin:models:update',
          api: 'api/v1/admin/models',
          payload: model,
          type: 'success'
        });
      } else {
        await createModel(model);
        addLog({
          action: `Model created: ${model.name}`,
          category: 'admin:models:create',
          api: 'api/v1/admin/models',
          payload: model,
          type: 'success'
        });
      }
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[ModelsTab] Failed to save model:', err);
      addLog({
        action: `Failed to save model: ${model.name}`,
        category: 'admin:models:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      const model = models.find(m => m.id === id);
      await deleteModelApi(id);
      addLog({
        action: `Model deleted: ${model?.name || id}`,
        category: 'admin:models:delete',
        api: 'api/v1/admin/models',
        payload: { modelId: id },
        type: 'warning'
      });
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[ModelsTab] Failed to delete model:', err);
      addLog({
        action: 'Failed to delete model',
        category: 'admin:models:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleToggleModel = async (id: string) => {
    const model = models.find(m => m.id === id);
    const newState = !model?.isActive;
    try {
      await toggleModel(id);
      addLog({
        action: `${model?.name}: ${newState ? 'enabled' : 'disabled'}`,
        category: 'admin:models:toggle',
        api: 'api/v1/admin/models/toggle',
        payload: { modelId: id, enabled: newState, name: model?.name },
        type: 'info'
      });
    } catch (err) {
      console.error('[ModelsTab] Failed to toggle model:', err);
      addLog({
        action: 'Failed to toggle model',
        category: 'admin:models:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
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
                    <Sparkles className={`h-4 w-4 ${model.color || 'text-purple-500'}`} />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h4 className="font-medium text-sm">{model.name}</h4>
                        <span className="text-xs text-gray-500 dark:text-gray-400">via {model.provider}</span>
                      </div>
                      <p className="text-xs text-gray-500 dark:text-gray-400">
                        {model.contextWindow?.toLocaleString()} tokens | {model.capabilities?.join(', ') || 'General purpose'}
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
                      checked={model.isActive}
                      onCheckedChange={() => handleToggleModel(model.id)}
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
  const { addLog } = useDebug();
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
      addLog({
        action: `Plugin updated: ${plugin.name}`,
        category: `plugin:${plugin.id}:update`,
        api: 'api/v1/admin/plugins',
        payload: plugin,
        type: 'success'
      });
    } else {
      setPlugins([...plugins, { ...plugin, icon: Wrench }]);
      addLog({
        action: `Plugin created: ${plugin.name}`,
        category: `plugin:${plugin.id}:create`,
        api: 'api/v1/admin/plugins',
        payload: plugin,
        type: 'success'
      });
    }
  };

  const handleDelete = (id: string) => {
    const plugin = plugins.find(p => p.id === id);
    setPlugins(plugins.filter((p) => p.id !== id));
    addLog({
      action: `Plugin deleted: ${plugin?.name}`,
      category: `plugin:${id}:delete`,
      api: 'api/v1/admin/plugins',
      payload: { pluginId: id },
      type: 'warning'
    });
  };

  const togglePlugin = (id: string) => {
    const plugin = plugins.find(p => p.id === id);
    const newEnabled = !plugin?.enabled;
    setPlugins(plugins.map((p) => (p.id === id ? { ...p, enabled: newEnabled } : p)));
    addLog({
      action: `${plugin?.name}: ${newEnabled ? 'enabled' : 'disabled'}`,
      category: `plugin:${id}:toggle`,
      api: 'api/v1/admin/plugins',
      payload: {
        pluginId: id,
        enabled: newEnabled,
        parameters: plugin?.parameters
      },
      type: 'info'
    });
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
  const { addLog } = useDebug();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedConnector, setSelectedConnector] = useState<any>(null);
  const { mcpServers, toggleMcpServer, createMcpServer, updateMcpServer, deleteMcpServer } = useAdmin();

  const handleEdit = (connector: any) => {
    setSelectedConnector(connector);
    setEditDialogOpen(true);
    addLog({
      action: `Editing MCP connector: ${connector.name}`,
      category: 'admin:mcp:edit',
      payload: { connectorId: connector.id, name: connector.name },
      type: 'info'
    });
  };

  const handleAdd = () => {
    setSelectedConnector(null);
    setEditDialogOpen(true);
    addLog({
      action: 'Adding new MCP connector',
      category: 'admin:mcp:add',
      type: 'info'
    });
  };

  const handleSave = async (connector: any) => {
    try {
      if (selectedConnector) {
        await updateMcpServer(connector.id, connector);
        addLog({
          action: `MCP connector updated: ${connector.name}`,
          category: 'admin:mcp:update',
          api: 'api/v1/admin/mcp',
          payload: connector,
          type: 'success'
        });
      } else {
        await createMcpServer(connector);
        addLog({
          action: `MCP connector created: ${connector.name}`,
          category: 'admin:mcp:create',
          api: 'api/v1/admin/mcp',
          payload: connector,
          type: 'success'
        });
      }
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[MCPTab] Failed to save MCP server:', err);
      addLog({
        action: `Failed to save MCP connector: ${connector.name}`,
        category: 'admin:mcp:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      const server = mcpServers.find(s => s.id === id);
      await deleteMcpServer(id);
      addLog({
        action: `MCP connector deleted: ${server?.name || id}`,
        category: 'admin:mcp:delete',
        api: 'api/v1/admin/mcp',
        payload: { connectorId: id },
        type: 'warning'
      });
      setEditDialogOpen(false);
    } catch (err) {
      console.error('[MCPTab] Failed to delete MCP server:', err);
      addLog({
        action: 'Failed to delete MCP connector',
        category: 'admin:mcp:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
  };

  const handleToggle = async (id: string) => {
    try {
      const server = mcpServers.find(s => s.id === id);
      const newState = !server?.enabled;
      await toggleMcpServer(id);
      addLog({
        action: `${server?.name}: ${newState ? 'enabled' : 'disabled'}`,
        category: 'admin:mcp:toggle',
        api: 'api/v1/admin/mcp/toggle',
        payload: { connectorId: id, enabled: newState, name: server?.name },
        type: 'info'
      });
    } catch (err) {
      console.error('[MCPTab] Failed to toggle MCP server:', err);
      addLog({
        action: 'Failed to toggle MCP connector',
        category: 'admin:mcp:error',
        payload: { error: String(err) },
        type: 'error'
      });
    }
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
            {mcpServers.length === 0 ? (
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
              mcpServers.map((server) => (
                <div
                  key={server.id}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors border border-gray-200 dark:border-gray-800"
                >
                  <div className="flex items-center gap-3 flex-1">
                    <div className="flex items-center gap-3 flex-1">
                      <Database className={`h-5 w-5 ${server.status === 'Connected' ? 'text-green-500' : server.status === 'Error' ? 'text-red-500' : 'text-gray-500'}`} />
                      <div>
                        <h4 className="font-medium">{server.name}</h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {server.description} • {server.tools.length} tools • {server.status}
                        </p>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Switch
                      checked={server.isActive}
                      onCheckedChange={() => handleToggle(server.id)}
                      className="data-[state=checked]:bg-purple-600"
                    />
                    <Button
                      size="icon"
                      variant="ghost"
                      className="h-8 w-8 cursor-pointer"
                      onClick={() => handleEdit(server)}
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
  const { addLog } = useDebug();
  const { settings, updateWebSearch } = useAdminSettings();
  const ws = settings.webSearch;

  const handleWebSearchToggle = (enabled: boolean) => {
    updateWebSearch({ webSearchEnabled: enabled });
    addLog({
      action: `Web search: ${enabled ? 'enabled' : 'disabled'}`,
      category: 'admin:websearch:toggle',
      api: 'api/v1/admin/settings/websearch',
      payload: { webSearchEnabled: enabled },
      type: 'info'
    });
  };

  const handleSearchEngineChange = (engine: string) => {
    updateWebSearch({ searchEngine: engine });
    addLog({
      action: `Search engine changed to: ${engine}`,
      category: 'admin:websearch:engine',
      api: 'api/v1/admin/settings/websearch',
      payload: { searchEngine: engine },
      type: 'info'
    });
  };

  return (
    <div className="space-y-6">
      {/* Enable Web Search */}
      <div className="flex items-center justify-between">
        <Label>Enable Web Search</Label>
        <Switch
          checked={ws.webSearchEnabled}
          onCheckedChange={handleWebSearchToggle}
          className="data-[state=checked]:bg-purple-600"
        />
      </div>

      <Separator />

      {/* Search Engine */}
      <div className="space-y-2">
        <Label>Search Engine</Label>
        <Select value={ws.searchEngine} onValueChange={handleSearchEngineChange}>
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
