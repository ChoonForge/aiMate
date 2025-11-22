import { useState } from "react";
import { Input } from "./ui/input";
import { Search, HelpCircle, ChevronRight } from "lucide-react";
import { BaseModal } from "./BaseModal";

interface HelpModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const faqs = [
  {
    id: 1,
    question: "How do I start a new chat?",
    answer: "Click the 'New Chat' button in the top right corner or use the sidebar to start a fresh conversation.",
    category: "Getting Started",
  },
  {
    id: 2,
    question: "Can I edit my messages?",
    answer: "Yes! Hover over your message and click the edit icon. This will regenerate the AI's response based on your edited message.",
    category: "Features",
  },
  {
    id: 3,
    question: "How do I switch between AI models?",
    answer: "Use the model dropdown in the header to select from available models like GPT-4, Claude, and others.",
    category: "Models",
  },
  {
    id: 4,
    question: "How do I attach files or notes?",
    answer: "Click the '+' button in the chat input area to access attachment options including files, webpages, notes, and knowledge base.",
    category: "Features",
  },
  {
    id: 5,
    question: "What are Tools and how do I enable them?",
    answer: "Tools are special capabilities like Web Search and Code Interpreter. Click the wrench icon in the chat input to enable/disable tools.",
    category: "Tools",
  },
  {
    id: 6,
    question: "How do I change the theme?",
    answer: "Go to Settings → Interface → Theme to switch between Light, Dark, or System theme.",
    category: "Customization",
  },
  {
    id: 7,
    question: "Can I delete conversations?",
    answer: "Yes, hover over a conversation in the sidebar and click the trash icon to delete it.",
    category: "Chat Management",
  },
  {
    id: 8,
    question: "How do I regenerate an AI response?",
    answer: "Click the regenerate icon below the AI's last message to get a new response.",
    category: "Features",
  },
  {
    id: 9,
    question: "What's the difference between the models?",
    answer: "Different models have varying capabilities, speeds, and costs. GPT-4 is most capable, GPT-3.5 is faster, and Claude offers different strengths.",
    category: "Models",
  },
  {
    id: 10,
    question: "How do I access Admin Panel?",
    answer: "Click your profile menu at the bottom of the sidebar and select 'Admin Panel' to configure system settings.",
    category: "Administration",
  },
  {
    id: 11,
    question: "How do I share a conversation?",
    answer: "Click the share button in the message actions or chat header to generate a shareable link.",
    category: "Features",
  },
  {
    id: 12,
    question: "What are Knowledge Base integrations?",
    answer: "You can connect external knowledge sources to provide context for your conversations.",
    category: "Features",
  },
  {
    id: 13,
    question: "How do I manage my subscription?",
    answer: "Go to Settings → Account → Subscription to view and manage your plan.",
    category: "Account",
  },
  {
    id: 14,
    question: "Can I export my data?",
    answer: "Yes, go to Settings → Data & Privacy → Export Data to download all your conversations.",
    category: "Account",
  },
  {
    id: 15,
    question: "How do I report a bug?",
    answer: "Use the Help menu to submit feedback or report issues directly to our support team.",
    category: "Support",
  },
];

export function HelpModal({ open, onOpenChange }: HelpModalProps) {
  const [searchQuery, setSearchQuery] = useState("");

  const filteredFaqs = faqs.filter(
    (faq) =>
      faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.category.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const groupedFaqs = filteredFaqs.reduce((acc, faq) => {
    if (!acc[faq.category]) {
      acc[faq.category] = [];
    }
    acc[faq.category].push(faq);
    return acc;
  }, {} as Record<string, typeof faqs>);

  return (
    <BaseModal
      open={open}
      onOpenChange={onOpenChange}
      title="Help & FAQ"
      description="Frequently asked questions and help documentation"
      icon={HelpCircle}
      size="lg"
      showSave={false}
      showDelete={false}
    >
      {/* Search Bar */}
      <div className="mb-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
          <Input
            placeholder="Search for help..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>

      {/* FAQ List */}
      <div className="space-y-6">
        {Object.keys(groupedFaqs).length === 0 ? (
          <div className="text-center py-8 text-gray-500 dark:text-gray-400">
            No results found for "{searchQuery}"
          </div>
        ) : (
          Object.entries(groupedFaqs).map(([category, items]) => (
            <div key={category}>
              <h3 className="font-semibold mb-3 text-purple-600 dark:text-purple-400">
                {category}
              </h3>
              <div className="space-y-2">
                {items.map((faq) => (
                  <details
                    key={faq.id}
                    className="group p-4 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
                  >
                    <summary className="flex items-center justify-between cursor-pointer list-none">
                      <span className="font-medium">{faq.question}</span>
                      <ChevronRight className="h-4 w-4 text-gray-400 transition-transform group-open:rotate-90" />
                    </summary>
                    <p className="mt-3 text-sm text-gray-600 dark:text-gray-300">
                      {faq.answer}
                    </p>
                  </details>
                ))}
              </div>
            </div>
          ))
        )}
      </div>
    </BaseModal>
  );
}
