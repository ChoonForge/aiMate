using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace AiMate.Web.Services;

/// <summary>
/// Service for rendering markdown with syntax highlighting
/// </summary>
public class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseAutoLinks()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();
    }

    /// <summary>
    /// Convert markdown to HTML with syntax highlighting support
    /// </summary>
    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var html = Markdown.ToHtml(markdown, _pipeline);
        return ProcessCodeBlocks(html);
    }

    /// <summary>
    /// Process code blocks to add syntax highlighting classes
    /// </summary>
    private string ProcessCodeBlocks(string html)
    {
        // Replace code blocks with highlighted versions
        html = System.Text.RegularExpressions.Regex.Replace(
            html,
            @"<code class=""language-(\w+)"">",
            match => $"<code class=\"language-{match.Groups[1].Value} hljs\">",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        return html;
    }

    /// <summary>
    /// Extract code blocks from markdown for special rendering
    /// </summary>
    public List<CodeBlock> ExtractCodeBlocks(string markdown)
    {
        var document = Markdown.Parse(markdown, _pipeline);
        var codeBlocks = new List<CodeBlock>();

        foreach (var block in document.Descendants<FencedCodeBlock>())
        {
            codeBlocks.Add(new CodeBlock
            {
                Language = block.Info ?? "text",
                Code = block.Lines.ToString()
            });
        }

        return codeBlocks;
    }
}

public class CodeBlock
{
    public required string Language { get; set; }
    public required string Code { get; set; }
}
