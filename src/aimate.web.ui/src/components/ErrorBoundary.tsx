/**
 * Error Boundary Component
 *
 * Catches React errors and displays fallback UI
 */

import { Component, ErrorInfo, ReactNode } from 'react';
import { AlertTriangle, RefreshCw } from 'lucide-react';
import { Button } from './ui/button';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
  context?: string; // e.g., 'chat', 'modal', 'sidebar'
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error(`[ErrorBoundary${this.props.context ? `:${this.props.context}` : ''}] Error caught:`, error, errorInfo);
    this.props.onError?.(error, errorInfo);
  }

  handleReset = (): void => {
    this.setState({ hasError: false, error: null });
  };

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <ErrorFallback
          error={this.state.error}
          context={this.props.context}
          onReset={this.handleReset}
        />
      );
    }

    return this.props.children;
  }
}

interface ErrorFallbackProps {
  error: Error | null;
  context?: string;
  onReset: () => void;
}

function ErrorFallback({ error, context, onReset }: ErrorFallbackProps) {
  const contextLabel = context ? `in ${context}` : '';

  return (
    <div className="flex flex-col items-center justify-center p-6 text-center gap-4">
      <div className="p-3 rounded-full bg-red-100 dark:bg-red-900/30">
        <AlertTriangle className="h-8 w-8 text-red-600 dark:text-red-400" />
      </div>
      <div className="space-y-2">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Something went wrong{contextLabel ? ` ${contextLabel}` : ''}
        </h3>
        <p className="text-sm text-gray-600 dark:text-gray-400 max-w-md">
          {error?.message || 'An unexpected error occurred. Please try again.'}
        </p>
      </div>
      <Button
        variant="outline"
        size="sm"
        onClick={onReset}
        className="gap-2"
      >
        <RefreshCw className="h-4 w-4" />
        Try Again
      </Button>
    </div>
  );
}

/**
 * Compact error fallback for smaller components
 */
export function CompactErrorFallback({ onReset }: { onReset?: () => void }) {
  return (
    <div className="flex items-center gap-2 p-3 text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 rounded-lg">
      <AlertTriangle className="h-4 w-4 flex-shrink-0" />
      <span>Failed to load</span>
      {onReset && (
        <Button variant="ghost" size="sm" onClick={onReset} className="h-6 px-2 ml-auto">
          Retry
        </Button>
      )}
    </div>
  );
}

/**
 * Modal-specific error fallback
 */
export function ModalErrorFallback({ onClose }: { onClose?: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-6 text-center gap-4">
      <div className="p-3 rounded-full bg-red-100 dark:bg-red-900/30">
        <AlertTriangle className="h-8 w-8 text-red-600 dark:text-red-400" />
      </div>
      <div className="space-y-2">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Unable to load content
        </h3>
        <p className="text-sm text-gray-600 dark:text-gray-400">
          Please close and try again.
        </p>
      </div>
      {onClose && (
        <Button variant="outline" size="sm" onClick={onClose}>
          Close
        </Button>
      )}
    </div>
  );
}

export default ErrorBoundary;
