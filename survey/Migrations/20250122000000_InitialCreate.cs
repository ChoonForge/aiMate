using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSurvey.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurveyResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgeRange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GeneralLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OccupationCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TechComfortLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrentlyUsesAI = table.Column<bool>(type: "bit", nullable: false),
                    AIToolsUsed = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryAITool = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FrequencyOfUse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UseForWork = table.Column<bool>(type: "bit", nullable: false),
                    UseForPersonalProjects = table.Column<bool>(type: "bit", nullable: false),
                    UseForLearning = table.Column<bool>(type: "bit", nullable: false),
                    UseForCreativeWork = table.Column<bool>(type: "bit", nullable: false),
                    UseForCoding = table.Column<bool>(type: "bit", nullable: false),
                    UseForWriting = table.Column<bool>(type: "bit", nullable: false),
                    UseForResearch = table.Column<bool>(type: "bit", nullable: false),
                    UseForOther = table.Column<bool>(type: "bit", nullable: false),
                    OtherUseCase = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BarrierCost = table.Column<bool>(type: "bit", nullable: false),
                    BarrierComplexity = table.Column<bool>(type: "bit", nullable: false),
                    BarrierPrivacyConcerns = table.Column<bool>(type: "bit", nullable: false),
                    BarrierDontKnowHowToStart = table.Column<bool>(type: "bit", nullable: false),
                    BarrierDontSeeBenefit = table.Column<bool>(type: "bit", nullable: false),
                    BarrierOther = table.Column<bool>(type: "bit", nullable: false),
                    OtherBarrier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhatsMissing = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdealAIFeatures = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BiggestFrustration = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InterestedInAlphaTesting = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AdditionalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferralSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SubmittedAt",
                table: "SurveyResponses",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_CurrentlyUsesAI",
                table: "SurveyResponses",
                column: "CurrentlyUsesAI");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_InterestedInAlphaTesting",
                table: "SurveyResponses",
                column: "InterestedInAlphaTesting");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_Email",
                table: "SurveyResponses",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_ReferralSource",
                table: "SurveyResponses",
                column: "ReferralSource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyResponses");
        }
    }
}
