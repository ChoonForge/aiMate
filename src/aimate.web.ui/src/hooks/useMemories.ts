/**
 * Memories Hook
 *
 * Manages persistent user memories across conversations.
 * Memories are facts or preferences the user has shared that should be remembered.
 */

import { useState, useEffect, useCallback } from 'react';
import { AppConfig } from '../utils/config';

export interface Memory {
  id: string;
  content: string;
  category: 'preference' | 'fact' | 'context' | 'custom';
  createdAt: string;
  updatedAt: string;
  source?: string; // Conversation ID where this was learned
}

const MEMORIES_STORAGE_KEY = 'aiMate_memories';
const MAX_MEMORIES = 100;

export function useMemories() {
  const [memories, setMemories] = useState<Memory[]>([]);
  const [loading, setLoading] = useState(true);

  // ============================================================================
  // LOAD MEMORIES FROM STORAGE
  // ============================================================================

  const loadMemories = useCallback(() => {
    try {
      const stored = localStorage.getItem(MEMORIES_STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored) as Memory[];
        setMemories(parsed);
      }
    } catch (err) {
      console.error('[useMemories] Failed to load memories:', err);
      setMemories([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // ============================================================================
  // SAVE MEMORIES TO STORAGE
  // ============================================================================

  const saveMemories = useCallback((newMemories: Memory[]) => {
    try {
      // Limit to max memories, keeping most recent
      const limited = newMemories.slice(-MAX_MEMORIES);
      localStorage.setItem(MEMORIES_STORAGE_KEY, JSON.stringify(limited));
      setMemories(limited);
    } catch (err) {
      console.error('[useMemories] Failed to save memories:', err);
    }
  }, []);

  // ============================================================================
  // ADD MEMORY
  // ============================================================================

  const addMemory = useCallback((
    content: string,
    category: Memory['category'] = 'fact',
    source?: string
  ) => {
    const newMemory: Memory = {
      id: `mem-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      content: content.trim(),
      category,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      source,
    };

    // Check for duplicates (similar content)
    const isDuplicate = memories.some(m =>
      m.content.toLowerCase() === content.toLowerCase().trim()
    );

    if (isDuplicate) {
      console.log('[useMemories] Duplicate memory, skipping:', content);
      return null;
    }

    const updated = [...memories, newMemory];
    saveMemories(updated);
    console.log('[useMemories] Added memory:', newMemory.id);
    return newMemory;
  }, [memories, saveMemories]);

  // ============================================================================
  // UPDATE MEMORY
  // ============================================================================

  const updateMemory = useCallback((id: string, content: string) => {
    const updated = memories.map(m =>
      m.id === id
        ? { ...m, content: content.trim(), updatedAt: new Date().toISOString() }
        : m
    );
    saveMemories(updated);
  }, [memories, saveMemories]);

  // ============================================================================
  // DELETE MEMORY
  // ============================================================================

  const deleteMemory = useCallback((id: string) => {
    const updated = memories.filter(m => m.id !== id);
    saveMemories(updated);
  }, [memories, saveMemories]);

  // ============================================================================
  // CLEAR ALL MEMORIES
  // ============================================================================

  const clearAllMemories = useCallback(() => {
    localStorage.removeItem(MEMORIES_STORAGE_KEY);
    setMemories([]);
  }, []);

  // ============================================================================
  // GET MEMORIES BY CATEGORY
  // ============================================================================

  const getByCategory = useCallback((category: Memory['category']) => {
    return memories.filter(m => m.category === category);
  }, [memories]);

  // ============================================================================
  // SEARCH MEMORIES
  // ============================================================================

  const searchMemories = useCallback((query: string) => {
    const lowerQuery = query.toLowerCase();
    return memories.filter(m =>
      m.content.toLowerCase().includes(lowerQuery)
    );
  }, [memories]);

  // ============================================================================
  // GET CONTEXT STRING FOR LLM
  // ============================================================================

  const getContextString = useCallback(() => {
    if (memories.length === 0) return '';

    const preferences = memories.filter(m => m.category === 'preference');
    const facts = memories.filter(m => m.category === 'fact');
    const context = memories.filter(m => m.category === 'context');

    let contextStr = '\n\n--- USER MEMORIES ---\n';

    if (preferences.length > 0) {
      contextStr += '\nUser Preferences:\n';
      preferences.forEach(m => {
        contextStr += `- ${m.content}\n`;
      });
    }

    if (facts.length > 0) {
      contextStr += '\nKnown Facts About User:\n';
      facts.forEach(m => {
        contextStr += `- ${m.content}\n`;
      });
    }

    if (context.length > 0) {
      contextStr += '\nRelevant Context:\n';
      context.forEach(m => {
        contextStr += `- ${m.content}\n`;
      });
    }

    contextStr += '--- END USER MEMORIES ---\n';

    return contextStr;
  }, [memories]);

  // ============================================================================
  // EXTRACT MEMORIES FROM CONVERSATION
  // ============================================================================

  const extractMemoriesFromText = useCallback((text: string, conversationId?: string) => {
    // Common patterns for facts/preferences the user might share
    const patterns = [
      { regex: /my name is (\w+)/i, category: 'fact' as const, template: (m: RegExpMatchArray) => `User's name is ${m[1]}` },
      { regex: /i(?:'m| am) from ([\w\s]+)/i, category: 'fact' as const, template: (m: RegExpMatchArray) => `User is from ${m[1].trim()}` },
      { regex: /i(?:'m| am) a ([\w\s]+)/i, category: 'fact' as const, template: (m: RegExpMatchArray) => `User is a ${m[1].trim()}` },
      { regex: /i work (?:at|for) ([\w\s]+)/i, category: 'fact' as const, template: (m: RegExpMatchArray) => `User works at ${m[1].trim()}` },
      { regex: /i prefer ([\w\s]+)/i, category: 'preference' as const, template: (m: RegExpMatchArray) => `User prefers ${m[1].trim()}` },
      { regex: /i like ([\w\s]+)/i, category: 'preference' as const, template: (m: RegExpMatchArray) => `User likes ${m[1].trim()}` },
      { regex: /i don't like ([\w\s]+)/i, category: 'preference' as const, template: (m: RegExpMatchArray) => `User doesn't like ${m[1].trim()}` },
      { regex: /please (?:always|remember to) ([\w\s]+)/i, category: 'preference' as const, template: (m: RegExpMatchArray) => `User wants: ${m[1].trim()}` },
    ];

    const extracted: Memory[] = [];

    for (const pattern of patterns) {
      const match = text.match(pattern.regex);
      if (match) {
        const content = pattern.template(match);
        const memory = addMemory(content, pattern.category, conversationId);
        if (memory) {
          extracted.push(memory);
        }
      }
    }

    return extracted;
  }, [addMemory]);

  // ============================================================================
  // INITIALIZATION
  // ============================================================================

  useEffect(() => {
    loadMemories();
  }, [loadMemories]);

  return {
    memories,
    loading,

    // Actions
    addMemory,
    updateMemory,
    deleteMemory,
    clearAllMemories,
    getByCategory,
    searchMemories,
    getContextString,
    extractMemoriesFromText,
    refresh: loadMemories,
  };
}
