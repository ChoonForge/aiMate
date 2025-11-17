using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AiMate.Shared.Models;
using AiMate.Shared.Plugins;

namespace AiMate.Shared.Plugins.Examples
{
    /// <summary>
    /// Web Search Plugin - Adds real-time web search capability
    /// Demonstrates how to add external data sources to conversations
    /// </summary>
    public class WebSearchPlugin : IMessageInterceptor, IUIExtension, IToolProvider
    {
        private readonly HttpClient _httpClient = new();
        private string? _apiKey;

        #region IPlugin Implementation

        public string Id => "web-search";
        public string Name => "Web Search";
        public string Description => "Search the web for current information";
        public string Version => "1.0.0";
        public string Author => "aiMate Team";
        public string Icon => "Search";

        public Task InitializeAsync()
        {
            // Load API key from settings
            _apiKey = Environment.GetEnvironmentVariable("SEARCH_API_KEY");
            Console.WriteLine($"[{Name}] Plugin initialized");
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _httpClient.Dispose();
            return Task.CompletedTask;
        }

        #endregion

        #region IMessageInterceptor Implementation

        public async Task<MessageInterceptResult> OnBeforeSendAsync(Message message, ConversationContext context)
        {
            // Detect if user is asking for current/recent information
            var needsSearch = ContainsSearchTriggers(message.Content);

            if (needsSearch && !message.Content.Contains("[WEB_SEARCH_RESULTS]"))
            {
                try
                {
                    // Perform web search
                    var searchQuery = ExtractSearchQuery(message.Content);
                    var results = await SearchWebAsync(searchQuery);

                    // Inject results into message
                    var enhancedContent = $@"{message.Content}

[WEB_SEARCH_RESULTS]
{FormatSearchResults(results)}

Please use the above web search results to answer the question accurately with current information.";

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
                            ["search_query"] = searchQuery,
                            ["results_count"] = results.Count,
                            ["search_performed"] = true
                        }
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Name}] Search failed: {ex.Message}");
                }
            }

            return new MessageInterceptResult { Continue = true };
        }

        public Task<MessageInterceptResult> OnAfterReceiveAsync(Message message, ConversationContext context)
        {
            // Could add citation links to web sources
            return Task.FromResult(new MessageInterceptResult { Continue = true });
        }

        #endregion

        #region IUIExtension Implementation

        public IEnumerable<MessageActionButton> GetMessageActions(Message message)
        {
            // Add "Search Web" button to user messages
            if (message.Role == "user")
            {
                yield return new MessageActionButton
                {
                    Id = "search-web",
                    Label = "Search Web",
                    Icon = "Search",
                    Tooltip = "Search web for this query",
                    ShowOnUserMessages = true,
                    OnClick = async (msg) =>
                    {
                        var results = await SearchWebAsync(msg.Content);
                        Console.WriteLine($"Found {results.Count} results");
                    }
                };
            }

            // Add "Verify Facts" button to assistant messages
            if (message.Role == "assistant")
            {
                yield return new MessageActionButton
                {
                    Id = "verify-facts",
                    Label = "Verify",
                    Icon = "FactCheck",
                    Tooltip = "Verify facts with web search",
                    ShowOnAssistantMessages = true,
                    OnClick = async (msg) =>
                    {
                        // Extract claims and verify them
                        Console.WriteLine("Verifying facts...");
                        await Task.CompletedTask;
                    }
                };
            }
        }

        public IEnumerable<ChatInputExtension> GetInputExtensions()
        {
            yield return new ChatInputExtension
            {
                Id = "quick-search",
                Icon = "TravelExplore",
                Tooltip = "Quick web search",
                Order = 20,
                OnClick = async () =>
                {
                    Console.WriteLine("Opening quick search...");
                    await Task.CompletedTask;
                }
            };
        }

        public PluginSettings GetSettingsUI()
        {
            return new PluginSettings
            {
                Title = "Web Search Settings",
                Fields = new List<SettingField>
                {
                    new SettingField
                    {
                        Key = "auto_search",
                        Label = "Auto-search for current info",
                        Type = SettingFieldType.Boolean,
                        DefaultValue = true
                    },
                    new SettingField
                    {
                        Key = "search_provider",
                        Label = "Search Provider",
                        Type = SettingFieldType.Dropdown,
                        DefaultValue = "Google",
                        Options = new List<string> { "Google", "Bing", "DuckDuckGo" }
                    },
                    new SettingField
                    {
                        Key = "max_results",
                        Label = "Max Results",
                        Type = SettingFieldType.Number,
                        DefaultValue = 5
                    },
                    new SettingField
                    {
                        Key = "api_key",
                        Label = "API Key",
                        Type = SettingFieldType.Text,
                        Placeholder = "Enter your search API key"
                    }
                }
            };
        }

        public string? RenderCustomContent(Message message)
        {
            return null;
        }

        #endregion

        #region IToolProvider Implementation

        public IEnumerable<PluginTool> GetTools()
        {
            yield return new PluginTool
            {
                Name = "web_search",
                Description = "Search the web for current information",
                RequiresConfirmation = false,
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "query",
                        Description = "Search query",
                        Type = typeof(string),
                        Required = true
                    },
                    new ToolParameter
                    {
                        Name = "num_results",
                        Description = "Number of results to return",
                        Type = typeof(int),
                        Required = false,
                        DefaultValue = 5
                    }
                }
            };

            yield return new PluginTool
            {
                Name = "get_webpage",
                Description = "Fetch and extract text from a webpage",
                RequiresConfirmation = false,
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "url",
                        Description = "URL of the webpage",
                        Type = typeof(string),
                        Required = true
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
                    "web_search" => await ExecuteWebSearchAsync(parameters),
                    "get_webpage" => await ExecuteGetWebpageAsync(parameters),
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

        private bool ContainsSearchTriggers(string content)
        {
            var triggers = new[]
            {
                "latest", "current", "recent", "today", "news", "what's happening",
                "what happened", "what is the", "who won", "who is the", "when did"
            };

            return triggers.Any(trigger => 
                content.ToLower().Contains(trigger));
        }

        private string ExtractSearchQuery(string content)
        {
            // Simple extraction - in reality, would use NLP
            return content.Length > 100 ? content.Substring(0, 100) : content;
        }

        private async Task<List<SearchResult>> SearchWebAsync(string query)
        {
            // Mock implementation - replace with actual search API
            await Task.Delay(100); // Simulate API call

            return new List<SearchResult>
            {
                new SearchResult
                {
                    Title = "Example Result 1",
                    Url = "https://example.com/1",
                    Snippet = "This is a sample search result snippet...",
                    Source = "example.com"
                },
                new SearchResult
                {
                    Title = "Example Result 2",
                    Url = "https://example.com/2",
                    Snippet = "Another example of search results...",
                    Source = "example.com"
                }
            };
        }

        private string FormatSearchResults(List<SearchResult> results)
        {
            return string.Join("\n\n", results.Select((r, i) => 
                $"[{i + 1}] {r.Title}\n{r.Snippet}\nSource: {r.Source}\nURL: {r.Url}"));
        }

        private async Task<ToolResult> ExecuteWebSearchAsync(Dictionary<string, object> parameters)
        {
            var query = parameters["query"].ToString()!;
            var numResults = parameters.GetValueOrDefault("num_results", 5);

            var results = await SearchWebAsync(query);

            return new ToolResult
            {
                Success = true,
                Result = results.Take((int)numResults).ToList(),
                Metadata = new Dictionary<string, object>
                {
                    ["query"] = query,
                    ["results_count"] = results.Count
                }
            };
        }

        private async Task<ToolResult> ExecuteGetWebpageAsync(Dictionary<string, object> parameters)
        {
            var url = parameters["url"].ToString()!;

            try
            {
                var content = await _httpClient.GetStringAsync(url);
                
                // Extract text (simplified - would use HTML parser in reality)
                var text = content.Length > 5000 ? content.Substring(0, 5000) : content;

                return new ToolResult
                {
                    Success = true,
                    Result = text,
                    Metadata = new Dictionary<string, object>
                    {
                        ["url"] = url,
                        ["content_length"] = content.Length
                    }
                };
            }
            catch (Exception ex)
            {
                return new ToolResult
                {
                    Success = false,
                    Error = $"Failed to fetch webpage: {ex.Message}"
                };
            }
        }

        #endregion

        private class SearchResult
        {
            public string Title { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string Snippet { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
        }
    }
}
