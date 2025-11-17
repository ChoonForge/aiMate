using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Fluxor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Knowledge;

public class KnowledgeEffects
{
    private readonly IKnowledgeGraphService _knowledgeGraphService;
    private readonly AiMateDbContext _context;
    private readonly ILogger<KnowledgeEffects> _logger;

    public KnowledgeEffects(
        IKnowledgeGraphService knowledgeGraphService,
        AiMateDbContext context,
        ILogger<KnowledgeEffects> logger)
    {
        _knowledgeGraphService = knowledgeGraphService;
        _context = context;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleSearchKnowledge(SearchKnowledgeAction action, IDispatcher dispatcher)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(action.Query))
            {
                dispatcher.Dispatch(new SearchKnowledgeSuccessAction(new List<Core.Entities.KnowledgeItem>()));
                return;
            }

            // DEMO MODE: Using hardcoded user ID (inject IState<AuthState> when auth is enabled)
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var results = await _knowledgeGraphService.SearchAsync(action.Query, userId, limit: 20);

            dispatcher.Dispatch(new SearchKnowledgeSuccessAction(results));

            _logger.LogInformation("Found {Count} knowledge items for query: {Query}", results.Count, action.Query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search knowledge for query: {Query}", action.Query);
            dispatcher.Dispatch(new SearchKnowledgeFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadKnowledgeItems(LoadKnowledgeItemsAction action, IDispatcher dispatcher)
    {
        try
        {
            // DEMO MODE: Using hardcoded user ID (inject IState<AuthState> when auth is enabled)
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var items = await _context.KnowledgeItems
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.UpdatedAt)
                .ToListAsync();

            dispatcher.Dispatch(new LoadKnowledgeItemsSuccessAction(items));

            _logger.LogInformation("Loaded {Count} knowledge items", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load knowledge items");
            dispatcher.Dispatch(new LoadKnowledgeItemsFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleLoadRelatedItems(LoadRelatedItemsAction action, IDispatcher dispatcher)
    {
        try
        {
            var relatedItems = await _knowledgeGraphService.GetRelatedAsync(action.ItemId, limit: 10);

            dispatcher.Dispatch(new LoadRelatedItemsSuccessAction(relatedItems));

            _logger.LogInformation("Found {Count} related items for {ItemId}", relatedItems.Count, action.ItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load related items for {ItemId}", action.ItemId);
            dispatcher.Dispatch(new SetKnowledgeErrorAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleCreateKnowledgeItem(CreateKnowledgeItemAction action, IDispatcher dispatcher)
    {
        try
        {
            // DEMO MODE: Using hardcoded user ID (inject IState<AuthState> when auth is enabled)
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var item = new Core.Entities.KnowledgeItem
            {
                UserId = userId,
                Title = action.Title,
                Content = action.Content,
                Tags = action.Tags ?? new List<string>(),
                Type = action.Type ?? "Manual",
                SourceUrl = action.SourceUrl
            };

            var created = await _knowledgeGraphService.UpsertKnowledgeItemAsync(item);

            dispatcher.Dispatch(new CreateKnowledgeItemSuccessAction(created));

            _logger.LogInformation("Created knowledge item {ItemId}: {Title}", created.Id, created.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create knowledge item");
            dispatcher.Dispatch(new SetKnowledgeErrorAction($"Failed to create item: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateKnowledgeItem(UpdateKnowledgeItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var item = await _context.KnowledgeItems.FindAsync(action.ItemId);

            if (item == null)
            {
                dispatcher.Dispatch(new SetKnowledgeErrorAction("Knowledge item not found"));
                return;
            }

            item.Title = action.Title;
            item.Content = action.Content;
            item.Tags = action.Tags ?? new List<string>();
            item.Type = action.Type ?? item.Type;
            item.SourceUrl = action.SourceUrl;

            var updated = await _knowledgeGraphService.UpsertKnowledgeItemAsync(item);

            dispatcher.Dispatch(new UpdateKnowledgeItemSuccessAction(updated));

            _logger.LogInformation("Updated knowledge item {ItemId}: {Title}", updated.Id, updated.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update knowledge item {ItemId}", action.ItemId);
            dispatcher.Dispatch(new SetKnowledgeErrorAction($"Failed to update item: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteKnowledgeItem(DeleteKnowledgeItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var item = await _context.KnowledgeItems.FindAsync(action.ItemId);

            if (item != null)
            {
                _context.KnowledgeItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            dispatcher.Dispatch(new DeleteKnowledgeItemSuccessAction(action.ItemId));

            _logger.LogInformation("Deleted knowledge item {ItemId}", action.ItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete knowledge item {ItemId}", action.ItemId);
            dispatcher.Dispatch(new SetKnowledgeErrorAction($"Failed to delete item: {ex.Message}"));
        }
    }
}
