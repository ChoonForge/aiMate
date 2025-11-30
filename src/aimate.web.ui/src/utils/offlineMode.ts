/**
 * Offline Mode Utilities
 *
 * Standardized helpers for consistent offline mode behavior across hooks
 */

import { AppConfig } from './config';

/**
 * Check if the application is in offline mode
 */
export const isOfflineMode = (): boolean => AppConfig.isOfflineMode();

/**
 * Execute an operation with offline mode fallback
 * Returns mock data in offline mode, executes the online operation otherwise
 *
 * @param offlineHandler - Function that returns mock data
 * @param onlineHandler - Function that performs the actual API call
 */
export async function withOfflineFallback<T>(
  offlineHandler: () => T | Promise<T>,
  onlineHandler: () => Promise<T>
): Promise<T> {
  if (isOfflineMode()) {
    return offlineHandler();
  }
  return onlineHandler();
}

/**
 * Execute a mutation with offline mode handling
 * In offline mode, performs optimistic update only
 * Online mode syncs with backend (with optional failure handling)
 *
 * @param offlineHandler - Function for offline operation (optimistic update)
 * @param onlineHandler - Function for online operation (API call)
 * @param onError - Optional error handler for online failures
 */
export async function withOfflineMutation<T>(
  offlineHandler: () => T | Promise<T>,
  onlineHandler: () => Promise<T>,
  onError?: (error: Error) => void
): Promise<T> {
  if (isOfflineMode()) {
    return offlineHandler();
  }

  try {
    return await onlineHandler();
  } catch (error) {
    if (onError) {
      onError(error as Error);
    }
    throw error;
  }
}

/**
 * Skip an operation in offline mode
 * Useful for operations that should not run offline (exports, sync, etc.)
 *
 * @param operation - The operation to perform
 * @param errorMessage - Custom error message for offline mode
 */
export async function skipInOfflineMode<T>(
  operation: () => Promise<T>,
  errorMessage: string = 'This operation is not available in offline mode'
): Promise<T> {
  if (isOfflineMode()) {
    throw new Error(errorMessage);
  }
  return operation();
}

/**
 * Silently skip an operation in offline mode
 * Returns undefined without throwing an error
 */
export async function silentlySkipInOfflineMode<T>(
  operation: () => Promise<T>
): Promise<T | undefined> {
  if (isOfflineMode()) {
    return undefined;
  }
  return operation();
}

/**
 * Try online operation with offline fallback
 * First tries online operation, falls back to offline handler on error
 * Useful for hybrid mode where online is preferred but offline should work
 */
export async function tryOnlineWithFallback<T>(
  onlineHandler: () => Promise<T>,
  offlineHandler: () => T | Promise<T>,
  shouldFallback: (error: Error) => boolean = () => true
): Promise<T> {
  if (isOfflineMode()) {
    return offlineHandler();
  }

  try {
    return await onlineHandler();
  } catch (error) {
    if (shouldFallback(error as Error)) {
      console.warn('[OfflineMode] Online operation failed, using fallback:', error);
      return offlineHandler();
    }
    throw error;
  }
}

/**
 * Generate a unique mock ID
 * Useful for creating mock entities in offline mode
 */
export function generateMockId(prefix: string = 'mock'): string {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Create a delayed mock response
 * Simulates network latency in offline mode for realistic UX
 */
export function createDelayedMockResponse<T>(
  data: T,
  delayMs: number = 300
): Promise<T> {
  return new Promise((resolve) => {
    setTimeout(() => resolve(data), delayMs);
  });
}

/**
 * Log offline mode operation
 * Consistent logging for debugging
 */
export function logOfflineOperation(
  hookName: string,
  operation: string,
  data?: unknown
): void {
  if (import.meta.env?.DEV) {
    console.log(`[${hookName}] Offline mode: ${operation}`, data ?? '');
  }
}

/**
 * Offline mode decorator for methods
 * Use to wrap hook methods with consistent offline handling
 */
export function offlineAware<T extends (...args: unknown[]) => Promise<unknown>>(
  hookName: string,
  operationName: string,
  method: T,
  offlineResult?: unknown
): T {
  return (async (...args: unknown[]) => {
    if (isOfflineMode()) {
      logOfflineOperation(hookName, operationName, args);
      return offlineResult;
    }
    return method(...args);
  }) as T;
}
