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
    }
}
