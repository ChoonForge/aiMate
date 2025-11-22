import { ReactNode, useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { ScrollArea } from "./ui/scroll-area";
import { X } from "lucide-react";
import { useSwipeGesture } from "../utils/useSwipeGesture";

interface Tab {
  id: string;
  label: string;
  icon?: React.ComponentType<{ className?: string }>;
  content: ReactNode;
}

interface BaseModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  icon?: React.ComponentType<{ className?: string }>;
  tabs?: Tab[];
  children?: ReactNode;
  onSave?: () => void;
  onDelete?: () => void;
  saveLabel?: string;
  deleteLabel?: string;
  showSave?: boolean;
  showDelete?: boolean;
  isSaveDisabled?: boolean;
  isDeleteDisabled?: boolean;
  size?: "md" | "lg" | "xl";
}

export function BaseModal({
  open,
  onOpenChange,
  title,
  description,
  icon: Icon,
  tabs,
  children,
  onSave,
  onDelete,
  saveLabel = "Save",
  deleteLabel = "Delete",
  showSave = true,
  showDelete = false,
  isSaveDisabled = false,
  isDeleteDisabled = false,
  size = "lg",
}: BaseModalProps) {
  const [activeTab, setActiveTab] = useState(tabs?.[0]?.id || "");

  // Reset to first tab when modal opens
  useEffect(() => {
    if (open && tabs && tabs.length > 0) {
      setActiveTab(tabs[0].id);
    }
  }, [open]); // Only reset when modal opens, not when tabs change

  // Swipe gesture handlers for tabs
  const handleSwipeLeft = () => {
    if (!tabs) return;
    const currentIndex = tabs.findIndex((tab) => tab.id === activeTab);
    if (currentIndex < tabs.length - 1) {
      setActiveTab(tabs[currentIndex + 1].id);
    }
  };

  const handleSwipeRight = () => {
    if (!tabs) return;
    const currentIndex = tabs.findIndex((tab) => tab.id === activeTab);
    if (currentIndex > 0) {
      setActiveTab(tabs[currentIndex - 1].id);
    }
  };

  const swipeRef = useSwipeGesture({
    onSwipeLeft: handleSwipeLeft,
    onSwipeRight: handleSwipeRight,
  });

  const sizeClasses = {
    md: "max-w-2xl",
    lg: "max-w-2xl lg:max-w-5xl",
    xl: "max-w-2xl lg:max-w-7xl",
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className={`${sizeClasses[size]} w-full h-full sm:w-[90vw] sm:h-auto sm:max-h-[85vh] md:w-[80vw] md:max-h-[90vh] md:h-[90vh] p-0 gap-0 flex flex-col`}
      >
        {/* Header */}
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <DialogTitle className="flex items-center gap-2">
            {Icon && <Icon className="h-5 w-5 text-purple-500" />}
            {title}
          </DialogTitle>
          {description && (
            <DialogDescription>{description}</DialogDescription>
          )}
        </DialogHeader>

        {/* Horizontal Scrolling Tabs */}
        {tabs && tabs.length > 0 && (
          <div className="flex-shrink-0 border-b border-gray-200 dark:border-gray-800 overflow-x-auto tab-scrollbar">
            <div className="flex px-6 gap-1 min-w-max h-full">
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={(e) => {
                      setActiveTab(tab.id);
                      e.currentTarget.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
                    }}
                    className={`flex items-center gap-2 px-4 py-2 border-b-2 transition-colors whitespace-nowrap cursor-pointer ${
                      activeTab === tab.id
                        ? "border-purple-500 text-purple-600 dark:text-purple-400"
                        : "border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                    }`}
                  >
                    {Icon && <Icon className="h-4 w-4" />}
                    {tab.label}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Content with Vertical Scrolling */}
        <div className="flex-1 overflow-hidden">
          <ScrollArea className="h-full">
            <div ref={swipeRef} className="px-6 py-4 space-y-4">
              {tabs
                ? tabs.map(
                    (tab) =>
                      activeTab === tab.id && (
                        <div key={tab.id}>{tab.content}</div>
                      )
                  )
                : children}
            </div>
          </ScrollArea>
        </div>

        {/* Footer */}
        {(showSave || showDelete) && (
          <div className="px-6 h-[60px] flex-shrink-0 border-t border-gray-200 dark:border-gray-800 flex items-center justify-between">
            <div>
              {showDelete && onDelete && (
                <Button
                  variant="destructive"
                  onClick={onDelete}
                  disabled={isDeleteDisabled}
                >
                  {deleteLabel}
                </Button>
              )}
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              {showSave && onSave && (
                <Button onClick={onSave} disabled={isSaveDisabled}>
                  {saveLabel}
                </Button>
              )}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
