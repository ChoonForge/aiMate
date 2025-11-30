/**
 * Tool Call Display Component
 *
 * Displays MCP tool calls and their results in chat messages
 */

import { useState } from 'react';
import { cn } from './ui/utils';
import { Button } from './ui/button';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from './ui/collapsible';
import {
  Wrench,
  ChevronDown,
  ChevronRight,
  CheckCircle2,
  XCircle,
  Loader2,
  Clock,
  Copy,
  ExternalLink,
} from 'lucide-react';
import { toast } from 'sonner';

// ============================================================================
// TYPES
// ============================================================================

export interface ToolCall {
  id: string;
  name: string;
  parameters: Record<string, any>;
  status: 'pending' | 'executing' | 'success' | 'error';
  result?: any;
  error?: string;
  executionTime?: number;
}

interface ToolCallDisplayProps {
  toolCall: ToolCall;
  className?: string;
}

interface ToolCallsListProps {
  toolCalls: ToolCall[];
  className?: string;
}

// ============================================================================
// TOOL CALL DISPLAY COMPONENT
// ============================================================================

export function ToolCallDisplay({ toolCall, className }: ToolCallDisplayProps) {
  const [isOpen, setIsOpen] = useState(false);

  const statusIcon = {
    pending: <Clock className="h-4 w-4 text-gray-400" />,
    executing: <Loader2 className="h-4 w-4 text-blue-500 animate-spin" />,
    success: <CheckCircle2 className="h-4 w-4 text-green-500" />,
    error: <XCircle className="h-4 w-4 text-red-500" />,
  };

  const statusColors = {
    pending: 'bg-gray-50 dark:bg-gray-900 border-gray-200 dark:border-gray-800',
    executing: 'bg-blue-50 dark:bg-blue-950/30 border-blue-200 dark:border-blue-800',
    success: 'bg-green-50 dark:bg-green-950/30 border-green-200 dark:border-green-800',
    error: 'bg-red-50 dark:bg-red-950/30 border-red-200 dark:border-red-800',
  };

  const handleCopyResult = () => {
    const resultText = JSON.stringify(toolCall.result, null, 2);
    navigator.clipboard.writeText(resultText);
    toast.success('Result copied to clipboard');
  };

  return (
    <Collapsible open={isOpen} onOpenChange={setIsOpen}>
      <div
        className={cn(
          'rounded-lg border overflow-hidden',
          statusColors[toolCall.status],
          className
        )}
      >
        {/* Header */}
        <CollapsibleTrigger asChild>
          <button className="w-full flex items-center gap-3 p-3 hover:bg-black/5 dark:hover:bg-white/5 transition-colors text-left">
            <div className="flex-shrink-0">
              <Wrench className="h-4 w-4 text-gray-500" />
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <span className="font-medium text-sm text-gray-900 dark:text-gray-100">
                  {formatToolName(toolCall.name)}
                </span>
                {toolCall.executionTime && (
                  <span className="text-xs text-gray-500">
                    {toolCall.executionTime}ms
                  </span>
                )}
              </div>
              {!isOpen && toolCall.status === 'success' && toolCall.result && (
                <p className="text-xs text-gray-500 truncate mt-0.5">
                  {getResultPreview(toolCall.result)}
                </p>
              )}
            </div>
            <div className="flex items-center gap-2 flex-shrink-0">
              {statusIcon[toolCall.status]}
              {isOpen ? (
                <ChevronDown className="h-4 w-4 text-gray-400" />
              ) : (
                <ChevronRight className="h-4 w-4 text-gray-400" />
              )}
            </div>
          </button>
        </CollapsibleTrigger>

        {/* Content */}
        <CollapsibleContent>
          <div className="border-t border-inherit">
            {/* Parameters */}
            <div className="p-3 bg-black/5 dark:bg-white/5">
              <div className="text-xs font-medium text-gray-500 mb-2">Parameters</div>
              <pre className="text-xs text-gray-700 dark:text-gray-300 overflow-x-auto">
                {JSON.stringify(toolCall.parameters, null, 2)}
              </pre>
            </div>

            {/* Result or Error */}
            {toolCall.status === 'success' && toolCall.result && (
              <div className="p-3">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs font-medium text-gray-500">Result</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2"
                    onClick={handleCopyResult}
                  >
                    <Copy className="h-3 w-3 mr-1" />
                    Copy
                  </Button>
                </div>
                <div className="text-sm text-gray-700 dark:text-gray-300">
                  {renderToolResult(toolCall.name, toolCall.result)}
                </div>
              </div>
            )}

            {toolCall.status === 'error' && toolCall.error && (
              <div className="p-3">
                <div className="text-xs font-medium text-red-500 mb-2">Error</div>
                <p className="text-sm text-red-600 dark:text-red-400">
                  {toolCall.error}
                </p>
              </div>
            )}

            {toolCall.status === 'executing' && (
              <div className="p-3 flex items-center gap-2 text-sm text-blue-600 dark:text-blue-400">
                <Loader2 className="h-4 w-4 animate-spin" />
                Executing tool...
              </div>
            )}
          </div>
        </CollapsibleContent>
      </div>
    </Collapsible>
  );
}

