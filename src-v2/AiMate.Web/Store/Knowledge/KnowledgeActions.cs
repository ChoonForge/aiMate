using AiMate.Core.Entities;

namespace AiMate.Web.Store.Knowledge;

// Search
public record SearchKnowledgeAction(string Query);
public record SearchKnowledgeSuccessAction(List<KnowledgeItem> Results);
public record SearchKnowledgeFailureAction(string Error);

// Load all knowledge items
public record LoadKnowledgeItemsAction;
public record LoadKnowledgeItemsSuccessAction(List<KnowledgeItem> Items);
public record LoadKnowledgeItemsFailureAction(string Error);

// Select item
public record SelectKnowledgeItemAction(Guid ItemId);
public record LoadRelatedItemsAction(Guid ItemId);
public record LoadRelatedItemsSuccessAction(List<KnowledgeItem> RelatedItems);

// Filter by tags
public record ToggleTagFilterAction(string Tag);
public record ClearTagFiltersAction;

// Create/Update knowledge item
public record CreateKnowledgeItemAction(
    string Title,
    string Content,
    List<string> Tags,
    string? Type = null,
    string? SourceUrl = null);
public record CreateKnowledgeItemSuccessAction(KnowledgeItem Item);

public record UpdateKnowledgeItemAction(
    Guid ItemId,
    string Title,
    string Content,
    List<string> Tags,
    string? Type = null,
    string? SourceUrl = null);
public record UpdateKnowledgeItemSuccessAction(KnowledgeItem Item);

// Delete
public record DeleteKnowledgeItemAction(Guid ItemId);
public record DeleteKnowledgeItemSuccessAction(Guid ItemId);

// UI actions
public record OpenKnowledgeItemEditorAction(Guid? ItemId = null);
public record CloseKnowledgeItemEditorAction;

// Update search query (for UI binding)
public record UpdateSearchQueryAction(string Query);

// Error handling
public record SetKnowledgeErrorAction(string Error);
public record ClearKnowledgeErrorAction;
