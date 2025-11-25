import { MessageSquare, Lightbulb, Code, Pencil, Sparkles, Table } from "lucide-react";

const suggestions = [
  {
    icon: MessageSquare,
    title: "Have a conversation",
    description: "Ask me anything you'd like to know",
    prompt: "Let's chat",
  },
  {
    icon: Lightbulb,
    title: "Get creative ideas",
    description: "Brainstorm new concepts and solutions",
    prompt: "Let's brainstorm ideas",
  },
  {
    icon: Code,
    title: "Write code",
    description: "Help with programming questions",
    prompt: "Help me with code",
  },
  {
    icon: Table,
    title: "Structured content demo",
    description: "Try: GetTable, GetList, GetKV, GetForm",
    prompt: "GetTable",
  },
];

interface EmptyStateProps {
  onSendMessage?: (message: string) => void;
}

export function EmptyState({ onSendMessage }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center h-full px-4 py-8">
      <div className="max-w-2xl w-full space-y-8">
        <div className="text-center space-y-4">
          <div className="flex justify-center">
            <div className="h-16 w-16 rounded-full bg-gradient-to-r from-purple-600 to-blue-500 flex items-center justify-center">
              <Sparkles className="h-8 w-8 text-white" />
            </div>
          </div>
          <h2 className="text-3xl">How can I help you today?</h2>
          <p className="text-gray-500 dark:text-gray-400">
            Start a conversation with AI assistant
          </p>
        </div>
        
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          {suggestions.map((suggestion, index) => (
            <button
              key={index}
              className="flex flex-col items-start gap-2 p-4 rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 hover:border-purple-500 dark:hover:border-purple-500 transition-colors text-left group"
              onClick={() => onSendMessage?.(suggestion.prompt)}
            >
              <suggestion.icon className="h-5 w-5 text-purple-500 group-hover:text-purple-600 transition-colors" />
              <div>
                <div className="font-medium">{suggestion.title}</div>
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  {suggestion.description}
                </div>
              </div>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
