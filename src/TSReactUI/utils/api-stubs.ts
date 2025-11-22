/**
 * API Stub Functions
 * These functions simulate API calls with mock data and logging
 */

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  metadata?: {
    endpoint: string;
    timestamp: Date;
    userId?: string;
  };
}

// User ID for mock API calls
const MOCK_USER_ID = "12324";

/**
 * Simulate API delay
 */
const delay = (ms: number = 300) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * Get all chats for user
 */
export async function getChats(userId: string = MOCK_USER_ID): Promise<ApiResponse<any[]>> {
  await delay();
  
  const chats = [
    { 
      id: "chat_001", 
      title: "Project Discussion", 
      date: "Nov 4, 2025",
      messageCount: 24,
      lastMessage: "Let's finalize the requirements",
      model: "GPT-4"
    },
    { 
      id: "chat_002", 
      title: "Code Review Session", 
      date: "Nov 3, 2025",
      messageCount: 15,
      lastMessage: "The implementation looks good",
      model: "Claude-3.5"
    },
    { 
      id: "chat_003", 
      title: "Architecture Planning", 
      date: "Nov 2, 2025",
      messageCount: 42,
      lastMessage: "We should use microservices",
      model: "GPT-4"
    },
    { 
      id: "chat_004", 
      title: "Bug Fix Discussion", 
      date: "Nov 1, 2025",
      messageCount: 8,
      lastMessage: "Fixed the authentication issue",
      model: "Claude-3.5"
    },
    { 
      id: "chat_005", 
      title: "Database Optimization", 
      date: "Oct 31, 2025",
      messageCount: 19,
      lastMessage: "Added proper indexes",
      model: "GPT-4"
    },
  ];

  return {
    success: true,
    data: chats,
    metadata: {
      endpoint: `/api/v1/GetChats?UserID=${userId}`,
      timestamp: new Date(),
      userId
    }
  };
}

/**
 * Get all notes for user
 */
export async function getNotes(userId: string = MOCK_USER_ID): Promise<ApiResponse<any[]>> {
  await delay();
  
  const notes = [
    { 
      id: "note_001", 
      title: "Project Requirements", 
      content: "List of requirements for the new project including authentication, API design, and database schema.",
      date: "Nov 3, 2025",
      tags: ["project", "requirements"]
    },
    { 
      id: "note_002", 
      title: "Meeting Notes", 
      content: "Discussion points from today's meeting about the sprint planning and deliverables.",
      date: "Nov 2, 2025",
      tags: ["meeting", "sprint"]
    },
    { 
      id: "note_003", 
      title: "Design Ideas", 
      content: "Ideas for the new design system including color palette, typography, and component library.",
      date: "Oct 30, 2025",
      tags: ["design", "ui"]
    },
    { 
      id: "note_004", 
      title: "API Endpoints", 
      content: "Documentation for all REST API endpoints with request/response examples.",
      date: "Oct 28, 2025",
      tags: ["api", "documentation"]
    },
  ];

  return {
    success: true,
    data: notes,
    metadata: {
      endpoint: `/api/v1/GetNotes?UserID=${userId}`,
      timestamp: new Date(),
      userId
    }
  };
}

/**
 * Get knowledge base items for user
 */
