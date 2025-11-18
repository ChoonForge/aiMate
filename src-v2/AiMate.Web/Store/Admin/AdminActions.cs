namespace AiMate.Web.Store.Admin;

// UI Actions
public record SetActiveAdminTabAction(int TabIndex);
public record ClearAdminErrorAction();

// Data Loading Actions
public record LoadAdminDataAction();
public record LoadAdminDataSuccessAction(AdminState State);
public record LoadAdminDataFailureAction(string Error);

// Save Actions
public record SaveAdminChangesAction();
public record SaveAdminChangesSuccessAction();
public record SaveAdminChangesFailureAction(string Error);

// Model Management Actions
public record AddModelAction(AIModelConfig Model);
public record UpdateModelAction(AIModelConfig Model);
public record DeleteModelAction(string ModelId);
public record ToggleModelAction(string ModelId);

// MCP Server Actions
public record AddMcpServerAction(MCPServerConfig Server);
public record UpdateMcpServerAction(MCPServerConfig Server);
public record DeleteMcpServerAction(string ServerId);
public record ReconnectMcpServerAction(string ServerId);

// Connection Actions
public record UpdateAdminLiteLLMUrlAction(string Url);
public record UpdateAdminLiteLLMApiKeyAction(string? ApiKey);
public record TestLiteLLMConnectionAction();
public record TestLiteLLMConnectionSuccessAction();
public record TestLiteLLMConnectionFailureAction(string Error);

// System Actions
public record ClearLocalStorageAction();
public record ClearIndexedDBAction();
public record RefreshSystemLogsAction();
public record ClearSystemLogsAction();
public record ExportSystemLogsAction();
public record ExportSystemConfigAction();
public record ImportSystemConfigAction(string ConfigJson);
public record CreateBackupAction();
public record FactoryResetAction();

// Provider Connection Actions
public record UpdateProviderApiKeyAction(string Provider, string ApiKey);
public record TestProviderConnectionAction(string Provider);
