/**
 * Application configuration utilities
 * Manages runtime configuration with localStorage persistence
 */

const OFFLINE_MODE_KEY = 'aimate_offline_mode';
const BACKEND_AVAILABLE_KEY = 'aimate_backend_available';

export const AppConfig = {
  /**
   * Check if the app is in offline mode
   * @returns true if offline mode is enabled OR backend is unavailable
   */
  isOfflineMode(): boolean {
    if (typeof window === 'undefined') return true; // Default to offline on server

    // Check explicit offline mode setting - respect user's choice
    const explicitOffline = localStorage.getItem(OFFLINE_MODE_KEY);
    if (explicitOffline === 'true') return true;
    if (explicitOffline === 'false') return false; // User explicitly disabled offline mode

    // No explicit setting - check if backend is available (checked on startup)
    const backendAvailable = localStorage.getItem(BACKEND_AVAILABLE_KEY);
    if (backendAvailable === 'false') return true;

    // Default to offline mode if never checked
    if (backendAvailable === null) return true;

    return false;
  },

  /**
   * Set offline mode state
   * @param enabled - true to enable offline mode, false to disable
   */
  setOfflineMode(enabled: boolean): void {
    if (typeof window === 'undefined') return;
    localStorage.setItem(OFFLINE_MODE_KEY, String(enabled));
  },

  /**
   * Set backend availability status
   * @param available - true if backend is reachable
   */
  setBackendAvailable(available: boolean): void {
    if (typeof window === 'undefined') return;
    localStorage.setItem(BACKEND_AVAILABLE_KEY, String(available));
  },

  /**
   * Check if backend is explicitly marked as available
   */
  isBackendAvailable(): boolean {
    if (typeof window === 'undefined') return false;
    return localStorage.getItem(BACKEND_AVAILABLE_KEY) === 'true';
  },

  /**
   * Get the API base URL based on current mode
   * @returns API base URL or empty string for offline mode
   */
  getApiBaseUrl(): string {
    if (this.isOfflineMode()) {
      return ''; // No API calls in offline mode
    }
    return import.meta.env.VITE_API_BASE_URL || '/api/v1';
  }
};