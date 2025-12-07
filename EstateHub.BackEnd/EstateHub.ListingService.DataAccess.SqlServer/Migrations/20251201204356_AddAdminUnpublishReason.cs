using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateHub.ListingService.DataAccess.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUnpublishReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminUnpublishReason",
                table: "Listings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminUnpublishReason",
                table: "Listings");
        }
    }
}
