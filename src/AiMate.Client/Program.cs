using AiMate.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Blazored.LocalStorage;
using AiMate.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add application services
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<StorageService>();

// Configure HttpClient for LiteLLM
var liteLLMUrl = builder.Configuration["LiteLLM:BaseUrl"] ?? "http://localhost:4000";
var apiKey = builder.Configuration["LiteLLM:ApiKey"];

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(liteLLMUrl), Timeout = TimeSpan.FromMinutes(5) };
    return new LiteLLMService(httpClient, liteLLMUrl, apiKey);
});

var app = builder.Build();

// Load saved state on startup
var appState = app.Services.GetRequiredService<AppStateService>();
var storage = app.Services.GetRequiredService<StorageService>();

try
{
    var conversations = await storage.LoadConversationsAsync();
    foreach (var conv in conversations)
    {
        appState.LoadConversation(conv);
    }

    var knowledge = await storage.LoadKnowledgeAsync();
    foreach (var item in knowledge)
    {
        appState.AddKnowledgeItem(item);
    }

    var preferences = await storage.LoadPreferencesAsync();
    appState.LoadPreferences(preferences);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load saved state: {ex.Message}");
}

await app.RunAsync();
