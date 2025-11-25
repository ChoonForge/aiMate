import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import { Facebook, Twitter, Linkedin, Mail, Link, MessageCircle } from "lucide-react";
import { toast } from "sonner";

interface ShareModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  conversationTitle?: string;
}

const shareOptions = [
  {
    id: "link",
    name: "Copy Link",
    icon: Link,
    color: "bg-gray-500 hover:bg-gray-600",
  },
  {
    id: "twitter",
    name: "Twitter",
    icon: Twitter,
    color: "bg-blue-400 hover:bg-blue-500",
  },
  {
    id: "facebook",
    name: "Facebook",
    icon: Facebook,
    color: "bg-blue-600 hover:bg-blue-700",
  },
  {
    id: "linkedin",
    name: "LinkedIn",
    icon: Linkedin,
    color: "bg-blue-700 hover:bg-blue-800",
  },
  {
    id: "whatsapp",
    name: "WhatsApp",
    icon: MessageCircle,
    color: "bg-green-500 hover:bg-green-600",
  },
  {
    id: "email",
    name: "Email",
    icon: Mail,
    color: "bg-gray-700 hover:bg-gray-800",
  },
];

export function ShareModal({ open, onOpenChange, conversationTitle }: ShareModalProps) {
  const handleShare = (platform: string) => {
    if (platform === "link") {
      // Copy link to clipboard
      const url = window.location.href;
      navigator.clipboard.writeText(url);
      toast.success("Link copied to clipboard");
    } else {
      toast.info(`Share to ${platform} not implemented`);
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Share Conversation</DialogTitle>
          <DialogDescription>
            {conversationTitle ? `Share "${conversationTitle}"` : "Share this conversation"}
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-3 gap-4 py-4">
          {shareOptions.map((option) => (
            <button
              key={option.id}
              onClick={() => handleShare(option.id)}
              className="flex flex-col items-center gap-2 p-4 rounded-lg border border-gray-200 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors group"
            >
              <div className={`${option.color} p-3 rounded-full transition-colors`}>
                <option.icon className="h-5 w-5 text-white" />
              </div>
              <span className="text-sm font-medium">{option.name}</span>
            </button>
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}
