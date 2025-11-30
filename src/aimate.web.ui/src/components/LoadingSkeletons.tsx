/**
 * Loading Skeletons
 *
 * Reusable skeleton components for better perceived performance
 */

import { Skeleton } from "./ui/skeleton";

/**
 * Skeleton for a single conversation item in the sidebar
 */
export function ConversationItemSkeleton() {
  return (
    <div className="flex items-center gap-3 px-4 py-3">
      <Skeleton className="h-8 w-8 rounded-full shrink-0" />
      <div className="flex-1 space-y-2">
        <Skeleton className="h-4 w-3/4" />
        <Skeleton className="h-3 w-1/2" />
      </div>
    </div>
  );
}

/**
 * Skeleton for the conversation list in sidebar
 */
export function ConversationListSkeleton({ count = 5 }: { count?: number }) {
  return (
    <div className="space-y-1">
      {Array.from({ length: count }).map((_, i) => (
        <ConversationItemSkeleton key={i} />
      ))}
    </div>
  );
}

/**
 * Skeleton for a chat message
 */
export function ChatMessageSkeleton({ isUser = false }: { isUser?: boolean }) {
  return (
    <div className={`flex gap-3 ${isUser ? 'justify-end' : ''}`}>
      {!isUser && (
        <Skeleton className="h-8 w-8 rounded-full shrink-0" />
      )}
      <div className={`space-y-2 ${isUser ? 'max-w-[70%]' : 'flex-1 max-w-[70%]'}`}>
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-3/4" />
        {!isUser && <Skeleton className="h-4 w-1/2" />}
      </div>
      {isUser && (
        <Skeleton className="h-8 w-8 rounded-full shrink-0" />
      )}
    </div>
  );
}

/**
 * Skeleton for chat message list
 */
export function ChatMessageListSkeleton({ count = 4 }: { count?: number }) {
  return (
    <div className="space-y-6 p-4">
      {Array.from({ length: count }).map((_, i) => (
        <ChatMessageSkeleton key={i} isUser={i % 2 === 0} />
      ))}
    </div>
  );
}

/**
 * Skeleton for file item in files modal
 */
export function FileItemSkeleton() {
  return (
    <div className="flex items-start justify-between p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
      <div className="flex gap-3 flex-1">
        <Skeleton className="h-5 w-5 shrink-0" />
        <div className="flex-1 space-y-2">
          <Skeleton className="h-4 w-1/2" />
          <Skeleton className="h-3 w-1/3" />
        </div>
      </div>
      <div className="flex gap-2">
        <Skeleton className="h-8 w-8 rounded" />
        <Skeleton className="h-8 w-8 rounded" />
        <Skeleton className="h-8 w-8 rounded" />
      </div>
    </div>
  );
}

/**
 * Skeleton for file list in files modal
 */
export function FileListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <FileItemSkeleton key={i} />
      ))}
    </div>
  );
}

/**
 * Skeleton for knowledge item
 */
export function KnowledgeItemSkeleton() {
  return (
    <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
      <div className="flex items-start justify-between">
        <div className="flex gap-3 flex-1">
          <Skeleton className="h-10 w-10 rounded shrink-0" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-1/4" />
          </div>
        </div>
        <Skeleton className="h-8 w-8 rounded" />
      </div>
    </div>
  );
}

/**
 * Skeleton for knowledge list
 */
export function KnowledgeListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <KnowledgeItemSkeleton key={i} />
      ))}
    </div>
  );
}

/**
 * Skeleton for project card
 */
export function ProjectCardSkeleton() {
  return (
    <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
      <div className="flex items-start justify-between mb-3">
        <div className="space-y-2 flex-1">
          <Skeleton className="h-5 w-1/2" />
          <Skeleton className="h-3 w-3/4" />
        </div>
        <Skeleton className="h-6 w-16 rounded-full" />
      </div>
      <div className="flex gap-2">
        <Skeleton className="h-5 w-5" />
        <Skeleton className="h-4 w-20" />
      </div>
    </div>
  );
}

/**
 * Skeleton for project list
 */
export function ProjectListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <ProjectCardSkeleton key={i} />
      ))}
    </div>
  );
}

/**
 * Skeleton for modal content
 */
export function ModalContentSkeleton() {
  return (
    <div className="space-y-4 p-4">
      <Skeleton className="h-6 w-1/3" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-2/3" />
      <div className="flex gap-3 pt-4">
        <Skeleton className="h-10 w-24 rounded-md" />
        <Skeleton className="h-10 w-24 rounded-md" />
      </div>
    </div>
  );
}

/**
 * Skeleton for settings section
 */
export function SettingsSectionSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-5 w-32" />
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <div className="space-y-1">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-3 w-40" />
          </div>
          <Skeleton className="h-6 w-11 rounded-full" />
        </div>
        <div className="flex items-center justify-between">
          <div className="space-y-1">
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-3 w-44" />
          </div>
          <Skeleton className="h-6 w-11 rounded-full" />
        </div>
      </div>
    </div>
  );
}

/**
 * Full page loading skeleton
 */
export function PageLoadingSkeleton() {
  return (
    <div className="flex h-screen">
      {/* Sidebar skeleton */}
      <div className="w-80 border-r border-gray-200 dark:border-gray-800 p-4 space-y-4">
        <div className="flex items-center gap-2 mb-6">
          <Skeleton className="h-6 w-6" />
          <Skeleton className="h-6 w-20" />
        </div>
        <Skeleton className="h-10 w-full rounded-lg" />
        <ConversationListSkeleton count={6} />
      </div>
      {/* Main content skeleton */}
      <div className="flex-1 flex flex-col">
        <div className="border-b border-gray-200 dark:border-gray-800 p-4 flex items-center justify-between">
          <Skeleton className="h-8 w-32" />
          <Skeleton className="h-8 w-8 rounded" />
        </div>
        <div className="flex-1 p-4">
          <ChatMessageListSkeleton count={4} />
        </div>
        <div className="border-t border-gray-200 dark:border-gray-800 p-4">
          <Skeleton className="h-12 w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}
