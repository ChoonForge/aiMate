/**
 * Tools Service
 *
 * Handles MCP tool discovery, validation, and execution
 */

import { apiClient } from '../client';
import { ToolDto, ToolExecutionRequest, ToolExecutionResponse, ToolParameter } from '../types';
import { AppConfig } from '../../utils/config';

// Mock tools for offline development
const MOCK_TOOLS: Record<string, ToolDto[]> = {
  'mcp-1': [
    {
      name: 'weather_lookup',
      description: 'Get current weather for a location',
      category: 'utilities',
      parameters: {
        location: {
          type: 'string',
          description: 'City name or coordinates',
          required: true,
        },
        units: {
          type: 'string',
          description: 'Temperature units (celsius/fahrenheit)',
          required: false,
          default: 'celsius',
        },
      },
    },
    {
      name: 'web_search',
      description: 'Search the web for information',
      category: 'search',
      parameters: {
        query: {
          type: 'string',
          description: 'Search query',
          required: true,
        },
        limit: {
          type: 'integer',
          description: 'Number of results to return',
          required: false,
          default: 5,
        },
      },
    },
  ],
  'mcp-2': [
    {
      name: 'code_execute',
      description: 'Execute code in a sandboxed environment',
      category: 'code',
      parameters: {
        language: {
          type: 'string',
          description: 'Programming language (python, javascript, etc.)',
          required: true,
        },
        code: {
          type: 'string',
          description: 'Code to execute',
          required: true,
        },
        timeout: {
          type: 'integer',
          description: 'Execution timeout in seconds',
          required: false,
          default: 30,
        },
      },
    },
    {
      name: 'file_read',
      description: 'Read contents of a file',
      category: 'filesystem',
      parameters: {
        path: {
          type: 'string',
          description: 'File path to read',
          required: true,
        },
        encoding: {
          type: 'string',
          description: 'File encoding (utf-8, base64, etc.)',
          required: false,
          default: 'utf-8',
        },
      },
    },
  ],
};

class ToolsService {
  // ============================================================================
  // TOOL DISCOVERY
  // ============================================================================

  /**
   * Get all tools from an MCP server
   */
  async getToolsByServer(serverId: string): Promise<ToolDto[]> {
    if (AppConfig.isOfflineMode()) {
      console.log('[ToolsService] Returning mock tools for server:', serverId);
      return MOCK_TOOLS[serverId] || [];
    }

    return apiClient.get<ToolDto[]>(`/mcp/${serverId}/tools`);
  }

  /**
   * Get a specific tool's full schema
   */
  async getToolSchema(serverId: string, toolName: string): Promise<ToolDto> {
    if (AppConfig.isOfflineMode()) {
      const tools = MOCK_TOOLS[serverId] || [];
      const tool = tools.find(t => t.name === toolName);
      if (!tool) {
        throw new Error(`Tool ${toolName} not found on server ${serverId}`);
      }
      return tool;
    }

    return apiClient.get<ToolDto>(`/mcp/${serverId}/tools/${toolName}`);
  }

  /**
   * Get all tools from all enabled MCP servers
   */
  async getAllTools(serverIds: string[]): Promise<Record<string, ToolDto[]>> {
    const results: Record<string, ToolDto[]> = {};

    await Promise.all(
      serverIds.map(async (serverId) => {
        try {
          results[serverId] = await this.getToolsByServer(serverId);
        } catch (err) {
          console.error(`[ToolsService] Failed to get tools from ${serverId}:`, err);
          results[serverId] = [];
        }
      })
    );

    return results;
  }

  // ============================================================================
  // TOOL VALIDATION
  // ============================================================================

  /**
   * Validate tool parameters against schema
   */
  validateParameters(
    tool: ToolDto,
    parameters: Record<string, any>
  ): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    // Check required parameters
    for (const [name, param] of Object.entries(tool.parameters)) {
      if (param.required && !(name in parameters)) {
        errors.push(`Missing required parameter: ${name}`);
        continue;
      }

      // Type validation
      if (name in parameters) {
        const value = parameters[name];
        const expectedType = param.type;

        if (!this.validateType(value, expectedType)) {
          errors.push(`Invalid type for ${name}: expected ${expectedType}, got ${typeof value}`);
        }
      }
    }

    // Check for unknown parameters
    for (const name of Object.keys(parameters)) {
      if (!(name in tool.parameters)) {
        errors.push(`Unknown parameter: ${name}`);
      }
    }

