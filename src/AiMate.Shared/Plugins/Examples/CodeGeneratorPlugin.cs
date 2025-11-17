using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AiMate.Shared.Models;
using AiMate.Shared.Plugins;

namespace AiMate.Shared.Plugins.Examples
{
    /// <summary>
    /// Example plugin that generates C# code from natural language
    /// Demonstrates all plugin capabilities
    /// </summary>
    public class CodeGeneratorPlugin : IMessageInterceptor, IUIExtension, IToolProvider
    {
        #region IPlugin Implementation

        public string Id => "code-generator";
        public string Name => "C# Code Generator";
        public string Description => "Generates C# code from natural language descriptions";
        public string Version => "1.0.0";
        public string Author => "aiMate Team";
        public string Icon => "Code";

        public Task InitializeAsync()
        {
            Console.WriteLine($"[{Name}] Plugin initialized!");
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Console.WriteLine($"[{Name}] Plugin disposed");
            return Task.CompletedTask;
        }

        #endregion

        #region IMessageInterceptor Implementation

        public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
        {
            // Check if user is requesting code generation
            if (message.Content.ToLower().Contains("generate code") || 
                message.Content.ToLower().Contains("write a function"))
            {
                // Enhance the prompt with coding-specific instructions
                var enhancedContent = $@"{message.Content}

[Code Generation Instructions]
- Use C# (.NET 9)
- Follow best practices
- Include XML documentation comments
- Add error handling
- Make it production-ready";

                return new MessageInterceptResult
                {
                    Continue = true,
                    ModifiedMessage = new Message
                    {
                        Id = message.Id,
                        Role = message.Role,
                        Content = enhancedContent,
                        Timestamp = message.Timestamp
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["enhanced_by"] = Id,
                        ["original_content"] = message.Content
                    }
                };
            }

            return new MessageInterceptResult { Continue = true };
        }

        public async Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context)
        {
            // If response contains code, add metadata for syntax highlighting
            if (message.Content.Contains("```csharp") || message.Content.Contains("```cs"))
            {
                // Could trigger automatic code validation, formatting, etc.
                Console.WriteLine($"[{Name}] Detected C# code in response");
            }

            return new MessageInterceptResult { Continue = true };
        }

        #endregion

        #region IUIExtension Implementation

        public IEnumerable<MessageActionButton> GetMessageActions(Message message)
        {
            // Add "Run Code" button if message contains code
            if (message.Content.Contains("```csharp"))
            {
                yield return new MessageActionButton
                {
                    Id = "run-code",
                    Label = "Run Code",
                    Icon = "PlayArrow",
                    Tooltip = "Execute this C# code",
                    ShowOnAssistantMessages = true,
                    OnClick = async (msg) =>
                    {
                        await RunCodeAsync(msg.Content);
                    }
                };

                yield return new MessageActionButton
                {
                    Id = "save-code",
                    Label = "Save to File",
                    Icon = "Save",
                    Tooltip = "Save code to .cs file",
                    ShowOnAssistantMessages = true,
                    OnClick = async (msg) =>
                    {
                        await SaveCodeToFileAsync(msg.Content);
                    }
                };
            }
        }

        public IEnumerable<ChatInputExtension> GetInputExtensions()
        {
            // Add "Quick Code" button to chat input
            yield return new ChatInputExtension
            {
                Id = "quick-code",
                Icon = "Code",
                Tooltip = "Generate code from template",
                Order = 10,
                OnClick = async () =>
                {
                    // Open code template picker
                    Console.WriteLine("Opening code template picker...");
                    await Task.CompletedTask;
                }
            };
        }

        public PluginSettings GetSettingsUI()
        {
            return new PluginSettings
            {
                Title = "Code Generator Settings",
                Fields = new List<SettingField>
                {
                    new SettingField
                    {
                        Key = "auto_enhance",
                        Label = "Auto-enhance code prompts",
                        Type = SettingFieldType.Boolean,
                        DefaultValue = true
                    },
                    new SettingField
                    {
                        Key = "target_framework",
                        Label = "Target Framework",
                        Type = SettingFieldType.Dropdown,
                        DefaultValue = "net9.0",
                        Options = new List<string> { "net9.0", "net8.0", "net7.0" }
                    },
                    new SettingField
                    {
                        Key = "code_style",
                        Label = "Code Style",
                        Type = SettingFieldType.Dropdown,
                        DefaultValue = "Microsoft",
                        Options = new List<string> { "Microsoft", "Google", "Custom" }
                    }
                }
            };
        }

