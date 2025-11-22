import { ReactNode } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { ScrollArea } from "./ui/scroll-area";

interface BaseDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  children: ReactNode;
  onSave?: () => void;
  onDelete?: () => void;
  saveLabel?: string;
  deleteLabel?: string;
  cancelLabel?: string;
  showSave?: boolean;
  showDelete?: boolean;
  showCancel?: boolean;
  isSaveDisabled?: boolean;
  isDeleteDisabled?: boolean;
  size?: "sm" | "md" | "lg";
}

export function BaseDialog({
  open,
  onOpenChange,
  title,
  description,
  children,
  onSave,
  onDelete,
  saveLabel = "Save",
  deleteLabel = "Delete",
  cancelLabel = "Cancel",
  showSave = true,
  showDelete = false,
  showCancel = true,
  isSaveDisabled = false,
  isDeleteDisabled = false,
  size = "md",
}: BaseDialogProps) {
  const sizeClasses = {
    sm: "max-w-md",
    md: "max-w-2xl",
    lg: "max-w-4xl",
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className={`${sizeClasses[size]} w-full h-full sm:w-[90vw] sm:h-auto sm:max-h-[85vh] md:w-auto md:max-h-[90vh] p-0 gap-0 flex flex-col`}
      >
        {/* Header */}
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex flex-row items-center justify-between">
          <div>
            <DialogTitle>{title}</DialogTitle>
            {description && (
              <DialogDescription className="mt-1">
                {description}
              </DialogDescription>
            )}
          </div>
        </DialogHeader>

        {/* Content with Vertical Scrolling */}
        <ScrollArea className="flex-1">
          <div className="px-6 py-4 space-y-4">{children}</div>
        </ScrollArea>

        {/* Footer */}
        <div className="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex justify-between">
          {showDelete && onDelete ? (
            <Button
              variant="destructive"
              onClick={onDelete}
              disabled={isDeleteDisabled}
            >
              {deleteLabel}
            </Button>
          ) : (
            <div />
          )}
          <div className="flex gap-2">
            {showCancel && (
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                {cancelLabel}
              </Button>
            )}
            {showSave && onSave && (
              <Button onClick={onSave} disabled={isSaveDisabled}>
                {saveLabel}
              </Button>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
