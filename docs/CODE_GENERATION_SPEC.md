# Code Generation & IDE Features - Implementation Plan

## Overview
Integrate Monaco Editor and Roslyn to enable code generation, editing, compilation, and hot-reload directly from chat.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Chat Interface                        │
│  "Add a new component for file uploads"                 │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              AI Code Generator                           │
│  • Understands project structure                        │
│  • Generates C#/Razor/JSON code                         │
│  • Follows conventions                                  │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              Monaco Editor                               │
│  • Syntax highlighting                                  │
│  • Multi-file tabs                                      │
│  • Diff viewer                                          │
│  • Basic IntelliSense                                   │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              Roslyn Services                             │
│  • Syntax validation                                    │
│  • Compilation                                          │
│  • Code completion                                      │
│  • Semantic analysis                                    │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              Virtual File System                         │
│  • IndexedDB storage                                    │
│  • Project structure                                    │
│  • Version control                                      │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│              Hot Reload Engine                           │
│  • Dynamic component loading                            │
│  • State preservation                                   │
│  • Instant feedback                                     │
└─────────────────────────────────────────────────────────┘
```

---

## Phase 1: Monaco Editor Integration (2-4 hours)

### 1.1 Add Monaco to Project
```xml
<!-- BlazorMonaco NuGet package -->
<PackageReference Include="BlazorMonaco" Version="3.2.0" />
```

### 1.2 Create Code Editor Component
**File:** `src/AiMate.Client/Components/Code/CodeEditor.razor`

Features:
- Multi-file tabs
- Language detection
- Theme switching (matches app theme)
- Full-screen mode
- Copy/paste
- Search/replace
- Minimap

### 1.3 Create File Tree Component
**File:** `src/AiMate.Client/Components/Code/FileTree.razor`

Features:
- Folder structure
- File creation/deletion
- Rename support
- Context menu
- Drag & drop (future)

---

## Phase 2: Roslyn Integration (4-6 hours)

### 2.1 Add Roslyn Packages
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.8.0" />
```

### 2.2 Create Compilation Service
**File:** `src/AiMate.Shared/Services/RoslynService.cs`

Capabilities:
- Compile C# code in-browser
- Syntax validation
- Semantic analysis
- Code completion suggestions
- Diagnostic messages (errors/warnings)

### 2.3 IntelliSense Provider
**File:** `src/AiMate.Shared/Services/IntelliSenseService.cs`

Features:
- Real-time completions
- Method signatures
- Documentation tooltips
- Parameter info

---

## Phase 3: Virtual File System (3-4 hours)

### 3.1 Create File System Service
**File:** `src/AiMate.Shared/Services/VirtualFileSystem.cs`

Storage:
- IndexedDB for persistence
- In-memory cache for speed
- Project structure metadata
- File content with versioning

### 3.2 Project Manager
**File:** `src/AiMate.Shared/Services/ProjectManager.cs`

Features:
- Load/save project state
- File CRUD operations
- Dependency tracking
- Build configuration

---

## Phase 4: AI Code Generation (4-6 hours)

### 4.1 Code Generation Service
**File:** `src/AiMate.Shared/Services/CodeGenerationService.cs`

Responsibilities:
- Parse user intent from chat
- Generate appropriate code
- Follow project conventions
- Understand context (existing files, patterns)
- Create multiple files if needed

### 4.2 Code Understanding
- Parse existing project structure
- Understand component relationships
- Infer naming conventions
- Maintain consistency

### 4.3 Structured Prompts
Template system for code generation:
```csharp
public class CodeGenPrompt
{
    public string Intent { get; set; }
    public ProjectContext Context { get; set; }
    public CodeStyle Style { get; set; }
    public string[] RequiredFiles { get; set; }
}
```

---

## Phase 5: Hot Reload (6-8 hours)

### 5.1 Dynamic Component Loader
**File:** `src/AiMate.Client/Services/DynamicComponentLoader.cs`

Capabilities:
- Compile Razor components at runtime
- Load into running app
- Preserve state where possible
- Fallback to full reload if needed

### 5.2 Assembly Management
- Track loaded assemblies
- Handle dependencies
- Unload old versions
- Memory management

---

## Phase 6: UI Integration (4-6 hours)

### 6.1 Code Modal
**File:** `src/AiMate.Client/Components/Modals/CodeModal.razor`

Layout:
```
┌────────────────────────────────────────────────┐
│ Code Editor                                    │
│ ┌──────────────┬─────────────────────────────┐│
│ │ File Tree    │ Monaco Editor               ││
│ │              │                             ││
│ │ src/         │ @page "/upload"             ││
│ │  Components/ │ <MudFileUpload...>          ││
│ │   Upload/    │                             ││
│ │    Upload.   │                             ││
│ │     razor    │                             ││
│ │              │                             ││
│ │              │                             ││
│ └──────────────┴─────────────────────────────┘│
│                                                │
│ [Compile] [Test] [Apply] [Cancel]             │
└────────────────────────────────────────────────┘
```

### 6.2 Code Chat Commands
Special commands in chat:
- `/code` - Open code editor
- `/edit <file>` - Edit specific file
- `/create <component>` - Generate new component
- `/compile` - Compile current code
- `/apply` - Apply changes and hot reload

### 6.3 Inline Code Blocks
When AI generates code in chat:
- Syntax highlighted code block
- "Edit in Monaco" button
- "Compile" button
- "Apply" button
- Show compilation errors inline

