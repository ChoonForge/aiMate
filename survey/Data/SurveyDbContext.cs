using Microsoft.EntityFrameworkCore;
using AlphaSurvey.Models;

namespace AlphaSurvey.Data;

public class SurveyDbContext : DbContext
{
    public SurveyDbContext(DbContextOptions<SurveyDbContext> options)
        : base(options)
    {
    }

    public DbSet<SurveyResponse> SurveyResponses { get; set; } = null!;
    public DbSet<UseCaseCategory> UseCaseCategories { get; set; } = null!;
    public DbSet<UseCaseOption> UseCaseOptions { get; set; } = null!;
    public DbSet<SurveyResponseUseCase> SurveyResponseUseCases { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            // Indexes for common queries
            entity.HasIndex(e => e.SubmittedAt);
            entity.HasIndex(e => e.CurrentlyUsesAI);
            entity.HasIndex(e => e.InterestedInAlphaTesting);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.ReferralSource);
        });

        modelBuilder.Entity<UseCaseCategory>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<UseCaseOption>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(o => o.Category)
                .WithMany(c => c.Options)
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SurveyResponseUseCase>(entity =>
        {
            // Composite key
            entity.HasKey(sruc => new { sruc.SurveyResponseId, sruc.UseCaseOptionId });

            entity.HasOne(sruc => sruc.SurveyResponse)
                .WithMany(sr => sr.SelectedUseCases)
                .HasForeignKey(sruc => sruc.SurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sruc => sruc.UseCaseOption)
                .WithMany(uco => uco.SurveyResponses)
                .HasForeignKey(sruc => sruc.UseCaseOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
