import React, { createContext, useContext, useState, ReactNode } from 'react';

export interface DebugLog {
  id: string;
  timestamp: Date;
  action: string;
  api?: string;
  payload?: any;
  type: 'info' | 'success' | 'error' | 'warning';
}

interface DebugContextType {
  debugEnabled: boolean;
  setDebugEnabled: (enabled: boolean) => void;
  showcaseMode: boolean;
  setShowcaseMode: (enabled: boolean) => void;
  logs: DebugLog[];
  addLog: (log: Omit<DebugLog, 'id' | 'timestamp'>) => void;
  clearLogs: () => void;
}

const DebugContext = createContext<DebugContextType | undefined>(undefined);

export function DebugProvider({ children }: { children: ReactNode }) {
  const [debugEnabled, setDebugEnabled] = useState(() => {
    const saved = localStorage.getItem('debugEnabled');
    return saved === 'true';
  });
  const [showcaseMode, setShowcaseMode] = useState(false);
  const [logs, setLogs] = useState<DebugLog[]>([]);

  const handleSetDebugEnabled = (enabled: boolean) => {
    setDebugEnabled(enabled);
    localStorage.setItem('debugEnabled', String(enabled));
    if (!enabled) {
      setLogs([]);
      setShowcaseMode(false);
    }
  };

  const handleSetShowcaseMode = (enabled: boolean) => {
    setShowcaseMode(enabled);
  };

  const addLog = (log: Omit<DebugLog, 'id' | 'timestamp'>) => {
    if (!debugEnabled) return;
    
    const newLog: DebugLog = {
      ...log,
      id: Date.now().toString() + Math.random(),
      timestamp: new Date(),
    };
    
    setLogs(prev => [newLog, ...prev].slice(0, 100)); // Keep last 100 logs
  };

  const clearLogs = () => {
    setLogs([]);
  };

  return (
    <DebugContext.Provider value={{ debugEnabled, setDebugEnabled: handleSetDebugEnabled, showcaseMode, setShowcaseMode: handleSetShowcaseMode, logs, addLog, clearLogs }}>
      {children}
    </DebugContext.Provider>
  );
}

export function useDebug() {
  const context = useContext(DebugContext);
  if (context === undefined) {
    throw new Error('useDebug must be used within a DebugProvider');
  }
  return context;
}