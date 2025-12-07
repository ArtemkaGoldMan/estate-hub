using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateHub.ListingService.DataAccess.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAIQuestionUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIQuestionUsage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIQuestionUsage", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIQuestionUsage_Date",
                table: "AIQuestionUsage",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuestionUsage_UserId",
                table: "AIQuestionUsage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIQuestionUsage_UserId_Date",
                table: "AIQuestionUsage",
                columns: new[] { "UserId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIQuestionUsage");
        }
    }
}
