using Fluxor;

namespace AiMate.Web.Store.Knowledge;

public static class KnowledgeReducers
{
    // Search
    [ReducerMethod]
    public static KnowledgeState OnSearchKnowledge(KnowledgeState state, SearchKnowledgeAction action)
    {
        return state with { IsSearching = true, SearchQuery = action.Query, Error = null };
    }

    [ReducerMethod]
    public static KnowledgeState OnSearchKnowledgeSuccess(KnowledgeState state, SearchKnowledgeSuccessAction action)
    {
        return state with { SearchResults = action.Results, IsSearching = false };
    }

    [ReducerMethod]
    public static KnowledgeState OnSearchKnowledgeFailure(KnowledgeState state, SearchKnowledgeFailureAction action)
    {
        return state with { IsSearching = false, Error = action.Error };
    }

    // Load all
    [ReducerMethod]
    public static KnowledgeState OnLoadKnowledgeItems(KnowledgeState state, LoadKnowledgeItemsAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static KnowledgeState OnLoadKnowledgeItemsSuccess(KnowledgeState state, LoadKnowledgeItemsSuccessAction action)
    {
        var items = action.Items.ToDictionary(i => i.Id);

        // Extract all unique tags
        var allTags = action.Items
            .SelectMany(i => i.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            KnowledgeItems = items,
            AllTags = allTags,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static KnowledgeState OnLoadKnowledgeItemsFailure(KnowledgeState state, LoadKnowledgeItemsFailureAction action)
    {
        return state with { IsLoading = false, Error = action.Error };
    }

    // Select item
    [ReducerMethod]
    public static KnowledgeState OnSelectKnowledgeItem(KnowledgeState state, SelectKnowledgeItemAction action)
    {
        return state with { SelectedItemId = action.ItemId };
    }

    [ReducerMethod]
    public static KnowledgeState OnLoadRelatedItems(KnowledgeState state, LoadRelatedItemsAction action)
    {
        return state with { IsLoading = true };
    }

    [ReducerMethod]
    public static KnowledgeState OnLoadRelatedItemsSuccess(KnowledgeState state, LoadRelatedItemsSuccessAction action)
    {
        return state with { RelatedItems = action.RelatedItems, IsLoading = false };
    }

    // Tag filtering
    [ReducerMethod]
    public static KnowledgeState OnToggleTagFilter(KnowledgeState state, ToggleTagFilterAction action)
    {
        var selectedTags = state.SelectedTags.ToList();

        if (selectedTags.Contains(action.Tag))
        {
            selectedTags.Remove(action.Tag);
        }
        else
        {
            selectedTags.Add(action.Tag);
        }

        return state with { SelectedTags = selectedTags };
    }

    [ReducerMethod]
    public static KnowledgeState OnClearTagFilters(KnowledgeState state, ClearTagFiltersAction action)
    {
        return state with { SelectedTags = new List<string>() };
    }

    // Create
    [ReducerMethod]
    public static KnowledgeState OnCreateKnowledgeItem(KnowledgeState state, CreateKnowledgeItemAction action)
    {
        return state with { IsSaving = true, Error = null };
    }

    [ReducerMethod]
    public static KnowledgeState OnCreateKnowledgeItemSuccess(KnowledgeState state, CreateKnowledgeItemSuccessAction action)
    {
        var newItems = new Dictionary<Guid, Core.Entities.KnowledgeItem>(state.KnowledgeItems)
        {
            [action.Item.Id] = action.Item
        };

        // Update all tags
        var allTags = newItems.Values
            .SelectMany(i => i.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            KnowledgeItems = newItems,
            AllTags = allTags,
            IsSaving = false,
            ShowItemEditor = false,
            EditingItemId = null,
            SelectedItemId = action.Item.Id
        };
    }

    // Update
    [ReducerMethod]
    public static KnowledgeState OnUpdateKnowledgeItem(KnowledgeState state, UpdateKnowledgeItemAction action)
    {
        return state with { IsSaving = true, Error = null };
    }

    [ReducerMethod]
    public static KnowledgeState OnUpdateKnowledgeItemSuccess(KnowledgeState state, UpdateKnowledgeItemSuccessAction action)
    {
        var newItems = new Dictionary<Guid, Core.Entities.KnowledgeItem>(state.KnowledgeItems)
        {
            [action.Item.Id] = action.Item
        };

        // Update all tags
        var allTags = newItems.Values
            .SelectMany(i => i.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            KnowledgeItems = newItems,
            AllTags = allTags,
            IsSaving = false,
            ShowItemEditor = false,
            EditingItemId = null
        };
    }

    // Delete
    [ReducerMethod]
    public static KnowledgeState OnDeleteKnowledgeItem(KnowledgeState state, DeleteKnowledgeItemAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static KnowledgeState OnDeleteKnowledgeItemSuccess(KnowledgeState state, DeleteKnowledgeItemSuccessAction action)
    {
        var newItems = new Dictionary<Guid, Core.Entities.KnowledgeItem>(state.KnowledgeItems);
        newItems.Remove(action.ItemId);

        // Update all tags
        var allTags = newItems.Values
            .SelectMany(i => i.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var newSelectedItemId = state.SelectedItemId == action.ItemId ? null : state.SelectedItemId;

        return state with
        {
            KnowledgeItems = newItems,
            AllTags = allTags,
            SelectedItemId = newSelectedItemId,
            IsLoading = false
        };
    }

    // UI actions
    [ReducerMethod]
    public static KnowledgeState OnOpenKnowledgeItemEditor(KnowledgeState state, OpenKnowledgeItemEditorAction action)
    {
        return state with
        {
            ShowItemEditor = true,
            EditingItemId = action.ItemId,
            Error = null
        };
    }

    [ReducerMethod]
    public static KnowledgeState OnCloseKnowledgeItemEditor(KnowledgeState state, CloseKnowledgeItemEditorAction action)
    {
        return state with
        {
            ShowItemEditor = false,
            EditingItemId = null,
            Error = null
        };
    }

    [ReducerMethod]
    public static KnowledgeState OnUpdateSearchQuery(KnowledgeState state, UpdateSearchQueryAction action)
    {
        return state with { SearchQuery = action.Query };
    }

    // Error handling
    [ReducerMethod]
    public static KnowledgeState OnSetKnowledgeError(KnowledgeState state, SetKnowledgeErrorAction action)
    {
        return state with { Error = action.Error, IsLoading = false, IsSearching = false, IsSaving = false };
    }

    [ReducerMethod]
    public static KnowledgeState OnClearKnowledgeError(KnowledgeState state, ClearKnowledgeErrorAction action)
    {
        return state with { Error = null };
    }
}
