import { useDebug } from "./DebugContext";
import { ScrollArea } from "./ui/scroll-area";
import { Button } from "./ui/button";
import { Trash2, Bug, ChevronDown, ChevronUp, Camera } from "lucide-react";
import { useState, useEffect, useRef } from "react";

export function DebugPanel() {
  const { debugEnabled, showcaseMode, setShowcaseMode, logs, clearLogs } = useDebug();
  const [isExpanded, setIsExpanded] = useState(true);
  const [height, setHeight] = useState(200);
  const [isResizing, setIsResizing] = useState(false);
  const previousLogsLength = useRef(logs.length);

  // Auto-expand when new logs arrive and panel is minimized
  useEffect(() => {
    if (logs.length > previousLogsLength.current && !isExpanded) {
      setIsExpanded(true);
    }
    previousLogsLength.current = logs.length;
  }, [logs.length, isExpanded]);

  // Handle resize
  useEffect(() => {
    if (!isResizing) return;

    const handleMouseMove = (e: MouseEvent) => {
      const newHeight = window.innerHeight - e.clientY;
      setHeight(Math.max(100, Math.min(600, newHeight)));
    };

    const handleMouseUp = () => {
      setIsResizing(false);
    };

    document.addEventListener("mousemove", handleMouseMove);
    document.addEventListener("mouseup", handleMouseUp);

    return () => {
      document.removeEventListener("mousemove", handleMouseMove);
      document.removeEventListener("mouseup", handleMouseUp);
    };
  }, [isResizing]);

  if (!debugEnabled) return null;

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString("en-US", {
      hour12: false,
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      fractionalSecondDigits: 3,
    });
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case "success":
        return "text-green-500 dark:text-green-400";
      case "error":
        return "text-red-500 dark:text-red-400";
      case "warning":
        return "text-yellow-500 dark:text-yellow-400";
      default:
        return "text-blue-500 dark:text-blue-400";
    }
  };

  return (
    <div className="border-t border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50">
      {/* Resize Handle */}
      <div
        className="h-1 bg-gray-200 dark:bg-gray-800 hover:bg-purple-500 dark:hover:bg-purple-500 transition-colors cursor-ns-resize"
        onMouseDown={() => setIsResizing(true)}
      />
      
      <div 
        className="flex items-center justify-between px-4 py-2 border-b border-gray-200 dark:border-gray-800 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800/50 transition-colors"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <div className="flex items-center gap-2">
          <Bug className="h-4 w-4 text-purple-500" />
          <span className="font-medium text-sm">Debug Console</span>
          <span className="text-xs text-gray-500 dark:text-gray-400">
            ({logs.length} {logs.length === 1 ? "entry" : "entries"})
          </span>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              setShowcaseMode(!showcaseMode);
            }}
            className={`h-8 px-2 cursor-pointer ${showcaseMode ? 'bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400' : ''}`}
          >
            <Camera className="h-3 w-3 mr-1" />
            Showcase
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              clearLogs();
            }}
            className="h-8 px-2 cursor-pointer"
          >
            <Trash2 className="h-3 w-3 mr-1" />
            Clear
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              setIsExpanded(!isExpanded);
            }}
            className="h-8 px-2 cursor-pointer"
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronUp className="h-4 w-4" />
            )}
          </Button>
        </div>
      </div>

      {isExpanded && (
        <ScrollArea style={{ height: `${height}px` }}>
          <div className="px-4 py-2 space-y-1 font-mono text-xs">
            {logs.length === 0 ? (
              <div className="text-gray-500 dark:text-gray-400 text-center py-8">
                No debug logs yet
              </div>
            ) : (
              logs.map((log) => (
                <div
                  key={log.id}
                  className="py-1 border-b border-gray-200 dark:border-gray-800 last:border-0"
                >
                  <div className="flex items-start gap-2">
                    <span className="text-gray-500 dark:text-gray-400 shrink-0">
                      [{formatTime(log.timestamp)}]
                    </span>
                    <span
                      className={`${getTypeColor(log.type)} shrink-0 uppercase`}
                    >
                      {log.type}
                    </span>
                    <span className="text-gray-700 dark:text-gray-300">
                      {log.action}
                    </span>
                  </div>
                  {log.api && (
                    <div className="ml-24 text-purple-600 dark:text-purple-400">
                      → {log.api}
                    </div>
                  )}
                  {log.payload && (
                    <div className="ml-24 text-gray-600 dark:text-gray-400">
                      <details className="cursor-pointer">
                        <summary className="hover:text-gray-800 dark:hover:text-gray-200 cursor-pointer">
                          Payload
                        </summary>
                        <pre className="mt-1 text-[10px] overflow-x-auto bg-gray-100 dark:bg-gray-800 p-2 rounded">
                          {JSON.stringify(log.payload, null, 2)}
                        </pre>
                      </details>
                    </div>
                  )}
                </div>
              ))
            )}
          </div>
        </ScrollArea>
      )}
    </div>
  );
}