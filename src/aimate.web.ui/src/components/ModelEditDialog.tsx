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
import { Checkbox } from "./ui/checkbox";
import { Slider } from "./ui/slider";
import { Switch } from "./ui/switch";
import { Separator } from "./ui/separator";
import { ChevronDown, ChevronUp, Download, Loader2, Zap, CheckCircle2, XCircle } from "lucide-react";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "./ui/collapsible";
import { toast } from "sonner";

interface Model {
  id: string;
  name: string;
  description: string;
  color: string;
  connection?: string;
  provider?: string;
  contextWindow?: number;
  maxTokens?: number;
}

interface ModelEditDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  model?: Model | null;
  onSave: (model: Model) => void;
  onDelete?: (id: string) => void;
}

export function ModelEditDialog({
  open,
  onOpenChange,
  model,
  onSave,
  onDelete,
}: ModelEditDialogProps) {
  const [activeTab, setActiveTab] = useState("general");
  const [formData, setFormData] = useState<Model>(
    model || {
      id: Date.now().toString(),
      name: "",
      description: "",
      color: "text-purple-500",
      connection: "OpenAI",
      provider: "OpenAI",
      contextWindow: 8000,
      maxTokens: 4000,
    }
  );

  // Connection testing state
  const [testingConnection, setTestingConnection] = useState(false);
  const [connectionStatus, setConnectionStatus] = useState<"idle" | "success" | "error">("idle");
  const [apiKey, setApiKey] = useState("");
  const [endpoint, setEndpoint] = useState("");

  // Model Params State
  const [systemPrompt, setSystemPrompt] = useState("Write your model system prompt content here\ne.g.) You are Mario from Super Mario Bros, acting as an assistant.");
  const [advancedParamsOpen, setAdvancedParamsOpen] = useState(false);
  const [promptSuggestions, setPromptSuggestions] = useState("Default");
  const [jsonPreviewOpen, setJsonPreviewOpen] = useState(false);

  // Tools
  const [tools, setTools] = useState({
    supremeFileManagement: false,
    weatherForecast: false,
    echoMCP: true,
  });

  // Filters
  const [filters, setFilters] = useState({
    autolearn: true,
    chatMetrics: true,
  });

  // Actions
  const [actions, setActions] = useState({
    addToMemories: true,
    sora2AsyncPromptHelper: false,
  });

  // Capabilities
  const [capabilities, setCapabilities] = useState({
    vision: true,
    fileUpload: true,
    webSearch: true,
    imageGeneration: true,
    codeInterpreter: true,
    usage: true,
    citations: true,
    statusUpdates: true,
    structuredContent: false,
  });

  // Default Features
  const [defaultFeatures, setDefaultFeatures] = useState({
    anonymousChat: false,
  });

  // Advanced Params with state (off, default, custom)
  const [temperatureState, setTemperatureState] = useState<"off" | "default" | "custom">("default");
  const [temperature, setTemperature] = useState([0.8]);
  const [topPState, setTopPState] = useState<"off" | "default" | "custom">("default");
  const [topP, setTopP] = useState([0.95]);
  const [topKState, setTopKState] = useState<"off" | "default" | "custom">("default");
  const [topK, setTopK] = useState([50]);
  const [seed, setSeed] = useState("");
  const [maxTokensValue, setMaxTokensValue] = useState("4096");
  const [minPValue, setMinPValue] = useState("0");
  const [frequencyPenalty, setFrequencyPenalty] = useState([0]);
  const [repeatPenalty, setRepeatPenalty] = useState([1.1]);
  const [mirostatMode, setMirostatMode] = useState("0");
  const [mirostatTau, setMirostatTau] = useState([5]);
  const [mirostatEta, setMirostatEta] = useState([0.1]);
  const [contextLength, setContextLength] = useState("4096");
  const [batchSize, setBatchSize] = useState("512");
  const [keepAlive, setKeepAlive] = useState("5m");
  const [stopSequence, setStopSequence] = useState("");
  const [numPredict, setNumPredict] = useState("");
  const [tfsZ, setTfsZ] = useState([1]);
  const [numCtx, setNumCtx] = useState("2048");
  const [numBatch, setNumBatch] = useState("512");
  const [typicalP, setTypicalP] = useState([1]);

  useEffect(() => {
    if (model) {
      setFormData(model);
    } else {
      setFormData({
        id: Date.now().toString(),
        name: "",
        description: "",
        color: "text-purple-500",
        provider: "OpenAI",
        contextWindow: 8000,
        maxTokens: 4000,
      });
    }
  }, [model, open]);

  useEffect(() => {
    if (open) {
      setActiveTab("general");
      setConnectionStatus("idle");
    }
  }, [open]);

  const handleTestConnection = async () => {
    // Build endpoint based on connection type or use custom endpoint
    let testUrl = endpoint;
    if (!testUrl) {
      // Default endpoints by provider
      const defaultEndpoints: Record<string, string> = {
        'OpenAI': 'https://api.openai.com/v1',
        'Anthropic': 'https://api.anthropic.com/v1',
        'Google': 'https://generativelanguage.googleapis.com/v1',
        'Local': 'http://localhost:1234/v1',
        'LM Studio': 'http://localhost:1234/v1',
      };
      testUrl = defaultEndpoints[formData.connection || 'OpenAI'] || '';
    }

    if (!testUrl) {
      toast.error("Please enter an endpoint URL");
      return;
    }

    setTestingConnection(true);
    setConnectionStatus("idle");

    try {
      const modelsUrl = testUrl.replace(/\/$/, '') + '/models';
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      if (apiKey) {
        headers['Authorization'] = `Bearer ${apiKey}`;
      }

      const response = await fetch(modelsUrl, {
        headers,
        signal: AbortSignal.timeout(10000),
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      const models = data.data || data.models || [];
      const modelIds = models.map((m: { id: string }) => m.id);

      // Check if our model ID exists
      if (formData.id && modelIds.length > 0 && !modelIds.includes(formData.id)) {
        setConnectionStatus("error");
        toast.warning(`Model "${formData.id}" not found. Available: ${modelIds.slice(0, 3).join(', ')}${modelIds.length > 3 ? '...' : ''}`);
      } else {
        setConnectionStatus("success");
        toast.success(`Connected! ${modelIds.length} models available.`);
      }
    } catch (error) {
      setConnectionStatus("error");
      const message = error instanceof Error ? error.message : 'Unknown error';
      toast.error(`Connection failed: ${message}`);
    } finally {
      setTestingConnection(false);
    }
  };

  const handleSave = () => {
    onSave(formData);
    onOpenChange(false);
  };

  const handleDelete = () => {
    if (model && onDelete) {
      onDelete(model.id);
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col">
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <DialogTitle>
            {model ? "Edit Model" : "Add Model"}
          </DialogTitle>
          <DialogDescription className="sr-only">
            Configure AI model settings
          </DialogDescription>
        </DialogHeader>

        {/* Horizontal Scrolling Tabs */}
        <div className="flex-shrink-0 border-b border-gray-200 dark:border-gray-800 overflow-x-auto tab-scrollbar">
          <div className="flex px-6 gap-1 min-w-max">
            <button
              onClick={() => setActiveTab("general")}
              className={`flex items-center gap-2 px-4 py-2.5 border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === "general"
                ? "border-purple-500 text-purple-600 dark:text-purple-400"
                : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                }`}
            >
              General
            </button>
            <button
              onClick={() => setActiveTab("params")}
              className={`flex items-center gap-2 px-4 py-2.5 border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === "params"
                ? "border-purple-500 text-purple-600 dark:text-purple-400"
                : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                }`}
            >
              Model Params
            </button>
            <button
              onClick={() => setActiveTab("advanced")}
              className={`flex items-center gap-2 px-4 py-2.5 border-b-2 transition-colors whitespace-nowrap cursor-pointer ${activeTab === "advanced"
                ? "border-purple-500 text-purple-600 dark:text-purple-400"
                : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                }`}
            >
              Advanced Params
            </button>
          </div>
        </div>

        {/* Tab Content */}
        <div className="flex-1 overflow-hidden">
          <ScrollArea className="h-full">
            <div className="px-6 py-4 space-y-4">
              {activeTab === "general" && (
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="model-name">Model Name</Label>
                    <Input
                      id="model-name"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder="e.g., GPT-4"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="model-id">Model ID</Label>
                    <Input
                      id="model-id"
                      value={formData.id}
                      onChange={(e) => setFormData({ ...formData, id: e.target.value })}
                      placeholder="e.g., gpt-4"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="description">Description</Label>
                    <Textarea
                      id="description"
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      placeholder="Brief description of the model"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="connection">Connection</Label>
                    <Select
                      value={formData.connection}
                      onValueChange={(value) => setFormData({ ...formData, connection: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="OpenAI">OpenAI</SelectItem>
                        <SelectItem value="Anthropic">Anthropic</SelectItem>
                        <SelectItem value="Google">Google</SelectItem>
                        <SelectItem value="Azure">Azure</SelectItem>
                        <SelectItem value="Local">Local (Ollama)</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  {/* API Configuration */}
                  <div className="space-y-2">
                    <Label htmlFor="api-key">API Key</Label>
                    <Input
                      id="api-key"
                      type="password"
                      value={apiKey}
                      onChange={(e) => setApiKey(e.target.value)}
                      placeholder="Enter your API key"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="endpoint">
                      Custom Endpoint <span className="text-gray-400">(optional)</span>
                    </Label>
                    <Input
                      id="endpoint"
                      value={endpoint}
                      onChange={(e) => setEndpoint(e.target.value)}
                      placeholder="https://api.example.com/v1"
                    />
                  </div>

                  {/* Test Connection Button */}
                  <div className="flex items-center gap-2">
                    <Button
                      type="button"
                      variant="outline"
                      onClick={handleTestConnection}
                      disabled={testingConnection || !apiKey}
                      className="gap-2"
                    >
                      {testingConnection ? (
                        <>
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Testing...
                        </>
                      ) : (
                        <>
                          <Zap className="h-4 w-4" />
                          Test Connection
                        </>
                      )}
                    </Button>
                    {connectionStatus === "success" && (
                      <div className="flex items-center gap-1 text-green-600 dark:text-green-400">
                        <CheckCircle2 className="h-4 w-4" />
                        <span className="text-sm">Connected</span>
                      </div>
                    )}
                    {connectionStatus === "error" && (
                      <div className="flex items-center gap-1 text-red-600 dark:text-red-400">
                        <XCircle className="h-4 w-4" />
                        <span className="text-sm">Failed</span>
                      </div>
                    )}
                  </div>

                  <Separator />

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="context-window">Context Window</Label>
                      <Input
                        id="context-window"
                        type="number"
                        value={formData.contextWindow}
                        onChange={(e) => setFormData({ ...formData, contextWindow: parseInt(e.target.value) })}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="max-tokens">Max Tokens</Label>
                      <Input
                        id="max-tokens"
                        type="number"
                        value={formData.maxTokens}
                        onChange={(e) => setFormData({ ...formData, maxTokens: parseInt(e.target.value) })}
                      />
                    </div>
                  </div>

                  <Separator />

                  {/* Capabilities */}
                  <div className="space-y-3">
                    <Label>Capabilities</Label>
                    <div className="grid grid-cols-2 gap-2">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="vision"
                          checked={capabilities.vision}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, vision: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="vision" className="cursor-pointer">
                          Vision
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="file-upload"
                          checked={capabilities.fileUpload}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, fileUpload: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="file-upload" className="cursor-pointer">
                          File Upload
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="web-search"
                          checked={capabilities.webSearch}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, webSearch: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="web-search" className="cursor-pointer">
                          Web Search
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="image-gen"
                          checked={capabilities.imageGeneration}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, imageGeneration: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="image-gen" className="cursor-pointer">
                          Image Generation
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="code-interpreter"
                          checked={capabilities.codeInterpreter}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, codeInterpreter: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="code-interpreter" className="cursor-pointer">
                          Code Interpreter
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="usage-cap"
                          checked={capabilities.usage}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, usage: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="usage-cap" className="cursor-pointer">
                          Usage
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="citations-cap"
                          checked={capabilities.citations}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, citations: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="citations-cap" className="cursor-pointer">
                          Citations
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="status-cap"
                          checked={capabilities.statusUpdates}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, statusUpdates: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="status-cap" className="cursor-pointer">
                          Status Updates
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="structured-cap"
                          checked={capabilities.structuredContent}
                          onCheckedChange={(checked) =>
                            setCapabilities({ ...capabilities, structuredContent: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="structured-cap" className="cursor-pointer">
                          Structured Content
                        </Label>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Default Features */}
                  <div className="space-y-3">
                    <Label>Default Features</Label>
                    <div className="flex items-center gap-2">
                      <Checkbox
                        id="anonymous-chat"
                        checked={defaultFeatures.anonymousChat}
                        onCheckedChange={(checked) =>
                          setDefaultFeatures({ ...defaultFeatures, anonymousChat: checked as boolean })
                        }
                        className="cursor-pointer"
                      />
                      <Label htmlFor="anonymous-chat" className="cursor-pointer">
                        Anonymous Chat
                      </Label>
                    </div>
                  </div>
                </div>
              )}

              {activeTab === "params" && (
                <div className="space-y-4">
                  {/* System Prompt */}
                  <div className="space-y-2">
                    <Label>System Prompt</Label>
                    <Textarea
                      value={systemPrompt}
                      onChange={(e) => setSystemPrompt(e.target.value)}
                      className="min-h-[120px] font-mono text-sm"
                    />
                  </div>

                  <Separator />

                  {/* Prompt Suggestions */}
                  <div className="space-y-2">
                    <Label>Prompt Suggestions</Label>
                    <Select value={promptSuggestions} onValueChange={setPromptSuggestions}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Default">Default</SelectItem>
                        <SelectItem value="Custom">Custom</SelectItem>
                        <SelectItem value="None">None</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <Separator />

                  {/* Tools */}
                  <div className="space-y-3">
                    <Label>Tools</Label>
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="supreme-file"
                          checked={tools.supremeFileManagement}
                          onCheckedChange={(checked) =>
                            setTools({ ...tools, supremeFileManagement: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="supreme-file" className="cursor-pointer">
                          Supreme File Management
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="weather"
                          checked={tools.weatherForecast}
                          onCheckedChange={(checked) =>
                            setTools({ ...tools, weatherForecast: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="weather" className="cursor-pointer">
                          Weather Forecast
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="echo-mcp"
                          checked={tools.echoMCP}
                          onCheckedChange={(checked) =>
                            setTools({ ...tools, echoMCP: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="echo-mcp" className="cursor-pointer">
                          EchoMCP
                        </Label>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Filters */}
                  <div className="space-y-3">
                    <Label>Filters</Label>
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="autolearn"
                          checked={filters.autolearn}
                          onCheckedChange={(checked) =>
                            setFilters({ ...filters, autolearn: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="autolearn" className="cursor-pointer">
                          Autolearn
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="chat-metrics"
                          checked={filters.chatMetrics}
                          onCheckedChange={(checked) =>
                            setFilters({ ...filters, chatMetrics: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="chat-metrics" className="cursor-pointer">
                          Chat Metrics
                        </Label>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Actions */}
                  <div className="space-y-3">
                    <Label>Actions</Label>
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="add-memories"
                          checked={actions.addToMemories}
                          onCheckedChange={(checked) =>
                            setActions({ ...actions, addToMemories: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="add-memories" className="cursor-pointer">
                          Add to Memories
                        </Label>
                      </div>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          id="sora2"
                          checked={actions.sora2AsyncPromptHelper}
                          onCheckedChange={(checked) =>
                            setActions({ ...actions, sora2AsyncPromptHelper: checked as boolean })
                          }
                          className="cursor-pointer"
                        />
                        <Label htmlFor="sora2" className="cursor-pointer">
                          Sora 2 Async Prompt Helper
                        </Label>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {activeTab === "advanced" && (
                <div className="space-y-6">
                  {/* Temperature */}
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Label>Temperature</Label>
                      <Select value={temperatureState} onValueChange={(v: any) => setTemperatureState(v)}>
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="off">Off</SelectItem>
                          <SelectItem value="default">Default</SelectItem>
                          <SelectItem value="custom">Custom</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    {temperatureState === "custom" && (
                      <>
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-gray-500">Value: {temperature[0]}</span>
                        </div>
                        <Slider
                          value={temperature}
                          onValueChange={setTemperature}
                          min={0}
                          max={2}
                          step={0.1}
                          className="w-full"
                        />
                      </>
                    )}
                  </div>

                  {/* Top P */}
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Label>Top P</Label>
                      <Select value={topPState} onValueChange={(v: any) => setTopPState(v)}>
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="off">Off</SelectItem>
                          <SelectItem value="default">Default</SelectItem>
                          <SelectItem value="custom">Custom</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    {topPState === "custom" && (
                      <>
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-gray-500">Value: {topP[0]}</span>
                        </div>
                        <Slider
                          value={topP}
                          onValueChange={setTopP}
                          min={0}
                          max={1}
                          step={0.05}
                          className="w-full"
                        />
                      </>
                    )}
                  </div>

                  {/* Top K */}
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <Label>Top K</Label>
                      <Select value={topKState} onValueChange={(v: any) => setTopKState(v)}>
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="off">Off</SelectItem>
                          <SelectItem value="default">Default</SelectItem>
                          <SelectItem value="custom">Custom</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    {topKState === "custom" && (
                      <>
                        <div className="flex items-center justify-between">
                          <span className="text-sm text-gray-500">Value: {topK[0]}</span>
                        </div>
                        <Slider
                          value={topK}
                          onValueChange={setTopK}
                          min={0}
                          max={100}
                          step={1}
                          className="w-full"
                        />
                      </>
                    )}
                  </div>

                  <Separator />

                  {/* Additional Parameters */}
                  <Collapsible open={advancedParamsOpen} onOpenChange={setAdvancedParamsOpen}>
                    <CollapsibleTrigger className="flex items-center justify-between w-full py-2 cursor-pointer">
                      <Label className="cursor-pointer">Additional Parameters</Label>
                      {advancedParamsOpen ? (
                        <ChevronUp className="h-4 w-4" />
                      ) : (
                        <ChevronDown className="h-4 w-4" />
                      )}
                    </CollapsibleTrigger>
                    <CollapsibleContent className="space-y-4 pt-4">
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label>Seed</Label>
                          <Input value={seed} onChange={(e) => setSeed(e.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label>Max Tokens</Label>
                          <Input value={maxTokensValue} onChange={(e) => setMaxTokensValue(e.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label>Min P</Label>
                          <Input value={minPValue} onChange={(e) => setMinPValue(e.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label>Context Length</Label>
                          <Input value={contextLength} onChange={(e) => setContextLength(e.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label>Batch Size</Label>
                          <Input value={batchSize} onChange={(e) => setBatchSize(e.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label>Keep Alive</Label>
                          <Input value={keepAlive} onChange={(e) => setKeepAlive(e.target.value)} />
                        </div>
                      </div>

                      <div className="space-y-3">
                        <div className="flex items-center justify-between">
                          <Label>Frequency Penalty</Label>
                          <span className="text-sm text-gray-500">{frequencyPenalty[0]}</span>
                        </div>
                        <Slider
                          value={frequencyPenalty}
                          onValueChange={setFrequencyPenalty}
                          min={-2}
                          max={2}
                          step={0.1}
                          className="w-full"
                        />
                      </div>

                      <div className="space-y-3">
                        <div className="flex items-center justify-between">
                          <Label>Repeat Penalty</Label>
                          <span className="text-sm text-gray-500">{repeatPenalty[0]}</span>
                        </div>
                        <Slider
                          value={repeatPenalty}
                          onValueChange={setRepeatPenalty}
                          min={0}
                          max={2}
                          step={0.1}
                          className="w-full"
                        />
                      </div>
                    </CollapsibleContent>
                  </Collapsible>
                </div>
              )}
            </div>
          </ScrollArea>
        </div>

        <div className="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-between">
          {model && onDelete ? (
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