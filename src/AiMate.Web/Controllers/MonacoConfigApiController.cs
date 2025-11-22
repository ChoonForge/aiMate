using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for Monaco Editor configuration (themes, languages, settings)
/// </summary>
[ApiController]
[Route("api/v1/monaco")]
[Authorize]
public class MonacoConfigApiController : ControllerBase
{
    private readonly ILogger<MonacoConfigApiController> _logger;

    public MonacoConfigApiController(ILogger<MonacoConfigApiController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get available Monaco themes
    /// </summary>
    /// <returns>List of available themes</returns>
    /// <response code="200">Returns list of themes</response>
    [HttpGet("themes")]
    [ProducesResponseType(typeof(List<MonacoTheme>), StatusCodes.Status200OK)]
    public IActionResult GetThemes()
    {
        var themes = new List<MonacoTheme>
        {
            new MonacoTheme
            {
                Id = "vs",
                Name = "Visual Studio Light",
                Type = "light",
                IsBuiltIn = true
            },
            new MonacoTheme
            {
                Id = "vs-dark",
                Name = "Visual Studio Dark",
                Type = "dark",
                IsBuiltIn = true
            },
            new MonacoTheme
            {
                Id = "hc-black",
                Name = "High Contrast Dark",
                Type = "dark",
                IsBuiltIn = true
            },
            new MonacoTheme
            {
                Id = "hc-light",
                Name = "High Contrast Light",
                Type = "light",
                IsBuiltIn = true
            }
        };

        return Ok(themes);
    }

    /// <summary>
    /// Get supported programming languages
    /// </summary>
    /// <returns>List of supported languages</returns>
    /// <response code="200">Returns list of languages</response>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(List<MonacoLanguage>), StatusCodes.Status200OK)]
    public IActionResult GetLanguages()
    {
        var languages = new List<MonacoLanguage>
        {
            new MonacoLanguage { Id = "csharp", Name = "C#", Extensions = new[] { ".cs" }, Aliases = new[] { "C#", "csharp", "cs" } },
            new MonacoLanguage { Id = "razor", Name = "Razor", Extensions = new[] { ".razor", ".cshtml" }, Aliases = new[] { "Razor", "razor" } },
            new MonacoLanguage { Id = "javascript", Name = "JavaScript", Extensions = new[] { ".js", ".mjs" }, Aliases = new[] { "JavaScript", "javascript", "js" } },
            new MonacoLanguage { Id = "typescript", Name = "TypeScript", Extensions = new[] { ".ts", ".tsx" }, Aliases = new[] { "TypeScript", "typescript", "ts" } },
            new MonacoLanguage { Id = "json", Name = "JSON", Extensions = new[] { ".json" }, Aliases = new[] { "JSON", "json" } },
            new MonacoLanguage { Id = "xml", Name = "XML", Extensions = new[] { ".xml", ".csproj", ".config" }, Aliases = new[] { "XML", "xml" } },
            new MonacoLanguage { Id = "html", Name = "HTML", Extensions = new[] { ".html", ".htm" }, Aliases = new[] { "HTML", "html" } },
            new MonacoLanguage { Id = "css", Name = "CSS", Extensions = new[] { ".css" }, Aliases = new[] { "CSS", "css" } },
            new MonacoLanguage { Id = "scss", Name = "SCSS", Extensions = new[] { ".scss" }, Aliases = new[] { "SCSS", "scss" } },
            new MonacoLanguage { Id = "markdown", Name = "Markdown", Extensions = new[] { ".md", ".markdown" }, Aliases = new[] { "Markdown", "markdown", "md" } },
            new MonacoLanguage { Id = "yaml", Name = "YAML", Extensions = new[] { ".yml", ".yaml" }, Aliases = new[] { "YAML", "yaml" } },
            new MonacoLanguage { Id = "sql", Name = "SQL", Extensions = new[] { ".sql" }, Aliases = new[] { "SQL", "sql" } },
            new MonacoLanguage { Id = "powershell", Name = "PowerShell", Extensions = new[] { ".ps1", ".psm1" }, Aliases = new[] { "PowerShell", "powershell", "ps1" } },
            new MonacoLanguage { Id = "dockerfile", Name = "Dockerfile", Extensions = new[] { "Dockerfile" }, Aliases = new[] { "Dockerfile", "dockerfile" } },
            new MonacoLanguage { Id = "python", Name = "Python", Extensions = new[] { ".py" }, Aliases = new[] { "Python", "python", "py" } }
        };

        return Ok(languages);
    }

    /// <summary>
    /// Get Monaco editor settings/options
    /// </summary>
    /// <returns>Default editor settings</returns>
    /// <response code="200">Returns editor settings</response>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(MonacoSettings), StatusCodes.Status200OK)]
    public IActionResult GetSettings()
    {
        var settings = new MonacoSettings
        {
            FontSize = 14,
            FontFamily = "Consolas, 'Courier New', monospace",
            LineHeight = 21,
            TabSize = 4,
            InsertSpaces = true,
            WordWrap = "on",
            MinimapEnabled = true,
            LineNumbers = "on",
            RenderWhitespace = "selection",
            ScrollBeyondLastLine = false,
            AutoClosingBrackets = "always",
            AutoClosingQuotes = "always",
            FormatOnPaste = true,
            FormatOnType = true,
            SuggestOnTriggerCharacters = true,
            AcceptSuggestionOnEnter = "on",
            QuickSuggestionsDelay = 10
        };

        return Ok(settings);
    }

    /// <summary>
    /// Get file icons configuration
    /// </summary>
    /// <returns>File icon mappings</returns>
    /// <response code="200">Returns file icon mappings</response>
    [HttpGet("file-icons")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public IActionResult GetFileIcons()
    {
        var icons = new Dictionary<string, string>
        {
            [".cs"] = "csharp",
            [".razor"] = "razor",
            [".cshtml"] = "razor",
            [".csproj"] = "csharp",
            [".sln"] = "visualstudio",
            [".js"] = "javascript",
            [".ts"] = "typescript",
            [".tsx"] = "react",
            [".jsx"] = "react",
            [".json"] = "json",
            [".xml"] = "xml",
            [".html"] = "html",
            [".css"] = "css",
            [".scss"] = "scss",
            [".md"] = "markdown",
            [".yml"] = "yaml",
            [".yaml"] = "yaml",
            [".sql"] = "database",
            [".ps1"] = "powershell",
            ["Dockerfile"] = "docker",
            [".py"] = "python"
        };

        return Ok(icons);
    }

    /// <summary>
    /// Get keyboard shortcuts
    /// </summary>
    /// <returns>List of keyboard shortcuts</returns>
    /// <response code="200">Returns keyboard shortcuts</response>
    [HttpGet("shortcuts")]
    [ProducesResponseType(typeof(List<KeyboardShortcut>), StatusCodes.Status200OK)]
    public IActionResult GetShortcuts()
    {
        var shortcuts = new List<KeyboardShortcut>
        {
            new KeyboardShortcut { Key = "Ctrl+S", Command = "save", Description = "Save file" },
            new KeyboardShortcut { Key = "Ctrl+Shift+F", Command = "format", Description = "Format document" },
            new KeyboardShortcut { Key = "F12", Command = "goToDefinition", Description = "Go to definition" },
            new KeyboardShortcut { Key = "Ctrl+K Ctrl+D", Command = "formatDocument", Description = "Format entire document" },
            new KeyboardShortcut { Key = "Ctrl+Space", Command = "triggerSuggest", Description = "Trigger IntelliSense" },
            new KeyboardShortcut { Key = "Ctrl+Shift+Space", Command = "triggerParameterHints", Description = "Show parameter hints" },
            new KeyboardShortcut { Key = "F2", Command = "rename", Description = "Rename symbol" },
            new KeyboardShortcut { Key = "Ctrl+.", Command = "quickFix", Description = "Quick fix" },
            new KeyboardShortcut { Key = "Ctrl+/", Command = "toggleComment", Description = "Toggle comment" },
            new KeyboardShortcut { Key = "Ctrl+F", Command = "find", Description = "Find" },
            new KeyboardShortcut { Key = "Ctrl+H", Command = "replace", Description = "Replace" }
        };

        return Ok(shortcuts);
    }
}

/// <summary>
/// Monaco theme configuration
/// </summary>
public class MonacoTheme
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "light" or "dark"
    public bool IsBuiltIn { get; set; }
}

/// <summary>
/// Monaco language configuration
/// </summary>
public class MonacoLanguage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Extensions { get; set; } = Array.Empty<string>();
    public string[] Aliases { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Monaco editor settings
/// </summary>
public class MonacoSettings
{
    public int FontSize { get; set; }
    public string FontFamily { get; set; } = string.Empty;
    public int LineHeight { get; set; }
    public int TabSize { get; set; }
    public bool InsertSpaces { get; set; }
    public string WordWrap { get; set; } = string.Empty;
    public bool MinimapEnabled { get; set; }
    public string LineNumbers { get; set; } = string.Empty;
    public string RenderWhitespace { get; set; } = string.Empty;
    public bool ScrollBeyondLastLine { get; set; }
    public string AutoClosingBrackets { get; set; } = string.Empty;
    public string AutoClosingQuotes { get; set; } = string.Empty;
    public bool FormatOnPaste { get; set; }
    public bool FormatOnType { get; set; }
    public bool SuggestOnTriggerCharacters { get; set; }
    public string AcceptSuggestionOnEnter { get; set; } = string.Empty;
    public int QuickSuggestionsDelay { get; set; }
}

/// <summary>
/// Keyboard shortcut definition
/// </summary>
public class KeyboardShortcut
{
    public string Key { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