---

## Phase 7: Dogfooding Workflow (The Fun Part!)

### Example Session:
```
You: "I want to add a drag-drop file upload component"

AI: I'll create a new component for that. Here's what I'm going to build:

1. FileUploadZone.razor - The main component
2. FileUploadZone.razor.cs - Code-behind
3. FileUploadService.cs - Upload handling

Let me generate these files...

[Shows Monaco editor with 3 tabs]

AI: I've created the files. The component:
- Accepts drag & drop
- Shows file preview
- Has a progress bar
- Validates file types
- Integrates with your existing AttachmentService

Would you like me to compile it?

You: "Yes, compile and show me"

AI: [Compiling with Roslyn...]
✅ Compilation successful - No errors, 0 warnings

Ready to apply?

You: "Apply it"

AI: [Hot reloading...]
✅ Component loaded successfully!

You can now use <FileUploadZone /> in your chat input.

Want to test it?

You: "Yes, open the chat page and add it to the input area"

AI: [Opens Chat.razor in editor]
[Shows diff of changes]
[Applies changes]
[Hot reloads]

✅ Done! Your chat input now has drag & drop support.
Try dragging a file!

You: [drags file]
You: "Perfect! Now let's add image preview..."

[Continues building...]
```

---

## Technical Challenges & Solutions

### Challenge 1: Roslyn in WebAssembly
**Problem:** Roslyn is heavy, WASM has size limits
**Solution:** 
- Use trimmed/minimal Roslyn assemblies
- Lazy load compiler services
- Cache compiled assemblies
- Progressive loading

### Challenge 2: Hot Reload Limitations
**Problem:** Not all components can hot reload
**Solution:**
- Detect reloadable vs non-reloadable changes
- Warn user before full reload
- Preserve state in localStorage before reload
- Restore state after reload

### Challenge 3: File System Persistence
**Problem:** IndexedDB is async, can be slow
**Solution:**
- In-memory cache with write-behind
- Batch file operations
- Compression for large files
- Background sync

### Challenge 4: IntelliSense Performance
**Problem:** Real-time analysis is CPU intensive
**Solution:**
- Debounce analysis (300ms)
- Web Worker for compilation
- Cache symbol tables
- Incremental compilation

---

## Data Models

### Virtual File
```csharp
public class VirtualFile
{
    public string Path { get; set; }
    public string Content { get; set; }
    public string Language { get; set; }
    public DateTime LastModified { get; set; }
    public FileVersion[] Versions { get; set; }
    public bool IsDirty { get; set; }
}
```

### Compilation Result
```csharp
public class CompilationResult
{
    public bool Success { get; set; }
    public Diagnostic[] Errors { get; set; }
    public Diagnostic[] Warnings { get; set; }
    public byte[]? AssemblyBytes { get; set; }
    public TimeSpan CompilationTime { get; set; }
}
```

### Code Generation Request
```csharp
public class CodeGenRequest
{
    public string UserIntent { get; set; }
    public ProjectSnapshot CurrentProject { get; set; }
    public CodeStyle PreferredStyle { get; set; }
    public string[] AffectedFiles { get; set; }
}
```

---

## Implementation Order

### Week 1: Foundation
1. ✅ Add Monaco NuGet package
2. ✅ Create CodeEditor component
3. ✅ Create FileTree component
4. ✅ Integrate into layout (new modal)
5. ✅ Basic file CRUD

### Week 2: Compilation
1. ✅ Add Roslyn packages
2. ✅ Create RoslynService
3. ✅ Implement syntax validation
4. ✅ Show errors in Monaco
5. ✅ Basic compilation

### Week 3: Virtual FS
1. ✅ VirtualFileSystem service
2. ✅ IndexedDB persistence
3. ✅ Project structure management
4. ✅ File versioning

### Week 4: AI Integration
1. ✅ Code generation prompts
2. ✅ Project context extraction
3. ✅ Multi-file generation
4. ✅ Chat commands (/code, /edit)

### Week 5: Hot Reload
1. ✅ Dynamic component loading
2. ✅ Assembly management
3. ✅ State preservation
4. ✅ Error recovery

### Week 6: Polish
1. ✅ IntelliSense improvements
2. ✅ Diff viewer
3. ✅ Git integration (basic)
4. ✅ Performance optimization

---

## Success Metrics

- ✅ Generate a working component from chat in < 30 seconds
- ✅ Compile and validate in < 2 seconds
- ✅ Hot reload in < 1 second
- ✅ Support 100+ file projects
- ✅ Zero data loss (auto-save every 5 seconds)
- ✅ IntelliSense latency < 300ms

---

## Future Enhancements

1. **Full Git Integration**
   - Commit from chat
   - Branch management
   - Conflict resolution

2. **Collaborative Editing**
   - Real-time multi-user editing
   - Cursor sharing
   - Chat sync

3. **Package Management**
   - NuGet integration
   - Dependency resolution
   - Version management

4. **Testing Framework**
   - Unit test generation
   - Test runner in browser
   - Coverage reporting

5. **Deployment**
   - One-click deploy
   - GitHub Actions integration
   - Preview deployments

---

## Let's Build It!

This feature will transform aiMate from a chat app into a **full IDE powered by AI**. 

We can literally build aiMate inside aiMate, using AI to write code, compile it, and see it running instantly.

**Ready to start with Monaco integration?** 🚀
