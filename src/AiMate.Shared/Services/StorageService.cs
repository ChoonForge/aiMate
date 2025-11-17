using Blazored.LocalStorage;
using AiMate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AiMate.Shared.Services;

/// <summary>
/// Local storage service for persisting app state
/// </summary>
public class StorageService
{
    private readonly IJSRuntime _js;
    private readonly ILocalStorageService _localStorage;

    public StorageService(IJSRuntime js, ILocalStorageService localStorage)
    {
        _js = js;
        _localStorage = localStorage;
    }

    public async Task SaveConversationsAsync(List<Conversation> conversations)
    {
        await _localStorage.SetItemAsync("conversations", conversations);
    }

    public async Task<List<Conversation>> LoadConversationsAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<List<Conversation>>("conversations") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task SaveKnowledgeAsync(List<KnowledgeItem> items)
    {
        await _localStorage.SetItemAsync("knowledge", items);
    }

    public async Task<List<KnowledgeItem>> LoadKnowledgeAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<List<KnowledgeItem>>("knowledge") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task SavePreferencesAsync(UserPreferences preferences)
    {
        await _localStorage.SetItemAsync("preferences", preferences);
    }

    public async Task<UserPreferences> LoadPreferencesAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<UserPreferences>("preferences") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task SaveFoldersAsync(List<Folder> folders)
    {
        await _localStorage.SetItemAsync("folders", folders);
    }

    public async Task<List<Folder>> LoadFoldersAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<List<Folder>>("folders") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task ClearAllAsync()
    {
        await _localStorage.ClearAsync();
    }

    public async Task<string> ExportAllDataAsync()
    {
        var conversations = await LoadConversationsAsync();
        var knowledge = await LoadKnowledgeAsync();
        var preferences = await LoadPreferencesAsync();
        var folders = await LoadFoldersAsync();

        var exportData = new
        {
            version = "1.0",
            exportedAt = DateTime.UtcNow,
            conversations,
            knowledge,
            preferences,
            folders
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public async Task ImportDataAsync(string jsonData)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            if (root.TryGetProperty("conversations", out var convsElement))
            {
                var conversations = JsonSerializer.Deserialize<List<Conversation>>(convsElement.GetRawText());
                if (conversations != null)
                    await SaveConversationsAsync(conversations);
            }

            if (root.TryGetProperty("knowledge", out var knowledgeElement))
            {
                var knowledge = JsonSerializer.Deserialize<List<KnowledgeItem>>(knowledgeElement.GetRawText());
                if (knowledge != null)
                    await SaveKnowledgeAsync(knowledge);
            }

            if (root.TryGetProperty("preferences", out var prefsElement))
            {
                var preferences = JsonSerializer.Deserialize<UserPreferences>(prefsElement.GetRawText());
                if (preferences != null)
                    await SavePreferencesAsync(preferences);
            }

            if (root.TryGetProperty("folders", out var foldersElement))
            {
                var folders = JsonSerializer.Deserialize<List<Folder>>(foldersElement.GetRawText());
                if (folders != null)
                    await SaveFoldersAsync(folders);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to import data: {ex.Message}", ex);
        }
    }

    // File storage using IndexedDB
    public async Task<bool> SaveFileAsync(string id, byte[] fileData)
    {
        try
        {
            await _js.InvokeVoidAsync("fileStorage.saveFile", id, fileData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]?> GetFileAsync(string id)
    {
        try
        {
            return await _js.InvokeAsync<byte[]>("fileStorage.getFile", id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string id)
    {
        try
        {
            await _js.InvokeVoidAsync("fileStorage.deleteFile", id);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
