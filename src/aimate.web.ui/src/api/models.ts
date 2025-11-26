import apiClient from './client';
import { Model } from './types';

export interface AvailableModel {
  id: string;
  name: string;
  provider: string;
  color: string;
  isAvailable: boolean;
  contextWindow?: number;
  maxTokens?: number;
}

// The simulated model that's always available
export const SIMULATED_MODEL: AvailableModel = {
  id: 'simulated',
  name: 'Simulated',
  provider: 'Local',
  color: 'text-gray-400',
  isAvailable: true,
};

// Map provider names to Tailwind color classes
const providerColors: Record<string, string> = {
  'OpenAI': 'text-purple-500',
  'Anthropic': 'text-orange-500',
  'Azure': 'text-blue-500',
  'Google': 'text-green-500',
  'Local': 'text-gray-400',
  'Custom': 'text-cyan-500',
};

function getColorForProvider(provider: string): string {
  return providerColors[provider] || 'text-gray-500';
}

/**
 * Fetch available models from the backend
 * Returns simulated model + real models from configured connections
 */
export async function getAvailableModels(): Promise<AvailableModel[]> {
  try {
    // Try to fetch models from admin endpoint
    const models = await apiClient.get<Model[]>('/admin/models');

    const availableModels: AvailableModel[] = models
      .filter(m => m.isAvailable)
      .map(m => ({
        id: m.id,
        name: m.name,
        provider: m.provider,
        color: getColorForProvider(m.provider),
        isAvailable: m.isAvailable,
        contextWindow: m.contextWindow,
        maxTokens: m.maxTokens,
      }));

    // Always include simulated as first option
    return [SIMULATED_MODEL, ...availableModels];
  } catch (error) {
    console.warn('Failed to fetch models from backend, using simulated only:', error);
    // If backend is unavailable, return just the simulated model
    return [SIMULATED_MODEL];
  }
}

/**
 * Check if a model is the simulated model
 */
export function isSimulatedModel(modelId: string): boolean {
  return modelId === 'simulated';
}

export const modelsApi = {
  getAvailableModels,
  isSimulatedModel,
  SIMULATED_MODEL,
};

export default modelsApi;
