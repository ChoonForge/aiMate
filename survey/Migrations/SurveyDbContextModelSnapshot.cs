using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AlphaSurvey.Data;

#nullable disable

namespace AlphaSurvey.Migrations
{
    [DbContext(typeof(SurveyDbContext))]
    partial class SurveyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AlphaSurvey.Models.SurveyResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AIToolsUsed")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("AdditionalComments")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AgeRange")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("BarrierComplexity")
                        .HasColumnType("bit");

                    b.Property<bool>("BarrierCost")
                        .HasColumnType("bit");

                    b.Property<bool>("BarrierDontKnowHowToStart")
                        .HasColumnType("bit");

                    b.Property<bool>("BarrierDontSeeBenefit")
                        .HasColumnType("bit");

                    b.Property<bool>("BarrierOther")
                        .HasColumnType("bit");

                    b.Property<bool>("BarrierPrivacyConcerns")
                        .HasColumnType("bit");

                    b.Property<string>("BiggestFrustration")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("CurrentlyUsesAI")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("FrequencyOfUse")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("GeneralLocation")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("IdealAIFeatures")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("InterestedInAlphaTesting")
                        .HasColumnType("bit");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("OccupationCategory")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("OtherBarrier")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("OtherUseCase")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("PrimaryAITool")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ReferralSource")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("SubmittedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("TechComfortLevel")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("UseForCoding")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForCreativeWork")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForLearning")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForOther")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForPersonalProjects")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForResearch")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForWork")
                        .HasColumnType("bit");

                    b.Property<bool>("UseForWriting")
                        .HasColumnType("bit");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("WhatsMissing")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.HasKey("Id");

                    b.HasIndex("CurrentlyUsesAI");

                    b.HasIndex("Email");

                    b.HasIndex("InterestedInAlphaTesting");

                    b.HasIndex("ReferralSource");

                    b.HasIndex("SubmittedAt");

                    b.ToTable("SurveyResponses");
                });
#pragma warning restore 612, 618
        }
    }
}
