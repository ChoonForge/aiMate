import { Share2, Copy, Mail, MessageSquare } from "lucide-react";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Separator } from "./ui/separator";
import { toast } from "sonner";
import { BaseDialog } from "./BaseDialog";

interface ShareDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  content: string;
  title?: string;
}

export function ShareDialog({
  open,
  onOpenChange,
  content,
  title = "Share Message",
}: ShareDialogProps) {
  const shareUrl = `https://chat.example.com/share/${Math.random().toString(36).substring(7)}`;

  const handleCopyLink = () => {
    navigator.clipboard.writeText(shareUrl);
    toast.success("Link copied to clipboard");
  };

  const handleCopyContent = () => {
    navigator.clipboard.writeText(content);
    toast.success("Content copied to clipboard");
  };

  return (
    <BaseDialog
      open={open}
      onOpenChange={onOpenChange}
      title={title}
      description="Share this message with others"
      size="md"
      showSave={false}
      showDelete={false}
      showCancel={true}
      cancelLabel="Close"
    >
      <div className="space-y-4">
        <div>
          <h3 className="font-semibold mb-3">Share Link</h3>
          <div className="flex gap-2">
            <Input value={shareUrl} readOnly className="flex-1" />
            <Button onClick={handleCopyLink} size="icon">
              <Copy className="h-4 w-4" />
            </Button>
          </div>
        </div>

        <Separator />

        <div>
          <h3 className="font-semibold mb-3">Quick Actions</h3>
          <div className="space-y-2">
            <Button
              variant="outline"
              className="w-full justify-start gap-3"
              onClick={handleCopyContent}
            >
              <Copy className="h-4 w-4" />
              Copy Content
            </Button>
            <Button
              variant="outline"
              className="w-full justify-start gap-3"
              onClick={() => toast.info("Email share not implemented")}
            >
              <Mail className="h-4 w-4" />
              Share via Email
            </Button>
            <Button
              variant="outline"
              className="w-full justify-start gap-3"
              onClick={() => toast.info("Social share not implemented")}
            >
              <MessageSquare className="h-4 w-4" />
              Share on Social Media
            </Button>
          </div>
        </div>
      </div>
    </BaseDialog>
  );
}
