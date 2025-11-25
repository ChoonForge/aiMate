import { Sparkles } from "lucide-react";
import { BaseDialog } from "./BaseDialog";

interface AboutModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function AboutModal({ open, onOpenChange }: AboutModalProps) {
  return (
    <BaseDialog
      open={open}
      onOpenChange={onOpenChange}
      title="About Aimate"
      description="Aimate AI chat system"
      size="md"
      showSave={false}
      showDelete={false}
      showCancel={false}
    >
      <div className="text-center space-y-2 py-4">
        <div className="flex items-center justify-center mb-4">
          <Sparkles className="h-8 w-8 text-purple-500" />
        </div>
        <p className="text-gray-700 dark:text-gray-300">
          Aimate AI chat system.
        </p>
        <p className="text-gray-500 dark:text-gray-400">
          Version 0.0.1
        </p>
        <p className="text-gray-500 dark:text-gray-400">
          Copyright 2025 Aimate
        </p>
      </div>
    </BaseDialog>
  );
}
