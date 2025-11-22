using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFeedbackAndErrorLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UserFeedbacks table
            migrationBuilder.CreateTable(
                name: "UserFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FeedbackType = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    CurrentPage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScreenResolution = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFeedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserFeedbacks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // ErrorLogs table
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorType = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    ComponentName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BrowserInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdditionalDataJson = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FirstOccurrence = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastOccurrence = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create indexes for UserFeedbacks
            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_UserId",
                table: "UserFeedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_FeedbackType",
                table: "UserFeedbacks",
                column: "FeedbackType");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_Status",
                table: "UserFeedbacks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_CreatedAt",
                table: "UserFeedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_AssignedToUserId",
                table: "UserFeedbacks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFeedbacks_Status_CreatedAt",
                table: "UserFeedbacks",
                columns: new[] { "Status", "CreatedAt" });

            // Create indexes for ErrorLogs
            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_UserId",
                table: "ErrorLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_ErrorType",
                table: "ErrorLogs",
                column: "ErrorType");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Severity",
                table: "ErrorLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_IsResolved",
                table: "ErrorLogs",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_CreatedAt",
                table: "ErrorLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_FirstOccurrence",
                table: "ErrorLogs",
                column: "FirstOccurrence");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_LastOccurrence",
                table: "ErrorLogs",
                column: "LastOccurrence");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Severity_IsResolved",
                table: "ErrorLogs",
                columns: new[] { "Severity", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_ErrorType_IsResolved",
                table: "ErrorLogs",
                columns: new[] { "ErrorType", "IsResolved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserFeedbacks");
            migrationBuilder.DropTable(name: "ErrorLogs");
        }
    }
}