export async function getKnowledge(userId: string = MOCK_USER_ID): Promise<ApiResponse<any[]>> {
  await delay();
  
  const knowledge = [
    { 
      id: "kb_001", 
      title: "React Documentation - Hooks & Context API", 
      type: "Web",
      url: "https://react.dev",
      date: "Nov 1, 2025",
      size: "2.4 MB",
      tags: ["React", "Frontend", "JavaScript"],
      summary: "Comprehensive guide to React hooks, context, and modern patterns for building user interfaces.",
      entities: ["useState", "useEffect", "Context API", "Custom Hooks"],
      collection: "Frontend Development",
      usageCount: 42,
      lastUsed: "Nov 5, 2025",
      source: "react.dev",
      version: 3,
      updatedBy: "John Doe"
    },
    { 
      id: "kb_002", 
      title: "TypeScript Handbook - Advanced Types", 
      type: "PDF",
      url: "typescript-handbook.pdf",
      date: "Oct 28, 2025",
      size: "1.8 MB",
      tags: ["TypeScript", "Programming", "Types"],
      summary: "Deep dive into TypeScript's type system including generics, conditional types, and mapped types.",
      entities: ["Generics", "Union Types", "Intersection Types", "Type Guards"],
      collection: "Frontend Development",
      usageCount: 35,
      lastUsed: "Nov 4, 2025",
      source: "TypeScript Docs",
      version: 2,
      updatedBy: "Sarah Smith"
    },
    { 
      id: "kb_003", 
      title: "Company Guidelines - Code Review Best Practices", 
      type: "Document",
      url: "company-guidelines.docx",
      date: "Oct 25, 2025",
      size: "856 KB",
      tags: ["Guidelines", "Best Practices", "Code Review"],
      summary: "Internal guidelines for conducting effective code reviews and maintaining code quality standards.",
      entities: ["Code Review", "Pull Requests", "Quality Standards"],
      collection: "API Documentation",
      usageCount: 28,
      lastUsed: "Nov 3, 2025",
      source: "Internal Docs",
      version: 5,
      updatedBy: "Mike Johnson"
    },
    { 
      id: "kb_004", 
      title: "REST API Authentication - OAuth 2.0 Guide", 
      type: "Web",
      url: "https://restfulapi.net",
      date: "Oct 20, 2025",
      size: "1.2 MB",
      tags: ["API", "Auth", "OAuth", "Security"],
      summary: "Complete guide to implementing OAuth 2.0 authentication in REST APIs with code examples.",
      entities: ["OAuth 2.0", "JWT", "Access Tokens", "Refresh Tokens"],
      collection: "API Documentation",
      usageCount: 56,
      lastUsed: "Nov 6, 2025",
      source: "restfulapi.net",
      version: 1,
      updatedBy: "Emily Davis"
    },
    { 
      id: "kb_005", 
      title: "Design System Specification v2.0", 
      type: "PDF",
      url: "design-system.pdf",
      date: "Oct 18, 2025",
      size: "3.1 MB",
      tags: ["Design", "UI", "Components", "Figma"],
      summary: "Complete design system including components, color palette, typography, spacing, and usage guidelines.",
      entities: ["Color Palette", "Typography", "Components", "Spacing System"],
      collection: "Design Specs",
      usageCount: 19,
      lastUsed: "Nov 1, 2025",
      source: "Figma",
      version: 2,
      updatedBy: "Design Team"
    },
    { 
      id: "kb_006", 
      title: "PostgreSQL Performance Tuning", 
      type: "PDF",
      url: "postgres-tuning.pdf",
      date: "Oct 15, 2025",
      size: "2.2 MB",
      tags: ["Database", "PostgreSQL", "Performance"],
      summary: "Advanced techniques for optimizing PostgreSQL database performance including indexing strategies.",
      entities: ["Indexing", "Query Optimization", "EXPLAIN ANALYZE", "Vacuum"],
      collection: "API Documentation",
      usageCount: 23,
      lastUsed: "Oct 30, 2025",
      source: "PostgreSQL Wiki",
      version: 1,
      updatedBy: "Dev Team"
    },
    { 
      id: "kb_007", 
      title: "Meeting Notes - Q4 Product Planning", 
      type: "Document",
      url: "q4-planning.docx",
      date: "Oct 10, 2025",
      size: "425 KB",
      tags: ["Meeting", "Planning", "Product"],
      summary: "Notes from Q4 planning meeting covering roadmap, priorities, and resource allocation.",
      entities: ["Roadmap", "Priorities", "Resources", "Timeline"],
      collection: "Meeting Notes",
      usageCount: 12,
      lastUsed: "Oct 28, 2025",
      source: "Internal",
      version: 1,
      updatedBy: "Product Team"
    },
    { 
      id: "kb_008", 
      title: "Docker Containerization Tutorial", 
      type: "Video",
      url: "docker-tutorial.mp4",
      date: "Oct 5, 2025",
      size: "156 MB",
      tags: ["Docker", "DevOps", "Containers"],
      summary: "Video tutorial covering Docker basics, Dockerfile creation, and container orchestration.",
      entities: ["Dockerfile", "Docker Compose", "Containers", "Images"],
      collection: "Frontend Development",
      usageCount: 8,
      lastUsed: "Oct 25, 2025",
      source: "YouTube",
      version: 1,
      updatedBy: "DevOps Team"
    },
    { 
      id: "kb_009", 
      title: "GraphQL API Schema Design", 
      type: "Code",
      url: "graphql-schema.graphql",
      date: "Sep 28, 2025",
      size: "89 KB",
      tags: ["GraphQL", "API", "Schema"],
      summary: "Complete GraphQL schema definition with queries, mutations, and subscriptions.",
      entities: ["Queries", "Mutations", "Subscriptions", "Types"],
      collection: "API Documentation",
      usageCount: 15,
      lastUsed: "Oct 20, 2025",
      source: "GitHub",
      version: 3,
      updatedBy: "API Team"
    },
    { 
      id: "kb_010", 
      title: "Tailwind CSS Cheat Sheet", 
      type: "Image",
      url: "tailwind-cheatsheet.png",
      date: "Sep 20, 2025",
      size: "1.5 MB",
      tags: ["CSS", "Tailwind", "Frontend"],
      summary: "Visual reference for Tailwind CSS utility classes and responsive design patterns.",
      entities: ["Utilities", "Responsive", "Flexbox", "Grid"],
      collection: "Frontend Development",
      usageCount: 0,
      lastUsed: undefined,
      source: "Tailwind Labs",
      version: 1,
      updatedBy: "Frontend Team"
    },
  ];

  return {
    success: true,
    data: knowledge,
    metadata: {
      endpoint: `/api/v1/GetKnowledge?UserID=${userId}`,
      timestamp: new Date(),
      userId
    }
  };
}

