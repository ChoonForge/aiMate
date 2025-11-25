import { useState } from "react";
import { Avatar, AvatarFallback } from "./ui/avatar";
import { Button } from "./ui/button";
import { Textarea } from "./ui/textarea";
import { Bot, User, Edit2, Check, X, RotateCw, Sparkles, Copy, Volume2, Info, ThumbsUp, ThumbsDown, Play, Share2, Send, Brain } from "lucide-react";
import ReactMarkdown from "react-markdown";
import { toast } from "sonner";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "./ui/popover";
import {
  Tooltip,
  TooltipTrigger,
  TooltipContent,
} from "./ui/tooltip";
import { RatingModal } from "./RatingModal";
import { ShareModal } from "./ShareModal";
import { StructuredPanel } from "./StructuredPanel";

interface ChatMessageProps {
  role: "user" | "assistant";
  content: string;
  timestamp: string;
  structuredContent?: any;
  onEdit?: (newContent: string) => void;
  onRegenerate?: () => void;
}

export function ChatMessage({
  role,
  content,
  timestamp,
  structuredContent,
  onEdit,
  onRegenerate,
}: ChatMessageProps) {
  const isUser = role === "user";
  const [isEditing, setIsEditing] = useState(false);
  const [editedContent, setEditedContent] = useState(content);
  const [infoOpen, setInfoOpen] = useState(false);
  const [ratingOpen, setRatingOpen] = useState(false);
  const [ratingType, setRatingType] = useState<"up" | "down">("up");
  const [shareOpen, setShareOpen] = useState(false);
  
  // Mock performance metrics
  const performanceMetrics = {
    tokensPerSecond: 12.27,
    totalTokens: 68,
    timeSeconds: 5.54,
  };

  const handleSaveEdit = () => {
    if (editedContent.trim() && onEdit) {
      onEdit(editedContent.trim());
      setIsEditing(false);
    }
  };

  const handleCancelEdit = () => {
    setEditedContent(content);
    setIsEditing(false);
  };

  const [copyTooltipOpen, setCopyTooltipOpen] = useState(false);
  const [brainTooltipOpen, setBrainTooltipOpen] = useState(false);

  const handleCopy = async () => {
    try {
      // Try modern Clipboard API first
      await navigator.clipboard.writeText(content);
      setCopyTooltipOpen(true);
      setTimeout(() => setCopyTooltipOpen(false), 2000);
    } catch (err) {
      // Fallback to execCommand for when Clipboard API is blocked
      try {
        const textArea = document.createElement('textarea');
        textArea.value = content;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        textArea.style.top = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        const successful = document.execCommand('copy');
        document.body.removeChild(textArea);
        
        if (successful) {
          setCopyTooltipOpen(true);
          setTimeout(() => setCopyTooltipOpen(false), 2000);
        } else {
          toast.error("Failed to copy to clipboard");
        }
      } catch (fallbackErr) {
        toast.error("Failed to copy to clipboard");
      }
    }
  };

  const handleReadAloud = () => {
    toast.info("Text-to-speech not implemented");
  };

  const handleLike = () => {
    setRatingType("up");
    setRatingOpen(true);
  };

  const handleDislike = () => {
    setRatingType("down");
    setRatingOpen(true);
  };

  const handleContinue = () => {
    toast.info("Continue not implemented");
  };

  const handleShare = () => {
    setShareOpen(true);
  };

  const handleSaveToKnowledge = () => {
    setBrainTooltipOpen(true);
    setTimeout(() => setBrainTooltipOpen(false), 2000);
  };

  return (
    <>
      <RatingModal
        open={ratingOpen}
        onOpenChange={setRatingOpen}
        type={ratingType}
      />
      <ShareModal
        open={shareOpen}
        onOpenChange={setShareOpen}
      />
      <div className={`flex gap-3 ${isUser ? "flex-row-reverse" : "flex-row"} group`}>
        <Avatar className="h-8 w-8 shrink-0 hidden md:flex">
        {isUser ? (
          <AvatarFallback className="bg-gray-600 dark:bg-gray-700">
            <User className="h-4 w-4 text-white" />
          </AvatarFallback>
        ) : (
          <AvatarFallback className="bg-gray-600 dark:bg-gray-700">
            <Sparkles className="h-4 w-4 text-white" />
          </AvatarFallback>
        )}
      </Avatar>

      <div className={`flex flex-col max-w-[95%] md:max-w-[80%] ${isUser ? "items-end" : "items-start"}`}>
        {isEditing ? (
          <div className="w-full space-y-2">
            <Textarea
              value={editedContent}
              onChange={(e) => setEditedContent(e.target.value)}
              className="min-h-[100px]"
              autoFocus
            />
            <div className="flex gap-2">
              <Button
                size="sm"
                onClick={handleSaveEdit}
                className="gap-2"
              >
                <Send className="h-3 w-3" />
                Submit
              </Button>
              <Button
                size="sm"
                variant="outline"
                onClick={handleCancelEdit}
                className="gap-2"
              >
                <X className="h-3 w-3" />
                Cancel
              </Button>
            </div>
          </div>
        ) : (
          <>
            <div
              className={`rounded-2xl px-3 py-2 ${
                isUser
                  ? "bg-gray-700 dark:bg-gray-700 text-white"
                  : "bg-gray-100 text-gray-900 dark:bg-gray-800 dark:text-gray-100"
              }`}
            >
              {isUser ? (
                <p className="whitespace-pre-wrap break-words">{content}</p>
              ) : (
                <div className="prose prose-sm dark:prose-invert max-w-none prose-p:my-2 prose-pre:my-2 prose-ul:my-2 prose-ol:my-2">
                  <ReactMarkdown
                    components={{
                      code: ({ node, inline, className, children, ...props }: any) => {
                        // Hide structured content code blocks
                        const codeString = String(children);
                        if (!inline && codeString.includes('```structured')) {
                          return null;
                        }
                        if (!inline && className === 'language-structured') {
                          return null;
                        }
                        
                        return inline ? (
                          <code
                            className="bg-gray-200 dark:bg-gray-700 px-1.5 py-0.5 rounded text-sm"
                            {...props}
                          >
                            {children}
                          </code>
                        ) : (
                          <code
                            className="block bg-gray-200 dark:bg-gray-700 p-3 rounded text-sm overflow-x-auto"
                            {...props}
                          >
                            {children}
                          </code>
                        );
                      },
                    }}
                  >
                    {structuredContent ? content.replace(/```structured[\s\S]*?```/g, '').trim() : content}
                  </ReactMarkdown>
                </div>
              )}
            </div>

            {/* Structured Content */}
            {structuredContent && (
              <div className="w-full mt-2">
                <StructuredPanel 
                  data={structuredContent} 
                  onAction={(action, rowData) => {
                    toast.info(`Action: ${action.title} (Tool: ${action.tool})`);
                    console.log('Structured action:', action, rowData);
                  }}
                />
              </div>
            )}
            
            {/* Action Icons */}
            {!isUser ? (
              <div className="flex items-center gap-1 mt-2">
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={() => setIsEditing(true)}
                  title="Edit"
                >
                  <Edit2 className="h-4 w-4" />
                </Button>
                <Tooltip open={copyTooltipOpen}>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                      onClick={handleCopy}
                      title="Copy"
                    >
                      <Copy className="h-4 w-4" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>Copied to clipboard</TooltipContent>
                </Tooltip>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={handleReadAloud}
                  title="Read Aloud"
                >
                  <Volume2 className="h-4 w-4" />
                </Button>
                <Popover open={infoOpen} onOpenChange={setInfoOpen}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                      title="Info"
                    >
                      <Info className="h-4 w-4" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent side="top" className="w-auto p-3">
                    <div className="space-y-1">
                      <h4 className="font-medium text-sm mb-2">Performance Metrics</h4>
                      <div className="text-sm text-gray-600 dark:text-gray-300 font-mono">
                        {performanceMetrics.tokensPerSecond} T/s | {performanceMetrics.totalTokens} tokens | {performanceMetrics.timeSeconds} sec
                      </div>
                    </div>
                  </PopoverContent>
                </Popover>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={handleLike}
                  title="Like"
                >
                  <ThumbsUp className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={handleDislike}
                  title="Dislike"
                >
                  <ThumbsDown className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={handleContinue}
                  title="Continue"
                >
                  <Play className="h-4 w-4" />
                </Button>
                {onRegenerate && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    onClick={onRegenerate}
                    title="Regenerate"
                  >
                    <RotateCw className="h-4 w-4" />
                  </Button>
                )}
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                  onClick={handleShare}
                  title="Share"
                >
                  <Share2 className="h-4 w-4" />
                </Button>
                <Tooltip open={brainTooltipOpen}>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                      onClick={handleSaveToKnowledge}
                      title="Save to Knowledge"
                    >
                      <Brain className="h-4 w-4" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>Added to knowledge</TooltipContent>
                </Tooltip>
              </div>
            ) : (
              <div className="flex items-center gap-2 mt-1 px-2">
                <span className="text-xs text-gray-500">{timestamp}</span>
                {onEdit && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                    onClick={() => setIsEditing(true)}
                  >
                    <Edit2 className="h-3 w-3" />
                  </Button>
                )}
                <Tooltip open={copyTooltipOpen}>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                      onClick={handleCopy}
                      title="Copy"
                    >
                      <Copy className="h-3 w-3" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>Copied to clipboard</TooltipContent>
                </Tooltip>
                <Tooltip open={brainTooltipOpen}>
                  <TooltipTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                      onClick={handleSaveToKnowledge}
                      title="Save to Knowledge"
                    >
                      <Brain className="h-3 w-3" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>Added to knowledge</TooltipContent>
                </Tooltip>
              </div>
            )}
          </>
        )}
      </div>
    </div>
    </>
  );
}
