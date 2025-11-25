import apiClient from './client';
import { LoginRequest, LoginResponse, User, ApiKey } from './types';

export const authApi = {
  /**
   * Login with email and password
   */
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    // Store tokens
    localStorage.setItem('auth_token', response.token);
    if (response.refreshToken) {
      localStorage.setItem('refresh_token', response.refreshToken);
    }
    return response;
  },

  /**
   * Logout - clear tokens and optionally notify backend
   */
  async logout(): Promise<void> {
    try {
      // Optionally call backend to invalidate token
      await apiClient.post('/auth/logout');
    } catch (error) {
      // Continue with logout even if backend call fails
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('refresh_token');
    }
  },

  /**
   * Get current user profile
   */
  async getCurrentUser(): Promise<User> {
    return apiClient.get<User>('/auth/me');
  },

  /**
   * Refresh access token using refresh token
   */
  async refreshToken(): Promise<LoginResponse> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await apiClient.post<LoginResponse>('/auth/refresh', {
      refreshToken,
    });

    // Update stored tokens
    localStorage.setItem('auth_token', response.token);
    if (response.refreshToken) {
      localStorage.setItem('refresh_token', response.refreshToken);
    }

    return response;
  },

  /**
   * Register new user
   */
  async register(data: {
    email: string;
    password: string;
    name: string;
  }): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/register', data);
    // Store tokens
    localStorage.setItem('auth_token', response.token);
    if (response.refreshToken) {
      localStorage.setItem('refresh_token', response.refreshToken);
    }
    return response;
  },

  /**
   * Request password reset
   */
  async requestPasswordReset(email: string): Promise<void> {
    await apiClient.post('/auth/password-reset', { email });
  },

  /**
   * Reset password with token
   */
  async resetPassword(token: string, newPassword: string): Promise<void> {
    await apiClient.post('/auth/password-reset/confirm', {
      token,
      newPassword,
    });
  },

  /**
   * Get all API keys for current user
   */
  async getApiKeys(): Promise<ApiKey[]> {
    return apiClient.get<ApiKey[]>('/auth/api-keys');
  },

  /**
   * Create new API key
   */
  async createApiKey(name: string): Promise<ApiKey> {
    return apiClient.post<ApiKey>('/auth/api-keys', { name });
  },

  /**
   * Delete API key
   */
  async deleteApiKey(keyId: string): Promise<void> {
    await apiClient.delete(`/auth/api-keys/${keyId}`);
  },

  /**
   * Check if user is authenticated (has valid token)
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  },

  /**
   * Get stored auth token
   */
  getToken(): string | null {
    return localStorage.getItem('auth_token');
  },
};
