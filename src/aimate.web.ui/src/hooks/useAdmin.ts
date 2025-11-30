/**
 * Admin Data Hook
 *
 * Manages admin data fetching, caching, and mutations.
 * In offline mode, uses AdminSettingsContext for localStorage persistence.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  adminService,
  connectionsService,
  ModelDto,
  ConnectionDto,
  McpServerDto,
  AdminDashboardDto,
} from '../api/services';
import { AppConfig } from '../utils/config';
import { useAdminSettings, Connection as AdminConnection, Model as AdminModel } from '../context/AdminSettingsContext';

// Storage key for offline admin data
const OFFLINE_STORAGE_KEY = 'aimate-admin-offline-data';

// Helper to convert AdminSettingsContext Connection to ConnectionDto
function toConnectionDto(conn: AdminConnection): ConnectionDto {
  return {
    id: conn.id,
    provider: (conn.type as ConnectionDto['provider']) || 'Custom',
    name: conn.name,
    isActive: conn.enabled,
    apiKeyPrefix: conn.apiKey ? conn.apiKey.substring(0, 10) + '...' : '',
    createdAt: new Date().toISOString(),
    models: [],
    enabled: conn.enabled,
    url: conn.url,
  };
}

// Helper to convert AdminSettingsContext Model to ModelDto
function toModelDto(model: AdminModel): ModelDto {
  return {
    id: model.id,
    name: model.name,
    provider: model.connection || 'Custom',
    isActive: true,
    contextWindow: 4096,
    color: model.color,
    capabilities: ['chat'],
  };
}

// Helper to convert ConnectionDto to AdminConnection
function toAdminConnection(dto: ConnectionDto): AdminConnection {
  return {
    id: dto.id,
    name: dto.name,
    type: dto.provider,
    url: (dto as any).url || '',
    enabled: dto.isActive ?? dto.enabled ?? true,
    apiKey: dto.apiKeyPrefix,
  };
}

// Helper to convert ModelDto to AdminModel
function toAdminModel(dto: ModelDto): AdminModel {
  return {
    id: dto.id,
    name: dto.name,
    color: dto.color || 'text-gray-500',
    description: '',
    connection: dto.provider,
  };
}

// Helper to extract apiKey from headers JSON string (used by ConnectionEditDialog)
function extractApiKeyFromHeaders(data: any): string | undefined {
  // If apiKey is directly provided, use it
  if (data.apiKey) return data.apiKey;

  // Try to extract from headers JSON
  if (data.headers && (data.auth === 'Bearer Token' || data.auth === 'API Key')) {
    try {
      const headers = typeof data.headers === 'string' ? JSON.parse(data.headers) : data.headers;
      return headers.Authorization?.replace('Bearer ', '') || headers['api-key'] || headers['x-api-key'];
    } catch {
      // Headers not valid JSON, ignore
    }
  }
  return undefined;
}

export function useAdmin() {
  const adminSettings = useAdminSettings();
  const [dashboard, setDashboard] = useState<AdminDashboardDto | null>(null);
  const [models, setModels] = useState<ModelDto[]>([]);
  const [connections, setConnections] = useState<ConnectionDto[]>([]);
  const [mcpServers, setMcpServers] = useState<McpServerDto[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ============================================================================
  // DASHBOARD
  // ============================================================================

  const loadDashboard = useCallback(async () => {
    if (AppConfig.isOfflineMode()) {
      // Use mock data in offline mode
      setDashboard({
        totalUsers: 3,
        activeUsers: 2,
        totalWorkspaces: 5,
        totalConversations: 12,
        totalMessages: 234,
        systemHealth: 'Healthy',
        uptime: 99.9,
        lastBackup: new Date().toISOString(),
        totalTokensUsed: 125000,
        totalCostTracked: 2.50,
      });
      return;
    }

    try {
      const data = await adminService.getDashboard();
      setDashboard(data);
    } catch (err) {
      console.error('[useAdmin] Failed to load dashboard:', err);
      // Fallback to mock data on error
      setDashboard({
        totalUsers: 0,
        activeUsers: 0,
        totalWorkspaces: 0,
        totalConversations: 0,
        totalMessages: 0,
        systemHealth: 'Degraded',
        uptime: 0,
        lastBackup: new Date().toISOString(),
        totalTokensUsed: 0,
        totalCostTracked: 0,
      });
    }
  }, []);

  // ============================================================================
  // MODELS
  // ============================================================================

  const loadModels = useCallback(async () => {
    // Always load from localStorage first (AdminSettingsContext)
    const settingsModels = adminSettings.settings.models;
    const localModelDtos = settingsModels.map(toModelDto);
    setModels(localModelDtos);

    // If not offline, also try to load from backend and merge
    if (!AppConfig.isOfflineMode()) {
      try {
        const backendModels = await adminService.getModels();
        // Merge: keep local models, add any from backend that don't exist locally
        const localIds = new Set(localModelDtos.map(m => m.id));
        const newBackendModels = backendModels.filter(m => !localIds.has(m.id));
        if (newBackendModels.length > 0) {
          setModels([...localModelDtos, ...newBackendModels]);
        }
      } catch (err) {
        console.warn('[useAdmin] Failed to load models from backend, using localStorage:', err);
      }
    }
  }, [adminSettings.settings.models]);

  const toggleModel = useCallback(async (modelId: string) => {
    // Optimistic update
    setModels(prev => prev.map(m => 
      m.id === modelId ? { ...m, isActive: !m.isActive } : m
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await adminService.toggleModel(modelId);
      await loadModels(); // Refresh
    } catch (err) {
      console.error('[useAdmin] Failed to toggle model:', err);
      // Revert optimistic update
      await loadModels();
    }
  }, [loadModels]);

  const createModel = useCallback(async (data: Partial<ModelDto>) => {
    if (AppConfig.isOfflineMode()) {
      const newId = data.id || `model-${Date.now()}`;
      const newModel: ModelDto = {
        id: newId,
        name: data.name || 'New Model',
        provider: data.provider || 'Custom',
        isActive: data.isActive ?? true,
        contextWindow: data.contextWindow || 4096,
        color: data.color || 'text-gray-500',
        capabilities: data.capabilities || ['chat'],
        ...data,
      };
      setModels(prev => [...prev, newModel]);

      // Persist to AdminSettingsContext
      const newAdminModel: AdminModel = {
        id: newId,
        name: data.name || 'New Model',
        color: data.color || 'text-gray-500',
        description: '',
        connection: data.provider || 'Custom',
      };
      adminSettings.updateModels([...adminSettings.settings.models, newAdminModel]);

      return newModel;
    }

    try {
      const model = await adminService.createModel(data);
      await loadModels();
      return model;
    } catch (err) {
      console.error('[useAdmin] Failed to create model:', err);
      throw err;
    }
  }, [loadModels, adminSettings]);

  const updateModel = useCallback(async (modelId: string, data: Partial<ModelDto>) => {
    // Optimistic update
    setModels(prev => prev.map(m =>
      m.id === modelId ? { ...m, ...data } : m
    ));

    if (AppConfig.isOfflineMode()) {
      // Persist to AdminSettingsContext
      const updatedModels = adminSettings.settings.models.map(m =>
        m.id === modelId
          ? {
              ...m,
              name: data.name ?? m.name,
              color: data.color ?? m.color,
              connection: data.provider ?? m.connection,
            }
          : m
      );
      adminSettings.updateModels(updatedModels);
      return;
    }

    try {
      await adminService.updateModel(modelId, data);
      await loadModels();
    } catch (err) {
      console.error('[useAdmin] Failed to update model:', err);
      await loadModels(); // Revert
      throw err;
    }
  }, [loadModels, adminSettings]);

  const deleteModel = useCallback(async (modelId: string) => {
    // Optimistic update
    setModels(prev => prev.filter(m => m.id !== modelId));

    if (AppConfig.isOfflineMode()) {
      // Persist to AdminSettingsContext
      const updatedModels = adminSettings.settings.models.filter(m => m.id !== modelId);
      adminSettings.updateModels(updatedModels);
      return;
    }

    try {
      await adminService.deleteModel(modelId);
    } catch (err) {
      console.error('[useAdmin] Failed to delete model:', err);
      await loadModels(); // Revert
      throw err;
    }
  }, [loadModels, adminSettings]);

  // Add multiple models from a connection (works in all modes - persists to localStorage)
  const addModelsFromConnection = useCallback((modelsToAdd: Array<{ id: string; name: string; connectionName: string }>) => {
    console.log('[useAdmin] addModelsFromConnection called with:', modelsToAdd);

    // Filter out models that already exist
    const existingIds = new Set(adminSettings.settings.models.map(m => m.id));
    const newModels = modelsToAdd.filter(m => !existingIds.has(m.id));

    if (newModels.length === 0) {
      console.log('[useAdmin] All models already exist, skipping');
      return;
    }

    // Add to AdminSettingsContext (localStorage)
    const newAdminModels = newModels.map(m => ({
      id: m.id,
      name: m.name,
      color: 'text-cyan-500',
      description: `From ${m.connectionName}`,
      connection: m.connectionName,
    }));
    const updatedModels = [...adminSettings.settings.models, ...newAdminModels];
    console.log('[useAdmin] Persisting models to localStorage:', updatedModels);
    adminSettings.updateModels(updatedModels);

    // Also update local state
    const newModelDtos = newAdminModels.map(toModelDto);
    setModels(prev => [...prev, ...newModelDtos]);

    console.log(`[useAdmin] Added ${newModels.length} models from connection`);
  }, [adminSettings]);

  // ============================================================================
  // CONNECTIONS
  // ============================================================================

  const loadConnections = useCallback(async () => {
    // Always load from localStorage first (AdminSettingsContext)
    const settingsConnections = adminSettings.settings.connections;
    const localConnectionDtos = settingsConnections.map(toConnectionDto);
    setConnections(localConnectionDtos);

    // If not offline, also try to load from backend and merge
    if (!AppConfig.isOfflineMode()) {
      try {
        const backendConnections = await connectionsService.getConnections();
        // Merge: keep local connections, add any from backend that don't exist locally
        const localIds = new Set(localConnectionDtos.map(c => c.id));
        const newBackendConnections = backendConnections.filter(c => !localIds.has(c.id));
        if (newBackendConnections.length > 0) {
          setConnections([...localConnectionDtos, ...newBackendConnections]);
        }
      } catch (err) {
        console.warn('[useAdmin] Failed to load connections from backend, using localStorage:', err);
      }
    }
  }, [adminSettings.settings.connections]);

  const toggleConnection = useCallback(async (connectionId: string) => {
    // Optimistic update
    let wasActive = false;
    setConnections(prev => {
      const connection = prev.find(c => c.id === connectionId);
      wasActive = connection?.isActive || false;
      return prev.map(c =>
        c.id === connectionId ? { ...c, isActive: !c.isActive, enabled: !c.isActive } : c
      );
    });

    if (AppConfig.isOfflineMode()) {
      // Persist to AdminSettingsContext
      const updatedConnections = adminSettings.settings.connections.map(c =>
        c.id === connectionId ? { ...c, enabled: !c.enabled } : c
      );
      adminSettings.updateConnections(updatedConnections);
      return;
    }

    try {
      if (wasActive) {
        await connectionsService.deactivateConnection(connectionId);
      } else {
        await connectionsService.activateConnection(connectionId);
      }
      await loadConnections();
    } catch (err) {
      console.error('[useAdmin] Failed to toggle connection:', err);
      await loadConnections(); // Revert
    }
  }, [loadConnections, adminSettings]);

  const createConnection = useCallback(async (data: any) => {
    console.log('[useAdmin] createConnection called with:', data);

    const newId = `conn-${Date.now()}`;
    const apiKey = extractApiKeyFromHeaders(data);

    // Always create local connection first
    const newConnection: ConnectionDto = {
      id: newId,
      provider: data.provider || data.providerType || 'Custom',
      name: data.name,
      isActive: data.isActive ?? data.enabled ?? true,
      apiKeyPrefix: apiKey ? apiKey.substring(0, 10) + '...' : '',
      createdAt: new Date().toISOString(),
      models: data.modelIds || [],
      enabled: data.enabled ?? true,
      url: data.url,
    };
    setConnections(prev => [...prev, newConnection]);

    // Always persist to AdminSettingsContext (localStorage)
    const newAdminConnection: AdminConnection = {
      id: newId,
      name: data.name,
      type: data.provider || data.providerType || 'Custom',
      url: data.url || '',
      enabled: data.enabled ?? true,
      apiKey: apiKey,
    };

    const updatedConnections = [...adminSettings.settings.connections, newAdminConnection];
    console.log('[useAdmin] Persisting connections to localStorage:', updatedConnections);
    adminSettings.updateConnections(updatedConnections);

    // If not offline, also try to sync with backend (but don't fail if it errors)
    if (!AppConfig.isOfflineMode()) {
      try {
        await connectionsService.createConnection(data);
        console.log('[useAdmin] Synced connection to backend');
      } catch (err) {
        console.warn('[useAdmin] Failed to sync connection to backend (will use localStorage):', err);
      }
    }

    return newConnection;
  }, [adminSettings]);

  const updateConnection = useCallback(async (connectionId: string, data: any) => {
    console.log('[useAdmin] updateConnection called:', { connectionId, data });

    const apiKey = extractApiKeyFromHeaders(data);

    // Optimistic update to local state
    setConnections(prev => prev.map(c =>
      c.id === connectionId ? { ...c, ...data } : c
    ));

    // Always persist to AdminSettingsContext (localStorage)
    const updatedConnections = adminSettings.settings.connections.map(c =>
      c.id === connectionId
        ? {
            ...c,
            name: data.name ?? c.name,
            type: data.provider ?? data.providerType ?? c.type,
            url: data.url ?? c.url,
            enabled: data.enabled ?? data.isActive ?? c.enabled,
            apiKey: apiKey ?? c.apiKey,
          }
        : c
    );
    console.log('[useAdmin] Updating connections in localStorage:', updatedConnections);
    adminSettings.updateConnections(updatedConnections);

    // If not offline, also try to sync with backend
    if (!AppConfig.isOfflineMode()) {
      try {
        await connectionsService.updateConnection(connectionId, data);
        console.log('[useAdmin] Synced connection update to backend');
      } catch (err) {
        console.warn('[useAdmin] Failed to sync connection update to backend (using localStorage):', err);
      }
    }
  }, [adminSettings]);

  const deleteConnection = useCallback(async (connectionId: string) => {
    // Optimistic update
    setConnections(prev => prev.filter(c => c.id !== connectionId));

    if (AppConfig.isOfflineMode()) {
      // Persist to AdminSettingsContext
      const updatedConnections = adminSettings.settings.connections.filter(c => c.id !== connectionId);
      adminSettings.updateConnections(updatedConnections);
      return;
    }

    try {
      await connectionsService.deleteConnection(connectionId);
    } catch (err) {
      console.error('[useAdmin] Failed to delete connection:', err);
      await loadConnections(); // Revert
      throw err;
    }
  }, [loadConnections, adminSettings]);

  const testConnection = useCallback(async (connectionId: string) => {
    // In offline mode, test directly against the LM server
    if (AppConfig.isOfflineMode()) {
      const connection = adminSettings.settings.connections.find(c => c.id === connectionId);
      if (!connection?.url) {
        return {
          success: false,
          message: 'No URL configured for this connection',
          provider: connection?.type || 'Unknown',
          availableModels: [],
        };
      }

      try {
        // Test connection by fetching models from the LM server
        const modelsUrl = connection.url.replace(/\/$/, '') + '/models';
        const headers: Record<string, string> = {
          'Content-Type': 'application/json',
        };
        if (connection.apiKey) {
          headers['Authorization'] = `Bearer ${connection.apiKey}`;
        }

        const response = await fetch(modelsUrl, { headers });
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        const modelIds = (data.data || data.models || []).map((m: any) => m.id || m.name || m);

        return {
          success: true,
          message: `Connected! Found ${modelIds.length} models`,
          provider: connection.type || 'Custom',
          availableModels: modelIds,
        };
      } catch (err) {
        console.error('[useAdmin] LM server connection test failed:', err);
        return {
          success: false,
          message: `Connection failed: ${err instanceof Error ? err.message : 'Unknown error'}`,
          provider: connection.type || 'Custom',
          availableModels: [],
        };
      }
    }

    try {
      return await connectionsService.testConnection(connectionId);
    } catch (err) {
      console.error('[useAdmin] Failed to test connection:', err);
      throw err;
    }
  }, [adminSettings.settings.connections]);

  // Fetch models from an LM server endpoint and optionally add them
  const fetchModelsFromServer = useCallback(async (connectionId: string, addToModels = false) => {
    const connection = adminSettings.settings.connections.find(c => c.id === connectionId);
    if (!connection?.url) {
      throw new Error('No URL configured for this connection');
    }

    const modelsUrl = connection.url.replace(/\/$/, '') + '/models';
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    if (connection.apiKey) {
      headers['Authorization'] = `Bearer ${connection.apiKey}`;
    }

    const response = await fetch(modelsUrl, { headers });
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const data = await response.json();
    const serverModels = (data.data || data.models || []).map((m: any) => ({
      id: m.id || m.name || m,
      name: m.id || m.name || m,
      owned_by: m.owned_by || connection.name,
    }));

    if (addToModels && AppConfig.isOfflineMode()) {
      // Add fetched models that don't already exist
      const existingIds = new Set(adminSettings.settings.models.map(m => m.id));
      const newModels = serverModels
        .filter((m: any) => !existingIds.has(m.id))
        .map((m: any) => ({
          id: m.id,
          name: m.name,
          color: 'text-cyan-500',
          description: `From ${connection.name}`,
          connection: connection.name,
        }));

      if (newModels.length > 0) {
        adminSettings.updateModels([...adminSettings.settings.models, ...newModels]);
        // Also update local state
        const newModelDtos = newModels.map(toModelDto);
        setModels(prev => [...prev, ...newModelDtos]);
      }
    }

    return serverModels;
  }, [adminSettings]);

  // ============================================================================
  // MCP SERVERS
  // ============================================================================

  const loadMcpServers = useCallback(async () => {
    if (AppConfig.isOfflineMode()) {
      // Use mock MCP servers in offline mode
      setMcpServers([
        {
          id: '1',
          name: 'GitHub MCP',
          description: 'GitHub repository management',
          isActive: true,
          status: 'Connected',
          tools: ['create_issue', 'list_repos', 'search_code'],
          version: '1.0.0',
        },
        {
          id: '2',
          name: 'Slack MCP',
          description: 'Slack messaging integration',
          isActive: false,
          status: 'Disconnected',
          tools: ['send_message', 'list_channels'],
          version: '1.2.0',
        },
      ]);
      return;
    }

    try {
      const data = await adminService.getMcpServers();
      setMcpServers(data);
    } catch (err) {
      console.error('[useAdmin] Failed to load MCP servers:', err);
      setMcpServers([]);
    }
  }, []);

  const toggleMcpServer = useCallback(async (serverId: string) => {
    // Optimistic update
    setMcpServers(prev => prev.map(s => 
      s.id === serverId ? { ...s, isActive: !s.isActive } : s
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await adminService.toggleMcpServer(serverId);
      await loadMcpServers();
    } catch (err) {
      console.error('[useAdmin] Failed to toggle MCP server:', err);
      await loadMcpServers(); // Revert
    }
  }, [loadMcpServers]);

  const createMcpServer = useCallback(async (data: Partial<McpServerDto>) => {
    if (AppConfig.isOfflineMode()) {
      const newServer: McpServerDto = {
        id: `mcp-${Date.now()}`,
        name: data.name || 'New MCP Server',
        description: data.description || '',
        isActive: data.isActive ?? true,
        status: 'Disconnected',
        tools: data.tools || [],
        version: data.version || '1.0.0',
      };
      setMcpServers(prev => [...prev, newServer]);
      return newServer;
    }

    try {
      const server = await adminService.createMcpServer(data);
      await loadMcpServers();
      return server;
    } catch (err) {
      console.error('[useAdmin] Failed to create MCP server:', err);
      throw err;
    }
  }, [loadMcpServers]);

  const updateMcpServer = useCallback(async (serverId: string, data: Partial<McpServerDto>) => {
    // Optimistic update
    setMcpServers(prev => prev.map(s => 
      s.id === serverId ? { ...s, ...data } : s
    ));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await adminService.updateMcpServer(serverId, data);
      await loadMcpServers();
    } catch (err) {
      console.error('[useAdmin] Failed to update MCP server:', err);
      await loadMcpServers(); // Revert
      throw err;
    }
  }, [loadMcpServers]);

  const deleteMcpServer = useCallback(async (serverId: string) => {
    // Optimistic update
    setMcpServers(prev => prev.filter(s => s.id !== serverId));

    if (AppConfig.isOfflineMode()) {
      return;
    }

    try {
      await adminService.deleteMcpServer(serverId);
    } catch (err) {
      console.error('[useAdmin] Failed to delete MCP server:', err);
      await loadMcpServers(); // Revert
      throw err;
    }
  }, [loadMcpServers]);

  // ============================================================================
  // USERS
  // ============================================================================

  const loadUsers = useCallback(async () => {
    if (AppConfig.isOfflineMode()) {
      // Use mock users in offline mode
      setUsers([
        { id: '1', username: 'Rich', email: 'rich@example.com', userTier: 'Admin', isActive: true, createdAt: new Date().toISOString() },
        { id: '2', username: 'John Doe', email: 'john@example.com', userTier: 'BYOK', isActive: true, createdAt: new Date().toISOString() },
        { id: '3', username: 'Jane Smith', email: 'jane@example.com', userTier: 'Free', isActive: false, createdAt: new Date().toISOString() },
      ]);
      return;
    }

    try {
      const data = await adminService.getUsers();
      setUsers(data);
    } catch (err) {
      console.error('[useAdmin] Failed to load users:', err);
      setUsers([]);
    }
  }, []);

  // ============================================================================
  // INITIALIZATION
  // ============================================================================

  const loadAll = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      await Promise.all([
        loadDashboard(),
        loadModels(),
        loadConnections(),
        loadMcpServers(),
        loadUsers(),
      ]);
    } catch (err) {
      console.error('[useAdmin] Failed to load admin data:', err);
      setError('Failed to load admin data');
    } finally {
      setLoading(false);
    }
  }, [loadDashboard, loadModels, loadConnections, loadMcpServers, loadUsers]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  return {
    // Data
    dashboard,
    models,
    connections,
    mcpServers,
    users,
    
    // State
    loading,
    error,
    
    // Actions
    refresh: loadAll,
    
    // Models
    toggleModel,
    createModel,
    updateModel,
    deleteModel,
    addModelsFromConnection,

    // Connections
    toggleConnection,
    createConnection,
    updateConnection,
    deleteConnection,
    testConnection,
    fetchModelsFromServer,

    // MCP Servers
    toggleMcpServer,
    createMcpServer,
    updateMcpServer,
    deleteMcpServer,
    
    // Users
    refreshUsers: loadUsers,
  };
}