/**
 * Get files for user
 */
export async function getFiles(userId: string = MOCK_USER_ID): Promise<ApiResponse<any[]>> {
  await delay();
  
  const files = [
    { 
      id: "file_001", 
      title: "presentation.pdf", 
      size: "2.4 MB",
      type: "PDF",
      uploadDate: "Nov 5, 2025",
      path: "/documents/presentation.pdf"
    },
    { 
      id: "file_002", 
      title: "data.xlsx", 
      size: "1.1 MB",
      type: "Spreadsheet",
      uploadDate: "Nov 4, 2025",
      path: "/documents/data.xlsx"
    },
    { 
      id: "file_003", 
      title: "image.png", 
      size: "850 KB",
      type: "Image",
      uploadDate: "Nov 3, 2025",
      path: "/images/image.png"
    },
    { 
      id: "file_004", 
      title: "report.docx", 
      size: "456 KB",
      type: "Document",
      uploadDate: "Nov 2, 2025",
      path: "/documents/report.docx"
    },
  ];

  return {
    success: true,
    data: files,
    metadata: {
      endpoint: `/api/v1/GetFiles?UserID=${userId}`,
      timestamp: new Date(),
      userId
    }
  };
}

/**
 * Attach a file to the current chat
 */
export async function attachFile(fileId: string, chatId: string): Promise<ApiResponse<any>> {
  await delay();
  
  return {
    success: true,
    data: {
      fileId,
      chatId,
      attached: true,
      timestamp: new Date()
    },
    metadata: {
      endpoint: `/api/v1/AttachFile`,
      timestamp: new Date()
    }
  };
}

/**
 * Attach a note to the current chat
 */
export async function attachNote(noteId: string, chatId: string): Promise<ApiResponse<any>> {
  await delay();
  
  return {
    success: true,
    data: {
      noteId,
      chatId,
      attached: true,
      timestamp: new Date()
    },
    metadata: {
      endpoint: `/api/v1/AttachNote`,
      timestamp: new Date()
    }
  };
}

/**
 * Attach knowledge to the current chat
 */
export async function attachKnowledge(knowledgeId: string, chatId: string): Promise<ApiResponse<any>> {
  await delay();
  
  return {
    success: true,
    data: {
      knowledgeId,
      chatId,
      attached: true,
      timestamp: new Date()
    },
    metadata: {
      endpoint: `/api/v1/AttachKnowledge`,
      timestamp: new Date()
    }
  };
}

/**
 * Reference another chat in the current chat
 */
export async function referenceChat(referenceChatId: string, currentChatId: string): Promise<ApiResponse<any>> {
  await delay();
  
  return {
    success: true,
    data: {
      referenceChatId,
      currentChatId,
      referenced: true,
      timestamp: new Date()
    },
    metadata: {
      endpoint: `/api/v1/ReferenceChat`,
      timestamp: new Date()
    }
  };
}

/**
 * Attach a webpage to the current chat
 */
export async function attachWebpage(url: string, chatId: string): Promise<ApiResponse<any>> {
  await delay();
  
  return {
    success: true,
    data: {
      url,
      chatId,
      attached: true,
      timestamp: new Date(),
      title: "Webpage Title",
      description: "Webpage description extracted from URL"
    },
    metadata: {
      endpoint: `/api/v1/AttachWebpage`,
      timestamp: new Date()
    }
  };
}

/**
 * Test connection to an AI model/API
 */
export async function testConnection(
  provider: string,
  apiKey: string,
  endpoint?: string,
  modelId?: string
): Promise<ApiResponse<{ status: string; latency: number; model?: string }>> {
  // Simulate longer delay for connection test
  await delay(1500);
  
  // Simulate 90% success rate
  const success = Math.random() > 0.1;
  
  if (success) {
    return {
      success: true,
      data: {
        status: "connected",
        latency: Math.floor(Math.random() * 500) + 100, // 100-600ms
        model: modelId || `${provider}-default-model`
      },
      metadata: {
        endpoint: `/api/v1/TestConnection`,
        timestamp: new Date()
      }
    };
  } else {
    return {
      success: false,
      error: "Connection failed. Please check your API key and endpoint.",
      metadata: {
        endpoint: `/api/v1/TestConnection`,
        timestamp: new Date()
      }
    };
  }
}
