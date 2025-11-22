# Structured Content System - Complete Specification

## Overview
The Structured Content System enables AI responses to include rich, interactive, structured data that can be rendered in various formats with customizable templates, styling, and actions.

**Think:** ChatGPT Canvas + Claude Artifacts, but more extensible and customizable.

---

## 🎯 Goals

1. **Rich AI Responses** - Enable AI to return structured data (tables, forms, charts, etc.)
2. **Interactive** - Support user actions (view, edit, delete, export, etc.)
3. **Extensible** - Easy to add new content types and templates
4. **Customizable** - Support custom templates and styling
5. **Type-Safe** - Backend validation of schemas
6. **Performant** - Lazy loading, pagination, caching

---

## 🏗️ Architecture

### Core Components

```
┌─────────────────────────────────────────────────────────┐
│                     AI Response                          │
│  Contains structured content marker + JSON payload      │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│            StructuredContentPlugin                       │
│  • Intercepts messages with structured content          │
│  • Parses JSON schema                                   │
│  • Validates against template                           │
│  • Adds metadata for UI rendering                       │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│         StructuredContentService                         │
│  • Template management                                   │
│  • Schema validation                                     │
│  • Action execution                                      │
│  • Export functionality                                  │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              Action Handlers                             │
│  • NavigationHandler (open URLs, routes)                │
│  • ModalHandler (open modals)                           │
│  • ApiCallHandler (call backend APIs)                   │
│  • ExportHandler (CSV, JSON, Excel)                     │
│  • CopyHandler (copy to clipboard)                      │
│  • CustomHandler (user-defined)                         │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Content Types

### Built-in Types

| Type | Description | Use Cases |
|------|-------------|-----------|
| `Table` | Tabular data with sorting, filtering, pagination | Data lists, reports, search results |
| `KeyValueList` | Key-value pairs, property lists | Configuration, settings, object details |
| `Card` | Card-based grid layouts | Dashboards, galleries, product catalogs |
| `Timeline` | Chronological events | History, activity logs, project milestones |
| `Chart` | Data visualizations (bar, line, pie, etc.) | Analytics, metrics, statistics |
| `Form` | Interactive forms with validation | Data entry, surveys, configuration |
| `CodeBlock` | Code with syntax highlighting & actions | Code snippets, file contents, diffs |
| `FileTree` | Hierarchical file/folder structures | Project explorer, directory listing |
| `Gallery` | Image/media grid with lightbox | Photos, videos, attachments |
| `Kanban` | Kanban board with drag-drop | Task management, workflows |
| `Custom` | User-defined types | Anything not covered above |

---

## 🔧 JSON Schema Format

### Base Structure

```json
{
  "structuredContent": {
    "version": "1.0",
    "type": "table|keyValueList|card|...",
    "template": "default|compact|cards|custom-name",
    "metadata": {
      "title": "Display title",
      "description": "Optional description",
      "icon": "icon-name",
      "color": "#hex-color",
      "tags": ["tag1", "tag2"]
    },
    "schema": {
      // Type-specific schema definition
    },
    "data": {
      // Actual data payload
    },
    "options": {
      // Rendering options
    },
    "styles": {
      // Custom styling
    }
  }
}
```

### Table Schema

```json
{
  "schema": {
    "columns": [
      {
        "key": "fieldName",
        "label": "Display Name",
        "type": "string|number|boolean|date|enum|badge|user|custom",
        "sortable": true,
        "filterable": true,
        "width": "20%",
        "align": "left|center|right",
        "format": "currency|percentage|date|custom",
        "required": true,
        "validation": {
          "min": 0,
          "max": 100,
          "pattern": "regex"
        },
        "colorMap": {
          "value1": "color1",
          "value2": "color2"
        },
        "icons": {
          "value1": "icon1"
        }
      }
    ],
    "actions": {
      "row": [
        {
          "id": "unique-action-id",
          "label": "Action Label",
          "icon": "icon-name",
          "handler": "navigation|modal|apiCall|export|copy|custom",
          "method": "GET|POST|PUT|DELETE",
          "endpoint": "/api/endpoint/{id}",
          "confirm": true,
          "permissions": ["required-permission"],
          "style": "primary|secondary|danger|warning|success",
          "parameters": {
            "key": "value"
          }
        }
      ],
      "bulk": [
        {
          "id": "export-selected",
          "label": "Export",
          "formats": ["csv", "json", "excel"]
        }
      ],
      "global": [
        {
          "id": "create-new",
          "label": "Create New",
          "handler": "modal"
        }
      ]
    }
  },
  "data": [
    {
      "id": "unique-id",
      "field1": "value1",
      "field2": "value2"
    }
  ],
  "options": {
    "searchable": true,
    "filterable": true,
    "sortable": true,
    "paginated": true,
    "pageSize": 20,
    "exportable": true,
    "selectable": true,
    "multiSelect": true,
    "expandable": false,
    "stickyHeader": true,
    "dense": false
  }
}
```

### KeyValueList Schema

```json
{
  "schema": {
    "sections": [
      {
        "title": "Section Title",
        "description": "Optional description",
        "collapsible": true,
        "defaultExpanded": true,
        "items": [
          {
            "key": "propertyName",
            "label": "Display Label",
            "type": "string|number|boolean|date|duration|url|email|json",
            "value": "value",
            "copyable": true,
            "editable": false,
            "format": "date|currency|bytes|duration",
            "unit": "seconds|MB|etc",
            "icon": "icon-name",
            "tooltip": "Help text",
            "link": "https://...",
            "badge": {
              "text": "Badge text",
              "color": "color"
            }
          }
        ],
        "actions": [
          {
            "id": "edit-section",
            "label": "Edit",
            "handler": "modal"
          }
        ]
      }
    ],
    "actions": {
      "item": [
        {
          "id": "copy",
          "label": "Copy Value",
          "handler": "copy"
        }
      ],
      "global": [
        {
          "id": "export",
          "label": "Export Configuration",
          "handler": "export"
        }
      ]
    }
  },
  "data": {
    "propertyName": "value",
    "anotherProperty": 123
  }
}
```

### Card Schema

```json
{
  "schema": {
    "card": {
      "title": "{{fieldName}}",
      "subtitle": "{{anotherField}}",
      "description": "{{description}}",
      "image": "{{imageUrl}}",
      "badge": {
        "field": "status",
        "colorMap": {
          "active": "green",
          "inactive": "gray"
        }
      },
      "metadata": [
        {
          "label": "Created",
          "field": "createdAt",
          "format": "date"
        }
      ]
    },
    "layout": {
      "columns": 3,
      "gap": "medium",
      "aspectRatio": "16:9"
    },
    "actions": {
      "primary": {
        "id": "view",
        "label": "View",
        "handler": "navigation"
      },
      "secondary": [
        {
          "id": "edit",
          "label": "Edit"
        }
      ]
    }
  },
  "data": [
    {
      "id": "card-1",
      "fieldName": "Title",
      "imageUrl": "https://..."
    }
  ]
}
```

### Chart Schema

```json
{
  "schema": {
    "chartType": "bar|line|pie|doughnut|radar|scatter|area",
    "axes": {
      "x": {
        "label": "X Axis",
        "type": "category|linear|time"
      },
      "y": {
        "label": "Y Axis",
        "format": "number|currency|percentage"
      }
    },
    "series": [
      {
        "name": "Series 1",
        "color": "#color",
        "type": "line|bar"
      }
    ],
    "legend": {
      "position": "top|bottom|left|right",
      "align": "start|center|end"
    }
  },
  "data": {
    "labels": ["Jan", "Feb", "Mar"],
    "datasets": [
      {
        "label": "Sales",
        "data": [100, 200, 150]
      }
    ]
  }
}
```

---

## 🎨 Template System

### Template Definition

```csharp
public class StructuredContentTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "default", "compact", "custom-name"
    public StructuredContentType Type { get; set; } // Table, KeyValueList, etc.
    public string SchemaJson { get; set; }         // JSON schema definition
    public string? ComponentPath { get; set; }     // Optional custom component path
    public string? StylesJson { get; set; }        // Custom CSS/Tailwind classes
    public bool IsBuiltIn { get; set; }
    public bool IsPublic { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Built-in Templates

| Type | Template | Description |
|------|----------|-------------|
| Table | `default` | Standard table with all features |
| Table | `compact` | Dense table, minimal padding |
| Table | `cards` | Card-based mobile-friendly layout |
| Table | `minimal` | Clean, no borders |
| KeyValueList | `default` | Standard property list |
| KeyValueList | `horizontal` | Horizontal layout |
| KeyValueList | `tabs` | Tabbed sections |
| Card | `grid` | Grid layout |
| Card | `masonry` | Pinterest-style masonry |
| Card | `list` | List view with images |

---

## ⚡ Action System

### Action Types

```csharp
public enum ActionHandler
{
    Navigation,    // Navigate to URL or route
    Modal,         // Open modal dialog
    ApiCall,       // Call backend API
    Export,        // Export data
    Copy,          // Copy to clipboard
    Custom         // User-defined handler
}
```

### Action Definition

```json
{
  "id": "unique-id",
  "label": "Button Label",
  "icon": "icon-name",
  "handler": "navigation|modal|apiCall|export|copy|custom",
  "style": "primary|secondary|danger|warning|success|ghost",
  "size": "sm|md|lg",
  "position": "left|right",
  "shortcut": "Ctrl+S",
  "permissions": ["required-permission"],
  "confirm": {
    "enabled": true,
    "title": "Confirm Action",
    "message": "Are you sure?",
    "confirmText": "Yes",
    "cancelText": "No"
  },
  "config": {
    // Handler-specific configuration
  }
}
```

### Handler-Specific Config

**Navigation:**
```json
{
  "handler": "navigation",
  "config": {
    "url": "/route/{id}",
    "target": "_self|_blank",
    "params": {
      "key": "value"
    }
  }
}
```

**Modal:**
```json
{
  "handler": "modal",
  "config": {
    "component": "EditForm",
    "title": "Edit Item",
    "size": "sm|md|lg|xl|full",
    "data": {
      "itemId": "{id}"
    }
  }
}
```

**API Call:**
```json
{
  "handler": "apiCall",
  "config": {
    "method": "GET|POST|PUT|DELETE",
    "endpoint": "/api/endpoint/{id}",
    "headers": {
      "Custom-Header": "value"
    },
    "body": {
      "key": "value"
    },
    "successMessage": "Action completed",
    "errorMessage": "Action failed",
    "refreshOnSuccess": true
  }
}
```

**Export:**
```json
{
  "handler": "export",
  "config": {
    "formats": ["csv", "json", "excel", "pdf"],
    "filename": "export-{date}",
    "includeColumns": ["col1", "col2"],
    "excludeColumns": ["internal-id"]
  }
}
```

---

## 🎨 Styling System

### Theme Integration

```json
{
  "styles": {
    "theme": "light|dark|auto",
    "colors": {
      "primary": "tailwind-class or #hex",
      "secondary": "tailwind-class",
      "accent": "tailwind-class",
      "background": "tailwind-class",
      "text": "tailwind-class"
    },
    "spacing": "tight|comfortable|relaxed",
    "borders": true,
    "rounded": "none|sm|md|lg|full",
    "shadows": "none|sm|md|lg|xl",
    "animations": true,
    "font": {
      "family": "font-sans|font-serif|font-mono",
      "size": "text-sm|text-base|text-lg",
      "weight": "font-normal|font-medium|font-bold"
    }
  }
}
```

### Custom CSS Classes

```json
{
  "styles": {
    "container": "max-w-7xl mx-auto p-6",
    "header": "text-2xl font-bold mb-4",
    "table": "w-full border-collapse",
    "row": "hover:bg-gray-50 transition",
    "cell": "px-4 py-2 border-b"
  }
}
```

---

## 🔌 API Endpoints

### Template Management

```
GET    /api/v1/structured-content/templates
GET    /api/v1/structured-content/templates/{id}
POST   /api/v1/structured-content/templates
PUT    /api/v1/structured-content/templates/{id}
DELETE /api/v1/structured-content/templates/{id}
```

### Content Operations

```
POST   /api/v1/structured-content/parse         - Parse and validate content
POST   /api/v1/structured-content/render        - Render with template
POST   /api/v1/structured-content/validate      - Validate against schema
```

### Action Execution

```
POST   /api/v1/structured-content/actions/{actionId}/execute
```

### Export

```
POST   /api/v1/structured-content/export        - Export to various formats
```

---

## 📝 AI Integration

### Prompt Instructions

```markdown
You can return structured data that will be beautifully rendered. Use this format:

```structuredcontent
{
  "type": "table",
  "metadata": {
    "title": "Results"
  },
  "schema": {
    "columns": [...],
    "actions": [...]
  },
  "data": [...]
}
```

**Supported Types:**
- `table` - Tabular data with sorting/filtering
- `keyValueList` - Property lists, configurations
- `card` - Card-based layouts
- `chart` - Data visualizations
- `form` - Interactive forms
- `codeBlock` - Code snippets
- `fileTree` - File hierarchies

**When to use:**
- Presenting lists of items
- Showing configuration/settings
- Displaying data that benefits from structure
- Creating interactive interfaces

**Example - User List:**
```json
{
  "type": "table",
  "metadata": { "title": "Users" },
  "schema": {
    "columns": [
      { "key": "name", "label": "Name", "type": "string" },
      { "key": "email", "label": "Email", "type": "string" },
      { "key": "role", "label": "Role", "type": "badge" }
    ],
    "actions": {
      "row": [
        { "id": "view", "label": "View", "handler": "navigation" }
      ]
    }
  },
  "data": [
    { "name": "Alice", "email": "alice@example.com", "role": "Admin" }
  ]
}
```
```

---

## 🚀 Implementation Phases

### Phase 1: Core Infrastructure ✅
- [x] Entity models
- [x] Database schema
- [x] Service interfaces
- [x] Basic CRUD operations

### Phase 2: Content Types 🔄
- [ ] Table type implementation
- [ ] KeyValueList type implementation
- [ ] Card type implementation
- [ ] Chart type (basic)

### Phase 3: Actions ⏳
- [ ] Action handler system
- [ ] Built-in handlers
- [ ] Custom handler support

### Phase 4: Templates ⏳
- [ ] Template management
- [ ] Built-in templates
- [ ] Custom template support

### Phase 5: Export ⏳
- [ ] CSV export
- [ ] JSON export
- [ ] Excel export

### Phase 6: Advanced Features ⏳
- [ ] Schema validation
- [ ] Template inheritance
- [ ] Action chaining
- [ ] Conditional rendering
- [ ] Real-time updates

---

## 🔒 Security Considerations

1. **Validation**
   - Validate all schemas against allowed types
   - Sanitize user input in templates
   - Validate action endpoints

2. **Permissions**
   - Check user permissions before action execution
   - Respect row-level security
   - Template visibility control

3. **Rate Limiting**
   - Limit action execution frequency
   - Prevent export abuse
   - API call throttling

4. **Data Privacy**
   - Mask sensitive data
   - Respect field-level permissions
   - Audit action execution

---

## 📊 Performance Optimization

1. **Lazy Loading**
   - Load data on scroll
   - Paginate large datasets
   - Defer action handlers

2. **Caching**
   - Cache templates
   - Cache schema validations
   - Memoize computed values

3. **Optimization**
   - Index frequently queried fields
   - Batch API calls
   - Minimize re-renders

---

## 🧪 Testing Strategy

1. **Unit Tests**
   - Schema validation
   - Template parsing
   - Action execution

2. **Integration Tests**
   - End-to-end content rendering
   - Action handler integration
   - Export functionality

3. **Performance Tests**
   - Large dataset rendering
   - Export performance
   - Action execution speed

---

## 📚 Examples

See `/docs/examples/structured-content/` for:
- Table examples
- KeyValueList examples
- Card examples
- Chart examples
- Custom template examples
- Action handler examples

---

## 🔗 Related Documentation

- [Plugin System](./PLUGIN_SYSTEM.md)
- [API Documentation](./api/README.md)
- [Code Generation Spec](./CODE_GENERATION_SPEC.md)

---

**Version:** 1.0
**Last Updated:** 2025-01-22
**Status:** In Development
