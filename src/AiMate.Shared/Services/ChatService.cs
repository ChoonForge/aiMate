using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AiMate.Shared.Models;

namespace AiMate.Shared.Services
{
    public interface IChatService
    {
        Task<Conversation> GetCurrentConversation();
        Task<Conversation> CreateNewConversation();
        Task<string> SendMessage(string message);
        Task<List<Conversation>> GetConversationHistory(int take = 50);
        Task DeleteConversation(Guid conversationId);
    }

    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
        private Conversation? _currentConversation;

        public ChatService(HttpClient httpClient, Blazored.LocalStorage.ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<Conversation> GetCurrentConversation()
        {
            if (_currentConversation != null)
            {
                return _currentConversation;
            }

            var conversationId = await _localStorage.GetItemAsync<Guid?>("current-conversation-id");
            
            if (conversationId.HasValue)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"api/chat/conversations/{conversationId.Value}");
                    if (response.IsSuccessStatusCode)
                    {
                        var conversation = await response.Content.ReadFromJsonAsync<Conversation>();
                        if (conversation != null)
                        {
                            _currentConversation = conversation;
                            return conversation;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading conversation: {ex.Message}");
                }
            }

            // Create new conversation if none exists
            return await CreateNewConversation();
        }

        public async Task<Conversation> CreateNewConversation()
        {
            var conversation = new Conversation
            {
                Title = $"Conversation {DateTime.Now:yyyy-MM-dd HH:mm}"
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/chat/conversations", conversation);
                if (response.IsSuccessStatusCode)
                {
                    var createdConversation = await response.Content.ReadFromJsonAsync<Conversation>();
                    if (createdConversation != null)
                    {
                        _currentConversation = createdConversation;
                        await _localStorage.SetItemAsync("current-conversation-id", createdConversation.Id);
                        return createdConversation;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating conversation: {ex.Message}");
            }

            // Fallback to local conversation
            _currentConversation = conversation;
            return conversation;
        }

        public async Task<string> SendMessage(string message)
        {
            var conversation = await GetCurrentConversation();
            
            var chatRequest = new
            {
                Message = message,
                ConversationId = conversation.Id,
                Model = await _localStorage.GetItemAsync<string>("ai-model") ?? "gpt-4",
                Temperature = await _localStorage.GetItemAsync<double?>("temperature") ?? 0.7,
                MaxTokens = await _localStorage.GetItemAsync<int?>("max-tokens") ?? 2000
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/chat/send", chatRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
                    return result?.Response ?? "I'm having trouble responding right now. Please try again.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return "I'm having trouble connecting to the AI service right now. Please try again in a moment.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send message error: {ex.Message}");
                return "Sorry, there was an error sending your message.";
            }
        }

        public async Task<List<Conversation>> GetConversationHistory(int take = 50)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/chat/conversations?take={take}");
                if (response.IsSuccessStatusCode)
                {
                    var conversations = await response.Content.ReadFromJsonAsync<List<Conversation>>();
                    return conversations ?? new List<Conversation>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading history: {ex.Message}");
            }

            return new List<Conversation>();
        }

        public async Task DeleteConversation(Guid conversationId)
        {
            try
            {
                await _httpClient.DeleteAsync($"api/chat/conversations/{conversationId}");
                
                if (_currentConversation?.Id == conversationId.ToString())
                {
                    _currentConversation = null;
                    await _localStorage.RemoveItemAsync("current-conversation-id");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting conversation: {ex.Message}");
            }
        }
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public string? Model { get; set; }
        public int? TokenUsage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
