/**
 * Tool Call Card
 *
 * Displays a tool call with its status, parameters, and result
 */

import { useState } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import {
  Wrench,
  CheckCircle2,
  XCircle,
  Loader2,
  Clock,
  ChevronDown,
  ChevronUp,
  Copy,
  RefreshCw,
} from "lucide-react";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "./ui/collapsible";
import { ToolCall } from "../hooks/useTools";
import { toast } from "sonner";

interface ToolCallCardProps {
  toolCall: ToolCall;
  onRetry?: () => void;
}

const STATUS_CONFIG = {
  pending: {
    icon: Clock,
    color: 'text-yellow-600 dark:text-yellow-400',
    bg: 'bg-yellow-50 dark:bg-yellow-900/20',
    border: 'border-yellow-200 dark:border-yellow-800',
    label: 'Pending',
  },
  running: {
    icon: Loader2,
    color: 'text-blue-600 dark:text-blue-400',
    bg: 'bg-blue-50 dark:bg-blue-900/20',
    border: 'border-blue-200 dark:border-blue-800',
    label: 'Running',
  },
  completed: {
    icon: CheckCircle2,
    color: 'text-green-600 dark:text-green-400',
    bg: 'bg-green-50 dark:bg-green-900/20',
    border: 'border-green-200 dark:border-green-800',
    label: 'Completed',
  },
  failed: {
    icon: XCircle,
    color: 'text-red-600 dark:text-red-400',
    bg: 'bg-red-50 dark:bg-red-900/20',
    border: 'border-red-200 dark:border-red-800',
    label: 'Failed',
  },
};

export function ToolCallCard({ toolCall, onRetry }: ToolCallCardProps) {
  const [showParams, setShowParams] = useState(false);
  const [showResult, setShowResult] = useState(true);

  const config = STATUS_CONFIG[toolCall.status];
  const StatusIcon = config.icon;

  const handleCopyResult = () => {
    if (toolCall.result) {
      navigator.clipboard.writeText(JSON.stringify(toolCall.result.result, null, 2));
      toast.success('Result copied to clipboard');
    }
  };

  const executionTime = toolCall.result?.executionTime
    ? `${toolCall.result.executionTime}ms`
    : toolCall.completedAt && toolCall.startedAt
      ? `${new Date(toolCall.completedAt).getTime() - new Date(toolCall.startedAt).getTime()}ms`
      : null;

  return (
    <div className={`rounded-lg border ${config.border} ${config.bg} p-3 my-2`}>
      {/* Header */}
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Wrench className={`h-4 w-4 ${config.color}`} />
          <span className="font-mono text-sm font-medium">{toolCall.toolName}</span>
          <Badge variant="outline" className="text-xs">
            {toolCall.serverId}
          </Badge>
        </div>

        <div className="flex items-center gap-2">
          {executionTime && (
            <span className="text-xs text-gray-500">{executionTime}</span>
          )}
          <StatusIcon
            className={`h-4 w-4 ${config.color} ${toolCall.status === 'running' ? 'animate-spin' : ''}`}
          />
          <span className={`text-xs font-medium ${config.color}`}>
            {config.label}
          </span>
        </div>
      </div>

      {/* Parameters */}
      <Collapsible open={showParams} onOpenChange={setShowParams}>
        <CollapsibleTrigger asChild>
          <Button variant="ghost" size="sm" className="mt-2 h-7 text-xs gap-1 px-2">
            {showParams ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
            Parameters
          </Button>
        </CollapsibleTrigger>
        <CollapsibleContent>
          <pre className="mt-2 p-2 bg-gray-100 dark:bg-gray-800 rounded text-xs overflow-x-auto">
            {JSON.stringify(toolCall.parameters, null, 2)}
          </pre>
        </CollapsibleContent>
      </Collapsible>

      {/* Result */}
      {toolCall.status === 'completed' && toolCall.result && (
        <Collapsible open={showResult} onOpenChange={setShowResult}>
          <div className="flex items-center justify-between mt-2">
            <CollapsibleTrigger asChild>
              <Button variant="ghost" size="sm" className="h-7 text-xs gap-1 px-2">
                {showResult ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
                Result
              </Button>
            </CollapsibleTrigger>
            <Button variant="ghost" size="sm" className="h-7 text-xs gap-1 px-2" onClick={handleCopyResult}>
              <Copy className="h-3 w-3" />
              Copy
            </Button>
          </div>
          <CollapsibleContent>
            <pre className="mt-2 p-2 bg-gray-100 dark:bg-gray-800 rounded text-xs overflow-x-auto max-h-48">
              {JSON.stringify(toolCall.result.result, null, 2)}
            </pre>
          </CollapsibleContent>
        </Collapsible>
      )}

      {/* Error */}
      {toolCall.status === 'failed' && toolCall.error && (
        <div className="mt-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-red-600 dark:text-red-400">Error</span>
            {onRetry && (
              <Button
                variant="ghost"
                size="sm"
                className="h-7 text-xs gap-1 px-2"
                onClick={onRetry}
              >
                <RefreshCw className="h-3 w-3" />
                Retry
              </Button>
            )}
          </div>
          <p className="mt-1 p-2 bg-red-100 dark:bg-red-900/30 rounded text-xs text-red-700 dark:text-red-300">
            {toolCall.error}
          </p>
        </div>
      )}
    </div>
  );
}

/**
 * Parse and render tool calls from message content
 */
export function renderToolCalls(
  content: string,
  toolCalls: ToolCall[],
  onRetry?: (toolCall: ToolCall) => void
): React.ReactNode[] {
  const nodes: React.ReactNode[] = [];

  // Check for XML-style tool calls in content
  const xmlPattern = /<tool_call\s+name="([^"]+)"\s+server="([^"]+)">([\s\S]*?)<\/tool_call>/g;
  let lastIndex = 0;
  let match;
  let foundCalls = false;

  while ((match = xmlPattern.exec(content)) !== null) {
    foundCalls = true;

    // Add text before the tool call
    if (match.index > lastIndex) {
      nodes.push(
        <span key={`text-${lastIndex}`}>
          {content.slice(lastIndex, match.index)}
        </span>
      );
    }

    const [, toolName, serverId] = match;

    // Find matching tool call from state
    const toolCall = toolCalls.find(
      tc => tc.toolName === toolName && tc.serverId === serverId
    );

    if (toolCall) {
      nodes.push(
        <ToolCallCard
          key={toolCall.id}
          toolCall={toolCall}
          onRetry={onRetry ? () => onRetry(toolCall) : undefined}
        />
      );
    }

    lastIndex = match.index + match[0].length;
  }

  // Add remaining text
  if (foundCalls && lastIndex < content.length) {
    nodes.push(
      <span key={`text-${lastIndex}`}>
        {content.slice(lastIndex)}
      </span>
    );
  }

  // If no XML tool calls found, just render the tool call cards at the end
  if (!foundCalls && toolCalls.length > 0) {
    nodes.push(
      <span key="content">{content}</span>
    );
    for (const toolCall of toolCalls) {
      nodes.push(
        <ToolCallCard
          key={toolCall.id}
          toolCall={toolCall}
          onRetry={onRetry ? () => onRetry(toolCall) : undefined}
        />
      );
    }
  }

  return nodes.length > 0 ? nodes : [<span key="content">{content}</span>];
}
