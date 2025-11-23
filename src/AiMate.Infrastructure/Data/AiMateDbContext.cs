using System.Text.Json;
using AiMate.Core.Entities;
// Add this if UserFeedback is in a different namespace
// using AiMate.Core.Entities.Feedback; // <-- Example, adjust as needed
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Pgvector.EntityFrameworkCore;

namespace AiMate.Infrastructure.Data;

/// <summary>
/// Main database context for aiMate
/// </summary>
public class AiMateDbContext : DbContext
{
    public AiMateDbContext(DbContextOptions<AiMateDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ToolCall> ToolCalls => Set<ToolCall>();
    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();
    public DbSet<WorkspaceFile> WorkspaceFiles => Set<WorkspaceFile>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<MessageFeedback> MessageFeedbacks => Set<MessageFeedback>();
    public DbSet<FeedbackTag> FeedbackTags => Set<FeedbackTag>();
    public DbSet<FeedbackTagTemplate> FeedbackTagTemplates => Set<FeedbackTagTemplate>();
    public DbSet<FeedbackTagOption> FeedbackTagOptions => Set<FeedbackTagOption>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<PluginSettings> PluginSettings => Set<PluginSettings>();
    public DbSet<CodeFile> CodeFiles => Set<CodeFile>();
    public DbSet<StructuredContentTemplate> StructuredContentTemplates => Set<StructuredContentTemplate>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<UserFeedback> UserFeedbacks => Set<UserFeedback>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasMany(e => e.Workspaces)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Projects)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.KnowledgeItems)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ApiKeys)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => new { e.UserId, e.IsArchived });

            entity.HasMany(e => e.Workspaces)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Workspace configuration
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProjectId);

            entity.HasMany(e => e.Conversations)
                .WithOne(e => e.Workspace)
                .HasForeignKey(e => e.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Files)
                .WithOne(e => e.Workspace)
                .HasForeignKey(e => e.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkspaceId);

            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing for conversation branching
            entity.HasOne(e => e.ParentConversation)
                .WithMany(e => e.BranchedConversations)
                .HasForeignKey(e => e.ParentConversationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId);

            entity.HasMany(e => e.ToolCalls)
                .WithOne(e => e.Message)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing for message editing
            entity.HasOne(e => e.ParentMessage)
                .WithMany(e => e.EditedVersions)
                .HasForeignKey(e => e.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Metadata dictionary as JSON column with value comparer
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, default(JsonSerializerOptions)))
                .HasColumnType("jsonb");
            entity.Property(e => e.Metadata)
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new Dictionary<string, object>(c)));
        });

        // ToolCall configuration
        modelBuilder.Entity<ToolCall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);
        });

        // KnowledgeItem configuration
        modelBuilder.Entity<KnowledgeItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.WorkspaceId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => new { e.UserId, e.IsPublished });
            entity.HasIndex(e => new { e.UserId, e.Visibility });

            // Vector column for pgvector semantic search
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)"); // OpenAI embedding dimension
        });

        // WorkspaceFile configuration
        modelBuilder.Entity<WorkspaceFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkspaceId);

            // Vector column for file content embeddings
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)");
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });
        });

        // MessageFeedback configuration
        modelBuilder.Entity<MessageFeedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ModelId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Rating);

            entity.HasOne(e => e.Message)
                .WithMany()
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tags)
                .WithOne(e => e.MessageFeedback)
                .HasForeignKey(e => e.MessageFeedbackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FeedbackTag configuration
        modelBuilder.Entity<FeedbackTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageFeedbackId);
            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => new { e.Key, e.Value });
        });

        // FeedbackTagTemplate configuration
        modelBuilder.Entity<FeedbackTagTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayOrder);

            entity.HasMany(e => e.Options)
                .WithOne(e => e.FeedbackTagTemplate)
                .HasForeignKey(e => e.FeedbackTagTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FeedbackTagOption configuration
        modelBuilder.Entity<FeedbackTagOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FeedbackTagTemplateId);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // Note configuration
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsPinned);
            entity.HasIndex(e => new { e.UserId, e.IsArchived });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Connection configuration
        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => new { e.OwnerId, e.IsEnabled });
            entity.HasIndex(e => e.OrganizationId);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PluginSettings configuration
        modelBuilder.Entity<PluginSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PluginId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.PluginId, e.UserId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CodeFile configuration
        modelBuilder.Entity<CodeFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Path);
            entity.HasIndex(e => new { e.ProjectId, e.Path }).IsUnique();
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.LastModified);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Metadata dictionary as JSON column with value comparer
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, default(JsonSerializerOptions)) ?? new Dictionary<string, string>())
                .HasColumnType("jsonb");
            entity.Property(e => e.Metadata)
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new Dictionary<string, string>(c)));

            // Configure Tags list as JSON column with value comparer
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("jsonb");
            entity.Property(e => e.Tags)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new List<string>(c)));
        });

        // StructuredContentTemplate configuration
        modelBuilder.Entity<StructuredContentTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => new { e.Name, e.Type });
            entity.HasIndex(e => e.IsBuiltIn);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.UsageCount);

            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasMany(e => e.Members)
                .WithOne(e => e.Organization)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Groups)
                .WithOne(e => e.Organization)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Connections)
                .WithOne(e => e.Organization)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OrganizationMember configuration
        modelBuilder.Entity<OrganizationMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.Role);

            entity.HasOne(e => e.User)
                .WithMany(e => e.OrganizationMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Group configuration
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Owner)
                .WithMany(e => e.OwnedGroups)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Members)
                .WithOne(e => e.Group)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Connections)
                .WithMany(c => c.AllowedGroups)
                .UsingEntity(j => j.ToTable("GroupConnections"));
        });

        // GroupMember configuration
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.Role);

            entity.HasOne(e => e.User)
                .WithMany(e => e.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserFeedback configuration
        modelBuilder.Entity<UserFeedback>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.FeedbackType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ErrorLog configuration
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ErrorType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.IsResolved);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.FirstOccurrence);
            entity.HasIndex(e => e.LastOccurrence);
            entity.HasIndex(e => new { e.Severity, e.IsResolved });
            entity.HasIndex(e => new { e.ErrorType, e.IsResolved });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

    }
}
