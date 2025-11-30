import { useState, useEffect, useCallback, useRef } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import {
  Brain,
  FileText,
  X,
  ChevronDown,
  ChevronUp,
  Sparkles,
  Plus,
  FolderOpen,
  Loader2
} from "lucide-react";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "./ui/collapsible";
import { knowledgeService } from "../api/services";
import { AppConfig } from "../utils/config";

interface KnowledgeItem {
  id: string;
  title: string;
  type: string;
  collection?: string;
  summary?: string;
}

interface Collection {
  id: string;
  name: string;
  itemCount: number;
}

interface KnowledgeSuggestionsProps {
  inputValue: string;
  attachedKnowledge: string[];
  onAttach: (id: string) => void;
  onDetach: (id: string) => void;
  onAttachCollection: (collectionId: string) => void;
}

export function KnowledgeSuggestions({
  inputValue,
  attachedKnowledge,
  onAttach,
  onDetach,
  onAttachCollection
}: KnowledgeSuggestionsProps) {
  const [suggestions, setSuggestions] = useState<KnowledgeItem[]>([]);
  const [collections, setCollections] = useState<Collection[]>([]);
  const [isOpen, setIsOpen] = useState(true);
  const [showCollections, setShowCollections] = useState(false);
  const [searching, setSearching] = useState(false);
  const debounceRef = useRef<NodeJS.Timeout | null>(null);

  // Semantic search with debouncing
  const performSemanticSearch = useCallback(async (query: string) => {
    if (query.length < 15) {
      setSuggestions([]);
      return;
    }

    setSearching(true);
    try {
      const result = await knowledgeService.semanticSearch({ query, limit: 5 });
      const items: KnowledgeItem[] = result.results.map((r: any) => ({
        id: r.id,
        title: r.title,
        type: r.type || 'Document',
        collection: r.collection,
        summary: r.content?.substring(0, 150) + '...',
      }));
      setSuggestions(items);
      setIsOpen(items.length > 0);
    } catch (err) {
      console.error('[KnowledgeSuggestions] Semantic search failed:', err);
      // Fallback to keyword-based suggestions
      const fallbackSuggestions = generateKeywordSuggestions(query);
      setSuggestions(fallbackSuggestions);
      setIsOpen(fallbackSuggestions.length > 0);
    } finally {
      setSearching(false);
    }
  }, []);

  // Debounced search effect
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    if (inputValue.length > 15) {
      debounceRef.current = setTimeout(() => {
        performSemanticSearch(inputValue);
      }, 500); // 500ms debounce
    } else {
      setSuggestions([]);
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [inputValue, performSemanticSearch]);

  useEffect(() => {
    // Load collections
    setCollections([
      { id: "col_1", name: "API Documentation", itemCount: 15 },
      { id: "col_2", name: "Frontend Development", itemCount: 23 },
      { id: "col_3", name: "Design Specs", itemCount: 12 },
    ]);
  }, []);

  // Fallback keyword-based suggestions for offline mode or API errors
  const generateKeywordSuggestions = (query: string): KnowledgeItem[] => {
    const lowerQuery = query.toLowerCase();

    if (lowerQuery.includes("auth") || lowerQuery.includes("login") || lowerQuery.includes("oauth")) {
      return [
        {
          id: "kb_004",
          title: "REST API Authentication - OAuth 2.0 Guide",
          type: "Web",
          collection: "API Documentation",
          summary: "Complete guide to implementing OAuth 2.0 authentication in REST APIs"
        },
        {
          id: "kb_003",
          title: "Company Guidelines - Code Review Best Practices",
          type: "Document",
          collection: "API Documentation",
          summary: "Internal guidelines for conducting effective code reviews"
        }
      ];
    }

    if (lowerQuery.includes("react") || lowerQuery.includes("hooks") || lowerQuery.includes("component")) {
      return [
        {
          id: "kb_001",
          title: "React Documentation - Hooks & Context API",
          type: "Web",
          collection: "Frontend Development",
          summary: "Comprehensive guide to React hooks, context, and modern patterns"
        },
        {
          id: "kb_002",
          title: "TypeScript Handbook - Advanced Types",
          type: "PDF",
          collection: "Frontend Development",
          summary: "Deep dive into TypeScript's type system"
        }
      ];
    }

    if (lowerQuery.includes("design") || lowerQuery.includes("ui")) {
      return [
        {
          id: "kb_005",
          title: "Design System Specification v2.0",
          type: "PDF",
          collection: "Design Specs",
          summary: "Complete design system including components, color palette, typography"
        },
        {
          id: "kb_010",
          title: "Tailwind CSS Cheat Sheet",
          type: "Image",
          collection: "Frontend Development",
          summary: "Visual reference for Tailwind CSS utility classes"
        }
      ];
    }

    if (lowerQuery.includes("database") || lowerQuery.includes("sql") || lowerQuery.includes("postgres")) {
      return [
        {
          id: "kb_006",
          title: "PostgreSQL Performance Tuning",
          type: "PDF",
          collection: "API Documentation",
          summary: "Advanced techniques for optimizing PostgreSQL database performance"
        }
      ];
    }

    if (lowerQuery.includes("api") || lowerQuery.includes("graphql") || lowerQuery.includes("rest")) {
      return [
        {
          id: "kb_009",
          title: "GraphQL API Schema Design",
          type: "Code",
          collection: "API Documentation",
          summary: "Complete GraphQL schema definition with queries, mutations, and subscriptions"
        },
        {
          id: "kb_004",
          title: "REST API Authentication - OAuth 2.0 Guide",
          type: "Web",
          collection: "API Documentation",
          summary: "Complete guide to implementing OAuth 2.0 authentication"
        }
      ];
    }

    return [];
  };

  const relevantSuggestions = suggestions.filter(s => !attachedKnowledge.includes(s.id));

  // Show loading indicator while searching
  if (searching) {
    return (
      <div className="mb-3 flex items-center gap-2 text-sm text-gray-500">
        <Loader2 className="h-4 w-4 animate-spin" />
        <span>Searching knowledge base...</span>
      </div>
    );
  }

  if (relevantSuggestions.length === 0) {
    return null;
  }

  return (
    <div className="mb-3">
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
        <div className="flex items-center justify-between mb-2">
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="sm" className="gap-2 p-0 h-auto hover:bg-transparent">
              <Sparkles className="h-4 w-4 text-purple-500" />
              <span className="text-sm font-medium">
                Suggested Knowledge
                <Badge variant="secondary" className="ml-2">
                  {relevantSuggestions.length}
                </Badge>
              </span>
              {isOpen ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </Button>
          </CollapsibleTrigger>
        </div>

        <CollapsibleContent className="space-y-2">
          {/* Suggested Knowledge */}
          {relevantSuggestions.length > 0 && (
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <p className="text-xs text-gray-600 dark:text-gray-400 flex items-center gap-1">
                  <Sparkles className="h-3 w-3" />
                  Relevant to your query
                </p>
                <Button
                  variant="link"
                  size="sm"
                  className="h-auto p-0 text-xs"
                  onClick={() => setShowCollections(!showCollections)}
                >
                  {showCollections ? "Hide collections" : "Or attach entire collection"}
                </Button>
              </div>

              {showCollections && (
                <div className="flex flex-wrap gap-2 mb-2">
                  {collections.map((col) => (
                    <Button
                      key={col.id}
                      variant="outline"
                      size="sm"
                      className="gap-2 h-8"
                      onClick={() => onAttachCollection(col.id)}
                    >
                      <FolderOpen className="h-3 w-3" />
                      <span className="text-xs">{col.name}</span>
                      <Badge variant="secondary" className="ml-1 text-xs">
                        {col.itemCount}
                      </Badge>
                    </Button>
                  ))}
                </div>
              )}
              
              <div className="space-y-1">
                {relevantSuggestions.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-start gap-2 p-2 border border-gray-200 dark:border-gray-800 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors group"
                  >
                    <FileText className="h-4 w-4 mt-0.5 text-gray-400 flex-shrink-0" />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium truncate">{item.title}</p>
                          {item.summary && (
                            <p className="text-xs text-gray-600 dark:text-gray-400 line-clamp-1">
                              {item.summary}
                            </p>
                          )}
                          {item.collection && (
                            <p className="text-xs text-purple-600 dark:text-purple-400 mt-0.5">
                              {item.collection}
                            </p>
                          )}
                        </div>
                        <Button
                          size="sm"
                          variant="ghost"
                          className="h-7 gap-1 opacity-0 group-hover:opacity-100 transition-opacity"
                          onClick={() => onAttach(item.id)}
                        >
                          <Plus className="h-3 w-3" />
                          <span className="text-xs">Attach</span>
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CollapsibleContent>
      </Collapsible>
    </div>
  );
}
