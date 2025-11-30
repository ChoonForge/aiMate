import { useState, useEffect, useMemo } from "react";
import { Button } from "./ui/button";
import { Settings, Palette, Link2, User, Sparkles, BarChart3, Brain } from "lucide-react";
import { MemoriesPanel } from "./MemoriesPanel";
import { Switch } from "./ui/switch";
import { Label } from "./ui/label";
import { useUsage } from "../hooks/useUsage";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "./ui/table";
import { useTheme } from "./ThemeProvider";
import { toast } from "sonner";
import { useDebug, useUIEventLogger } from "./DebugContext";
import { useUserSettings } from "../context/UserSettingsContext";
import { UsageDetailsDialog } from "./UsageDetailsDialog";
import { BaseModal } from "./BaseModal";

interface SettingsModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SettingsModal({ open, onOpenChange }: SettingsModalProps) {
  const { addLog } = useDebug();
  const { logUIEvent } = useUIEventLogger();

  useEffect(() => {
    if (open) {
      logUIEvent('Settings Modal opened', 'ui:settings:open');
      addLog({
        action: 'User Settings opened',
        category: 'settings:open',
        api: 'api/v1/GetUserSettings',
        type: 'info'
      });
    }
  }, [open, addLog, logUIEvent]);

  const handleSave = () => {
    toast.success("User settings saved!");
    addLog({
      action: 'Settings Saved',
      category: 'settings:save',
      api: 'api/v1/SaveUserSettings',
      type: 'success'
    });
    onOpenChange(false);
  };

  // MVP visible tabs - connections hidden until BYOK phase
  const visibleTabs = ['general', 'interface', 'personalisation', 'memories', 'account', 'usage'];

  const tabs = useMemo(() => [
    {
      id: "general",
      label: "General",
      icon: Settings,
      content: <GeneralTab />,
    },
    {
      id: "interface",
      label: "Interface",
      icon: Palette,
      content: <InterfaceTab />,
    },
    {
      id: "connections",
      label: "Connections",
      icon: Link2,
      content: <ConnectionsTab />,
    },
    {
      id: "personalisation",
      label: "Personalisation",
      icon: Sparkles,
      content: <PersonalisationTab />,
    },
    {
      id: "account",
      label: "Account",
      icon: User,
      content: <AccountTab />,
    },
    {
      id: "memories",
      label: "Memories",
      icon: Brain,
      content: <MemoriesPanel />,
    },
    {
      id: "usage",
      label: "Usage",
      icon: BarChart3,
      content: <UsageTab />,
    },
  ].filter(tab => visibleTabs.includes(tab.id)), []);

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title="Settings"
      description="Manage your application settings and preferences"
      icon={Settings}
      size="lg"
      onSave={handleSave}
      showSave={true}
      showDelete={false}
      tabs={tabs}
    />
  );
}

function GeneralTab() {
  const { settings, updateGeneral } = useUserSettings();
  const general = settings.general || {};

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">WebUI Settings</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label htmlFor="language">Language</Label>
            <Select value={general.language} onValueChange={(v) => updateGeneral({ language: v })}>
              <SelectTrigger className="w-[180px]" id="language">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="en-gb">English (GB)</SelectItem>
                <SelectItem value="en-us">English (US)</SelectItem>
                <SelectItem value="es">Spanish</SelectItem>
                <SelectItem value="fr">French</SelectItem>
                <SelectItem value="de">German</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="notifications">Notifications</Label>
            <Select value={general.notifications} onValueChange={(v) => updateGeneral({ notifications: v })}>
              <SelectTrigger className="w-[180px]" id="notifications">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="off">Off</SelectItem>
                <SelectItem value="on">On</SelectItem>
                <SelectItem value="mentions">Mentions Only</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">System Prompt</h3>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-3">
          Your name is Echo and you are an AI assistant, my name is Rich.
        </p>
        <Textarea
          placeholder="Enter your system prompt..."
          value={general.systemPrompt}
          onChange={(e) => updateGeneral({ systemPrompt: e.target.value })}
          className="min-h-[240px] font-mono text-sm"
        />
      </div>
    </div>
  );
}

