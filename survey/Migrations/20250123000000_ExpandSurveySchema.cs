using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSurvey.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSurveySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Use Case Categories table
            migrationBuilder.CreateTable(
                name: "UseCaseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UseCaseCategories", x => x.Id);
                });

            // Create Use Case Options table
            migrationBuilder.CreateTable(
                name: "UseCaseOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UseCaseOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UseCaseOptions_UseCaseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "UseCaseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create junction table for Survey Responses and Use Cases
            migrationBuilder.CreateTable(
                name: "SurveyResponseUseCases",
                columns: table => new
                {
                    SurveyResponseId = table.Column<int>(type: "int", nullable: false),
                    UseCaseOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseUseCases", x => new { x.SurveyResponseId, x.UseCaseOptionId });
                    table.ForeignKey(
                        name: "FK_SurveyResponseUseCases_SurveyResponses_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "SurveyResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyResponseUseCases_UseCaseOptions_UseCaseOptionId",
                        column: x => x.UseCaseOptionId,
                        principalTable: "UseCaseOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add indexes for use case tables
            migrationBuilder.CreateIndex(
                name: "IX_UseCaseCategories_DisplayOrder",
                table: "UseCaseCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_UseCaseCategories_IsActive",
                table: "UseCaseCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UseCaseOptions_CategoryId",
                table: "UseCaseOptions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UseCaseOptions_DisplayOrder",
                table: "UseCaseOptions",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_UseCaseOptions_IsActive",
                table: "UseCaseOptions",
                column: "IsActive");

            // Add new columns to SurveyResponses table
            // Context & Conversation Needs
            migrationBuilder.AddColumn<string>(
                name: "TypicalConversationLength",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsLongTermMemory",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsContextAcrossSessions",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // File & Document Handling
            migrationBuilder.AddColumn<bool>(
                name: "WorksWithDocuments",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WorksWithImages",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WorksWithCode",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WorksWithData",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TypicalFileSize",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Response Preferences
            migrationBuilder.AddColumn<string>(
                name: "PreferredResponseStyle",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualityVsSpeed",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Advanced Features
            migrationBuilder.AddColumn<bool>(
                name: "NeedsCodeExecution",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsImageGeneration",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsWebSearch",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsDataVisualization",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Collaboration & Sharing
            migrationBuilder.AddColumn<bool>(
                name: "NeedsTeamCollaboration",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsWorkspaceOrganization",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WouldShareConversations",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Integration Needs
            migrationBuilder.AddColumn<bool>(
                name: "NeedsAPIAccess",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsIntegrationWithTools",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SpecificIntegrations",
                table: "SurveyResponses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Privacy & Data Preferences
            migrationBuilder.AddColumn<string>(
                name: "DataPrivacyConcern",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredDataLocation",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WillingToShareDataForImprovement",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Pricing & Cost
            migrationBuilder.AddColumn<string>(
                name: "WillingToPayMonthly",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredPricingModel",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MostImportantValue",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Platform Preferences
            migrationBuilder.AddColumn<bool>(
                name: "UsesWeb",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsesDesktop",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsesMobile",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPlatform",
                table: "SurveyResponses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // LLM Familiarity
            migrationBuilder.AddColumn<bool>(
                name: "FamiliarWithGPT4",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FamiliarWithClaude",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FamiliarWithGemini",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FamiliarWithOpenSource",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLLMFeatures",
                table: "SurveyResponses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            // Multimodal Needs
            migrationBuilder.AddColumn<bool>(
                name: "NeedsVoiceInput",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsVoiceOutput",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsVideoAnalysis",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Language Preferences
            migrationBuilder.AddColumn<string>(
                name: "PrimaryLanguage",
                table: "SurveyResponses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsMultilingualSupport",
                table: "SurveyResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new tables
            migrationBuilder.DropTable(name: "SurveyResponseUseCases");
            migrationBuilder.DropTable(name: "UseCaseOptions");
            migrationBuilder.DropTable(name: "UseCaseCategories");

            // Drop new columns from SurveyResponses
            var columnsToRemove = new[]
            {
                "TypicalConversationLength", "NeedsLongTermMemory", "NeedsContextAcrossSessions",
                "WorksWithDocuments", "WorksWithImages", "WorksWithCode", "WorksWithData", "TypicalFileSize",
                "PreferredResponseStyle", "QualityVsSpeed",
                "NeedsCodeExecution", "NeedsImageGeneration", "NeedsWebSearch", "NeedsDataVisualization",
                "NeedsTeamCollaboration", "NeedsWorkspaceOrganization", "WouldShareConversations",
                "NeedsAPIAccess", "NeedsIntegrationWithTools", "SpecificIntegrations",
                "DataPrivacyConcern", "PreferredDataLocation", "WillingToShareDataForImprovement",
                "WillingToPayMonthly", "PreferredPricingModel", "MostImportantValue",
                "UsesWeb", "UsesDesktop", "UsesMobile", "PrimaryPlatform",
                "FamiliarWithGPT4", "FamiliarWithClaude", "FamiliarWithGemini", "FamiliarWithOpenSource", "PreferredLLMFeatures",
                "NeedsVoiceInput", "NeedsVoiceOutput", "NeedsVideoAnalysis",
                "PrimaryLanguage", "NeedsMultilingualSupport"
            };

            foreach (var column in columnsToRemove)
            {
                migrationBuilder.DropColumn(name: column, table: "SurveyResponses");
            }
        }
    }
}
