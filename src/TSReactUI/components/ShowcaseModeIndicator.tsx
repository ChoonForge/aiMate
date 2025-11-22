import { Camera } from "lucide-react";
import { useDebug } from "./DebugContext";

export function ShowcaseModeIndicator() {
  const { showcaseMode } = useDebug();

  if (!showcaseMode) return null;

  return (
    <div className="fixed top-0 left-0 right-0 z-[9999] bg-purple-600 text-white py-2 px-4 flex items-center justify-center gap-2 shadow-lg">
      <Camera className="h-4 w-4 animate-pulse" />
      <span className="font-medium text-sm">
        Showcase Mode - Click menus to toggle, they won't auto-close
      </span>
    </div>
  );
}