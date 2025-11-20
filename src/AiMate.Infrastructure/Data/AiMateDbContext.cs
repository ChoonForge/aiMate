using AiMate.Core.Entities;
using Microsoft.EntityFrameworkCore;
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
    }
}
