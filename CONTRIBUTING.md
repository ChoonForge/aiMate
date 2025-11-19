# Contributing to aiMate

**First off, thanks for being here!** We built aiMate to be truly open source - no gatekeeping, no "benefactors" tier. Every contribution matters.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Contributing Code](#contributing-code)
  - [Improving Documentation](#improving-documentation)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Code Standards](#code-standards)
- [Testing](#testing)
- [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you're expected to uphold this code. Please report unacceptable behaviour to conduct@aimate.co.nz.

## How Can I Contribute?

### Reporting Bugs

**Before submitting a bug report:**
- Check the [existing issues](https://github.com/ChoonForge/aiMate/issues) to avoid duplicates
- Try the latest version - it might already be fixed
- Collect debugging information (logs, screenshots, browser console)

**How to submit a good bug report:**

Use our [bug report template](.github/ISSUE_TEMPLATE/bug_report.md) and include:

- **Clear title** - "Chat crashes when uploading 10MB+ files" not "it broke"
- **Steps to reproduce** - Numbered, specific steps
- **Expected behaviour** - What should happen
- **Actual behaviour** - What actually happened
- **Environment** - OS, browser, .NET version, Docker version
- **Logs** - Error messages, stack traces, console output
- **Screenshots** - If UI-related

### Suggesting Features

We love new ideas! Before suggesting:

- Check [existing feature requests](https://github.com/ChoonForge/aiMate/issues?q=is%3Aissue+label%3Aenhancement)
- Consider if it fits aiMate's vision (workspace-focused, privacy-first, Kiwi-friendly)

**Use our [feature request template](.github/ISSUE_TEMPLATE/feature_request.md) and include:**

- **Problem statement** - What problem does this solve?
- **Proposed solution** - How would it work?
- **Alternatives considered** - Other approaches you've thought about
- **Use cases** - Real-world scenarios where this helps

### Contributing Code

**Types of contributions we need:**

- 🐛 Bug fixes
- ✨ New features
- 🎨 UI/UX improvements
- 📝 Documentation
- 🧪 Tests
- ♿ Accessibility improvements
- 🌍 Translations (especially Te Reo Māori!)
- 🔧 Performance optimizations

**Not sure where to start?** Look for issues labeled:
- `good first issue` - Perfect for newcomers
- `help wanted` - We'd love community help on these
- `documentation` - Improve our docs

### Improving Documentation

Documentation is just as important as code! You can help by:

- Fixing typos and grammatical errors
- Adding examples and tutorials
- Improving API documentation
- Translating docs to other languages
- Adding diagrams and screenshots

## Development Setup

### Prerequisites

**Minimum (for InMemory database):**
- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git**
- **Your favorite IDE** (Visual Studio, Rider, VS Code)

**Recommended (for full stack):**
- **Docker & Docker Compose** - [Download here](https://docs.docker.com/get-docker/)
- **PostgreSQL 16+** with pgvector extension (optional - only if not using Docker or InMemory)

### Quick Start

**Option 1: Local Development with InMemory Database (Fastest)**

Perfect for quick feature development and testing.

```bash
# 1. Fork the repository on GitHub
# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/aiMate.git
cd aiMate

# 3. Add upstream remote
git remote add upstream https://github.com/ChoonForge/aiMate.git

# 4. Create a branch
git checkout -b feature/your-feature-name

# 5. Navigate to src
cd src

# 6. Restore dependencies
dotnet restore

# 7. Run the application (uses InMemory database by default)
dotnet run --project AiMate.Web

# 8. Open browser
# https://localhost:5001
```

**Option 2: Docker with PostgreSQL (Production-like)**

For testing with persistent storage and full services.

```bash
# 1-4. Same as above (fork, clone, add upstream, create branch)

# 5. Navigate to src
cd src

# 6. Copy environment template
cp .env.example .env

# 7. Edit .env and set DATABASE_PROVIDER=PostgreSQL
nano .env

# 8. Start all services with Docker
docker-compose -f docker-compose.production.yml --profile with-postgres up -d

# 9. Application is now running at http://localhost:5000
```

### Development Tips

**Hot Reload:**
```bash
dotnet watch run --project AiMate.Web
```

**View Logs:**
```bash
docker-compose logs -f
```

**Database Console:**
```bash
docker-compose exec postgres psql -U aimate
```

**Run Tests:**
```bash
dotnet test
```

## Pull Request Process

### Before You Submit

- [ ] Your code builds without errors (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] You've added tests for new functionality
- [ ] Code follows our style guidelines (see below)
- [ ] Comments explain "why", not "what"
- [ ] Documentation updated if needed
- [ ] No unnecessary dependencies added
- [ ] Commit messages are clear and descriptive

### PR Checklist

1. **Update your fork:**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create Pull Request** on GitHub

4. **Fill out the PR template** completely

5. **Link related issues** (e.g., "Fixes #123")

6. **Be responsive** to review feedback

### PR Title Format

Use conventional commits:

- `feat: Add workspace sharing functionality`
- `fix: Resolve chat crash on large file uploads`
- `docs: Update API documentation for feedback system`
- `refactor: Simplify personality detection logic`
- `test: Add unit tests for KnowledgeGraphService`
- `chore: Update dependencies`

### Review Process

- PRs require at least one approval from a maintainer
- CI checks must pass (build, tests, linting)
- We aim to review within 48 hours
- Be patient - quality over speed

## Code Standards

### C# Style

We follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// ✅ Good
public async Task<Workspace> CreateWorkspaceAsync(CreateWorkspaceRequest request)
{
    var workspace = new Workspace
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        CreatedAt = DateTime.UtcNow
    };

    await _context.Workspaces.AddAsync(workspace);
    await _context.SaveChangesAsync();

    return workspace;
}

// ❌ Bad
public async Task<Workspace> create(CreateWorkspaceRequest req)
{
    var ws = new Workspace { Id=Guid.NewGuid(),Name=req.Name,CreatedAt=DateTime.UtcNow };
    await _context.Workspaces.AddAsync(ws);await _context.SaveChangesAsync();return ws;
}
```

**Key Guidelines:**

- **PascalCase** for classes, methods, properties
- **camelCase** for local variables, parameters
- **_camelCase** for private fields
- **4 spaces** for indentation (no tabs)
- **Explicit types** over `var` for clarity (except LINQ)
- **Async suffix** for async methods
- **XML comments** for public APIs

### Architecture

We use **Clean Architecture**:

```
AiMate.Core/           # Domain entities, interfaces, business logic
  ├── Entities/        # Database models (User, Workspace, Message, etc.)
  ├── Services/        # Service interfaces (IWorkspaceService, etc.)
  └── Enums/           # Enumerations

AiMate.Infrastructure/ # External concerns (database, APIs, file storage)
  ├── Data/            # EF Core DbContext
  ├── Services/        # Service implementations
  └── Migrations/      # Database migrations

AiMate.Web/           # UI and presentation layer
  ├── Components/      # Razor components
  ├── Store/           # Fluxor state management
  └── Program.cs       # App configuration

AiMate.Shared/        # DTOs and shared models
```

**Rules:**
- Core has NO dependencies on other layers
- Infrastructure depends on Core
- Web depends on Core and Infrastructure
- Never skip layers (e.g., Web → Infrastructure directly for business logic)

### Blazor Components

```razor
@* ✅ Good component structure *@
<MudCard Class="workspace-card">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Workspace.Name</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudIconButton Icon="@Icons.Material.Filled.Edit"
                          OnClick="@OnEditClicked" />
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudText>@Workspace.Description</MudText>
    </MudCardContent>
</MudCard>

@code {
    [Parameter, EditorRequired]
    public Workspace Workspace { get; set; } = null!;

    [Parameter]
    public EventCallback<Workspace> OnEdit { get; set; }

    private async Task OnEditClicked()
    {
        await OnEdit.InvokeAsync(Workspace);
    }
}
```

### State Management (Fluxor)

Follow the Flux pattern:

```csharp
// 1. Define state
public record WorkspaceState
{
    public List<Workspace> Workspaces { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
}

// 2. Define actions
public record LoadWorkspacesAction;
public record LoadWorkspacesSuccessAction(List<Workspace> Workspaces);
public record LoadWorkspacesFailureAction(string Error);

// 3. Define reducer
public static class WorkspaceReducers
{
    [ReducerMethod]
    public static WorkspaceState ReduceLoadWorkspaces(
        WorkspaceState state,
        LoadWorkspacesAction action)
    {
        return state with { IsLoading = true, Error = null };
    }
}

// 4. Define effect
public class WorkspaceEffects
{
    [EffectMethod]
    public async Task HandleLoadWorkspaces(
        LoadWorkspacesAction action,
        IDispatcher dispatcher)
    {
        try
        {
            var workspaces = await _workspaceService.GetAllAsync();
            dispatcher.Dispatch(new LoadWorkspacesSuccessAction(workspaces));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadWorkspacesFailureAction(ex.Message));
        }
    }
}
```

## Testing

### Unit Tests

Use xUnit + FluentAssertions:

```csharp
public class WorkspaceServiceTests
{
    [Fact]
    public async Task CreateWorkspace_ValidRequest_ReturnsWorkspace()
    {
        // Arrange
        var service = new WorkspaceService(_mockContext.Object);
        var request = new CreateWorkspaceRequest { Name = "Test" };

        // Act
        var result = await service.CreateWorkspaceAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }
}
```

### Integration Tests

Test full stack with TestServer:

```csharp
public class ChatApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task PostMessage_ValidRequest_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { message = "Hello" };

        // Act
        var response = await client.PostAsJsonAsync("/api/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Test Coverage

Aim for:
- **80%+ coverage** for business logic (Core/Infrastructure)
- **60%+ coverage** for UI components (Web)
- **100% coverage** for critical paths (auth, payments, data loss)

## Community

### Where to Ask Questions

- **GitHub Discussions** - General questions, ideas, showcase
- **Issues** - Bug reports and feature requests
- **Discord** - Real-time chat (coming soon)
- **Email** - hello@aimate.co.nz

### Recognition

Contributors are recognized in:
- README.md contributors section
- Release notes
- Our gratitude!

### Becoming a Maintainer

Active contributors may be invited to become maintainers. We look for:
- Consistent, quality contributions
- Helping others in issues/discussions
- Understanding of the codebase and vision
- Good communication skills

## Questions?

Don't hesitate to ask! We're here to help:

- Open a [discussion](https://github.com/ChoonForge/aiMate/discussions)
- Ask in an issue
- Email us: hello@aimate.co.nz

## Thank You!

Every contribution makes aiMate better for everyone. Whether it's a typo fix or a major feature, we appreciate you taking the time to help.

**Built with ❤️ by the community, for the community.** 🇳🇿

Kia ora! 🥝
