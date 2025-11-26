# aiMate API - Complete CRUD Operations Reference

**Version:** 2.0
**Date:** 2025-01-18
**Status:** Comprehensive CRUD mapping for all API resources

---

## Table of Contents

1. [CRUD Operations Overview](#crud-operations-overview)
2. [Complete CRUD Resources](#complete-crud-resources)
3. [Partial CRUD Resources (Missing Operations)](#partial-crud-resources-missing-operations)
4. [Implementation Guide](#implementation-guide)
5. [Standard CRUD Patterns](#standard-crud-patterns)

---

## CRUD Operations Overview

All aiMate API resources follow RESTful CRUD principles:

- **CREATE** - `POST /api/v1/{resource}` - Create new resource
- **READ** - `GET /api/v1/{resource}` - Get one/all resources
- **UPDATE** - `PUT /api/v1/{resource}/{id}` - Update existing resource
- **DELETE** - `DELETE /api/v1/{resource}/{id}` - Delete resource

---

## Complete CRUD Resources

These resources have full CRUD implementation:

### 1. Workspaces

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List all workspaces | GET | `/api/v1/workspaces` | ✅ Implemented |
| Get workspace by ID | GET | `/api/v1/workspaces/{id}` | ✅ Implemented |
| Create workspace | POST | `/api/v1/workspaces` | ✅ Implemented |
| Update workspace | PUT | `/api/v1/workspaces/{id}` | ✅ Implemented |
| Delete workspace | DELETE | `/api/v1/workspaces/{id}` | ✅ Implemented |

**Example Requests:**

```bash
# Create
POST /api/v1/workspaces
{
  "name": "My Project",
  "description": "Project workspace",
  "type": "Project",
  "personality": "KiwiDev"
}

# Read
GET /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000

# Update
PUT /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000
{
  "name": "Updated Name",
  "description": "New description"
}

# Delete
DELETE /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000
```

---

### 2. Conversations

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List conversations in workspace | GET | `/api/v1/workspaces/{workspaceId}/conversations` | ✅ Implemented |
| Get conversation by ID | GET | `/api/v1/workspaces/{workspaceId}/conversations/{conversationId}` | ✅ **NEW** |
| Create conversation | POST | `/api/v1/workspaces/{workspaceId}/conversations` | ✅ Implemented |
| Update conversation | PUT | `/api/v1/workspaces/{workspaceId}/conversations/{conversationId}` | ✅ **NEW** |
| Delete conversation | DELETE | `/api/v1/workspaces/{workspaceId}/conversations/{conversationId}` | ✅ **NEW** |

**Example Requests:**

```bash
# Create
POST /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations
{
  "title": "Project Discussion"
}

# Read single
GET /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000

# Update
PUT /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000
{
  "title": "Updated Title",
  "modelId": "gpt-4",
  "isPinned": true,
  "isArchived": false
}

# Delete
DELETE /api/v1/workspaces/550e8400-e29b-41d4-a716-446655440000/conversations/660e8400-e29b-41d4-a716-446655440000
```

---

### 3. Code Files

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List code files | GET | `/api/v1/codefiles` | ✅ Implemented |
| Get code file by ID | GET | `/api/v1/codefiles/{id}` | ✅ Implemented |
| Create code file | POST | `/api/v1/codefiles` | ✅ Implemented |
| Update code file | PUT | `/api/v1/codefiles/{id}` | ✅ Implemented |
| Delete code file | DELETE | `/api/v1/codefiles/{id}` | ✅ Implemented |

---

### 4. Connections

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List connections | GET | `/api/v1/connection` | ✅ Implemented |
| Get connection by ID | GET | `/api/v1/connection/{id}` | ✅ Implemented |
| Create connection | POST | `/api/v1/connection` | ✅ Implemented |
| Update connection | PUT | `/api/v1/connection/{id}` | ✅ Implemented |
| Delete connection | DELETE | `/api/v1/connection/{id}` | ✅ Implemented |
| Test connection | POST | `/api/v1/connection/{id}/test` | ✅ Action |

---

### 5. Knowledge Articles

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List knowledge articles | GET | `/api/v1/knowledge` | ✅ Implemented |
| Get article by ID | GET | `/api/v1/knowledge/{id}` | ✅ Implemented |
| Create article | POST | `/api/v1/knowledge` | ✅ Implemented |
| Update article | PUT | `/api/v1/knowledge/{id}` | ✅ Implemented |
| Delete article | DELETE | `/api/v1/knowledge/{id}` | ✅ Implemented |

---

### 6. Notes

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List notes | GET | `/api/v1/notesapi` | ✅ Implemented |
| Get note by ID | GET | `/api/v1/notesapi/{id}` | ✅ Implemented |
| Create note | POST | `/api/v1/notesapi` | ✅ Implemented |
| Update note | PUT | `/api/v1/notesapi/{id}` | ✅ Implemented |
| Delete note | DELETE | `/api/v1/notesapi/{id}` | ✅ Implemented |

---

### 7. Projects

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List projects | GET | `/api/v1/projects` | ✅ Implemented |
| Get project by ID | GET | `/api/v1/projects/{id}` | ✅ Implemented |
| Create project | POST | `/api/v1/projects` | ✅ Implemented |
| Update project | PUT | `/api/v1/projects/{id}` | ✅ Implemented |
| Delete project | DELETE | `/api/v1/projects/{id}` | ✅ Implemented |

---

### 8. Groups

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List groups | GET | `/api/v1/group` | ✅ Implemented |
| Get group by ID | GET | `/api/v1/group/{id}` | ✅ Implemented |
| Create group | POST | `/api/v1/group` | ✅ Implemented |
| Update group | PUT | `/api/v1/group/{id}` | ✅ Implemented |
| Delete group | DELETE | `/api/v1/group/{id}` | ✅ Implemented |

**Group Members Sub-resource:**

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List group members | GET | `/api/v1/group/{groupId}/members` | ✅ Implemented |
| Add member to group | POST | `/api/v1/group/{groupId}/members` | ✅ Implemented |
| Update member in group | PUT | `/api/v1/group/{groupId}/members/{memberId}` | ✅ Implemented |
| Remove member from group | DELETE | `/api/v1/group/{groupId}/members/{memberId}` | ✅ Implemented |

---

### 9. Organizations

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List organizations | GET | `/api/v1/organization` | ✅ Implemented |
| Get organization by ID | GET | `/api/v1/organization/{id}` | ✅ Implemented |
| Create organization | POST | `/api/v1/organization` | ✅ Implemented |
| Update organization | PUT | `/api/v1/organization/{id}` | ✅ Implemented |
| Delete organization | DELETE | `/api/v1/organization/{id}` | ✅ Implemented |

**Organization Members Sub-resource:**

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List org members | GET | `/api/v1/organization/{orgId}/members` | ✅ Implemented |
| Add member to org | POST | `/api/v1/organization/{orgId}/members` | ✅ Implemented |
| Update member in org | PUT | `/api/v1/organization/{orgId}/members/{memberId}` | ✅ Implemented |
| Remove member from org | DELETE | `/api/v1/organization/{orgId}/members/{memberId}` | ✅ Implemented |

---

### 10. Feedback Templates

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List feedback templates | GET | `/api/v1/feedback/templates` | ✅ Implemented |
| Get template by ID | GET | `/api/v1/feedback/templates/{id}` | ✅ Implemented |
| Create template | POST | `/api/v1/feedback/templates` | ✅ Implemented |
| Update template | PUT | `/api/v1/feedback/templates/{id}` | ✅ Implemented |
| Delete template | DELETE | `/api/v1/feedback/templates/{id}` | ✅ Implemented |

---

### 11. User Feedback & Error Logs

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List user feedback | GET | `/api/v1/system-feedback` | ✅ Implemented |
| Get feedback by ID | GET | `/api/v1/system-feedback/{id}` | ✅ Implemented |
| Create feedback | POST | `/api/v1/system-feedback` | ✅ Implemented |
| Update feedback | PUT | `/api/v1/system-feedback/{id}` | ✅ Implemented |
| Delete feedback | DELETE | `/api/v1/system-feedback/{id}` | ✅ Implemented |

---

### 12. Structured Content Templates

| Operation | Method | Endpoint | Status |
|-----------|--------|----------|--------|
| List templates | GET | `/api/v1/structuredcontent` | ✅ Implemented |
| Get template by ID | GET | `/api/v1/structuredcontent/{id}` | ✅ Implemented |
| Create template | POST | `/api/v1/structuredcontent` | ✅ Implemented |
| Update template | PUT | `/api/v1/structuredcontent/{id}` | ✅ Implemented |
| Delete template | DELETE | `/api/v1/structuredcontent/{id}` | ✅ Implemented |

---

## Partial CRUD Resources (Missing Operations)

These resources have incomplete CRUD implementation. **Priority implementation list:**

### HIGH PRIORITY (Missing Update/Delete)

#### 1. Message Feedback
**Missing:** UPDATE (PUT)

```bash
# Current: GET, POST, DELETE
GET /api/v1/feedback/messages/{messageId}
POST /api/v1/feedback/messages/{messageId}
DELETE /api/v1/feedback/messages/{messageId}

# TODO: Add UPDATE
PUT /api/v1/feedback/messages/{messageId}
{
  "rating": 9,
  "textFeedback": "Updated feedback",
  "tags": [...]
}
```

---

#### 2. User Settings
**Missing:** UPDATE (PUT), DELETE

```bash
# Current: GET, POST (non-standard)
GET /api/v1/settings
POST /api/v1/settings

# TODO: Add standard UPDATE
PUT /api/v1/settings
{
  "theme": "dark",
  "notifications": "on",
  "language": "en-us"
}

# TODO: Add DELETE (Reset to defaults)
DELETE /api/v1/settings
```

---

#### 3. Workspace Files
**Missing:** UPDATE (PUT)

```bash
# Current: GET, POST, DELETE
GET /api/v1/files
GET /api/v1/files/{id}
POST /api/v1/files
DELETE /api/v1/files/{id}

# TODO: Add UPDATE
PUT /api/v1/files/{id}
{
  "name": "Updated filename",
  "description": "Updated description"
}
```

---

#### 4. Error Logs
**Missing:** DELETE

```bash
# Current: GET, POST, PUT (for status)
GET /api/v1/system-feedback/error-logs
GET /api/v1/system-feedback/error-logs/{id}
POST /api/v1/system-feedback/error-logs
PUT /api/v1/system-feedback/error-logs/{id}/status

# TODO: Add DELETE
DELETE /api/v1/system-feedback/error-logs/{id}
```

---

#### 5. Plugin Settings
**Missing:** UPDATE (PUT), DELETE

```bash
# Current: GET, POST
GET /api/v1/plugin/settings
POST /api/v1/plugin/settings

# TODO: Add UPDATE
PUT /api/v1/plugin/settings/{id}
{
  "enabled": true,
  "parameters": {...}
}

# TODO: Add DELETE
DELETE /api/v1/plugin/settings/{id}
```

---

### MEDIUM PRIORITY (Additional Features)

#### 6. Note Collections (Metadata)
**Missing:** CREATE, UPDATE, DELETE

```bash
# Current: GET (Read-only)
GET /api/v1/notesapi/collections

# TODO: Add full CRUD
POST /api/v1/notesapi/collections
{
  "name": "My Collection",
  "description": "Collection of related notes"
}

PUT /api/v1/notesapi/collections/{id}
{
  "name": "Updated name"
}

DELETE /api/v1/notesapi/collections/{id}
```

---

#### 7. Note Tags (Metadata)
**Missing:** CREATE, UPDATE, DELETE

```bash
# Current: GET (Read-only)
GET /api/v1/notesapi/tags

# TODO: Add full CRUD
POST /api/v1/notesapi/tags
{
  "name": "Important",
  "color": "#FF0000"
}

PUT /api/v1/notesapi/tags/{id}
{
  "name": "Critical",
  "color": "#FF5555"
}

DELETE /api/v1/notesapi/tags/{id}
```

---

## Implementation Guide

### Step-by-Step for Missing CRUD Operations

#### For Developers implementing missing endpoints:

### Template: Adding PUT (Update) Endpoint

```csharp
/// <summary>
/// Update an existing {resource}
/// </summary>
/// <param name="id">{Resource} ID to update</param>
/// <param name="request">{Resource} update request</param>
/// <returns>Updated {resource}</returns>
[HttpPut("{id}")]
[ProducesResponseType(typeof({ResourceDto}), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Update{Resource}(Guid id, [FromBody] Update{Resource}Request request)
{
    try
    {
        _logger.LogInformation("Updating {resource} {Id}", nameof({Resource}), id);

        var entity = await _service.Get{Resource}ByIdAsync(id);
        if (entity == null)
            return NotFound(new { error = "{Resource} not found" });

        // Update fields from request
        if (!string.IsNullOrEmpty(request.Field1))
            entity.Field1 = request.Field1;

        if (request.Field2.HasValue)
            entity.Field2 = request.Field2.Value;

        await _service.Update{Resource}Async(entity);

        var dto = MapToDto(entity);
        return Ok(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating {resource}", nameof({Resource}));
        return StatusCode(500, new { error = "Failed to update {resource}", message = ex.Message });
    }
}
```

### Template: Adding DELETE Endpoint

```csharp
/// <summary>
/// Delete a {resource}
/// </summary>
/// <param name="id">{Resource} ID to delete</param>
/// <returns>Success message</returns>
[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Delete{Resource}(Guid id)
{
    try
    {
        _logger.LogInformation("Deleting {resource} {Id}", nameof({Resource}), id);

        var entity = await _service.Get{Resource}ByIdAsync(id);
        if (entity == null)
            return NotFound(new { error = "{Resource} not found" });

        await _service.Delete{Resource}Async(id);

        return Ok(new { success = true, message = "{Resource} deleted successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting {resource}", nameof({Resource}));
        return StatusCode(500, new { error = "Failed to delete {resource}", message = ex.Message });
    }
}
```

---

## Standard CRUD Patterns

### Route Patterns

**Collection Endpoints:**
```
GET /api/v1/{resource}              → List all
POST /api/v1/{resource}             → Create
```

**Single Item Endpoints:**
```
GET /api/v1/{resource}/{id}         → Get by ID
PUT /api/v1/{resource}/{id}         → Update
DELETE /api/v1/{resource}/{id}      → Delete
```

**Nested Resources:**
```
GET /api/v1/{parent}/{parentId}/{child}
POST /api/v1/{parent}/{parentId}/{child}
GET /api/v1/{parent}/{parentId}/{child}/{childId}
PUT /api/v1/{parent}/{parentId}/{child}/{childId}
DELETE /api/v1/{parent}/{parentId}/{child}/{childId}
```

### Status Codes

| Operation | Success | Not Found | Conflict | Error |
|-----------|---------|-----------|----------|-------|
| **CREATE (POST)** | 201 Created | - | 409 Conflict | 500 |
| **READ (GET)** | 200 OK | 404 | - | 500 |
| **UPDATE (PUT)** | 200 OK | 404 | 409 Conflict | 500 |
| **DELETE** | 200 OK | 404 | - | 500 |

### Request/Response Format

**Success Response (200):**
```json
{
  "id": "uuid",
  "name": "Resource name",
  "createdAt": "2025-01-18T10:00:00Z",
  "updatedAt": "2025-01-18T10:00:00Z"
}
```

**Error Response (4xx/5xx):**
```json
{
  "error": {
    "message": "Human-readable error message",
    "type": "error_type",
    "code": "ERROR_CODE"
  }
}
```

---

## Summary

### Current Status

| Category | Count | Status |
|----------|-------|--------|
| **Complete CRUD** | 12 | ✅ Fully Implemented |
| **Partial CRUD** | 7 | ⚠️ Missing operations |
| **Read-Only** | 8 | 📖 Intentional (analytics, queries) |
| **Action-Based** | 25+ | ⚡ Special operations |
| **Total Endpoints** | 150+ | 📊 Comprehensive API |

### Priority Implementation Order

1. **Phase 1 (Critical):** Message Feedback (PUT), User Settings (PUT, DELETE)
2. **Phase 2 (Important):** Workspace Files (PUT), Error Logs (DELETE), Plugin Settings (PUT, DELETE)
3. **Phase 3 (Enhancement):** Note Collections/Tags (POST, PUT, DELETE)

---

**Built with ❤️ from New Zealand** 🇳🇿

*Last Updated: 2025-01-18*
*API Version: 2.0*
