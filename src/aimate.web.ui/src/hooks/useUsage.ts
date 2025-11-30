/**
 * Usage Analytics Hook
 * 
 * Manages usage tracking, billing, and analytics data
 */

import { useState, useEffect, useCallback } from 'react';
import { usageService, UsageStatsDto } from '../api/services';
import { AppConfig } from '../utils/config';

export function useUsage() {
  const [stats, setStats] = useState<UsageStatsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // ============================================================================
  // LOAD USAGE STATS
  // ============================================================================

  const loadStats = useCallback(async () => {
    if (AppConfig.isOfflineMode()) {
      // Use mock usage data in offline mode
      const mockStats: UsageStatsDto = {
        totalMessages: 234,
        totalTokens: 125000,
        totalCost: 2.50,
        billingPeriodStart: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
        billingPeriodEnd: new Date().toISOString(),
        usageByModel: [
          {
            model: 'GPT-4 Turbo',
            connection: 'OpenAI',
            messages: 89,
            tokens: 45000,
            cost: 1.20,
            color: '#8B5CF6',
          },
          {
            model: 'GPT-4',
            connection: 'OpenAI',
            messages: 56,
            tokens: 38000,
            cost: 0.85,
            color: '#3B82F6',
          },
          {
            model: 'Claude 3 Sonnet',
            connection: 'Anthropic',
            messages: 45,
            tokens: 28000,
            cost: 0.30,
            color: '#F59E0B',
          },
          {
            model: 'GPT-3.5 Turbo',
            connection: 'OpenAI',
            messages: 44,
            tokens: 14000,
            cost: 0.15,
            color: '#10B981',
          },
        ],
      };
      setStats(mockStats);
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const data = await usageService.getUsageStats();
      setStats(data);
      setError(null);
    } catch (err) {
      console.error('[useUsage] Failed to load usage stats:', err);
      setError('Failed to load usage data');
      // Fallback to zero stats on error
      setStats({
        totalMessages: 0,
        totalTokens: 0,
        totalCost: 0,
        billingPeriodStart: new Date().toISOString(),
        billingPeriodEnd: new Date().toISOString(),
        usageByModel: [],
      });
    } finally {
      setLoading(false);
    }
  }, []);

  // ============================================================================
  // LOAD BY DATE RANGE
  // ============================================================================

  const loadByDateRange = useCallback(async (startDate: string, endDate: string) => {
    if (AppConfig.isOfflineMode()) {
      // Return mock data for date range
      return stats;
    }

    try {
      setLoading(true);
      const data = await usageService.getUsageByDateRange(startDate, endDate);
      setStats(data);
      return data;
    } catch (err) {
      console.error('[useUsage] Failed to load usage by date range:', err);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  // ============================================================================
  // DAILY USAGE
  // ============================================================================

  const loadDailyUsage = useCallback(async (days: number = 30) => {
    if (AppConfig.isOfflineMode()) {
      // Generate mock daily data
      const mockDaily = Array.from({ length: days }, (_, i) => {
        const date = new Date();
        date.setDate(date.getDate() - (days - i - 1));
        return {
          date: date.toISOString().split('T')[0],
          messages: Math.floor(Math.random() * 20) + 5,
          tokens: Math.floor(Math.random() * 5000) + 1000,
          cost: Math.random() * 0.5,
        };
      });
      return mockDaily;
    }

    try {
      return await usageService.getDailyUsage(days);
    } catch (err) {
      console.error('[useUsage] Failed to load daily usage:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // COST PROJECTION
  // ============================================================================

  const getCostProjection = useCallback(async () => {
    if (AppConfig.isOfflineMode()) {
      return {
        currentCost: 2.50,
        projectedCost: 3.75,
        daysRemaining: 15,
      };
    }

    try {
      return await usageService.getCostProjection();
    } catch (err) {
      console.error('[useUsage] Failed to get cost projection:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // EXPORT USAGE
  // ============================================================================

  const exportUsage = useCallback(async (startDate: string, endDate: string) => {
    if (AppConfig.isOfflineMode()) {
      throw new Error('Cannot export in offline mode');
    }

    try {
      const blob = await usageService.exportUsage(startDate, endDate);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `usage-${startDate}-to-${endDate}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      console.error('[useUsage] Failed to export usage:', err);
      throw err;
    }
  }, []);

  // ============================================================================
  // GET USAGE BY CONNECTION
  // ============================================================================

  const getUsageByConnection = useCallback(() => {
    if (!stats?.usageByModel) return [];

    // Aggregate usage by connection
    const byConnection = new Map<string, {
      connection: string;
      messages: number;
      tokens: number;
      cost: number;
      models: string[];
    }>();

    for (const model of stats.usageByModel) {
      const conn = model.connection || 'Unknown';
      const existing = byConnection.get(conn) || {
        connection: conn,
        messages: 0,
        tokens: 0,
        cost: 0,
        models: [],
      };
      existing.messages += model.messages;
      existing.tokens += model.tokens;
      existing.cost += model.cost;
      if (!existing.models.includes(model.model)) {
        existing.models.push(model.model);
      }
      byConnection.set(conn, existing);
    }

    return Array.from(byConnection.values()).sort((a, b) => b.tokens - a.tokens);
  }, [stats]);

  // ============================================================================
  // TRACK LOCAL USAGE (for direct LM server calls)
  // ============================================================================

  const trackLocalUsage = useCallback((connectionName: string, model: string, tokens: number) => {
    // Store in localStorage for session tracking
    const key = 'aiMate_localUsage';
    const existing = JSON.parse(localStorage.getItem(key) || '[]');
    existing.push({
      timestamp: new Date().toISOString(),
      connection: connectionName,
      model,
      tokens,
    });
    // Keep last 1000 entries
    if (existing.length > 1000) {
      existing.splice(0, existing.length - 1000);
    }
    localStorage.setItem(key, JSON.stringify(existing));
  }, []);

  const getLocalUsage = useCallback(() => {
    const key = 'aiMate_localUsage';
    return JSON.parse(localStorage.getItem(key) || '[]') as Array<{
      timestamp: string;
      connection: string;
      model: string;
      tokens: number;
    }>;
  }, []);

  // ============================================================================
  // INITIALIZATION
  // ============================================================================

  useEffect(() => {
    loadStats();
  }, [loadStats]);

  return {
    stats,
    loading,
    error,
    refresh: loadStats,
    loadByDateRange,
    loadDailyUsage,
    getCostProjection,
    exportUsage,
    getUsageByConnection,
    trackLocalUsage,
    getLocalUsage,
  };
}
