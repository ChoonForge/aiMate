# aiMate v2 - Killer Features Roadmap 🚀

**Status:** Prioritized feature list for world-class AI platform
**Focus:** AI Mates (Persistent Agents) + CodeMate (Roslyn C# First)

---

## 🎯 Priority 1: AI Mates - Persistent AI Agents

**Vision:** Every user gets multiple specialized AI companions that remember everything, learn over time, and proactively help.

### Phase 1.1: Core Architecture (1-2 weeks)

#### Database Schema
```sql
-- New entity: AiMate
CREATE TABLE "AiMates" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Name" text NOT NULL,                  -- "Code Mate", "Research Mate", etc.
    "Avatar" text NULL,                    -- Avatar image URL
    "Personality" text NOT NULL,            -- Base personality type
    "SystemPrompt" text NOT NULL,          -- Custom system instructions
    "SpecialtyArea" text NULL,             -- e.g., "C# Development", "Research"
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastActiveAt" timestamp with time zone NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "ConversationCount" integer NOT NULL DEFAULT 0,
    "TotalInteractions" integer NOT NULL DEFAULT 0,
    "UserPreferences" jsonb NULL,          -- Custom settings
    CONSTRAINT "FK_AiMates_Users_UserId" FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Link conversations to specific AI Mates
ALTER TABLE "Conversations" ADD COLUMN "AiMateId" uuid NULL;
ALTER TABLE "Conversations" ADD CONSTRAINT "FK_Conversations_AiMates_AiMateId"
    FOREIGN KEY ("AiMateId") REFERENCES "AiMates" ("Id") ON DELETE SET NULL;

-- AI Mate Memory - links to Knowledge Graph
CREATE TABLE "AiMateMemories" (
    "Id" uuid PRIMARY KEY,
    "AiMateId" uuid NOT NULL,
    "KnowledgeItemId" uuid NOT NULL,
    "Importance" integer NOT NULL DEFAULT 5,    -- 1-10 scale
    "LastAccessedAt" timestamp with time zone NULL,
    "AccessCount" integer NOT NULL DEFAULT 0,
    "MemoryType" text NOT NULL,                 -- "fact", "preference", "skill", "experience"
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_AiMateMemories_AiMates_AiMateId"
        FOREIGN KEY ("AiMateId") REFERENCES "AiMates" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AiMateMemories_KnowledgeItems_KnowledgeItemId"
        FOREIGN KEY ("KnowledgeItemId") REFERENCES "KnowledgeItems" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AiMateMemories_AiMateId_Importance" ON "AiMateMemories" ("AiMateId", "Importance" DESC);
```

#### Entity Classes
- **`AiMate.cs`** (Core.Entities)
  - Id, Name, Personality, SystemPrompt, SpecialtyArea
  - Navigation: User, Conversations, Memories
  - Methods: `GetMemoryContext()`, `UpdateLastActive()`, `IncrementInteractions()`

- **`AiMateMemory.cs`** (Core.Entities)
  - Links AiMate → KnowledgeItem with metadata
  - Importance scoring (1-10)
  - Memory types: fact, preference, skill, experience

#### Services
- **`IAiMateService`** (Core.Interfaces)
  ```csharp
  Task<AiMate> CreateAiMateAsync(Guid userId, string name, string personality);
  Task<List<AiMate>> GetUserAiMatesAsync(Guid userId);
  Task<AiMate?> GetActiveAiMateAsync(Guid userId);
  Task<string> GetMemoryContextAsync(Guid aiMateId, int maxTokens = 2000);
  Task AddMemoryAsync(Guid aiMateId, string content, string memoryType, int importance);
  Task<List<string>> GetProactiveSuggestionsAsync(Guid aiMateId);
  ```

- **`AiMateService.cs`** (Infrastructure.Services)
  - CRUD operations for AI Mates
  - Memory retrieval with embeddings (use existing pgvector)
  - Proactive suggestion generation
  - Context building from Knowledge Graph

### Phase 1.2: Memory System (1 week)

#### Auto-Capture from Conversations
```csharp
// After each conversation turn, extract important info
public class MemoryCaptureEffect : Effect<SendMessageSuccessAction>
{
    public override async Task HandleAsync(SendMessageSuccessAction action, IDispatcher dispatcher)
    {
        // Use LiteLLM to analyze conversation and extract:
        // - User preferences ("I prefer TypeScript over JavaScript")
        // - Facts about user ("Working on React project")
        // - Skills learned ("Now understands async/await")
        // - Important decisions ("Decided to use PostgreSQL")

        var prompt = $@"Analyze this conversation and extract memorable information:
User: {action.UserMessage.Content}
Assistant: {action.AssistantMessage.Content}

Extract:
1. User preferences (what they like/dislike)
2. Facts about the user (their work, interests, projects)
3. Skills or knowledge gained
4. Important decisions made

Return as JSON: {{""memories"": [{{""content"": ""..."", ""type"": ""preference|fact|skill|decision"", ""importance"": 1-10}}]}}";

        var memories = await _liteLLMService.ExtractMemoriesAsync(prompt);

        foreach (var memory in memories)
        {
            await _aiMateService.AddMemoryAsync(
                action.AiMateId,
                memory.Content,
                memory.Type,
                memory.Importance
            );
        }
    }
}
```

#### Memory Retrieval
```csharp
public async Task<string> GetMemoryContextAsync(Guid aiMateId, int maxTokens = 2000)
{
    // 1. Get recent memories (last 30 days)
    var recentMemories = await _dbContext.AiMateMemories
        .Where(m => m.AiMateId == aiMateId &&
                    m.CreatedAt > DateTime.UtcNow.AddDays(-30))
        .OrderByDescending(m => m.Importance)
        .ThenByDescending(m => m.LastAccessedAt)
        .Take(20)
        .Include(m => m.KnowledgeItem)
        .ToListAsync();

    // 2. Get high-importance memories (anytime)
    var importantMemories = await _dbContext.AiMateMemories
        .Where(m => m.AiMateId == aiMateId && m.Importance >= 8)
        .OrderByDescending(m => m.Importance)
        .Take(10)
        .Include(m => m.KnowledgeItem)
        .ToListAsync();

    // 3. Build context string
    var context = new StringBuilder();
    context.AppendLine("=== AI MATE LONG-TERM MEMORY ===\n");
    context.AppendLine("Important facts about the user:\n");

    foreach (var memory in importantMemories.DistinctBy(m => m.Id))
    {
        context.AppendLine($"- [{memory.MemoryType}] {memory.KnowledgeItem.Content}");
    }

    context.AppendLine("\nRecent context:\n");
    foreach (var memory in recentMemories.Where(m => !importantMemories.Contains(m)))
    {
        context.AppendLine($"- {memory.KnowledgeItem.Content}");
    }

    // 4. Truncate to maxTokens (rough estimate: 1 token ≈ 4 chars)
    var maxChars = maxTokens * 4;
    return context.Length > maxChars
        ? context.ToString().Substring(0, maxChars) + "..."
        : context.ToString();
}
```

### Phase 1.3: UI Components (1 week)

#### AI Mates Management Page
**Location:** `Components/Pages/AiMates.razor`

```razor
@page "/ai-mates"
@using Fluxor
@using AiMate.Web.Store.AiMates
@inject IState<AiMatesState> AiMatesState
@inject IDispatcher Dispatcher

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Your AI Mates</MudText>

    <MudGrid>
        @foreach (var mate in AiMatesState.Value.AiMates.Values)
        {
            <MudItem xs="12" md="6" lg="4">
                <MudCard>
                    <MudCardHeader>
                        <CardHeaderContent>
                            <MudAvatar Size="Size.Large" Color="Color.Primary">
                                @mate.Name.Substring(0, 2).ToUpper()
                            </MudAvatar>
                            <MudText Typo="Typo.h6">@mate.Name</MudText>
                            <MudText Typo="Typo.caption">@mate.SpecialtyArea</MudText>
                        </CardHeaderContent>
                        <CardHeaderActions>
                            @if (mate.IsActive)
                            {
                                <MudChip Size="Size.Small" Color="Color.Success">Active</MudChip>
                            }
                        </CardHeaderActions>
                    </MudCardHeader>

                    <MudCardContent>
                        <MudText Typo="Typo.body2" Class="mb-2">
                            <strong>@mate.TotalInteractions</strong> interactions
                        </MudText>
                        <MudText Typo="Typo.body2" Class="mb-2">
                            <strong>@mate.ConversationCount</strong> conversations
                        </MudText>
                        <MudText Typo="Typo.caption" Color="Color.Secondary">
                            Last active: @GetRelativeTime(mate.LastActiveAt)
                        </MudText>
                    </MudCardContent>

                    <MudCardActions>
                        <MudButton Size="Size.Small"
                                  OnClick="@(() => StartConversationWith(mate.Id))">
                            Chat
                        </MudButton>
                        <MudButton Size="Size.Small"
                                  OnClick="@(() => ViewMemories(mate.Id))">
                            Memories
                        </MudButton>
                        <MudButton Size="Size.Small"
                                  OnClick="@(() => ConfigureMate(mate.Id))">
                            Configure
                        </MudButton>
                    </MudCardActions>
                </MudCard>
            </MudItem>
        }

        @* Create new AI Mate card *@
        <MudItem xs="12" md="6" lg="4">
            <MudCard Style="border: 2px dashed var(--mud-palette-primary);">
                <MudCardContent Class="d-flex flex-column align-center justify-center" Style="min-height: 300px;">
                    <MudIcon Icon="@Icons.Material.Filled.Add" Size="Size.Large" Color="Color.Primary" />
                    <MudText Typo="Typo.h6" Class="mt-2">Create New AI Mate</MudText>
                    <MudButton Variant="Variant.Filled"
                              Color="Color.Primary"
                              OnClick="@OpenCreateDialog"
                              Class="mt-3">
                        Get Started
                    </MudButton>
                </MudCardContent>
            </MudCard>
        </MudItem>
    </MudGrid>
</MudContainer>
```

#### AI Mate Selector in Chat
**Update:** `Components/Layout/MainLayout.razor`

Add dropdown/tabs to switch between AI Mates:
```razor
<MudAppBar>
    <MudSelect T="Guid?"
              Value="@ChatState.Value.ActiveAiMateId"
              ValueChanged="@((Guid? id) => Dispatcher.Dispatch(new SwitchAiMateAction(id)))">
        @foreach (var mate in AiMatesState.Value.AiMates.Values)
        {
            <MudSelectItem Value="@mate.Id">@mate.Name</MudSelectItem>
        }
    </MudSelect>
</MudAppBar>
```

### Phase 1.4: Proactive Suggestions (1 week)

#### Background Service
**Location:** `Infrastructure/Services/ProactiveSuggestionsService.cs`

```csharp
public class ProactiveSuggestionsService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Every 30 minutes, check all active AI Mates
            var activeMates = await _dbContext.AiMates
                .Where(m => m.IsActive && m.LastActiveAt > DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            foreach (var mate in activeMates)
            {
                var suggestions = await GenerateSuggestionsAsync(mate);

                if (suggestions.Any())
                {
                    // Store suggestions in cache/database
                    await _cacheService.SetAsync(
                        $"suggestions:{mate.Id}",
                        suggestions,
                        TimeSpan.FromHours(1)
                    );

                    // Optional: Send notification to user
                    await _notificationService.SendAsync(
                        mate.UserId,
                        $"{mate.Name} has {suggestions.Count} suggestions for you"
                    );
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task<List<string>> GenerateSuggestionsAsync(AiMate mate)
    {
        // Get recent memories and context
        var context = await _aiMateService.GetMemoryContextAsync(mate.Id);

        // Ask AI to generate suggestions
        var prompt = $@"Based on this user's context, generate 3 proactive suggestions:

{context}

Generate helpful suggestions like:
- ""You mentioned learning React. Want me to explain hooks?""
- ""You're working on an API. Should we discuss error handling?""
- ""You saved this article 3 days ago. Want to discuss it?""

Return as JSON: {{""suggestions"": [""..."", ""..."", ""...""]}}";

        var response = await _liteLLMService.GetCompletionAsync(prompt);
        var suggestions = JsonSerializer.Deserialize<SuggestionsResponse>(response);

        return suggestions?.Suggestions ?? new List<string>();
    }
}
```

#### Display in UI
Add to chat interface:
```razor
@if (ProactiveSuggestions.Any())
{
    <MudPaper Class="pa-3 mb-3" Elevation="2">
        <MudText Typo="Typo.subtitle2" Color="Color.Primary">
            💡 @AiMateName has some ideas:
        </MudText>
        @foreach (var suggestion in ProactiveSuggestions)
        {
            <MudChip Size="Size.Small"
                    OnClick="@(() => SendMessage(suggestion))"
                    Class="mt-1">
                @suggestion
            </MudChip>
        }
    </MudPaper>
}
```

---

## 🎯 Priority 2: CodeMate - Roslyn C# Development Environment

**Vision:** Build and test C# code in real-time with AI. Perfect for creating aiMate plugins collaboratively.

### Phase 2.1: Roslyn Scripting Integration (1 week)

#### Service Interface
**Location:** `Core/Interfaces/ICodeExecutionService.cs`

```csharp
public interface ICodeExecutionService
{
    Task<CodeExecutionResult> ExecuteCSharpAsync(string code, TimeSpan timeout);
    Task<CodeExecutionResult> ExecuteScriptAsync(string code, Dictionary<string, object> globals);
    Task<List<CompilationDiagnostic>> CompileAsync(string code);
    Task<string> FormatCodeAsync(string code);
    Task<List<CodeCompletion>> GetCompletionsAsync(string code, int position);
}

public class CodeExecutionResult
{
    public bool Success { get; set; }
    public object? ReturnValue { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Errors { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public List<CompilationDiagnostic> Diagnostics { get; set; } = new();
}

public class CompilationDiagnostic
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DiagnosticSeverity Severity { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}
```

#### Roslyn Service Implementation
**Location:** `Infrastructure/Services/RoslynCodeExecutionService.cs`

```csharp
public class RoslynCodeExecutionService : ICodeExecutionService
{
    private readonly ScriptOptions _scriptOptions;
    private readonly ILogger<RoslynCodeExecutionService> _logger;

    public RoslynCodeExecutionService(ILogger<RoslynCodeExecutionService> logger)
    {
        _logger = logger;

        // Configure Roslyn with safe defaults
        _scriptOptions = ScriptOptions.Default
            .AddReferences(
                typeof(object).Assembly,                    // System
                typeof(Console).Assembly,                   // System.Console
                typeof(Enumerable).Assembly,                // System.Linq
                typeof(HttpClient).Assembly,                // System.Net.Http
                typeof(JsonSerializer).Assembly             // System.Text.Json
            )
            .AddImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text",
                "System.Text.Json",
                "System.Net.Http"
            );
    }

    public async Task<CodeExecutionResult> ExecuteCSharpAsync(string code, TimeSpan timeout)
    {
        var result = new CodeExecutionResult();
        var sw = Stopwatch.StartNew();

        try
        {
            // Capture console output
            var originalOut = Console.Out;
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            // Create cancellation token for timeout
            using var cts = new CancellationTokenSource(timeout);

            // Compile and execute
            var script = CSharpScript.Create(code, _scriptOptions);
            var compilation = script.GetCompilation();

            // Check for compilation errors
            var diagnostics = compilation.GetDiagnostics()
                .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                .ToList();

            if (diagnostics.Any())
            {
                result.Success = false;
                result.Errors = string.Join("\n", diagnostics.Select(d =>
                    $"Line {d.Location.GetLineSpan().StartLinePosition.Line + 1}: {d.GetMessage()}"));
                result.Diagnostics = diagnostics.Select(d => new CompilationDiagnostic
                {
                    Id = d.Id,
                    Message = d.GetMessage(),
                    Severity = (DiagnosticSeverity)(int)d.Severity,
                    Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                    Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
                }).ToList();

                return result;
            }

            // Execute with timeout
            var scriptState = await script.RunAsync(cancellationToken: cts.Token);

            result.Success = true;
            result.ReturnValue = scriptState.ReturnValue;
            result.Output = outputWriter.ToString();

            // Restore console
            Console.SetOut(originalOut);
        }
        catch (CompilationErrorException ex)
        {
            result.Success = false;
            result.Errors = string.Join("\n", ex.Diagnostics.Select(d => d.GetMessage()));
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.Errors = $"Execution timeout ({timeout.TotalSeconds}s)";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors = ex.Message;
            _logger.LogError(ex, "Code execution failed");
        }
        finally
        {
            sw.Stop();
            result.ExecutionTime = sw.Elapsed;
        }

        return result;
    }

    public async Task<string> FormatCodeAsync(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        var formatted = root.NormalizeWhitespace();
        return formatted.ToFullString();
    }

    public async Task<List<CodeCompletion>> GetCompletionsAsync(string code, int position)
    {
        // Use Roslyn's completion API
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("temp")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);

        // Get symbols at position
        var node = root.FindToken(position).Parent;
        var symbols = semanticModel.LookupSymbols(position);

        return symbols.Select(s => new CodeCompletion
        {
            Label = s.Name,
            Kind = s.Kind.ToString(),
            Detail = s.ToDisplayString()
        }).ToList();
    }
}
```

### Phase 2.2: CodeMate Chat Integration (1 week)

#### MCP Tool for Code Execution
**Update:** `Infrastructure/Services/MCPToolService.cs`

```csharp
private async Task<MCPToolResult> ExecuteCSharpCodeAsync(Dictionary<string, object> parameters)
{
    var code = parameters.GetValueOrDefault("code")?.ToString() ?? string.Empty;
    var timeoutSeconds = int.Parse(parameters.GetValueOrDefault("timeout")?.ToString() ?? "30");

    _logger.LogInformation("Executing C# code (timeout: {Timeout}s)", timeoutSeconds);

    var result = await _codeExecutionService.ExecuteCSharpAsync(
        code,
        TimeSpan.FromSeconds(timeoutSeconds)
    );

    var output = new
    {
        success = result.Success,
        output = result.Output,
        return_value = result.ReturnValue?.ToString(),
        errors = result.Errors,
        execution_time_ms = result.ExecutionTime.TotalMilliseconds,
        diagnostics = result.Diagnostics
    };

    return new MCPToolResult
    {
        Success = result.Success,
        Data = output,
        Message = result.Success
            ? "Code executed successfully"
            : $"Compilation/execution failed: {result.Errors}",
        Metadata = new Dictionary<string, object>
        {
            ["execution_time"] = result.ExecutionTime.TotalMilliseconds,
            ["has_output"] = !string.IsNullOrEmpty(result.Output)
        }
    };
}
```

#### Register Tool
```csharp
private readonly MCPToolDefinition _csharpExecuteTool = new()
{
    Name = "execute_csharp",
    Description = "Execute C# code using Roslyn scripting. Perfect for testing code snippets, algorithms, and building aiMate plugins.",
    Parameters = new Dictionary<string, MCPToolParameter>
    {
        ["code"] = new()
        {
            Type = "string",
            Description = "The C# code to execute. Can include Console.WriteLine, return values, etc.",
            Required = true
        },
        ["timeout"] = new()
        {
            Type = "integer",
            Description = "Execution timeout in seconds (default: 30, max: 120)",
            Required = false
        }
    },
    Examples = new[]
    {
        @"Execute: Console.WriteLine(""Hello, World!"");",
        @"Calculate: return Enumerable.Range(1, 100).Sum();",
        @"Test plugin:
var plugin = new MyAiMatePlugin();
var result = await plugin.ExecuteAsync(""test"");
return result;"
    }
};
```

### Phase 2.3: Live Code UI (1 week)

#### Code Editor Component
**Location:** `Components/Shared/CodeEditor.razor`

Use Monaco Editor (VS Code editor) via JS interop:

```razor
@inject IJSRuntime JS

<div id="monaco-editor-@_editorId" style="height: @Height; width: 100%;"></div>

@code {
    private string _editorId = Guid.NewGuid().ToString("N");

    [Parameter]
    public string Code { get; set; } = string.Empty;

    [Parameter]
    public string Language { get; set; } = "csharp";

    [Parameter]
    public string Height { get; set; } = "400px";

    [Parameter]
    public EventCallback<string> OnCodeChanged { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("monacoEditor.create", _editorId, Code, Language);
        }
    }

    public async Task<string> GetCodeAsync()
    {
        return await JS.InvokeAsync<string>("monacoEditor.getValue", _editorId);
    }

    public async Task SetCodeAsync(string code)
    {
        await JS.InvokeVoidAsync("monacoEditor.setValue", _editorId, code);
    }

    public async Task FormatAsync()
    {
        await JS.InvokeVoidAsync("monacoEditor.format", _editorId);
    }
}
```

#### CodeMate Page
**Location:** `Components/Pages/CodeMate.razor`

```razor
@page "/codemate"
@inject ICodeExecutionService CodeExecution
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">
        CodeMate - Live C# Development
    </MudText>

    <MudGrid>
        <MudItem xs="12" md="6">
            <MudPaper Class="pa-3" Elevation="2">
                <MudText Typo="Typo.h6" Class="mb-3">Code Editor</MudText>

                <CodeEditor @ref="_editor"
                           Code="@_code"
                           Language="csharp"
                           Height="500px" />

                <div class="d-flex gap-2 mt-3">
                    <MudButton Variant="Variant.Filled"
                              Color="Color.Primary"
                              StartIcon="@Icons.Material.Filled.PlayArrow"
                              OnClick="@RunCode"
                              Disabled="@_isExecuting">
                        @(_isExecuting ? "Running..." : "Run Code")
                    </MudButton>

                    <MudButton Variant="Variant.Outlined"
                              StartIcon="@Icons.Material.Filled.FormatAlignLeft"
                              OnClick="@FormatCode">
                        Format
                    </MudButton>

                    <MudButton Variant="Variant.Outlined"
                              StartIcon="@Icons.Material.Filled.Clear"
                              OnClick="@ClearCode">
                        Clear
                    </MudButton>

                    <MudButton Variant="Variant.Outlined"
                              StartIcon="@Icons.Material.Filled.Help"
                              OnClick="@AskAI">
                        Ask AI Mate
                    </MudButton>
                </div>
            </MudPaper>
        </MudItem>

        <MudItem xs="12" md="6">
            <MudPaper Class="pa-3" Elevation="2" Style="height: 100%;">
                <MudText Typo="Typo.h6" Class="mb-3">Output</MudText>

                @if (_result != null)
                {
                    @if (_result.Success)
                    {
                        <MudAlert Severity="Severity.Success" Class="mb-3">
                            Executed in @_result.ExecutionTime.TotalMilliseconds ms
                        </MudAlert>

                        @if (!string.IsNullOrEmpty(_result.Output))
                        {
                            <MudText Typo="Typo.subtitle2" Class="mb-1">Console Output:</MudText>
                            <MudPaper Class="pa-2 mb-3" Elevation="0" Style="background: #1e1e1e; color: #d4d4d4;">
                                <pre style="margin: 0; font-family: 'Consolas', monospace;">@_result.Output</pre>
                            </MudPaper>
                        }

                        @if (_result.ReturnValue != null)
                        {
                            <MudText Typo="Typo.subtitle2" Class="mb-1">Return Value:</MudText>
                            <MudPaper Class="pa-2" Elevation="0" Style="background: #1e1e1e; color: #4ec9b0;">
                                <pre style="margin: 0; font-family: 'Consolas', monospace;">@_result.ReturnValue</pre>
                            </MudPaper>
                        }
                    }
                    else
                    {
                        <MudAlert Severity="Severity.Error" Class="mb-3">
                            Compilation/Execution Failed
                        </MudAlert>

                        <MudPaper Class="pa-2" Elevation="0" Style="background: #3c1414; color: #f48771;">
                            <pre style="margin: 0; font-family: 'Consolas', monospace;">@_result.Errors</pre>
                        </MudPaper>

                        @if (_result.Diagnostics.Any())
                        {
                            <MudText Typo="Typo.subtitle2" Class="mt-3 mb-1">Diagnostics:</MudText>
                            @foreach (var diag in _result.Diagnostics)
                            {
                                <MudChip Size="Size.Small" Color="Color.Error" Class="mb-1">
                                    Line @diag.Line: @diag.Message
                                </MudChip>
                            }
                        }
                    }
                }
                else
                {
                    <MudText Color="Color.Secondary">
                        Write C# code and click "Run Code" to see output here.
                    </MudText>
                }
            </MudPaper>
        </MudItem>
    </MudGrid>
</MudContainer>

@code {
    private CodeEditor _editor = null!;
    private string _code = @"// Welcome to CodeMate! Write C# code here.
// Example: Calculate factorial

int Factorial(int n)
{
    if (n <= 1) return 1;
    return n * Factorial(n - 1);
}

var result = Factorial(10);
Console.WriteLine($""Factorial of 10 is: {result}"");
return result;";

    private CodeExecutionResult? _result;
    private bool _isExecuting = false;

    private async Task RunCode()
    {
        _isExecuting = true;
        StateHasChanged();

        try
        {
            var code = await _editor.GetCodeAsync();
            _result = await CodeExecution.ExecuteCSharpAsync(code, TimeSpan.FromSeconds(30));

            if (_result.Success)
            {
                Snackbar.Add("Code executed successfully!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Compilation failed. Check output for details.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isExecuting = false;
            StateHasChanged();
        }
    }

    private async Task FormatCode()
    {
        await _editor.FormatAsync();
        Snackbar.Add("Code formatted", Severity.Info);
    }

    private async Task ClearCode()
    {
        await _editor.SetCodeAsync("");
        _result = null;
    }

    private async Task AskAI()
    {
        // Open chat with current code as context
        var code = await _editor.GetCodeAsync();
        // Navigate to chat with pre-filled message
        // NavigationManager.NavigateTo($"/chat?code={Uri.EscapeDataString(code)}");
    }
}
```

### Phase 2.4: Plugin Scaffolding (1 week)

#### Plugin Generator Service
**Location:** `Infrastructure/Services/PluginScaffoldingService.cs`

```csharp
public class PluginScaffoldingService
{
    public string GeneratePluginCode(string pluginName, string description)
    {
        return $@"using AiMate.Core.Plugins;
using System.Threading.Tasks;

namespace AiMate.Plugins.Custom
{{
    /// <summary>
    /// {description}
    /// </summary>
    public class {pluginName}Plugin : IPlugin
    {{
        public string Name => ""{pluginName}"";
        public string Description => ""{description}"";
        public string Version => ""1.0.0"";

        public async Task<PluginResult> ExecuteAsync(PluginContext context)
        {{
            // TODO: Implement your plugin logic here

            var input = context.Input;

            // Example: Process the input
            var result = $""Processed: {{input}}"";

            return new PluginResult
            {{
                Success = true,
                Output = result,
                Metadata = new Dictionary<string, object>
                {{
                    [""processed_at""] = DateTime.UtcNow,
                    [""plugin_name""] = Name
                }}
            }};
        }}

        public Task<bool> ValidateAsync(PluginContext context)
        {{
            // TODO: Add validation logic
            return Task.FromResult(true);
        }}
    }}
}}";
    }

    public string GeneratePluginTests(string pluginName)
    {
        return $@"using Xunit;
using AiMate.Plugins.Custom;

namespace AiMate.Tests.Plugins
{{
    public class {pluginName}PluginTests
    {{
        private readonly {pluginName}Plugin _plugin;

        public {pluginName}PluginTests()
        {{
            _plugin = new {pluginName}Plugin();
        }}

        [Fact]
        public async Task Execute_WithValidInput_ReturnsSuccess()
        {{
            // Arrange
            var context = new PluginContext
            {{
                Input = ""test input"",
                UserId = Guid.NewGuid()
            }};

            // Act
            var result = await _plugin.ExecuteAsync(context);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Output);
        }}

        [Fact]
        public async Task Validate_WithValidContext_ReturnsTrue()
        {{
            // Arrange
            var context = new PluginContext();

            // Act
            var isValid = await _plugin.ValidateAsync(context);

            // Assert
            Assert.True(isValid);
        }}
    }}
}}";
    }
}
```

#### Plugin Creation Dialog
```razor
<MudDialog>
    <DialogContent>
        <MudTextField @bind-Value="@_pluginName"
                     Label="Plugin Name"
                     Required="true" />

        <MudTextField @bind-Value="@_description"
                     Label="Description"
                     Lines="3"
                     Required="true" />

        <MudCheckBox @bind-Checked="@_includeTests">
            Generate unit tests
        </MudCheckBox>
    </DialogContent>

    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" OnClick="Generate">
            Generate Plugin
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    private async Task Generate()
    {
        var code = _scaffolding.GeneratePluginCode(_pluginName, _description);

        // Open in CodeMate
        NavigationManager.NavigateTo($"/codemate?code={Uri.EscapeDataString(code)}");

        MudDialog.Close();
    }
}
```

---

## 🚀 Implementation Timeline

### Month 1: AI Mates Foundation
- **Week 1:** Database schema, entities, migrations
- **Week 2:** Memory system (auto-capture, retrieval)
- **Week 3:** UI components (management page, selector)
- **Week 4:** Proactive suggestions, polish

### Month 2: CodeMate Core
- **Week 1:** Roslyn integration, basic execution
- **Week 2:** MCP tool, chat integration
- **Week 3:** Monaco editor, live UI
- **Week 4:** Plugin scaffolding, examples

### Month 3: Integration & Polish
- **Week 1:** AI Mates ↔ CodeMate integration
- **Week 2:** Example plugins, tutorials
- **Week 3:** Testing, bug fixes
- **Week 4:** Documentation, launch prep

---

## 📊 Success Metrics

### AI Mates
- Users create average 3+ AI Mates per account
- 60%+ daily active users interact with their mates
- Memory capture rate: 80%+ conversations generate memories
- Proactive suggestion click-through: 30%+

### CodeMate
- 50%+ of developers use CodeMate weekly
- Average 10+ code executions per session
- Plugin creation: 20%+ of power users create plugins
- Zero critical security incidents

---

## 🔒 Security Considerations

### Code Execution
- ✅ No file system access (Roslyn scripts are sandboxed)
- ✅ No network access by default
- ✅ Memory limits (max 512MB per execution)
- ✅ CPU timeout (max 30s per execution)
- ✅ Rate limiting (10 executions per minute)
- ⚠️ Future: Run in Docker containers for full isolation

### AI Mate Memories
- ✅ Encrypted at rest (PostgreSQL encryption)
- ✅ User-specific isolation (foreign key constraints)
- ✅ GDPR compliance (full memory export/delete)
- ✅ No PII in embeddings (strip before vectorization)

---

## 💡 Quick Wins (Can Start Immediately)

1. **Basic AI Mate Creation** (2 days)
   - Just the entity, service, and UI
   - No memory system yet
   - Users can create and name their mates

2. **Simple Code Execution** (3 days)
   - Just Roslyn execution, no UI
   - Add to MCP tools
   - Test in chat

3. **Memory Auto-Capture** (2 days)
   - After each message, extract key points
   - Store in Knowledge Base
   - No complex retrieval yet

Start with these to validate the concepts, then build out full features.

---

## 🎯 Next Steps

1. **Review this roadmap** - Any changes to priorities?
2. **Start Phase 1.1** - Create database migration for AI Mates
3. **Proof of concept** - Simple AI Mate with one hardcoded memory
4. **Iterate** - Get user feedback, adjust course

---

**Ready to start building the future of AI collaboration?** 🚀🇳🇿

Which phase should we tackle first? I recommend starting with Phase 1.1 (AI Mates database schema) - we can have that done in an hour!