        public string? RenderCustomContent(Message message)
        {
            // Could render code with custom syntax highlighting
            return null;
        }

        #endregion

        #region IToolProvider Implementation

        public IEnumerable<PluginTool> GetTools()
        {
            yield return new PluginTool
            {
                Name = "generate_class",
                Description = "Generate a C# class with properties and methods",
                RequiresConfirmation = false,
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "class_name",
                        Description = "Name of the class to generate",
                        Type = typeof(string),
                        Required = true
                    },
                    new ToolParameter
                    {
                        Name = "properties",
                        Description = "List of properties (JSON array)",
                        Type = typeof(string),
                        Required = true
                    },
                    new ToolParameter
                    {
                        Name = "namespace",
                        Description = "Namespace for the class",
                        Type = typeof(string),
                        Required = false,
                        DefaultValue = "MyApp"
                    }
                }
            };

            yield return new PluginTool
            {
                Name = "refactor_code",
                Description = "Refactor existing C# code with improvements",
                RequiresConfirmation = true,
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "code",
                        Description = "The C# code to refactor",
                        Type = typeof(string),
                        Required = true
                    },
                    new ToolParameter
                    {
                        Name = "improvements",
                        Description = "Comma-separated list: performance, readability, security",
                        Type = typeof(string),
                        Required = false,
                        DefaultValue = "readability,performance"
                    }
                }
            };
        }

        public async Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            try
            {
                return toolName switch
                {
                    "generate_class" => await GenerateClassAsync(parameters),
                    "refactor_code" => await RefactorCodeAsync(parameters),
                    _ => new ToolResult { Success = false, Error = $"Unknown tool: {toolName}" }
                };
            }
            catch (Exception ex)
            {
                return new ToolResult { Success = false, Error = ex.Message };
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<ToolResult> GenerateClassAsync(Dictionary<string, object> parameters)
        {
            var className = parameters["class_name"].ToString();
            var propertiesJson = parameters["properties"].ToString();
            var namespaceName = parameters.GetValueOrDefault("namespace", "MyApp").ToString();

            var properties = JsonSerializer.Deserialize<List<PropertyDefinition>>(propertiesJson!);

            var code = $@"namespace {namespaceName};

/// <summary>
/// {className} class
/// </summary>
public class {className}
{{
{string.Join("\n", properties!.Select(p => $"    public {p.Type} {p.Name} {{ get; set; }}"))}

    public {className}()
    {{
        // Constructor
    }}
}}";

            return new ToolResult
            {
                Success = true,
                Result = code,
                Metadata = new Dictionary<string, object>
                {
                    ["language"] = "csharp",
                    ["class_name"] = className!,
                    ["namespace"] = namespaceName!
                }
            };
        }

        private async Task<ToolResult> RefactorCodeAsync(Dictionary<string, object> parameters)
        {
            var code = parameters["code"].ToString();
            var improvements = parameters.GetValueOrDefault("improvements", "readability").ToString();

            // In reality, this would call an LLM or static analysis tool
            var refactoredCode = $"// Refactored with: {improvements}\n{code}";

            return new ToolResult
            {
                Success = true,
                Result = refactoredCode,
                Metadata = new Dictionary<string, object>
                {
                    ["improvements_applied"] = improvements!
                }
            };
        }

        private async Task RunCodeAsync(string content)
        {
            // Extract code blocks
            var codeBlock = ExtractCodeBlock(content);
            Console.WriteLine($"[{Name}] Running code: {codeBlock.Substring(0, Math.Min(50, codeBlock.Length))}...");
            
            // In reality, would compile and execute using Roslyn
            await Task.CompletedTask;
        }

        private async Task SaveCodeToFileAsync(string content)
        {
            var codeBlock = ExtractCodeBlock(content);
            Console.WriteLine($"[{Name}] Saving code to file...");
            
            // Would trigger file download
            await Task.CompletedTask;
        }

        private string ExtractCodeBlock(string content)
        {
            var startIndex = content.IndexOf("```csharp");
            if (startIndex == -1) startIndex = content.IndexOf("```cs");
            if (startIndex == -1) return content;

            startIndex = content.IndexOf('\n', startIndex) + 1;
            var endIndex = content.IndexOf("```", startIndex);
            
            return endIndex == -1 ? content[startIndex..] : content[startIndex..endIndex].Trim();
        }

        #endregion

        private class PropertyDefinition
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "string";
        }
    }
}
