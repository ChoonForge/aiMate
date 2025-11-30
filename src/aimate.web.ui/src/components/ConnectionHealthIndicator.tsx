/**
 * Connection Health Indicator
 *
 * Shows the status of the active LM server connection
 */

import { useState, useEffect, useCallback } from "react";
import { Wifi, WifiOff, Loader2, AlertCircle, CheckCircle2, Server } from "lucide-react";
import { Badge } from "./ui/badge";
import { Button } from "./ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "./ui/popover";
import { useAdminSettings } from "../context/AdminSettingsContext";
import { AppConfig } from "../utils/config";

type ConnectionStatus = 'connected' | 'connecting' | 'disconnected' | 'error' | 'none';

interface ConnectionHealth {
  status: ConnectionStatus;
  message: string;
  connectionName?: string;
  connectionUrl?: string;
  lastChecked?: Date;
  modelCount?: number;
}

export function ConnectionHealthIndicator() {
  const { settings } = useAdminSettings();
  const [health, setHealth] = useState<ConnectionHealth>({
    status: 'none',
    message: 'No connection configured',
  });
  const [checking, setChecking] = useState(false);

  // Get active LM server connection
  const getActiveConnection = useCallback(() => {
    const connections = settings.connections || [];
    return connections.find(c => c.enabled && c.url);
  }, [settings.connections]);

  // Check connection health
  const checkConnection = useCallback(async () => {
    const connection = getActiveConnection();

    if (!connection) {
      setHealth({
        status: 'none',
        message: 'No connection configured',
      });
      return;
    }

    setChecking(true);
    setHealth(prev => ({
      ...prev,
      status: 'connecting',
      message: 'Checking connection...',
      connectionName: connection.name,
      connectionUrl: connection.url,
    }));

    try {
      const modelsUrl = connection.url.replace(/\/$/, '') + '/models';
      const headers: HeadersInit = {
        'Content-Type': 'application/json',
      };
      if (connection.apiKey) {
        headers['Authorization'] = `Bearer ${connection.apiKey}`;
      }

      const response = await fetch(modelsUrl, {
        method: 'GET',
        headers,
        signal: AbortSignal.timeout(5000), // 5 second timeout
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      const modelCount = data.data?.length || 0;

      setHealth({
        status: 'connected',
        message: `Connected - ${modelCount} model${modelCount !== 1 ? 's' : ''} available`,
        connectionName: connection.name,
        connectionUrl: connection.url,
        lastChecked: new Date(),
        modelCount,
      });
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Connection failed';
      setHealth({
        status: 'error',
        message: errorMessage,
        connectionName: connection.name,
        connectionUrl: connection.url,
        lastChecked: new Date(),
      });
    } finally {
      setChecking(false);
    }
  }, [getActiveConnection]);

  // Check on mount and when connections change
  useEffect(() => {
    const connection = getActiveConnection();
    if (connection) {
      checkConnection();
    } else {
      setHealth({
        status: 'none',
        message: 'No connection configured',
      });
    }
  }, [settings.connections, getActiveConnection, checkConnection]);

  // Don't show if in offline mode (OfflineModeIndicator handles that)
  if (AppConfig.isOfflineMode() && health.status === 'none') {
    return null;
  }

  const statusConfig = {
    connected: {
      icon: CheckCircle2,
      color: 'text-green-600 dark:text-green-400',
      bgColor: 'bg-green-100 dark:bg-green-900/20',
      badgeClass: 'border-green-600 dark:border-green-400 text-green-600 dark:text-green-400',
      label: 'Connected',
    },
    connecting: {
      icon: Loader2,
      color: 'text-blue-600 dark:text-blue-400',
      bgColor: 'bg-blue-100 dark:bg-blue-900/20',
      badgeClass: 'border-blue-600 dark:border-blue-400 text-blue-600 dark:text-blue-400',
      label: 'Connecting',
    },
    disconnected: {
      icon: WifiOff,
      color: 'text-gray-600 dark:text-gray-400',
      bgColor: 'bg-gray-100 dark:bg-gray-900/20',
      badgeClass: 'border-gray-600 dark:border-gray-400 text-gray-600 dark:text-gray-400',
      label: 'Disconnected',
    },
    error: {
      icon: AlertCircle,
      color: 'text-red-600 dark:text-red-400',
      bgColor: 'bg-red-100 dark:bg-red-900/20',
      badgeClass: 'border-red-600 dark:border-red-400 text-red-600 dark:text-red-400',
      label: 'Error',
    },
    none: {
      icon: Server,
      color: 'text-gray-500 dark:text-gray-500',
      bgColor: 'bg-gray-100 dark:bg-gray-900/20',
      badgeClass: 'border-gray-500 dark:border-gray-500 text-gray-500 dark:text-gray-500',
      label: 'No Server',
    },
  };

  const config = statusConfig[health.status];
  const StatusIcon = config.icon;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          size="sm"
          className={`gap-2 ${config.color} hover:opacity-80`}
        >
          <StatusIcon className={`w-4 h-4 ${health.status === 'connecting' ? 'animate-spin' : ''}`} />
          <span className="text-xs font-medium hidden sm:inline">
            {health.connectionName || config.label}
          </span>
          {health.status === 'connected' && (
            <span className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80" align="end">
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <div className={`w-8 h-8 rounded-full ${config.bgColor} flex items-center justify-center shrink-0`}>
              <StatusIcon className={`w-4 h-4 ${config.color} ${health.status === 'connecting' ? 'animate-spin' : ''}`} />
            </div>
            <div className="flex-1 space-y-1">
              <h4 className="font-medium text-sm">
                {health.connectionName || 'LM Server Connection'}
              </h4>
              <p className="text-xs text-gray-600 dark:text-gray-400">
                {health.message}
              </p>
            </div>
          </div>

          {health.connectionUrl && (
            <div className="text-xs text-gray-600 dark:text-gray-400 space-y-1">
              <p>
                <span className="font-medium text-gray-900 dark:text-gray-100">Endpoint:</span>{' '}
                <code className="px-1 py-0.5 bg-gray-100 dark:bg-gray-800 rounded">
                  {health.connectionUrl}
                </code>
              </p>
              {health.lastChecked && (
                <p>
                  <span className="font-medium text-gray-900 dark:text-gray-100">Last checked:</span>{' '}
                  {health.lastChecked.toLocaleTimeString()}
                </p>
              )}
            </div>
          )}

          <div className="pt-2 border-t border-gray-200 dark:border-gray-800 flex justify-between items-center">
            <Button
              variant="ghost"
              size="sm"
              onClick={checkConnection}
              disabled={checking}
              className="text-xs"
            >
              {checking ? (
                <>
                  <Loader2 className="w-3 h-3 mr-1 animate-spin" />
                  Checking...
                </>
              ) : (
                'Check Now'
              )}
            </Button>
            {health.status === 'connected' && health.modelCount !== undefined && (
              <Badge variant="outline" className={config.badgeClass}>
                {health.modelCount} models
              </Badge>
            )}
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
}