// ============================================================================
// TOOL CALLS LIST COMPONENT
// ============================================================================

export function ToolCallsList({ toolCalls, className }: ToolCallsListProps) {
  if (!toolCalls || toolCalls.length === 0) return null;

  return (
    <div className={cn('space-y-2', className)}>
      {toolCalls.map((toolCall) => (
        <ToolCallDisplay key={toolCall.id} toolCall={toolCall} />
      ))}
    </div>
  );
}

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

function formatToolName(name: string): string {
  return name
    .replace(/_/g, ' ')
    .replace(/\b\w/g, (c) => c.toUpperCase());
}

function getResultPreview(result: any): string {
  if (typeof result === 'string') {
    return result.length > 100 ? result.substring(0, 100) + '...' : result;
  }
  if (typeof result === 'object') {
    const str = JSON.stringify(result);
    return str.length > 100 ? str.substring(0, 100) + '...' : str;
  }
  return String(result);
}

function renderToolResult(toolName: string, result: any): React.ReactNode {
  // Custom rendering for specific tools
  switch (toolName) {
    case 'web_search':
      return renderSearchResults(result);
    case 'get_weather':
      return renderWeatherResult(result);
    case 'generate_image':
      return renderImageResult(result);
    case 'read_url':
      return renderUrlResult(result);
    default:
      return (
        <pre className="text-xs overflow-x-auto bg-gray-100 dark:bg-gray-800 p-2 rounded">
          {JSON.stringify(result, null, 2)}
        </pre>
      );
  }
}

function renderSearchResults(result: any): React.ReactNode {
  if (!result?.results || !Array.isArray(result.results)) {
    return <pre className="text-xs">{JSON.stringify(result, null, 2)}</pre>;
  }

  return (
    <div className="space-y-2">
      {result.results.map((item: any, index: number) => (
        <div
          key={index}
          className="p-2 bg-gray-100 dark:bg-gray-800 rounded text-xs"
        >
          <a
            href={item.url}
            target="_blank"
            rel="noopener noreferrer"
            className="font-medium text-blue-600 dark:text-blue-400 hover:underline flex items-center gap-1"
          >
            {item.title}
            <ExternalLink className="h-3 w-3" />
          </a>
          <p className="text-gray-600 dark:text-gray-400 mt-1">{item.snippet}</p>
        </div>
      ))}
    </div>
  );
}

function renderWeatherResult(result: any): React.ReactNode {
  return (
    <div className="flex items-center gap-4 p-3 bg-gradient-to-r from-blue-100 to-sky-100 dark:from-blue-900/30 dark:to-sky-900/30 rounded-lg">
      <div className="text-3xl font-bold text-gray-800 dark:text-gray-100">
        {result.temperature}°
      </div>
      <div>
        <div className="font-medium text-gray-800 dark:text-gray-100">
          {result.location}
        </div>
        <div className="text-sm text-gray-600 dark:text-gray-400">
          {result.conditions} • {result.humidity}% humidity
        </div>
        <div className="text-xs text-gray-500">{result.wind}</div>
      </div>
    </div>
  );
}

function renderImageResult(result: any): React.ReactNode {
  return (
    <div className="space-y-2">
      <img
        src={result.imageUrl}
        alt={result.prompt}
        className="rounded-lg max-w-full h-auto"
      />
      <p className="text-xs text-gray-500 italic">"{result.prompt}"</p>
    </div>
  );
}

function renderUrlResult(result: any): React.ReactNode {
  return (
    <div className="space-y-2">
      <div className="flex items-center gap-2">
        <a
          href={result.url}
          target="_blank"
          rel="noopener noreferrer"
          className="font-medium text-blue-600 dark:text-blue-400 hover:underline flex items-center gap-1"
        >
          {result.title}
          <ExternalLink className="h-3 w-3" />
        </a>
      </div>
      <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-3">
        {result.content}
      </p>
      <div className="text-xs text-gray-500">{result.wordCount} words</div>
    </div>
  );
}

export default ToolCallDisplay;