    return { valid: errors.length === 0, errors };
  }

  private validateType(value: any, expectedType: ToolParameter['type']): boolean {
    switch (expectedType) {
      case 'string':
        return typeof value === 'string';
      case 'integer':
        return Number.isInteger(value);
      case 'number':
        return typeof value === 'number';
      case 'boolean':
        return typeof value === 'boolean';
      case 'array':
        return Array.isArray(value);
      case 'object':
        return typeof value === 'object' && value !== null && !Array.isArray(value);
      default:
        return true;
    }
  }

  // ============================================================================
  // TOOL EXECUTION
  // ============================================================================

  /**
   * Execute a tool on an MCP server
   */
  async executeTool(
    serverId: string,
    toolName: string,
    parameters: Record<string, any>
  ): Promise<ToolExecutionResponse> {
    const startTime = Date.now();

    if (AppConfig.isOfflineMode()) {
      console.log('[ToolsService] Mock executing tool:', { serverId, toolName, parameters });

      // Simulate execution delay
      await new Promise(resolve => setTimeout(resolve, 500 + Math.random() * 500));

      // Return mock result based on tool name
      return this.getMockExecutionResult(toolName, parameters, startTime);
    }

    const request: ToolExecutionRequest = {
      tool: toolName,
      parameters,
    };

    return apiClient.post<ToolExecutionResponse>(
      `/mcp/${serverId}/tools/${toolName}/execute`,
      request
    );
  }

  private getMockExecutionResult(
    toolName: string,
    parameters: Record<string, any>,
    startTime: number
  ): ToolExecutionResponse {
    const executionTime = Date.now() - startTime;

    switch (toolName) {
      case 'weather_lookup':
        return {
          success: true,
          toolName,
          result: {
            location: parameters.location,
            temperature: Math.round(15 + Math.random() * 15),
            conditions: ['Sunny', 'Cloudy', 'Partly Cloudy', 'Rainy'][Math.floor(Math.random() * 4)],
            humidity: Math.round(40 + Math.random() * 40),
            wind: `${Math.round(5 + Math.random() * 20)} km/h`,
          },
          executionTime,
          timestamp: new Date().toISOString(),
        };

      case 'web_search':
        return {
          success: true,
          toolName,
          result: {
            query: parameters.query,
            results: [
              { title: `Result 1 for "${parameters.query}"`, url: 'https://example.com/1', snippet: 'Sample search result...' },
              { title: `Result 2 for "${parameters.query}"`, url: 'https://example.com/2', snippet: 'Another relevant result...' },
              { title: `Result 3 for "${parameters.query}"`, url: 'https://example.com/3', snippet: 'More information about...' },
            ].slice(0, parameters.limit || 5),
          },
          executionTime,
          timestamp: new Date().toISOString(),
        };

      case 'code_execute':
        return {
          success: true,
          toolName,
          result: {
            language: parameters.language,
            output: `Executed ${parameters.language} code successfully.\nOutput: Hello, World!`,
            exitCode: 0,
          },
          executionTime,
          timestamp: new Date().toISOString(),
        };

      case 'file_read':
        return {
          success: true,
          toolName,
          result: {
            path: parameters.path,
            content: `Contents of ${parameters.path}:\n\nThis is sample file content...`,
            size: 1234,
            encoding: parameters.encoding || 'utf-8',
          },
          executionTime,
          timestamp: new Date().toISOString(),
        };

      default:
        return {
          success: true,
          toolName,
          result: { message: `Tool ${toolName} executed successfully`, parameters },
          executionTime,
          timestamp: new Date().toISOString(),
        };
    }
  }

  // ============================================================================
  // TOOL SEARCH
  // ============================================================================

  /**
   * Search for tools matching a query across all servers
   */
  async searchTools(
    query: string,
    serverIds: string[]
  ): Promise<Array<{ serverId: string; tool: ToolDto; relevance: number }>> {
    const allTools = await this.getAllTools(serverIds);
    const results: Array<{ serverId: string; tool: ToolDto; relevance: number }> = [];
    const lowerQuery = query.toLowerCase();

    for (const [serverId, tools] of Object.entries(allTools)) {
      for (const tool of tools) {
        let relevance = 0;

        // Check name match
        if (tool.name.toLowerCase().includes(lowerQuery)) {
          relevance += 10;
        }

        // Check description match
        if (tool.description.toLowerCase().includes(lowerQuery)) {
          relevance += 5;
        }

        // Check category match
        if (tool.category.toLowerCase().includes(lowerQuery)) {
          relevance += 3;
        }

        // Check parameter descriptions
        for (const param of Object.values(tool.parameters)) {
          if (param.description.toLowerCase().includes(lowerQuery)) {
            relevance += 1;
          }
        }

        if (relevance > 0) {
          results.push({ serverId, tool, relevance });
        }
      }
    }

    // Sort by relevance
    return results.sort((a, b) => b.relevance - a.relevance);
  }
}

export const toolsService = new ToolsService();
export default toolsService;
