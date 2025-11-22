import { useState } from "react";
import { Button } from "./ui/button";
import { Textarea } from "./ui/textarea";
import { ThumbsUp, ThumbsDown } from "lucide-react";
import { toast } from "sonner@2.0.3";
import { BaseDialog } from "./BaseDialog";

interface RatingModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  type: "up" | "down";
}

export function RatingModal({ open, onOpenChange, type }: RatingModalProps) {
  const [selectedRating, setSelectedRating] = useState<number | null>(null);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [feedback, setFeedback] = useState("");

  const tags = [
    "Followed Instructions",
    "Detailed Response",
    "Accurate Information",
    "Well Formatted",
    "Creative Solution",
    "Clear Explanation",
    "Helpful Examples",
    "Quick Response",
  ];

  const handleToggleTag = (tag: string) => {
    setSelectedTags((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag]
    );
  };

  const handleSave = () => {
    // Save rating logic here
    console.log({
      type,
      rating: selectedRating,
      tags: selectedTags,
      feedback,
    });
    toast.success("Rating saved!");
    onOpenChange(false);
    // Reset state
    setSelectedRating(null);
    setSelectedTags([]);
    setFeedback("");
  };

  return (
    <BaseDialog
      open={open}
      onOpenChange={onOpenChange}
      title={type === "up" ? "Positive Feedback" : "Negative Feedback"}
      description="Provide detailed feedback about this response"
      size="lg"
      onSave={handleSave}
      showSave={true}
      showDelete={false}
    >
      <div className="space-y-6">
        {/* Rating Scale */}
        <div>
          <h3 className="font-semibold mb-3">Rate this response</h3>
          <div className="flex gap-2 flex-wrap">
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((num) => (
              <Button
                key={num}
                variant={selectedRating === num ? "default" : "outline"}
                size="sm"
                onClick={() => setSelectedRating(num)}
                className={
                  selectedRating === num
                    ? "bg-purple-500 hover:bg-purple-600"
                    : ""
                }
              >
                {num}
              </Button>
            ))}
          </div>
        </div>

        {/* Tags */}
        <div>
          <h3 className="font-semibold mb-3">What did you like/dislike?</h3>
          <div className="flex gap-2 flex-wrap">
            {tags.map((tag) => (
              <Button
                key={tag}
                variant={selectedTags.includes(tag) ? "default" : "outline"}
                size="sm"
                onClick={() => handleToggleTag(tag)}
                className={
                  selectedTags.includes(tag)
                    ? "bg-purple-500 hover:bg-purple-600"
                    : ""
                }
              >
                {tag}
              </Button>
            ))}
          </div>
        </div>

        {/* Feedback Text */}
        <div>
          <h3 className="font-semibold mb-3">Additional Comments</h3>
          <Textarea
            placeholder="Tell us more about your experience..."
            value={feedback}
            onChange={(e) => setFeedback(e.target.value)}
            className="min-h-[120px]"
          />
        </div>
      </div>
    </BaseDialog>
  );
}