function InterfaceTab() {
  const { addLog } = useDebug();
  const { theme, setTheme, fontSize, setFontSize, colorTheme, setColorTheme } = useTheme();
  const { settings, updateInterface } = useUserSettings();
  const iface = settings.interface || {};
  const [localTheme, setLocalTheme] = useState(theme);
  const [localFontSize, setLocalFontSize] = useState(fontSize);
  const [localColorTheme, setLocalColorTheme] = useState(colorTheme);

  useEffect(() => {
    setTheme(localTheme);
  }, [localTheme, setTheme]);

  useEffect(() => {
    setFontSize(localFontSize);
  }, [localFontSize, setFontSize]);

  useEffect(() => {
    setColorTheme(localColorTheme);
  }, [localColorTheme, setColorTheme]);

  const handleTimestampsToggle = (checked: boolean) => {
    updateInterface({ showTimestamps: checked });
    addLog({
      action: `Show timestamps: ${checked ? 'enabled' : 'disabled'}`,
      category: 'settings:interface:timestamps',
      api: 'api/v1/settings/interface',
      payload: { showTimestamps: checked },
      type: 'info'
    });
  };

  const handleSyntaxHighlightingToggle = (checked: boolean) => {
    updateInterface({ syntaxHighlighting: checked });
    addLog({
      action: `Syntax highlighting: ${checked ? 'enabled' : 'disabled'}`,
      category: 'settings:interface:syntax',
      api: 'api/v1/settings/interface',
      payload: { syntaxHighlighting: checked },
      type: 'info'
    });
  };

  const handleMarkdownToggle = (checked: boolean) => {
    updateInterface({ markdownSupport: checked });
    addLog({
      action: `Markdown support: ${checked ? 'enabled' : 'disabled'}`,
      category: 'settings:interface:markdown',
      api: 'api/v1/settings/interface',
      payload: { markdownSupport: checked },
      type: 'info'
    });
  };

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Theme</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label htmlFor="theme">Appearance</Label>
            <Select value={localTheme} onValueChange={(value) => setLocalTheme(value as "light" | "dark" | "system")}>
              <SelectTrigger className="w-[180px]" id="theme">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="light">Light</SelectItem>
                <SelectItem value="dark">Dark</SelectItem>
                <SelectItem value="system">System</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="color-theme">Color Theme</Label>
            <Select value={localColorTheme} onValueChange={(value) => setLocalColorTheme(value as "purple" | "green" | "orange" | "pink" | "cyan")}>
              <SelectTrigger className="w-[180px]" id="color-theme">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="purple">Purple</SelectItem>
                <SelectItem value="blue">Blue</SelectItem>
                <SelectItem value="green">Green</SelectItem>
                <SelectItem value="orange">Orange</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center justify-between">
            <Label htmlFor="font-size">Font Size</Label>
            <Select value={localFontSize} onValueChange={(value) => setLocalFontSize(value as "small" | "medium" | "large")}>
              <SelectTrigger className="w-[180px]" id="font-size">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="small">Small</SelectItem>
                <SelectItem value="medium">Medium</SelectItem>
                <SelectItem value="large">Large</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Chat Display</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="timestamps">Show timestamps</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Display message timestamps
              </p>
            </div>
            <Switch
              id="timestamps"
              checked={iface.showTimestamps}
              onCheckedChange={handleTimestampsToggle}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="code-syntax">Syntax highlighting</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Highlight code in messages
              </p>
            </div>
            <Switch
              id="code-syntax"
              checked={iface.syntaxHighlighting}
              onCheckedChange={handleSyntaxHighlightingToggle}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="markdown">Markdown support</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Render markdown formatting
              </p>
            </div>
            <Switch
              id="markdown"
              checked={iface.markdownSupport}
              onCheckedChange={handleMarkdownToggle}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function ConnectionsTab() {
  const { settings, updateConnections } = useUserSettings();
  const conn = settings.connections || {};

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">API Connections</h3>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
          Configure API endpoints and authentication
        </p>

        <div className="space-y-4">
          <div>
            <Label htmlFor="openai-key">OpenAI API Key</Label>
            <Input
              id="openai-key"
              type="password"
              placeholder="sk-..."
              value={conn.openaiApiKey}
              onChange={(e) => updateConnections({ openaiApiKey: e.target.value })}
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="anthropic-key">Anthropic API Key</Label>
            <Input
              id="anthropic-key"
              type="password"
              placeholder="sk-ant-..."
              value={conn.anthropicApiKey}
              onChange={(e) => updateConnections({ anthropicApiKey: e.target.value })}
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="ollama-url">Ollama Base URL</Label>
            <Input
              id="ollama-url"
              type="url"
              placeholder="http://localhost:11434"
              value={conn.ollamaBaseUrl}
              onChange={(e) => updateConnections({ ollamaBaseUrl: e.target.value })}
              className="mt-2"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function PersonalisationTab() {
  const { settings, updatePersonalisation } = useUserSettings();
  const pers = settings.personalisation || {};

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">AI Behavior</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="creativity">Creativity Level</Label>
            <Select value={pers.creativityLevel} onValueChange={(v) => updatePersonalisation({ creativityLevel: v })}>
              <SelectTrigger className="mt-2" id="creativity">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="precise">Precise</SelectItem>
                <SelectItem value="balanced">Balanced</SelectItem>
                <SelectItem value="creative">Creative</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label htmlFor="response-style">Response Style</Label>
            <Select value={pers.responseStyle} onValueChange={(v) => updatePersonalisation({ responseStyle: v })}>
              <SelectTrigger className="mt-2" id="response-style">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="concise">Concise</SelectItem>
                <SelectItem value="balanced">Balanced</SelectItem>
                <SelectItem value="detailed">Detailed</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Custom Instructions</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="custom-instructions">Additional Context</Label>
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-2">
              Customize how the AI responds to you
            </p>
            <Textarea
              id="custom-instructions"
              placeholder="e.g., Always respond in a friendly tone, use emojis occasionally..."
              value={pers.customInstructions}
              onChange={(e) => updatePersonalisation({ customInstructions: e.target.value })}
              className="min-h-[120px]"
            />
          </div>

          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="remember-context">Remember conversation context</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                AI remembers details from previous messages
              </p>
            </div>
            <Switch
              id="remember-context"
              checked={pers.rememberContext}
              onCheckedChange={(v) => updatePersonalisation({ rememberContext: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

function AccountTab() {
  const { settings, updateAccount, resetSettings } = useUserSettings();
  const account = settings.account || {};

  const handleResetSettings = () => {
    resetSettings();
    toast.success("All settings have been reset to defaults");
  };

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Account Information</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              placeholder="your.email@example.com"
              value={account.email}
              onChange={(e) => updateAccount({ email: e.target.value })}
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="username">Username</Label>
            <Input
              id="username"
              placeholder="username"
              value={account.username}
              onChange={(e) => updateAccount({ username: e.target.value })}
              className="mt-2"
            />
          </div>

          <Button variant="outline">Update Profile</Button>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Password</h3>
        <div className="space-y-4">
          <div>
            <Label htmlFor="current-password">Current Password</Label>
            <Input
              id="current-password"
              type="password"
              placeholder="Enter current password"
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="new-password">New Password</Label>
            <Input
              id="new-password"
              type="password"
              placeholder="Enter new password"
              className="mt-2"
            />
          </div>

          <div>
            <Label htmlFor="confirm-password">Confirm Password</Label>
            <Input
              id="confirm-password"
              type="password"
              placeholder="Confirm new password"
              className="mt-2"
            />
          </div>

          <Button variant="outline">Change Password</Button>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Privacy & Data</h3>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="analytics">Allow Analytics</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Help improve the app with anonymous usage data
              </p>
            </div>
            <Switch
              id="analytics"
              checked={account.allowAnalytics}
              onCheckedChange={(v) => updateAccount({ allowAnalytics: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <div className="flex items-center justify-between">
            <div>
              <Label htmlFor="personalization">Personalization</Label>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Use data to personalize your experience
              </p>
            </div>
            <Switch
              id="personalization"
              checked={account.personalization}
              onCheckedChange={(v) => updateAccount({ personalization: v })}
              className="data-[state=checked]:bg-purple-600"
            />
          </div>

          <Button variant="outline">Download My Data</Button>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-4">Subscription</h3>
        <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
          <div className="flex items-center justify-between mb-2">
            <div>
              <h4 className="font-medium">Free Plan</h4>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                Basic features with limited usage
              </p>
            </div>
            <Button>Upgrade</Button>
          </div>
        </div>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-4 text-red-600 dark:text-red-400">Danger Zone</h3>
        <div className="space-y-3">
          <Button variant="outline" className="w-full justify-start text-red-600 dark:text-red-400 border-red-200 dark:border-red-900 hover:bg-red-50 dark:hover:bg-red-950">
            Clear All Conversations
          </Button>
          <Button
            variant="outline"
            className="w-full justify-start text-red-600 dark:text-red-400 border-red-200 dark:border-red-900 hover:bg-red-50 dark:hover:bg-red-950"
            onClick={handleResetSettings}
          >
            Reset All Settings
          </Button>
          <Button variant="outline" className="w-full justify-start text-red-600 dark:text-red-400 border-red-200 dark:border-red-900 hover:bg-red-50 dark:hover:bg-red-950">
            Delete Account
          </Button>
        </div>
      </div>
    </div>
  );
}

function UsageTab() {
  const [detailsOpen, setDetailsOpen] = useState(false);
  const { stats, loading } = useUsage();
  
  if (loading || !stats) {
    return (
      <div className="flex items-center justify-center p-8">
        <p className="text-gray-500 dark:text-gray-400">Loading usage data...</p>
      </div>
    );
  }

  const totalMessages = stats.totalMessages;
  const totalTokens = stats.totalTokens;
  const totalCost = stats.totalCost;
  
  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-NZ', { month: 'short', day: 'numeric', year: 'numeric' });
  };

  return (
    <div className="space-y-4">
      <div>
        <h3 className="font-semibold mb-3">Current Billing Period</h3>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
          {formatDate(stats.billingPeriodStart)} - {formatDate(stats.billingPeriodEnd)}
        </p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Total Messages</p>
            <p className="text-2xl font-semibold">{totalMessages.toLocaleString()}</p>
          </div>
          <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Total Tokens</p>
            <p className="text-2xl font-semibold">{(totalTokens / 1000).toFixed(1)}K</p>
          </div>
          <div className="p-4 border border-purple-200 dark:border-purple-800 bg-purple-50 dark:bg-purple-950/20 rounded-lg">
            <p className="text-sm text-purple-600 dark:text-purple-400 mb-1">Total Cost</p>
            <p className="text-2xl font-semibold text-purple-700 dark:text-purple-300">
              ${totalCost.toFixed(2)}
            </p>
          </div>
        </div>

        <Button onClick={() => setDetailsOpen(true)} variant="outline" className="w-full">
          View Detailed Analytics
        </Button>
      </div>

      <Separator />

      <div>
        <h3 className="font-semibold mb-3">Usage by Model</h3>
        <div className="border border-gray-200 dark:border-gray-800 rounded-lg overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Model</TableHead>
                <TableHead>Connection</TableHead>
                <TableHead className="text-right">Messages</TableHead>
                <TableHead className="text-right">Tokens</TableHead>
                <TableHead className="text-right">Cost</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {stats.usageByModel?.map((item) => (
                <TableRow key={item.model}>
                  <TableCell className="font-medium">
                    <span style={{ color: item.color }}>{item.model}</span>
                  </TableCell>
                  <TableCell>{item.connection}</TableCell>
                  <TableCell className="text-right">{item.messages.toLocaleString()}</TableCell>
                  <TableCell className="text-right">{(item.tokens / 1000).toFixed(1)}K</TableCell>
                  <TableCell className="text-right">${item.cost.toFixed(2)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>

      <UsageDetailsDialog open={detailsOpen} onOpenChange={setDetailsOpen} />
    </div>
  );
}