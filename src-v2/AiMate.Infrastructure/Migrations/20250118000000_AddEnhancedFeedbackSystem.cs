using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnhancedFeedbackSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FeedbackTagTemplates table
            migrationBuilder.CreateTable(
                name: "FeedbackTagTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackTagTemplates", x => x.Id);
                });

            // FeedbackTagOptions table
            migrationBuilder.CreateTable(
                name: "FeedbackTagOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedbackTagTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Sentiment = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackTagOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackTagOptions_FeedbackTagTemplates_FeedbackTagTemplateId",
                        column: x => x.FeedbackTagTemplateId,
                        principalTable: "FeedbackTagTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // MessageFeedbacks table
            migrationBuilder.CreateTable(
                name: "MessageFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    TextFeedback = table.Column<string>(type: "text", nullable: true),
                    ModelId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageFeedbacks_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageFeedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // FeedbackTags table
            migrationBuilder.CreateTable(
                name: "FeedbackTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageFeedbackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Sentiment = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackTags_MessageFeedbacks_MessageFeedbackId",
                        column: x => x.MessageFeedbackId,
                        principalTable: "MessageFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_MessageFeedbacks_MessageId",
                table: "MessageFeedbacks",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageFeedbacks_UserId",
                table: "MessageFeedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageFeedbacks_ModelId",
                table: "MessageFeedbacks",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageFeedbacks_CreatedAt",
                table: "MessageFeedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessageFeedbacks_Rating",
                table: "MessageFeedbacks",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTags_MessageFeedbackId",
                table: "FeedbackTags",
                column: "MessageFeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTags_Key",
                table: "FeedbackTags",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTags_Key_Value",
                table: "FeedbackTags",
                columns: new[] { "Key", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTagTemplates_Category",
                table: "FeedbackTagTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTagTemplates_IsActive",
                table: "FeedbackTagTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTagTemplates_DisplayOrder",
                table: "FeedbackTagTemplates",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTagOptions_FeedbackTagTemplateId",
                table: "FeedbackTagOptions",
                column: "FeedbackTagTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTagOptions_DisplayOrder",
                table: "FeedbackTagOptions",
                column: "DisplayOrder");

            // Insert default tag templates
            InsertDefaultTagTemplates(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FeedbackTags");
            migrationBuilder.DropTable(name: "MessageFeedbacks");
            migrationBuilder.DropTable(name: "FeedbackTagOptions");
            migrationBuilder.DropTable(name: "FeedbackTagTemplates");
        }

        private void InsertDefaultTagTemplates(MigrationBuilder migrationBuilder)
        {
            // Quality template
            var qualityTemplateId = Guid.NewGuid();
            migrationBuilder.InsertData(
                table: "FeedbackTagTemplates",
                columns: new[] { "Id", "Category", "Label", "Description", "IsActive", "DisplayOrder", "IsRequired", "CreatedAt" },
                values: new object[] { qualityTemplateId, "Quality", "Response Quality", "Rate the overall quality of the response", true, 1, false, DateTime.UtcNow }
            );

            migrationBuilder.InsertData(
                table: "FeedbackTagOptions",
                columns: new[] { "Id", "FeedbackTagTemplateId", "Value", "Color", "Sentiment", "DisplayOrder", "Icon" },
                values: new object[,]
                {
                    { Guid.NewGuid(), qualityTemplateId, "Excellent", "#4caf50", 0, 1, "Stars" },
                    { Guid.NewGuid(), qualityTemplateId, "Good", "#8bc34a", 0, 2, "ThumbUp" },
                    { Guid.NewGuid(), qualityTemplateId, "Fair", "#ff9800", 1, 3, "Remove" },
                    { Guid.NewGuid(), qualityTemplateId, "Poor", "#f44336", 2, 4, "ThumbDown" }
                });

            // Accuracy template
            var accuracyTemplateId = Guid.NewGuid();
            migrationBuilder.InsertData(
                table: "FeedbackTagTemplates",
                columns: new[] { "Id", "Category", "Label", "Description", "IsActive", "DisplayOrder", "IsRequired", "CreatedAt" },
                values: new object[] { accuracyTemplateId, "Accuracy", "Factual Accuracy", "How accurate and factual was the response?", true, 2, false, DateTime.UtcNow }
            );

            migrationBuilder.InsertData(
                table: "FeedbackTagOptions",
                columns: new[] { "Id", "FeedbackTagTemplateId", "Value", "Color", "Sentiment", "DisplayOrder", "Icon" },
                values: new object[,]
                {
                    { Guid.NewGuid(), accuracyTemplateId, "Very Accurate", "#4caf50", 0, 1, "CheckCircle" },
                    { Guid.NewGuid(), accuracyTemplateId, "Mostly Accurate", "#8bc34a", 0, 2, "Check" },
                    { Guid.NewGuid(), accuracyTemplateId, "Somewhat Accurate", "#ff9800", 1, 3, "Help" },
                    { Guid.NewGuid(), accuracyTemplateId, "Inaccurate", "#f44336", 2, 4, "Cancel" }
                });

            // Helpfulness template
            var helpfulnessTemplateId = Guid.NewGuid();
            migrationBuilder.InsertData(
                table: "FeedbackTagTemplates",
                columns: new[] { "Id", "Category", "Label", "Description", "IsActive", "DisplayOrder", "IsRequired", "CreatedAt" },
                values: new object[] { helpfulnessTemplateId, "Helpfulness", "How Helpful", "Did this response help solve your problem?", true, 3, false, DateTime.UtcNow }
            );

            migrationBuilder.InsertData(
                table: "FeedbackTagOptions",
                columns: new[] { "Id", "FeedbackTagTemplateId", "Value", "Color", "Sentiment", "DisplayOrder", "Icon" },
                values: new object[,]
                {
                    { Guid.NewGuid(), helpfulnessTemplateId, "Very Helpful", "#4caf50", 0, 1, "EmojiEmotions" },
                    { Guid.NewGuid(), helpfulnessTemplateId, "Helpful", "#8bc34a", 0, 2, "SentimentSatisfied" },
                    { Guid.NewGuid(), helpfulnessTemplateId, "Somewhat Helpful", "#ff9800", 1, 3, "SentimentNeutral" },
                    { Guid.NewGuid(), helpfulnessTemplateId, "Not Helpful", "#f44336", 2, 4, "SentimentDissatisfied" }
                });

            // Tone template
            var toneTemplateId = Guid.NewGuid();
            migrationBuilder.InsertData(
                table: "FeedbackTagTemplates",
                columns: new[] { "Id", "Category", "Label", "Description", "IsActive", "DisplayOrder", "IsRequired", "CreatedAt" },
                values: new object[] { toneTemplateId, "Tone", "Response Tone", "Was the tone appropriate?", true, 4, false, DateTime.UtcNow }
            );

            migrationBuilder.InsertData(
                table: "FeedbackTagOptions",
                columns: new[] { "Id", "FeedbackTagTemplateId", "Value", "Color", "Sentiment", "DisplayOrder", "Icon" },
                values: new object[,]
                {
                    { Guid.NewGuid(), toneTemplateId, "Professional", "#2196f3", 0, 1, "Business" },
                    { Guid.NewGuid(), toneTemplateId, "Friendly", "#4caf50", 0, 2, "EmojiPeople" },
                    { Guid.NewGuid(), toneTemplateId, "Casual", "#9c27b0", 1, 3, "Chat" },
                    { Guid.NewGuid(), toneTemplateId, "Too Formal", "#ff9800", 1, 4, "MenuBook" },
                    { Guid.NewGuid(), toneTemplateId, "Inappropriate", "#f44336", 2, 5, "Warning" }
                });
        }
    }
}
